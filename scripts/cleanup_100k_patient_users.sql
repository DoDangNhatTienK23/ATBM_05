-- ============================================================
-- TEST 100K PATIENT USERS - CLEANUP
-- Drops generated Oracle users and deletes only generated patient rows.
--
-- Safe delete marker:
--   MABN = BN000001..BN100000
--   ORACLE_USERNAME = same username
--   CCCD = 9 + 11-digit sequence
--   TENBN starts with [TEST100K]
--
-- Run as: BVDBA, SYS, SYSTEM, or an admin user in the correct PDB
-- ============================================================

SET SERVEROUTPUT ON SIZE UNLIMITED;

DECLARE
    c_total         CONSTANT PLS_INTEGER := 100000; -- Match generate script
    c_start_no      CONSTANT PLS_INTEGER := 1;
    c_batch_size    CONSTANT PLS_INTEGER := 1000;
    c_progress_step CONSTANT PLS_INTEGER := 5000;

    v_username      VARCHAR2(30);
    v_cccd          VARCHAR2(12);
    v_dropped_users NUMBER := 0;
    v_missing_users NUMBER := 0;
    v_deleted_rows  NUMBER := 0;
    v_errors        NUMBER := 0;

    PROCEDURE log_msg(p_msg VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE(TO_CHAR(SYSTIMESTAMP, 'HH24:MI:SS') || ' - ' || p_msg);
    END;
BEGIN
    log_msg('Start cleanup_100k_patient_users.sql');
    log_msg('Configured c_total = ' || c_total || ', start = ' || c_start_no);

    FOR i IN c_start_no .. (c_start_no + c_total - 1) LOOP
        v_username := 'BN' || LPAD(i, 6, '0');
        v_cccd := '9' || LPAD(i, 11, '0');

        BEGIN
            EXECUTE IMMEDIATE 'DROP USER ' || v_username || ' CASCADE';
            v_dropped_users := v_dropped_users + 1;
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE = -1918 THEN
                    v_missing_users := v_missing_users + 1;
                ELSE
                    v_errors := v_errors + 1;
                    log_msg('Drop user failed for ' || v_username || ': ' || SQLERRM);
                END IF;
        END;

        BEGIN
            DELETE FROM BVDBA.BENHNHAN
            WHERE MABN = v_username
              AND ORACLE_USERNAME = v_username
              AND CCCD = v_cccd
              AND TENBN LIKE N'[TEST100K]%';

            v_deleted_rows := v_deleted_rows + SQL%ROWCOUNT;
        EXCEPTION
            WHEN OTHERS THEN
                v_errors := v_errors + 1;
                log_msg('Delete patient failed for ' || v_username || ': ' || SQLERRM);
        END;

        IF MOD(i - c_start_no + 1, c_batch_size) = 0 THEN
            COMMIT;
        END IF;

        IF MOD(i - c_start_no + 1, c_progress_step) = 0 THEN
            log_msg('Processed cleanup ' || (i - c_start_no + 1) || '/' || c_total ||
                    ', dropped users=' || v_dropped_users ||
                    ', deleted rows=' || v_deleted_rows ||
                    ', errors=' || v_errors);
        END IF;
    END LOOP;

    COMMIT;

    log_msg('Cleanup done.');
    log_msg('Users dropped : ' || v_dropped_users);
    log_msg('Users missing : ' || v_missing_users);
    log_msg('Rows deleted  : ' || v_deleted_rows);
    log_msg('Errors        : ' || v_errors);
END;
/

PROMPT
PROMPT Suggested next step:
PROMPT   Run scripts/check_100k_patient_users.sql to confirm counts are 0.

