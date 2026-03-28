-- ============================================================
-- FILE: phan_he_1_clean.sql
-- MÔN HỌC: CSC12001 - An toàn và Bảo mật Dữ liệu trong HTTT
-- MỤC ĐÍCH: Chỉ phục vụ PHÂN HỆ 1 - Ứng dụng Quản trị CSDL Oracle
--
-- NỘI DUNG:
--   SECTION 0: Bootstrap - SYS tạo BVDBA (DBA của hệ thống)
--   SECTION 1: BVDBA tạo các bảng mẫu (đối tượng để demo cấp quyền)
--   SECTION 2: BVDBA tạo user/role mẫu (đối tượng để demo cấp quyền)
--   SECTION 3: BVDBA tạo view/procedure/function mẫu (đủ loại đối tượng)
--   SECTION 4: Các procedure/function phục vụ giao diện WinForm Phân hệ 1
--   SECTION 5: Kiểm tra nhanh
--
-- THỰC THI: Chạy theo thứ tự từ trên xuống.
-- ============================================================


-- ============================================================
-- SECTION 0: BOOTSTRAP
-- Đăng nhập: SYS AS SYSDBA, Service name = XEPDB1
-- Mục đích: Tạo tài khoản BVDBA - DBA của hệ thống bệnh viện.
--            Đây là tài khoản duy nhất mà Phân hệ 1 (WinForm) dùng
--            để kết nối và thực hiện toàn bộ thao tác quản trị.
-- Phục vụ: Tất cả 5 yêu cầu của Phân hệ 1
-- ============================================================

-- Đảm bảo đang làm việc trong đúng PDB
ALTER SESSION SET CONTAINER = XEPDB1;

-- Tạo tablespace riêng cho hệ thống bệnh viện
CREATE TABLESPACE BENHVIEN_TBS
    DATAFILE 'benhvien01.dbf' SIZE 100M
    AUTOEXTEND ON NEXT 50M MAXSIZE 1G
    EXTENT MANAGEMENT LOCAL SEGMENT SPACE MANAGEMENT AUTO;

-- Tạo BVDBA - schema owner và DBA của hệ thống
CREATE USER BVDBA
    IDENTIFIED BY "BvDba#2026"
    DEFAULT TABLESPACE BENHVIEN_TBS
    QUOTA UNLIMITED ON BENHVIEN_TBS
    PROFILE DEFAULT;

-- Cấp DBA role để BVDBA có đủ quyền quản trị
GRANT DBA TO BVDBA;

-- Cấp thêm các quyền cần thiết để BVDBA quản lý user/role/privilege
-- (Phân hệ 1 yêu cầu: tạo/xóa/sửa user, role, cấp/thu hồi quyền)
GRANT CREATE USER             TO BVDBA;
GRANT ALTER USER              TO BVDBA;
GRANT DROP USER               TO BVDBA;
GRANT CREATE ROLE             TO BVDBA;
GRANT DROP ANY ROLE           TO BVDBA;
GRANT GRANT ANY ROLE          TO BVDBA;
GRANT GRANT ANY PRIVILEGE     TO BVDBA;
GRANT GRANT ANY OBJECT PRIVILEGE TO BVDBA;

-- Cấp quyền đọc data dictionary để giao diện hiển thị user/role/privilege
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
-- SECTION 1: TẠO CÁC BẢNG MẪU
-- Đăng nhập: BVDBA, Service name = XEPDB1
-- Mục đích: Tạo các bảng đại diện cho schema bệnh viện.
--            Đây là các ĐỐI TƯỢNG để Phân hệ 1 demo:
--            cấp quyền SELECT/INSERT/UPDATE/DELETE/EXECUTE trên đó,
--            bao gồm cấp quyền đến mức cột (SELECT/UPDATE theo cột).
-- Phục vụ: Yêu cầu 3 (cấp quyền đối tượng), Yêu cầu 5 (xem quyền)
-- ============================================================

-- Bảng NHÂNVIÊN - dùng để demo cấp quyền SELECT cột, UPDATE cột
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

-- Bảng BỆNHNHÂN - dùng để demo cấp quyền INSERT (không theo cột)
CREATE TABLE BENHNHAN (
    MABN        VARCHAR2(20)   NOT NULL,
    TENBN       NVARCHAR2(100) NOT NULL,
    PHAI        CHAR(1)        CHECK (PHAI IN ('M','F')),
    NGAYSINH    DATE,
    CCCD        VARCHAR2(12),
    DIACHI      NVARCHAR2(300),
    CONSTRAINT PK_BENHNHAN PRIMARY KEY (MABN)
);

-- Bảng HSBA - dùng để demo cấp UPDATE theo cột cụ thể
CREATE TABLE HSBA (
    MAHSBA      VARCHAR2(20)   NOT NULL,
    MABN        VARCHAR2(20),
    NGAY        DATE,
    CHANDOAN    NVARCHAR2(500),
    DIEUTRI     NVARCHAR2(500),
    KETLUAN     NVARCHAR2(500),
    CONSTRAINT PK_HSBA PRIMARY KEY (MAHSBA)
);

-- Bảng DONTHUOC - dùng để demo DELETE (không theo cột)
CREATE TABLE DONTHUOC (
    MAHSBA      VARCHAR2(20)   NOT NULL,
    TENTHUOC    NVARCHAR2(200) NOT NULL,
    LIEUDUNG    NVARCHAR2(500),
    NGAYDT      DATE,
    CONSTRAINT PK_DONTHUOC PRIMARY KEY (MAHSBA, TENTHUOC)
);

-- Dữ liệu mẫu tối thiểu để giao diện không trống
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
-- SECTION 2: TẠO USER VÀ ROLE MẪU
-- Đăng nhập: BVDBA, Service name = XEPDB1
-- Mục đích: Tạo sẵn một số user và role mẫu để Phân hệ 1 demo
--            các thao tác: xem danh sách, cấp quyền, thu hồi quyền,
--            xem thông tin quyền.
-- Phục vụ: Yêu cầu 1, 2, 3, 4, 5 của Phân hệ 1
-- ============================================================

-- --- Role mẫu đại diện các nhóm trong bệnh viện ---
-- Phân hệ 1 sẽ demo: tạo role, cấp quyền cho role, cấp role cho user

CREATE ROLE ROLE_BACSI;         -- đại diện nhóm Bác sĩ/Y sĩ
CREATE ROLE ROLE_DIEUPHOIVIEN;  -- đại diện nhóm Điều phối viên
CREATE ROLE ROLE_KYTHUATVIEN;   -- đại diện nhóm Kỹ thuật viên
CREATE ROLE ROLE_BENHNHAN;      -- đại diện nhóm Bệnh nhân

-- --- User mẫu đại diện từng vai trò ---
-- Đặt password đơn giản để dễ demo; trong thực tế phải theo chính sách mật khẩu

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

-- Tất cả user cần CREATE SESSION để đăng nhập
GRANT CREATE SESSION TO U_BACSI01, U_BACSI02, U_DPV01, U_KTV01, U_BN01;

-- Gán role cho user (demo cấp role cho user - Yêu cầu 3a)
GRANT ROLE_BACSI        TO U_BACSI01, U_BACSI02;
GRANT ROLE_DIEUPHOIVIEN TO U_DPV01;
GRANT ROLE_KYTHUATVIEN  TO U_KTV01;
GRANT ROLE_BENHNHAN     TO U_BN01;

COMMIT;


-- ============================================================
-- SECTION 3: TẠO VIEW / PROCEDURE / FUNCTION MẪU
-- Đăng nhập: BVDBA, Service name = XEPDB1
-- Mục đích: Tạo đủ 4 loại đối tượng mà Phân hệ 1 yêu cầu hỗ trợ
--            cấp quyền: TABLE (đã có), VIEW, STORED PROCEDURE, FUNCTION.
--            Lưu ý: quyền trên mỗi loại đối tượng khác nhau.
-- Phục vụ: Yêu cầu 3c (cấp quyền trên table/view/proc/function)
-- ============================================================

-- --- VIEW mẫu ---
-- Quyền trên view: SELECT, INSERT, UPDATE, DELETE (tương tự table)
-- SELECT/UPDATE có thể phân quyền đến cột
CREATE OR REPLACE VIEW V_BENHNHAN_BASIC AS
    SELECT MABN, TENBN, PHAI, NGAYSINH
    FROM   BENHNHAN;

CREATE OR REPLACE VIEW V_HSBA_SUMMARY AS
    SELECT H.MAHSBA, H.MABN, B.TENBN, H.NGAY, H.CHANDOAN, H.KETLUAN
    FROM   HSBA H JOIN BENHNHAN B ON H.MABN = B.MABN;

-- --- STORED PROCEDURE mẫu ---
-- Quyền trên procedure: chỉ có EXECUTE (không phân cấp theo cột)
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

-- --- FUNCTION mẫu ---
-- Quyền trên function: chỉ có EXECUTE (không phân cấp theo cột)
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
-- SECTION 4: PROCEDURE/FUNCTION PHỤC VỤ GIAO DIỆN PHÂN HỆ 1
-- Đăng nhập: BVDBA, Service name = XEPDB1
-- Mục đích: WinForm Phân hệ 1 gọi các procedure/function này
--            để thực hiện 5 yêu cầu quản trị mà không viết DDL
--            trực tiếp trong C# (an toàn hơn, tránh SQL injection).
-- ============================================================

-- -------------------------------------------------------
-- YÊU CẦU 1: Tạo / Xóa / Sửa USER hoặc ROLE
-- -------------------------------------------------------

-- Tạo user mới
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

-- Xóa user
CREATE OR REPLACE PROCEDURE SP_DROP_USER (
    p_username IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'DROP USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) || ' CASCADE';
END SP_DROP_USER;
/

-- Đổi mật khẩu (sửa user)
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

-- Khóa / Mở khóa tài khoản (sửa user)
-- p_action: 'LOCK' hoặc 'UNLOCK'
CREATE OR REPLACE PROCEDURE SP_LOCK_UNLOCK_USER (
    p_username IN VARCHAR2,
    p_action   IN VARCHAR2  -- 'LOCK' | 'UNLOCK'
) AS
BEGIN
    IF UPPER(p_action) NOT IN ('LOCK','UNLOCK') THEN
        RAISE_APPLICATION_ERROR(-20001, 'p_action phải là LOCK hoặc UNLOCK');
    END IF;
    EXECUTE IMMEDIATE
        'ALTER USER ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_username) ||
        ' ACCOUNT '   || UPPER(p_action);
END SP_LOCK_UNLOCK_USER;
/

-- Tạo role
CREATE OR REPLACE PROCEDURE SP_CREATE_ROLE (
    p_rolename IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'CREATE ROLE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_rolename);
END SP_CREATE_ROLE;
/

-- Xóa role
CREATE OR REPLACE PROCEDURE SP_DROP_ROLE (
    p_rolename IN VARCHAR2
) AS
BEGIN
    EXECUTE IMMEDIATE
        'DROP ROLE ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_rolename);
END SP_DROP_ROLE;
/


-- -------------------------------------------------------
-- YÊU CẦU 2: Xem danh sách USER và ROLE
-- -------------------------------------------------------

-- Danh sách user: WinForm gọi SELECT * FROM TABLE(FN_LIST_USERS)
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
        WHERE  USERNAME NOT IN (  -- loại bỏ user hệ thống Oracle
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

-- Danh sách role: WinForm gọi SELECT * FROM TABLE(FN_LIST_ROLES)
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
        WHERE  ROLE NOT IN (  -- loại bỏ role hệ thống Oracle
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

-- Danh sách object có thể cấp quyền (table/view/proc/function của BVDBA)
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

-- Danh sách cột của một bảng/view (dùng khi cấp quyền SELECT/UPDATE theo cột)
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
-- YÊU CẦU 3: Cấp quyền
--   3a. Cấp quyền cho user, cấp quyền cho role, cấp role cho user
--   3b. Có/không WITH GRANT OPTION
--   3c. Phân quyền đến mức cột với SELECT/UPDATE;
--       INSERT/DELETE không theo cột
-- -------------------------------------------------------

-- Cấp quyền hệ thống (system privilege) cho user/role
-- p_with_admin_opt: 'YES' = WITH ADMIN OPTION | 'NO' = không
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

-- Cấp quyền đối tượng (object privilege) cho user/role
-- Hỗ trợ: TABLE, VIEW  -> SELECT, INSERT, UPDATE, DELETE
--          PROCEDURE, FUNCTION -> EXECUTE
-- p_columns: danh sách cột cách nhau bởi dấu phẩy, VD: 'HOTEN,SODT'
--            Chỉ áp dụng khi p_privilege là SELECT hoặc UPDATE
--            NULL = cấp trên toàn bảng/view
-- p_with_grant_opt: 'YES' = WITH GRANT OPTION | 'NO' = không
CREATE OR REPLACE PROCEDURE SP_GRANT_OBJ_PRIV (
    p_privilege      IN VARCHAR2,
    p_object_owner   IN VARCHAR2,
    p_object_name    IN VARCHAR2,
    p_grantee        IN VARCHAR2,
    p_columns        IN VARCHAR2 DEFAULT NULL,
    p_with_grant_opt IN VARCHAR2 DEFAULT 'NO'
) AS
    v_sql    VARCHAR2(2000);
    v_object VARCHAR2(300);
    v_priv   VARCHAR2(20);
BEGIN
    v_priv   := UPPER(TRIM(p_privilege));
    v_object := DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_owner) || '.' ||
                DBMS_ASSERT.SIMPLE_SQL_NAME(p_object_name);

    -- Validate: INSERT và DELETE không được cấp theo cột
    IF p_columns IS NOT NULL AND v_priv IN ('INSERT','DELETE','EXECUTE') THEN
        RAISE_APPLICATION_ERROR(-20002,
            v_priv || ' không hỗ trợ phân quyền theo cột');
    END IF;

    -- Xây dựng câu lệnh GRANT
    IF p_columns IS NOT NULL AND v_priv IN ('SELECT','UPDATE') THEN
        -- Cấp theo cột cụ thể (p_columns đã được validate whitelist ở tầng C#)
        v_sql := 'GRANT ' || v_priv ||
                 ' (' || p_columns || ')' ||
                 ' ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    ELSE
        -- Cấp trên toàn đối tượng
        v_sql := 'GRANT ' || v_priv ||
                 ' ON ' || v_object ||
                 ' TO ' || DBMS_ASSERT.SIMPLE_SQL_NAME(p_grantee);
    END IF;

    IF UPPER(p_with_grant_opt) = 'YES' THEN
        v_sql := v_sql || ' WITH GRANT OPTION';
    END IF;

    EXECUTE IMMEDIATE v_sql;
END SP_GRANT_OBJ_PRIV;
/

-- Cấp role cho user
-- p_with_admin_opt: 'YES' = WITH ADMIN OPTION | 'NO' = không
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


-- -------------------------------------------------------
-- YÊU CẦU 4: Thu hồi quyền từ user hoặc role
-- -------------------------------------------------------

-- Thu hồi quyền đối tượng
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

-- Thu hồi quyền hệ thống
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

-- Thu hồi role từ user
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
-- YÊU CẦU 5: Xem thông tin quyền của user hoặc role
-- -------------------------------------------------------

-- Xem quyền đối tượng (bao gồm quyền cấp theo cột)
-- WinForm gọi: SELECT * FROM TABLE(FN_GET_OBJ_PRIVS('U_BACSI01'))
CREATE OR REPLACE TYPE T_OBJPRIV_ROW AS OBJECT (
    GRANTEE     VARCHAR2(128),
    OWNER       VARCHAR2(128),
    OBJECT_NAME VARCHAR2(128),
    OBJECT_TYPE VARCHAR2(23),
    PRIVILEGE   VARCHAR2(40),
    GRANTABLE   VARCHAR2(3),
    COLUMN_NAME VARCHAR2(128)   -- NULL nếu quyền cấp trên toàn đối tượng
);
/
CREATE OR REPLACE TYPE T_OBJPRIV_TABLE AS TABLE OF T_OBJPRIV_ROW;
/

CREATE OR REPLACE FUNCTION FN_GET_OBJ_PRIVS (
    p_grantee IN VARCHAR2
) RETURN T_OBJPRIV_TABLE PIPELINED AS
BEGIN
    -- Quyền cấp trên toàn đối tượng (table/view/proc/function)
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

    -- Quyền cấp theo cột (SELECT/UPDATE cột cụ thể)
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

-- Xem quyền hệ thống của user/role
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

-- Xem role được cấp cho user/role
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
-- SECTION 5: KIỂM TRA NHANH
-- Đăng nhập: BVDBA
-- ============================================================

-- Kiểm tra bảng đã tạo
SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
FROM   USER_OBJECTS
WHERE  OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION')
ORDER  BY OBJECT_TYPE, OBJECT_NAME;

-- Kiểm tra user mẫu
SELECT * FROM TABLE(FN_LIST_USERS);

-- Kiểm tra role mẫu
SELECT * FROM TABLE(FN_LIST_ROLES);

-- Kiểm tra quyền của U_BACSI01 (sau khi gán ROLE_BACSI)
SELECT * FROM TABLE(FN_GET_ROLE_PRIVS('U_BACSI01'));

-- Kiểm tra dữ liệu mẫu
SELECT COUNT(*) AS SO_NV   FROM NHANVIEN;
SELECT COUNT(*) AS SO_BN   FROM BENHNHAN;
SELECT COUNT(*) AS SO_HSBA FROM HSBA;

-- ============================================================
-- TÓM TẮT ĐỐI TƯỢNG ĐÃ TẠO
-- ============================================================
-- TABLESPACE : BENHVIEN_TBS
-- USER (DBA) : BVDBA
-- USER (mẫu) : U_BACSI01, U_BACSI02, U_DPV01, U_KTV01, U_BN01
-- ROLE (mẫu) : ROLE_BACSI, ROLE_DIEUPHOIVIEN, ROLE_KYTHUATVIEN, ROLE_BENHNHAN
-- TABLE      : NHANVIEN, BENHNHAN, HSBA, DONTHUOC
-- VIEW       : V_BENHNHAN_BASIC, V_HSBA_SUMMARY
-- PROCEDURE  : SP_THEM_BENHNHAN, SP_CAP_NHAT_HSBA (đối tượng demo)
--              SP_CREATE_USER, SP_DROP_USER, SP_ALTER_USER_PASSWORD,
--              SP_LOCK_UNLOCK_USER, SP_CREATE_ROLE, SP_DROP_ROLE,
--              SP_GRANT_SYS_PRIV, SP_GRANT_OBJ_PRIV, SP_GRANT_ROLE,
--              SP_REVOKE_OBJ_PRIV, SP_REVOKE_SYS_PRIV, SP_REVOKE_ROLE
-- FUNCTION   : FN_DEM_BENHNHAN, FN_TEN_BENHNHAN (đối tượng demo)
--              FN_LIST_USERS, FN_LIST_ROLES, FN_LIST_OBJECTS,
--              FN_LIST_COLUMNS, FN_GET_OBJ_PRIVS, FN_GET_SYS_PRIVS,
--              FN_GET_ROLE_PRIVS
-- ============================================================