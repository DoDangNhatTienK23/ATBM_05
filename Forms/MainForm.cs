using System;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Forms
{
    public class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblConnInfo;

        private Button btnUsers;
        private Button btnRoles;
        private Button btnGrantPriv;
        private Button btnRevokePriv;
        private Button btnViewPriv;
        private Button btnLogout;

        private UserManagementPanel _userPanel;
        private RoleManagementPanel _rolePanel;
        private GrantPrivilegePanel _grantPanel;
        private RevokePrivilegePanel _revokePanel;
        private ViewPrivilegePanel _viewPanel;

        public string ConnectionString { get; set; }
        public string ConnectedUser { get; set; }

        public MainForm(string connectionString, string connectedUser)
        {
            ConnectionString = connectionString;
            ConnectedUser = connectedUser;
            InitializeLayout();
            ShowPanel(btnUsers);
        }

        private void InitializeLayout()
        {
            this.Text = "Oracle DB Admin Tool";
            this.Size = new Size(1200, 750);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Font = new Font("Segoe UI", 9f);

            // ── Header ──────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(30, 50, 80)
            };

            lblTitle = new Label
            {
                Text = "Oracle DB Admin",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 14)
            };

            lblConnInfo = new Label
            {
                Text = "Ket noi: " + ConnectedUser,
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(900, 19)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblConnInfo);

            // ── Sidebar ──────────────────────────────────────────────────────
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = Color.FromArgb(22, 40, 65)
            };

            int topY = 20;
            btnUsers      = CreateSidebarButton("  Quan ly User",    topY); topY += 50;
            btnRoles      = CreateSidebarButton("  Quan ly Role",    topY); topY += 50;
            btnGrantPriv  = CreateSidebarButton("  Cap quyen",       topY); topY += 50;
            btnRevokePriv = CreateSidebarButton("  Thu hoi quyen",   topY); topY += 50;
            btnViewPriv   = CreateSidebarButton("  Xem quyen",       topY); topY += 50;

            btnUsers.Click      += (s, e) => ShowPanel(btnUsers);
            btnRoles.Click      += (s, e) => ShowPanel(btnRoles);
            btnGrantPriv.Click  += (s, e) => ShowPanel(btnGrantPriv);
            btnRevokePriv.Click += (s, e) => ShowPanel(btnRevokePriv);
            btnViewPriv.Click   += (s, e) => ShowPanel(btnViewPriv);

            pnlSidebar.Controls.Add(btnUsers);
            pnlSidebar.Controls.Add(btnRoles);
            pnlSidebar.Controls.Add(btnGrantPriv);
            pnlSidebar.Controls.Add(btnRevokePriv);
            pnlSidebar.Controls.Add(btnViewPriv);

            var sep = new Panel
            {
                Location = new Point(15, topY + 5),
                Size = new Size(180, 1),
                BackColor = Color.FromArgb(60, 80, 110)
            };
            pnlSidebar.Controls.Add(sep);

            btnLogout = CreateSidebarButton("  Dang xuat", topY + 15);
            btnLogout.ForeColor = Color.FromArgb(255, 140, 140);
            btnLogout.Click += (s, e) => Logout();
            pnlSidebar.Controls.Add(btnLogout);

            // ── Content ──────────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
        }

        private Button CreateSidebarButton(string text, int top)
        {
            var btn = new Button
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 220, 255),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(210, 44),
                Location = new Point(0, top),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 70, 110);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 90, 140);
            return btn;
        }

        private void SetActiveButton(Button active)
        {
            Button[] all = { btnUsers, btnRoles, btnGrantPriv, btnRevokePriv, btnViewPriv };
            foreach (var b in all)
            {
                b.BackColor = Color.Transparent;
                b.ForeColor = Color.FromArgb(200, 220, 255);
                b.Font = new Font("Segoe UI", 9.5f);
            }
            active.BackColor = Color.FromArgb(45, 90, 160);
            active.ForeColor = Color.White;
            active.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }

        private void ShowPanel(Button sender)
        {
            SetActiveButton(sender);
            pnlContent.Controls.Clear();

            UserControl panel = null;

            if (sender == btnUsers)
            {
                if (_userPanel == null) _userPanel = new UserManagementPanel(ConnectionString);
                panel = _userPanel;
            }
            else if (sender == btnRoles)
            {
                if (_rolePanel == null) _rolePanel = new RoleManagementPanel(ConnectionString);
                panel = _rolePanel;
            }
            else if (sender == btnGrantPriv)
            {
                if (_grantPanel == null) _grantPanel = new GrantPrivilegePanel(ConnectionString);
                panel = _grantPanel;
            }
            else if (sender == btnRevokePriv)
            {
                if (_revokePanel == null) _revokePanel = new RevokePrivilegePanel(ConnectionString);
                panel = _revokePanel;
            }
            else if (sender == btnViewPriv)
            {
                if (_viewPanel == null) _viewPanel = new ViewPrivilegePanel(ConnectionString);
                panel = _viewPanel;
            }

            if (panel != null)
            {
                panel.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(panel);
            }
        }

        private void Logout()
        {
            if (MessageBox.Show("Ban co chac muon dang xuat?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var login = new LoginForm();
                login.Show();
                this.Close();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1088, 969);
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
    }
}
