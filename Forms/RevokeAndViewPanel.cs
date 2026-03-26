using System;
using System.Drawing;
using System.Windows.Forms;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    // ══════════════════════════════════════════════════════════════════════════
    // REVOKE PRIVILEGE PANEL
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
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Thu hoi quyen",
                "Xem va thu hoi quyen dang duoc cap cho user hoac role");
            this.Controls.Add(header);

            // Selection card
            var card = UIHelper.CreateCard(10, 65, 580, 100, "CHON DOI TUONG");
            this.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loai", 10, 32, 110, out cmbTargetType);
            cmbTargetType.Items.AddRange(new object[] { "User", "Role" });
            cmbTargetType.SelectedIndex = 0;

            UIHelper.CreateLabeledCombo(card, "Ten User / Role", 140, 32, 200, out cmbTarget);
            cmbTarget.Items.AddRange(new object[] { "SCOTT", "HR", "TEST_USER", "DEMO", "CONNECT", "RESOURCE" });
            cmbTarget.SelectedIndex = 0;

            UIHelper.CreateLabeledCombo(card, "Loai quyen hien thi", 360, 32, 200, out cmbPrivType);
            cmbPrivType.Items.AddRange(new object[] { "Tat ca", "Quyen tren doi tuong", "Quyen he thong", "Role duoc cap" });
            cmbPrivType.SelectedIndex = 0;

            btnLoadPrivs = UIHelper.CreateButton("Tai danh sach quyen", ButtonStyle.Secondary);
            btnLoadPrivs.Size = new Size(180, 34);
            btnLoadPrivs.Location = new Point(10, 175);
            btnLoadPrivs.ForeColor = UIHelper.TextDark;
            btnLoadPrivs.Click += (s, e) => LoadPrivileges();
            this.Controls.Add(btnLoadPrivs);

            // Grid
            var pnlGrid = new Panel
            {
                Location = new Point(10, 218),
                Size = new Size(580, 295),
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(pnlGrid);

            dgvCurrentPrivs = UIHelper.CreateGrid();
            dgvCurrentPrivs.Dock = DockStyle.Fill;
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIV_TYPE",    HeaderText = "Loai",          FillWeight = 15 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "PRIVILEGE",    HeaderText = "Quyen / Role",  FillWeight = 20 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_OWNER", HeaderText = "Schema",        FillWeight = 15 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "OBJECT_NAME",  HeaderText = "Doi tuong",     FillWeight = 20 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "COLUMNS",      HeaderText = "Cot",           FillWeight = 15 });
            dgvCurrentPrivs.Columns.Add(new DataGridViewTextBoxColumn { Name = "GRANT_OPTION", HeaderText = "Grant Option",  FillWeight = 15 });
            pnlGrid.Controls.Add(dgvCurrentPrivs);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);

            // Revoke button
            btnRevoke = UIHelper.CreateButton("THU HOI QUYEN DA CHON", ButtonStyle.Danger);
            btnRevoke.Size = new Size(210, 36);
            btnRevoke.Location = new Point(380, 523);
            btnRevoke.Click += BtnRevoke_Click;
            this.Controls.Add(btnRevoke);
        }

        private void LoadPrivileges()
        {
            dgvCurrentPrivs.Rows.Clear();
            // TODO: Query DBA_TAB_PRIVS, DBA_COL_PRIVS, DBA_SYS_PRIVS, DBA_ROLE_PRIVS
            dgvCurrentPrivs.Rows.Add("Doi tuong", "SELECT",         "SCOTT", "EMP",          "ENAME, SAL", "YES");
            dgvCurrentPrivs.Rows.Add("Doi tuong", "INSERT",         "SCOTT", "DEPT",          "",           "NO");
            dgvCurrentPrivs.Rows.Add("Doi tuong", "UPDATE",         "SCOTT", "EMP",           "SAL",        "NO");
            dgvCurrentPrivs.Rows.Add("Doi tuong", "EXECUTE",        "SCOTT", "GET_EMP_INFO",  "",           "NO");
            dgvCurrentPrivs.Rows.Add("Role",      "CONNECT",        "",      "",              "",           "NO");
            dgvCurrentPrivs.Rows.Add("He thong",  "CREATE SESSION", "",      "",              "",           "NO");
            UIHelper.SetStatus(lblStatus, dgvCurrentPrivs.Rows.Count + " quyen dang duoc cap.", StatusType.Success);
        }

        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (dgvCurrentPrivs.SelectedRows.Count == 0)
            {
                UIHelper.SetStatus(lblStatus, "Vui long chon quyen can thu hoi!", StatusType.Warning);
                return;
            }

            DataGridViewRow row = dgvCurrentPrivs.SelectedRows[0];
            string priv     = row.Cells["PRIVILEGE"].Value != null    ? row.Cells["PRIVILEGE"].Value.ToString()    : "";
            string target   = cmbTarget.SelectedItem != null          ? cmbTarget.SelectedItem.ToString()          : "";
            string privType = row.Cells["PRIV_TYPE"].Value != null    ? row.Cells["PRIV_TYPE"].Value.ToString()    : "";
            string owner    = row.Cells["OBJECT_OWNER"].Value != null ? row.Cells["OBJECT_OWNER"].Value.ToString() : "";
            string objName  = row.Cells["OBJECT_NAME"].Value != null  ? row.Cells["OBJECT_NAME"].Value.ToString()  : "";
            string obj      = owner + "." + objName;

            string confirmMsg = privType == "Role"
                ? "Thu hoi role \"" + priv + "\" tu \"" + target + "\"?"
                : "Thu hoi quyen \"" + priv + "\" tren \"" + obj + "\" tu \"" + target + "\"?";

            if (MessageBox.Show(confirmMsg + "\nHanh dong nay khong the hoan tac!",
                "Xac nhan thu hoi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string sql = privType == "Role"
                        ? "REVOKE " + priv + " FROM " + target
                        : "REVOKE " + priv + " ON " + obj + " FROM " + target;
                    // TODO: Execute sql
                    UIHelper.SetStatus(lblStatus, "Da thu hoi quyen thanh cong.", StatusType.Success);
                    LoadPrivileges();
                }
                catch (Exception ex)
                {
                    UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
                }
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // VIEW PRIVILEGE PANEL
    // ══════════════════════════════════════════════════════════════════════════
    public class ViewPrivilegePanel : UserControl
    {
        private ComboBox cmbViewType, cmbViewTarget;
        private TabControl tabResults;
        private DataGridView dgvObjectPrivs, dgvSysPrivs, dgvRolePrivs, dgvColPrivs;
        private Button btnView;
        private readonly string _connStr;

        public ViewPrivilegePanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Xem quyen",
                "Xem toan bo quyen dang duoc cap cho user hoac role");
            this.Controls.Add(header);

            var card = UIHelper.CreateCard(10, 65, 580, 90, "TIM KIEM");
            this.Controls.Add(card);

            UIHelper.CreateLabeledCombo(card, "Loai", 10, 30, 110, out cmbViewType);
            cmbViewType.Items.AddRange(new object[] { "User", "Role" });
            cmbViewType.SelectedIndex = 0;

            UIHelper.CreateLabeledCombo(card, "Ten User / Role", 140, 30, 210, out cmbViewTarget);
            cmbViewTarget.Items.AddRange(new object[] { "SCOTT", "HR", "TEST_USER", "DEMO" });
            cmbViewTarget.SelectedIndex = 0;

            btnView = UIHelper.CreateButton("Xem quyen", ButtonStyle.Primary);
            btnView.Size = new Size(130, 34);
            btnView.Location = new Point(370, 36);
            card.Controls.Add(btnView);
            btnView.Click += (s, e) => LoadPrivileges();

            tabResults = new TabControl
            {
                Location = new Point(10, 165),
                Size = new Size(580, 335),
                Font = new Font("Segoe UI", 9f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };

            tabResults.TabPages.Add(BuildPrivTab("  Quyen doi tuong  ", out dgvObjectPrivs,
                new string[] { "OWNER", "OBJECT_NAME", "OBJECT_TYPE", "PRIVILEGE", "GRANTABLE" },
                new string[] { "Schema", "Doi tuong", "Loai", "Quyen", "Grant Option" }));

            tabResults.TabPages.Add(BuildPrivTab("  Quyen he thong  ", out dgvSysPrivs,
                new string[] { "PRIVILEGE", "ADMIN_OPTION", "COMMON" },
                new string[] { "Quyen he thong", "Admin Option", "Common" }));

            tabResults.TabPages.Add(BuildPrivTab("  Role duoc cap  ", out dgvRolePrivs,
                new string[] { "GRANTED_ROLE", "ADMIN_OPTION", "DEFAULT_ROLE" },
                new string[] { "Role", "Admin Option", "Default" }));

            tabResults.TabPages.Add(BuildPrivTab("  Quyen theo cot  ", out dgvColPrivs,
                new string[] { "OWNER", "TABLE_NAME", "COLUMN_NAME", "PRIVILEGE", "GRANTABLE" },
                new string[] { "Schema", "Bang", "Cot", "Quyen", "Grant Option" }));

            this.Controls.Add(tabResults);
        }

        private TabPage BuildPrivTab(string title, out DataGridView dgv,
            string[] colNames, string[] headers)
        {
            var tab = new TabPage(title) { BackColor = UIHelper.CardBg };
            var grid = UIHelper.CreateGrid();
            grid.Dock = DockStyle.Fill;
            for (int i = 0; i < colNames.Length; i++)
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = colNames[i], HeaderText = headers[i] });
            tab.Controls.Add(grid);
            dgv = grid;
            return tab;
        }

        private void LoadPrivileges()
        {
            string target = cmbViewTarget.SelectedItem != null ? cmbViewTarget.SelectedItem.ToString() : "";
            if (string.IsNullOrEmpty(target)) return;

            // TODO: Real queries from DBA_TAB_PRIVS, DBA_SYS_PRIVS, DBA_ROLE_PRIVS, DBA_COL_PRIVS

            dgvObjectPrivs.Rows.Clear();
            dgvObjectPrivs.Rows.Add("SCOTT", "EMP",       "TABLE",    "SELECT",  "YES");
            dgvObjectPrivs.Rows.Add("SCOTT", "DEPT",      "TABLE",    "INSERT",  "NO");
            dgvObjectPrivs.Rows.Add("SCOTT", "GET_SAL",   "FUNCTION", "EXECUTE", "NO");
            dgvObjectPrivs.Rows.Add("SCOTT", "EMP_VIEW",  "VIEW",     "SELECT",  "NO");

            dgvSysPrivs.Rows.Clear();
            dgvSysPrivs.Rows.Add("CREATE SESSION", "NO", "NO");
            dgvSysPrivs.Rows.Add("CREATE TABLE",   "NO", "NO");

            dgvRolePrivs.Rows.Clear();
            dgvRolePrivs.Rows.Add("CONNECT",  "NO", "YES");
            dgvRolePrivs.Rows.Add("RESOURCE", "NO", "YES");

            dgvColPrivs.Rows.Clear();
            dgvColPrivs.Rows.Add("SCOTT", "EMP", "ENAME", "SELECT", "YES");
            dgvColPrivs.Rows.Add("SCOTT", "EMP", "SAL",   "SELECT", "YES");
            dgvColPrivs.Rows.Add("SCOTT", "EMP", "SAL",   "UPDATE", "NO");
        }
    }
}
