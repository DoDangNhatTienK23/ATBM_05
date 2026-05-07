using System;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Forms
{
    public class NotificationOnlyForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblConnInfo;
        private Button btnLogout;
        private Panel pnlContent;

        private string ConnectionString { get; set; }
        private string ConnectedUser { get; set; }

        public NotificationOnlyForm(string connectionString, string connectedUser)
        {
            ConnectionString = connectionString;
            ConnectedUser = connectedUser;
            InitializeLayout();
            LoadThongBaoPanel();
        }

        private void InitializeLayout()
        {
            this.Text = "Hệ thống Xem Thông Báo (OLS Test)";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // ── Header ──────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(34, 142, 112) // Màu xanh bệnh viện giống LoginForm
            };

            lblTitle = new Label
            {
                Text = "BẢNG THÔNG BÁO BỆNH VIỆN",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            lblConnInfo = new Label
            {
                Text = "Đang đăng nhập: " + ConnectedUser,
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 10f),
                AutoSize = true,
                Location = new Point(650, 20)
            };

            btnLogout = new Button
            {
                Text = "Đăng xuất",
                Size = new Size(100, 35),
                Location = new Point(860, 12),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 92, 92),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => Logout();

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblConnInfo);
            pnlHeader.Controls.Add(btnLogout);

            // ── Content ──────────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private void LoadThongBaoPanel()
        {
            // Nhúng ThongBaoPanel bạn đã viết sẵn vào đây
            var thongBaoPanel = new ThongBaoPanel(ConnectionString);
            thongBaoPanel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(thongBaoPanel);
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var login = new LoginForm();
                login.Show();
                this.Close();
            }
        }
    }
}