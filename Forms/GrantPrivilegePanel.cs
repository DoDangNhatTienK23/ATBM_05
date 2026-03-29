using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class GrantPrivilegePanel : UserControl
    {
        private TabControl tabGrantType;
        private TabPage tabObjectPriv, tabRoleToUser;

        // Tab 1
        private ComboBox cmbGranteeType, cmbGrantee, cmbObjectType, cmbObjectOwner, cmbObjectName;
        private CheckedListBox clbPrivileges, clbColumns;
        private CheckBox chkGrantOption, chkAllColumns;
        private Button btnGrant;
        private Label lblStatus1;
        private Panel pnlColumnArea;

        // Tab 2
        private ComboBox cmbRoleToAssign, cmbRoleGrantee;
        private CheckBox chkAdminOption;
        private Button btnGrantRole;
        private Label lblStatus2;

        private readonly string _connStr;

        public GrantPrivilegePanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            PopulateData();
        }

        // ============================================================
        // KHỞI TẠO GIAO DIỆN
        // ============================================================

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Cap quyen",
                "Cap quyen tren doi tuong cho user/role, hoac cap role cho user");
            header.Dock = DockStyle.Top;

            tabGrantType = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f)
            };

            tabObjectPriv = new TabPage("  Quyen tren doi tuong  ");
            tabRoleToUser = new TabPage("  Cap Role cho User  ");

            BuildObjectPrivTab();
            BuildRoleToUserTab();

            tabGrantType.TabPages.Add(tabObjectPriv);
            tabGrantType.TabPages.Add(tabRoleToUser);

            // ĐÚNG THỨ TỰ trong WinForms: control Dock=Fill add TRƯỚC, Dock=Top add SAU
            // Layout engine xử lý từ cuối list lên đầu:
            // → header (Top) được xử lý sau cùng, ghim trên cùng
            // → tabGrantType (Fill) fill toàn bộ phần còn lại bên dưới
            this.Controls.Add(tabGrantType);
            this.Controls.Add(header);
        }

        private void BuildObjectPrivTab()
        {
            tabObjectPriv.BackColor = UIHelper.LightBg;

            var cardGrantee = UIHelper.CreateCard(10, 10, 560, 100, "NGUOI NHAN QUYEN");
            tabObjectPriv.Controls.Add(cardGrantee);

            UIHelper.CreateLabeledCombo(cardGrantee, "Loai", 10, 32, 130, out cmbGranteeType);
            cmbGranteeType.Items.AddRange(new object[] { "User", "Role" });
            cmbGranteeType.SelectedIndex = 0;
            cmbGranteeType.SelectedIndexChanged += (s, e) => LoadGrantees();

            UIHelper.CreateLabeledCombo(cardGrantee, "Ten User / Role", 160, 32, 370, out cmbGrantee);

            var cardObject = UIHelper.CreateCard(10, 120, 560, 100, "DOI TUONG CAP QUYEN");
            tabObjectPriv.Controls.Add(cardObject);

            UIHelper.CreateLabeledCombo(cardObject, "Loai doi tuong", 10, 32, 120, out cmbObjectType);
            cmbObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            cmbObjectType.SelectedIndex = 0;
            cmbObjectType.SelectedIndexChanged += (s, e) =>
            {
                RefreshPrivilegeList();
                LoadObjects();
            };

            // Owner cố định là BVDBA (schema duy nhất trong hệ thống)
            UIHelper.CreateLabeledCombo(cardObject, "Schema / Owner", 150, 32, 150, out cmbObjectOwner);
            cmbObjectOwner.DropDownStyle = ComboBoxStyle.DropDownList;

            UIHelper.CreateLabeledCombo(cardObject, "Ten doi tuong", 320, 32, 220, out cmbObjectName);
            cmbObjectName.SelectedIndexChanged += (s, e) => LoadColumns();

            var cardPriv = UIHelper.CreateCard(10, 230, 270, 220, "CHON QUYEN");
            tabObjectPriv.Controls.Add(cardPriv);

            clbPrivileges = new CheckedListBox
            {
                Location = new Point(10, 35),
                Size = new Size(245, 160),
                Font = new Font("Segoe UI", 9.5f),
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = UIHelper.CardBg
            };
            clbPrivileges.ItemCheck += ClbPrivileges_ItemCheck;
            cardPriv.Controls.Add(clbPrivileges);

            pnlColumnArea = UIHelper.CreateCard(290, 230, 280, 220, "PHAN QUYEN DEN COT");
            tabObjectPriv.Controls.Add(pnlColumnArea);

            chkAllColumns = new CheckBox
            {
                Text = "Tat ca cac cot",
                Location = new Point(10, 35),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted
            };
            chkAllColumns.CheckedChanged += (s, e) => clbColumns.Enabled = !chkAllColumns.Checked;
            pnlColumnArea.Controls.Add(chkAllColumns);

            clbColumns = new CheckedListBox
            {
                Location = new Point(10, 60),
                Size = new Size(255, 130),
                Font = new Font("Segoe UI", 9f),
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = UIHelper.LightBg
            };
            pnlColumnArea.Controls.Add(clbColumns);

            chkGrantOption = new CheckBox
            {
                Text = "WITH GRANT OPTION (nguoi duoc cap co the cap lai cho nguoi khac)",
                Location = new Point(10, 460),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted
            };
            tabObjectPriv.Controls.Add(chkGrantOption);

            btnGrant = UIHelper.CreateButton("THUC HIEN CAP QUYEN", ButtonStyle.Success);
            btnGrant.Size = new Size(210, 38);
            btnGrant.Location = new Point(360, 490);
            btnGrant.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnGrant.Click += BtnGrant_Click;
            tabObjectPriv.Controls.Add(btnGrant);

            lblStatus1 = new Label
            {
                Location = new Point(10, 500),
                Size = new Size(340, 20),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            tabObjectPriv.Controls.Add(lblStatus1);
        }

        private void BuildRoleToUserTab()
        {
            tabRoleToUser.BackColor = UIHelper.LightBg;

            var card = UIHelper.CreateCard(10, 10, 560, 200, "CAP ROLE CHO USER");
            tabRoleToUser.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Role can cap", 10, 32, 250, out cmbRoleToAssign);
            UIHelper.CreateLabeledCombo(card, "Cap cho User", 280, 32, 260, out cmbRoleGrantee);

            chkAdminOption = new CheckBox
            {
                Text = "WITH ADMIN OPTION (user co the cap role nay cho nguoi khac)",
                Location = new Point(10, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted
            };
            card.Controls.Add(chkAdminOption);

            var infoPanel = new Panel
            {
                Location = new Point(10, 120),
                Size = new Size(530, 50),
                BackColor = Color.FromArgb(230, 240, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            infoPanel.Controls.Add(new Label
            {
                Text = "WITH ADMIN OPTION cho phep nguoi nhan role tiep tuc cap role do\n" +
                       "cho user hoac role khac. Tuong tu WITH GRANT OPTION nhung cho role.",
                Location = new Point(8, 6),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(30, 70, 150)
            });
            card.Controls.Add(infoPanel);

            btnGrantRole = UIHelper.CreateButton("CAP ROLE", ButtonStyle.Success);
            btnGrantRole.Size = new Size(130, 38);
            btnGrantRole.Location = new Point(440, 225);
            btnGrantRole.Click += BtnGrantRole_Click;
            tabRoleToUser.Controls.Add(btnGrantRole);

            lblStatus2 = new Label
            {
                Location = new Point(10, 235),
                Size = new Size(420, 20),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            tabRoleToUser.Controls.Add(lblStatus2);
        }

        // ============================================================
        // LOAD DỮ LIỆU TỪ ORACLE
        // ============================================================

        /// <summary>
        /// Gọi khi form load lần đầu: điền tất cả combo cần thiết.
        /// </summary>
        private void PopulateData()
        {
            // Owner cố định là BVDBA — chỉ schema của hệ thống
            cmbObjectOwner.Items.Add("BVDBA");
            cmbObjectOwner.SelectedIndex = 0;

            // Điền các combo phụ thuộc DB
            LoadGrantees();          // Tab 1: danh sách user/role nhận quyền
            LoadObjects();           // Tab 1: danh sách objects
            RefreshPrivilegeList();  // Tab 1: các quyền tương ứng loại object

            LoadRoles();             // Tab 2: danh sách role để cấp
            LoadUsers();             // Tab 2: danh sách user nhận role
        }

        /// <summary>
        /// Load danh sách User hoặc Role vào cmbGrantee (Tab 1).
        /// Gọi: SELECT * FROM TABLE(FN_LIST_USERS) hoặc FN_LIST_ROLES
        /// </summary>
        private void LoadGrantees()
        {
            cmbGrantee.Items.Clear();
            bool isUser = cmbGranteeType.SelectedItem?.ToString() == "User";

            string sql = isUser
                ? "SELECT USERNAME      AS NAME FROM TABLE(BVDBA.FN_LIST_USERS) ORDER BY NAME"
                : "SELECT ROLE          AS NAME FROM TABLE(BVDBA.FN_LIST_ROLES) ORDER BY NAME";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbGrantee.Items.Add(reader.GetString(0));
                    }
                }
                if (cmbGrantee.Items.Count > 0)
                    cmbGrantee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi tai danh sach grantee: " + ex.Message, StatusType.Error);
            }
        }

        /// <summary>
        /// Load danh sách objects theo loại đang chọn vào cmbObjectName (Tab 1).
        /// Gọi: SELECT * FROM TABLE(FN_LIST_OBJECTS) WHERE OBJECT_TYPE = :t
        /// </summary>
        private void LoadObjects()
        {
            cmbObjectName.Items.Clear();
            string objType = cmbObjectType.SelectedItem?.ToString() ?? "TABLE";

            const string sql =
                "SELECT OBJECT_NAME FROM TABLE(BVDBA.FN_LIST_OBJECTS) " +
                "WHERE OBJECT_TYPE = :objType ORDER BY OBJECT_NAME";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("objType", OracleDbType.Varchar2).Value = objType;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                cmbObjectName.Items.Add(reader.GetString(0));
                        }
                    }
                }
                if (cmbObjectName.Items.Count > 0)
                    cmbObjectName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi tai danh sach doi tuong: " + ex.Message, StatusType.Error);
            }
        }

        /// <summary>
        /// Load danh sách cột của bảng/view đang chọn vào clbColumns (Tab 1).
        /// Gọi: SELECT * FROM TABLE(FN_LIST_COLUMNS(:objectName))
        /// Chỉ áp dụng cho TABLE và VIEW.
        /// </summary>
        private void LoadColumns()
        {
            clbColumns.Items.Clear();
            string objType = cmbObjectType.SelectedItem?.ToString() ?? "";
            string objName = cmbObjectName.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(objName) || (objType != "TABLE" && objType != "VIEW"))
                return;

            const string sql =
                "SELECT COLUMN_NAME FROM TABLE(BVDBA.FN_LIST_COLUMNS(:objName)) ORDER BY 1";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("objName", OracleDbType.Varchar2).Value = objName;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                clbColumns.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi tai danh sach cot: " + ex.Message, StatusType.Error);
            }
        }

        /// <summary>
        /// Load danh sách roles vào cmbRoleToAssign (Tab 2).
        /// Gọi: SELECT * FROM TABLE(FN_LIST_ROLES)
        /// </summary>
        private void LoadRoles()
        {
            cmbRoleToAssign.Items.Clear();
            const string sql = "SELECT ROLE FROM TABLE(BVDBA.FN_LIST_ROLES) ORDER BY ROLE";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbRoleToAssign.Items.Add(reader.GetString(0));
                    }
                }
                if (cmbRoleToAssign.Items.Count > 0)
                    cmbRoleToAssign.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus2, "Loi tai danh sach role: " + ex.Message, StatusType.Error);
            }
        }

        /// <summary>
        /// Load danh sách users vào cmbRoleGrantee (Tab 2).
        /// Gọi: SELECT * FROM TABLE(FN_LIST_USERS)
        /// </summary>
        private void LoadUsers()
        {
            cmbRoleGrantee.Items.Clear();
            const string sql = "SELECT USERNAME FROM TABLE(BVDBA.FN_LIST_USERS) ORDER BY USERNAME";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            cmbRoleGrantee.Items.Add(reader.GetString(0));
                    }
                }
                if (cmbRoleGrantee.Items.Count > 0)
                    cmbRoleGrantee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus2, "Loi tai danh sach user: " + ex.Message, StatusType.Error);
            }
        }

        // ============================================================
        // LOGIC HIỂN THỊ / ẨN PANEL CỘT
        // ============================================================

        private void RefreshPrivilegeList()
        {
            clbPrivileges.Items.Clear();
            pnlColumnArea.Visible = false;

            string objType = cmbObjectType.SelectedItem?.ToString() ?? "TABLE";

            if (objType == "TABLE" || objType == "VIEW")
            {
                clbPrivileges.Items.Add("SELECT");
                clbPrivileges.Items.Add("INSERT");
                clbPrivileges.Items.Add("UPDATE");
                clbPrivileges.Items.Add("DELETE");
                if (objType == "TABLE")
                {
                    clbPrivileges.Items.Add("ALTER");
                    clbPrivileges.Items.Add("INDEX");
                    clbPrivileges.Items.Add("REFERENCES");
                }
            }
            else // PROCEDURE, FUNCTION
            {
                clbPrivileges.Items.Add("EXECUTE");
            }
        }

        private void ClbPrivileges_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string item = clbPrivileges.Items[e.Index].ToString();
            if (item != "SELECT" && item != "UPDATE") return;

            bool show = e.NewValue == CheckState.Checked;

            // Nếu đang bỏ chọn, kiểm tra xem còn SELECT/UPDATE nào khác không
            if (!show)
            {
                foreach (int i in clbPrivileges.CheckedIndices)
                {
                    if (i == e.Index) continue;
                    string other = clbPrivileges.Items[i].ToString();
                    if (other == "SELECT" || other == "UPDATE") { show = true; break; }
                }
            }

            string objType = cmbObjectType.SelectedItem?.ToString() ?? "";
            pnlColumnArea.Visible = show && (objType == "TABLE" || objType == "VIEW");

            // Lazy load: chỉ gọi DB khi panel hiện ra và chưa có dữ liệu
            if (pnlColumnArea.Visible && clbColumns.Items.Count == 0)
                LoadColumns();
        }

        // ============================================================
        // XỬ LÝ SỰ KIỆN BUTTON
        // ============================================================

        /// <summary>
        /// Cấp quyền đối tượng.
        /// Gọi SP_GRANT_OBJ_PRIV cho mỗi quyền được chọn.
        /// Tham số: p_privilege, p_object_owner, p_object_name,
        ///          p_grantee, p_columns (NULL nếu toàn bảng),
        ///          p_with_grant_opt ('YES'/'NO')
        /// </summary>
        private void BtnGrant_Click(object sender, EventArgs e)
        {
            if (cmbGrantee.SelectedItem == null || cmbObjectName.SelectedItem == null)
            {
                UIHelper.SetStatus(lblStatus1, "Vui long chon day du thong tin!", StatusType.Warning);
                return;
            }
            if (clbPrivileges.CheckedItems.Count == 0)
            {
                UIHelper.SetStatus(lblStatus1, "Vui long chon it nhat mot quyen!", StatusType.Warning);
                return;
            }

            string grantee = cmbGrantee.SelectedItem.ToString();
            string owner = cmbObjectOwner.SelectedItem?.ToString() ?? "BVDBA";
            string objName = cmbObjectName.SelectedItem.ToString();
            string grantOpt = chkGrantOption.Checked ? "YES" : "NO";

            // Xây dựng danh sách cột (chỉ áp dụng khi không chọn "tất cả cột")
            string columnList = null;
            if (!chkAllColumns.Checked && clbColumns.CheckedItems.Count > 0)
            {
                var cols = new List<string>();
                foreach (string col in clbColumns.CheckedItems) cols.Add(col);
                columnList = string.Join(",", cols);
            }

            int successCount = 0;
            var errors = new List<string>();

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    foreach (string priv in clbPrivileges.CheckedItems)
                    {
                        // INSERT/DELETE/EXECUTE không hỗ trợ phân quyền cột
                        // — truyền NULL để SP tự xử lý
                        string colsForPriv = (priv == "SELECT" || priv == "UPDATE")
                            ? columnList
                            : null;

                        try
                        {
                            // Nhúng p_columns trực tiếp vào PL/SQL string để tránh lỗi ORA-00969
                            // ODP.NET không bind NULL đúng cách cho VARCHAR2 IN parameter
                            string colsLiteral = colsForPriv != null
                                ? $"'{colsForPriv}'"  // VD: 'HOTEN,SODT'
                                : "NULL";             // NULL literal, không dùng bind variable

                            string plsql =
                                "BEGIN BVDBA.SP_GRANT_OBJ_PRIV(" +
                                "  p_privilege      => :p_privilege," +
                                "  p_object_owner   => :p_object_owner," +
                                "  p_object_name    => :p_object_name," +
                                "  p_grantee        => :p_grantee," +
                               $"  p_columns        => {colsLiteral}," +
                                "  p_with_grant_opt => :p_with_grant_opt" +
                                "); END;";

                            using (var cmd = new OracleCommand(plsql, conn))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add("p_privilege", OracleDbType.Varchar2).Value = priv;
                                cmd.Parameters.Add("p_object_owner", OracleDbType.Varchar2).Value = owner;
                                cmd.Parameters.Add("p_object_name", OracleDbType.Varchar2).Value = objName;
                                cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                                cmd.Parameters.Add("p_with_grant_opt", OracleDbType.Varchar2).Value = grantOpt;
                                cmd.ExecuteNonQuery();
                                successCount++;
                            }
                        }
                        catch (OracleException oex)
                        {
                            // Ghi nhận lỗi từng quyền nhưng tiếp tục cấp các quyền còn lại
                            errors.Add($"{priv}: {oex.Message}");
                        }
                    }
                }

                if (errors.Count == 0)
                {
                    UIHelper.SetStatus(lblStatus1,
                        $"Da cap {successCount} quyen cho {grantee} tren {owner}.{objName}",
                        StatusType.Success);
                }
                else if (successCount > 0)
                {
                    UIHelper.SetStatus(lblStatus1,
                        $"Cap duoc {successCount} quyen; loi: {string.Join("; ", errors)}",
                        StatusType.Warning);
                }
                else
                {
                    UIHelper.SetStatus(lblStatus1, "Loi: " + string.Join("; ", errors), StatusType.Error);
                }
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi ket noi: " + ex.Message, StatusType.Error);
            }
        }

        /// <summary>
        /// Cấp role cho user.
        /// Gọi SP_GRANT_ROLE(p_role, p_grantee, p_with_admin_opt)
        /// </summary>
        private void BtnGrantRole_Click(object sender, EventArgs e)
        {
            string role = cmbRoleToAssign.SelectedItem?.ToString();
            string user = cmbRoleGrantee.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(user))
            {
                UIHelper.SetStatus(lblStatus2, "Vui long chon day du thong tin!", StatusType.Warning);
                return;
            }

            string adminOpt = chkAdminOption.Checked ? "YES" : "NO";

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string plsql =
                        "BEGIN BVDBA.SP_GRANT_ROLE(" +
                        "  p_role           => :p_role," +
                        "  p_grantee        => :p_grantee," +
                        "  p_with_admin_opt => :p_with_admin_opt" +
                        "); END;";

                    using (var cmd = new OracleCommand(plsql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = role;
                        cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = user;
                        cmd.Parameters.Add("p_with_admin_opt", OracleDbType.Varchar2).Value = adminOpt;
                        cmd.ExecuteNonQuery();
                    }
                }
                UIHelper.SetStatus(lblStatus2,
                    $"Da cap role {role} cho {user}" +
                    (chkAdminOption.Checked ? " (WITH ADMIN OPTION)" : ""),
                    StatusType.Success);
            }
            catch (OracleException oex)
            {
                UIHelper.SetStatus(lblStatus2, "Loi Oracle: " + oex.Message, StatusType.Error);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus2, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }
}