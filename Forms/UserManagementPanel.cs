using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
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

            // ── Toolbar ────────────────────────────────────────────────────
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

            btnRefresh = UIHelper.CreateButton("Lam moi",    ButtonStyle.Secondary);
            btnCreate  = UIHelper.CreateButton("+ Tao User", ButtonStyle.Primary);
            btnEdit    = UIHelper.CreateButton("Sua",         ButtonStyle.Warning);
            btnDelete  = UIHelper.CreateButton("Xoa",         ButtonStyle.Danger);
            btnLock    = UIHelper.CreateButton("Khoa",        ButtonStyle.Secondary);
            btnUnlock  = UIHelper.CreateButton("Mo khoa",     ButtonStyle.Success);

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

            // ── Grid ───────────────────────────────────────────────────────
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvUsers = UIHelper.CreateGrid();
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "USERNAME",           HeaderText = "Ten dang nhap", FillWeight = 20 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ACCOUNT_STATUS",     HeaderText = "Trang thai",    FillWeight = 15 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "DEFAULT_TABLESPACE", HeaderText = "Tablespace",    FillWeight = 15 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "PROFILE",            HeaderText = "Profile",       FillWeight = 12 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CREATED",            HeaderText = "Ngay tao",      FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "EXPIRY_DATE",        HeaderText = "Ngay het han",  FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "LOCK_DATE",          HeaderText = "Ngay khoa",     FillWeight = 12 });

            dgvUsers.CellFormatting += DgvUsers_CellFormatting;
            pnlGrid.Controls.Add(dgvUsers);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);
            this.Controls.Add(pnlGrid);
        }

        // ── Tô màu cột Trang thai ─────────────────────────────────────────
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

        // ── Load danh sách user từ Oracle qua FN_LIST_USERS ───────────────
        // SQL file: FN_LIST_USERS trả về T_USER_TABLE (pipelined)
        // Cột: USERNAME, ACCOUNT_STATUS, CREATED, DEFAULT_TABLESPACE, PROFILE
        // Thêm EXPIRY_DATE, LOCK_DATE từ DBA_USERS để hiển thị đủ 7 cột
        private void LoadData()
        {
            UIHelper.SetStatus(lblStatus, "Dang tai du lieu...", StatusType.Info);
            dgvUsers.Rows.Clear();

            try
            {
                // Dùng DBA_USERS trực tiếp để lấy đủ 7 cột (FN_LIST_USERS không có EXPIRY_DATE/LOCK_DATE)
                // Lọc bỏ user hệ thống giống FN_LIST_USERS
                const string sql = @"
                    SELECT USERNAME,
                           ACCOUNT_STATUS,
                           DEFAULT_TABLESPACE,
                           PROFILE,
                           TO_CHAR(CREATED,     'DD/MM/YYYY') AS CREATED,
                           TO_CHAR(EXPIRY_DATE, 'DD/MM/YYYY') AS EXPIRY_DATE,
                           TO_CHAR(LOCK_DATE,   'DD/MM/YYYY') AS LOCK_DATE
                    FROM   DBA_USERS
                    WHERE  USERNAME NOT IN (
                        'SYS','SYSTEM','DBSNMP','APPQOSSYS','AUDSYS','CTXSYS',
                        'DVSYS','GSMADMIN_INTERNAL','LBACSYS','MDSYS','OJVMSYS',
                        'OLAPSYS','ORDDATA','ORDSYS','OUTLN','REMOTE_SCHEDULER_AGENT',
                        'SI_INFORMTN_SCHEMA','SYS$UMF','SYSBACKUP','SYSDG','SYSKM',
                        'SYSRAC','WMSYS','XDB','XS$NULL'
                    )
                    ORDER BY USERNAME";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvUsers.Rows.Add(
                                reader["USERNAME"].ToString(),
                                reader["ACCOUNT_STATUS"].ToString(),
                                reader["DEFAULT_TABLESPACE"].ToString(),
                                reader["PROFILE"].ToString(),
                                reader["CREATED"].ToString(),
                                reader["EXPIRY_DATE"].ToString(),
                                reader["LOCK_DATE"].ToString()
                            );
                        }
                    }
                }

                UIHelper.SetStatus(lblStatus, "Tong cong " + dgvUsers.Rows.Count + " user.", StatusType.Success);
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
            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                row.Visible = string.IsNullOrEmpty(q) ||
                    (row.Cells["USERNAME"].Value != null &&
                     row.Cells["USERNAME"].Value.ToString().ToUpper().Contains(q));
            }
        }

        // ── Mở dialog tạo user mới ────────────────────────────────────────
        private void OpenCreateDialog()
        {
            using (var dlg = new UserEditDialog(_connStr, null))
            {
                if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        // ── Mở dialog sửa user đang chọn ─────────────────────────────────
        private void OpenEditDialog()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui long chon user can sua.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();
            using (var dlg = new UserEditDialog(_connStr, username))
            {
                if (dlg.ShowDialog() == DialogResult.OK) LoadData();
            }
        }

        // ── Xóa user đang chọn (gọi SP_DROP_USER) ────────────────────────
        private void DeleteSelected()
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();

            if (MessageBox.Show(
                    "Xac nhan xoa user \"" + username + "\"?\nHanh dong nay khong the hoan tac!",
                    "Xac nhan xoa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                // Gọi SP_DROP_USER(p_username) → DROP USER ... CASCADE
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand("SP_DROP_USER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                        cmd.ExecuteNonQuery();
                    }
                }
                UIHelper.SetStatus(lblStatus, "Da xoa user " + username, StatusType.Success);
                LoadData();
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }

        // ── Khóa / Mở khóa user (gọi SP_LOCK_UNLOCK_USER) ────────────────
        // unlock = false → LOCK | unlock = true → UNLOCK
        private void ToggleLock(bool unlock)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            string username = dgvUsers.SelectedRows[0].Cells["USERNAME"].Value?.ToString();
            string action   = unlock ? "UNLOCK" : "LOCK";
            string msgVN    = unlock ? "mo khoa" : "khoa";

            if (MessageBox.Show(
                    "Xac nhan " + msgVN + " user \"" + username + "\"?", "Xac nhan",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // Gọi SP_LOCK_UNLOCK_USER(p_username, p_action)
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand("SP_LOCK_UNLOCK_USER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                        cmd.Parameters.Add("p_action",   OracleDbType.Varchar2).Value = action;
                        cmd.ExecuteNonQuery();
                    }
                }
                UIHelper.SetStatus(lblStatus, "Da " + msgVN + " user " + username, StatusType.Success);
                LoadData();
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }
}