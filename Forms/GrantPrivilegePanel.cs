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

        // Tab 1 - Object privilege
        private ComboBox cmbGranteeType, cmbGrantee, cmbObjectType, cmbObjectOwner, cmbObjectName;
        private CheckedListBox clbPrivileges, clbColumns;
        private CheckBox chkGrantOption, chkAllColumns;
        private Button btnGrant;
        private Label lblStatus1;
        private Panel pnlColumnArea;
        private DataGridView dgvObjectPrivileges;
        private DataGridView dgvGrantOptionOnly;

        // Giữ reference để layout động
        private Panel cardGridAllObject;
        private Panel cardGridGrantOptionObject;

        // Tab 2 - Role to user
        private ComboBox cmbRoleToAssign, cmbRoleGrantee;
        private CheckBox chkAdminOption;
        private Button btnGrantRole;
        private Label lblStatus2;
        private DataGridView dgvRolePrivileges;
        private DataGridView dgvAdminOptionOnly;

        private readonly string _connStr;

        public GrantPrivilegePanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadInitialData();
        }

        public void RefreshRolesList()
        {
            LoadRolesForRoleTab();
            LoadUsersForRoleTab();
        }

        private void InitializeLayout()
        {
            Dock = DockStyle.Fill;
            BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Cap quyen",
                "Cap quyen tren doi tuong cho user/role, hoac cap role cho user");

            tabGrantType = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(0, header.Bottom + 5)
            };

            tabObjectPriv = new TabPage("  Quyen tren doi tuong  ");
            tabRoleToUser = new TabPage("  Cap Role cho User  ");

            BuildObjectPrivTab();
            BuildRoleToUserTab();

            tabGrantType.TabPages.Add(tabObjectPriv);
            tabGrantType.TabPages.Add(tabRoleToUser);

            Controls.Add(tabGrantType);
            Controls.Add(header);
        }

        private void BuildObjectPrivTab()
        {
            tabObjectPriv.BackColor = UIHelper.LightBg;

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UIHelper.LightBg
            };
            scrollPanel.AutoScrollMinSize = new Size(1260, 900);
            tabObjectPriv.Controls.Add(scrollPanel);

            var cardGrantee = UIHelper.CreateCard(10, 10, 700, 100, "NGUOI NHAN QUYEN");
            scrollPanel.Controls.Add(cardGrantee);

            UIHelper.CreateLabeledCombo(cardGrantee, "Loai", 10, 32, 130, out cmbGranteeType);
            cmbGranteeType.Items.AddRange(new object[] { "User", "Role" });
            cmbGranteeType.SelectedIndex = 0;
            cmbGranteeType.SelectedIndexChanged += (s, e) =>
            {
                LoadGrantees();
                UpdateGrantOptionState();
                LoadObjectPrivilegeGrid();
                LoadGrantOptionGrid();
            };

            UIHelper.CreateLabeledCombo(cardGrantee, "Ten User / Role", 160, 32, 500, out cmbGrantee);
            cmbGrantee.SelectedIndexChanged += (s, e) =>
            {
                LoadObjectPrivilegeGrid();
                LoadGrantOptionGrid();
            };

            var cardObject = UIHelper.CreateCard(10, 120, 700, 100, "DOI TUONG CAP QUYEN");
            scrollPanel.Controls.Add(cardObject);

            UIHelper.CreateLabeledCombo(cardObject, "Loai doi tuong", 10, 32, 120, out cmbObjectType);
            cmbObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            cmbObjectType.SelectedIndex = 0;
            cmbObjectType.SelectedIndexChanged += (s, e) =>
            {
                RefreshPrivilegeList();
                LoadObjects();
                LoadColumns();
                UpdateObjectPrivLayout();
            };

            UIHelper.CreateLabeledCombo(cardObject, "Schema / Owner", 150, 32, 150, out cmbObjectOwner);
            cmbObjectOwner.SelectedIndexChanged += (s, e) =>
            {
                LoadObjects();
                LoadColumns();
            };

            UIHelper.CreateLabeledCombo(cardObject, "Ten doi tuong", 320, 32, 340, out cmbObjectName);
            cmbObjectName.SelectedIndexChanged += (s, e) => LoadColumns();

            var cardPriv = UIHelper.CreateCard(10, 230, 270, 190, "CHON QUYEN");
            scrollPanel.Controls.Add(cardPriv);

            clbPrivileges = new CheckedListBox
            {
                Location = new Point(10, 35),
                Size = new Size(245, 130),
                Font = new Font("Segoe UI", 9.5f),
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = UIHelper.CardBg
            };
            clbPrivileges.ItemCheck += ClbPrivileges_ItemCheck;
            cardPriv.Controls.Add(clbPrivileges);

            pnlColumnArea = UIHelper.CreateCard(290, 230, 420, 190, "PHAN QUYEN DEN COT");
            pnlColumnArea.Visible = false;
            scrollPanel.Controls.Add(pnlColumnArea);

            chkAllColumns = new CheckBox
            {
                Text = "Tat ca cac cot",
                Location = new Point(10, 35),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted,
                Checked = true
            };
            chkAllColumns.CheckedChanged += (s, e) => clbColumns.Enabled = !chkAllColumns.Checked;
            pnlColumnArea.Controls.Add(chkAllColumns);

            clbColumns = new CheckedListBox
            {
                Location = new Point(10, 60),
                Size = new Size(390, 100),
                Font = new Font("Segoe UI", 9f),
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = UIHelper.LightBg,
                Enabled = false
            };
            pnlColumnArea.Controls.Add(clbColumns);

            chkGrantOption = new CheckBox
            {
                Text = "WITH GRANT OPTION (nguoi duoc cap co the cap lai cho nguoi khac)",
                Location = new Point(10, 430),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted
            };
            scrollPanel.Controls.Add(chkGrantOption);

            btnGrant = UIHelper.CreateButton("THUC HIEN CAP QUYEN", ButtonStyle.Success);
            btnGrant.Size = new Size(210, 38);
            btnGrant.Location = new Point(500, 425);
            btnGrant.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnGrant.Click += BtnGrant_Click;
            scrollPanel.Controls.Add(btnGrant);

            lblStatus1 = new Label
            {
                Location = new Point(10, 470),
                Size = new Size(680, 35),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            scrollPanel.Controls.Add(lblStatus1);

            cardGridAllObject = UIHelper.CreateCard(10, 510, 700, 190, "QUYEN OBJECT HIEN TAI");
            scrollPanel.Controls.Add(cardGridAllObject);

            dgvObjectPrivileges = CreateGrid(new Size(680, 150), new Point(10, 30));
            cardGridAllObject.Controls.Add(dgvObjectPrivileges);

            cardGridGrantOptionObject = UIHelper.CreateCard(720, 510, 520, 190, "CAC DONG CO WITH GRANT OPTION");
            scrollPanel.Controls.Add(cardGridGrantOptionObject);

            dgvGrantOptionOnly = CreateGrid(new Size(500, 150), new Point(10, 30));
            cardGridGrantOptionObject.Controls.Add(dgvGrantOptionOnly);

            UpdateObjectPrivLayout();
        }

        private void BuildRoleToUserTab()
        {
            tabRoleToUser.BackColor = UIHelper.LightBg;

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UIHelper.LightBg
            };
            scrollPanel.AutoScrollMinSize = new Size(1260, 560);
            tabRoleToUser.Controls.Add(scrollPanel);

            var card = UIHelper.CreateCard(10, 10, 700, 200, "CAP ROLE CHO USER");
            scrollPanel.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Role can cap", 10, 32, 300, out cmbRoleToAssign);
            UIHelper.CreateLabeledCombo(card, "Cap cho User", 330, 32, 330, out cmbRoleGrantee);

            cmbRoleGrantee.SelectedIndexChanged += (s, e) =>
            {
                LoadRolePrivilegeGrid();
                LoadAdminOptionGrid();
            };

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
                Size = new Size(650, 50),
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
            btnGrantRole.Location = new Point(580, 220);
            btnGrantRole.Click += BtnGrantRole_Click;
            scrollPanel.Controls.Add(btnGrantRole);

            lblStatus2 = new Label
            {
                Location = new Point(10, 225),
                Size = new Size(560, 35),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            scrollPanel.Controls.Add(lblStatus2);

            var cardGridAll = UIHelper.CreateCard(10, 275, 700, 220, "ROLE HIEN TAI CUA USER");
            scrollPanel.Controls.Add(cardGridAll);

            dgvRolePrivileges = CreateGrid(new Size(680, 180), new Point(10, 30));
            cardGridAll.Controls.Add(dgvRolePrivileges);

            var cardGridAdminOnly = UIHelper.CreateCard(720, 275, 520, 220, "CAC DONG CO WITH ADMIN OPTION");
            scrollPanel.Controls.Add(cardGridAdminOnly);

            dgvAdminOptionOnly = CreateGrid(new Size(500, 180), new Point(10, 30));
            cardGridAdminOnly.Controls.Add(dgvAdminOptionOnly);
        }

        private void UpdateObjectPrivLayout()
        {
            int nextTop;

            if (pnlColumnArea.Visible)
            {
                nextTop = pnlColumnArea.Bottom + 10;
            }
            else
            {
                nextTop = 230 + 190 + 10; // cardPriv.Bottom + spacing
            }

            chkGrantOption.Location = new Point(10, nextTop);
            btnGrant.Location = new Point(500, nextTop - 5);
            lblStatus1.Location = new Point(10, nextTop + 40);

            cardGridAllObject.Location = new Point(10, nextTop + 80);
            cardGridGrantOptionObject.Location = new Point(720, nextTop + 80);
        }

        private DataGridView CreateGrid(Size size, Point location)
        {
            return new DataGridView
            {
                Location = location,
                Size = size,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
        }

        private void LoadInitialData()
        {
            LoadGrantees();
            UpdateGrantOptionState();
            LoadObjectOwners();
            LoadRolesForRoleTab();
            LoadUsersForRoleTab();
            RefreshPrivilegeList();

            LoadObjectPrivilegeGrid();
            LoadGrantOptionGrid();

            LoadRolePrivilegeGrid();
            LoadAdminOptionGrid();
        }

        private void ExecuteReaderToCombo(ComboBox combo, string query, OracleParameter[] parameters = null)
        {
            combo.Items.Clear();

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                combo.Items.Add(reader[0].ToString());
                        }
                    }
                }
            }
            catch
            {
                if (query.Contains("DBA_"))
                    ExecuteReaderToComboFallback(combo, query.Replace("DBA_", "ALL_"), parameters);
            }

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void ExecuteReaderToComboFallback(ComboBox combo, string fallbackQuery, OracleParameter[] parameters)
        {
            try
            {
                OracleParameter[] clonedParameters = null;
                if (parameters != null)
                {
                    clonedParameters = new OracleParameter[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        clonedParameters[i] = new OracleParameter(parameters[i].ParameterName, parameters[i].Value);
                }

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(fallbackQuery, conn))
                    {
                        cmd.BindByName = true;
                        if (clonedParameters != null)
                            cmd.Parameters.AddRange(clonedParameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                combo.Items.Add(reader[0].ToString());
                        }
                    }
                }
            }
            catch
            {
            }

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private DataTable ExecuteToDataTable(string query, OracleParameter[] parameters = null, bool allowFallback = true)
        {
            var dt = new DataTable();

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (var da = new OracleDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch
            {
                if (allowFallback && query.Contains("DBA_"))
                {
                    string fallbackQuery = query.Replace("DBA_", "ALL_");
                    try
                    {
                        using (var conn = new OracleConnection(_connStr))
                        {
                            conn.Open();
                            using (var cmd = new OracleCommand(fallbackQuery, conn))
                            {
                                cmd.BindByName = true;
                                if (parameters != null)
                                {
                                    var cloned = new OracleParameter[parameters.Length];
                                    for (int i = 0; i < parameters.Length; i++)
                                        cloned[i] = new OracleParameter(parameters[i].ParameterName, parameters[i].Value);

                                    cmd.Parameters.AddRange(cloned);
                                }

                                using (var da = new OracleDataAdapter(cmd))
                                {
                                    da.Fill(dt);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return dt;
        }

        private void LoadGrantees()
        {
            string granteeType = cmbGranteeType.SelectedItem?.ToString();

            if (granteeType == "User")
            {
                string query = "SELECT USERNAME FROM DBA_USERS WHERE USERNAME NOT IN ('SYS', 'SYSTEM') ORDER BY USERNAME";
                ExecuteReaderToCombo(cmbGrantee, query);
            }
            else
            {
                string query = "SELECT ROLE FROM DBA_ROLES ORDER BY ROLE";
                ExecuteReaderToCombo(cmbGrantee, query);
            }

            UIHelper.SetStatus(lblStatus1, "Da tai danh sach " + granteeType, StatusType.Success);
        }

        private void LoadObjectOwners()
        {
            string query = "SELECT DISTINCT OWNER FROM DBA_OBJECTS WHERE OBJECT_TYPE IN ('TABLE','VIEW','PROCEDURE','FUNCTION') ORDER BY OWNER";
            ExecuteReaderToCombo(cmbObjectOwner, query);
        }

        private void LoadRolesForRoleTab()
        {
            string query = "SELECT ROLE FROM DBA_ROLES ORDER BY ROLE";
            ExecuteReaderToCombo(cmbRoleToAssign, query);
        }

        private void LoadUsersForRoleTab()
        {
            string query = "SELECT USERNAME FROM DBA_USERS WHERE ACCOUNT_STATUS = 'OPEN' AND USERNAME NOT IN ('SYS','SYSTEM','DBSNMP','XDB') ORDER BY USERNAME";
            ExecuteReaderToCombo(cmbRoleGrantee, query);
        }

        private void LoadObjects()
        {
            if (cmbObjectOwner.SelectedItem == null || cmbObjectType.SelectedItem == null)
            {
                cmbObjectName.Items.Clear();
                return;
            }

            string owner = cmbObjectOwner.SelectedItem.ToString();
            string objType = cmbObjectType.SelectedItem.ToString();

            string query = "SELECT OBJECT_NAME FROM DBA_OBJECTS WHERE OWNER = :owner AND OBJECT_TYPE = :objType ORDER BY OBJECT_NAME";
            OracleParameter[] parameters = {
                new OracleParameter("owner", owner),
                new OracleParameter("objType", objType)
            };

            ExecuteReaderToCombo(cmbObjectName, query, parameters);
        }

        private void LoadColumns()
        {
            clbColumns.Items.Clear();

            if (cmbObjectType.SelectedItem == null)
                return;

            string objType = cmbObjectType.SelectedItem.ToString();
            if (objType != "TABLE" && objType != "VIEW")
                return;

            if (cmbObjectOwner.SelectedItem == null || cmbObjectName.SelectedItem == null)
                return;

            string owner = cmbObjectOwner.SelectedItem.ToString();
            string objName = cmbObjectName.SelectedItem.ToString();

            string query = "SELECT COLUMN_NAME FROM DBA_TAB_COLUMNS WHERE OWNER = :owner AND TABLE_NAME = :objName ORDER BY COLUMN_ID";
            OracleParameter[] parameters = {
                new OracleParameter("owner", owner),
                new OracleParameter("objName", objName)
            };

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.AddRange(parameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                clbColumns.Items.Add(reader[0].ToString());
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    string fallbackQuery = "SELECT COLUMN_NAME FROM ALL_TAB_COLUMNS WHERE OWNER = :owner AND TABLE_NAME = :objName ORDER BY COLUMN_ID";
                    using (var conn = new OracleConnection(_connStr))
                    {
                        conn.Open();
                        using (var cmd = new OracleCommand(fallbackQuery, conn))
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.AddRange(parameters);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    clbColumns.Items.Add(reader[0].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UIHelper.SetStatus(lblStatus1, "Loi tai danh sach cot: " + ex.Message, StatusType.Error);
                }
            }
        }

        private void UpdateGrantOptionState()
        {
            string granteeType = cmbGranteeType.SelectedItem?.ToString();
            if (granteeType == "Role")
            {
                chkGrantOption.Checked = false;
                chkGrantOption.Enabled = false;
            }
            else
            {
                chkGrantOption.Enabled = true;
            }
        }

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
            }
            else
            {
                clbPrivileges.Items.Add("EXECUTE");
            }

            UpdateObjectPrivLayout();
        }

        private void ClbPrivileges_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke((MethodInvoker)(() =>
            {
                bool show = false;
                string objType = cmbObjectType.SelectedItem?.ToString() ?? "";

                for (int i = 0; i < clbPrivileges.Items.Count; i++)
                {
                    bool isChecked = clbPrivileges.GetItemChecked(i);

                    if (i == e.Index)
                        isChecked = e.NewValue == CheckState.Checked;

                    if (isChecked)
                    {
                        string priv = clbPrivileges.Items[i].ToString();
                        if (priv == "SELECT" || priv == "UPDATE")
                        {
                            show = true;
                            break;
                        }
                    }
                }

                pnlColumnArea.Visible = show && (objType == "TABLE" || objType == "VIEW");

                if (pnlColumnArea.Visible && clbColumns.Items.Count == 0)
                    LoadColumns();

                UpdateObjectPrivLayout();
            }));
        }

        private void LoadObjectPrivilegeGrid()
        {
            if (cmbGrantee.SelectedItem == null)
            {
                dgvObjectPrivileges.DataSource = null;
                return;
            }

            string grantee = cmbGrantee.SelectedItem.ToString();

            string query = @"
                SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE
                FROM DBA_TAB_PRIVS
                WHERE GRANTEE = :grantee
                ORDER BY OWNER, TABLE_NAME, PRIVILEGE";

            OracleParameter[] parameters = {
                new OracleParameter("grantee", grantee.ToUpper())
            };

            dgvObjectPrivileges.DataSource = ExecuteToDataTable(query, parameters);
        }

        private void LoadGrantOptionGrid()
        {
            if (cmbGrantee.SelectedItem == null)
            {
                dgvGrantOptionOnly.DataSource = null;
                return;
            }

            string grantee = cmbGrantee.SelectedItem.ToString();

            string query = @"
                SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE
                FROM DBA_TAB_PRIVS
                WHERE GRANTEE = :grantee
                  AND GRANTABLE = 'YES'
                ORDER BY OWNER, TABLE_NAME, PRIVILEGE";

            OracleParameter[] parameters = {
                new OracleParameter("grantee", grantee.ToUpper())
            };

            dgvGrantOptionOnly.DataSource = ExecuteToDataTable(query, parameters);
        }

        private void LoadRolePrivilegeGrid()
        {
            if (cmbRoleGrantee.SelectedItem == null)
            {
                dgvRolePrivileges.DataSource = null;
                return;
            }

            string grantee = cmbRoleGrantee.SelectedItem.ToString();

            string query = @"
                SELECT GRANTEE, GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
                FROM DBA_ROLE_PRIVS
                WHERE GRANTEE = :grantee
                ORDER BY GRANTED_ROLE";

            OracleParameter[] parameters = {
                new OracleParameter("grantee", grantee.ToUpper())
            };

            dgvRolePrivileges.DataSource = ExecuteToDataTable(query, parameters);
        }

        private void LoadAdminOptionGrid()
        {
            if (cmbRoleGrantee.SelectedItem == null)
            {
                dgvAdminOptionOnly.DataSource = null;
                return;
            }

            string grantee = cmbRoleGrantee.SelectedItem.ToString();

            string query = @"
                SELECT GRANTEE, GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
                FROM DBA_ROLE_PRIVS
                WHERE GRANTEE = :grantee
                  AND ADMIN_OPTION = 'YES'
                ORDER BY GRANTED_ROLE";

            OracleParameter[] parameters = {
                new OracleParameter("grantee", grantee.ToUpper())
            };

            dgvAdminOptionOnly.DataSource = ExecuteToDataTable(query, parameters);
        }

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

            try
            {
                string granteeType = cmbGranteeType.SelectedItem?.ToString();
                string grantee = cmbGrantee.SelectedItem.ToString();
                string objOwner = cmbObjectOwner.Text;
                string objName = cmbObjectName.Text;

                string grantOption = "NO";
                if (granteeType == "User")
                    grantOption = chkGrantOption.Checked ? "YES" : "NO";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    foreach (string priv in clbPrivileges.CheckedItems)
                    {
                        object columnsParam = DBNull.Value;

                        if ((priv == "SELECT" || priv == "UPDATE")
                            && !chkAllColumns.Checked
                            && clbColumns.CheckedItems.Count > 0)
                        {
                            var cols = new List<string>();
                            foreach (string col in clbColumns.CheckedItems)
                                cols.Add(col);

                            columnsParam = string.Join(",", cols);
                        }

                        using (var cmd = new OracleCommand("SP_GRANT_OBJ_PRIV", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.BindByName = true;

                            cmd.Parameters.Add("p_privilege", OracleDbType.Varchar2).Value = priv;
                            cmd.Parameters.Add("p_object_owner", OracleDbType.Varchar2).Value = objOwner;
                            cmd.Parameters.Add("p_object_name", OracleDbType.Varchar2).Value = objName;
                            cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                            cmd.Parameters.Add("p_columns", OracleDbType.Varchar2).Value = columnsParam;
                            cmd.Parameters.Add("p_with_grant_opt", OracleDbType.Varchar2).Value = grantOption;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                LoadObjectPrivilegeGrid();
                LoadGrantOptionGrid();

                string grantInfo = (granteeType == "User" && grantOption == "YES")
                    ? " (WITH GRANT OPTION)"
                    : "";

                UIHelper.SetStatus(
                    lblStatus1,
                    "Da cap quyen thanh cong cho " + grantee + " tren " + objOwner + "." + objName + grantInfo,
                    StatusType.Success
                );
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi cap quyen: " + ex.Message, StatusType.Error);
            }
        }

        private void BtnGrantRole_Click(object sender, EventArgs e)
        {
            string role = cmbRoleToAssign.SelectedItem?.ToString();
            string grantee = cmbRoleGrantee.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(grantee))
            {
                UIHelper.SetStatus(lblStatus2, "Vui long chon day du thong tin!", StatusType.Warning);
                return;
            }

            try
            {
                string adminOption = chkAdminOption.Checked ? "YES" : "NO";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand("SP_GRANT_ROLE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;

                        cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = role;
                        cmd.Parameters.Add("p_grantee", OracleDbType.Varchar2).Value = grantee;
                        cmd.Parameters.Add("p_with_admin_opt", OracleDbType.Varchar2).Value = adminOption;

                        cmd.ExecuteNonQuery();
                    }
                }
                LoadRolesForRoleTab();
                LoadRolePrivilegeGrid();
                LoadAdminOptionGrid();


                string adminInfo = adminOption == "YES" ? " (WITH ADMIN OPTION)" : "";

                UIHelper.SetStatus(
                    lblStatus2,
                    "Da cap role " + role + " cho " + grantee + adminInfo,
                    StatusType.Success
                );
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus2, "Loi cap role: " + ex.Message, StatusType.Error);
            }
        }
    }
}