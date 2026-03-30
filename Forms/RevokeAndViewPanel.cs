using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    // ══════════════════════════════════════════════════════════════════════════
    // REVOKE PRIVILEGE PANEL
    // Yêu cầu 4: Thu hồi quyền từ user hoặc role
    //
    // Flow:
    //   1. Chọn Loại (User / Role) → cmbTargetType
    //   2. Chọn tên              → cmbTarget  (load từ FN_LIST_USERS / FN_LIST_ROLES)
    //   3. Chọn Loại quyền hiển thị → cmbPrivType
    //   4. Bấm "Tải danh sách quyền" → LoadPrivileges()
    //      - Quyền đối tượng + cột : SELECT * FROM TABLE(FN_GET_OBJ_PRIVS(:grantee))
    //      - Quyền hệ thống        : SELECT * FROM TABLE(FN_GET_SYS_PRIVS(:grantee))
    //      - Role được cấp         : SELECT * FROM TABLE(FN_GET_ROLE_PRIVS(:grantee))
    //   5. Chọn dòng trong grid → bấm "Thu hồi quyền đã chọn"
    //      - Quyền đối tượng  → SP_REVOKE_OBJ_PRIV(privilege, owner, object, grantee, columns)
    //      - Quyền hệ thống   → SP_REVOKE_SYS_PRIV(privilege, grantee)
    //      - Role             → SP_REVOKE_ROLE(role, grantee)
    // ══════════════════════════════════════════════════════════════════════════
    public class RevokePrivilegePanel : UserControl
    {
        private ComboBox cmbTargetType, cmbTarget, cmbPrivType;
        private DataGridView dgvCurrentPrivs;
        private Button btnLoadPrivs, btnRevoke;
        private Label lblStatus;
        private readonly string _connStr;

        public RevokePrivilegePanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadTargets(); // load user/role ngay khi khởi tạo
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;
            this.Padding = new Padding(10);

            // ── Tạo tất cả controls trước, add theo đúng thứ tự Dock ──────

            // Header (Dock Top)
            var header = UIHelper.CreateSectionHeader(
                "Thu hoi quyen",
                "Xem va thu hoi quyen dang duoc cap cho user hoac role");

            // Panel Top: card + nut Load (Dock Top)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Color.Transparent
            };

            var card = UIHelper.CreateCard(0, 0, 720, 105, "CHON DOI TUONG");
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlTop.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loai", 10, 32, 110, out cmbTargetType);
            cmbTargetType.Items.AddRange(new object[] { "User", "Role" });
            cmbTargetType.SelectedIndex = 0;
            cmbTargetType.SelectedIndexChanged += (s, e) => LoadTargets();

            UIHelper.CreateLabeledCombo(card, "Ten User / Role", 140, 32, 220, out cmbTarget);

            UIHelper.CreateLabeledCombo(card, "Loai quyen hien thi", 380, 32, 220, out cmbPrivType);
            cmbPrivType.Items.AddRange(new object[] {
                "Tat ca",
                "Quyen tren doi tuong",
                "Quyen he thong",
                "Role duoc cap"
            });
            cmbPrivType.SelectedIndex = 0;

            btnLoadPrivs = UIHelper.CreateButton("Tai danh sach quyen", ButtonStyle.Secondary);
            btnLoadPrivs.Size = new Size(180, 34);
            btnLoadPrivs.Location = new Point(0, 115);
            btnLoadPrivs.ForeColor = UIHelper.TextDark;
            btnLoadPrivs.Click += (s, e) => LoadPrivileges();
            pnlTop.Controls.Add(btnLoadPrivs);

            // Panel Bottom: nut Revoke (Dock Bottom)
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Transparent
            };

            btnRevoke = UIHelper.CreateButton("THU HOI QUYEN DA CHON", ButtonStyle.Danger);
            btnRevoke.Size = new Size(220, 36);
            btnRevoke.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnRevoke.Click += BtnRevoke_Click;
            pnlBottom.Controls.Add(btnRevoke);
            pnlBottom.Resize += (s, e) =>
                btnRevoke.Location = new Point(pnlBottom.Width - 230, 7);

            // Grid (Dock Fill)
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvCurrentPrivs = UIHelper.CreateGrid();
            dgvCurrentPrivs.Dock = DockStyle.Fill;
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIV_TYPE", HeaderText = "Loai quyen", FillWeight = 14 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIVILEGE", HeaderText = "Ten quyen / Role", FillWeight = 20 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_OWNER", HeaderText = "Schema", FillWeight = 13 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_NAME", HeaderText = "Ten doi tuong", FillWeight = 18 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_TYPE", HeaderText = "Loai doi tuong", FillWeight = 12 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "COLUMNS", HeaderText = "Cot (neu co)", FillWeight = 13 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "GRANT_OPTION", HeaderText = "Tuy chon cap", FillWeight = 10 });

            dgvCurrentPrivs.CellFormatting += DgvPrivs_CellFormatting;
            pnlGrid.Controls.Add(dgvCurrentPrivs);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);

            // ── Thứ tự add: Fill truoc, Bottom sau, Top sau cung ──────────
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
            this.Controls.Add(header);
        }

        // ── Tô màu dòng theo loại quyền ───────────────────────────────────
        private void DgvPrivs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCurrentPrivs.Columns[e.ColumnIndex].Name == "PRIV_TYPE" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "Doi tuong": e.CellStyle.ForeColor = UIHelper.Primary; break;
                    case "Cot": e.CellStyle.ForeColor = Color.DarkCyan; break;
                    case "He thong": e.CellStyle.ForeColor = UIHelper.Warning; break;
                    case "Role": e.CellStyle.ForeColor = UIHelper.Success; break;
                }
            }
        }

        // ── Load danh sách User hoặc Role vào cmbTarget ───────────────────
        // User: SELECT * FROM TABLE(FN_LIST_USERS)  → cột USERNAME
        // Role: SELECT * FROM TABLE(FN_LIST_ROLES)  → cột ROLE
        private void LoadTargets()
        {
            cmbTarget.Items.Clear();
            bool isUser = cmbTargetType.SelectedIndex == 0;

            try
            {
                string sql = isUser
                    ? "SELECT USERNAME FROM TABLE(FN_LIST_USERS) ORDER BY USERNAME"
                    : "SELECT ROLE FROM TABLE(FN_LIST_ROLES) ORDER BY ROLE";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbTarget.Items.Add(reader[0].ToString());
                    }
                }

                if (cmbTarget.Items.Count > 0)
                    cmbTarget.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi tai danh sach: " + ex.Message, StatusType.Error);
            }
        }

        // ── Load danh sách quyền hiện có của grantee ──────────────────────
        // Gọi 3 pipelined function tuỳ theo cmbPrivType:
        //   FN_GET_OBJ_PRIVS  → quyền đối tượng (TABLE/VIEW/PROC/FUNC) + quyền theo cột
        //   FN_GET_SYS_PRIVS  → quyền hệ thống
        //   FN_GET_ROLE_PRIVS → role được cấp
        private void LoadPrivileges()
        {
            if (cmbTarget.SelectedItem == null)
            {
                UIHelper.SetStatus(lblStatus, "Vui long chon User hoac Role!", StatusType.Warning);
                return;
            }

            dgvCurrentPrivs.Rows.Clear();
            string grantee = cmbTarget.SelectedItem.ToString();
            string privType = cmbPrivType.SelectedItem?.ToString() ?? "Tat ca";
            bool showObj = privType == "Tat ca" || privType == "Quyen tren doi tuong";
            bool showSys = privType == "Tat ca" || privType == "Quyen he thong";
            bool showRole = privType == "Tat ca" || privType == "Role duoc cap";

            UIHelper.SetStatus(lblStatus, "Dang tai...", StatusType.Info);

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    // ── 1. Quyền đối tượng + cột (FN_GET_OBJ_PRIVS) ──────
                    // Trả về: GRANTEE, OWNER, OBJECT_NAME, OBJECT_TYPE, PRIVILEGE, GRANTABLE, COLUMN_NAME
                    // OBJECT_TYPE = 'COLUMN' khi là quyền cấp theo cột
                    if (showObj)
                    {
                        const string sqlObj = @"
                            SELECT OWNER, OBJECT_NAME, OBJECT_TYPE,
                                   PRIVILEGE, GRANTABLE, COLUMN_NAME
                            FROM   TABLE(FN_GET_OBJ_PRIVS(:g))
                            ORDER  BY OWNER, OBJECT_NAME, PRIVILEGE";

                        using (var cmd = new OracleCommand(sqlObj, conn))
                        {
                            cmd.Parameters.Add("g", OracleDbType.Varchar2).Value = grantee;
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    string objType = r["OBJECT_TYPE"]?.ToString() ?? "";
                                    string colName = r["COLUMN_NAME"]?.ToString() ?? "";
                                    string privLabel = objType == "COLUMN" ? "Cot" : "Doi tuong";

                                    dgvCurrentPrivs.Rows.Add(
                                        privLabel,
                                        r["PRIVILEGE"].ToString(),
                                        r["OWNER"].ToString(),
                                        r["OBJECT_NAME"].ToString(),
                                        objType == "COLUMN" ? "COLUMN" : objType,
                                        colName,
                                        r["GRANTABLE"].ToString()
                                    );
                                }
                            }
                        }
                    }

                    // ── 2. Quyền hệ thống (FN_GET_SYS_PRIVS) ────────────
                    // Trả về: GRANTEE, PRIVILEGE, ADMIN_OPT
                    if (showSys)
                    {
                        const string sqlSys = @"
                            SELECT PRIVILEGE, ADMIN_OPT
                            FROM   TABLE(FN_GET_SYS_PRIVS(:g))
                            ORDER  BY PRIVILEGE";

                        using (var cmd = new OracleCommand(sqlSys, conn))
                        {
                            cmd.Parameters.Add("g", OracleDbType.Varchar2).Value = grantee;
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    dgvCurrentPrivs.Rows.Add(
                                        "He thong",
                                        r["PRIVILEGE"].ToString(),
                                        "", "", "SYSTEM", "",
                                        r["ADMIN_OPT"].ToString()
                                    );
                                }
                            }
                        }
                    }

                    // ── 3. Role được cấp (FN_GET_ROLE_PRIVS) ─────────────
                    // Trả về: GRANTEE, GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
                    if (showRole)
                    {
                        const string sqlRole = @"
                            SELECT GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
                            FROM   TABLE(FN_GET_ROLE_PRIVS(:g))
                            ORDER  BY GRANTED_ROLE";

                        using (var cmd = new OracleCommand(sqlRole, conn))
                        {
                            cmd.Parameters.Add("g", OracleDbType.Varchar2).Value = grantee;
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    dgvCurrentPrivs.Rows.Add(
                                        "Role",
                                        r["GRANTED_ROLE"].ToString(),
                                        "", "", "ROLE", "",
                                        r["ADMIN_OPTION"].ToString()
                                    );
                                }
                            }
                        }
                    }
                }

                UIHelper.SetStatus(lblStatus,
                    dgvCurrentPrivs.Rows.Count + " quyen dang duoc cap cho " + grantee + ".",
                    StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }

        // ── Thu hồi quyền đang chọn ───────────────────────────────────────
        // Đọc dòng được chọn → xác định loại → gọi đúng SP:
        //   "Doi tuong" / "Cot" → SP_REVOKE_OBJ_PRIV(privilege, owner, object, grantee, columns)
        //   "He thong"          → SP_REVOKE_SYS_PRIV(privilege, grantee)
        //   "Role"              → SP_REVOKE_ROLE(role, grantee)
        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (dgvCurrentPrivs.SelectedRows.Count == 0)
            {
                UIHelper.SetStatus(lblStatus, "Vui long chon quyen can thu hoi!", StatusType.Warning);
                return;
            }
            if (cmbTarget.SelectedItem == null) return;

            DataGridViewRow row = dgvCurrentPrivs.SelectedRows[0];
            string grantee = cmbTarget.SelectedItem.ToString();
            string privType = row.Cells["PRIV_TYPE"].Value?.ToString() ?? "";
            string privilege = row.Cells["PRIVILEGE"].Value?.ToString() ?? "";
            string owner = row.Cells["OBJECT_OWNER"].Value?.ToString() ?? "";
            string objName = row.Cells["OBJECT_NAME"].Value?.ToString() ?? "";
            string columns = row.Cells["COLUMNS"].Value?.ToString() ?? "";

            // Xây thông báo xác nhận
            string confirmMsg;
            if (privType == "Role")
                confirmMsg = $"Thu hoi role \"{privilege}\" tu \"{grantee}\"?";
            else if (privType == "He thong")
                confirmMsg = $"Thu hoi quyen he thong \"{privilege}\" tu \"{grantee}\"?";
            else if (privType == "Cot")
                confirmMsg = $"Thu hoi quyen \"{privilege}\" tren cot \"{columns}\" cua \"{owner}.{objName}\" tu \"{grantee}\"?";
            else
                confirmMsg = $"Thu hoi quyen \"{privilege}\" tren \"{owner}.{objName}\" tu \"{grantee}\"?";

            if (MessageBox.Show(
                    confirmMsg + "\nHanh dong nay khong the hoan tac!",
                    "Xac nhan thu hoi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    if (privType == "Role")
                    {
                        // SP_REVOKE_ROLE(p_role, p_grantee)
                        using (var cmd = new OracleCommand("SP_REVOKE_ROLE", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = privilege;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (privType == "He thong")
                    {
                        // SP_REVOKE_SYS_PRIV(p_privilege, p_grantee)
                        using (var cmd = new OracleCommand("SP_REVOKE_SYS_PRIV", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_privilege", OracleDbType.Varchar2).Value = privilege;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // "Doi tuong" hoặc "Cot"
                        // SP_REVOKE_OBJ_PRIV(p_privilege, p_object_owner, p_object_name,
                        //                    p_grantee,   p_columns DEFAULT NULL)
                        // p_columns chỉ truyền khi privType == "Cot"
                        using (var cmd = new OracleCommand("SP_REVOKE_OBJ_PRIV", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_privilege", OracleDbType.Varchar2).Value = privilege;
                            cmd.Parameters.Add("p_object_owner", OracleDbType.Varchar2).Value = owner;
                            cmd.Parameters.Add("p_object_name", OracleDbType.Varchar2).Value = objName;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;

                            var colParam = cmd.Parameters.Add("p_columns", OracleDbType.Varchar2);
                            colParam.Value = (privType == "Cot" && !string.IsNullOrEmpty(columns))
                                ? (object)columns
                                : DBNull.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                UIHelper.SetStatus(lblStatus, "Da thu hoi quyen thanh cong!", StatusType.Success);
                LoadPrivileges(); // refresh grid
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }


    // ══════════════════════════════════════════════════════════════════════════
    // VIEW PRIVILEGE PANEL
    // Yêu cầu 5: Xem toàn bộ quyền của user hoặc role
    //
    // Flow:
    //   1. Chọn Loại (User / Role) → cmbViewType
    //   2. Chọn tên                → cmbViewTarget (load từ FN_LIST_USERS / FN_LIST_ROLES)
    //   3. Bấm "Xem quyền" → LoadPrivileges()
    //      Tab "Quyen doi tuong" : FN_GET_OBJ_PRIVS  (OBJECT_TYPE != 'COLUMN')
    //      Tab "Quyen theo cot"  : FN_GET_OBJ_PRIVS  (OBJECT_TYPE == 'COLUMN')
    //      Tab "Quyen he thong"  : FN_GET_SYS_PRIVS
    //      Tab "Role duoc cap"   : FN_GET_ROLE_PRIVS
    // ══════════════════════════════════════════════════════════════════════════
    public class ViewPrivilegePanel : UserControl
    {
        private ComboBox cmbViewType, cmbViewTarget;
        private TabControl tabResults;
        private DataGridView dgvObjectPrivs, dgvSysPrivs, dgvRolePrivs, dgvColPrivs;
        private Button btnView;
        private Label lblCount;
        private readonly string _connStr;

        public ViewPrivilegePanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadTargets();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Xem quyen",
                "Xem toan bo quyen dang duoc cap cho user hoac role");
            this.Controls.Add(header);

            // ── Card tìm kiếm ──────────────────────────────────────────────
            var card = UIHelper.CreateCard(10, 65, 700, 90, "TIM KIEM");
            this.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loai", 10, 30, 110, out cmbViewType);
            cmbViewType.Items.AddRange(new object[] { "User", "Role" });
            cmbViewType.SelectedIndex = 0;
            cmbViewType.SelectedIndexChanged += (s, e) => LoadTargets();

            UIHelper.CreateLabeledCombo(card, "Ten User / Role", 140, 30, 240, out cmbViewTarget);

            btnView = UIHelper.CreateButton("Xem quyen", ButtonStyle.Primary);
            btnView.Size = new Size(130, 34);
            btnView.Location = new Point(400, 36);
            card.Controls.Add(btnView);
            btnView.Click += (s, e) => LoadPrivileges();

            lblCount = new Label
            {
                Location = new Point(10, 163),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            this.Controls.Add(lblCount);

            // ── TabControl kết quả ─────────────────────────────────────────
            tabResults = new TabControl
            {
                Location = new Point(10, 182),
                Size = new Size(700, 340),
                Font = new Font("Segoe UI", 9f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyen doi tuong  ", out dgvObjectPrivs,
                new[] { "OWNER", "OBJECT_NAME", "OBJECT_TYPE", "PRIVILEGE", "GRANTABLE" },
                new[] { "Schema", "Doi tuong", "Kieu", "Quyen", "Grant Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyen theo cot  ", out dgvColPrivs,
                new[] { "OWNER", "OBJECT_NAME", "COLUMN_NAME", "PRIVILEGE", "GRANTABLE" },
                new[] { "Schema", "Bang/View", "Cot", "Quyen", "Grant Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyen he thong  ", out dgvSysPrivs,
                new[] { "PRIVILEGE", "ADMIN_OPT" },
                new[] { "Quyen he thong", "Admin Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Role duoc cap  ", out dgvRolePrivs,
                new[] { "GRANTED_ROLE", "ADMIN_OPTION", "DEFAULT_ROLE" },
                new[] { "Role", "Admin Option", "Default" }));

            this.Controls.Add(tabResults);
        }

        private TabPage BuildPrivTab(string title, out DataGridView dgv,
            string[] colNames, string[] headers)
        {
            var tab = new TabPage(title) { BackColor = UIHelper.CardBg };
            var grid = UIHelper.CreateGrid();
            grid.Dock = DockStyle.Fill;
            for (int i = 0; i < colNames.Length; i++)
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = colNames[i],
                    HeaderText = headers[i]
                });
            tab.Controls.Add(grid);
            dgv = grid;
            return tab;
        }

        // ── Load danh sách User hoặc Role vào cmbViewTarget ───────────────
        private void LoadTargets()
        {
            cmbViewTarget.Items.Clear();
            bool isUser = cmbViewType.SelectedIndex == 0;

            try
            {
                string sql = isUser
                    ? "SELECT USERNAME FROM TABLE(FN_LIST_USERS) ORDER BY USERNAME"
                    : "SELECT ROLE FROM TABLE(FN_LIST_ROLES) ORDER BY ROLE";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            cmbViewTarget.Items.Add(reader[0].ToString());
                }

                if (cmbViewTarget.Items.Count > 0)
                    cmbViewTarget.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblCount.Text = "Loi tai danh sach: " + ex.Message;
                lblCount.ForeColor = UIHelper.Danger;
            }
        }

        // ── Load quyền thật từ Oracle ──────────────────────────────────────
        private void LoadPrivileges()
        {
            if (cmbViewTarget.SelectedItem == null) return;
            string target = cmbViewTarget.SelectedItem.ToString();

            dgvObjectPrivs.Rows.Clear();
            dgvColPrivs.Rows.Clear();
            dgvSysPrivs.Rows.Clear();
            dgvRolePrivs.Rows.Clear();
            lblCount.Text = "Dang tai...";
            lblCount.ForeColor = UIHelper.TextMuted;

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    // ── Quyền đối tượng + quyền cột (FN_GET_OBJ_PRIVS) ───
                    // OBJECT_TYPE = 'COLUMN' → tab Quyen theo cot
                    // Còn lại                → tab Quyen doi tuong
                    const string sqlObj = @"
                        SELECT OWNER, OBJECT_NAME, OBJECT_TYPE,
                               PRIVILEGE, GRANTABLE, COLUMN_NAME
                        FROM   TABLE(FN_GET_OBJ_PRIVS(:t))
                        ORDER  BY OWNER, OBJECT_NAME, OBJECT_TYPE, PRIVILEGE";

                    using (var cmd = new OracleCommand(sqlObj, conn))
                    {
                        cmd.Parameters.Add("t", OracleDbType.Varchar2).Value = target;
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string objType = r["OBJECT_TYPE"]?.ToString() ?? "";
                                string colName = r["COLUMN_NAME"]?.ToString() ?? "";

                                if (objType == "COLUMN")
                                {
                                    dgvColPrivs.Rows.Add(
                                        r["OWNER"].ToString(),
                                        r["OBJECT_NAME"].ToString(),
                                        colName,
                                        r["PRIVILEGE"].ToString(),
                                        r["GRANTABLE"].ToString()
                                    );
                                }
                                else
                                {
                                    dgvObjectPrivs.Rows.Add(
                                        r["OWNER"].ToString(),
                                        r["OBJECT_NAME"].ToString(),
                                        objType,
                                        r["PRIVILEGE"].ToString(),
                                        r["GRANTABLE"].ToString()
                                    );
                                }
                            }
                        }
                    }

                    // ── Quyền hệ thống (FN_GET_SYS_PRIVS) ────────────────
                    const string sqlSys = @"
                        SELECT PRIVILEGE, ADMIN_OPT
                        FROM   TABLE(FN_GET_SYS_PRIVS(:t))
                        ORDER  BY PRIVILEGE";

                    using (var cmd = new OracleCommand(sqlSys, conn))
                    {
                        cmd.Parameters.Add("t", OracleDbType.Varchar2).Value = target;
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                dgvSysPrivs.Rows.Add(
                                    r["PRIVILEGE"].ToString(),
                                    r["ADMIN_OPT"].ToString()
                                );
                    }

                    // ── Role được cấp (FN_GET_ROLE_PRIVS) ────────────────
                    const string sqlRole = @"
                        SELECT GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
                        FROM   TABLE(FN_GET_ROLE_PRIVS(:t))
                        ORDER  BY GRANTED_ROLE";

                    using (var cmd = new OracleCommand(sqlRole, conn))
                    {
                        cmd.Parameters.Add("t", OracleDbType.Varchar2).Value = target;
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                dgvRolePrivs.Rows.Add(
                                    r["GRANTED_ROLE"].ToString(),
                                    r["ADMIN_OPTION"].ToString(),
                                    r["DEFAULT_ROLE"].ToString()
                                );
                    }
                }

                int total = dgvObjectPrivs.Rows.Count + dgvColPrivs.Rows.Count
                          + dgvSysPrivs.Rows.Count + dgvRolePrivs.Rows.Count;
                lblCount.Text = $"{target}: {dgvObjectPrivs.Rows.Count} quyen doi tuong  |  " +
                                     $"{dgvColPrivs.Rows.Count} quyen cot  |  " +
                                     $"{dgvSysPrivs.Rows.Count} quyen he thong  |  " +
                                     $"{dgvRolePrivs.Rows.Count} role  (tong: {total})";
                lblCount.ForeColor = UIHelper.Success;
            }
            catch (Exception ex)
            {
                lblCount.Text = "Loi: " + ex.Message;
                lblCount.ForeColor = UIHelper.Danger;
            }
        }
    }
}