using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
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
            LoadTargets();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;
            this.Padding = new Padding(10);

            var header = UIHelper.CreateSectionHeader(
                "Thu hồi quyền",
                "Xem và thu hồi quyền đang được cấp cho user hoặc role");

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Color.Transparent
            };

            var card = UIHelper.CreateCard(0, 0, 720, 105, "CHỌN ĐỐI TƯỢNG");
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlTop.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loại", 10, 32, 110, out cmbTargetType);
            cmbTargetType.Items.AddRange(new object[] { "User", "Role" });
            cmbTargetType.SelectedIndex = 0;
            cmbTargetType.SelectedIndexChanged += (s, e) => LoadTargets();

            UIHelper.CreateLabeledCombo(card, "Tên User / Role", 140, 32, 220, out cmbTarget);

            UIHelper.CreateLabeledCombo(card, "Loại quyền hiển thị", 380, 32, 220, out cmbPrivType);
            cmbPrivType.Items.AddRange(new object[] {
                "Tất cả",
                "Quyền trên đối tượng",
                "Quyền hệ thống",
                "Role được cấp"
            });
            cmbPrivType.SelectedIndex = 0;

            btnLoadPrivs = UIHelper.CreateButton("Tải danh sách quyền", ButtonStyle.Secondary);
            btnLoadPrivs.Size = new Size(180, 34);
            btnLoadPrivs.Location = new Point(0, 115);
            btnLoadPrivs.ForeColor = UIHelper.TextDark;
            btnLoadPrivs.Click += (s, e) => LoadPrivileges();
            pnlTop.Controls.Add(btnLoadPrivs);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Transparent
            };

            btnRevoke = UIHelper.CreateButton("THU HỒI QUYỀN ĐÃ CHỌN", ButtonStyle.Danger);
            btnRevoke.Size = new Size(220, 36);
            btnRevoke.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnRevoke.Click += BtnRevoke_Click;
            pnlBottom.Controls.Add(btnRevoke);
            pnlBottom.Resize += (s, e) =>
                btnRevoke.Location = new Point(pnlBottom.Width - 230, 7);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvCurrentPrivs = UIHelper.CreateGrid();
            dgvCurrentPrivs.Dock = DockStyle.Fill;
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIV_TYPE", HeaderText = "Loại quyền", FillWeight = 14 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIVILEGE", HeaderText = "Tên quyền / Role", FillWeight = 20 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_OWNER", HeaderText = "Schema", FillWeight = 13 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_NAME", HeaderText = "Tên đối tượng", FillWeight = 18 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_TYPE", HeaderText = "Loại đối tượng", FillWeight = 12 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "COLUMNS", HeaderText = "Cột (nếu có)", FillWeight = 13 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "GRANT_OPTION", HeaderText = "Tùy chọn cấp", FillWeight = 10 });

            dgvCurrentPrivs.CellFormatting += DgvPrivs_CellFormatting;
            pnlGrid.Controls.Add(dgvCurrentPrivs);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
            this.Controls.Add(header);
        }

        private void DgvPrivs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCurrentPrivs.Columns[e.ColumnIndex].Name == "PRIV_TYPE" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "Đối tượng": e.CellStyle.ForeColor = UIHelper.Primary; break;
                    case "Cột": e.CellStyle.ForeColor = Color.DarkCyan; break;
                    case "Hệ thống": e.CellStyle.ForeColor = UIHelper.Warning; break;
                    case "Role": e.CellStyle.ForeColor = UIHelper.Success; break;
                }
            }
        }

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
                UIHelper.SetStatus(lblStatus, "Lỗi tải danh sách: " + ex.Message, StatusType.Error);
            }
        }

        private void LoadPrivileges()
        {
            if (cmbTarget.SelectedItem == null)
            {
                UIHelper.SetStatus(lblStatus, "Vui lòng chọn User hoặc Role!", StatusType.Warning);
                return;
            }

            dgvCurrentPrivs.Rows.Clear();
            string grantee = cmbTarget.SelectedItem.ToString();
            string privType = cmbPrivType.SelectedItem?.ToString() ?? "Tất cả";
            bool showObj = privType == "Tất cả" || privType == "Quyền trên đối tượng";
            bool showSys = privType == "Tất cả" || privType == "Quyền hệ thống";
            bool showRole = privType == "Tất cả" || privType == "Role được cấp";

            UIHelper.SetStatus(lblStatus, "Đang tải...", StatusType.Info);

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

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
                                    string privLabel = objType == "COLUMN" ? "Cột" : "Đối tượng";

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
                                        "Hệ thống",
                                        r["PRIVILEGE"].ToString(),
                                        "", "", "SYSTEM", "",
                                        r["ADMIN_OPT"].ToString()
                                    );
                                }
                            }
                        }
                    }

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
                    dgvCurrentPrivs.Rows.Count + " quyền đang được cấp cho " + grantee + ".",
                    StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (dgvCurrentPrivs.SelectedRows.Count == 0)
            {
                UIHelper.SetStatus(lblStatus, "Vui lòng chọn quyền cần thu hồi!", StatusType.Warning);
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

            string confirmMsg;
            if (privType == "Role")
                confirmMsg = $"Thu hồi role \"{privilege}\" từ \"{grantee}\"?";
            else if (privType == "Hệ thống")
                confirmMsg = $"Thu hồi quyền hệ thống \"{privilege}\" từ \"{grantee}\"?";
            else if (privType == "Cột")
                confirmMsg = $"Thu hồi quyền \"{privilege}\" trên cột \"{columns}\" của \"{owner}.{objName}\" từ \"{grantee}\"?";
            else
                confirmMsg = $"Thu hồi quyền \"{privilege}\" trên \"{owner}.{objName}\" từ \"{grantee}\"?";

            if (MessageBox.Show(
                    confirmMsg + "\nHành động này không thể hoàn tác!",
                    "Xác nhận thu hồi",
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
                        using (var cmd = new OracleCommand("SP_REVOKE_ROLE", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = privilege;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (privType == "Hệ thống")
                    {
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
                        using (var cmd = new OracleCommand("SP_REVOKE_OBJ_PRIV", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_privilege", OracleDbType.Varchar2).Value = privilege;
                            cmd.Parameters.Add("p_object_owner", OracleDbType.Varchar2).Value = owner;
                            cmd.Parameters.Add("p_object_name", OracleDbType.Varchar2).Value = objName;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;

                            var colParam = cmd.Parameters.Add("p_columns", OracleDbType.Varchar2);
                            colParam.Value = (privType == "Cột" && !string.IsNullOrEmpty(columns))
                                ? (object)columns
                                : DBNull.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                UIHelper.SetStatus(lblStatus, "Đã thu hồi quyền thành công!", StatusType.Success);
                LoadPrivileges();
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }
    }


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
                "Xem quyền",
                "Xem toàn bộ quyền đang được cấp cho user hoặc role");
            this.Controls.Add(header);

            var card = UIHelper.CreateCard(10, 65, 700, 90, "TÌM KIẾM");
            this.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loại", 10, 30, 110, out cmbViewType);
            cmbViewType.Items.AddRange(new object[] { "User", "Role" });
            cmbViewType.SelectedIndex = 0;
            cmbViewType.SelectedIndexChanged += (s, e) => LoadTargets();

            UIHelper.CreateLabeledCombo(card, "Tên User / Role", 140, 30, 240, out cmbViewTarget);

            btnView = UIHelper.CreateButton("Xem quyền", ButtonStyle.Primary);
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

            tabResults = new TabControl
            {
                Location = new Point(10, 182),
                Size = new Size(700, 340),
                Font = new Font("Segoe UI", 9f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyền đối tượng  ", out dgvObjectPrivs,
                new[] { "OWNER", "OBJECT_NAME", "OBJECT_TYPE", "PRIVILEGE", "GRANTABLE" },
                new[] { "Schema", "Đối tượng", "Kiểu", "Quyền", "Grant Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyền theo cột  ", out dgvColPrivs,
                new[] { "OWNER", "OBJECT_NAME", "COLUMN_NAME", "PRIVILEGE", "GRANTABLE" },
                new[] { "Schema", "Bảng/View", "Cột", "Quyền", "Grant Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Quyền hệ thống  ", out dgvSysPrivs,
                new[] { "PRIVILEGE", "ADMIN_OPT" },
                new[] { "Quyền hệ thống", "Admin Option" }));

            tabResults.TabPages.Add(BuildPrivTab(
                "  Role được cấp  ", out dgvRolePrivs,
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
                lblCount.Text = "Lỗi tải danh sách: " + ex.Message;
                lblCount.ForeColor = UIHelper.Danger;
            }
        }

        private void LoadPrivileges()
        {
            if (cmbViewTarget.SelectedItem == null) return;
            string target = cmbViewTarget.SelectedItem.ToString();

            dgvObjectPrivs.Rows.Clear();
            dgvColPrivs.Rows.Clear();
            dgvSysPrivs.Rows.Clear();
            dgvRolePrivs.Rows.Clear();
            lblCount.Text = "Đang tải...";
            lblCount.ForeColor = UIHelper.TextMuted;

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

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
                lblCount.Text = $"{target}: {dgvObjectPrivs.Rows.Count} quyền đối tượng  |  " +
                                     $"{dgvColPrivs.Rows.Count} quyền cột  |  " +
                                     $"{dgvSysPrivs.Rows.Count} quyền hệ thống  |  " +
                                     $"{dgvRolePrivs.Rows.Count} role  (tổng: {total})";
                lblCount.ForeColor = UIHelper.Success;
            }
            catch (Exception ex)
            {
                lblCount.Text = "Lỗi: " + ex.Message;
                lblCount.ForeColor = UIHelper.Danger;
            }
        }
    }
}
