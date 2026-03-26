using System;
using System.Drawing;
using System.Windows.Forms;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class UserManagementPanel : UserControl
    {
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private Button btnRefresh, btnCreate, btnEdit, btnDelete, btnLock, btnUnlock;
        private Label lblStatus;

        private readonly string _connStr;

        public UserManagementPanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadData();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;
            this.Padding = new Padding(5);

            var header = UIHelper.CreateSectionHeader(
                "Quan ly User",
                "Tao moi, chinh sua, xoa tai khoan nguoi dung Oracle");
            this.Controls.Add(header);

            // Toolbar
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };

            txtSearch = new TextBox
            {
                Location = new Point(5, 10),
                Size = new Size(200, 26),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => FilterGrid();

            btnRefresh = UIHelper.CreateButton("Lam moi",   ButtonStyle.Secondary);
            btnCreate  = UIHelper.CreateButton("+ Tao User", ButtonStyle.Primary);
            btnEdit    = UIHelper.CreateButton("Sua",        ButtonStyle.Warning);
            btnDelete  = UIHelper.CreateButton("Xoa",        ButtonStyle.Danger);
            btnLock    = UIHelper.CreateButton("Khoa",       ButtonStyle.Secondary);
            btnUnlock  = UIHelper.CreateButton("Mo khoa",    ButtonStyle.Success);

            int bx = 215;
            Button[] toolBtns = { btnRefresh, btnCreate, btnEdit, btnDelete, btnLock, btnUnlock };
            foreach (var btn in toolBtns)
            {
                btn.Location = new Point(bx, 8);
                btn.Width = 90;
                pnlToolbar.Controls.Add(btn);
                bx += 96;
            }
            pnlToolbar.Controls.Add(txtSearch);

            btnRefresh.Click += (s, e) => LoadData();
            btnCreate.Click  += (s, e) => OpenCreateDialog();
            btnEdit.Click    += (s, e) => OpenEditDialog();
            btnDelete.Click  += (s, e) => DeleteSelected();
            btnLock.Click    += (s, e) => ToggleLock(false);
            btnUnlock.Click  += (s, e) => ToggleLock(true);

            this.Controls.Add(pnlToolbar);

            // Grid
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvUsers = UIHelper.CreateGrid();
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "USERNAME",           HeaderText = "Ten dang nhap",   FillWeight = 20 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ACCOUNT_STATUS",     HeaderText = "Trang thai",      FillWeight = 15 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "DEFAULT_TABLESPACE", HeaderText = "Tablespace",      FillWeight = 15 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "PROFILE",            HeaderText = "Profile",         FillWeight = 12 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CREATED",            HeaderText = "Ngay tao",        FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "EXPIRY_DATE",        HeaderText = "Ngay het han",    FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "LOCK_DATE",          HeaderText = "Ngay khoa",       FillWeight = 12 });

            dgvUsers.CellFormatting += DgvUsers_CellFormatting;
            pnlGrid.Controls.Add(dgvUsers);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);
            this.Controls.Add(pnlGrid);
        }

        private void DgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsers.Columns[e.ColumnIndex].Name == "ACCOUNT_STATUS" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "OPEN")
                    e.CellStyle.ForeColor = UIHelper.Success;
                else if (status.Contains("LOCK"))
                    e.CellStyle.ForeColor = UIHelper.Danger;
                else if (status.Contains("EXPIRED"))
                    e.CellStyle.ForeColor = UIHelper.Warning;
            }
        }

        private void LoadData()
        {
            UIHelper.SetStatus(lblStatus, "Dang tai du lieu...", StatusType.Info);
            dgvUsers.Rows.Clear();

            try
            {
                // TODO: thay bang query Oracle thuc
                // string sql = "SELECT USERNAME, ACCOUNT_STATUS, DEFAULT_TABLESPACE, PROFILE, " +
                //              "TO_CHAR(CREATED,'DD/MM/YYYY'), TO_CHAR(EXPIRY_DATE,'DD/MM/YYYY'), " +
                //              "TO_CHAR(LOCK_DATE,'DD/MM/YYYY') FROM DBA_USERS ORDER BY USERNAME";

                dgvUsers.Rows.Add("SYSTEM",    "OPEN",    "SYSTEM", "DEFAULT", "01/01/2024", "",           "");
                dgvUsers.Rows.Add("SCOTT",     "OPEN",    "USERS",  "DEFAULT", "15/02/2024", "15/02/2025", "");
                dgvUsers.Rows.Add("HR",        "EXPIRED", "USERS",  "DEFAULT", "10/03/2024", "10/03/2025", "");
                dgvUsers.Rows.Add("TEST_USER", "LOCKED",  "USERS",  "DEFAULT", "20/04/2024", "",           "01/06/2024");
                dgvUsers.Rows.Add("DEMO",      "OPEN",    "USERS",  "DEFAULT", "05/05/2024", "",           "");

                UIHelper.SetStatus(lblStatus, "Tong cong " + dgvUsers.Rows.Count + " user.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }

        private void FilterGrid()
        {
            string q = txtSearch.Text.Trim().ToUpper();
            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                row.Visible = string.IsNullOrEmpty(q) ||
                    (row.Cells["USERNAME"].Value != null &&
                     row.Cells["USERNAME"].Value.ToString().ToUpper().Contains(q));
            }
        }

        private void OpenCreateDialog()
        {
            UserEditDialog dlg = new UserEditDialog(_connStr, null);
            if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            dlg.Dispose();
        }

        private void OpenEditDialog()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui long chon user can sua.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();
            UserEditDialog dlg = new UserEditDialog(_connStr, username);
            if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            dlg.Dispose();
        }

        private void DeleteSelected()
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();
            if (MessageBox.Show("Xac nhan xoa user \"" + username + "\"?\nHanh dong nay khong the hoan tac!",
                "Xac nhan xoa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    // TODO: DROP USER {username} CASCADE
                    UIHelper.SetStatus(lblStatus, "Da xoa user " + username, StatusType.Success);
                    LoadData();
                }
                catch (Exception ex)
                {
                    UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
                }
            }
        }

        private void ToggleLock(bool unlock)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();
            string msg = unlock ? "mo khoa" : "khoa";
            if (MessageBox.Show("Xac nhan " + msg + " user \"" + username + "\"?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // TODO: ALTER USER {username} ACCOUNT LOCK/UNLOCK
                    UIHelper.SetStatus(lblStatus, "Da " + msg + " user " + username, StatusType.Success);
                    LoadData();
                }
                catch (Exception ex)
                {
                    UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
                }
            }
        }
    }
}
