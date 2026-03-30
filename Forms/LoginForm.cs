using System;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtHost, txtPort, txtService, txtUsername, txtPassword;
        private Button btnConnect;
        private Label lblStatus;
        private Panel pnlCard;
        private CheckBox chkSysDba;

        public LoginForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text = "Oracle DB Admin – Đăng nhập";
            this.Size = new Size(460, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 50, 80);
            this.Font = new Font("Segoe UI", 9.5f);

            // Logo/Title area
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.Transparent
            };
            var lblLogo = new Label
            {
                Text = "⚙",
                Font = new Font("Segoe UI", 32f),
                ForeColor = Color.FromArgb(100, 160, 255),
                AutoSize = true,
                Location = new Point(190, 18)
            };
            var lblAppName = new Label
            {
                Text = "Oracle DB Admin Tool",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(120, 68)
            };
            pnlTop.Controls.Add(lblLogo);
            pnlTop.Controls.Add(lblAppName);

            // Card panel
            pnlCard = new Panel
            {
                BackColor = Color.White,
                Size = new Size(380, 420),
                Location = new Point(38, 120)
            };
            RoundPanel(pnlCard);

            int y = 22;
            // Host
            AddFormRow(pnlCard, "Host / IP", ref y, out txtHost);
            txtHost.Text = "localhost";
            // Port
            AddFormRow(pnlCard, "Port", ref y, out txtPort);
            txtPort.Text = "1521";
            // Service Name
            AddFormRow(pnlCard, "Service Name", ref y, out txtService);
            txtService.Text = "XEPDB1";
            // Username
            AddFormRow(pnlCard, "Username", ref y, out txtUsername);
            txtUsername.Text = "SYSTEM";
            // Password
            AddFormRow(pnlCard, "Password", ref y, out txtPassword);
            txtPassword.PasswordChar = '●';

            // SYSDBA checkbox
            chkSysDba = new CheckBox
            {
                Text = "Kết nối với quyền SYSDBA",
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 100, 130),
                Font = new Font("Segoe UI", 9f)
            };
            pnlCard.Controls.Add(chkSysDba);
            y += 30;

            // Connect button
            btnConnect = new Button
            {
                Text = "KẾT NỐI",
                Size = new Size(340, 42),
                Location = new Point(20, y + 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 90, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.MouseEnter += (s, e) => btnConnect.BackColor = Color.FromArgb(20, 70, 170);
            btnConnect.MouseLeave += (s, e) => btnConnect.BackColor = Color.FromArgb(30, 90, 200);
            btnConnect.Click += BtnConnect_Click;
            pnlCard.Controls.Add(btnConnect);

            // Status label
            lblStatus = new Label
            {
                Text = "",
                AutoSize = false,
                Size = new Size(380, 24),
                Location = new Point(38, 555),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 180, 80),
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.Transparent
            };

            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlCard);
            this.Controls.Add(lblStatus);

            // Enter key triggers connect
            this.AcceptButton = btnConnect;
        }

        private void AddFormRow(Panel parent, string labelText, ref int y, out TextBox txt)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(90, 110, 140),
                Font = new Font("Segoe UI", 8.5f)
            };
            y += 20;
            txt = new TextBox
            {
                Location = new Point(20, y),
                Size = new Size(340, 28),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(248, 250, 255)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            y += 40;
        }

        private void RoundPanel(Panel p)
        {
            // Simple border for Classic WinForms
            p.BorderStyle = BorderStyle.FixedSingle;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowStatus("⚠ Vui lòng nhập đầy đủ thông tin!", Color.FromArgb(255, 150, 50));
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "Đang kết nối...";
            lblStatus.Text = "";

            string dbaMode = chkSysDba.Checked ? ";DBA Privilege=SYSDBA" : "";
            string connStr = $"User Id={txtUsername.Text.Trim()};" +
                             $"Password={txtPassword.Text};" +
                             $"Data Source={txtHost.Text.Trim()}:{txtPort.Text.Trim()}/{txtService.Text.Trim()}" +
                             dbaMode;

            // Test connection in background
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                string error = null;
                try
                {
                    using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(connStr))
                    {
                        conn.Open(); // chỉ cần mở là đủ test connection
                    }

                    System.Threading.Thread.Sleep(800); // giả lập delay
                }
                catch (Exception ex) { error = ex.Message; }

                this.Invoke((Action)(() =>
                {
                    btnConnect.Enabled = true;
                    btnConnect.Text = "KẾT NỐI";

                    if (error != null)
                    {
                        ShowStatus($"✗ Lỗi: {error}", Color.FromArgb(255, 100, 100));
                    }
                    else
                    {
                        var main = new MainForm(connStr, txtUsername.Text.Trim().ToUpper());
                        main.Show();
                        this.Hide();
                    }
                }));
            });
        }

        private void ShowStatus(string msg, Color color)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = color;
        }
    }
}