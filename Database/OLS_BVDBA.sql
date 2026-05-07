-- ============================================================
-- OLS_BVDBA.sql
-- Chay bang: BVDBA
-- Thu tu: Chay sau OLS_SYS.sql
-- ============================================================
-- Noi dung:
--   Buoc 5: Gan nhan OLS cho du lieu THONGBAO (TB001~TB007)
--   Buoc 7: Tao user demo (u1~u8) va cap quyen SELECT
--   Buoc 8: Cau lenh kiem tra ket qua
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
-- BUOC 7: TAO USER DEMO (u1~u8) VA CAP QUYEN
-- ============================================================
 
DECLARE
    PROCEDURE create_demo_user(p_user VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE
            'CREATE USER ' || p_user ||
            ' IDENTIFIED BY Welcome#123' ||
            ' DEFAULT TABLESPACE USERS QUOTA 0M ON USERS';
        DBMS_OUTPUT.PUT_LINE('Da tao user: ' || p_user);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('User da ton tai hoac loi: ' || p_user || ' - ' || SQLERRM);
    END;
BEGIN
    create_demo_user('U1_GIAMDOC');
    create_demo_user('U2_LDKTIMMACH_HCM');
    create_demo_user('U3_LDKTHANKINH_HN');
    create_demo_user('U4_NVTHANKINH_HCM');
    create_demo_user('U5_NVTIMMACH_HCM');
    create_demo_user('U6_LDPTIMMACH_HCM');
    create_demo_user('U7_LDPTOBO');
    create_demo_user('U8_NVTIEUHOA_HN');
END;
/
 
-- Cap quyen dang nhap
GRANT CREATE SESSION TO
    U1_GIAMDOC, U2_LDKTIMMACH_HCM, U3_LDKTHANKINH_HN,
    U4_NVTHANKINH_HCM, U5_NVTIMMACH_HCM, U6_LDPTIMMACH_HCM,
    U7_LDPTOBO, U8_NVTIEUHOA_HN;
 
-- Cap quyen xem THONGBAO
GRANT SELECT ON THONGBAO TO
    U1_GIAMDOC, U2_LDKTIMMACH_HCM, U3_LDKTHANKINH_HN,
    U4_NVTHANKINH_HCM, U5_NVTIMMACH_HCM, U6_LDPTIMMACH_HCM,
    U7_LDPTOBO, U8_NVTIEUHOA_HN;
 
PROMPT ===== Tao user demo va cap quyen hoan thanh =====
 
 
-- ============================================================
-- BUOC 8: KIEM TRA KET QUA
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
    'U1_GIAMDOC','U2_LDKTIMMACH_HCM','U3_LDKTHANKINH_HN',
    'U4_NVTHANKINH_HCM','U5_NVTIMMACH_HCM','U6_LDPTIMMACH_HCM',
    'U7_LDPTOBO','U8_NVTIEUHOA_HN'
)
ORDER BY USER_NAME;
