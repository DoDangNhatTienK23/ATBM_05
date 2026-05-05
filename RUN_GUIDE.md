# RUN GUIDE - Đồ án Oracle Security Hospital System

## 1. Tổng quan project

Project này là ứng dụng WinForms quản lý bảo mật CSDL Oracle cho hệ thống bệnh viện. Source hiện có 2 nhóm chức năng chính:

- Phân hệ 1: app DBA quản lý user, role, grant, revoke, xem quyền trong Oracle.
- Phân hệ 2: app theo vai trò bệnh viện gồm Điều phối viên, Bác sĩ/Y sĩ, Kỹ thuật viên, Bệnh nhân.
- Database dùng Oracle, schema chính là `BVDBA`.
- App dùng C# WinForms, `.NET Framework 4.7.2`, package `Oracle.ManagedDataAccess`.

Lưu ý quan trọng: source có phần OLS hiển thị thông báo trong `Forms/ThongBaoPanel.cs`, nhưng các user OLS mẫu `U1...U8` không được route qua `LoginForm` vì không có dòng mapping trong `VW_TC1_TOI_LA_AI`. Vì vậy OLS nên test chắc chắn bằng SQL Developer; phần UI thông báo có thể chưa demo được với `U1...U8` nếu không sửa source.

## 2. Yêu cầu môi trường

- Oracle Database XE hoặc Oracle Database có PDB. Source mặc định dùng service name `XEPDB1`.
- SQL Developer để chạy script.
- Windows, vì project là WinForms `.NET Framework`.
- Visual Studio 2022 Community hoặc Visual Studio Build Tools có MSBuild.
- .NET Framework 4.7.2 Developer Pack/Targeting Pack.
- VS Code nếu muốn mở source và chạy lệnh build từ terminal.
- Package đã khai báo trong `packages.config`, quan trọng nhất là `Oracle.ManagedDataAccess` version `23.26.100`.
- Không thấy script backup/recovery riêng trong source.

## 3. Cấu trúc thư mục project

```text
.
|-- OracleAdminApp.sln
|-- OracleAdminApp.csproj
|-- App.config
|-- packages.config
|-- Program.cs
|-- Forms/
|   |-- LoginForm.cs
|   |-- MainForm.cs
|   |-- UserManagementPanel.cs
|   |-- RoleManagementPanel.cs
|   |-- GrantPrivilegePanel.cs
|   |-- RevokeAndViewPanel.cs
|   |-- CoordinatorForm.cs
|   |-- DoctorForm.cs
|   |-- TechnicianForm.cs
|   |-- PatientForm.cs
|   |-- ThongBaoPanel.cs
|-- Helpers/
|   |-- UIHelper.cs
|-- Database/
|   |-- hospital_setup.sql
|   |-- OLS_SYS.sql
|   |-- OLS_BVDBA.sql
|   |-- TEST.sql
|-- Properties/
|-- packages/
```

- `OracleAdminApp.sln`: solution chính.
- `OracleAdminApp.csproj`: project WinForms chính, target `.NET Framework 4.7.2`.
- `App.config`: cấu hình runtime/provider Oracle Managed DataAccess, không chứa connection string app.
- `packages.config`: danh sách NuGet package kiểu project cũ.
- `Database/hospital_setup.sql`: script database chính, gồm tạo `BVDBA`, bảng, data, user nghiệp vụ, role, RBAC, VPD, thủ tục cho app DBA, audit.
- `Database/OLS_SYS.sql`: script OLS chạy bằng `SYS AS SYSDBA`.
- `Database/OLS_BVDBA.sql`: script OLS chạy bằng `BVDBA`.
- `Database/TEST.sql`: script test audit/FGA bằng nhiều connection.

## 4. Chuẩn bị Oracle Database

1. Mở Oracle service. Với Oracle XE thường cần service Oracle và listener đang chạy.
2. Mở SQL Developer.
3. Tạo connection quản trị:
   - Username: `SYS`
   - Role: `SYSDBA`
   - Host: `localhost`
   - Port: `1521`
   - Service name: `XEPDB1`
4. Nếu không dùng `SYS`, có thể dùng `SYSTEM` cho vài bước, nhưng `OLS_SYS.sql` cần `SYS AS SYSDBA`.
5. Kiểm tra container:

```sql
SHOW CON_NAME;

SELECT SYS_CONTEXT('USERENV', 'CON_NAME') AS CON_NAME
FROM dual;
```

Kết quả mong đợi là `XEPDB1`. Nếu đang ở `CDB$ROOT`, chạy:

```sql
ALTER SESSION SET CONTAINER = XEPDB1;
```

## 5. Thứ tự chạy script database

Không nên bấm Run toàn bộ mọi file cùng lúc. `hospital_setup.sql` có lẫn block chạy bằng `SYS` và block chạy bằng `BVDBA`; `OLS_SYS.sql` cũng có đoạn gán nhãn cho user nhưng user lại được tạo trong `OLS_BVDBA.sql`.

### Bước DB-01: Chạy `Database/hospital_setup.sql`, dòng 14-65

- Mục đích: tạo tablespace `BENHVIEN_TBS`, tạo user/schema `BVDBA`, cấp quyền DBA và quyền đọc data dictionary cho app quản trị.
- Đăng nhập bằng user: `SYS AS SYSDBA`.
- Chạy: chỉ dòng 14 đến dòng 65, tức `BLOCK A: SYS AS SYSDBA`.
- Kết quả mong đợi: user `BVDBA` tồn tại, password là `BvDba#2026`.
- SQL kiểm tra:

```sql
SELECT username, account_status, default_tablespace
FROM dba_users
WHERE username = 'BVDBA';
```

- Lỗi thường gặp:
  - `ORA-65096`: đang ở CDB root khi tạo local user. Chuyển về `XEPDB1`.
  - `ORA-01031`: user chạy script không đủ quyền. Dùng `SYS AS SYSDBA`.

### Bước DB-02: Chạy `Database/hospital_setup.sql`, dòng 94-531

- Mục đích: xóa bảng cũ nếu có, tạo bảng, index, insert dữ liệu mẫu.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 94 đến dòng 531.
- Kết quả mong đợi:
  - Bảng: `BENHNHAN`, `NHANVIEN`, `HSBA`, `HSBA_DV`, `DONTHUOC`, `THONGBAO`.
  - Data mẫu: `NHANVIEN` 35 dòng, `BENHNHAN` 20 dòng, `HSBA` 19 dòng, `HSBA_DV` 16 dòng, `DONTHUOC` 23 dòng, `THONGBAO` 7 dòng.
- SQL kiểm tra:

```sql
SELECT table_name FROM user_tables ORDER BY table_name;

SELECT 'NHANVIEN' bang, COUNT(*) so_dong FROM NHANVIEN UNION ALL
SELECT 'BENHNHAN', COUNT(*) FROM BENHNHAN UNION ALL
SELECT 'HSBA', COUNT(*) FROM HSBA UNION ALL
SELECT 'HSBA_DV', COUNT(*) FROM HSBA_DV UNION ALL
SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC UNION ALL
SELECT 'THONGBAO', COUNT(*) FROM THONGBAO;
```

- Lỗi thường gặp:
  - `ORA-00955 name is already used`: object cũ còn tồn tại. Chạy lại từ phần xóa bảng dòng 94.
  - Lỗi tiếng Việt bị sai font trong SQL Developer: mở file bằng UTF-8.

### Bước DB-03: Chạy `Database/hospital_setup.sql`, dòng 537-705

- Mục đích: TC#1, chuẩn hóa `ORACLE_USERNAME`, tạo view mapping `VW_TC1_NGUOIDUNG`, `VW_TC1_TOI_LA_AI`, tạo role `RL_TC1_USER`, tạo Oracle user cho nhân viên và bệnh nhân.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 537 đến dòng 704.
- Password user nghiệp vụ được tạo: `Welcome#123`.
- Kết quả mong đợi: các user như `BS_AN`, `BS_BAO`, `KTV_NAM`, `BN_ANH`, `DPV_LAN` được tạo.
- SQL kiểm tra:

```sql
SELECT * FROM VW_TC1_NGUOIDUNG ORDER BY LOAI_NGUOIDUNG, MA_NGUOIDUNG;

SELECT username
FROM dba_users
WHERE username IN ('BS_AN','BS_BAO','KTV_NAM','BN_ANH','DPV_LAN')
ORDER BY username;
```

- Lỗi thường gặp:
  - `ORA-01920 user name conflicts`: user đã tồn tại; block có xử lý bỏ qua phần lớn trường hợp này.
  - App login user nghiệp vụ lỗi không định danh được: kiểm tra `VW_TC1_TOI_LA_AI` bằng chính user đó.

### Bước DB-04: Chạy `Database/hospital_setup.sql`, dòng 708-797

- Mục đích: RBAC cho Kỹ thuật viên và Bệnh nhân.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 708 đến dòng 797.
- Object tạo/cấp quyền:
  - Role `RL_KYTHUATVIEN`, `RL_BENHNHAN`.
  - View `VW_BN_THONGTIN_CANHAN`.
  - View `VW_NV_THONGTIN_CANHAN`.
  - View `VW_KTV_HSBA_DV`.
  - FGA policy `AUDIT_KTV_UPDATE_KETQUA`.
- SQL kiểm tra:

```sql
SELECT role FROM dba_roles WHERE role IN ('RL_KYTHUATVIEN','RL_BENHNHAN');

SELECT object_name, object_type, status
FROM user_objects
WHERE object_name IN ('VW_BN_THONGTIN_CANHAN','VW_NV_THONGTIN_CANHAN','VW_KTV_HSBA_DV');

SELECT grantee, granted_role
FROM dba_role_privs
WHERE granted_role IN ('RL_KYTHUATVIEN','RL_BENHNHAN')
ORDER BY granted_role, grantee;
```

- Lỗi thường gặp:
  - `ORA-01031` ở `DBMS_FGA.ADD_POLICY`: `BVDBA` chưa có quyền đủ mạnh. Kiểm tra DB-01 đã grant `DBA` cho `BVDBA`.

### Bước DB-05: Chạy `Database/hospital_setup.sql`, dòng 801-1047

- Mục đích: VPD cho Điều phối viên và Bác sĩ/Y sĩ.
- Đăng nhập bằng user: `BVDBA`, không chạy bằng `SYS` hoặc `SYSTEM`.
- Chạy: dòng 801 đến dòng 1047.
- Object tạo:
  - Role `RL_DIEUPHOIVIEN`, `RL_BACSI`.
  - Package `PKG_VPD_HOSPITAL`.
  - Context `HOSPITAL_CTX`.
  - Trigger `TRG_SET_CTX_HOSPITAL`.
  - Policy `VPD_BENHNHAN_TC2_TC3`, `VPD_HSBA_TC2_TC3`, `VPD_HSBA_DV_TC2_TC3`, `VPD_DONTHUOC_TC3`.
- SQL kiểm tra:

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name IN ('PKG_VPD_HOSPITAL','TRG_SET_CTX_HOSPITAL');

SELECT object_name, policy_name, policy_function
FROM dba_policies
WHERE object_owner = 'BVDBA'
ORDER BY object_name;
```

- Lỗi thường gặp:
  - `ORA-20010 Vui long chay script bang schema owner`: bạn đang chạy bằng `SYS` hoặc `SYSTEM`.
  - User bác sĩ thấy 0 dòng: đăng nhập lại để trigger logon set context, hoặc chạy `EXEC BVDBA.PKG_VPD_HOSPITAL.set_ctx;` trong session test.

### Bước DB-06: Chạy `Database/hospital_setup.sql`, dòng 1051-1660

- Mục đích: tạo object phục vụ Phân hệ 1 DBA app và một số view/procedure nghiệp vụ.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 1051 đến dòng 1660.
- Object quan trọng:
  - Type: `T_USER_ROW`, `T_USER_TABLE`, `T_ROLE_ROW`, `T_ROLE_TABLE`, `T_OBJECT_ROW`, ...
  - Procedure: `SP_CREATE_USER`, `SP_DROP_USER`, `SP_ALTER_USER_PASSWORD`, `SP_LOCK_UNLOCK_USER`, `SP_CREATE_ROLE`, `SP_DROP_ROLE`, `SP_GRANT_OBJ_PRIV`, `SP_GRANT_ROLE`, `SP_REVOKE_OBJ_PRIV`, `SP_REVOKE_SYS_PRIV`, `SP_REVOKE_ROLE`.
  - Function: `FN_LIST_USERS`, `FN_LIST_ROLES`, `FN_GET_OBJ_PRIVS`, `FN_GET_SYS_PRIVS`, `FN_GET_ROLE_PRIVS`.
  - Procedure nghiệp vụ: `SP_THEM_BENHNHAN`, `SP_CAP_NHAT_HSBA`.
- SQL kiểm tra:

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_type IN ('TYPE','PROCEDURE','FUNCTION','VIEW','PACKAGE')
ORDER BY object_type, object_name;

SELECT * FROM TABLE(FN_LIST_USERS);
SELECT * FROM TABLE(FN_LIST_ROLES);
SELECT * FROM TABLE(FN_GET_OBJ_PRIVS('RL_BACSI'));
```

- Lỗi thường gặp:
  - App DBA không load được user/role: kiểm tra các function `FN_LIST_USERS`, `FN_LIST_ROLES` có `VALID`.

### Bước DB-07: Chạy block audit hệ thống trong `Database/hospital_setup.sql`, dòng 1826-1841

- Mục đích: kiểm tra `audit_trail`, cấp quyền đọc audit log cho `BVDBA`.
- Đăng nhập bằng user: `SYS AS SYSDBA`.
- Chạy: chỉ dòng 1826 đến dòng 1841.
- Kết quả mong đợi: thấy tham số `audit_trail`, `BVDBA` đọc được `DBA_AUDIT_TRAIL`.
- SQL kiểm tra:

```sql
SHOW PARAMETER audit_trail;

SELECT * FROM dba_sys_privs
WHERE grantee = 'BVDBA';

SELECT * FROM dba_role_privs
WHERE grantee = 'BVDBA'
AND granted_role = 'SELECT_CATALOG_ROLE';

SELECT owner, table_name, grantee, privilege
FROM dba_tab_privs
WHERE owner = 'SYS'
AND table_name = 'DBA_AUDIT_TRAIL'
AND grantee = 'BVDBA';
```

- Lỗi thường gặp:
  - `audit_trail = NONE`: bật audit cần chỉnh tham số và restart database. Phần source không có script restart.

### Bước DB-08: Chạy audit/FGA trong `Database/hospital_setup.sql`, dòng 1663-1824

- Mục đích: tạo FGA policy và Standard Audit cho các ngữ cảnh chính.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 1663 đến dòng 1824.
- Policy FGA:
  - `FGA_DT_UPDATE_AFTER_CREATE` trên `DONTHUOC`.
  - `FGA_HSBA_BS_UPDATE` trên `HSBA`.
  - `FGA_HSBA_DV_ILLEGAL_DML` trên `HSBA_DV`.
- Standard Audit:
  - `SELECT`, `UPDATE` trên `BVDBA.HSBA`.
  - DML thất bại trên `BVDBA.HSBA_DV`.
  - `EXECUTE` trên `BVDBA.SP_CAP_NHAT_HSBA`.
  - `SELECT` trên `BVDBA.VW_BN_THONGTIN_CANHAN`.
- SQL kiểm tra:

```sql
SELECT object_schema, object_name, policy_name, enabled
FROM dba_audit_policies
WHERE policy_name LIKE 'FGA_%';
```

Nếu query trên không có dữ liệu trong phiên bản Oracle của bạn, dùng:

```sql
SELECT object_schema, object_name, policy_name
FROM dba_fga_audit_trail
WHERE object_schema = 'BVDBA';
```

- Lỗi thường gặp:
  - `ORA-00942` khi đọc audit view: đăng nhập `BVDBA` chưa được cấp quyền ở DB-07.

### Bước DB-09: Chạy audit bổ sung trong `Database/hospital_setup.sql`, dòng 1855-2123

- Mục đích: tạo `VW_AUDIT_BENHNHAN_LIENQUAN`, `FN_AUDIT_DEM_HSBA_BACSI`, grant role và bật thêm 5 ngữ cảnh Standard Audit.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 1855 đến dòng 2123.
- Không chạy tiếp dòng 2125-2252 nếu muốn giữ hành vi VPD/KTV ổn định cho app. Đoạn 2125-2252 tạo lại package body `PKG_VPD_HOSPITAL` và trong source hiện tại có thể làm KTV không thấy dữ liệu `HSBA_DV` qua VPD.
- SQL kiểm tra:

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name IN ('VW_AUDIT_BENHNHAN_LIENQUAN','FN_AUDIT_DEM_HSBA_BACSI');

SELECT owner, object_name, sel, ins, upd, del, exe
FROM dba_obj_audit_opts
WHERE owner = 'BVDBA';
```

### Bước DB-10: Chạy OLS policy trong `Database/OLS_SYS.sql`, dòng 19-204

- Mục đích: enable OLS cho schema `BVDBA`, tạo policy `BENHVIEN_POL`, level, compartment, group, data label.
- Đăng nhập bằng user: `SYS AS SYSDBA`.
- Chạy: dòng 19 đến dòng 204.
- Không chạy dòng 214-348 ở bước này nếu user `U1...U8` chưa tồn tại.
- SQL kiểm tra:

```sql
SELECT policy_name, column_name
FROM dba_sa_policies
WHERE policy_name = 'BENHVIEN_POL';
```

- Lỗi thường gặp:
  - `LBACSYS` hoặc package `SA_*` không tồn tại: Oracle Label Security chưa được cài/enable trong database.

### Bước DB-11: Chạy gán nhãn data trong `Database/OLS_BVDBA.sql`, dòng 17-55

- Mục đích: gán `OLS_LABEL` cho `TB001` đến `TB007`.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 17 đến dòng 55.
- SQL kiểm tra:

```sql
SELECT MATHONGBAO,
       LABEL_TO_CHAR('BENHVIEN_POL', OLS_LABEL) AS NHAN_OLS
FROM THONGBAO
ORDER BY MATHONGBAO;
```

### Bước DB-12: Chạy tạo user OLS trong `Database/OLS_BVDBA.sql`, dòng 62-96

- Mục đích: tạo user demo `U1...U8`, password `Welcome#123`, grant `CREATE SESSION` và `SELECT ON THONGBAO`.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 62 đến dòng 96.
- SQL kiểm tra:

```sql
SELECT username
FROM dba_users
WHERE username LIKE 'U%_%'
ORDER BY username;
```

### Bước DB-13: Chạy gán nhãn user OLS trong `Database/OLS_SYS.sql`, dòng 214-348

- Mục đích: gán label đọc cho user `U1...U8`.
- Đăng nhập bằng user: `SYS AS SYSDBA`.
- Chạy: dòng 214 đến dòng 348.
- SQL kiểm tra:

```sql
SELECT user_name, read_label, read_compartments, read_groups
FROM dba_sa_user_labels
WHERE policy_name = 'BENHVIEN_POL'
ORDER BY user_name;
```

### Bước DB-14: Chạy kiểm tra OLS trong `Database/OLS_BVDBA.sql`, dòng 104-117

- Mục đích: xem lại nhãn user OLS.
- Đăng nhập bằng user: `BVDBA`.
- Chạy: dòng 104 đến dòng 117.
- Sau đó test từng user OLS bằng SQL Developer:

```sql
SELECT MATHONGBAO, NOIDUNG
FROM BVDBA.THONGBAO
ORDER BY MATHONGBAO;
```

### Bước DB-15: Chạy `Database/TEST.sql`

- Mục đích: tạo tình huống sinh FGA/audit log.
- Chạy theo đúng comment trong file:
  - Connection 2: `BS_AN / Welcome#123`, chạy dòng 2-14.
  - Connection 3: `BS_BAO / Welcome#123`, chạy dòng 17-20.
  - Connection 4: `KTV_NAM / Welcome#123`, chạy dòng 23-32.
  - Connection 1: `BVDBA / BvDba#2026`, chạy dòng 36-40 để đọc log.
- Kết quả mong đợi: có dòng trong `DBA_FGA_AUDIT_TRAIL`.

## 6. Kiểm tra database sau khi setup

Chạy bằng `BVDBA`:

```sql
SELECT table_name FROM user_tables ORDER BY table_name;

SELECT object_name, object_type, status
FROM user_objects
ORDER BY object_type, object_name;

SELECT 'NHANVIEN' bang, COUNT(*) so_dong FROM NHANVIEN UNION ALL
SELECT 'BENHNHAN', COUNT(*) FROM BENHNHAN UNION ALL
SELECT 'HSBA', COUNT(*) FROM HSBA UNION ALL
SELECT 'HSBA_DV', COUNT(*) FROM HSBA_DV UNION ALL
SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC UNION ALL
SELECT 'THONGBAO', COUNT(*) FROM THONGBAO;

SELECT username
FROM dba_users
WHERE username IN ('BVDBA','BS_AN','BS_BAO','KTV_NAM','BN_ANH','DPV_LAN')
ORDER BY username;

SELECT role
FROM dba_roles
WHERE role IN ('RL_TC1_USER','RL_BENHNHAN','RL_KYTHUATVIEN','RL_DIEUPHOIVIEN','RL_BACSI');

SELECT grantee, owner, table_name, privilege, grantable
FROM dba_tab_privs
WHERE owner = 'BVDBA'
ORDER BY grantee, table_name, privilege;

SELECT grantee, owner, table_name, column_name, privilege, grantable
FROM dba_col_privs
WHERE owner = 'BVDBA'
ORDER BY grantee, table_name, column_name;

SELECT object_name, policy_name, policy_function
FROM dba_policies
WHERE object_owner = 'BVDBA'
ORDER BY object_name;

SELECT db_user, object_name, policy_name, statement_type, sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'BVDBA'
ORDER BY timestamp DESC;

SELECT username, owner, obj_name, action_name, returncode
FROM dba_audit_trail
WHERE owner = 'BVDBA'
ORDER BY timestamp DESC;
```

Nếu dùng Unified Audit:

```sql
SELECT dbusername, object_schema, object_name, action_name, return_code
FROM unified_audit_trail
WHERE object_schema = 'BVDBA'
ORDER BY event_timestamp DESC;
```

## 7. Cấu hình connection cho WinForm

Không có connection string cố định trong `App.config`. Màn hình `Forms/LoginForm.cs` tạo connection string từ UI:

```text
User Id=<Username>;Password=<Password>;Data Source=<Host>:<Port>/<Service Name>
```

Giá trị mặc định trên form:

- Host: `localhost`
- Port: `1521`
- Service Name: `XEPDB1`
- Username: `SYSTEM`
- Có checkbox `Kết nối với quyền SYSDBA`

Khi login:

- Nếu username là `BVDBA`, app mở `MainForm` của phân hệ DBA.
- Nếu là user nghiệp vụ, app query `BVDBA.VW_TC1_TOI_LA_AI` để lấy `VAITRO`.
- `Benh nhan` mở `PatientForm`.
- `Ky thuat vien` mở `TechnicianForm`.
- `Dieu phoi vien` mở `CoordinatorForm`.
- `Bac si/Y si` mở `DoctorForm`.

App dùng `Oracle.ManagedDataAccess`, thường không cần Oracle Client full. Nếu build/run báo thiếu `Oracle.ManagedDataAccess.dll`, kiểm tra thư mục `packages/Oracle.ManagedDataAccess.23.26.100/lib/net472`.

## 8. Cách chạy WinForm bằng VS Code

Project này là `.NET Framework 4.7.2` kiểu cũ, không phải SDK-style project. Vì vậy `dotnet run` thường không phải cách phù hợp. Dùng MSBuild.

1. Mở VS Code tại thư mục project:

```powershell
cd E:\ATBM_05_PhanHe2
code .
```

2. Mở Terminal trong VS Code.

3. Kiểm tra .NET SDK nếu muốn:

```powershell
dotnet --version
```

4. Build bằng MSBuild. Lệnh này đã được ghi trong `README.MD`:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\OracleAdminApp.sln /t:Restore,Build /p:Configuration=Release
```

Nếu dùng Build Tools, đường dẫn có thể là:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" .\OracleAdminApp.sln /t:Restore,Build /p:Configuration=Release
```

5. Run app:

```powershell
.\bin\Release\OracleAdminApp.exe
```

Hoặc build Debug:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\OracleAdminApp.sln /t:Restore,Build /p:Configuration=Debug
.\bin\Debug\OracleAdminApp.exe
```

Nếu muốn chạy dễ hơn, mở `OracleAdminApp.sln` bằng Visual Studio và bấm Start.

## 9. Tài khoản mẫu để test

| Username | Password | Vai trò | Dùng để test |
|---|---|---|---|
| `BVDBA` | `BvDba#2026` | DBA/app admin | Phân hệ 1 |
| `DPV_LAN` | `Welcome#123` | Điều phối viên | Thêm/sửa bệnh nhân, HSBA, phân công |
| `DPV_MINH` | `Welcome#123` | Điều phối viên | Test thêm điều phối viên |
| `BS_AN` | `Welcome#123` | Bác sĩ/Y sĩ | HSBA `BS001`, audit update |
| `BS_BAO` | `Welcome#123` | Bác sĩ/Y sĩ | Negative test sửa HSBA của BS khác |
| `KTV_NAM` | `Welcome#123` | Kỹ thuật viên | Dịch vụ `KTV01`, cập nhật `KETQUA` |
| `BN_ANH` | `Welcome#123` | Bệnh nhân | Xem/sửa thông tin cá nhân |
| `BN_BINH` | `Welcome#123` | Bệnh nhân | Negative test bệnh nhân khác |
| `U1_GIAMDOC` | `Welcome#123` | OLS Giám đốc | Xem toàn bộ thông báo bằng SQL Developer |
| `U2_LDKTIMMACH_HCM` | `Welcome#123` | OLS lãnh đạo Tim mạch HCM | Test OLS |
| `U3_LDKTHANKINH_HN` | `Welcome#123` | OLS lãnh đạo Thần kinh HN | Test OLS |
| `U4_NVTHANKINH_HCM` | `Welcome#123` | OLS nhân viên Thần kinh HCM | Test OLS |
| `U5_NVTIMMACH_HCM` | `Welcome#123` | OLS nhân viên Tim mạch HCM | Test OLS |
| `U6_LDPTIMMACH_HCM` | `Welcome#123` | OLS lãnh đạo phòng Tim mạch HCM | Test OLS |
| `U7_LDPTOBO` | `Welcome#123` | OLS lãnh đạo phòng toàn bộ | Test OLS |
| `U8_NVTIEUHOA_HN` | `Welcome#123` | OLS nhân viên Tiêu hóa HN | Test OLS |

Tất cả username trong Oracle được chuẩn hóa uppercase ở TC#1.

## 10. Hướng dẫn test Phân hệ 1 - DBA App

Đăng nhập app bằng:

- Host: `localhost`
- Port: `1521`
- Service Name: `XEPDB1`
- Username: `BVDBA`
- Password: `BvDba#2026`

Checklist:

| Chức năng | Thao tác UI | Dữ liệu mẫu | SQL kiểm chứng |
|---|---|---|---|
| Xem user | Tab `Quan ly User` | Bấm `Lam moi` | `SELECT * FROM TABLE(BVDBA.FN_LIST_USERS);` |
| Tạo user | `+ Tao User` | `TEST_USER_01` / `Test#123` / `BENHVIEN_TBS` | `SELECT username FROM dba_users WHERE username='TEST_USER_01';` |
| Sửa user | Chọn user, bấm `Sua` | đổi password hoặc lock/unlock | `SELECT account_status FROM dba_users WHERE username='TEST_USER_01';` |
| Xóa user | Chọn user, bấm `Xoa` | `TEST_USER_01` | `SELECT username FROM dba_users WHERE username='TEST_USER_01';` |
| Xem role | Tab `Quan ly Role` | Bấm `Lam moi` | `SELECT * FROM TABLE(BVDBA.FN_LIST_ROLES);` |
| Tạo role | `+ Tao Role` | `TEST_ROLE_01` | `SELECT role FROM dba_roles WHERE role='TEST_ROLE_01';` |
| Xóa role | Chọn role, bấm `Xoa Role` | `TEST_ROLE_01` | `SELECT role FROM dba_roles WHERE role='TEST_ROLE_01';` |
| Cấp quyền user | Tab `Cap quyen` | User `BS_AN`, object `BVDBA.HSBA`, `SELECT` | `SELECT * FROM dba_tab_privs WHERE grantee='BS_AN';` |
| Cấp quyền role | Tab `Cap quyen` | Role `RL_BACSI`, object `BVDBA.HSBA`, `SELECT` | `SELECT * FROM dba_tab_privs WHERE grantee='RL_BACSI';` |
| Cấp role cho user | Tab `Cap Role cho User` | Role `RL_BACSI` cho `BS_AN` | `SELECT * FROM dba_role_privs WHERE grantee='BS_AN';` |
| WITH GRANT OPTION | Tick checkbox khi cấp quyền cho user | `SELECT ON BVDBA.THONGBAO TO BS_AN` | `SELECT grantable FROM dba_tab_privs WHERE grantee='BS_AN' AND table_name='THONGBAO';` |
| SELECT theo cột | Chọn `SELECT`, bỏ `Tat ca cac cot`, chọn cột | `MABN,TENBN` trên `BENHNHAN` | Source tạo view trung gian `V_<object>_<grantee>` |
| UPDATE theo cột | Chọn `UPDATE`, chọn cột | `UPDATE(TIENSUBENH)` | `SELECT * FROM dba_col_privs WHERE grantee='...';` |
| Thu hồi quyền | Tab `Thu hoi quyen` | Chọn quyền rồi bấm thu hồi | `SELECT * FROM dba_tab_privs WHERE grantee='...';` |
| Xem quyền | Tab `Xem quyen` | Chọn user/role | So với `DBA_TAB_PRIVS`, `DBA_COL_PRIVS`, `DBA_ROLE_PRIVS` |

Lỗi thường gặp: thiếu các procedure/function ở DB-06 thì panel sẽ báo lỗi khi load hoặc khi bấm thao tác.

## 11. Hướng dẫn test Phân hệ 2 - Hospital App

### Điều phối viên

- User test: `DPV_LAN / Welcome#123`.
- UI mở: `CoordinatorForm`.
- Test:
  - Tab `Benh nhan`: xem danh sách, thêm bệnh nhân `BN999`, sửa địa chỉ hoặc tiền sử.
  - Tab `Ho so benh an`: thêm HSBA mới, cập nhật `MABS`, `MAKHOA`.
  - Tab `Dieu phoi KTV`: chọn dòng `HSBA_DV`, đổi `MAKTV`.
- SQL kiểm chứng:

```sql
SELECT * FROM BVDBA.BENHNHAN WHERE MABN = 'BN999';
SELECT MAHSBA, MABS, MAKHOA FROM BVDBA.HSBA WHERE MAHSBA = '<ma_hsba>';
SELECT MAHSBA, LOAIDV, MAKTV FROM BVDBA.HSBA_DV WHERE MAHSBA = '<ma_hsba>';
```

- Negative test: thử đăng nhập bằng bệnh nhân rồi insert `HSBA`, phải bị chặn.

### Bác sĩ/Y sĩ

- User test: `BS_AN / Welcome#123`.
- UI mở: `DoctorForm`.
- Test:
  - Tab `HSBA cua toi`: chỉ thấy HSBA có `MABS='BS001'`.
  - Cập nhật `CHANDOAN`, `DIEUTRI`, `KETLUAN`.
  - Tab `Benh nhan lien quan`: chỉ thấy bệnh nhân có HSBA do `BS001` phụ trách.
  - Tab `HSBA_DV`: thêm/xóa dịch vụ cho HSBA của mình.
  - Tab `Don thuoc`: thêm/sửa/xóa đơn thuốc cho HSBA của mình.
- SQL kiểm chứng:

```sql
-- chạy bằng BS_AN
SELECT DISTINCT MABS FROM BVDBA.HSBA;

-- chạy bằng BVDBA
SELECT * FROM dba_fga_audit_trail
WHERE object_schema='BVDBA'
ORDER BY timestamp DESC;
```

- Negative test: `BS_BAO` sửa `HSBA001` của `BS_AN`; kết quả đúng là không sửa được hoặc update 0 dòng/lỗi do VPD/SP.

### Kỹ thuật viên

- User test: `KTV_NAM / Welcome#123`.
- UI mở: `TechnicianForm`.
- Test:
  - Tab dịch vụ: chỉ thấy dòng `HSBA_DV` có `MAKTV='KTV01'`.
  - Cập nhật `KETQUA`.
  - Tab thông tin cá nhân: chỉ sửa `QUEQUAN`, `SODT`.
- SQL kiểm chứng:

```sql
-- chạy bằng KTV_NAM
SELECT MAHSBA, LOAIDV, MAKTV, KETQUA
FROM BVDBA.VW_KTV_HSBA_DV;

-- chạy bằng BVDBA
SELECT db_user, object_name, policy_name, statement_type, sql_text
FROM dba_fga_audit_trail
WHERE object_schema='BVDBA'
ORDER BY timestamp DESC;
```

### Bệnh nhân

- User test: `BN_ANH / Welcome#123`.
- UI mở: `PatientForm`.
- Test:
  - Chỉ thấy thông tin cá nhân của `BN_ANH`.
  - Sửa `SONHA`, `TENDUONG`, `QUANHUYEN`, `TINHTHANH`, `TIENSUBENH`, `TIENSUBENHGD`, `DIUNGTHUOC`.
  - Không sửa được `MABN`, `TENBN`, `PHAI`, `NGAYSINH`, `CCCD` trên UI vì textbox readonly.
- SQL kiểm chứng:

```sql
-- chạy bằng BN_ANH
SELECT MABN, TENBN, CCCD, SONHA, TENDUONG
FROM BVDBA.VW_BN_THONGTIN_CANHAN;
```

## 12. Hướng dẫn test RBAC

Role trong source:

- `RL_TC1_USER`: có quyền `SELECT ON VW_TC1_TOI_LA_AI`.
- `RL_BENHNHAN`: `CREATE SESSION`, `SELECT/UPDATE` view `VW_BN_THONGTIN_CANHAN`.
- `RL_KYTHUATVIEN`: `CREATE SESSION`, `SELECT/UPDATE` view `VW_NV_THONGTIN_CANHAN`, `SELECT/UPDATE(KETQUA)` view `VW_KTV_HSBA_DV`.
- `RL_DIEUPHOIVIEN`: quyền trên `BENHNHAN`, `HSBA`, `HSBA_DV`.
- `RL_BACSI`: quyền trên `HSBA`, `BENHNHAN`, `HSBA_DV`, `DONTHUOC`.

SQL kiểm tra:

```sql
SELECT grantee, granted_role
FROM dba_role_privs
WHERE granted_role IN ('RL_TC1_USER','RL_BENHNHAN','RL_KYTHUATVIEN','RL_DIEUPHOIVIEN','RL_BACSI')
ORDER BY granted_role, grantee;

SELECT grantee, table_name, privilege
FROM dba_tab_privs
WHERE owner='BVDBA'
ORDER BY grantee, table_name, privilege;

SELECT grantee, table_name, column_name, privilege
FROM dba_col_privs
WHERE owner='BVDBA'
ORDER BY grantee, table_name, column_name;
```

Test hợp lệ:

- `BN_ANH` select/update view cá nhân.
- `KTV_NAM` update `KETQUA` qua `VW_KTV_HSBA_DV`.

Test bất hợp lệ:

- `BN_ANH` update `BVDBA.BENHNHAN.CCCD`.
- `KTV_NAM` insert `BVDBA.HSBA_DV`.

Expected result: hợp lệ chạy thành công; bất hợp lệ báo `ORA-01031`, `ORA-00942`, hoặc update 0 dòng do view/VPD.

## 13. Hướng dẫn test VPD

Policy trong source:

- `VPD_BENHNHAN_TC2_TC3` trên `BENHNHAN`.
- `VPD_HSBA_TC2_TC3` trên `HSBA`.
- `VPD_HSBA_DV_TC2_TC3` trên `HSBA_DV`.
- `VPD_DONTHUOC_TC3` trên `DONTHUOC`.
- Policy function: `PKG_VPD_HOSPITAL.vpd_predicate`.
- Context: `HOSPITAL_CTX`, set bởi trigger `TRG_SET_CTX_HOSPITAL`.

Predicate chính:

- Điều phối viên: thấy toàn bộ `BENHNHAN`, `HSBA`, `HSBA_DV`.
- Bác sĩ/Y sĩ: thấy `HSBA` có `MABS` là mã bác sĩ của mình; `BENHNHAN` và `DONTHUOC` liên quan HSBA của mình.
- Kỹ thuật viên: trong package body đầu, `HSBA_DV` lọc theo `MAKTV`.

SQL kiểm tra:

```sql
SELECT object_name, policy_name, policy_function, sel, ins, upd, del
FROM dba_policies
WHERE object_owner='BVDBA'
ORDER BY object_name;

-- chạy bằng BS_AN
SELECT MAHSBA, MABS FROM BVDBA.HSBA ORDER BY MAHSBA;

-- chạy bằng DPV_LAN
SELECT COUNT(*) FROM BVDBA.HSBA;
```

Phân biệt lỗi thiếu quyền và VPD:

- Thiếu quyền thường báo `ORA-01031` hoặc `ORA-00942`.
- VPD thường không báo lỗi mà chỉ trả ít dòng hơn hoặc update 0 dòng.
- Nếu `SP_CAP_NHAT_HSBA` update 0 dòng, procedure raise `ORA-20010`.

## 14. Hướng dẫn test OLS

Policy name: `BENHVIEN_POL`.

Label components:

- Level: `BGD`, `LDK`, `NV`.
- Compartment/khoa: `TH`, `TK`, `TM`.
- Group/cơ sở: `HCM`, `HP`, `HN`.

Data labels:

| Thông báo | Label |
|---|---|
| `TB001` | `NV` |
| `TB002` | `BGD` |
| `TB003` | `LDK` |
| `TB004` | `LDK:TH` |
| `TB005` | `NV:TH:HCM` |
| `TB006` | `NV:TH:HN` |
| `TB007` | `LDK:TH,TK:HP` |

Expected visibility theo script:

| User | Dự kiến thấy |
|---|---|
| `U1_GIAMDOC` | `TB001` đến `TB007` |
| `U2_LDKTIMMACH_HCM` | `TB001`, `TB003` |
| `U3_LDKTHANKINH_HN` | `TB001`, `TB003` |
| `U4_NVTHANKINH_HCM` | `TB001` |
| `U5_NVTIMMACH_HCM` | `TB001` |
| `U6_LDPTIMMACH_HCM` | `TB001`, `TB003` |
| `U7_LDPTOBO` | `TB001`, `TB003`, `TB004`, `TB005`, `TB006`, `TB007` |
| `U8_NVTIEUHOA_HN` | `TB001`, `TB006` |

SQL test bằng từng user OLS:

```sql
SELECT MATHONGBAO, SUBSTR(NOIDUNG,1,80) AS NOIDUNG
FROM BVDBA.THONGBAO
ORDER BY MATHONGBAO;
```

Dấu hiệu cấu hình sai:

- `ORA-124xx`: lỗi OLS component/policy/label.
- User thấy 0 dòng: chưa gán label user hoặc chưa grant `SELECT ON THONGBAO`.
- Chạy nguyên `OLS_SYS.sql` lần đầu báo user không tồn tại: cần làm DB-10, DB-12, rồi DB-13.

## 15. Hướng dẫn test Audit

Standard Audit theo source:

- `SELECT`, `UPDATE` trên `HSBA`.
- `INSERT/UPDATE/DELETE` thất bại trên `HSBA_DV`.
- `EXECUTE` trên `SP_CAP_NHAT_HSBA`.
- `SELECT` trên `VW_BN_THONGTIN_CANHAN`.
- Audit bổ sung DB-09 có thêm audit `BENHNHAN`, `VW_AUDIT_BENHNHAN_LIENQUAN`, `FN_AUDIT_DEM_HSBA_BACSI`.

FGA theo source:

- `FGA_DT_UPDATE_AFTER_CREATE` audit update `TENTHUOC,LIEUDUNG` trên `DONTHUOC`.
- `FGA_HSBA_BS_UPDATE` audit update `CHANDOAN,DIEUTRI,KETLUAN` trên `HSBA`.
- `FGA_HSBA_DV_ILLEGAL_DML` audit DML trên `HSBA_DV`.
- `AUDIT_KTV_UPDATE_KETQUA` audit update `KETQUA` trên `HSBA_DV`.

Tạo hành vi audit:

```sql
-- BS_AN
UPDATE BVDBA.HSBA
SET CHANDOAN = N'Test audit HSBA'
WHERE MAHSBA = 'HSBA001';
COMMIT;

UPDATE BVDBA.DONTHUOC
SET LIEUDUNG = N'Test audit don thuoc'
WHERE MAHSBA='HSBA001'
AND NGAYDT=DATE '2024-01-10'
AND TENTHUOC=N'Omeprazole 20mg';
COMMIT;
```

Đọc log:

```sql
SELECT TO_CHAR(timestamp,'YYYY-MM-DD HH24:MI:SS') thoi_gian,
       db_user, object_name, policy_name, statement_type,
       SUBSTR(sql_text,1,200) sql_text
FROM dba_fga_audit_trail
WHERE object_schema='BVDBA'
ORDER BY timestamp DESC;

SELECT TO_CHAR(timestamp,'YYYY-MM-DD HH24:MI:SS') thoi_gian,
       username, owner, obj_name, action_name, returncode
FROM dba_audit_trail
WHERE owner='BVDBA'
ORDER BY timestamp DESC;
```

## 16. Negative test case bảo mật

| Test | User | Thao tác | Kết quả đúng | Audit |
|---|---|---|---|---|
| Bệnh nhân A xem B | `BN_ANH` | Query `VW_BN_THONGTIN_CANHAN` | Chỉ thấy `BN001` | Standard audit nếu bật SELECT view |
| Bệnh nhân sửa CCCD | `BN_ANH` | `UPDATE BVDBA.VW_BN_THONGTIN_CANHAN SET CCCD='...'` | `ORA-01031` hoặc không cho sửa | Có thể có audit thất bại nếu bật |
| BS A xem HSBA BS B | `BS_AN` | `SELECT * FROM BVDBA.HSBA WHERE MABS <> 'BS001'` | 0 dòng | Standard audit SELECT HSBA |
| BS A sửa HSBA BS B | `BS_BAO` | Sửa `HSBA001` | Bị VPD/SP chặn | Standard/FGA tùy statement |
| KTV A xem của KTV B | `KTV_NAM` | Query `VW_KTV_HSBA_DV` | Chỉ thấy `MAKTV='KTV01'` | Không bắt buộc |
| KTV A sửa của KTV B | `KTV_NAM` | Update dòng `MAKTV <> 'KTV01'` | 0 dòng hoặc lỗi | FGA nếu statement tác động object |
| OLS đọc sai label | `U8_NVTIEUHOA_HN` | Select `THONGBAO` | Không thấy `TB002`, `TB005` | Chưa thấy audit OLS trong source |
| User không quyền gọi procedure | `BN_ANH` | `EXEC BVDBA.SP_CAP_NHAT_HSBA(...)` | `ORA-01031` | Standard audit nếu bật EXEC thất bại |

## 17. Lỗi thường gặp và cách xử lý

| Lỗi | Nguyên nhân | Cách kiểm tra | Cách xử lý |
|---|---|---|---|
| `ORA-01017 invalid username/password` | Sai username/password/service | Test connection SQL Developer | User nghiệp vụ dùng uppercase, password `Welcome#123`; `BVDBA` dùng `BvDba#2026` |
| `ORA-00942 table or view does not exist` | Chưa chạy script, sai schema, thiếu quyền | `SELECT table_name FROM all_tables WHERE owner='BVDBA';` | Chạy lại DB-02 đến DB-06, dùng prefix `BVDBA.` |
| `ORA-01031 insufficient privileges` | Role chưa grant, user sai | `DBA_ROLE_PRIVS`, `DBA_TAB_PRIVS` | Chạy lại DB-03/04/05/06 |
| `ORA-28110` hoặc lỗi VPD | Policy function lỗi | `SHOW ERRORS PACKAGE BODY PKG_VPD_HOSPITAL` | Compile lại package DB-05; không chạy đoạn override dòng 2125-2252 nếu không cần |
| OLS `ORA-124xx` | OLS chưa enable hoặc label sai | `DBA_SA_POLICIES`, `DBA_SA_USER_LABELS` | Chạy DB-10 đến DB-14 đúng thứ tự |
| App không login OLS user | `LoginForm` cần `VW_TC1_TOI_LA_AI`, OLS user không có mapping | Login `U1_GIAMDOC` báo không định danh | Test OLS bằng SQL Developer; source hiện chưa route UI cho `U1...U8` |
| Lỗi connection string | Sai service name/SID | SQL Developer test `localhost:1521/XEPDB1` | Dùng Service Name `XEPDB1`, không dùng SID nếu đang Oracle XE PDB |
| Thiếu `Oracle.ManagedDataAccess` | Package chưa restore/copy | Kiểm tra `packages/Oracle.ManagedDataAccess.23.26.100` | Build bằng MSBuild `/t:Restore,Build` |
| `dotnet run` không chạy | Project là `.NET Framework` kiểu cũ | Mở `.csproj` thấy `TargetFrameworkVersion v4.7.2` | Dùng MSBuild hoặc Visual Studio |
| Sai container | Đang ở `CDB$ROOT` | `SHOW CON_NAME;` | `ALTER SESSION SET CONTAINER = XEPDB1;` |
| KTV thấy 0 dịch vụ sau khi chạy audit cuối | Đã chạy dòng 2125-2252 làm override VPD package body | Kiểm tra package body phần `HSBA_DV` có nhánh KTV không | Chạy lại DB-05 để restore package body ban đầu |

## 18. Checklist chạy demo trước khi chấm

1. Oracle service và listener đang chạy.
2. SQL Developer kết nối được `SYS AS SYSDBA`.
3. `SHOW CON_NAME;` trả về `XEPDB1`.
4. User `BVDBA` tồn tại, login được.
5. Schema `BVDBA` có đủ 6 bảng chính.
6. Data mẫu có đủ: `NHANVIEN`, `BENHNHAN`, `HSBA`, `HSBA_DV`, `DONTHUOC`, `THONGBAO`.
7. User nghiệp vụ như `BS_AN`, `KTV_NAM`, `BN_ANH`, `DPV_LAN` tồn tại.
8. Role `RL_BENHNHAN`, `RL_KYTHUATVIEN`, `RL_DIEUPHOIVIEN`, `RL_BACSI` tồn tại.
9. RBAC pass: bệnh nhân/KTV chỉ thao tác view được cấp.
10. VPD pass: bác sĩ chỉ thấy HSBA của mình; điều phối viên thấy toàn bộ.
11. OLS pass bằng SQL Developer với `U1...U8`.
12. Audit/FGA sinh log sau khi chạy `TEST.sql`.
13. WinForm build được bằng MSBuild.
14. Login `BVDBA` mở Phân hệ 1.
15. Login `DPV_LAN`, `BS_AN`, `KTV_NAM`, `BN_ANH` mở đúng màn hình.
16. Negative test bị chặn đúng.
17. Trước demo, không chạy nhầm `hospital_setup.sql` dòng 2125-2252 nếu cần KTV thấy dữ liệu.
