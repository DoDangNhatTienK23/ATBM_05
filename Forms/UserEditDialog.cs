using System;
using System.Drawing;
using System.Windows.Forms;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class UserEditDialog : Form
    {
        private TextBox txtUsername, txtPassword, txtConfirmPwd, txtDefaultTS, txtTempTS;
        private ComboBox cmbProfile, cmbStatus;
        private CheckBox chkExpire;
        private Button btnSave, btnCancel;
        private Label lblStatus;

        private readonly string _connStr;
        private readonly string _existingUser;
        private bool IsEdit { get { return _existingUser != null; } }

        public UserEditDialog(string connStr, string existingUser)
        {
            _connStr = connStr;
            _existingUser = existingUser;
            InitializeLayout();
            if (IsEdit) LoadUserData();
        }

        private void InitializeLayout()
        {
            this.Text = IsEdit ? "Chinh sua User: " + _existingUser : "Tao moi User";
            this.Size = new Size(460, 490);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UIHelper.LightBg;
            this.Font = new Font("Segoe UI", 9.5f);

            // Title bar
            var pnlTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(30, 50, 80)
            };
            var lblTitleText = IsEdit
                ? "Sua: " + _existingUser
                : "+ Tao User moi";
            pnlTitle.Controls.Add(new Label
            {
                Text = lblTitleText,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 12)
            });

            // Content card
            var card = UIHelper.CreateCard(15, 55, 415, 355);

            int y = 12;

            // Username
            UIHelper.CreateLabeledInput(card, "Ten dang nhap (USERNAME) *", 10, y, 385, out txtUsername);
            y += 52;
            if (IsEdit)
            {
                txtUsername.Text = _existingUser;
                txtUsername.Enabled = false;
                txtUsername.BackColor = Color.FromArgb(235, 235, 240);
            }

            // Password
            UIHelper.CreateLabeledInput(card, IsEdit ? "Mat khau moi (de trong = khong doi)" : "Mat khau *",
                10, y, 185, out txtPassword, true);
            UIHelper.CreateLabeledInput(card, "Xac nhan mat khau",
                210, y, 185, out txtConfirmPwd, true);
            y += 52;

            // Tablespace
            UIHelper.CreateLabeledInput(card, "Default Tablespace", 10, y, 185, out txtDefaultTS);
            txtDefaultTS.Text = "USERS";
            UIHelper.CreateLabeledInput(card, "Temporary Tablespace", 210, y, 185, out txtTempTS);
            txtTempTS.Text = "TEMP";
            y += 52;

            // Profile
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
                Text = "Yeu cau doi mat khau lan dau dang nhap (PASSWORD EXPIRE)",
                Location = new Point(10, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };
            card.Controls.Add(chkExpire);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(15, 418),
                Size = new Size(415, 20),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = UIHelper.TextMuted
            };

            // Buttons
            btnCancel = UIHelper.CreateButton("Huy", ButtonStyle.Secondary);
            btnCancel.Size = new Size(90, 34);
            btnCancel.Location = new Point(245, 428);
            btnCancel.ForeColor = UIHelper.TextDark;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave = UIHelper.CreateButton(IsEdit ? "Luu thay doi" : "+ Tao User", ButtonStyle.Primary);
            btnSave.Size = new Size(130, 34);
            btnSave.Location = new Point(310, 428);
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(pnlTitle);
            this.Controls.Add(card);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadUserData()
        {
            // TODO: Query DBA_USERS WHERE USERNAME = :u
            // SELECT DEFAULT_TABLESPACE, TEMPORARY_TABLESPACE, PROFILE, ACCOUNT_STATUS
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!IsEdit && string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowStatus("Vui long nhap ten dang nhap!", StatusType.Error); return;
            }
            if (!IsEdit && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowStatus("Vui long nhap mat khau!", StatusType.Error); return;
            }
            if (!string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text != txtConfirmPwd.Text)
            {
                ShowStatus("Mat khau xac nhan khong khop!", StatusType.Error); return;
            }

            try
            {
                if (!IsEdit)
                {
                    // CREATE USER
                    string sql = "CREATE USER " + txtUsername.Text.Trim() +
                                 " IDENTIFIED BY \"" + txtPassword.Text + "\"" +
                                 " DEFAULT TABLESPACE " + txtDefaultTS.Text.Trim() +
                                 " TEMPORARY TABLESPACE " + txtTempTS.Text.Trim() +
                                 " PROFILE " + cmbProfile.Text;
                    if (chkExpire.Checked) sql += " PASSWORD EXPIRE";
                    // TODO: Execute sql
                    // Also: GRANT CREATE SESSION TO {username}
                }
                else
                {
                    // ALTER USER
                    if (!string.IsNullOrEmpty(txtPassword.Text))
                    {
                        // TODO: ALTER USER {_existingUser} IDENTIFIED BY "{password}"
                    }
                    if (cmbStatus != null)
                    {
                        string lockSql = cmbStatus.Text == "LOCKED"
                            ? "ALTER USER " + _existingUser + " ACCOUNT LOCK"
                            : "ALTER USER " + _existingUser + " ACCOUNT UNLOCK";
                        // TODO: Execute lockSql
                    }
                    if (chkExpire.Checked)
                    {
                        // TODO: ALTER USER {_existingUser} PASSWORD EXPIRE
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
