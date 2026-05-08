-- ============================================================
-- YEU CAU 4: SAO LUU VA PHUC HOI DU LIEU
-- PHUONG PHAP 1: DATA PUMP (LOGICAL BACKUP)
-- Chay BLOCK A bang: SYS AS SYSDBA
-- Chay BLOCK B bang: BVDBA
-- ============================================================
 
-- ------------------------------------------------------------
-- BLOCK A (SYS AS SYSDBA): Tao thu muc, cap quyen
-- ------------------------------------------------------------
ALTER SESSION SET CONTAINER = XEPDB1;
 
-- Buoc 1: Tao directory object tro den thu muc that tren he thong
-- (Tao thu muc C:\BV_Backup truoc khi chay, hoac doi thanh duong dan nao do tuy nhu cau su dung)
-- Luu y: Do cac service cua Oracle chi cai tren o C nen backup chi duoc setup tren o C
DECLARE
BEGIN
    EXECUTE IMMEDIATE
        'CREATE OR REPLACE DIRECTORY BVDBA_BACKUP_DIR
         AS ''C:\BV_Backup''';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
 
-- Buoc 2: Cap quyen Read/Write tren directory cho BVDBA
GRANT READ, WRITE ON DIRECTORY BVDBA_BACKUP_DIR TO BVDBA;
 
-- Buoc 3: Cap quyen EXP_FULL_DATABASE de export schema nguoi khac (neu can)
GRANT EXP_FULL_DATABASE TO BVDBA;
GRANT IMP_FULL_DATABASE TO BVDBA;
 
-- Kiem tra directory da tao
SELECT DIRECTORY_NAME, DIRECTORY_PATH
FROM DBA_DIRECTORIES
WHERE DIRECTORY_NAME = 'BVDBA_BACKUP_DIR';
 
COMMIT;
 

-- ------------------------------------------------------------
-- BLOCK B (BVDBA): Kiem tra truoc khi backup
-- ------------------------------------------------------------
ALTER SESSION SET CONTAINER = XEPDB1;
 
-- Kiem tra du lieu hien tai truoc khi backup
SELECT 'BENHNHAN' AS BANG, COUNT(*) AS SO_DONG FROM BENHNHAN
UNION ALL
SELECT 'NHANVIEN',  COUNT(*) FROM NHANVIEN
UNION ALL
SELECT 'HSBA',      COUNT(*) FROM HSBA
UNION ALL
SELECT 'HSBA_DV',   COUNT(*) FROM HSBA_DV
UNION ALL
SELECT 'DONTHUOC',  COUNT(*) FROM DONTHUOC;
 

-- ------------------------------------------------------------
-- LENH CHAY TREN TERMINAL (ngoai SQL*Plus):
-- Chay voi quyen Admin hoac mo Powershell
-- Thuc hien export toan bo schema BVDBA ra file .dmp
-- Chay voi quyen SYS hoac BVDBA co quyen EXP_FULL_DATABASE
-- ------------------------------------------------------------
-- Cu phap mau:
-- expdp "bvdba/BvDba#2026@localhost:1521/XEPDB1" schemas=BVDBA directory=BVDBA_BACKUP_DIR dumpfile=bvdba_backup.dmp logfile=bvdba_export.log

-- ------------------------------------------------------------
-- MO PHONG SU CO: Sau khi backup, gia su co hanh vi xau
-- Chay bang: BS_BAO (user khong hop le) hoac BVDBA gia lap
-- ------------------------------------------------------------
-- (Chay bang BVDBA de gia lap su co - xoa du lieu quan trong)
DELETE FROM DONTHUOC WHERE MAHSBA = 'HSBA001';
DELETE FROM HSBA_DV   WHERE MAHSBA = 'HSBA001';
UPDATE HSBA
SET    CHANDOAN = N'Du lieu bi pha hong boi hanh vi trai phep',
       KETLUAN  = NULL
WHERE  MAHSBA = 'HSBA001';
COMMIT;
 
-- Kiem tra du lieu sau su co
SELECT MAHSBA, CHANDOAN, KETLUAN FROM HSBA WHERE MAHSBA = 'HSBA001';
SELECT COUNT(*) AS SO_DONG_DONTHUOC FROM DONTHUOC WHERE MAHSBA = 'HSBA001';
SELECT COUNT(*) AS SO_DONG_HSBA_DV  FROM HSBA_DV  WHERE MAHSBA = 'HSBA001';


-- ------------------------------------------------------------
-- LENH PHUC HOI bang impdp (chay tren terminal):
-- Import lai schema BVDBA tu file .dmp
-- TABLE_EXISTS_ACTION=REPLACE: xoa bang cu, tao lai tu file backup
-- ------------------------------------------------------------
-- Cu phap mau:
-- impdp "bvdba/BvDba#2026@localhost:1521/XEPDB1" schemas=BVDBA directory=BVDBA_BACKUP_DIR dumpfile=bvdba_backup.dmp logfile=bvdba_export.log table_exists_action=REPLACE
 
-- Sau khi import, kiem tra lai so lieu
SELECT 'BENHNHAN' AS BANG, COUNT(*) AS SO_DONG FROM BENHNHAN
UNION ALL
SELECT 'NHANVIEN',  COUNT(*) FROM NHANVIEN
UNION ALL
SELECT 'HSBA',      COUNT(*) FROM HSBA
UNION ALL
SELECT 'HSBA_DV',   COUNT(*) FROM HSBA_DV
UNION ALL
SELECT 'DONTHUOC',  COUNT(*) FROM DONTHUOC;
 
SELECT MAHSBA, CHANDOAN, KETLUAN FROM HSBA WHERE MAHSBA = 'HSBA001';