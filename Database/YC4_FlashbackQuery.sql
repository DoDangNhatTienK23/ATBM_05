-- ============================================================
-- YEU CAU 4: SAO LUU VA PHUC HOI DU LIEU
-- PHUONG PHAP 2: FLASHBACK QUERY
-- Phuc hoi du lieu dua vao nhat ky kiem toan (SCN / Timestamp)
-- De chay duoc doan nay, hay chay file TEST.sql de thu kich ban audit
-- Chay bang: BVDBA (co DBA role)
-- ============================================================
 
ALTER SESSION SET CONTAINER = XEPDB1;
 
-- ------------------------------------------------------------
-- Buoc 1: Doc nhat ky FGA de tim thoi diem su co
-- Tim hanh vi cap nhat trai phep tren HSBA (BS_BAO)
-- ------------------------------------------------------------
SELECT
    TO_CHAR(TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS') AS THOI_GIAN,
    DB_USER,
    STATEMENT_TYPE,
    SCN,
    SUBSTR(SQL_TEXT, 1, 200) AS NOI_DUNG_SQL
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA  = 'BVDBA'
  AND STATEMENT_TYPE = 'UPDATE'
ORDER BY TIMESTAMP DESC;
 
 
-- ------------------------------------------------------------
-- Buoc 2: Kiem tra du lieu TRUOC su co dung Flashback Query
-- Thay TIMESTAMP hoac SCN lay tu ket qua tren
-- ------------------------------------------------------------

-- Buoc 2a: Xem HSBA truoc su co
SELECT MAHSBA, CHANDOAN, DIEUTRI, KETLUAN, MABS
FROM BVDBA.HSBA AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
WHERE MAHSBA = 'HSBA001';

-- Buoc 2b: Xem DONTHUOC truoc su co
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM BVDBA.DONTHUOC AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
WHERE MAHSBA = 'HSBA001';

-- Buoc 2c: Xem HSBA_DV truoc su co
SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM BVDBA.HSBA_DV AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
WHERE MAHSBA = 'HSBA001';
 
 
-- ------------------------------------------------------------
-- Buoc 3: Phuc hoi du lieu tu ket qua Flashback Query
-- Ghi de gia tri bi sua trai phep bang gia tri cu
-- ------------------------------------------------------------
 
-- Phuc hoi HSBA (cap nhat lai CHANDOAN, KETLUAN bi sua trai phep)
UPDATE BVDBA.HSBA target
SET (CHANDOAN, DIEUTRI, KETLUAN) = (
    SELECT CHANDOAN, DIEUTRI, KETLUAN
    FROM BVDBA.HSBA AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
    WHERE MAHSBA = 'HSBA001'
)
WHERE MAHSBA = 'HSBA001';

COMMIT;
 
-- Phuc hoi DONTHUOC (chen lai cac dong bi xoa)
-- Buoc 3a: Xoa cac dong hien tai (neu bi sua sai)
DELETE FROM BVDBA.DONTHUOC WHERE MAHSBA = 'HSBA001';
 
-- Buoc 3b: Chen lai du lieu dung tu flashback
INSERT INTO BVDBA.DONTHUOC (MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG)
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM BVDBA.DONTHUOC AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
WHERE MAHSBA = 'HSBA001';
COMMIT;
 
-- Phuc hoi HSBA_DV (chen lai cac dong dich vu bi xoa)
DELETE FROM BVDBA.HSBA_DV WHERE MAHSBA = 'HSBA001';
 
INSERT INTO BVDBA.HSBA_DV (MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA)
SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM BVDBA.HSBA_DV AS OF SCN 150048789 -- thay SCN thuc te tu FGA log
WHERE MAHSBA = 'HSBA001';
 
COMMIT;
 
 
-- ------------------------------------------------------------
-- Buoc 4: Kiem tra ket qua sau phuc hoi
-- ------------------------------------------------------------
SELECT MAHSBA, CHANDOAN, KETLUAN FROM BVDBA.HSBA WHERE MAHSBA = 'HSBA001';
 
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM BVDBA.DONTHUOC WHERE MAHSBA = 'HSBA001';
 
SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM BVDBA.HSBA_DV WHERE MAHSBA = 'HSBA001';
 
 
-- ------------------------------------------------------------
-- Buoc 5: Kiem tra Undo Retention (anh huong den kha nang Flashback)
-- Chay bang SYS AS SYSDBA neu can chinh
-- ------------------------------------------------------------
-- SHOW PARAMETER undo_retention;
-- Tang undo retention len 3 gio neu can truy van xa hon trong qua khu:
-- ALTER SYSTEM SET UNDO_RETENTION = 10800;
 
-- Kiem tra Undo Tablespace con du dung luong khong
SELECT TABLESPACE_NAME,
       ROUND(SUM(BYTES) / 1048576, 2) AS TOTAL_MB,
       STATUS
FROM DBA_UNDO_EXTENTS
GROUP BY TABLESPACE_NAME, STATUS
ORDER BY TABLESPACE_NAME, STATUS;
 
-- Xem thoi gian undo con giu duoc toi da (phutu thuoc vao undo retention)
SELECT TO_CHAR(SYSDATE - (VALUE/86400), 'YYYY-MM-DD HH24:MI:SS') AS CO_THE_FLASHBACK_TU,
       VALUE || ' giay' AS UNDO_RETENTION_HIEN_TAI
FROM V$PARAMETER
WHERE NAME = 'undo_retention';
 
 
-- ------------------------------------------------------------
-- Mo rong: Flashback Versions Query
-- Xem tat ca phien ban thay doi cua 1 dong theo thoi gian
-- Huu ich khi can truy vet lich su sua doi day du
-- ------------------------------------------------------------
-- L?y SCN hien tai va SCN cu nhat con trong undo
SELECT CURRENT_SCN FROM V$DATABASE;

-- B??c 2: Dung VERSIONS BETWEEN SCN
SELECT VERSIONS_STARTTIME,
       VERSIONS_ENDTIME,
       VERSIONS_OPERATION AS HANH_VI,
       VERSIONS_XID       AS TRANSACTION_ID,
       CHANDOAN,
       KETLUAN,
       MABS
FROM BVDBA.HSBA
VERSIONS BETWEEN SCN MINVALUE AND MAXVALUE
WHERE MAHSBA = 'HSBA001'
ORDER BY VERSIONS_STARTTIME DESC;