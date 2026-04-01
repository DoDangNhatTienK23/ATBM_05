-- ============================================================
-- NOI DUNG:
--   SECTION 0: Bootstrap - SYS tao BVDBA (DBA cua he thong)
--   SECTION 1: BVDBA tao cac bang (Doi tuong cap quyen)
--   SECTION 2: BVDBA tao user/role 
--   SECTION 3: BVDBA tao view/procedure/function
--   SECTION 4: Cac procedure/function API phuc vu ung dung
--   SECTION 5: Kiem tra script
--
-- THUC THI: Chay theo thu tu tu tren xuong.
-- ============================================================

-- ============================================================
-- SECTION 0: BOOTSTRAP
-- Dang nhap: SYS AS SYSDBA, Service name = XEPDB1
-- Muc dich: Tao tai khoan BVDBA - DBA cua he thong benh vien.
--           Tai khoan nay dung de ket noi va thuc hien thao tac quan tri.
-- ============================================================

-- Dam bao dang lam viec trong dung PDB
ALTER SESSION SET CONTAINER = XEPDB1;

-- Tao tablespace rieng cho he thong benh vien
CREATE TABLESPACE BENHVIEN_TBS
    DATAFILE 'benhvien01.dbf' SIZE 100M
    AUTOEXTEND ON NEXT 50M MAXSIZE 1G
    EXTENT MANAGEMENT LOCAL SEGMENT SPACE MANAGEMENT AUTO;

-- Tao BVDBA - schema owner va DBA cua he thong
CREATE USER BVDBA
    IDENTIFIED BY "BvDba#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA UNLIMITED ON BENHVIEN_TBS
    PROFILE DEFAULT;

-- Cap DBA role cho BVDBA
GRANT DBA TO BVDBA;

-- Cap quyen de BVDBA co the quan ly user/role/privilege tren ung dung
GRANT CREATE USER             TO BVDBA;
GRANT ALTER USER              TO BVDBA;
GRANT DROP USER               TO BVDBA;
GRANT CREATE ROLE             TO BVDBA;
GRANT CREATE VIEW             TO BVDBA;
GRANT DROP ANY ROLE           TO BVDBA;
GRANT GRANT ANY ROLE          TO BVDBA;
GRANT GRANT ANY PRIVILEGE     TO BVDBA;
GRANT GRANT ANY OBJECT PRIVILEGE TO BVDBA;

-- Cap quyen doc data dictionary de ung dung hien thi thong tin
GRANT SELECT ON DBA_USERS        TO BVDBA;
GRANT SELECT ON DBA_ROLES        TO BVDBA;
GRANT SELECT ON DBA_ROLE_PRIVS   TO BVDBA;
GRANT SELECT ON DBA_SYS_PRIVS    TO BVDBA;
GRANT SELECT ON DBA_TAB_PRIVS    TO BVDBA;
GRANT SELECT ON DBA_COL_PRIVS    TO BVDBA;
GRANT SELECT ON DBA_OBJECTS      TO BVDBA;
GRANT SELECT ON DBA_TABLES       TO BVDBA;
GRANT SELECT ON DBA_VIEWS        TO BVDBA;
GRANT SELECT ON DBA_PROCEDURES   TO BVDBA;
GRANT SELECT_CATALOG_ROLE        TO BVDBA;

COMMIT;


-- ============================================================
-- SECTION 1: TAO CAC BANG
-- Dang nhap: BVDBA, Service name = XEPDB1
-- Muc dich: Tao cac bang dai dien cho schema benh vien.
-- ============================================================

-- Bang NHANVIEN 
CREATE TABLE NHANVIEN (
    MANV        VARCHAR2(20)   NOT NULL,
    HOTEN       NVARCHAR2(100) NOT NULL,
    PHAI        CHAR(1)        CHECK (PHAI IN ('M','F')),
    NGAYSINH    DATE,
    CMND        VARCHAR2(12),
    SODT        VARCHAR2(15),
    VAITRO      NVARCHAR2(30),
    CHUYENKHOA  NVARCHAR2(100),
    CONSTRAINT PK_NHANVIEN PRIMARY KEY (MANV)
);

-- Bang BENHNHAN 
CREATE TABLE BENHNHAN (
    MABN        VARCHAR2(20)   NOT NULL,
    TENBN       NVARCHAR2(100) NOT NULL,
    PHAI        CHAR(1)        CHECK (PHAI IN ('M','F')),
    NGAYSINH    DATE,
    CCCD        VARCHAR2(12),
    DIACHI      NVARCHAR2(300),
    CONSTRAINT PK_BENHNHAN PRIMARY KEY (MABN)
);

-- Bang HSBA 
CREATE TABLE HSBA (
    MAHSBA      VARCHAR2(20)   NOT NULL,
    MABN        VARCHAR2(20),
    NGAY        DATE,
    CHANDOAN    NVARCHAR2(500),
    DIEUTRI     NVARCHAR2(500),
    KETLUAN     NVARCHAR2(500),
    CONSTRAINT PK_HSBA PRIMARY KEY (MAHSBA)
);

-- Bang DONTHUOC 
CREATE TABLE DONTHUOC (
    MAHSBA      VARCHAR2(20)   NOT NULL,
    TENTHUOC    NVARCHAR2(200) NOT NULL,
    LIEUDUNG    NVARCHAR2(500),
    NGAYDT      DATE,
    CONSTRAINT PK_DONTHUOC PRIMARY KEY (MAHSBA, TENTHUOC)
);

-- Du lieu khoi tao
INSERT INTO NHANVIEN VALUES ('NV001', N'Nguyễn Văn An',  'M', DATE '1985-01-01', '012345678901', '0901000001', N'Bác sĩ/Y sĩ',    N'Tiêu hóa');
INSERT INTO NHANVIEN VALUES ('NV002', N'Trần Thị Bình',  'F', DATE '1990-05-15', '012345678902', '0901000002', N'Điều phối viên', NULL);
INSERT INTO NHANVIEN VALUES ('NV003', N'Lê Văn Cường',   'M', DATE '1988-08-20', '012345678903', '0901000003', N'Kỹ thuật viên',  N'Xét nghiệm');
INSERT INTO BENHNHAN VALUES ('BN001', N'Phạm Thị Dung',  'F', DATE '1995-03-10', '098765432101', N'123 Lê Lợi, Q1, TP.HCM');
INSERT INTO BENHNHAN VALUES ('BN002', N'Hoàng Văn Em',   'M', DATE '1980-11-22', '098765432102', N'456 Trần Hưng Đạo, Q5, TP.HCM');
INSERT INTO HSBA VALUES ('HSBA001', 'BN001', DATE '2026-01-10', N'Viêm dạ dày', N'Kháng axit', N'Ổn định');
INSERT INTO HSBA VALUES ('HSBA002', 'BN002', DATE '2026-01-15', N'Tăng huyết áp', N'Thuốc hạ áp', NULL);
INSERT INTO DONTHUOC VALUES ('HSBA001', N'Omeprazole 20mg', N'1 viên/ngày trước ăn', DATE '2026-01-10');
INSERT INTO DONTHUOC VALUES ('HSBA002', N'Amlodipine 5mg',  N'1 viên/ngày buổi sáng', DATE '2026-01-15');

COMMIT;


-- ============================================================
-- SECTION 2: TAO USER VA ROLE 
-- Dang nhap: BVDBA, Service name = XEPDB1
-- Muc dich: Khoi tao cac user va role co ban cho he thong
-- ============================================================

-- --- Role dai dien cac nhom trong benh vien ---
CREATE ROLE ROLE_BACSI;         
CREATE ROLE ROLE_DIEUPHOIVIEN;  
CREATE ROLE ROLE_KYTHUATVIEN;   
CREATE ROLE ROLE_BENHNHAN;      

-- --- User dai dien tung vai tro ---
CREATE USER U_BACSI01
    IDENTIFIED BY "Bacsi01#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA 0 ON BENHVIEN_TBS;

CREATE USER U_BACSI02
    IDENTIFIED BY "Bacsi02#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA 0 ON BENHVIEN_TBS;

CREATE USER U_DPV01
    IDENTIFIED BY "Dpv01#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA 0 ON BENHVIEN_TBS;

CREATE USER U_KTV01
    IDENTIFIED BY "Ktv01#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA 0 ON BENHVIEN_TBS;

CREATE USER U_BN01
    IDENTIFIED BY "Bn01#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA 0 ON BENHVIEN_TBS;

-- Cap quyen login cho cac user
GRANT CREATE SESSION TO U_BACSI01, U_BACSI02, U_DPV01, U_KTV01, U_BN01;

-- Gan role khoi tao cho user 
GRANT ROLE_BACSI        TO U_BACSI01, U_BACSI02;
GRANT ROLE_DIEUPHOIVIEN TO U_DPV01;
GRANT ROLE_KYTHUATVIEN  TO U_KTV01;
GRANT ROLE_BENHNHAN     TO U_BN01;

COMMIT;


-- ============================================================
-- SECTION 3: TAO VIEW / PROCEDURE / FUNCTION 
-- Dang nhap: BVDBA, Service name = XEPDB1
-- ============================================================

-- --- VIEW ---
CREATE OR REPLACE VIEW V_BENHNHAN_BASIC AS
    SELECT MABN, TENBN, PHAI, NGAYSINH
    FROM   BENHNHAN;

CREATE OR REPLACE VIEW V_HSBA_SUMMARY AS
    SELECT H.MAHSBA, H.MABN, B.TENBN, H.NGAY, H.CHANDOAN, H.KETLUAN
    FROM   HSBA H JOIN BENHNHAN B ON H.MABN = B.MABN;

-- --- STORED PROCEDURE ---
CREATE OR REPLACE PROCEDURE SP_THEM_BENHNHAN (
    p_mabn   IN VARCHAR2,
    p_tenbn  IN NVARCHAR2,
    p_phai   IN CHAR,
    p_ngaysinh IN DATE,
    p_cccd   IN VARCHAR2,
    p_diachi IN NVARCHAR2
) AS
BEGIN
    INSERT INTO BENHNHAN (MABN, TENBN, PHAI, NGAYSINH, CCCD, DIACHI)
    VALUES (p_mabn, p_tenbn, p_phai, p_ngaysinh, p_cccd, p_diachi);
    COMMIT;
END SP_THEM_BENHNHAN;
/

CREATE OR REPLACE PROCEDURE SP_CAP_NHAT_HSBA (
    p_mahsba   IN VARCHAR2,
    p_chandoan IN NVARCHAR2,
    p_dieutri  IN NVARCHAR2,
    p_ketluan  IN NVARCHAR2
) AS
BEGIN
    UPDATE HSBA
    SET    CHANDOAN = p_chandoan,
           DIEUTRI  = p_dieutri,
           KETLUAN  = p_ketluan
    WHERE  MAHSBA = p_mahsba;
    COMMIT;
END SP_CAP_NHAT_HSBA;
/

-- --- FUNCTION ---
CREATE OR REPLACE FUNCTION FN_DEM_BENHNHAN
RETURN NUMBER AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM BENHNHAN;
    RETURN v_count;
END FN_DEM_BENHNHAN;
/

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
-- SECTION 4: PROCEDURE/FUNCTION API PHUC VU UNG DUNG
-- Dang nhap: BVDBA, Service name = XEPDB1
-- Muc dich: Cung cap cac API de ung dung goi, tranh viet DDL 
--           truc tiep nham giam thieu rui ro SQL Injection.
-- ============================================================

-- -------------------------------------------------------
-- QUAN LY USER / ROLE
-- -------------------------------------------------------

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

-- Khoa / Mo khoa tai khoan 
-- p_action: 'LOCK' hoac 'UNLOCK'
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


-- -------------------------------------------------------
-- DANH SACH USER VA ROLE
-- -------------------------------------------------------

-- Danh sach user
CREATE OR REPLACE TYPE T_USER_ROW AS OBJECT (
    USERNAME          VARCHAR2(128),
    ACCOUNT_STATUS    VARCHAR2(32),
    CREATED           DATE,
    DEFAULT_TABLESPACE VARCHAR2(30),
    PROFILE           VARCHAR2(128)
);
/
CREATE OR REPLACE TYPE T_USER_TABLE AS TABLE OF T_USER_ROW;
/

CREATE OR REPLACE FUNCTION FN_LIST_USERS
RETURN T_USER_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT USERNAME, ACCOUNT_STATUS, CREATED, DEFAULT_TABLESPACE, PROFILE
        FROM   DBA_USERS
        WHERE  USERNAME NOT IN (  -- Loai bo user he thong Oracle
            'SYS','SYSTEM','DBSNMP','APPQOSSYS','AUDSYS','CTXSYS',
            'DVSYS','GSMADMIN_INTERNAL','LBACSYS','MDSYS','OJVMSYS',
            'OLAPSYS','ORDDATA','ORDSYS','OUTLN','REMOTE_SCHEDULER_AGENT',
            'SI_INFORMTN_SCHEMA','SYS$UMF','SYSBACKUP','SYSDG','SYSKM',
            'SYSRAC','WMSYS','XDB','XS$NULL'
        )
        ORDER BY USERNAME
    ) LOOP
        PIPE ROW(T_USER_ROW(r.USERNAME, r.ACCOUNT_STATUS, r.CREATED,
                            r.DEFAULT_TABLESPACE, r.PROFILE));
    END LOOP;
END FN_LIST_USERS;
/

-- Danh sach role
CREATE OR REPLACE TYPE T_ROLE_ROW AS OBJECT (
    ROLE              VARCHAR2(128),
    PASSWORD_REQUIRED VARCHAR2(8)
);
/
CREATE OR REPLACE TYPE T_ROLE_TABLE AS TABLE OF T_ROLE_ROW;
/

CREATE OR REPLACE FUNCTION FN_LIST_ROLES
RETURN T_ROLE_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT ROLE, PASSWORD_REQUIRED
        FROM   DBA_ROLES
        WHERE  ROLE NOT IN (  -- Loai bo role he thong Oracle
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

-- Danh sach cac object 
CREATE OR REPLACE TYPE T_OBJECT_ROW AS OBJECT (
    OBJECT_NAME  VARCHAR2(128),
    OBJECT_TYPE  VARCHAR2(23),
    STATUS       VARCHAR2(7)
);
/
CREATE OR REPLACE TYPE T_OBJECT_TABLE AS TABLE OF T_OBJECT_ROW;
/

CREATE OR REPLACE FUNCTION FN_LIST_OBJECTS
RETURN T_OBJECT_TABLE PIPELINED AS
BEGIN
    FOR r IN (
        SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
        FROM   USER_OBJECTS
        WHERE  OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION')
        ORDER  BY OBJECT_TYPE, OBJECT_NAME
    ) LOOP
        PIPE ROW(T_OBJECT_ROW(r.OBJECT_NAME, r.OBJECT_TYPE, r.STATUS));
    END LOOP;
END FN_LIST_OBJECTS;
/

-- Danh sach cot cua mot bang/view 
CREATE OR REPLACE TYPE T_COLUMN_ROW AS OBJECT (
    COLUMN_NAME  VARCHAR2(128),
    DATA_TYPE    VARCHAR2(128),
    NULLABLE     VARCHAR2(1)
);
/
CREATE OR REPLACE TYPE T_COLUMN_TABLE AS TABLE OF T_COLUMN_ROW;
/

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


-- -------------------------------------------------------
-- THUC THI CAP QUYEN
-- -------------------------------------------------------

-- Cap quyen he thong (system privilege)
-- p_with_admin_opt: 'YES' = WITH ADMIN OPTION | 'NO' = khong
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

-- Cap quyen doi tuong (object privilege) 
-- p_columns: danh sach cot cach nhau boi dau phay, VD: 'HOTEN,SODT'
-- p_with_grant_opt: 'YES' = WITH GRANT OPTION | 'NO' = khong
CREATE OR REPLACE PROCEDURE SP_GRANT_OBJ_PRIV (
    p_privilege      IN VARCHAR2,
    p_object_owner   IN VARCHAR2,
    p_object_name    IN VARCHAR2,
    p_grantee        IN VARCHAR2,
    p_columns        IN VARCHAR2 DEFAULT NULL,
    p_with_grant_opt IN VARCHAR2 DEFAULT 'NO'
) AS
    v_sql          VARCHAR2(2000);
    v_object       VARCHAR2(300);
    v_priv         VARCHAR2(20);
    v_view_name    VARCHAR2(128);
BEGIN
    v_priv   := UPPER(TRIM(p_privilege));
    v_object := DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_name);

    -- Kiem tra tinh hop le khi cap quyen theo cot
    IF p_columns IS NOT NULL AND v_priv IN ('INSERT','DELETE','EXECUTE') THEN
        RAISE_APPLICATION_ERROR(-20002, 'Quyen ' || v_priv || ' khong ho tro phan quyen theo cot!');
    END IF;

    -- Xu ly quyen UPDATE tren muc cot 
    IF p_columns IS NOT NULL AND v_priv = 'UPDATE' THEN
        v_sql := 'GRANT UPDATE (' || p_columns || ') ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);

    -- Xu ly quyen SELECT tren muc cot (Tao View trung gian)
    ELSIF p_columns IS NOT NULL AND v_priv = 'SELECT' THEN
        v_view_name := 'V_' || SUBSTR(p_object_name, 1, 15) || '_' || SUBSTR(p_grantee, 1, 10);

        v_sql := 'CREATE OR REPLACE VIEW ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' || v_view_name ||
                 ' AS SELECT ' || p_columns || ' FROM ' || v_object;
        EXECUTE IMMEDIATE v_sql;

        v_sql := 'GRANT SELECT ON ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' || v_view_name ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);

    -- Xu ly cap quyen tren toan doi tuong
    ELSE
        v_sql := 'GRANT ' || v_priv || ' ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    END IF;

    -- Xu ly tuy chon WITH GRANT OPTION
    IF UPPER(p_with_grant_opt) = 'YES' THEN
        v_sql := v_sql || ' WITH GRANT OPTION';
    END IF;

    EXECUTE IMMEDIATE v_sql;
END SP_GRANT_OBJ_PRIV;
/

-- Cap role cho user
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

-- Test thuc thi tao user va cap quyen
EXEC SP_CREATE_USER('U_TEST_SQL', '123456');

SELECT * FROM TABLE(FN_LIST_USERS) WHERE USERNAME = 'U_TEST_SQL';

EXEC SP_GRANT_OBJ_PRIV('SELECT', 'BVDBA', 'BENHNHAN', 'U_TEST_SQL', NULL, 'NO');

SELECT * FROM DBA_TAB_PRIVS WHERE GRANTEE = 'U_TEST_SQL' AND TABLE_NAME = 'BENHNHAN';

EXEC SP_GRANT_OBJ_PRIV('SELECT', 'BVDBA', 'NHANVIEN', 'U_TEST_SQL', 'HOTEN, SODT', 'NO');

-- -------------------------------------------------------
-- THU HOI QUYEN 
-- -------------------------------------------------------

-- Thu hoi quyen doi tuong
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


-- -------------------------------------------------------
-- TRUY VAN THONG TIN QUYEN 
-- -------------------------------------------------------

-- Xem quyen doi tuong 
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

CREATE OR REPLACE FUNCTION FN_GET_OBJ_PRIVS (
    p_grantee IN VARCHAR2
) RETURN T_OBJPRIV_TABLE PIPELINED AS
BEGIN
    -- Quyen cap tren toan doi tuong 
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

    -- Quyen cap theo cot 
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

-- Xem quyen he thong 
CREATE OR REPLACE TYPE T_SYSPRIV_ROW AS OBJECT (
    GRANTEE   VARCHAR2(128),
    PRIVILEGE VARCHAR2(40),
    ADMIN_OPT VARCHAR2(3)
);
/
CREATE OR REPLACE TYPE T_SYSPRIV_TABLE AS TABLE OF T_SYSPRIV_ROW;
/

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

-- Xem role
CREATE OR REPLACE TYPE T_ROLEPRIV_ROW AS OBJECT (
    GRANTEE      VARCHAR2(128),
    GRANTED_ROLE VARCHAR2(128),
    ADMIN_OPTION VARCHAR2(3),
    DEFAULT_ROLE VARCHAR2(3)
);
/
CREATE OR REPLACE TYPE T_ROLEPRIV_TABLE AS TABLE OF T_ROLEPRIV_ROW;
/

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

COMMIT;


-- ============================================================
-- SECTION 5: KIEM TRA TRANG THAI
-- Dang nhap: BVDBA
-- ============================================================

-- Kiem tra object da tao
SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
FROM   USER_OBJECTS
WHERE  OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION')
ORDER  BY OBJECT_TYPE, OBJECT_NAME;

-- Kiem tra user 
SELECT * FROM TABLE(FN_LIST_USERS);

-- Kiem tra role 
SELECT * FROM TABLE(FN_LIST_ROLES);

-- Kiem tra quyen cua U_BACSI01
SELECT * FROM TABLE(FN_GET_ROLE_PRIVS('U_BACSI01'));

-- Kiem tra du lieu 
SELECT COUNT(*) AS SO_NV   FROM NHANVIEN;
SELECT COUNT(*) AS SO_BN   FROM BENHNHAN;
SELECT COUNT(*) AS SO_HSBA FROM HSBA;

-- ============================================================
-- TOM TAT DOI TUONG DA TAO
-- ============================================================
-- TABLESPACE : BENHVIEN_TBS
-- USER (DBA) : BVDBA
-- USER       : U_BACSI01, U_BACSI02, U_DPV01, U_KTV01, U_BN01
-- ROLE       : ROLE_BACSI, ROLE_DIEUPHOIVIEN, ROLE_KYTHUATVIEN, ROLE_BENHNHAN
-- TABLE      : NHANVIEN, BENHNHAN, HSBA, DONTHUOC
-- VIEW       : V_BENHNHAN_BASIC, V_HSBA_SUMMARY
-- PROCEDURE  : SP_THEM_BENHNHAN, SP_CAP_NHAT_HSBA 
--              SP_CREATE_USER, SP_DROP_USER, SP_ALTER_USER_PASSWORD,
--              SP_LOCK_UNLOCK_USER, SP_CREATE_ROLE, SP_DROP_ROLE,
--              SP_GRANT_SYS_PRIV, SP_GRANT_OBJ_PRIV, SP_GRANT_ROLE,
--              SP_REVOKE_OBJ_PRIV, SP_REVOKE_SYS_PRIV, SP_REVOKE_ROLE
-- FUNCTION   : FN_DEM_BENHNHAN, FN_TEN_BENHNHAN 
--              FN_LIST_USERS, FN_LIST_ROLES, FN_LIST_OBJECTS,
--              FN_LIST_COLUMNS, FN_GET_OBJ_PRIVS, FN_GET_SYS_PRIVS,
--              FN_GET_ROLE_PRIVS
-- ============================================================