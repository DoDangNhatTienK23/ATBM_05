using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class UserEditDialog : Form
    {
        private TextBox   txtUsername, txtPassword, txtConfirmPwd, txtDefaultTS, txtTempTS;
        private ComboBox  cmbProfile, cmbStatus;
        private CheckBox  chkExpire;
        private Button    btnSave, btnCancel;
        private Label     lblStatus;

        private readonly string _connStr;
        private readonly string _existingUser;
        private bool IsEdit => _existingUser != null;

        public UserEditDialog(string connStr, string existingUser)
        {
            _connStr      = connStr;
            _existingUser = existingUser;
            InitializeLayout();
            if (IsEdit) LoadUserData();
        }

        private void InitializeLayout()
        {
            this.Text            = IsEdit ? "Chinh sua User: " + _existingUser : "Tao moi User";
            this.Size            = new Size(460, 490);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = UIHelper.LightBg;
            this.Font            = new Font("Segoe UI", 9.5f);

            // Title bar
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(30, 50, 80) };
            pnlTitle.Controls.Add(new Label
            {
                Text      = IsEdit ? "Sua: " + _existingUser : "+ Tao User moi",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(15, 12)
            });

            // Content card
            var card = UIHelper.CreateCard(15, 55, 415, 355);
            int y    = 12;

            // Username
            UIHelper.CreateLabeledInput(card, "Ten dang nhap (USERNAME) *", 10, y, 385, out txtUsername);
            y += 52;
            if (IsEdit)
            {
                txtUsername.Text      = _existingUser;
                txtUsername.Enabled   = false;
                txtUsername.BackColor = Color.FromArgb(235, 235, 240);
            }

            // Password
            UIHelper.CreateLabeledInput(card,
                IsEdit ? "Mat khau moi (de trong = khong doi)" : "Mat khau *",
                10, y, 185, out txtPassword, true);
            UIHelper.CreateLabeledInput(card, "Xac nhan mat khau",
                210, y, 185, out txtConfirmPwd, true);
            y += 52;

            // Tablespace
            UIHelper.CreateLabeledInput(card, "Default Tablespace",   10,  y, 185, out txtDefaultTS);
            txtDefaultTS.Text = "BENHVIEN_TBS";
            UIHelper.CreateLabeledInput(card, "Temporary Tablespace", 210, y, 185, out txtTempTS);
            txtTempTS.Text = "TEMP";
            y += 52;

            // Profile + Status (edit mode)
            UIHelper.CreateLabeledCombo(card, "Profile", 10, y, 185, out cmbProfile);
            cmbProfile.Items.AddRange(new object[] { "DEFAULT", "APP_PROFILE" });
            cmbProfile.SelectedIndex = 0;

            if (IsEdit)
            {
                UIHelper.CreateLabeledCombo(card, "Trang thai tai khoan", 210, y, 185, out cmbStatus);
                cmbStatus.Items.AddRange(new object[] { "OPEN", "LOCKED" });
                cmbStatus.SelectedIndex = 0;
            }
            y += 52;

            // Expire checkbox
            chkExpire = new CheckBox
            {
                Text     = "Yeu cau doi mat khau lan dau dang nhap (PASSWORD EXPIRE)",
                Location = new Point(10, y),
                AutoSize = true,
                Font     = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            card.Controls.Add(chkExpire);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(15, 418),
                Size     = new Size(415, 20),
                Font     = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };

            // Buttons
            btnCancel           = UIHelper.CreateButton("Huy", ButtonStyle.Secondary);
            btnCancel.Size      = new Size(90, 34);
            btnCancel.Location  = new Point(245, 428);
            btnCancel.ForeColor = UIHelper.TextDark;
            btnCancel.Click    += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave          = UIHelper.CreateButton(IsEdit ? "Luu thay doi" : "+ Tao User", ButtonStyle.Primary);
            btnSave.Size     = new Size(130, 34);
            btnSave.Location = new Point(310, 428);
            btnSave.Click   += BtnSave_Click;

            this.Controls.Add(pnlTitle);
            this.Controls.Add(card);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        // ── Load thông tin user hiện tại khi ở chế độ Edit ───────────────
        // Truy vấn DBA_USERS để điền sẵn các trường
        private void LoadUserData()
        {
            try
            {
                const string sql = @"
                    SELECT DEFAULT_TABLESPACE,
                           TEMPORARY_TABLESPACE,
                           PROFILE,
                           ACCOUNT_STATUS
                    FROM   DBA_USERS
                    WHERE  USERNAME = :u";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("u", OracleDbType.Varchar2).Value = _existingUser;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtDefaultTS.Text = reader["DEFAULT_TABLESPACE"].ToString();
                                txtTempTS.Text    = reader["TEMPORARY_TABLESPACE"].ToString();

                                // Chọn đúng Profile trong ComboBox (thêm nếu chưa có)
                                string profile = reader["PROFILE"].ToString();
                                if (!cmbProfile.Items.Contains(profile))
                                    cmbProfile.Items.Add(profile);
                                cmbProfile.SelectedItem = profile;

                                // Chọn đúng Status (OPEN / LOCKED)
                                if (cmbStatus != null)
                                {
                                    string st = reader["ACCOUNT_STATUS"].ToString();
                                    cmbStatus.SelectedItem = st.Contains("LOCK") ? "LOCKED" : "OPEN";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Khong the tai thong tin user: " + ex.Message, StatusType.Error);
            }
        }

        // ── Xử lý nút Lưu ────────────────────────────────────────────────
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // --- Validate ---
            if (!IsEdit && string.IsNullOrWhiteSpace(txtUsername.Text))
            { ShowStatus("Vui long nhap ten dang nhap!", StatusType.Error); return; }

            if (!IsEdit && string.IsNullOrWhiteSpace(txtPassword.Text))
            { ShowStatus("Vui long nhap mat khau!", StatusType.Error); return; }

            if (!string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text != txtConfirmPwd.Text)
            { ShowStatus("Mat khau xac nhan khong khop!", StatusType.Error); return; }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();

                    if (!IsEdit)
                    {
                        // ── TẠO USER MỚI: gọi SP_CREATE_USER ─────────────
                        // SP_CREATE_USER(p_username, p_password, p_tablespace)
                        // SP tự GRANT CREATE SESSION sau khi tạo
                        using (var cmd = new OracleCommand("SP_CREATE_USER", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_username",   OracleDbType.Varchar2).Value = txtUsername.Text.Trim().ToUpper();
                            cmd.Parameters.Add("p_password",   OracleDbType.Varchar2).Value = txtPassword.Text;
                            cmd.Parameters.Add("p_tablespace", OracleDbType.Varchar2).Value = txtDefaultTS.Text.Trim();
                            cmd.ExecuteNonQuery();
                        }

                        // PASSWORD EXPIRE nếu được chọn
                        if (chkExpire.Checked)
                        {
                            using (var cmd = new OracleCommand(
                                "ALTER USER " + txtUsername.Text.Trim().ToUpper() + " PASSWORD EXPIRE", conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // ── SỬA USER: gọi SP_ALTER_USER_PASSWORD nếu có mật khẩu mới ──
                        if (!string.IsNullOrEmpty(txtPassword.Text))
                        {
                            using (var cmd = new OracleCommand("SP_ALTER_USER_PASSWORD", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("p_username",    OracleDbType.Varchar2).Value = _existingUser;
                                cmd.Parameters.Add("p_newpassword", OracleDbType.Varchar2).Value = txtPassword.Text;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Khóa / Mở khóa nếu Status thay đổi
                        if (cmbStatus != null)
                        {
                            string action = cmbStatus.Text == "LOCKED" ? "LOCK" : "UNLOCK";
                            using (var cmd = new OracleCommand("SP_LOCK_UNLOCK_USER", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = _existingUser;
                                cmd.Parameters.Add("p_action",   OracleDbType.Varchar2).Value = action;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // PASSWORD EXPIRE nếu được chọn
                        if (chkExpire.Checked)
                        {
                            using (var cmd = new OracleCommand(
                                "ALTER USER " + _existingUser + " PASSWORD EXPIRE", conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowStatus("Loi: " + ex.Message, StatusType.Error);
            }
        }

        private void ShowStatus(string msg, StatusType type)
        {
            UIHelper.SetStatus(lblStatus, msg, type);
        }
    }
}