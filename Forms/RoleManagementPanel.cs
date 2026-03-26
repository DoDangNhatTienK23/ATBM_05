using System;
using System.Drawing;
using System.Windows.Forms;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class RoleManagementPanel : UserControl
    {
        private DataGridView dgvRoles;
        private TextBox txtSearch;
        private Button btnRefresh, btnCreate, btnDelete;
        private Label lblStatus;
        private readonly string _connStr;

        public RoleManagementPanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadData();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Quan ly Role",
                "Tao moi, xoa role va xem danh sach role trong he thong");
            this.Controls.Add(header);

            var pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            txtSearch = new TextBox
            {
                Location = new Point(5, 10),
                Size = new Size(200, 26),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.White
            };
            txtSearch.TextChanged += (s, e) => FilterGrid();

            btnRefresh = UIHelper.CreateButton("Lam moi",   ButtonStyle.Secondary);
            btnCreate  = UIHelper.CreateButton("+ Tao Role", ButtonStyle.Primary);
            btnDelete  = UIHelper.CreateButton("Xoa Role",   ButtonStyle.Danger);

            btnRefresh.Location = new Point(215, 8); btnRefresh.Width = 90;
            btnCreate.Location  = new Point(311, 8); btnCreate.Width  = 100;
            btnDelete.Location  = new Point(417, 8); btnDelete.Width  = 90;

            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnCreate);
            pnlToolbar.Controls.Add(btnDelete);

            btnRefresh.Click += (s, e) => LoadData();
            btnCreate.Click  += (s, e) => OpenCreateDialog();
            btnDelete.Click  += (s, e) => DeleteSelected();

            this.Controls.Add(pnlToolbar);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvRoles = UIHelper.CreateGrid();
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "ROLE",             HeaderText = "Ten Role",        FillWeight = 30 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "AUTHENTICATION",   HeaderText = "Xac thuc",        FillWeight = 20 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "COMMON",           HeaderText = "Common Role",     FillWeight = 15 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "ORACLE_MAINTAINED",HeaderText = "Oracle Built-in", FillWeight = 15 });

            pnlGrid.Controls.Add(dgvRoles);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);
            this.Controls.Add(pnlGrid);
        }

        private void LoadData()
        {
            UIHelper.SetStatus(lblStatus, "Dang tai du lieu...", StatusType.Info);
            dgvRoles.Rows.Clear();

            // TODO: SELECT ROLE, AUTHENTICATION_TYPE, COMMON, ORACLE_MAINTAINED FROM DBA_ROLES
            dgvRoles.Rows.Add("DBA",                 "NONE", "YES", "YES");
            dgvRoles.Rows.Add("CONNECT",             "NONE", "YES", "YES");
            dgvRoles.Rows.Add("RESOURCE",            "NONE", "YES", "YES");
            dgvRoles.Rows.Add("SELECT_CATALOG_ROLE", "NONE", "YES", "YES");
            dgvRoles.Rows.Add("APP_READ_ROLE",        "NONE", "NO",  "NO");
            dgvRoles.Rows.Add("APP_WRITE_ROLE",       "NONE", "NO",  "NO");

            UIHelper.SetStatus(lblStatus, dgvRoles.Rows.Count + " role trong he thong.", StatusType.Success);
        }

        private void FilterGrid()
        {
            string q = txtSearch.Text.Trim().ToUpper();
            foreach (DataGridViewRow row in dgvRoles.Rows)
                row.Visible = string.IsNullOrEmpty(q) ||
                    (row.Cells["ROLE"].Value != null &&
                     row.Cells["ROLE"].Value.ToString().ToUpper().Contains(q));
        }

        private void OpenCreateDialog()
        {
            RoleEditDialog dlg = new RoleEditDialog(_connStr);
            if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            dlg.Dispose();
        }

        private void DeleteSelected()
        {
            if (dgvRoles.SelectedRows.Count == 0) return;
            string role = dgvRoles.SelectedRows[0].Cells["ROLE"].Value?.ToString();
            if (MessageBox.Show("Xac nhan xoa role \"" + role + "\"?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // TODO: DROP ROLE {role}
                UIHelper.SetStatus(lblStatus, "Da xoa role " + role, StatusType.Success);
                LoadData();
            }
        }
    }

    // ── Role Create Dialog ─────────────────────────────────────────────────
    public class RoleEditDialog : Form
    {
        private TextBox txtRoleName, txtPassword;
        private ComboBox cmbAuth;
        private Button btnSave, btnCancel;
        private readonly string _connStr;

        public RoleEditDialog(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text = "Tao Role moi";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = UIHelper.LightBg;

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(30, 50, 80) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "+ Tao Role moi",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 12)
            });

            var card = UIHelper.CreateCard(15, 55, 360, 140);

            UIHelper.CreateLabeledInput(card, "Ten Role *", 10, 10, 330, out txtRoleName);

            UIHelper.CreateLabeledCombo(card, "Kieu xac thuc", 10, 60, 160, out cmbAuth);
            cmbAuth.Items.AddRange(new object[] { "NOT IDENTIFIED", "BY PASSWORD" });
            cmbAuth.SelectedIndex = 0;
            cmbAuth.SelectedIndexChanged += (s, e) => txtPassword.Visible = cmbAuth.SelectedIndex == 1;

            UIHelper.CreateLabeledInput(card, "Mat khau", 190, 60, 150, out txtPassword, true);
            txtPassword.Visible = false;

            btnCancel = UIHelper.CreateButton("Huy", ButtonStyle.Secondary);
            btnCancel.Size = new Size(80, 34);
            btnCancel.Location = new Point(195, 210);
            btnCancel.ForeColor = UIHelper.TextDark;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave = UIHelper.CreateButton("+ Tao", ButtonStyle.Primary);
            btnSave.Size = new Size(100, 34);
            btnSave.Location = new Point(285, 210);
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtRoleName.Text))
                {
                    MessageBox.Show("Vui long nhap ten role.", "Thong bao");
                    return;
                }
                // TODO: CREATE ROLE {roleName} [IDENTIFIED BY {pwd} | NOT IDENTIFIED]
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(pnlTitle);
            this.Controls.Add(card);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);
        }
    }
}
