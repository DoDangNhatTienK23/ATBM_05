using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class RoleManagementPanel : UserControl
    {
        private DataGridView dgvRoles;
        private TextBox      txtSearch;
        private Button       btnRefresh, btnCreate, btnDelete;
        private Label        lblStatus;
        private readonly string _connStr;

        public RoleManagementPanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadData();
        }

        private void InitializeLayout()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader(
                "Quan ly Role",
                "Tao moi, xoa role va xem danh sach role trong he thong");

            var pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            txtSearch = new TextBox
            {
                Location    = new Point(5, 10),
                Size        = new Size(200, 26),
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Color.White
            };
            txtSearch.TextChanged += (s, e) => FilterGrid();

            btnRefresh = UIHelper.CreateButton("Lam moi",    ButtonStyle.Secondary);
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

            var pnlGrid = new Panel
            {
                Dock        = DockStyle.Fill,
                BackColor   = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvRoles = UIHelper.CreateGrid();
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "ROLE",              HeaderText = "Ten Role",       FillWeight = 30 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "AUTHENTICATION",    HeaderText = "Xac thuc",       FillWeight = 20 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "COMMON",            HeaderText = "Common Role",    FillWeight = 15 });
            dgvRoles.Columns.Add(new DataGridViewTextBoxColumn { Name = "ORACLE_MAINTAINED", HeaderText = "Oracle Built-in",FillWeight = 15 });

            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);
            pnlGrid.Controls.Add(dgvRoles);

            // ── Thứ tự add vào this: Fill trước, Top sau (WinForms dock LIFO) ──
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(header);
        }

        // ── Load danh sách role từ Oracle qua FN_LIST_ROLES ──────────────
        // FN_LIST_ROLES trả về T_ROLE_TABLE (pipelined): ROLE, PASSWORD_REQUIRED
        // Lấy thêm COMMON, ORACLE_MAINTAINED từ DBA_ROLES để hiển thị đủ cột
        private void LoadData()
        {
            UIHelper.SetStatus(lblStatus, "Dang tai du lieu...", StatusType.Info);
            dgvRoles.Rows.Clear();

            try
            {
                const string sql = @"
                    SELECT F.ROLE,
                           R.AUTHENTICATION_TYPE,
                           R.COMMON,
                           R.ORACLE_MAINTAINED
                    FROM TABLE(FN_LIST_ROLES) F
                    LEFT JOIN DBA_ROLES R ON R.ROLE = F.ROLE
                    ORDER BY F.ROLE";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd    = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvRoles.Rows.Add(
                                reader["ROLE"].ToString(),
                                reader["AUTHENTICATION_TYPE"].ToString(),
                                reader["COMMON"].ToString(),
                                reader["ORACLE_MAINTAINED"].ToString()
                            );
                        }
                    }
                }

                UIHelper.SetStatus(lblStatus, dgvRoles.Rows.Count + " role trong he thong.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }

        // ── Lọc grid theo ô tìm kiếm ──────────────────────────────────────
        private void FilterGrid()
        {
            string q = txtSearch.Text.Trim().ToUpper();
            foreach (DataGridViewRow row in dgvRoles.Rows)
                row.Visible = string.IsNullOrEmpty(q) ||
                    (row.Cells["ROLE"].Value != null &&
                     row.Cells["ROLE"].Value.ToString().ToUpper().Contains(q));
        }

        // ── Mở dialog tạo role mới ────────────────────────────────────────
        private void OpenCreateDialog()
        {
            using (var dlg = new RoleEditDialog(_connStr))
            {
                if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        // ── Xóa role đang chọn (gọi SP_DROP_ROLE) ────────────────────────
        private void DeleteSelected()
        {
            if (dgvRoles.SelectedRows.Count == 0) return;
            string role = dgvRoles.SelectedRows[0].Cells["ROLE"].Value?.ToString();

            if (MessageBox.Show(
                    "Xac nhan xoa role \"" + role + "\"?", "Xac nhan",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                // Gọi SP_DROP_ROLE(p_rolename) → DROP ROLE ...
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand("SP_DROP_ROLE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_rolename", OracleDbType.Varchar2).Value = role;
                        cmd.ExecuteNonQuery();
                    }
                }
                UIHelper.SetStatus(lblStatus, "Da xoa role " + role, StatusType.Success);
                LoadData();
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }

    // =========================================================================
    // RoleEditDialog – Tạo role mới
    // =========================================================================
    public class RoleEditDialog : Form
    {
        private TextBox  txtRoleName, txtPassword;
        private ComboBox cmbAuth;
        private Button   btnSave, btnCancel;
        private Label    lblStatus;
        private readonly string _connStr;

        public RoleEditDialog(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text            = "Tao Role moi";
            this.Size            = new Size(400, 300);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = UIHelper.LightBg;

            // Title bar
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(30, 50, 80) };
            pnlTitle.Controls.Add(new Label
            {
                Text      = "+ Tao Role moi",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(15, 12)
            });

            var card = UIHelper.CreateCard(15, 55, 360, 155);

            UIHelper.CreateLabeledInput(card, "Ten Role *", 10, 10, 330, out txtRoleName);

            UIHelper.CreateLabeledCombo(card, "Kieu xac thuc", 10, 62, 160, out cmbAuth);
            cmbAuth.Items.AddRange(new object[] { "NOT IDENTIFIED", "BY PASSWORD" });
            cmbAuth.SelectedIndex = 0;
            cmbAuth.SelectedIndexChanged += (s, e) =>
                txtPassword.Visible = cmbAuth.SelectedIndex == 1;

            UIHelper.CreateLabeledInput(card, "Mat khau", 190, 62, 150, out txtPassword, true);
            txtPassword.Visible = false;

            // Status label
            lblStatus = new Label
            {
                Location  = new Point(15, 225),
                Size      = new Size(360, 20),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };

            // Buttons
            btnCancel           = UIHelper.CreateButton("Huy", ButtonStyle.Secondary);
            btnCancel.Size      = new Size(80, 34);
            btnCancel.Location  = new Point(185, 235);
            btnCancel.ForeColor = UIHelper.TextDark;
            btnCancel.Click    += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave          = UIHelper.CreateButton("+ Tao", ButtonStyle.Primary);
            btnSave.Size     = new Size(100, 34);
            btnSave.Location = new Point(275, 235);
            btnSave.Click   += BtnSave_Click;

            this.Controls.Add(pnlTitle);
            this.Controls.Add(card);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        // ── Xử lý nút Tạo ────────────────────────────────────────────────
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoleName.Text))
            {
                UIHelper.SetStatus(lblStatus, "Vui long nhap ten role.", StatusType.Error);
                return;
            }

            // Kiểm tra mật khẩu khi chọn BY PASSWORD
            if (cmbAuth.SelectedIndex == 1 && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                UIHelper.SetStatus(lblStatus, "Vui long nhap mat khau cho role.", StatusType.Error);
                return;
            }

            try
            {
                // ── TẠO ROLE: gọi SP_CREATE_ROLE(p_rolename) ─────────────
                // SP_CREATE_ROLE chỉ nhận tên role; nếu cần password thì
                // thực thi thêm ALTER ROLE ... IDENTIFIED BY sau khi tạo
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    // Bước 1: Tạo role (NOT IDENTIFIED)
                    using (var cmd = new OracleCommand("SP_CREATE_ROLE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_rolename", OracleDbType.Varchar2).Value =
                            txtRoleName.Text.Trim().ToUpper();
                        cmd.ExecuteNonQuery();
                    }

                    // Bước 2: Nếu chọn BY PASSWORD → ALTER ROLE ... IDENTIFIED BY
                    if (cmbAuth.SelectedIndex == 1)
                    {
                        string alterSql = "ALTER ROLE " + txtRoleName.Text.Trim().ToUpper() +
                                          " IDENTIFIED BY \"" + txtPassword.Text + "\"";
                        using (var cmd = new OracleCommand(alterSql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }
}