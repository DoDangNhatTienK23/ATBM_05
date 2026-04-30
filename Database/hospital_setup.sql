-- ============================================================
-- PHAN HE TICH HOP: ADMIN MODULE (PH1) -> HE THONG Y TE (PH2)
-- Chay thu tu:
--   BLOCK A: SYS AS SYSDBA
--   BLOCK B: BVDBA
-- ============================================================


-- ============================================================
-- BLOCK A: SYS AS SYSDBA
-- Tao BVDBA - DBA cua toan he thong
-- (Bo qua neu BVDBA da ton tai)
-- ============================================================
ALTER SESSION SET CONTAINER = XEPDB1;

-- Tao tablespace (neu chua co)
DECLARE
BEGIN
    EXECUTE IMMEDIATE
        'CREATE TABLESPACE BENHVIEN_TBS
         DATAFILE ''benhvien01.dbf'' SIZE 100M
         AUTOEXTEND ON NEXT 50M MAXSIZE 1G
         EXTENT MANAGEMENT LOCAL SEGMENT SPACE MANAGEMENT AUTO';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- Tao BVDBA
DECLARE
BEGIN
    EXECUTE IMMEDIATE
        'CREATE USER BVDBA
         IDENTIFIED BY "BvDba#2026"
         DEFAULT TABLESPACE BENHVIEN_TBS
         QUOTA UNLIMITED ON BENHVIEN_TBS
         PROFILE DEFAULT';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

GRANT DBA                         TO BVDBA;
GRANT CREATE USER                 TO BVDBA;
GRANT ALTER USER                  TO BVDBA;
GRANT DROP USER                   TO BVDBA;
GRANT CREATE ROLE                 TO BVDBA;
GRANT CREATE VIEW                 TO BVDBA;
GRANT DROP ANY ROLE               TO BVDBA;
GRANT GRANT ANY ROLE              TO BVDBA;
GRANT GRANT ANY PRIVILEGE         TO BVDBA;
GRANT GRANT ANY OBJECT PRIVILEGE  TO BVDBA;

-- Quyen doc data dictionary (ho tro Phan he 1)
GRANT SELECT ON DBA_USERS         TO BVDBA;
GRANT SELECT ON DBA_ROLES         TO BVDBA;
GRANT SELECT ON DBA_ROLE_PRIVS    TO BVDBA;
GRANT SELECT ON DBA_SYS_PRIVS     TO BVDBA;
GRANT SELECT ON DBA_TAB_PRIVS     TO BVDBA;
GRANT SELECT ON DBA_COL_PRIVS     TO BVDBA;
GRANT SELECT ON DBA_OBJECTS       TO BVDBA;
GRANT SELECT ON DBA_TABLES        TO BVDBA;
GRANT SELECT ON DBA_VIEWS         TO BVDBA;
GRANT SELECT ON DBA_PROCEDURES    TO BVDBA;
GRANT SELECT_CATALOG_ROLE         TO BVDBA;

COMMIT;















-- BLOCK B: BVDBA


-- ============================================================
-- HE THONG QUAN LY DU LIEU Y TE - BENH VIEN X
-- Database: Oracle
-- Mo ta: Tao bang + du lieu mau phuc vu cac yeu cau
--        TC#1 ~ TC#5, RBAC, VPD, OLS, Audit, Backup
-- ============================================================

-- ============================================================
-- PHAN 0: XOA BANG CU (neu ton tai) - chay truoc khi setup
-- ============================================================
ALTER SESSION SET CONTAINER = XEPDB1;
SHOW CON_NAME;
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE DONTHUOC CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE HSBA_DV CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE HSBA CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE BENHNHAN CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE NHANVIEN CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE THONGBAO CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- ============================================================
-- PHAN 1: TAO CAC BANG DU LIEU
-- ============================================================

-- Bang BENHNHAN
CREATE TABLE BENHNHAN (
    MABN        VARCHAR2(10)    NOT NULL,
    TENBN       NVARCHAR2(100)  NOT NULL,
    PHAI        NVARCHAR2(3)        NOT NULL,   -- 'Nam' hoac 'Nu'
    NGAYSINH    DATE            NOT NULL,
    CCCD        VARCHAR2(12)    NOT NULL,
    SONHA       NVARCHAR2(20),
    TENDUONG    NVARCHAR2(100),
    QUANHUYEN   NVARCHAR2(100),
    TINHTHANH   NVARCHAR2(100),
    TIENSUBENH  NVARCHAR2(500),
    TIENSUBENHGD NVARCHAR2(500),
    DIUNGthuoc  NVARCHAR2(300),
    CONSTRAINT PK_BENHNHAN PRIMARY KEY (MABN),
    CONSTRAINT UK_BENHNHAN_CCCD UNIQUE (CCCD),
    CONSTRAINT CK_BENHNHAN_PHAI CHECK (PHAI IN (N'Nam', N'Nu'))
);

COMMENT ON TABLE  BENHNHAN              IS 'Quan he luu tru thong tin benh nhan';
COMMENT ON COLUMN BENHNHAN.MABN         IS 'Ma benh nhan - do benh vien cap, duy nhat';
COMMENT ON COLUMN BENHNHAN.CCCD         IS 'Can cuoc cong dan';
COMMENT ON COLUMN BENHNHAN.TIENSUBENH   IS 'Tien su benh ca nhan';
COMMENT ON COLUMN BENHNHAN.TIENSUBENHGD IS 'Tien su benh gia dinh';
COMMENT ON COLUMN BENHNHAN.DIUNGthuoc   IS 'Tinh trang di ung thuoc (neu co)';


-- Bang NHANVIEN
CREATE TABLE NHANVIEN (
    MANV        VARCHAR2(10)    NOT NULL,
    HOTEN       NVARCHAR2(100)  NOT NULL,
    PHAI        NVARCHAR2(3)        NOT NULL,
    NGAYSINH    DATE            NOT NULL,
    CMND        VARCHAR2(12)    NOT NULL,
    QUEQUAN     NVARCHAR2(200),
    SODT        VARCHAR2(15),
    VAITRO      NVARCHAR2(50)   NOT NULL,
    CHUYENKHOA  NVARCHAR2(100),
    -- Lien ket Oracle user account (TC#1: dung Oracle USERNAME, khong luu password), nếu ko dùng thì xóa!
    ORACLE_USERNAME VARCHAR2(30),
    CONSTRAINT PK_NHANVIEN PRIMARY KEY (MANV),
    CONSTRAINT UK_NHANVIEN_CMND UNIQUE (CMND),
    CONSTRAINT UK_NHANVIEN_ORACLE_USER UNIQUE (ORACLE_USERNAME),
    CONSTRAINT CK_NHANVIEN_PHAI CHECK (PHAI IN (N'Nam', N'Nu')),
    CONSTRAINT CK_NHANVIEN_VAITRO CHECK (
        VAITRO IN (
            N'Dieu phoi vien',
            N'Bac si/Y si',
            N'Ky thuat vien',
            N'Benh nhan'
        )
    )
);

-- Them cot ORACLE_USERNAME vao BENHNHAN (TC#1)
ALTER TABLE BENHNHAN ADD (
    ORACLE_USERNAME VARCHAR2(30),
    CONSTRAINT UK_BENHNHAN_ORACLE_USER UNIQUE (ORACLE_USERNAME)
);

COMMENT ON COLUMN NHANVIEN.ORACLE_USERNAME IS 'Ten tai khoan Oracle tuong ung (TC#1)';
COMMENT ON COLUMN BENHNHAN.ORACLE_USERNAME IS 'Ten tai khoan Oracle tuong ung (TC#1)';


-- Bang HSBA (Ho so benh an)
CREATE TABLE HSBA (
    MAHSBA      VARCHAR2(15)    NOT NULL,
    MABN        VARCHAR2(10)    NOT NULL,
    NGAY        DATE            NOT NULL,
    CHANDOAN    NVARCHAR2(500),
    DIEUTRI     NVARCHAR2(500),
    MABS        VARCHAR2(10)    NOT NULL,   -- Bac si / Y si dieu tri
    MAKHOA      VARCHAR2(10)    NOT NULL,   -- Ma khoa tiep nhan
    KETLUAN     NVARCHAR2(500),
    CONSTRAINT PK_HSBA PRIMARY KEY (MAHSBA),
    CONSTRAINT FK_HSBA_MABN FOREIGN KEY (MABN)
        REFERENCES BENHNHAN(MABN),
    CONSTRAINT FK_HSBA_MABS FOREIGN KEY (MABS)
        REFERENCES NHANVIEN(MANV)
);

COMMENT ON TABLE  HSBA         IS 'Ho so benh an';
COMMENT ON COLUMN HSBA.MABS    IS 'Ma bac si / y si dieu tri';
COMMENT ON COLUMN HSBA.MAKHOA  IS 'Ma khoa tiep nhan (TH=Tieu hoa, TK=Than kinh, TM=Tim mach)';


-- Bang HSBA_DV (Dich vu ho tro chan doan)
CREATE TABLE HSBA_DV (
    MAHSBA      VARCHAR2(15)    NOT NULL,
    LOAIDV      NVARCHAR2(100)  NOT NULL,   -- Loai dich vu: xet nghiem, chup hinh...
    NGAYDV      DATE            NOT NULL,
    MAKTV       VARCHAR2(10)    NOT NULL,   -- Ky thuat vien thuc hien
    KETQUA      NVARCHAR2(1000),
    CONSTRAINT PK_HSBA_DV PRIMARY KEY (MAHSBA, LOAIDV, NGAYDV),
    CONSTRAINT FK_HSBA_DV_MAHSBA FOREIGN KEY (MAHSBA)
        REFERENCES HSBA(MAHSBA),
    CONSTRAINT FK_HSBA_DV_MAKTV FOREIGN KEY (MAKTV)
        REFERENCES NHANVIEN(MANV)
);

COMMENT ON TABLE  HSBA_DV        IS 'Dich vu ho tro chan doan lien ket voi HSBA';
COMMENT ON COLUMN HSBA_DV.MAKTV  IS 'Ky thuat vien duoc dieu phoi thuc hien dich vu';
COMMENT ON COLUMN HSBA_DV.KETQUA IS 'Ket qua dich vu (cap nhat boi KTV, duoc ghi vet)';


-- Bang DONTHUOC
CREATE TABLE DONTHUOC (
    MAHSBA      VARCHAR2(15)    NOT NULL,
    NGAYDT      DATE            NOT NULL,
    TENTHUOC    NVARCHAR2(200)  NOT NULL,
    LIEUDUNG    NVARCHAR2(300)  NOT NULL,
    CONSTRAINT PK_DONTHUOC PRIMARY KEY (MAHSBA, NGAYDT, TENTHUOC),
    CONSTRAINT FK_DONTHUOC_MAHSBA FOREIGN KEY (MAHSBA)
        REFERENCES HSBA(MAHSBA)
);

COMMENT ON TABLE DONTHUOC IS 'Don thuoc do y si / bac si chi dinh cho benh nhan qua HSBA';


-- Bang THONGBAO (Yeu cau 2 - OLS)
CREATE TABLE THONGBAO (
    MATHONGBAO  VARCHAR2(10)    NOT NULL,
    NOIDUNG     NVARCHAR2(1000) NOT NULL,
    NGAYGIO     TIMESTAMP       NOT NULL,
    DIADIEM     NVARCHAR2(200),
    -- Cot nhan OLS se duoc them sau khi kich hoat OLS
    -- (Oracle Label Security tu dong them cot LABEL kieu NUMBER)
    CONSTRAINT PK_THONGBAO PRIMARY KEY (MATHONGBAO)
);

COMMENT ON TABLE THONGBAO IS 'Thong bao hop khan - su dung OLS de phan phat theo nhan';


-- ============================================================
-- PHAN 2: INDEX HO TRO TRUY VAN
-- ============================================================

CREATE INDEX IDX_HSBA_MABN   ON HSBA(MABN);
CREATE INDEX IDX_HSBA_MABS   ON HSBA(MABS);
CREATE INDEX IDX_HSBA_DV_KTV ON HSBA_DV(MAKTV);
CREATE INDEX IDX_NV_VAITRO   ON NHANVIEN(VAITRO);

-- Fix loi ORA-01408: "such column list already indexed"
-- Do cot ORACLE_USERNAME da co rang buoc UNIQUE luc CREATE TABLE nen Oracle da tu dong tao index.
-- CREATE INDEX IDX_NV_ORAUSER  ON NHANVIEN(ORACLE_USERNAME);
-- CREATE INDEX IDX_BN_ORAUSER  ON BENHNHAN(ORACLE_USERNAME);


-- ============================================================
-- PHAN 3: INSERT DU LIEU MAU - NHANVIEN
-- 20 Dieu phoi vien, 10 Bac si/Y si (dai dien 100), 5 KTV (dai dien 50)
-- ============================================================

-- ---- DIEU PHOI VIEN (20 nguoi) ----
INSERT INTO NHANVIEN VALUES ('NV001', N'Nguyễn Thị Lan', N'Nu',  DATE '1985-03-12', '001234567890', N'Hà Nội',     '0901000001', N'Dieu phoi vien', NULL, 'dpv_lan');
INSERT INTO NHANVIEN VALUES ('NV002', N'Trần Văn Minh',  N'Nam', DATE '1982-07-25', '001234567891', N'Hồ Chí Minh','0901000002', N'Dieu phoi vien', NULL, 'dpv_minh');
INSERT INTO NHANVIEN VALUES ('NV003', N'Lê Thị Hoa',     N'Nu',  DATE '1990-11-08', '001234567892', N'Đà Nẵng',    '0901000003', N'Dieu phoi vien', NULL, 'dpv_hoa');
INSERT INTO NHANVIEN VALUES ('NV004', N'Phạm Quốc Hùng', N'Nam', DATE '1988-05-17', '001234567893', N'Hải Phòng',  '0901000004', N'Dieu phoi vien', NULL, 'dpv_hung');
INSERT INTO NHANVIEN VALUES ('NV005', N'Võ Thị Mai',     N'Nu',  DATE '1992-09-30', '001234567894', N'Cần Thơ',    '0901000005', N'Dieu phoi vien', NULL, 'dpv_mai');
INSERT INTO NHANVIEN VALUES ('NV006', N'Hoàng Văn Tuấn', N'Nam', DATE '1986-01-20', '001234567895', N'Huế',        '0901000006', N'Dieu phoi vien', NULL, 'dpv_tuan');
INSERT INTO NHANVIEN VALUES ('NV007', N'Đặng Thị Thu',   N'Nu',  DATE '1993-06-14', '001234567896', N'Quảng Nam',  '0901000007', N'Dieu phoi vien', NULL, 'dpv_thu');
INSERT INTO NHANVIEN VALUES ('NV008', N'Bùi Minh Khoa',  N'Nam', DATE '1987-12-03', '001234567897', N'Bình Dương', '0901000008', N'Dieu phoi vien', NULL, 'dpv_khoa');
INSERT INTO NHANVIEN VALUES ('NV009', N'Nguyễn Văn Đức', N'Nam', DATE '1984-04-22', '001234567898', N'Long An',    '0901000009', N'Dieu phoi vien', NULL, 'dpv_duc');
INSERT INTO NHANVIEN VALUES ('NV010', N'Trần Thị Ngọc',  N'Nu',  DATE '1991-08-16', '001234567899', N'Tiền Giang', '0901000010', N'Dieu phoi vien', NULL, 'dpv_ngoc');
INSERT INTO NHANVIEN VALUES ('NV011', N'Lý Văn Phúc',    N'Nam', DATE '1983-02-28', '001234567810', N'Đồng Nai',   '0901000011', N'Dieu phoi vien', NULL, 'dpv_phuc');
INSERT INTO NHANVIEN VALUES ('NV012', N'Đinh Thị Yến',   N'Nu',  DATE '1994-10-05', '001234567811', N'Bà Rịa',     '0901000012', N'Dieu phoi vien', NULL, 'dpv_yen');
INSERT INTO NHANVIEN VALUES ('NV013', N'Phan Quang Vinh', N'Nam', DATE '1989-07-19', '001234567812', N'Khánh Hòa',  '0901000013', N'Dieu phoi vien', NULL, 'dpv_vinh');
INSERT INTO NHANVIEN VALUES ('NV014', N'Ngô Thị Bích',   N'Nu',  DATE '1995-03-07', '001234567813', N'Nghệ An',    '0901000014', N'Dieu phoi vien', NULL, 'dpv_bich');
INSERT INTO NHANVIEN VALUES ('NV015', N'Trịnh Văn Long',  N'Nam', DATE '1981-11-25', '001234567814', N'Thanh Hóa',  '0901000015', N'Dieu phoi vien', NULL, 'dpv_long');
INSERT INTO NHANVIEN VALUES ('NV016', N'Huỳnh Thị Kim',  N'Nu',  DATE '1990-05-13', '001234567815', N'Bình Định',  '0901000016', N'Dieu phoi vien', NULL, 'dpv_kim');
INSERT INTO NHANVIEN VALUES ('NV017', N'Đỗ Quốc Bảo',    N'Nam', DATE '1986-09-01', '001234567816', N'Gia Lai',    '0901000017', N'Dieu phoi vien', NULL, 'dpv_bao');
INSERT INTO NHANVIEN VALUES ('NV018', N'Vũ Thị Thanh',   N'Nu',  DATE '1993-12-18', '001234567817', N'Tây Ninh',   '0901000018', N'Dieu phoi vien', NULL, 'dpv_thanh');
INSERT INTO NHANVIEN VALUES ('NV019', N'Cao Minh Trí',   N'Nam', DATE '1988-04-09', '001234567818', N'Bến Tre',    '0901000019', N'Dieu phoi vien', NULL, 'dpv_tri');
INSERT INTO NHANVIEN VALUES ('NV020', N'Lương Thị Hằng', N'Nu',  DATE '1992-06-27', '001234567819', N'Vĩnh Long',  '0901000020', N'Dieu phoi vien', NULL, 'dpv_hang');

-- ---- BAC SI / Y SI (10 nguoi dai dien cho 100) ----
-- Khoa Tieu hoa (TH)
INSERT INTO NHANVIEN VALUES ('BS001', N'PGS.TS Nguyễn Văn An',   N'Nam', DATE '1975-05-10', '002345678901', N'Hà Nội',     '0902000001', N'Bac si/Y si', N'Tieu hoa',  'bs_an');
INSERT INTO NHANVIEN VALUES ('BS002', N'TS Trần Thị Bảo',        N'Nu',  DATE '1978-08-22', '002345678902', N'Hồ Chí Minh','0902000002', N'Bac si/Y si', N'Tieu hoa',  'bs_bao');
INSERT INTO NHANVIEN VALUES ('BS003', N'BS Lê Minh Cường',       N'Nam', DATE '1983-11-15', '002345678903', N'Đà Nẵng',    '0902000003', N'Bac si/Y si', N'Tieu hoa',  'bs_cuong');
-- Khoa Than kinh (TK)
INSERT INTO NHANVIEN VALUES ('BS004', N'PGS.TS Phạm Thị Duyên',  N'Nu',  DATE '1972-03-30', '002345678904', N'Hải Phòng',  '0902000004', N'Bac si/Y si', N'Than kinh', 'bs_duyen');
INSERT INTO NHANVIEN VALUES ('BS005', N'BS Võ Quang Đại',        N'Nam', DATE '1980-07-06', '002345678905', N'Cần Thơ',    '0902000005', N'Bac si/Y si', N'Than kinh', 'bs_dai');
INSERT INTO NHANVIEN VALUES ('BS006', N'TS Hoàng Thị Giang',     N'Nu',  DATE '1977-12-19', '002345678906', N'Huế',        '0902000006', N'Bac si/Y si', N'Than kinh', 'bs_giang');
-- Khoa Tim mach (TM)
INSERT INTO NHANVIEN VALUES ('BS007', N'GS.TS Đặng Văn Hải',     N'Nam', DATE '1968-09-04', '002345678907', N'Quảng Ngãi', '0902000007', N'Bac si/Y si', N'Tim mach',  'bs_hai');
INSERT INTO NHANVIEN VALUES ('BS008', N'PGS Bùi Thị Hồng',       N'Nu',  DATE '1974-04-28', '002345678908', N'Bình Dương', '0902000008', N'Bac si/Y si', N'Tim mach',  'bs_hong');
INSERT INTO NHANVIEN VALUES ('BS009', N'BS Nguyễn Quốc Khánh',   N'Nam', DATE '1985-01-17', '002345678909', N'Long An',    '0902000009', N'Bac si/Y si', N'Tim mach',  'bs_khanh');
INSERT INTO NHANVIEN VALUES ('BS010', N'BS Trần Thị Lan',         N'Nu',  DATE '1982-06-09', '002345678910', N'Tiền Giang', '0902000010', N'Bac si/Y si', N'Tim mach',  'bs_lanbs');

-- ---- KY THUAT VIEN (5 nguoi dai dien cho 50) ----
INSERT INTO NHANVIEN VALUES ('KTV01', N'Nguyễn Thành Nam',  N'Nam', DATE '1990-03-15', '003456789001', N'Hồ Chí Minh','0903000001', N'Ky thuat vien', N'Xet nghiem', 'ktv_nam');
INSERT INTO NHANVIEN VALUES ('KTV02', N'Trần Thị Oanh',     N'Nu',  DATE '1993-07-21', '003456789002', N'Đà Nẵng',    '0903000002', N'Ky thuat vien', N'Chup hinh',  'ktv_oanh');
INSERT INTO NHANVIEN VALUES ('KTV03', N'Lê Văn Phong',      N'Nam', DATE '1988-11-09', '003456789003', N'Hà Nội',     '0903000003', N'Ky thuat vien', N'Sieu am',    'ktv_phong');
INSERT INTO NHANVIEN VALUES ('KTV04', N'Phạm Thị Quyên',    N'Nu',  DATE '1995-05-03', '003456789004', N'Hải Phòng',  '0903000004', N'Ky thuat vien', N'Xet nghiem', 'ktv_quyen');
INSERT INTO NHANVIEN VALUES ('KTV05', N'Võ Minh Sang',      N'Nam', DATE '1991-09-27', '003456789005', N'Cần Thơ',    '0903000005', N'Ky thuat vien', N'Noi soi',    'ktv_sang');


-- ============================================================
-- PHAN 4: INSERT DU LIEU MAU - BENHNHAN (20 benh nhan)
-- ============================================================

INSERT INTO BENHNHAN VALUES ('BN001', N'Nguyễn Thị Ánh',  N'Nu',  DATE '1975-04-12', '004567890001', N'12',    N'Lê Lợi',           N'Quận 1',    N'Hồ Chí Minh', N'Viêm dạ dày mãn tính',  N'Cha: tiểu đường',  N'Penicillin', 'bn_anh');
INSERT INTO BENHNHAN VALUES ('BN002', N'Trần Văn Bình',    N'Nam', DATE '1968-09-25', '004567890002', N'45',    N'Nguyễn Huệ',        N'Quận 1',    N'Hồ Chí Minh', N'Tăng huyết áp',         N'Mẹ: tim mạch',     NULL,          'bn_binh');
INSERT INTO BENHNHAN VALUES ('BN003', N'Lê Thị Châu',      N'Nu',  DATE '1990-02-17', '004567890003', N'78',    N'Trần Hưng Đạo',     N'Quận 5',    N'Hồ Chí Minh', NULL,                     N'Không rõ',         N'Aspirin',    'bn_chau');
INSERT INTO BENHNHAN VALUES ('BN004', N'Phạm Quốc Dũng',   N'Nam', DATE '1982-07-08', '004567890004', N'23',    N'Cách Mạng Tháng 8', N'Quận 3',    N'Hồ Chí Minh', N'Đau đầu mãn tính',      N'Bà nội: tai biến', NULL,          'bn_dung');
INSERT INTO BENHNHAN VALUES ('BN005', N'Võ Thị Diễm',      N'Nu',  DATE '1985-11-30', '004567890005', N'56',    N'Điện Biên Phủ',     N'Bình Thạnh', N'Hồ Chí Minh', N'Rối loạn tiền đình',   NULL,                N'Sulfa',      'bn_diem');
INSERT INTO BENHNHAN VALUES ('BN006', N'Hoàng Văn Em',     N'Nam', DATE '1960-03-22', '004567890006', N'89',    N'Phan Đình Phùng',   N'Phú Nhuận', N'Hồ Chí Minh', N'Suy tim độ 2',          N'Cha: mất vì nhồi máu cơ tim', NULL, 'bn_em');
INSERT INTO BENHNHAN VALUES ('BN007', N'Đặng Thị Fương',   N'Nu',  DATE '1995-06-14', '004567890007', N'10',    N'Hoàng Văn Thụ',     N'Tân Bình',  N'Hồ Chí Minh', NULL,                     NULL,                NULL,          'bn_fuong');
INSERT INTO BENHNHAN VALUES ('BN008', N'Bùi Minh Giang',   N'Nam', DATE '1978-12-05', '004567890008', N'34',    N'Cộng Hòa',          N'Tân Bình',  N'Hồ Chí Minh', N'Viêm đại tràng',        N'Mẹ: ung thư đại tràng', NULL,  'bn_giang');
INSERT INTO BENHNHAN VALUES ('BN009', N'Nguyễn Thị Hà',    N'Nu',  DATE '1972-08-19', '004567890009', N'67',    N'Nguyễn Thị Minh Khai', N'Quận 3', N'Hồ Chí Minh', N'Đái tháo đường type 2', N'Bố mẹ: tiểu đường', N'Metformin liều cao', 'bn_ha');
INSERT INTO BENHNHAN VALUES ('BN010', N'Trần Văn Huy',     N'Nam', DATE '1965-05-28', '004567890010', N'102',   N'Lý Thường Kiệt',    N'Quận 10',   N'Hồ Chí Minh', N'Xơ gan',                N'Bác: xơ gan',      NULL,          'bn_huy');
INSERT INTO BENHNHAN VALUES ('BN011', N'Lê Thị Hương',     N'Nu',  DATE '1988-01-11', '004567890011', N'15',    N'Bà Huyện Thanh Quan', N'Quận 3',  N'Hồ Chí Minh', NULL,                     NULL,                N'Ibuprofen',  'bn_huong');
INSERT INTO BENHNHAN VALUES ('BN012', N'Phạm Văn Ích',     N'Nam', DATE '1955-10-03', '004567890012', N'200',   N'Hùng Vương',        N'Quận 5',    N'Hồ Chí Minh', N'Bệnh Parkinson giai đoạn đầu', N'Không rõ', NULL,    'bn_ich');
INSERT INTO BENHNHAN VALUES ('BN013', N'Võ Thị Kim',       N'Nu',  DATE '1993-03-16', '004567890013', N'31',    N'Nguyễn Đình Chiểu', N'Quận 3',    N'Hồ Chí Minh', N'Đau nửa đầu Migraine',  N'Chị: Migraine',    N'Codeine',    'bn_kim');
INSERT INTO BENHNHAN VALUES ('BN014', N'Hoàng Văn Lâm',    N'Nam', DATE '1970-07-07', '004567890014', N'77',    N'Võ Thị Sáu',        N'Quận 3',    N'Hồ Chí Minh', N'Bệnh động mạch vành',   N'Cha: đột quỵ',     N'Clopidogrel', 'bn_lam');
INSERT INTO BENHNHAN VALUES ('BN015', N'Đặng Thị Linh',    N'Nu',  DATE '1997-09-24', '004567890015', N'5',     N'Trương Định',       N'Quận 3',    N'Hồ Chí Minh', NULL,                     NULL,                NULL,          'bn_linh');
INSERT INTO BENHNHAN VALUES ('BN016', N'Bùi Văn Mạnh',     N'Nam', DATE '1963-04-18', '004567890016', N'88',    N'Nguyễn Bỉnh Khiêm', N'Quận 1',    N'Hồ Chí Minh', N'Suy thận mãn tính',     N'Anh trai: suy thận', NULL,         'bn_manh');
INSERT INTO BENHNHAN VALUES ('BN017', N'Nguyễn Thị Nga',   N'Nu',  DATE '1980-12-01', '004567890017', N'3',     N'Pasteur',           N'Quận 3',    N'Hồ Chí Minh', N'Viêm khớp dạng thấp',  NULL,                N'NSAID',      'bn_nga');
INSERT INTO BENHNHAN VALUES ('BN018', N'Trần Quang Nhật',  N'Nam', DATE '1975-06-15', '004567890018', N'111',   N'Nam Kỳ Khởi Nghĩa', N'Quận 3',    N'Hồ Chí Minh', N'Nhồi máu cơ tim cũ',   N'Bố: tim mạch',     N'Heparin',    'bn_nhat');
INSERT INTO BENHNHAN VALUES ('BN019', N'Lê Thị Oanh',      N'Nu',  DATE '1992-02-28', '004567890019', N'44',    N'Hai Bà Trưng',      N'Quận 3',    N'Hồ Chí Minh', N'Trầm cảm - đang điều trị', NULL,             N'Sertraline liều cao', 'bn_oanh');
INSERT INTO BENHNHAN VALUES ('BN020', N'Phạm Thanh Phong',  N'Nam', DATE '1988-08-20', '004567890020', N'60',    N'Đinh Tiên Hoàng',   N'Bình Thạnh', N'Hồ Chí Minh', N'Viêm tụy mãn tính',  N'Bác: ung thư tụy', NULL,          'bn_phong');


-- ============================================================
-- PHAN 5: INSERT DU LIEU MAU - HSBA (Ho so benh an)
-- Moi BS co it nhat 2-3 HSBA de test VPD (TC#3)
-- MAKHOA: TH = Tieu hoa, TK = Than kinh, TM = Tim mach
-- ============================================================

-- BS001 - Tieu hoa
INSERT INTO HSBA VALUES ('HSBA001', 'BN001', DATE '2024-01-10', N'Viêm loét dạ dày tá tràng', N'Dùng thuốc ức chế bơm proton kết hợp kháng sinh diệt H.pylori', 'BS001', 'TH', N'Bệnh nhân đáp ứng tốt với điều trị, tái khám sau 4 tuần');
INSERT INTO HSBA VALUES ('HSBA002', 'BN008', DATE '2024-01-25', N'Viêm đại tràng mãn tính', N'Mesalazine 800mg x 3 lần/ngày, kiêng thức ăn kích thích', 'BS001', 'TH', N'Ổn định, tiếp tục theo dõi');
INSERT INTO HSBA VALUES ('HSBA003', 'BN010', DATE '2024-02-08', N'Xơ gan Child-Pugh B', N'Hạn chế muối, bổ sung albumin, theo dõi báng bụng', 'BS001', 'TH', N'Cần nhập viện điều trị tích cực');

-- BS002 - Tieu hoa
INSERT INTO HSBA VALUES ('HSBA004', 'BN020', DATE '2024-01-15', N'Viêm tụy cấp mức độ nhẹ', N'Nhịn ăn, truyền dịch, giảm đau', 'BS002', 'TH', N'Xuất viện sau 5 ngày, theo dõi ngoại trú');
INSERT INTO HSBA VALUES ('HSBA005', 'BN001', DATE '2024-03-01', N'Tái khám viêm dạ dày', N'Tiếp tục điều trị, bổ sung men vi sinh', 'BS002', 'TH', NULL);

-- BS003 - Tieu hoa
INSERT INTO HSBA VALUES ('HSBA006', 'BN009', DATE '2024-02-20', N'Tiêu chảy mãn tính nghi ngờ bệnh Crohn', N'Nội soi đại tràng, sinh thiết, điều trị triệu chứng', 'BS003', 'TH', N'Chờ kết quả sinh thiết để xác định chẩn đoán');

-- BS004 - Than kinh
INSERT INTO HSBA VALUES ('HSBA007', 'BN004', DATE '2024-01-20', N'Đau đầu Tension-type mãn tính', N'Amitriptyline 25mg/ngày, vật lý trị liệu', 'BS004', 'TK', N'Cải thiện 60% sau 3 tuần');
INSERT INTO HSBA VALUES ('HSBA008', 'BN012', DATE '2024-02-05', N'Bệnh Parkinson giai đoạn 2 theo Hoehn và Yahr', N'Levodopa/Carbidopa, vật lý trị liệu, tập luyện chức năng', 'BS004', 'TK', N'Bệnh tiến triển chậm, kiểm soát được');

-- BS005 - Than kinh
INSERT INTO HSBA VALUES ('HSBA009', 'BN005', DATE '2024-01-30', N'Rối loạn tiền đình ngoại biên', N'Betahistine 16mg x 2 lần/ngày, tránh thay đổi tư thế đột ngột', 'BS005', 'TK', N'Triệu chứng giảm rõ rệt sau 2 tuần');
INSERT INTO HSBA VALUES ('HSBA010', 'BN013', DATE '2024-03-10', N'Migraine không có aura, tái phát thường xuyên', N'Sumatriptan khi cấp tính, Topiramate dự phòng', 'BS005', 'TK', N'Đang theo dõi đáp ứng thuốc dự phòng');

-- BS006 - Than kinh
INSERT INTO HSBA VALUES ('HSBA011', 'BN019', DATE '2024-02-12', N'Rối loạn lo âu lan tỏa kèm trầm cảm', N'Sertraline 50mg/ngày, liệu pháp nhận thức hành vi', 'BS006', 'TK', N'Tiếp tục điều trị và theo dõi tâm lý');

-- BS007 - Tim mach
INSERT INTO HSBA VALUES ('HSBA012', 'BN002', DATE '2024-01-12', N'Tăng huyết áp độ 2', N'Amlodipine 5mg + Perindopril 4mg/ngày, thay đổi lối sống', 'BS007', 'TM', N'Huyết áp được kiểm soát ở mức 130/80 mmHg');
INSERT INTO HSBA VALUES ('HSBA013', 'BN006', DATE '2024-01-28', N'Suy tim EF giảm (EF=35%)', N'Bisoprolol + Sacubitril/Valsartan + Spironolacton + Furosemide', 'BS007', 'TM', N'Cần theo dõi chặt chẽ, tái khám 2 tuần/lần');
INSERT INTO HSBA VALUES ('HSBA014', 'BN018', DATE '2024-02-15', N'Nhồi máu cơ tim cũ, suy mạch vành ổn định', N'Aspirin + Atorvastatin + Bisoprolol, theo dõi định kỳ', 'BS007', 'TM', N'Ổn định, tiếp tục duy trì thuốc');

-- BS008 - Tim mach
INSERT INTO HSBA VALUES ('HSBA015', 'BN014', DATE '2024-01-18', N'Bệnh động mạch vành ổn định, hẹp 70% LAD', N'Clopidogrel + Atorvastatin + Isosorbide mononitrat, xem xét can thiệp', 'BS008', 'TM', N'Đã can thiệp đặt stent thành công');
INSERT INTO HSBA VALUES ('HSBA016', 'BN002', DATE '2024-03-05', N'Tái khám tăng huyết áp', N'Tăng liều Amlodipine lên 10mg', 'BS008', 'TM', N'Đang theo dõi đáp ứng liều mới');

-- BS009 - Tim mach
INSERT INTO HSBA VALUES ('HSBA017', 'BN016', DATE '2024-02-25', N'Suy thận mãn giai đoạn 3, kèm tăng huyết áp', N'Erythropoietin, điều chỉnh huyết áp, hạn chế protein', 'BS009', 'TM', N'Cần hội chẩn thận học');

-- BS010 - Tim mach
INSERT INTO HSBA VALUES ('HSBA018', 'BN007', DATE '2024-01-22', N'Kiểm tra sức khỏe định kỳ - điện tim bình thường', N'Không cần điều trị, tư vấn lối sống', 'BS010', 'TM', N'Sức khỏe tốt, tái khám sau 1 năm');
INSERT INTO HSBA VALUES ('HSBA019', 'BN011', DATE '2024-03-15', N'Đau ngực không điển hình, nghi ngờ co thắt mạch vành', N'Nicorandil, theo dõi điện tim Holter 24 giờ', 'BS010', 'TM', N'Chờ kết quả Holter để quyết định điều trị');


-- ============================================================
-- PHAN 6: INSERT DU LIEU MAU - HSBA_DV (Dich vu ho tro chan doan)
-- ============================================================

-- Dich vu cho HSBA001 (Viem loet da day)
INSERT INTO HSBA_DV VALUES ('HSBA001', N'Xét nghiệm máu tổng quát', DATE '2024-01-10', 'KTV01', N'Hb: 12.5 g/dL, WBC: 8.2x10^9/L, PLT: 210x10^9/L - Bình thường');
INSERT INTO HSBA_DV VALUES ('HSBA001', N'Test HP (Helicobacter pylori)', DATE '2024-01-10', 'KTV01', N'Dương tính (++) - Nhiễm H.pylori');
INSERT INTO HSBA_DV VALUES ('HSBA001', N'Nội soi dạ dày', DATE '2024-01-11', 'KTV03', N'Loét hang vị 1.5cm, xung huyết niêm mạc, không chảy máu. Sinh thiết đã lấy.');

-- Dich vu cho HSBA003 (Xo gan)
INSERT INTO HSBA_DV VALUES ('HSBA003', N'Siêu âm bụng tổng quát', DATE '2024-02-08', 'KTV03', N'Gan to, bờ không đều, echo không đồng nhất. Lách to 14cm. Dịch ổ bụng lượng ít.');
INSERT INTO HSBA_DV VALUES ('HSBA003', N'Xét nghiệm chức năng gan', DATE '2024-02-08', 'KTV01', N'AST: 85 U/L, ALT: 72 U/L, GGT: 120 U/L, Bilirubin TP: 3.2 mg/dL, Albumin: 2.8 g/dL');

-- Dich vu cho HSBA007 (Dau dau)
INSERT INTO HSBA_DV VALUES ('HSBA007', N'Chụp MRI sọ não', DATE '2024-01-20', 'KTV02', N'Không thấy tổn thương cấu trúc. Không có dấu hiệu nhồi máu hay xuất huyết. Bình thường.');
INSERT INTO HSBA_DV VALUES ('HSBA007', N'Điện não đồ (EEG)', DATE '2024-01-21', 'KTV03', N'Sóng não bình thường, không có hoạt động bất thường.');

-- Dich vu cho HSBA008 (Parkinson)
INSERT INTO HSBA_DV VALUES ('HSBA008', N'Chụp CT não', DATE '2024-02-05', 'KTV02', N'Teo não nhẹ vùng trán thái dương, phù hợp tuổi. Không có tổn thương bất thường khác.');

-- Dich vu cho HSBA012 (Tang huyet ap)
INSERT INTO HSBA_DV VALUES ('HSBA012', N'Điện tâm đồ (ECG)', DATE '2024-01-12', 'KTV04', N'Nhịp xoang đều 75 lần/phút, trục bình thường, không có thay đổi ST-T.');
INSERT INTO HSBA_DV VALUES ('HSBA012', N'Siêu âm tim (Echocardiography)', DATE '2024-01-12', 'KTV03', N'EF 60%, dày nhẹ vách liên thất, không có hở van. Dày thất trái nhẹ do tăng huyết áp.');
INSERT INTO HSBA_DV VALUES ('HSBA012', N'Xét nghiệm sinh hóa máu', DATE '2024-01-12', 'KTV01', N'Creatinine: 1.1 mg/dL, K+: 4.2 mEq/L, Na+: 140 mEq/L, Glucose: 5.8 mmol/L - Trong giới hạn bình thường');

-- Dich vu cho HSBA013 (Suy tim)
INSERT INTO HSBA_DV VALUES ('HSBA013', N'Siêu âm tim Doppler', DATE '2024-01-28', 'KTV03', N'EF 35%, giãn buồng thất trái, hở van hai lá 2/4. Rối loạn vận động vùng thành sau thất trái.');
INSERT INTO HSBA_DV VALUES ('HSBA013', N'Xét nghiệm BNP', DATE '2024-01-28', 'KTV01', N'BNP: 850 pg/mL (tăng cao, suy tim nặng)');

-- Dich vu cho HSBA015 (Benh dong mach vanh)
INSERT INTO HSBA_DV VALUES ('HSBA015', N'Chụp mạch vành (Coronary Angiography)', DATE '2024-01-19', 'KTV02', N'Hẹp 75% đoạn giữa LAD, không tổn thương nhánh khác. Phù hợp chỉ định đặt stent.');

-- Dich vu chua co ket qua (test VPD - KTV chi thay cua minh)
INSERT INTO HSBA_DV VALUES ('HSBA009', N'Thăm dò chức năng tiền đình', DATE '2024-01-30', 'KTV05', NULL);
INSERT INTO HSBA_DV VALUES ('HSBA010', N'Điện não đồ theo dõi Migraine', DATE '2024-03-10', 'KTV05', NULL);


-- ============================================================
-- PHAN 7: INSERT DU LIEU MAU - DONTHUOC
-- ============================================================

-- Don thuoc cho HSBA001
INSERT INTO DONTHUOC VALUES ('HSBA001', DATE '2024-01-10', N'Omeprazole 20mg', N'1 viên x 2 lần/ngày, uống trước ăn 30 phút, trong 4 tuần');
INSERT INTO DONTHUOC VALUES ('HSBA001', DATE '2024-01-10', N'Amoxicillin 1000mg', N'1 viên x 2 lần/ngày x 14 ngày');
INSERT INTO DONTHUOC VALUES ('HSBA001', DATE '2024-01-10', N'Clarithromycin 500mg', N'1 viên x 2 lần/ngày x 14 ngày');

-- Don thuoc cho HSBA002
INSERT INTO DONTHUOC VALUES ('HSBA002', DATE '2024-01-25', N'Mesalazine 800mg', N'1 viên x 3 lần/ngày sau ăn, duy trì 6 tháng');
INSERT INTO DONTHUOC VALUES ('HSBA002', DATE '2024-01-25', N'Probiotic Enterogermina', N'1 ống x 2 lần/ngày x 4 tuần');

-- Don thuoc cho HSBA004
INSERT INTO DONTHUOC VALUES ('HSBA004', DATE '2024-01-15', N'Spasmaverine 40mg', N'1 viên x 3 lần/ngày khi đau');
INSERT INTO DONTHUOC VALUES ('HSBA004', DATE '2024-01-15', N'Metoclopramide 10mg', N'1 viên x 3 lần/ngày trước ăn 15 phút');

-- Don thuoc cho HSBA007
INSERT INTO DONTHUOC VALUES ('HSBA007', DATE '2024-01-20', N'Amitriptyline 25mg', N'1 viên/ngày uống trước ngủ, khởi đầu 25mg tăng dần');
INSERT INTO DONTHUOC VALUES ('HSBA007', DATE '2024-01-20', N'Paracetamol 500mg', N'1-2 viên khi đau, tối đa 4g/ngày');

-- Don thuoc cho HSBA008
INSERT INTO DONTHUOC VALUES ('HSBA008', DATE '2024-02-05', N'Levodopa/Carbidopa 250/25mg', N'0.5 viên x 3 lần/ngày, tăng dần theo đáp ứng');
INSERT INTO DONTHUOC VALUES ('HSBA008', DATE '2024-02-05', N'Trihexyphenidyl 2mg', N'1 viên x 2 lần/ngày, giảm run');

-- Don thuoc cho HSBA009
INSERT INTO DONTHUOC VALUES ('HSBA009', DATE '2024-01-30', N'Betahistine 16mg', N'1 viên x 2 lần/ngày sau ăn');
INSERT INTO DONTHUOC VALUES ('HSBA009', DATE '2024-01-30', N'Dimenhydrinate 50mg', N'1 viên khi chóng mặt cấp, tối đa 3 viên/ngày');

-- Don thuoc cho HSBA012
INSERT INTO DONTHUOC VALUES ('HSBA012', DATE '2024-01-12', N'Amlodipine 5mg', N'1 viên/ngày, uống buổi sáng');
INSERT INTO DONTHUOC VALUES ('HSBA012', DATE '2024-01-12', N'Perindopril 4mg', N'1 viên/ngày, uống buổi sáng');

-- Don thuoc cho HSBA013 (cap nhat sau)
INSERT INTO DONTHUOC VALUES ('HSBA013', DATE '2024-01-28', N'Bisoprolol 2.5mg', N'1 viên/ngày, tăng dần đến 10mg');
INSERT INTO DONTHUOC VALUES ('HSBA013', DATE '2024-01-28', N'Sacubitril/Valsartan 24/26mg', N'1 viên x 2 lần/ngày, tăng đến mức dung nạp');
INSERT INTO DONTHUOC VALUES ('HSBA013', DATE '2024-01-28', N'Spironolacton 25mg', N'1 viên/ngày, theo dõi K+ máu');
INSERT INTO DONTHUOC VALUES ('HSBA013', DATE '2024-01-28', N'Furosemide 40mg', N'1 viên/ngày buổi sáng, điều chỉnh theo cân nặng');

-- Don thuoc cho HSBA015
INSERT INTO DONTHUOC VALUES ('HSBA015', DATE '2024-01-18', N'Clopidogrel 75mg', N'1 viên/ngày, duy trì ít nhất 12 tháng');
INSERT INTO DONTHUOC VALUES ('HSBA015', DATE '2024-01-18', N'Atorvastatin 40mg', N'1 viên/ngày buổi tối');
INSERT INTO DONTHUOC VALUES ('HSBA015', DATE '2024-01-18', N'Isosorbide mononitrate 20mg', N'1 viên x 2 lần/ngày');

-- Don thuoc cho HSBA019
INSERT INTO DONTHUOC VALUES ('HSBA019', DATE '2024-03-15', N'Nicorandil 10mg', N'1 viên x 2 lần/ngày');


-- ============================================================
-- PHAN 8: INSERT DU LIEU MAU - THONGBAO (Yeu cau 2 - OLS)
-- t1..t7 theo de bai
-- ============================================================

INSERT INTO THONGBAO VALUES ('TB001', N'[T1] Họp toàn thể nhân viên: Triển khai quy trình phòng chống dịch mới theo Thông tư 38/2024. Bắt buộc tham dự.', TIMESTAMP '2024-04-01 08:00:00', N'Hội trường lớn - Tất cả cơ sở');
INSERT INTO THONGBAO VALUES ('TB002', N'[T2] Cuộc họp Ban Giám đốc khẩn: Phân tích báo cáo tài chính Q1/2024 và định hướng chiến lược phát triển bệnh viện.', TIMESTAMP '2024-04-02 09:00:00', N'Phòng họp BGĐ - Tầng 10');
INSERT INTO THONGBAO VALUES ('TB003', N'[T3] Họp Lãnh đạo các khoa: Cập nhật phác đồ điều trị mới nhất theo hướng dẫn Bộ Y tế 2024.', TIMESTAMP '2024-04-03 14:00:00', N'Phòng họp trực tuyến');
INSERT INTO THONGBAO VALUES ('TB004', N'[T4] Họp Lãnh đạo Khoa Tiêu hóa: Kết quả kiểm định thiết bị nội soi và kế hoạch nâng cấp trang thiết bị.', TIMESTAMP '2024-04-04 10:00:00', N'Phòng trưởng khoa Tiêu hóa');
INSERT INTO THONGBAO VALUES ('TB005', N'[T5] Thông báo nhân viên Khoa Tiêu hóa tại HCM: Lịch trực tháng 4/2024 và phân công ca mổ nội soi.', TIMESTAMP '2024-04-05 07:30:00', N'Khoa Tiêu hóa - CS Hồ Chí Minh');
INSERT INTO THONGBAO VALUES ('TB006', N'[T6] Thông báo nhân viên Khoa Tiêu hóa tại Hà Nội: Cập nhật danh sách bệnh nhân chờ nội soi và quy trình ưu tiên.', TIMESTAMP '2024-04-05 07:30:00', N'Khoa Tiêu hóa - CS Hà Nội');
INSERT INTO THONGBAO VALUES ('TB007', N'[T7] Họp khẩn Lãnh đạo Khoa Tiêu hóa và Khoa Thần kinh tại Hải Phòng: Sự cố thiết bị MRI 3T, phương án xử lý tạm thời.', TIMESTAMP '2024-04-06 15:00:00', N'Phòng họp - CS Hải Phòng');

COMMIT;


-- ============================================================
-- PHAN 9: KIEM TRA DU LIEU SAU KHI INSERT
-- ============================================================

PROMPT ======== KIEM TRA SO LUONG DONG ========
SELECT 'NHANVIEN'  AS BANG, COUNT(*) AS SO_DONG FROM NHANVIEN  UNION ALL
SELECT 'BENHNHAN'  AS BANG, COUNT(*) AS SO_DONG FROM BENHNHAN  UNION ALL
SELECT 'HSBA'      AS BANG, COUNT(*) AS SO_DONG FROM HSBA      UNION ALL
SELECT 'HSBA_DV'   AS BANG, COUNT(*) AS SO_DONG FROM HSBA_DV   UNION ALL
SELECT 'DONTHUOC'  AS BANG, COUNT(*) AS SO_DONG FROM DONTHUOC  UNION ALL
SELECT 'THONGBAO'  AS BANG, COUNT(*) AS SO_DONG FROM THONGBAO;

PROMPT ======== PHAN BO NHANVIEN THEO VAI TRO ========
SELECT VAITRO, COUNT(*) AS SO_LUONG
FROM NHANVIEN
GROUP BY VAITRO
ORDER BY VAITRO;

PROMPT ======== HSBA THEO BAC SI ========
SELECT NV.MANV, NV.HOTEN, NV.CHUYENKHOA, COUNT(H.MAHSBA) AS SO_HSBA
FROM NHANVIEN NV LEFT JOIN HSBA H ON NV.MANV = H.MABS
WHERE NV.VAITRO = N'Bac si/Y si'
GROUP BY NV.MANV, NV.HOTEN, NV.CHUYENKHOA
ORDER BY NV.MANV;

-- Xem toan bo du lieu cac bang
SELECT * FROM NHANVIEN;
SELECT * FROM BENHNHAN;
SELECT * FROM HSBA;
SELECT * FROM HSBA_DV;
SELECT * FROM DONTHUOC;
SELECT * FROM THONGBAO;

-- ============================================================
-- Yêu cầu 1
-- Câu 1
-- ------------TC1--------------
-- ============================================================

-- Chuẩn hóa Username về chữ hoa
UPDATE NHANVIEN
SET ORACLE_USERNAME = UPPER(TRIM(ORACLE_USERNAME))
WHERE ORACLE_USERNAME IS NOT NULL;

UPDATE BENHNHAN
SET ORACLE_USERNAME = UPPER(TRIM(ORACLE_USERNAME))
WHERE ORACLE_USERNAME IS NOT NULL;

-- KIEM TRA TRUNG USERNAME GIUA 2 BANG
-- Yeu cau: 1 account Oracle <-> 1 nguoi dung
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_cnt
    FROM (
        SELECT ORACLE_USERNAME
        FROM NHANVIEN
        WHERE ORACLE_USERNAME IS NOT NULL
        INTERSECT
        SELECT ORACLE_USERNAME
        FROM BENHNHAN
        WHERE ORACLE_USERNAME IS NOT NULL
    );

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(
            -20002,
            'Phat hien ORACLE_USERNAME bi trung giua NHANVIEN va BENHNHAN.'
        );
    END IF;
END;
/
SHOW ERRORS;

-- TAO VIEW HOP NHAT NGUOI DUNG TC#1
CREATE OR REPLACE VIEW VW_TC1_NGUOIDUNG AS
SELECT
    CAST(ORACLE_USERNAME AS VARCHAR2(30))     AS ORACLE_USERNAME,
    CAST(MANV AS VARCHAR2(10))                AS MA_NGUOIDUNG,
    CAST(HOTEN AS NVARCHAR2(100))             AS TEN_NGUOIDUNG,
    CAST(N'NHANVIEN' AS NVARCHAR2(20))        AS LOAI_NGUOIDUNG,
    CAST(VAITRO AS NVARCHAR2(50))             AS VAITRO,
    CAST(CHUYENKHOA AS NVARCHAR2(100))        AS CHUYENKHOA
FROM NHANVIEN
WHERE ORACLE_USERNAME IS NOT NULL

UNION ALL

SELECT
    CAST(ORACLE_USERNAME AS VARCHAR2(30))     AS ORACLE_USERNAME,
    CAST(MABN AS VARCHAR2(10))                AS MA_NGUOIDUNG,
    CAST(TENBN AS NVARCHAR2(100))             AS TEN_NGUOIDUNG,
    CAST(N'BENHNHAN' AS NVARCHAR2(20))        AS LOAI_NGUOIDUNG,
    CAST(N'Benh nhan' AS NVARCHAR2(50))       AS VAITRO,
    CAST(NULL AS NVARCHAR2(100))              AS CHUYENKHOA
FROM BENHNHAN
WHERE ORACLE_USERNAME IS NOT NULL;

-- TAO VIEW CHO PHEP MOI USER NHIN THAY CHINH MINH
CREATE OR REPLACE VIEW VW_TC1_TOI_LA_AI AS
SELECT *
FROM VW_TC1_NGUOIDUNG
WHERE ORACLE_USERNAME = SYS_CONTEXT('USERENV', 'SESSION_USER');

-- TAO ROLE DUNG CHO TOAN BO USER TC#1
-- Can quyen CREATE ROLE
DECLARE
    e_role_exists EXCEPTION;
    PRAGMA EXCEPTION_INIT(e_role_exists, -1921); -- ORA-01921
BEGIN
    EXECUTE IMMEDIATE 'CREATE ROLE RL_TC1_USER';
EXCEPTION
    WHEN e_role_exists THEN
        NULL;
END;
/

-- TAO USER ORACLE CHO TOAN BO NHANVIEN + BENHNHAN
-- Can quyen CREATE USER
-- Neu dang o PDB thi tao local user binh thuong
DECLARE
    e_user_exists EXCEPTION;
    PRAGMA EXCEPTION_INIT(e_user_exists, -1920); -- ORA-01920

    v_sql VARCHAR2(4000);
BEGIN
    FOR r IN (
        SELECT ORACLE_USERNAME AS UNAME
        FROM NHANVIEN
        WHERE ORACLE_USERNAME IS NOT NULL

        UNION

        SELECT ORACLE_USERNAME AS UNAME
        FROM BENHNHAN
        WHERE ORACLE_USERNAME IS NOT NULL
    )
    LOOP
        BEGIN
            v_sql :=
                'CREATE USER ' || r.UNAME ||
                ' IDENTIFIED BY Welcome#123 ' ||
                ' DEFAULT TABLESPACE USERS ' ||
                ' TEMPORARY TABLESPACE TEMP ' ||
                ' QUOTA 0M ON USERS';

            EXECUTE IMMEDIATE v_sql;
        EXCEPTION
            WHEN e_user_exists THEN
                NULL;
        END;

        BEGIN
            EXECUTE IMMEDIATE 'ALTER USER ' || r.UNAME || ' ACCOUNT UNLOCK';
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;

        BEGIN
            EXECUTE IMMEDIATE 'GRANT RL_TC1_USER TO ' || r.UNAME;
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;
    END LOOP;
END;
/

GRANT SELECT ON VW_TC1_TOI_LA_AI TO RL_TC1_USER;

-- BAO CAO DOI CHIEU USERNAME <-> NGUOI DUNG
-- ============================================================
PROMPT ===== DANH SACH ANH XA TAI KHOAN ORACLE =====
COLUMN ORACLE_USERNAME FORMAT A20
COLUMN MA_NGUOIDUNG    FORMAT A12
COLUMN TEN_NGUOIDUNG   FORMAT A30
COLUMN LOAI_NGUOIDUNG  FORMAT A12
COLUMN VAITRO          FORMAT A20
COLUMN CHUYENKHOA      FORMAT A15

SELECT *
FROM VW_TC1_NGUOIDUNG
ORDER BY LOAI_NGUOIDUNG, MA_NGUOIDUNG;

-- 11) KIEM TRA CAC USER DA DUOC TAO
-- Neu khong co quyen xem DBA_USERS, co the bo qua doan nay
-- ============================================================
PROMPT ===== CAC USER ORACLE THUOC TC#1 =====
BEGIN
    NULL;
END;
/

--Neu co quyen DBA_USERS :
SELECT USERNAME
FROM DBA_USERS
WHERE USERNAME IN (
     SELECT ORACLE_USERNAME FROM NHANVIEN WHERE ORACLE_USERNAME IS NOT NULL
     UNION
     SELECT ORACLE_USERNAME FROM BENHNHAN WHERE ORACLE_USERNAME IS NOT NULL
)
ORDER BY USERNAME;


-- ============================================================
-- Yêu cầu 1
-- Câu 2:  CHINH SACH BAO MAT BANG CO CHE RBAC + VIEW
-- Doi tuong: Ky thuat vien (TC#4) va Benh nhan (TC#5)
-- ============================================================

-- XOA ROLE CU (NEU TON TAI) 
BEGIN
    EXECUTE IMMEDIATE 'DROP ROLE RL_KYTHUATVIEN';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP ROLE RL_BENHNHAN';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- TAO ROLE VA CAP QUYEN DANG NHAP CO BAN
CREATE ROLE RL_KYTHUATVIEN;
CREATE ROLE RL_BENHNHAN;

GRANT CREATE SESSION TO RL_KYTHUATVIEN;
GRANT CREATE SESSION TO RL_BENHNHAN;


-- CHINH SACH CHO BENH NHAN (TC#5)

-- Tao View de Benh nhan chi thay dong du lieu cua chinh ho
CREATE OR REPLACE VIEW VW_BN_THONGTIN_CANHAN AS
SELECT * FROM BENHNHAN
WHERE ORACLE_USERNAME = SYS_CONTEXT('USERENV', 'SESSION_USER');

-- Cap quyen SELECT tren View cho Role Benh nhan
GRANT SELECT ON VW_BN_THONGTIN_CANHAN TO RL_BENHNHAN;

-- Cap quyen UPDATE (tru cac truong bi cam theo TC#5) cho Role Benh nhan
GRANT UPDATE (SONHA, TENDUONG, QUANHUYEN, TINHTHANH, TIENSUBENH, TIENSUBENHGD, DIUNGthuoc) 
ON VW_BN_THONGTIN_CANHAN TO RL_BENHNHAN;


-- CHINH SACH CHO KY THUAT VIEN (TC#4)

-- Cho phep KTV xem va sua thong tin ca nhan cua ho tren bang NHANVIEN (TC#5)
CREATE OR REPLACE VIEW VW_NV_THONGTIN_CANHAN AS
SELECT * FROM NHANVIEN
WHERE ORACLE_USERNAME = SYS_CONTEXT('USERENV', 'SESSION_USER');

GRANT SELECT ON VW_NV_THONGTIN_CANHAN TO RL_KYTHUATVIEN;
GRANT UPDATE (QUEQUAN, SODT) ON VW_NV_THONGTIN_CANHAN TO RL_KYTHUATVIEN;

-- Cho phep KTV xem cac dich vu do chinh ho thuc hien (TC#4)
CREATE OR REPLACE VIEW VW_KTV_HSBA_DV AS
SELECT * FROM HSBA_DV
WHERE MAKTV = (
    SELECT MANV FROM NHANVIEN 
    WHERE ORACLE_USERNAME = SYS_CONTEXT('USERENV', 'SESSION_USER')
);

-- Cap quyen SELECT va UPDATE truong KETQUA tren View cho Role KTV
GRANT SELECT ON VW_KTV_HSBA_DV TO RL_KYTHUATVIEN;
GRANT UPDATE (KETQUA) ON VW_KTV_HSBA_DV TO RL_KYTHUATVIEN;


-- GHI VET (AUDIT) thao tac cap nhat truong KETQUA (TC#4)
BEGIN
  DBMS_FGA.ADD_POLICY(
    object_schema   => SYS_CONTEXT('USERENV', 'CURRENT_USER'),
    object_name     => 'HSBA_DV',
    policy_name     => 'AUDIT_KTV_UPDATE_KETQUA',
    audit_column    => 'KETQUA',
    statement_types => 'UPDATE',
    audit_condition => '1=1' -- Bat moi truong hop UPDATE
  );
END;
/

-- TU DONG GAN ROLE CHO NGUOI DUNG HIEN TAI
BEGIN
    -- Gan Role RL_KYTHUATVIEN cho cac User co vai tro 'Ky thuat vien'
    FOR r IN (SELECT ORACLE_USERNAME FROM NHANVIEN WHERE VAITRO = N'Ky thuat vien' AND ORACLE_USERNAME IS NOT NULL) LOOP
        EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO ' || r.ORACLE_USERNAME;
    END LOOP;

    -- Gan Role RL_BENHNHAN cho tat ca Benh nhan
    FOR r IN (SELECT ORACLE_USERNAME FROM BENHNHAN WHERE ORACLE_USERNAME IS NOT NULL) LOOP
        EXECUTE IMMEDIATE 'GRANT RL_BENHNHAN TO ' || r.ORACLE_USERNAME;
    END LOOP;
END;
/

-- ============================================================
-- Yêu cầu 1 - Câu 3 (Phân hệ 2)
-- VPD cho Điều phối viên (TC#2) và Bác sĩ/Y sĩ (TC#3)
-- Luu y: chay bang schema owner (khong chay as SYS)
-- ============================================================

BEGIN
    IF USER IN ('SYS', 'SYSTEM') THEN
        RAISE_APPLICATION_ERROR(-20010, 'Vui long chay script bang schema owner, khong chay as SYS/SYSTEM.');
    END IF;
END;
/

-- -------------------------
-- 1) ROLE VA PHAN QUYEN CO BAN
-- -------------------------
BEGIN
    EXECUTE IMMEDIATE 'DROP ROLE RL_DIEUPHOIVIEN';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP ROLE RL_BACSI';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
CREATE ROLE RL_DIEUPHOIVIEN;
CREATE ROLE RL_BACSI;

GRANT CREATE SESSION TO RL_DIEUPHOIVIEN;
GRANT CREATE SESSION TO RL_BACSI;

-- Grant bang/cot (VPD chi loc dong, khong thay the GRANT)
GRANT SELECT, INSERT, UPDATE ON BENHNHAN TO RL_DIEUPHOIVIEN;
GRANT INSERT, UPDATE ON HSBA TO RL_DIEUPHOIVIEN;
GRANT UPDATE ON HSBA_DV TO RL_DIEUPHOIVIEN;

GRANT SELECT, UPDATE ON HSBA TO RL_BACSI;
GRANT SELECT, UPDATE ON BENHNHAN TO RL_BACSI;
GRANT INSERT, DELETE ON HSBA_DV TO RL_BACSI;
GRANT INSERT, UPDATE, DELETE ON DONTHUOC TO RL_BACSI;

GRANT UPDATE (MAKHOA, MABS) ON HSBA TO RL_DIEUPHOIVIEN;
GRANT UPDATE (MAKTV) ON HSBA_DV TO RL_DIEUPHOIVIEN;

GRANT UPDATE (CHANDOAN, DIEUTRI, KETLUAN) ON HSBA TO RL_BACSI;
GRANT UPDATE (TIENSUBENH, TIENSUBENHGD, DIUNGthuoc) ON BENHNHAN TO RL_BACSI;

-- Gan role cho user theo VAITRO
BEGIN
    FOR r IN (
        SELECT ORACLE_USERNAME FROM NHANVIEN
        WHERE VAITRO = N'Dieu phoi vien' AND ORACLE_USERNAME IS NOT NULL
    ) LOOP
        EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOIVIEN TO ' || r.ORACLE_USERNAME;
    END LOOP;

    FOR r IN (
        SELECT ORACLE_USERNAME FROM NHANVIEN
        WHERE VAITRO = N'Bac si/Y si' AND ORACLE_USERNAME IS NOT NULL
    ) LOOP
        EXECUTE IMMEDIATE 'GRANT RL_BACSI TO ' || r.ORACLE_USERNAME;
    END LOOP;
END;
/

-- -------------------------
-- 2) APPLICATION CONTEXT + LOGON TRIGGER
-- -------------------------
CREATE OR REPLACE PACKAGE PKG_VPD_HOSPITAL AS
    PROCEDURE set_ctx;
    FUNCTION vpd_predicate(p_schema VARCHAR2, p_obj VARCHAR2)
        RETURN VARCHAR2;
END PKG_VPD_HOSPITAL;
/

CREATE OR REPLACE PACKAGE BODY PKG_VPD_HOSPITAL AS
    PROCEDURE set_ctx IS
        v_user   VARCHAR2(30);
        v_manv   VARCHAR2(10);
        v_vaitro NVARCHAR2(50);
    BEGIN
        v_user := UPPER(SYS_CONTEXT('USERENV', 'SESSION_USER'));

        BEGIN
            SELECT MANV, VAITRO
            INTO v_manv, v_vaitro
            FROM NHANVIEN
            WHERE ORACLE_USERNAME = v_user;

            DBMS_SESSION.SET_CONTEXT('HOSPITAL_CTX', 'MANV', v_manv);
            DBMS_SESSION.SET_CONTEXT('HOSPITAL_CTX', 'VAITRO', v_vaitro);
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                DBMS_SESSION.SET_CONTEXT('HOSPITAL_CTX', 'MANV', NULL);
                DBMS_SESSION.SET_CONTEXT('HOSPITAL_CTX', 'VAITRO', NULL);
        END;
    END set_ctx;

    FUNCTION vpd_predicate(p_schema VARCHAR2, p_obj VARCHAR2)
        RETURN VARCHAR2
    IS
        v_role NVARCHAR2(50) := UPPER(TRIM(SYS_CONTEXT('HOSPITAL_CTX', 'VAITRO')));
        v_manv VARCHAR2(10)  := UPPER(TRIM(SYS_CONTEXT('HOSPITAL_CTX', 'MANV')));
        v_user VARCHAR2(30)  := UPPER(SYS_CONTEXT('USERENV', 'SESSION_USER'));
    BEGIN
        IF SYS_CONTEXT('USERENV', 'ISDBA') = 'TRUE'
           OR v_user IN ('SYS', 'SYSTEM') THEN
            RETURN '1=1';
        END IF;

        IF v_role IS NULL OR v_manv IS NULL THEN
            RETURN '1=0';
        END IF;

        CASE UPPER(p_obj)
            WHEN 'BENHNHAN' THEN
                IF v_role = 'DIEU PHOI VIEN' THEN
                    RETURN '1=1';
                ELSIF v_role = 'BAC SI/Y SI' THEN
                    RETURN 'EXISTS (SELECT 1 FROM HSBA H ' ||
                           'WHERE H.MABN = BENHNHAN.MABN ' ||
                           'AND H.MABS = ''' || v_manv || ''')';
                ELSE
                    RETURN '1=0';
                END IF;

            WHEN 'HSBA' THEN
                IF v_role = 'DIEU PHOI VIEN' THEN
                    RETURN '1=1';
                ELSIF v_role = 'BAC SI/Y SI' THEN
                    RETURN 'MABS = ''' || v_manv || '''';
                END IF;
                RETURN '1=0';

            WHEN 'HSBA_DV' THEN
                IF v_role = 'DIEU PHOI VIEN' THEN
                    RETURN '1=1';
                ELSIF v_role = 'BAC SI/Y SI' THEN
                    RETURN 'MAHSBA IN (SELECT MAHSBA FROM HSBA ' ||
                           'WHERE MABS = ''' || v_manv || ''')';
                ELSE
                    RETURN '1=0';
                END IF;

            WHEN 'DONTHUOC' THEN
                IF v_role = 'BAC SI/Y SI' THEN
                    RETURN 'MAHSBA IN (SELECT MAHSBA FROM HSBA ' ||
                           'WHERE MABS = ''' || v_manv || ''')';
                ELSE
                    RETURN '1=0';
                END IF;

            ELSE
                RETURN '1=0';
        END CASE;
    EXCEPTION
        WHEN OTHERS THEN
            RETURN '1=0';
    END vpd_predicate;
END PKG_VPD_HOSPITAL;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP CONTEXT HOSPITAL_CTX';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

CREATE CONTEXT HOSPITAL_CTX USING PKG_VPD_HOSPITAL;
/
CREATE OR REPLACE TRIGGER TRG_SET_CTX_HOSPITAL
AFTER LOGON ON DATABASE
BEGIN
    PKG_VPD_HOSPITAL.set_ctx;
END;
/

-- -------------------------
-- 3) GAN VPD POLICY
-- -------------------------
BEGIN
    DBMS_RLS.DROP_POLICY(USER, 'BENHNHAN', 'VPD_BENHNHAN_TC2_TC3');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_RLS.DROP_POLICY(USER, 'HSBA', 'VPD_HSBA_TC2_TC3');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_RLS.DROP_POLICY(USER, 'HSBA_DV', 'VPD_HSBA_DV_TC2_TC3');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_RLS.DROP_POLICY(USER, 'DONTHUOC', 'VPD_DONTHUOC_TC3');
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema     => USER,
        object_name       => 'BENHNHAN',
        policy_name       => 'VPD_BENHNHAN_TC2_TC3',
        function_schema   => USER,
        policy_function   => 'PKG_VPD_HOSPITAL.vpd_predicate',
        statement_types   => 'SELECT,INSERT,UPDATE',
        update_check      => TRUE,
        sec_relevant_cols => 'TIENSUBENH,TIENSUBENHGD,DIUNGthuoc'
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => USER,
        object_name       => 'HSBA',
        policy_name       => 'VPD_HSBA_TC2_TC3',
        function_schema   => USER,
        policy_function   => 'PKG_VPD_HOSPITAL.vpd_predicate',
        statement_types   => 'SELECT,INSERT,UPDATE',
        update_check      => TRUE,
        sec_relevant_cols => 'CHANDOAN,DIEUTRI,KETLUAN,MAKHOA,MABS'
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => USER,
        object_name       => 'HSBA_DV',
        policy_name       => 'VPD_HSBA_DV_TC2_TC3',
        function_schema   => USER,
        policy_function   => 'PKG_VPD_HOSPITAL.vpd_predicate',
        statement_types   => 'SELECT,INSERT,UPDATE,DELETE',
        update_check      => TRUE,
        sec_relevant_cols => 'MAKTV'
    );

    DBMS_RLS.ADD_POLICY(
        object_schema    => USER,
        object_name      => 'DONTHUOC',
        policy_name      => 'VPD_DONTHUOC_TC3',
        function_schema  => USER,
        policy_function  => 'PKG_VPD_HOSPITAL.vpd_predicate',
        statement_types  => 'SELECT,INSERT,UPDATE,DELETE',
        update_check     => TRUE
    );
END;
/



-- ============================================================
-- BLOCK B: BVDBA
-- (Cac section duoi day thuc thi voi session BVDBA)
-- Gia su schema BENHNHAN, NHANVIEN, HSBA, HSBA_DV, DONTHUOC,
-- THONGBAO, cac Role/View/Policy cua PH2 da ton tai.
-- ============================================================


-- ============================================================
-- B1: TYPES HO TRO PIPELINED FUNCTION
-- Tich hop tu PH1 Section 4
-- ============================================================

CREATE OR REPLACE TYPE T_USER_ROW AS OBJECT (
    USERNAME           VARCHAR2(128),
    ACCOUNT_STATUS     VARCHAR2(32),
    CREATED            DATE,
    DEFAULT_TABLESPACE VARCHAR2(30),
    PROFILE            VARCHAR2(128)
);
/
CREATE OR REPLACE TYPE T_USER_TABLE AS TABLE OF T_USER_ROW;
/

CREATE OR REPLACE TYPE T_ROLE_ROW AS OBJECT (
    ROLE              VARCHAR2(128),
    PASSWORD_REQUIRED VARCHAR2(8)
);
/
CREATE OR REPLACE TYPE T_ROLE_TABLE AS TABLE OF T_ROLE_ROW;
/

CREATE OR REPLACE TYPE T_OBJECT_ROW AS OBJECT (
    OBJECT_NAME VARCHAR2(128),
    OBJECT_TYPE VARCHAR2(23),
    STATUS      VARCHAR2(7)
);
/
CREATE OR REPLACE TYPE T_OBJECT_TABLE AS TABLE OF T_OBJECT_ROW;
/

CREATE OR REPLACE TYPE T_COLUMN_ROW AS OBJECT (
    COLUMN_NAME VARCHAR2(128),
    DATA_TYPE   VARCHAR2(128),
    NULLABLE    VARCHAR2(1)
);
/
CREATE OR REPLACE TYPE T_COLUMN_TABLE AS TABLE OF T_COLUMN_ROW;
/

CREATE OR REPLACE TYPE T_OBJPRIV_ROW AS OBJECT (
    GRANTEE     VARCHAR2(128),
    OWNER       VARCHAR2(128),
    OBJECT_NAME VARCHAR2(128),
    OBJECT_TYPE VARCHAR2(23),
    PRIVILEGE   VARCHAR2(40),
    GRANTABLE   VARCHAR2(3),
    COLUMN_NAME VARCHAR2(128)
);
/
CREATE OR REPLACE TYPE T_OBJPRIV_TABLE AS TABLE OF T_OBJPRIV_ROW;
/

CREATE OR REPLACE TYPE T_SYSPRIV_ROW AS OBJECT (
    GRANTEE   VARCHAR2(128),
    PRIVILEGE VARCHAR2(40),
    ADMIN_OPT VARCHAR2(3)
);
/
CREATE OR REPLACE TYPE T_SYSPRIV_TABLE AS TABLE OF T_SYSPRIV_ROW;
/

CREATE OR REPLACE TYPE T_ROLEPRIV_ROW AS OBJECT (
    GRANTEE      VARCHAR2(128),
    GRANTED_ROLE VARCHAR2(128),
    ADMIN_OPTION VARCHAR2(3),
    DEFAULT_ROLE VARCHAR2(3)
);
/
CREATE OR REPLACE TYPE T_ROLEPRIV_TABLE AS TABLE OF T_ROLEPRIV_ROW;
/


-- ============================================================
-- B2: QUAN LY USER / ROLE
-- Tich hop nguyen tu PH1 Section 4
-- ============================================================

-- Tao user moi
CREATE OR REPLACE PROCEDURE SP_CREATE_USER (
    p_username   IN VARCHAR2,
    p_password   IN VARCHAR2,
    p_tablespace IN VARCHAR2 DEFAULT 'BENHVIEN_TBS'
) AS
BEGIN
    EXECUTE IMMEDIATE
        'CREATE USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) ||
        ' IDENTIFIED BY "' || p_password || '"' ||
        ' DEFAULT TABLESPACE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_tablespace) ||
        ' QUOTA 0 ON '        || DBMS_ASSERT.SIMPLE_SQL_NAME(p_tablespace);
    EXECUTE IMMEDIATE
        'GRANT CREATE SESSION TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username);
END SP_CREATE_USER;
/

-- Xoa user
CREATE OR REPLACE PROCEDURE SP_DROP_USER (
    p_username IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'DROP USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) || ' CASCADE';
END SP_DROP_USER;
/

-- Doi mat khau
CREATE OR REPLACE PROCEDURE SP_ALTER_USER_PASSWORD (
    p_username    IN VARCHAR2,
    p_newpassword IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'ALTER USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) ||
        ' IDENTIFIED BY "' || p_newpassword || '"';
END SP_ALTER_USER_PASSWORD;
/

-- Khoa / Mo khoa tai khoan (p_action: 'LOCK' hoac 'UNLOCK')
CREATE OR REPLACE PROCEDURE SP_LOCK_UNLOCK_USER (
    p_username IN VARCHAR2,
    p_action   IN VARCHAR2
) AS
BEGIN
    IF UPPER(p_action) NOT IN ('LOCK','UNLOCK') THEN
        RAISE_APPLICATION_ERROR(-20001, 'p_action phai la LOCK hoac UNLOCK');
    END IF;
    EXECUTE IMMEDIATE
        'ALTER USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) ||
        ' ACCOUNT '   || UPPER(p_action);
END SP_LOCK_UNLOCK_USER;
/

-- Tao role
CREATE OR REPLACE PROCEDURE SP_CREATE_ROLE (
    p_rolename IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'CREATE ROLE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_rolename);
END SP_CREATE_ROLE;
/

-- Xoa role
CREATE OR REPLACE PROCEDURE SP_DROP_ROLE (
    p_rolename IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'DROP ROLE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_rolename);
END SP_DROP_ROLE;
/


-- ============================================================
-- B3: DANH SACH USER / ROLE / OBJECT / COLUMN
-- Tich hop tu PH1; bo sung LBACSYS vao exclusion list
-- ============================================================

-- Danh sach user ung dung (loai bo system user + LBACSYS)
CREATE OR REPLACE FUNCTION FN_LIST_USERS
RETURN T_USER_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT USERNAME, ACCOUNT_STATUS, CREATED, DEFAULT_TABLESPACE, PROFILE
        FROM   DBA_USERS
        WHERE  USERNAME NOT IN (
            'SYS','SYSTEM','DBSNMP','APPQOSSYS','AUDSYS','CTXSYS',
            'DVSYS','GSMADMIN_INTERNAL','LBACSYS','MDSYS','OJVMSYS',
            'OLAPSYS','ORDDATA','ORDSYS','OUTLN','REMOTE_SCHEDULER_AGENT',
            'SI_INFORMTN_SCHEMA','SYS$UMF','SYSBACKUP','SYSDG','SYSKM',
            'SYSRAC','WMSYS','XDB','XS$NULL',
            'BVDBA'  -- Loai bo chinh DBA khoi danh sach hien thi nguoi dung
        )
        ORDER BY USERNAME
    ) LOOP
        PIPE ROW(T_USER_ROW(r.USERNAME, r.ACCOUNT_STATUS, r.CREATED,
                            r.DEFAULT_TABLESPACE, r.PROFILE));
    END LOOP;
END FN_LIST_USERS;
/

-- Danh sach role ung dung (loai bo system role)
CREATE OR REPLACE FUNCTION FN_LIST_ROLES
RETURN T_ROLE_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT ROLE, PASSWORD_REQUIRED
        FROM   DBA_ROLES
        WHERE  ROLE NOT IN (
            'ADM_PARALLEL_EXECUTE_TASK','APEX_ADMINISTRATOR_ROLE',
            'AQ_ADMINISTRATOR_ROLE','AQ_USER_ROLE','AUDIT_ADMIN',
            'AUDIT_VIEWER','AUTHENTICATEDUSER','CAPTURE_ADMIN',
            'CDB_DBA','CONNECT','CTXAPP','DATAPUMP_EXP_FULL_DATABASE',
            'DATAPUMP_IMP_FULL_DATABASE','DBA','DBFS_ROLE',
            'DELETE_CATALOG_ROLE','EXECUTE_CATALOG_ROLE',
            'EXP_FULL_DATABASE','GATHER_SYSTEM_STATISTICS',
            'GDS_CATALOG_SELECT','GLOBAL_AQ_USER_ROLE',
            'HS_ADMIN_EXECUTE_ROLE','HS_ADMIN_ROLE','HS_ADMIN_SELECT_ROLE',
            'IMP_FULL_DATABASE','JAVA_ADMIN','JAVA_DEPLOY',
            'JMXSERVER','LBAC_DBA','LOGSTDBY_ADMINISTRATOR',
            'OEM_ADVISOR','OEM_MONITOR','OLAP_DBA','OLAP_USER',
            'OLAP_XS_ADMIN','OPTIMIZER_PROCESSING_RATE','ORDADMIN',
            'PDB_DBA','PROVISIONER','RECOVERY_CATALOG_OWNER',
            'RECOVERY_CATALOG_OWNER_VPD','RESOURCE','SCHEDULER_ADMIN',
            'SELECT_CATALOG_ROLE','SPATIAL_CSW_ADMIN','SPATIAL_WFS_ADMIN',
            'SYSUMF_ROLE','WM_ADMIN_ROLE','XDBADMIN','XDB_SET_INVOKER',
            'XDB_WEBSERVICES','XDB_WEBSERVICES_OVER_HTTP',
            'XDB_WEBSERVICES_WITH_PUBLIC'
        )
        ORDER BY ROLE
    ) LOOP
        PIPE ROW(T_ROLE_ROW(r.ROLE, r.PASSWORD_REQUIRED));
    END LOOP;
END FN_LIST_ROLES;
/

-- Danh sach object trong schema BVDBA
CREATE OR REPLACE FUNCTION FN_LIST_OBJECTS
RETURN T_OBJECT_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
        FROM   USER_OBJECTS
        WHERE  OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION','PACKAGE')
        ORDER  BY OBJECT_TYPE, OBJECT_NAME
    ) LOOP
        PIPE ROW(T_OBJECT_ROW(r.OBJECT_NAME, r.OBJECT_TYPE, r.STATUS));
    END LOOP;
END FN_LIST_OBJECTS;
/

-- Danh sach cot cua bang/view
CREATE OR REPLACE FUNCTION FN_LIST_COLUMNS (
    p_object_name IN VARCHAR2
) RETURN T_COLUMN_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT COLUMN_NAME, DATA_TYPE, NULLABLE
        FROM   USER_TAB_COLUMNS
        WHERE  TABLE_NAME = UPPER(p_object_name)
        ORDER  BY COLUMN_ID
    ) LOOP
        PIPE ROW(T_COLUMN_ROW(r.COLUMN_NAME, r.DATA_TYPE, r.NULLABLE));
    END LOOP;
END FN_LIST_COLUMNS;
/


-- ============================================================
-- B4: CAP QUYEN
-- Tich hop nguyen tu PH1 Section 4
-- ============================================================

-- Cap quyen he thong (WITH ADMIN OPTION tuy chon)
CREATE OR REPLACE PROCEDURE SP_GRANT_SYS_PRIV (
    p_privilege      IN VARCHAR2,
    p_grantee        IN VARCHAR2,
    p_with_admin_opt IN VARCHAR2 DEFAULT 'NO'
) AS
    v_sql VARCHAR2(500);
BEGIN
    v_sql := 'GRANT ' || p_privilege ||
             ' TO '   || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    IF UPPER(p_with_admin_opt) = 'YES' THEN
        v_sql := v_sql || ' WITH ADMIN OPTION';
    END IF;
    EXECUTE IMMEDIATE v_sql;
END SP_GRANT_SYS_PRIV;
/

-- Cap quyen doi tuong (ho tro phan quyen theo cot cho SELECT va UPDATE)
-- SELECT theo cot: tao View trung gian tranh lo thong tin nhay cam
-- UPDATE theo cot: GRANT UPDATE(col)
-- INSERT/DELETE/EXECUTE: khong ho tro theo cot
CREATE OR REPLACE PROCEDURE SP_GRANT_OBJ_PRIV (
    p_privilege      IN VARCHAR2,
    p_object_owner   IN VARCHAR2,
    p_object_name    IN VARCHAR2,
    p_grantee        IN VARCHAR2,
    p_columns        IN VARCHAR2 DEFAULT NULL,
    p_with_grant_opt IN VARCHAR2 DEFAULT 'NO'
) AS
    v_sql       VARCHAR2(2000);
    v_object    VARCHAR2(300);
    v_priv      VARCHAR2(20);
    v_view_name VARCHAR2(128);
BEGIN
    v_priv   := UPPER(TRIM(p_privilege));
    v_object := DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_name);

    IF p_columns IS NOT NULL AND v_priv IN ('INSERT','DELETE','EXECUTE') THEN
        RAISE_APPLICATION_ERROR(-20002,
            'Quyen ' || v_priv || ' khong ho tro phan quyen theo cot!');
    END IF;

    IF p_columns IS NOT NULL AND v_priv = 'UPDATE' THEN
        v_sql := 'GRANT UPDATE (' || p_columns || ') ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);

    ELSIF p_columns IS NOT NULL AND v_priv = 'SELECT' THEN
        -- Tao View trung gian cho SELECT theo cot
        v_view_name := 'V_' || SUBSTR(p_object_name,1,15) ||
                       '_'   || SUBSTR(p_grantee,1,10);
        v_sql := 'CREATE OR REPLACE VIEW ' ||
                 DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                 v_view_name ||
                 ' AS SELECT ' || p_columns || ' FROM ' || v_object;
        EXECUTE IMMEDIATE v_sql;
        v_sql := 'GRANT SELECT ON ' ||
                 DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                 v_view_name ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);

    ELSE
        v_sql := 'GRANT ' || v_priv || ' ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    END IF;

    IF UPPER(p_with_grant_opt) = 'YES' THEN
        v_sql := v_sql || ' WITH GRANT OPTION';
    END IF;

    EXECUTE IMMEDIATE v_sql;
END SP_GRANT_OBJ_PRIV;
/

-- Gan role cho user/role (WITH ADMIN OPTION tuy chon)
CREATE OR REPLACE PROCEDURE SP_GRANT_ROLE (
    p_role           IN VARCHAR2,
    p_grantee        IN VARCHAR2,
    p_with_admin_opt IN VARCHAR2 DEFAULT 'NO'
) AS
    v_sql VARCHAR2(300);
BEGIN
    v_sql := 'GRANT ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_role) ||
             ' TO '   || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    IF UPPER(p_with_admin_opt) = 'YES' THEN
        v_sql := v_sql || ' WITH ADMIN OPTION';
    END IF;
    EXECUTE IMMEDIATE v_sql;
END SP_GRANT_ROLE;
/


-- ============================================================
-- B5: THU HOI QUYEN
-- Tich hop nguyen tu PH1 Section 4
-- ============================================================

-- Thu hoi quyen doi tuong (ho tro theo cot)
CREATE OR REPLACE PROCEDURE SP_REVOKE_OBJ_PRIV (
    p_privilege    IN VARCHAR2,
    p_object_owner IN VARCHAR2,
    p_object_name  IN VARCHAR2,
    p_grantee      IN VARCHAR2,
    p_columns      IN VARCHAR2 DEFAULT NULL
) AS
    v_sql    VARCHAR2(2000);
    v_object VARCHAR2(300);
    v_priv   VARCHAR2(20);
BEGIN
    v_priv   := UPPER(TRIM(p_privilege));
    v_object := DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_name);

    IF p_columns IS NOT NULL AND v_priv IN ('SELECT','UPDATE') THEN
        v_sql := 'REVOKE ' || v_priv ||
                 ' (' || p_columns || ')' ||
                 ' ON ' || v_object ||
                 ' FROM ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    ELSE
        v_sql := 'REVOKE ' || v_priv ||
                 ' ON ' || v_object ||
                 ' FROM ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    END IF;

    EXECUTE IMMEDIATE v_sql;
END SP_REVOKE_OBJ_PRIV;
/

-- Thu hoi quyen he thong
CREATE OR REPLACE PROCEDURE SP_REVOKE_SYS_PRIV (
    p_privilege IN VARCHAR2,
    p_grantee   IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'REVOKE ' || p_privilege ||
        ' FROM '  || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
END SP_REVOKE_SYS_PRIV;
/

-- Thu hoi role
CREATE OR REPLACE PROCEDURE SP_REVOKE_ROLE (
    p_role    IN VARCHAR2,
    p_grantee IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'REVOKE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_role) ||
        ' FROM '  || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
END SP_REVOKE_ROLE;
/


-- ============================================================
-- B6: TRA CUU QUYEN CUA USER / ROLE
-- Tich hop nguyen tu PH1 Section 4
-- ============================================================

-- Quyen doi tuong (bang + cot) cua mot grantee
CREATE OR REPLACE FUNCTION FN_GET_OBJ_PRIVS (
    p_grantee IN VARCHAR2
) RETURN T_OBJPRIV_TABLE PIPELINED AS
BEGIN
    -- Quyen tren toan doi tuong
    FOR r IN (
        SELECT TP.GRANTEE, TP.OWNER, TP.TABLE_NAME,
               O.OBJECT_TYPE, TP.PRIVILEGE, TP.GRANTABLE
        FROM   DBA_TAB_PRIVS TP
               LEFT JOIN DBA_OBJECTS O
                   ON O.OWNER = TP.OWNER AND O.OBJECT_NAME = TP.TABLE_NAME
        WHERE  UPPER(TP.GRANTEE) = UPPER(p_grantee)
        ORDER  BY TP.OWNER, TP.TABLE_NAME, TP.PRIVILEGE
    ) LOOP
        PIPE ROW(T_OBJPRIV_ROW(r.GRANTEE, r.OWNER, r.TABLE_NAME,
                               r.OBJECT_TYPE, r.PRIVILEGE, r.GRANTABLE, NULL));
    END LOOP;

    -- Quyen theo cot
    FOR c IN (
        SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE, COLUMN_NAME
        FROM   DBA_COL_PRIVS
        WHERE  UPPER(GRANTEE) = UPPER(p_grantee)
        ORDER  BY OWNER, TABLE_NAME, COLUMN_NAME, PRIVILEGE
    ) LOOP
        PIPE ROW(T_OBJPRIV_ROW(c.GRANTEE, c.OWNER, c.TABLE_NAME,
                               'COLUMN', c.PRIVILEGE, c.GRANTABLE, c.COLUMN_NAME));
    END LOOP;
END FN_GET_OBJ_PRIVS;
/

-- Quyen he thong cua mot grantee
CREATE OR REPLACE FUNCTION FN_GET_SYS_PRIVS (
    p_grantee IN VARCHAR2
) RETURN T_SYSPRIV_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT GRANTEE, PRIVILEGE, ADMIN_OPTION
        FROM   DBA_SYS_PRIVS
        WHERE  UPPER(GRANTEE) = UPPER(p_grantee)
        ORDER  BY PRIVILEGE
    ) LOOP
        PIPE ROW(T_SYSPRIV_ROW(r.GRANTEE, r.PRIVILEGE, r.ADMIN_OPTION));
    END LOOP;
END FN_GET_SYS_PRIVS;
/

-- Role duoc gan cho mot grantee
CREATE OR REPLACE FUNCTION FN_GET_ROLE_PRIVS (
    p_grantee IN VARCHAR2
) RETURN T_ROLEPRIV_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT GRANTEE, GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
        FROM   DBA_ROLE_PRIVS
        WHERE  UPPER(GRANTEE) = UPPER(p_grantee)
        ORDER  BY GRANTED_ROLE
    ) LOOP
        PIPE ROW(T_ROLEPRIV_ROW(r.GRANTEE, r.GRANTED_ROLE,
                                r.ADMIN_OPTION, r.DEFAULT_ROLE));
    END LOOP;
END FN_GET_ROLE_PRIVS;
/


-- ============================================================
-- B7: VIEWS NGHIEP VU - THICH NGHI VOI SCHEMA PH2
-- Tu PH1 Section 3, chinh sua tuong thich voi PH2
-- ============================================================

-- V_BENHNHAN_BASIC: thich nghi (khong con DIACHI, them CCCD)
CREATE OR REPLACE VIEW V_BENHNHAN_BASIC AS
    SELECT MABN, TENBN, PHAI, NGAYSINH, CCCD
    FROM   BENHNHAN;

-- V_HSBA_SUMMARY: thich nghi (them MABS, MAKHOA tu PH2)
CREATE OR REPLACE VIEW V_HSBA_SUMMARY AS
    SELECT H.MAHSBA, H.MABN, B.TENBN, H.NGAY,
           H.CHANDOAN, H.KETLUAN,
           H.MABS, NV.HOTEN AS TEN_BACSI, H.MAKHOA
    FROM   HSBA H
           JOIN BENHNHAN B  ON H.MABN = B.MABN
           JOIN NHANVIEN  NV ON H.MABS = NV.MANV;


-- ============================================================
-- B8: STORED PROCEDURES NGHIEP VU - THICH NGHI VOI SCHEMA PH2
-- ============================================================

-- SP_THEM_BENHNHAN: dung schema day du cua PH2
CREATE OR REPLACE PROCEDURE SP_THEM_BENHNHAN (
    p_mabn          IN VARCHAR2,
    p_tenbn         IN NVARCHAR2,
    p_phai          IN NVARCHAR2,   -- 'Nam' hoac 'Nu'
    p_ngaysinh      IN DATE,
    p_cccd          IN VARCHAR2,
    p_sonha         IN NVARCHAR2,
    p_tenduong      IN NVARCHAR2,
    p_quanhuyen     IN NVARCHAR2,
    p_tinhthanh     IN NVARCHAR2,
    p_tiensubenh    IN NVARCHAR2 DEFAULT NULL,
    p_tiensubenhgd  IN NVARCHAR2 DEFAULT NULL,
    p_diungThuoc    IN NVARCHAR2 DEFAULT NULL,
    p_oracle_username IN VARCHAR2 DEFAULT NULL
) AS
BEGIN
    INSERT INTO BENHNHAN (
        MABN, TENBN, PHAI, NGAYSINH, CCCD,
        SONHA, TENDUONG, QUANHUYEN, TINHTHANH,
        TIENSUBENH, TIENSUBENHGD, DIUNGthuoc, ORACLE_USERNAME
    ) VALUES (
        p_mabn, p_tenbn, p_phai, p_ngaysinh, p_cccd,
        p_sonha, p_tenduong, p_quanhuyen, p_tinhthanh,
        p_tiensubenh, p_tiensubenhgd, p_diungThuoc,
        UPPER(TRIM(p_oracle_username))
    );
    COMMIT;
END SP_THEM_BENHNHAN;
/

-- SP_CAP_NHAT_HSBA: them kiem soat MABS (bac si chi sua HSBA cua chinh minh)
-- Luu y: VPD da tu dong loc dong; SP nay them tang bao ve tuong minh
CREATE OR REPLACE PROCEDURE SP_CAP_NHAT_HSBA (
    p_mahsba   IN VARCHAR2,
    p_chandoan IN NVARCHAR2,
    p_dieutri  IN NVARCHAR2,
    p_ketluan  IN NVARCHAR2
) AS
    v_rows NUMBER;
BEGIN
    UPDATE HSBA
    SET    CHANDOAN = p_chandoan,
           DIEUTRI  = p_dieutri,
           KETLUAN  = p_ketluan
    WHERE  MAHSBA = p_mahsba;
    -- VPD da loc: neu cap nhat 0 dong, co the la HSBA khong phai cua BS nay
    v_rows := SQL%ROWCOUNT;
    IF v_rows = 0 THEN
        RAISE_APPLICATION_ERROR(-20010,
            'HSBA khong ton tai hoac ban khong co quyen cap nhat HSBA nay.');
    END IF;
    COMMIT;
END SP_CAP_NHAT_HSBA;
/

-- FN_DEM_BENHNHAN: khong thay doi
CREATE OR REPLACE FUNCTION FN_DEM_BENHNHAN
RETURN NUMBER AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM BENHNHAN;
    RETURN v_count;
END FN_DEM_BENHNHAN;
/

-- FN_TEN_BENHNHAN: khong thay doi
CREATE OR REPLACE FUNCTION FN_TEN_BENHNHAN (
    p_mabn IN VARCHAR2
) RETURN NVARCHAR2 AS
    v_ten NVARCHAR2(100);
BEGIN
    SELECT TENBN INTO v_ten FROM BENHNHAN WHERE MABN = p_mabn;
    RETURN v_ten;
EXCEPTION
    WHEN NO_DATA_FOUND THEN RETURN NULL;
END FN_TEN_BENHNHAN;
/

COMMIT;


-- ============================================================
-- B9: KIEM TRA SAU TICH HOP
-- ============================================================v
SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
FROM   USER_OBJECTS
WHERE  OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION','PACKAGE','TYPE')
ORDER  BY OBJECT_TYPE, OBJECT_NAME;

-- Kiem tra danh sach user ung dung
SELECT * FROM TABLE(FN_LIST_USERS);

-- Kiem tra danh sach role ung dung
SELECT * FROM TABLE(FN_LIST_ROLES);

-- Kiem tra quyen doi tuong cua mot role
SELECT * FROM TABLE(FN_GET_OBJ_PRIVS('RL_BACSI'));
SELECT * FROM TABLE(FN_GET_ROLE_PRIVS('RL_BACSI'));


-- ============================================================
-- YEU CAU 3 - CAU 3 + CAU 4: FGA + STANDARD AUDIT
-- Chay voi session BVDBA
-- ============================================================

-- ---- BUOC 1: XOA POLICY CU (1 block duy nhat) ----
BEGIN
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'DONTHUOC', 'FGA_DT_UPDATE_AFTER_CREATE');  EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA',     'FGA_HSBA_BS_UPDATE');          EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA',     'FGA_HSBA_ILLEGAL_UPDATE');     EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA_DV',  'FGA_HSBA_DV_ILLEGAL_DML');    EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA_DV',  'FGA_HSBA_DV_KTV_UPDATE');     EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA_DV',  'AUDIT_KTV_UPDATE_KETQUA');    EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'HSBA',     'FGA_HSBA_DOCTOR_UPDATE');     EXCEPTION WHEN OTHERS THEN NULL; END;
    BEGIN DBMS_FGA.DROP_POLICY(USER, 'DONTHUOC', 'FGA_DONTHUOC_DOCTOR_UPDATE'); EXCEPTION WHEN OTHERS THEN NULL; END;
END;
/

-- ---- BUOC 2: TAO FGA POLICY MOI (1 block duy nhat) ----
BEGIN
    -- (a) Ghi vet UPDATE don thuoc sau khi da tao (TC#3e)
    DBMS_FGA.ADD_POLICY(
        object_schema   => USER,
        object_name     => 'DONTHUOC',
        policy_name     => 'FGA_DT_UPDATE_AFTER_CREATE',
        audit_column    => 'TENTHUOC,LIEUDUNG',
        statement_types => 'UPDATE',
        enable          => TRUE
    );

    -- (b) Ghi vet UPDATE THANH CONG cua Bac si/Y si tren HSBA (TC#3c)
    DBMS_FGA.ADD_POLICY(
        object_schema   => USER,
        object_name     => 'HSBA',
        policy_name     => 'FGA_HSBA_BS_UPDATE',
        audit_column    => 'CHANDOAN,DIEUTRI,KETLUAN',
        statement_types => 'UPDATE',
        enable          => TRUE
    );

    -- (c)+(d) Ghi vet moi DML tren HSBA_DV (bao gom ca bat hop phap)
    DBMS_FGA.ADD_POLICY(
        object_schema   => USER,
        object_name     => 'HSBA_DV',
        policy_name     => 'FGA_HSBA_DV_ILLEGAL_DML',
        audit_column    => 'MAKTV,KETQUA,LOAIDV,MAHSBA',
        statement_types => 'INSERT,UPDATE,DELETE',
        enable          => TRUE
    );
END;
/

-- ---- BUOC 3: STANDARD AUDIT (plain SQL, khong can /) ----

-- Xoa audit cu
NOAUDIT SELECT  ON BVDBA.HSBA;
NOAUDIT UPDATE  ON BVDBA.HSBA;
NOAUDIT INSERT  ON BVDBA.HSBA_DV;
NOAUDIT UPDATE  ON BVDBA.HSBA_DV;
NOAUDIT DELETE  ON BVDBA.HSBA_DV;
NOAUDIT UPDATE  ON BVDBA.DONTHUOC;
NOAUDIT EXECUTE ON BVDBA.SP_CAP_NHAT_HSBA;
NOAUDIT SELECT  ON BVDBA.VW_BN_THONGTIN_CANHAN;
NOAUDIT EXECUTE ON BVDBA.SP_THEM_BENHNHAN;

-- Ngu canh 1: Doc HSBA
AUDIT SELECT ON BVDBA.HSBA BY ACCESS WHENEVER SUCCESSFUL;
AUDIT SELECT ON BVDBA.HSBA BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngu canh 2: Sua HSBA (bao phu TC#3c - bat hop phap)
AUDIT UPDATE ON BVDBA.HSBA BY ACCESS WHENEVER SUCCESSFUL;
AUDIT UPDATE ON BVDBA.HSBA BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngu canh 3: DML bat hop phap tren HSBA_DV (TC#3d)
AUDIT INSERT ON BVDBA.HSBA_DV BY ACCESS WHENEVER NOT SUCCESSFUL;
AUDIT UPDATE ON BVDBA.HSBA_DV BY ACCESS WHENEVER NOT SUCCESSFUL;
AUDIT DELETE ON BVDBA.HSBA_DV BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngu canh 4: Stored Procedure cap nhat HSBA
AUDIT EXECUTE ON BVDBA.SP_CAP_NHAT_HSBA BY ACCESS WHENEVER SUCCESSFUL;
AUDIT EXECUTE ON BVDBA.SP_CAP_NHAT_HSBA BY ACCESS WHENEVER NOT SUCCESSFUL;

-- Ngu canh 5: Benh nhan xem thong tin ca nhan
AUDIT SELECT ON BVDBA.VW_BN_THONGTIN_CANHAN BY ACCESS WHENEVER SUCCESSFUL;
AUDIT SELECT ON BVDBA.VW_BN_THONGTIN_CANHAN BY ACCESS WHENEVER NOT SUCCESSFUL;

COMMIT;


-- [1] FGA: toan bo hanh vi
SELECT
    TO_CHAR(TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS') AS THOI_GIAN,
    DB_USER                                       AS NGUOI_DUNG,
    OBJECT_SCHEMA || '.' || OBJECT_NAME           AS DOI_TUONG,
    POLICY_NAME,
    STATEMENT_TYPE                                AS HANH_VI,
    SCN,
    SUBSTR(SQL_TEXT, 1, 200)                      AS NOI_DUNG_SQL
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA = 'BVDBA'
ORDER BY TIMESTAMP DESC;

-- [2] Standard Audit: toan bo
SELECT
    TO_CHAR(TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS')  AS THOI_GIAN,
    USERNAME                                       AS NGUOI_DUNG,
    OWNER || '.' || OBJ_NAME                       AS DOI_TUONG,
    ACTION_NAME                                    AS HANH_VI,
    CASE WHEN RETURNCODE = 0
         THEN 'THANH CONG'
         ELSE 'THAT BAI (ORA-' || RETURNCODE || ')'
    END                                            AS KET_QUA,
    SUBSTR(SQL_TEXT, 1, 200)                       AS NOI_DUNG_SQL
FROM DBA_AUDIT_TRAIL
WHERE OWNER = 'BVDBA'
ORDER BY TIMESTAMP DESC;

-- [3] Chi hanh vi THAT BAI
SELECT
    TO_CHAR(TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS')  AS THOI_GIAN,
    USERNAME                                       AS NGUOI_DUNG,
    OWNER || '.' || OBJ_NAME                       AS DOI_TUONG,
    ACTION_NAME                                    AS HANH_VI,
    RETURNCODE                                     AS MA_LOI,
    SUBSTR(SQL_TEXT, 1, 200)                       AS NOI_DUNG_SQL
FROM DBA_AUDIT_TRAIL
WHERE OWNER      = 'BVDBA'
  AND RETURNCODE != 0
ORDER BY TIMESTAMP DESC;

-- [4] FGA: UPDATE tren HSBA
SELECT
    TO_CHAR(TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS')  AS THOI_GIAN,
    DB_USER                                        AS NGUOI_DUNG,
    POLICY_NAME,
    SUBSTR(SQL_TEXT, 1, 300)                       AS NOI_DUNG_SQL
FROM DBA_FGA_AUDIT_TRAIL
WHERE OBJECT_SCHEMA  = 'BVDBA'
  AND OBJECT_NAME    = 'HSBA'
  AND STATEMENT_TYPE = 'UPDATE'
ORDER BY TIMESTAMP DESC;

-- [5] Thong ke tong hop
SELECT
    NGUOI_DUNG,
    DOI_TUONG,
    HANH_VI,
    COUNT(*)                                      AS TONG,
    SUM(CASE WHEN MA_LOI = 0  THEN 1 ELSE 0 END) AS THANH_CONG,
    SUM(CASE WHEN MA_LOI != 0 THEN 1 ELSE 0 END) AS THAT_BAI
FROM (
    SELECT
        USERNAME                  AS NGUOI_DUNG,
        OWNER || '.' || OBJ_NAME  AS DOI_TUONG,
        ACTION_NAME               AS HANH_VI,
        RETURNCODE                AS MA_LOI
    FROM DBA_AUDIT_TRAIL
    WHERE OWNER = 'BVDBA'
)
GROUP BY NGUOI_DUNG, DOI_TUONG, HANH_VI
ORDER BY NGUOI_DUNG, DOI_TUONG;

