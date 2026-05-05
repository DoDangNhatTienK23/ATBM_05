-- ============================================================
-- YEU CAU 4 - CAU 2: PHUC HOI DUA TREN AUDIT LOG (YEU CAU 3)
-- Chay bang: BVDBA (hoac SYS AS SYSDBA cho Flashback Database)
-- Tao thu muc truoc: C:\backup\benhvien\
-- ============================================================
-- CAC TINH HUONG SU CO VA PHUC HOI:
--   TH1: Bac si sua nham CHANDOAN/DIEUTRI/KETLUAN  -> FGA_HSBA_BS_UPDATE
--   TH2: Cap nhat bat hop phap CHANDOAN/DIEUTRI/KETLUAN -> FGA_HSBA_ILLEGAL_UPDATE  
--   TH3: DML bat hop phap tren HSBA_DV             -> FGA_HSBA_DV_ILLEGAL_DML
--   TH4: Update don thuoc sau khi da tao            -> FGA_DT_UPDATE_AFTER_CREATE
-- ============================================================


-- ============================================================
-- BUOC 0: CAU HINH TRUOC KHI PHUC HOI
-- Chay bang: SYS AS SYSDBA
-- ============================================================

-- Bat ARCHIVELOG mode (neu chua bat)
-- SHUTDOWN IMMEDIATE;
-- STARTUP MOUNT;
-- ALTER DATABASE ARCHIVELOG;
-- ALTER DATABASE OPEN;

-- Bat Flashback Database va cau hinh FRA
ALTER SYSTEM SET DB_RECOVERY_FILE_DEST      = 'C:\backup\benhvien\fra' SCOPE = BOTH;
ALTER SYSTEM SET DB_RECOVERY_FILE_DEST_SIZE = 10G                      SCOPE = BOTH;
ALTER DATABASE FLASHBACK ON;

-- Bat ROW MOVEMENT cho cac bang can Flashback Table
ALTER TABLE BVDBA.HSBA     ENABLE ROW MOVEMENT;
ALTER TABLE BVDBA.DONTHUOC ENABLE ROW MOVEMENT;
ALTER TABLE BVDBA.HSBA_DV  ENABLE ROW MOVEMENT;

-- Kiem tra cau hinh
SELECT LOG_MODE, FLASHBACK_ON FROM V$DATABASE;


-- ============================================================
-- BUOC 1: DOC AUDIT LOG DE PHAT HIEN SU CO
-- Chay bang: BVDBA
-- ============================================================

-- ---- [A] Doc FGA log - co SCN chinh xac ----
-- Day la nguon chinh xac nhat de xac dinh thoi diem phuc hoi
-- SCN (System Change Number) la con so nhan dang duy nhat
-- moi thay doi trong Oracle deu co SCN tuong ung

SELECT
    TO_CHAR(TIMESTAMP, 'DD/MM/YYYY HH24:MI:SS') AS THOI_GIAN,
    DB_USER                                       AS NGUOI_DUNG,
    OBJECT_NAME                                   AS BANG_BI_TAC_DONG,
    POLICY_NAME                                   AS LOAI_GIAM_SAT,
    STATEMENT_TYPE                                AS HANH_VI,
    SCN                                           AS MA_SCN,
    SUBSTR(SQL_TEXT, 1, 300)                      AS NOI_DUNG_SQL
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
  AND OBJECT_NAME IN ('HSBA', 'HSBA_DV', 'DONTHUOC')
ORDER BY TIMESTAMP DESC;


-- ---- [B] Doc Standard Audit log - hanh vi that bai ----
SELECT
    TO_CHAR(TIMESTAMP, 'DD/MM/YYYY HH24:MI:SS')  AS THOI_GIAN,
    USERNAME                                       AS NGUOI_DUNG,
    OBJ_NAME                                       AS BANG_BI_TAC_DONG,
    ACTION_NAME                                    AS HANH_VI,
    RETURNCODE                                     AS MA_LOI_ORA,
    SUBSTR(SQL_TEXT, 1, 300)                       AS NOI_DUNG_SQL
FROM DBA_AUDIT_TRAIL
WHERE OWNER      = 'BVDBA'
  AND RETURNCODE != 0
ORDER BY TIMESTAMP DESC;


-- ---- [C] Tim SCN cua hanh vi bat hop phap cu the ----
-- Gia su phat hien UPDATE bat hop phap tren HSBA luc 14:30 ngay 01/05/2026
-- Truy van tim SCN cua hanh vi do:

SELECT
    SCN                                           AS SCN_SU_CO,
    SCN - 1                                       AS SCN_TRUOC_SU_CO,
    TO_CHAR(TIMESTAMP, 'DD/MM/YYYY HH24:MI:SS')  AS THOI_GIAN_SU_CO,
    DB_USER                                       AS NGUOI_GAY_RA,
    POLICY_NAME,
    SUBSTR(SQL_TEXT, 1, 300)                      AS CAU_SQL_GAY_SU_CO
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
  AND OBJECT_NAME   = 'HSBA'
  AND STATEMENT_TYPE = 'UPDATE'
  -- Loc theo thoi gian gan su co
  AND TIMESTAMP >= TO_TIMESTAMP('01/05/2026 14:00:00', 'DD/MM/YYYY HH24:MI:SS')
  AND TIMESTAMP <= TO_TIMESTAMP('01/05/2026 15:00:00', 'DD/MM/YYYY HH24:MI:SS')
ORDER BY TIMESTAMP DESC;

-- Sau khi co SCN_SU_CO, dung SCN_TRUOC_SU_CO (= SCN_SU_CO - 1)
-- de phuc hoi ve trang thai truoc khi su co xay ra


-- ============================================================
-- BUOC 2: XAC NHAN DU LIEU TRUOC VA SAU SU CO
-- Dung Flashback Query kiem tra truoc khi thuc hien phuc hoi
-- ============================================================

-- Gia su SCN_TRUOC_SU_CO = 1234566 (lay tu ket qua Buoc 1)
-- Thay 1234566 bang SCN thuc te tu ket qua truy van o Buoc 1

-- Xem du lieu HSBA tai SCN truoc su co
SELECT MAHSBA, MABN, CHANDOAN, DIEUTRI, KETLUAN
FROM BVDBA.HSBA
AS OF SCN 1234566    -- <- thay bang SCN_TRUOC_SU_CO thuc te
ORDER BY MAHSBA;

-- So sanh voi du lieu hien tai (sau su co)
SELECT MAHSBA, MABN, CHANDOAN, DIEUTRI, KETLUAN
FROM BVDBA.HSBA
ORDER BY MAHSBA;

-- Xem du lieu DONTHUOC truoc su co
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM BVDBA.DONTHUOC
AS OF SCN 1234566    -- <- thay bang SCN_TRUOC_SU_CO thuc te
ORDER BY MAHSBA;

-- Xem du lieu HSBA_DV truoc su co
SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM BVDBA.HSBA_DV
AS OF SCN 1234566    -- <- thay bang SCN_TRUOC_SU_CO thuc te
ORDER BY MAHSBA;


-- ============================================================
-- BUOC 3: THUC HIEN PHUC HOI
-- Chon phuong phap phu hop voi muc do su co
-- ============================================================


-- ---- TINH HUONG 1: Su co nho - Chi 1 bang bi anh huong ----
-- Phuong phap: FLASHBACK TABLE (nhanh nhat, khong can backup)
-- Ap dung khi: Bac si sua nham CHANDOAN/DIEUTRI/KETLUAN

-- Phuc hoi bang HSBA ve truoc su co dua tren SCN tu FGA log
FLASHBACK TABLE BVDBA.HSBA
TO SCN 1234566;    -- <- SCN_TRUOC_SU_CO tu DBA_FGA_AUDIT_TRAIL

-- Hoac dung timestamp tu Standard Audit log
-- FLASHBACK TABLE BVDBA.HSBA
-- TO TIMESTAMP TO_TIMESTAMP('01/05/2026 14:29:00', 'DD/MM/YYYY HH24:MI:SS');

-- Kiem tra sau phuc hoi
SELECT MAHSBA, CHANDOAN, DIEUTRI, KETLUAN FROM BVDBA.HSBA ORDER BY MAHSBA;


-- ---- TINH HUONG 2: Su co tren DONTHUOC ----
-- Ap dung khi: Bac si cap nhat don thuoc sau khi da tao (FGA_DT_UPDATE_AFTER_CREATE)

-- Tim SCN su co tu FGA log
SELECT SCN - 1 AS SCN_PHUC_HOI, TIMESTAMP, DB_USER, SQL_TEXT
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
  AND OBJECT_NAME   = 'DONTHUOC'
  AND POLICY_NAME   = 'FGA_DT_UPDATE_AFTER_CREATE'
ORDER BY TIMESTAMP DESC
FETCH FIRST 5 ROWS ONLY;

-- Phuc hoi bang DONTHUOC
FLASHBACK TABLE BVDBA.DONTHUOC
TO SCN 1234566;    -- <- thay bang SCN thuc te

-- Kiem tra
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG FROM BVDBA.DONTHUOC ORDER BY MAHSBA;


-- ---- TINH HUONG 3: Su co tren HSBA_DV ----
-- Ap dung khi: DML bat hop phap tren HSBA_DV (FGA_HSBA_DV_ILLEGAL_DML)

-- Tim SCN su co
SELECT SCN - 1 AS SCN_PHUC_HOI, TIMESTAMP, DB_USER, STATEMENT_TYPE, SQL_TEXT
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
  AND OBJECT_NAME   = 'HSBA_DV'
  AND POLICY_NAME   = 'FGA_HSBA_DV_ILLEGAL_DML'
ORDER BY TIMESTAMP DESC
FETCH FIRST 5 ROWS ONLY;

-- Phuc hoi bang HSBA_DV
FLASHBACK TABLE BVDBA.HSBA_DV
TO SCN 1234566;    -- <- thay bang SCN thuc te


-- ---- TINH HUONG 4: Su co lon - Nhieu bang bi anh huong ----
-- Phuong phap: FLASHBACK DATABASE
-- Ap dung khi: Nhieu bang bi thay doi cung luc, can phuc hoi toan bo

-- Tim SCN som nhat cua toan bo su co
SELECT MIN(SCN) - 1 AS SCN_PHUC_HOI_TOAN_BO
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
  AND TIMESTAMP >= TO_TIMESTAMP('01/05/2026 14:00:00', 'DD/MM/YYYY HH24:MI:SS');

-- Thuc hien trong RMAN (CMD):
/*
    rman target sys@XEPDB1

    SHUTDOWN IMMEDIATE;
    STARTUP MOUNT;

    -- Thay 1234566 bang SCN_PHUC_HOI_TOAN_BO tu ket qua truy van tren
    FLASHBACK DATABASE TO SCN 1234566;

    -- Kiem tra du lieu truoc khi mo chinh thuc
    ALTER DATABASE OPEN READ ONLY;

    -- Neu du lieu dung:
    SHUTDOWN IMMEDIATE;
    STARTUP MOUNT;
    ALTER DATABASE OPEN RESETLOGS;
*/


-- ---- TINH HUONG 5: Su co rat nghiem trong - Mat du lieu hoac backup FGA ----
-- Phuong phap: RMAN RESTORE + RECOVER den SCN xac dinh tu audit log
-- Ap dung khi: Flashback logs bi xoa hoac FRA het cho

-- Thuc hien trong RMAN (CMD):
/*
    rman target sys@XEPDB1

    STARTUP MOUNT;

    -- Phuc hoi den SCN truoc su co (lay tu DBA_FGA_AUDIT_TRAIL)
    RUN {
        SET UNTIL SCN 1234566;
        RESTORE DATABASE;
        RECOVER DATABASE;
        ALTER DATABASE OPEN RESETLOGS;
    }
*/


-- ============================================================
-- BUOC 4: XAC NHAN PHUC HOI THANH CONG
-- ============================================================

-- Kiem tra du lieu sau khi phuc hoi
SELECT 'HSBA'     AS BANG, COUNT(*) AS SO_DONG FROM BVDBA.HSBA
UNION ALL
SELECT 'HSBA_DV'  AS BANG, COUNT(*) AS SO_DONG FROM BVDBA.HSBA_DV
UNION ALL
SELECT 'DONTHUOC' AS BANG, COUNT(*) AS SO_DONG FROM BVDBA.DONTHUOC;

-- Kiem tra du lieu cu the cua HSBA
SELECT MAHSBA, MABN, CHANDOAN, DIEUTRI, KETLUAN
FROM BVDBA.HSBA
ORDER BY MAHSBA;

-- Kiem tra xem audit log co ghi nhan hanh vi phuc hoi khong
SELECT
    TO_CHAR(TIMESTAMP, 'DD/MM/YYYY HH24:MI:SS') AS THOI_GIAN,
    DB_USER,
    POLICY_NAME,
    STATEMENT_TYPE,
    SUBSTR(SQL_TEXT, 1, 200) AS SQL_TEXT
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
ORDER BY TIMESTAMP DESC
FETCH FIRST 20 ROWS ONLY;

-- Tat ROW MOVEMENT sau khi phuc hoi xong
ALTER TABLE BVDBA.HSBA     DISABLE ROW MOVEMENT;
ALTER TABLE BVDBA.DONTHUOC DISABLE ROW MOVEMENT;
ALTER TABLE BVDBA.HSBA_DV  DISABLE ROW MOVEMENT;

COMMIT;


-- ============================================================
-- BUOC 5: SAO LUU SAU KHI PHUC HOI
-- Dam bao co ban sao sach sau khi da phuc hoi
-- ============================================================

-- Thuc hien trong RMAN (CMD) sau khi phuc hoi xong:
/*
    rman target sys@XEPDB1

    RUN {
        ALLOCATE CHANNEL ch1 TYPE DISK
            FORMAT 'C:\backup\benhvien\after_recovery_%d_%T_%s_%p.bak';
        BACKUP DATABASE PLUS ARCHIVELOG;
        DELETE OBSOLETE;
        RELEASE CHANNEL ch1;
    }
*/

-- Kiem tra danh sach backup sau khi hoan tat
-- (Chay trong RMAN):
-- LIST BACKUP SUMMARY;
