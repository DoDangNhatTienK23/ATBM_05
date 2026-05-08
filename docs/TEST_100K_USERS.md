# Kiểm thử tạo tự động 100000 user bệnh nhân

## Mục tiêu

Bộ script này dùng để chứng minh hệ thống có thể tự động sinh dữ liệu bệnh nhân và tạo Oracle users tương ứng theo yêu cầu đề: khoảng 100000 người dùng là bệnh nhân.

Source hiện tại đã có cơ chế mapping bệnh nhân bằng cột `BENHNHAN.ORACLE_USERNAME`, role bệnh nhân `RL_BENHNHAN`, và view tự xem thông tin cá nhân `BVDBA.VW_BN_THONGTIN_CANHAN`. Vì vậy script tích hợp trực tiếp với cơ chế RBAC hiện có, không sửa cấu trúc source code.

## File được thêm

- `scripts/generate_100k_patient_users.sql`: sinh bệnh nhân mẫu, tạo Oracle users, cấp role bệnh nhân.
- `scripts/check_100k_patient_users.sql`: kiểm tra số lượng user, số dòng bệnh nhân, role, mapping.
- `scripts/cleanup_100k_patient_users.sql`: drop user mẫu và xóa bệnh nhân mẫu an toàn.

## Cảnh báo trước khi chạy

Tạo thật 100000 Oracle users là thao tác nặng. SQL Developer có thể chậm, Oracle có thể mất nhiều thời gian để ghi dictionary, và máy yếu có thể bị treo lâu.

Nên chạy thử trước:

```sql
c_total CONSTANT PLS_INTEGER := 100;
```

hoặc:

```sql
c_total CONSTANT PLS_INTEGER := 1000;
```

Sau khi kiểm tra ổn mới đổi về:

```sql
c_total CONSTANT PLS_INTEGER := 100000;
```

## Tài khoản chạy script

Chạy trong đúng PDB đang dùng cho đồ án, ví dụ `XEPDB1`.

Tài khoản khuyến nghị:

- `BVDBA / BvDba#2026`, nếu đã chạy setup và user này có đủ quyền.
- Hoặc `SYS AS SYSDBA`.
- Hoặc `SYSTEM`, nếu có đủ quyền `CREATE USER`, `DROP USER`, `GRANT ANY ROLE`, đọc `DBA_USERS`, `DBA_ROLES`, `DBA_ROLE_PRIVS`.

Nếu dùng Oracle XE/PDB, kiểm tra container:

```sql
SHOW CON_NAME;

ALTER SESSION SET CONTAINER = XEPDB1;
```

## Thứ tự chạy bằng SQL Developer

1. Mở SQL Developer.
2. Kết nối vào PDB đúng, ví dụ `XEPDB1`.
3. Bật output:

```sql
SET SERVEROUTPUT ON SIZE UNLIMITED;
```

4. Đảm bảo đã chạy script setup chính trước, đặc biệt là các phần tạo:

- `BVDBA.BENHNHAN`
- `BENHNHAN.ORACLE_USERNAME`
- `RL_BENHNHAN`
- `BVDBA.VW_BN_THONGTIN_CANHAN`

5. Chạy thử tạo số lượng nhỏ:

Mở `scripts/generate_100k_patient_users.sql`, đổi:

```sql
c_total CONSTANT PLS_INTEGER := 100;
```

Sau đó chạy file.

6. Kiểm tra:

Chạy `scripts/check_100k_patient_users.sql`.

7. Nếu ổn, tăng số lượng lên `1000` hoặc `100000`, rồi chạy lại generate.

8. Khi cần dọn dữ liệu test:

Chạy `scripts/cleanup_100k_patient_users.sql`.

Sau cleanup, chạy lại `scripts/check_100k_patient_users.sql` để xác nhận số lượng đã về 0.

## Quy ước dữ liệu sinh ra

Username Oracle:

```text
BN000001, BN000002, ..., BN100000
```

Password mặc định:

```text
Bn@123456
```

Dòng bệnh nhân tương ứng:

- `MABN = username`, ví dụ `BN000001`.
- `ORACLE_USERNAME = username`.
- `TENBN` bắt đầu bằng marker `[TEST100K]`.
- `CCCD = 9` + số thứ tự 11 chữ số.

Cleanup chỉ xóa dòng thỏa đủ marker trên, nên không xóa nhầm bệnh nhân thật.

## Kiểm tra cô lập dữ liệu bệnh nhân

Sau khi generate, mở connection mới bằng:

```text
Username: BN000001
Password: Bn@123456
```

Chạy:

```sql
SELECT USER AS CURRENT_USER FROM DUAL;

SELECT MABN, TENBN, ORACLE_USERNAME
FROM BVDBA.VW_BN_THONGTIN_CANHAN;

SELECT COUNT(*) AS OTHER_ROWS
FROM BVDBA.VW_BN_THONGTIN_CANHAN
WHERE ORACLE_USERNAME <> USER;
```

Kết quả mong đợi:

- Chỉ thấy một dòng của `BN000001`.
- `OTHER_ROWS = 0`.

Mở connection khác:

```text
Username: BN000002
Password: Bn@123456
```

Chạy cùng câu SQL. Kết quả mong đợi là chỉ thấy dòng của `BN000002`.

Nếu thử truy cập trực tiếp bảng:

```sql
SELECT MABN, TENBN
FROM BVDBA.BENHNHAN
WHERE MABN = 'BN000002';
```

Thông thường patient user sẽ bị từ chối vì role bệnh nhân hiện tại chỉ được cấp quyền trên view `VW_BN_THONGTIN_CANHAN`, không cấp trực tiếp bảng `BENHNHAN`.

## Lỗi thường gặp

### ORA-01031: insufficient privileges

Tài khoản chạy script thiếu quyền tạo user, drop user, grant role hoặc đọc dictionary.

Cách xử lý:

- Chạy bằng `BVDBA` sau khi đã cấp quyền trong `hospital_setup.sql`.
- Hoặc chạy bằng `SYS AS SYSDBA`.
- Kiểm tra quyền `CREATE USER`, `DROP USER`, `GRANT ANY ROLE`, `SELECT_CATALOG_ROLE`.

### ORA-01920: user name conflicts with another user or role name

Tên user đã tồn tại hoặc trùng role.

Cách xử lý:

- Nếu muốn dùng lại user cũ, để `c_recreate_users := FALSE`.
- Nếu muốn drop tạo lại, đổi trong generate:

```sql
c_recreate_users CONSTANT BOOLEAN := TRUE;
```

- Hoặc chạy `cleanup_100k_patient_users.sql` trước.

### User already exists

Script generate mặc định bỏ qua user đã tồn tại và vẫn kiểm tra/cập nhật mapping bệnh nhân nếu là dữ liệu test.

Nếu muốn tạo sạch từ đầu, chạy cleanup trước hoặc bật `c_recreate_users`.

### Tablespace/quota

Script tạo user với:

```sql
DEFAULT TABLESPACE USERS
QUOTA 0 ON USERS
```

Vì bệnh nhân không tạo object riêng nên quota 0 là đủ. Nếu database không có tablespace `USERS`, đổi biến:

```sql
c_default_tablespace CONSTANT VARCHAR2(30) := 'TEN_TABLESPACE_CUA_BAN';
```

### Password verify function

Nếu profile Oracle có password verify function nghiêm ngặt, password `Bn@123456` có thể không đạt.

Cách xử lý:

- Đổi biến `c_password` trong generate.
- Hoặc chỉnh profile/password verify policy theo môi trường demo.

### SQL Developer bị chậm

Tạo 100000 user có rất nhiều DDL nên SQL Developer có thể chậm hoặc tưởng như đứng.

Cách xử lý:

- Chạy thử `100` hoặc `1000` trước.
- Mở DBMS Output để xem tiến độ mỗi 5000 user.
- Có thể chạy bằng SQL*Plus hoặc SQLcl nếu SQL Developer quá chậm.

### ORA-65096 khi đang dùng CDB/PDB

Bạn đang tạo local user ở `CDB$ROOT`.

Cách xử lý:

```sql
SHOW CON_NAME;
ALTER SESSION SET CONTAINER = XEPDB1;
```

Sau đó chạy lại script.

### ORA-01918: user does not exist khi cleanup

Không sao. Cleanup đã bắt exception này và bỏ qua user không tồn tại.

### Không thấy dữ liệu khi đăng nhập bằng BN000001

Kiểm tra:

```sql
SELECT *
FROM BVDBA.BENHNHAN
WHERE ORACLE_USERNAME = 'BN000001';

SELECT *
FROM DBA_ROLE_PRIVS
WHERE GRANTEE = 'BN000001';
```

Nếu chưa có role `RL_BENHNHAN`, hãy chạy phần RBAC trong `hospital_setup.sql` trước, hoặc dùng fallback role `ROLE_BENHNHAN_DEMO` do script generate tạo.

