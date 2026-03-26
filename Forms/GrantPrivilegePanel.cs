using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
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
            PopulateStaticData();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Cap quyen",
                "Cap quyen tren doi tuong cho user/role, hoac cap role cho user");
            this.Controls.Add(header);

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
            this.Controls.Add(tabGrantType);
        }

        private void BuildObjectPrivTab()
        {
            tabObjectPriv.BackColor = UIHelper.LightBg;

            // Card: Nguoi nhan quyen
            var cardGrantee = UIHelper.CreateCard(10, 10, 560, 100, "NGUOI NHAN QUYEN");
            tabObjectPriv.Controls.Add(cardGrantee);

            UIHelper.CreateLabeledCombo(cardGrantee, "Loai", 10, 32, 130, out cmbGranteeType);
            cmbGranteeType.Items.AddRange(new object[] { "User", "Role" });
            cmbGranteeType.SelectedIndex = 0;
            cmbGranteeType.SelectedIndexChanged += (s, e) => LoadGrantees();

            UIHelper.CreateLabeledCombo(cardGrantee, "Ten User / Role", 160, 32, 370, out cmbGrantee);

            // Card: Doi tuong CSDL
            var cardObject = UIHelper.CreateCard(10, 120, 560, 100, "DOI TUONG CAP QUYEN");
            tabObjectPriv.Controls.Add(cardObject);

            UIHelper.CreateLabeledCombo(cardObject, "Loai doi tuong", 10, 32, 120, out cmbObjectType);
            cmbObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            cmbObjectType.SelectedIndex = 0;
            cmbObjectType.SelectedIndexChanged += (s, e) => RefreshPrivilegeList();

            UIHelper.CreateLabeledCombo(cardObject, "Schema / Owner", 150, 32, 150, out cmbObjectOwner);
            cmbObjectOwner.SelectedIndexChanged += (s, e) => LoadObjects();

            UIHelper.CreateLabeledCombo(cardObject, "Ten doi tuong", 320, 32, 220, out cmbObjectName);
            cmbObjectName.SelectedIndexChanged += (s, e) => LoadColumns();

            // Card: Chon quyen
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

            // Card: Phan quyen den cot
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

            // Grant option
            chkGrantOption = new CheckBox
            {
                Text = "WITH GRANT OPTION (nguoi duoc cap co the cap lai cho nguoi khac)",
                Location = new Point(10, 460),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIHelper.TextMuted
            };
            tabObjectPriv.Controls.Add(chkGrantOption);

            // Grant button
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

        private void PopulateStaticData()
        {
            cmbObjectOwner.Items.AddRange(new object[] { "SCOTT", "HR", "SYSTEM", "DEMO" });
            cmbObjectOwner.SelectedIndex = 0;

            cmbGrantee.Items.AddRange(new object[] { "SCOTT", "HR", "TEST_USER", "DEMO" });
            cmbGrantee.SelectedIndex = 0;

            cmbRoleToAssign.Items.AddRange(new object[] { "CONNECT", "RESOURCE", "DBA", "APP_READ_ROLE", "APP_WRITE_ROLE" });
            cmbRoleToAssign.SelectedIndex = 0;

            cmbRoleGrantee.Items.AddRange(new object[] { "SCOTT", "HR", "TEST_USER", "DEMO" });
            cmbRoleGrantee.SelectedIndex = 0;

            RefreshPrivilegeList();
            LoadObjects();
        }

        private void LoadGrantees()
        {
            // TODO: Load DBA_USERS or DBA_ROLES based on cmbGranteeType
        }

        private void LoadObjects()
        {
            cmbObjectName.Items.Clear();
            // TODO: SELECT OBJECT_NAME FROM DBA_OBJECTS WHERE OWNER=:o AND OBJECT_TYPE=:t
            cmbObjectName.Items.AddRange(new object[] { "EMP", "DEPT", "SALGRADE", "BONUS" });
            if (cmbObjectName.Items.Count > 0) cmbObjectName.SelectedIndex = 0;
        }

        private void LoadColumns()
        {
            clbColumns.Items.Clear();
            // TODO: SELECT COLUMN_NAME FROM DBA_TAB_COLUMNS WHERE OWNER=:o AND TABLE_NAME=:t
            clbColumns.Items.AddRange(new object[] { "EMPNO", "ENAME", "JOB", "MGR", "HIREDATE", "SAL", "COMM", "DEPTNO" });
        }

        private void RefreshPrivilegeList()
        {
            clbPrivileges.Items.Clear();
            pnlColumnArea.Visible = false;

            string objType = cmbObjectType.SelectedItem != null ? cmbObjectType.SelectedItem.ToString() : "TABLE";
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
            else
            {
                clbPrivileges.Items.Add("EXECUTE");
            }
        }

        private void ClbPrivileges_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string item = clbPrivileges.Items[e.Index].ToString();
            if (item == "SELECT" || item == "UPDATE")
            {
                bool show = (e.NewValue == CheckState.Checked);
                if (!show)
                {
                    // Check if other column-level priv still checked
                    foreach (int i in clbPrivileges.CheckedIndices)
                    {
                        if (i != e.Index)
                        {
                            string other = clbPrivileges.Items[i].ToString();
                            if (other == "SELECT" || other == "UPDATE") { show = true; break; }
                        }
                    }
                }
                string objType = cmbObjectType.SelectedItem != null ? cmbObjectType.SelectedItem.ToString() : "";
                pnlColumnArea.Visible = show && (objType == "TABLE" || objType == "VIEW");
                if (pnlColumnArea.Visible && clbColumns.Items.Count == 0) LoadColumns();
            }
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
                string grantee = cmbGrantee.SelectedItem.ToString();
                string obj = cmbObjectOwner.Text + "." + cmbObjectName.Text;
                string grantOption = chkGrantOption.Checked ? " WITH GRANT OPTION" : "";

                foreach (string priv in clbPrivileges.CheckedItems)
                {
                    string sql;
                    if ((priv == "SELECT" || priv == "UPDATE")
                        && !chkAllColumns.Checked
                        && clbColumns.CheckedItems.Count > 0)
                    {
                        List<string> cols = new List<string>();
                        foreach (string col in clbColumns.CheckedItems) cols.Add(col);
                        sql = "GRANT " + priv + " (" + string.Join(", ", cols) + ") ON " + obj + " TO " + grantee + grantOption;
                    }
                    else
                    {
                        sql = "GRANT " + priv + " ON " + obj + " TO " + grantee + grantOption;
                    }
                    // TODO: Execute sql
                    System.Diagnostics.Debug.WriteLine(sql);
                }

                UIHelper.SetStatus(lblStatus1, "Da cap quyen thanh cong cho " + grantee + " tren " + obj, StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus1, "Loi: " + ex.Message, StatusType.Error);
            }
        }

        private void BtnGrantRole_Click(object sender, EventArgs e)
        {
            string role = cmbRoleToAssign.SelectedItem != null ? cmbRoleToAssign.SelectedItem.ToString() : null;
            string user = cmbRoleGrantee.SelectedItem != null ? cmbRoleGrantee.SelectedItem.ToString() : null;
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(user))
            {
                UIHelper.SetStatus(lblStatus2, "Vui long chon day du thong tin!", StatusType.Warning);
                return;
            }
            string adminOpt = chkAdminOption.Checked ? " WITH ADMIN OPTION" : "";
            string sql = "GRANT " + role + " TO " + user + adminOpt;
            // TODO: Execute sql
            UIHelper.SetStatus(lblStatus2, "Da cap role " + role + " cho " + user, StatusType.Success);
        }
    }
}
