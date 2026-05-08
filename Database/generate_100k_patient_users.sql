-- ============================================================
-- TEST 100K PATIENT USERS - GENERATE
-- Project: Oracle Hospital Security
-- Run as: BVDBA, SYS, SYSTEM, or an admin user in the correct PDB
-- Required privileges: CREATE USER, GRANT role, SELECT on DBA_USERS/DBA_ROLES,
--                      INSERT/UPDATE on BVDBA.BENHNHAN
--
-- WARNING:
--   Creating 100000 real Oracle users is heavy and may take a long time.
--   Try c_total := 100 or 1000 first. Increase to 100000 only after the
--   small test works.
--
-- Current source mapping:
--   Table       : BVDBA.BENHNHAN
--   Link column : ORACLE_USERNAME
--   Patient role: RL_BENHNHAN
--   Self-view   : BVDBA.VW_BN_THONGTIN_CANHAN
-- ============================================================

SET SERVEROUTPUT ON SIZE UNLIMITED;

DECLARE
    c_schema             CONSTANT VARCHAR2(30) := 'BVDBA';
    c_total              CONSTANT PLS_INTEGER  := 100; -- Change to 100/1000 for demo
    c_start_no           CONSTANT PLS_INTEGER  := 1;
    c_password           CONSTANT VARCHAR2(50) := 'Bn@123456';
    c_default_tablespace CONSTANT VARCHAR2(30) := 'USERS';
    c_batch_size         CONSTANT PLS_INTEGER  := 1000;
    c_progress_step      CONSTANT PLS_INTEGER  := 5000;
    c_recreate_users     CONSTANT BOOLEAN      := FALSE;  -- TRUE = drop then create again

    v_role_name          VARCHAR2(30);
    v_username           VARCHAR2(30);
    v_mabn               VARCHAR2(10);
    v_cccd               VARCHAR2(12);
    v_tenbn              NVARCHAR2(100);
    v_exists             NUMBER;
    v_has_table          NUMBER;
    v_has_mapping_col    NUMBER;
    v_has_role           NUMBER;
    v_has_view           NUMBER;
    v_created_users      NUMBER := 0;
    v_existing_users     NUMBER := 0;
    v_granted_roles      NUMBER := 0;
    v_inserted_patients  NUMBER := 0;
    v_updated_patients   NUMBER := 0;
    v_skipped_patients   NUMBER := 0;
    v_errors             NUMBER := 0;

    PROCEDURE log_msg(p_msg VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE(TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') || ' - ' || p_msg);
    END;

BEGIN
    log_msg('Start generate_100k_patient_users.sql');
    log_msg('Configured c_total = ' || c_total || ', start = ' || c_start_no);

    SELECT COUNT(*)
    INTO v_has_table
    FROM ALL_TABLES
    WHERE OWNER = c_schema
      AND TABLE_NAME = 'BENHNHAN';

    IF v_has_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20901, 'Table ' || c_schema || '.BENHNHAN not found.');
    END IF;

    SELECT COUNT(*)
    INTO v_has_mapping_col
    FROM ALL_TAB_COLUMNS
    WHERE OWNER = c_schema
      AND TABLE_NAME = 'BENHNHAN'
      AND COLUMN_NAME = 'ORACLE_USERNAME';

    IF v_has_mapping_col = 0 THEN
        RAISE_APPLICATION_ERROR(
            -20902,
            'Column ORACLE_USERNAME not found in ' || c_schema || '.BENHNHAN. ' ||
            'Minimal change if needed: ALTER TABLE BENHNHAN ADD ORACLE_USERNAME VARCHAR2(30);'
        );
    END IF;

    SELECT COUNT(*)
    INTO v_has_role
    FROM DBA_ROLES
    WHERE ROLE = 'RL_BENHNHAN';

    IF v_has_role > 0 THEN
        v_role_name := 'RL_BENHNHAN';
        log_msg('Using existing patient role RL_BENHNHAN.');
    ELSE
        v_role_name := 'ROLE_BENHNHAN_DEMO';
        SELECT COUNT(*)
        INTO v_has_role
        FROM DBA_ROLES
        WHERE ROLE = v_role_name;

        IF v_has_role = 0 THEN
            EXECUTE IMMEDIATE 'CREATE ROLE ' || v_role_name;
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || v_role_name;
            log_msg('Created fallback role ROLE_BENHNHAN_DEMO.');
        ELSE
            log_msg('Using existing fallback role ROLE_BENHNHAN_DEMO.');
        END IF;

        SELECT COUNT(*)
        INTO v_has_view
        FROM ALL_OBJECTS
        WHERE OWNER = c_schema
          AND OBJECT_NAME = 'VW_BN_THONGTIN_CANHAN'
          AND OBJECT_TYPE = 'VIEW';

        IF v_has_view > 0 THEN
            BEGIN
                EXECUTE IMMEDIATE 'GRANT SELECT ON ' || c_schema || '.VW_BN_THONGTIN_CANHAN TO ' || v_role_name;
                EXECUTE IMMEDIATE 'GRANT UPDATE (SONHA, TENDUONG, QUANHUYEN, TINHTHANH, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) ON ' ||
                                  c_schema || '.VW_BN_THONGTIN_CANHAN TO ' || v_role_name;
                log_msg('Granted self-view privileges to fallback role.');
            EXCEPTION
                WHEN OTHERS THEN
                    log_msg('Warning: cannot grant self-view privileges to fallback role: ' || SQLERRM);
            END;
        ELSE
            log_msg('Warning: VW_BN_THONGTIN_CANHAN not found. Fallback role only has CREATE SESSION.');
        END IF;
    END IF;

    FOR i IN c_start_no .. (c_start_no + c_total - 1) LOOP
        v_username := 'BN' || LPAD(i, 6, '0');
        v_mabn := v_username;
        v_cccd := '9' || LPAD(i, 11, '0');
        v_tenbn := N'[TEST100K] Benh nhan ' || TO_NCHAR(LPAD(i, 6, '0'));

        BEGIN
            IF c_recreate_users THEN
                BEGIN
                    EXECUTE IMMEDIATE 'DROP USER ' || v_username || ' CASCADE';
                EXCEPTION
                    WHEN OTHERS THEN
                        IF SQLCODE NOT IN (-1918) THEN
                            log_msg('Warning: cannot drop ' || v_username || ': ' || SQLERRM);
                        END IF;
                END;
            END IF;

            SELECT COUNT(*)
            INTO v_exists
            FROM DBA_USERS
            WHERE USERNAME = v_username;

            IF v_exists = 0 THEN
                EXECUTE IMMEDIATE
                    'CREATE USER ' || v_username ||
                    ' IDENTIFIED BY "' || c_password || '"' ||
                    ' DEFAULT TABLESPACE ' || c_default_tablespace ||
                    ' TEMPORARY TABLESPACE TEMP QUOTA 0 ON ' || c_default_tablespace;
                v_created_users := v_created_users + 1;
            ELSE
                v_existing_users := v_existing_users + 1;
            END IF;

            BEGIN
                SELECT COUNT(*)
                INTO v_exists
                FROM DBA_ROLE_PRIVS
                WHERE GRANTEE = v_username
                  AND GRANTED_ROLE = v_role_name;

                IF v_exists = 0 THEN
                    EXECUTE IMMEDIATE 'GRANT ' || v_role_name || ' TO ' || v_username;
                    v_granted_roles := v_granted_roles + 1;
                END IF;
            EXCEPTION
                WHEN OTHERS THEN
                    v_errors := v_errors + 1;
                    log_msg('Grant role failed for ' || v_username || ': ' || SQLERRM);
            END;

            SELECT COUNT(*)
            INTO v_exists
            FROM BVDBA.BENHNHAN
            WHERE MABN = v_mabn
              AND NOT (
                    NVL(ORACLE_USERNAME, '#') = v_username
                    OR TENBN LIKE N'[TEST100K]%'
                    OR CCCD = v_cccd
              );

            IF v_exists > 0 THEN
                v_skipped_patients := v_skipped_patients + 1;
                log_msg('Skip patient row because MABN may be real data: ' || v_mabn);
            ELSE
                UPDATE BVDBA.BENHNHAN
                SET TENBN          = v_tenbn,
                    PHAI           = CASE WHEN MOD(i, 2) = 0 THEN N'Nam' ELSE N'Nu' END,
                    NGAYSINH       = ADD_MONTHS(DATE '1960-01-01', MOD(i, 720)) + MOD(i, 27),
                    CCCD           = v_cccd,
                    SONHA          = TO_NCHAR(TO_CHAR(MOD(i, 300) + 1)),
                    TENDUONG       = N'Duong TEST100K ' || TO_NCHAR(TO_CHAR(MOD(i, 50) + 1)),
                    QUANHUYEN      = N'Quan demo ' || TO_NCHAR(TO_CHAR(MOD(i, 20) + 1)),
                    TINHTHANH      = CASE MOD(i, 3)
                                        WHEN 0 THEN N'Ho Chi Minh'
                                        WHEN 1 THEN N'Ha Noi'
                                        ELSE N'Hai Phong'
                                      END,
                    TIENSUBENH     = CASE MOD(i, 5)
                                        WHEN 0 THEN N'Tang huyet ap'
                                        WHEN 1 THEN N'Viem da day'
                                        WHEN 2 THEN N'Dau nua dau'
                                        WHEN 3 THEN N'Dai thao duong type 2'
                                        ELSE NULL
                                      END,
                    TIENSUBENHGD   = CASE MOD(i, 4)
                                        WHEN 0 THEN N'Gia dinh co tien su tim mach'
                                        WHEN 1 THEN N'Gia dinh co tien su tieu duong'
                                        ELSE NULL
                                      END,
                    DIUNGTHUOC     = CASE MOD(i, 6)
                                        WHEN 0 THEN N'Penicillin'
                                        WHEN 1 THEN N'Aspirin'
                                        ELSE NULL
                                      END,
                    ORACLE_USERNAME = v_username
                WHERE MABN = v_mabn
                  AND (
                        NVL(ORACLE_USERNAME, '#') = v_username
                        OR TENBN LIKE N'[TEST100K]%'
                        OR CCCD = v_cccd
                      );

                IF SQL%ROWCOUNT > 0 THEN
                    v_updated_patients := v_updated_patients + 1;
                ELSE
                    INSERT INTO BVDBA.BENHNHAN (
                        MABN, TENBN, PHAI, NGAYSINH, CCCD,
                        SONHA, TENDUONG, QUANHUYEN, TINHTHANH,
                        TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC,
                        ORACLE_USERNAME
                    ) VALUES (
                        v_mabn,
                        v_tenbn,
                        CASE WHEN MOD(i, 2) = 0 THEN N'Nam' ELSE N'Nu' END,
                        ADD_MONTHS(DATE '1960-01-01', MOD(i, 720)) + MOD(i, 27),
                        v_cccd,
                        TO_NCHAR(TO_CHAR(MOD(i, 300) + 1)),
                        N'Duong TEST100K ' || TO_NCHAR(TO_CHAR(MOD(i, 50) + 1)),
                        N'Quan demo ' || TO_NCHAR(TO_CHAR(MOD(i, 20) + 1)),
                        CASE MOD(i, 3)
                            WHEN 0 THEN N'Ho Chi Minh'
                            WHEN 1 THEN N'Ha Noi'
                            ELSE N'Hai Phong'
                        END,
                        CASE MOD(i, 5)
                            WHEN 0 THEN N'Tang huyet ap'
                            WHEN 1 THEN N'Viem da day'
                            WHEN 2 THEN N'Dau nua dau'
                            WHEN 3 THEN N'Dai thao duong type 2'
                            ELSE NULL
                        END,
                        CASE MOD(i, 4)
                            WHEN 0 THEN N'Gia dinh co tien su tim mach'
                            WHEN 1 THEN N'Gia dinh co tien su tieu duong'
                            ELSE NULL
                        END,
                        CASE MOD(i, 6)
                            WHEN 0 THEN N'Penicillin'
                            WHEN 1 THEN N'Aspirin'
                            ELSE NULL
                        END,
                        v_username
                    );
                    v_inserted_patients := v_inserted_patients + 1;
                END IF;
            END IF;

            IF MOD(i - c_start_no + 1, c_batch_size) = 0 THEN
                COMMIT;
            END IF;

            IF MOD(i - c_start_no + 1, c_progress_step) = 0 THEN
                log_msg('Processed ' || (i - c_start_no + 1) || '/' || c_total ||
                        ', users created=' || v_created_users ||
                        ', patients inserted=' || v_inserted_patients ||
                        ', updated=' || v_updated_patients ||
                        ', errors=' || v_errors);
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                v_errors := v_errors + 1;
                log_msg('Error at ' || v_username || ': ' || SQLERRM);
                IF MOD(i - c_start_no + 1, c_batch_size) = 0 THEN
                    COMMIT;
                END IF;
        END;
    END LOOP;

    COMMIT;

    log_msg('Done.');
    log_msg('Users created      : ' || v_created_users);
    log_msg('Users already exist: ' || v_existing_users);
    log_msg('Role grants tried  : ' || v_granted_roles || ' using role ' || v_role_name);
    log_msg('Patients inserted  : ' || v_inserted_patients);
    log_msg('Patients updated   : ' || v_updated_patients);
    log_msg('Patients skipped   : ' || v_skipped_patients);
    log_msg('Errors             : ' || v_errors);
END;
/

PROMPT
PROMPT Suggested next step:
PROMPT   Run scripts/check_100k_patient_users.sql
