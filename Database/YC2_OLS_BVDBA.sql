-- ============================================================
-- OLS_BVDBA.sql
-- Chay bang: BVDBA
-- Thu tu: Chay sau OLS_SYS.sql
-- ============================================================
-- Noi dung:
--   Buoc 5: Gan nhan OLS cho du lieu THONGBAO (TB001~TB007)
--   Buoc 6: Cau lenh kiem tra ket qua
-- ============================================================
 
 
-- ============================================================
-- BUOC 5: GAN NHAN CHO DU LIEU THONGBAO
-- ============================================================

-- TB001 -> t1: NV (moi nhan vien, moi khoa, moi co so)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'NV')
WHERE MATHONGBAO = 'TB001';
 
-- TB002 -> t2: BGD (chi Ban Giam Doc)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'BGD')
WHERE MATHONGBAO = 'TB002';
 
-- TB003 -> t3: LDK (lanh dao khoa, moi khoa, moi co so)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'LDK')
WHERE MATHONGBAO = 'TB003';
 
-- TB004 -> t4: LDK:TH (lanh dao Khoa Tieu hoa, moi co so)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'LDK:TH')
WHERE MATHONGBAO = 'TB004';
 
-- TB005 -> t5: NV:TH:HCM (nhan vien Khoa Tieu hoa tai HCM)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'NV:TH:HCM')
WHERE MATHONGBAO = 'TB005';
 
-- TB006 -> t6: NV:TH:HN (nhan vien Khoa Tieu hoa tai Ha Noi)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'NV:TH:HN')
WHERE MATHONGBAO = 'TB006';
 
-- TB007 -> t7: LDK:TH,TK:HP (lanh dao Khoa TH va TK tai Hai Phong)
UPDATE THONGBAO SET OLS_LABEL = CHAR_TO_LABEL('BENHVIEN_POL', 'LDK:TH,TK:HP')
WHERE MATHONGBAO = 'TB007';
 
COMMIT;
 
PROMPT ===== Gan nhan THONGBAO hoan thanh =====
 
-- Kiem tra nhan vua gan
SELECT MATHONGBAO,
       TO_CHAR(OLS_LABEL) AS NHAN_OLS,
       SUBSTR(NOIDUNG,1,50)
FROM THONGBAO;
 
 
-- ============================================================
-- BUOC 6: KIEM TRA KET QUA
-- ============================================================
 
-- Kiem tra nhan nguoi dung
SELECT USER_NAME,
       MAX_READ_LABEL  AS NHAN_DOC,
       MAX_WRITE_LABEL AS NHAN_GHI,
       DEFAULT_READ_LABEL,
       DEFAULT_WRITE_LABEL
FROM DBA_SA_USER_LABELS
WHERE POLICY_NAME = 'BENHVIEN_POL'
AND USER_NAME IN (
    'U1', 'U2', 'U3', 'U4', 'U5', 'U6', 'U7', 'U8'
)
ORDER BY USER_NAME;
