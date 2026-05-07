using System;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Forms
{
    public class LoginForm : Form
    {
        private const string DefaultHost = "localhost";
        private const string DefaultPort = "1521";
        private const string DefaultService = "XEPDB1";

        private TextBox txtUsername, txtPassword;
        private Button btnConnect;
        private Label lblStatus;
        private Panel pnlCard;

        public LoginForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text = "Oracle Hospital Security - Đăng nhập";
            this.Size = new Size(440, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(18, 32, 47);
            this.Font = new Font("Segoe UI", 9.5f);

            // Logo/Title area
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.Transparent
            };
            var lblLogo = new Label
            {
                Text = "+",
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(58, 58),
                Location = new Point(185, 24),
                BackColor = Color.FromArgb(34, 142, 112)
            };
            var lblAppName = new Label
            {
                Text = "Hospital Security",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 30),
                Location = new Point(35, 88)
            };
            var lblSubtitle = new Label
            {
                Text = "Đăng nhập hệ thống Oracle",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(177, 193, 211),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 24),
                Location = new Point(35, 118)
            };
            pnlTop.Controls.Add(lblLogo);
            pnlTop.Controls.Add(lblAppName);
            pnlTop.Controls.Add(lblSubtitle);

            // Card panel
            pnlCard = new Panel
            {
                BackColor = Color.FromArgb(248, 251, 253),
                Size = new Size(360, 275),
                Location = new Point(36, 155)
            };
            RoundPanel(pnlCard);

            int y = 26;
            // Username
            AddFormRow(pnlCard, "Tên đăng nhập", ref y, out txtUsername);
            txtUsername.Text = "BVDBA";
            // Password
            AddFormRow(pnlCard, "Mật khẩu", ref y, out txtPassword);
            txtPassword.PasswordChar = '*';

            // Connect button
            btnConnect = new Button
            {
                Text = "ĐĂNG NHẬP",
                Size = new Size(310, 44),
                Location = new Point(25, y + 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(34, 142, 112),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.MouseEnter += (s, e) => btnConnect.BackColor = Color.FromArgb(26, 121, 96);
            btnConnect.MouseLeave += (s, e) => btnConnect.BackColor = Color.FromArgb(34, 142, 112);
            btnConnect.Click += BtnConnect_Click;
            pnlCard.Controls.Add(btnConnect);

            var lblConnection = new Label
            {
                Text = $"Kết nối mặc định: {DefaultHost}:{DefaultPort}/{DefaultService}",
                AutoSize = false,
                Size = new Size(310, 22),
                Location = new Point(25, y + 65),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(102, 117, 135),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent
            };
            pnlCard.Controls.Add(lblConnection);

            // Status label
            lblStatus = new Label
            {
                Text = "",
                AutoSize = false,
                Size = new Size(360, 54),
                Location = new Point(36, 445),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(245, 177, 83),
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
                Location = new Point(25, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(74, 91, 109),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            y += 23;
            txt = new TextBox
            {
                Location = new Point(25, y),
                Size = new Size(310, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(27, 43, 59)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            y += 52;
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
                ShowStatus("Vui lòng nhập đầy đủ username và password.", Color.FromArgb(245, 177, 83));
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "ĐANG KẾT NỐI...";
            lblStatus.Text = "";

            string loginUsername = txtUsername.Text.Trim();
            string dbaMode = loginUsername.Equals("SYS", StringComparison.OrdinalIgnoreCase)
                ? ";DBA Privilege=SYSDBA"
                : "";
            string connStr = $"User Id={txtUsername.Text.Trim()};" +
                             $"Password={txtPassword.Text};" +
                             $"Data Source={DefaultHost}:{DefaultPort}/{DefaultService}" +
                             dbaMode;

            // Test connection in background
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                string error = null;
                string userRole = null; // Biến lưu vai trò của người dùng
                string upperUsername = loginUsername.ToUpper();

                try
                {
                    using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(connStr))
                    {
                        conn.Open();

                        // Nếu KHÔNG PHẢI là Admin (BVDBA), truy vấn xem họ là ai
                        if (upperUsername != "BVDBA" && upperUsername != "SYS" && upperUsername != "SYSTEM")
                        {
                            if (upperUsername.StartsWith("U") && upperUsername.Length == 2 && char.IsDigit(upperUsername[1]))
                            {
                                userRole = "OLS_TESTER";
                            }
                            else {
                                string sql = "SELECT VAITRO FROM BVDBA.VW_TC1_TOI_LA_AI";
                                using (var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn))
                                {
                                    var result = cmd.ExecuteScalar();
                                    if (result != null)
                                    {
                                        userRole = result.ToString();
                                    }
                                    else
                                    {
                                        error = "Không tìm thấy thông tin định danh của tài khoản này trong hệ thống Bệnh viện!";
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { error = ex.Message; }

                this.Invoke((Action)(() =>
                {
                    btnConnect.Enabled = true;
                    btnConnect.Text = "ĐĂNG NHẬP";

                    if (error != null)
                    {
                        ShowStatus($"Lỗi: {error}", Color.FromArgb(232, 92, 92));
                    }
                    else
                    {
                        // ----------------------------------------------------
                        // BỘ ĐỊNH TUYẾN (ROUTER) RẼ NHÁNH GIAO DIỆN
                        // ----------------------------------------------------
                        if (upperUsername == "BVDBA")
                        {
                            // 1. Mở giao diện Admin (Phân hệ 1)
                            var main = new MainForm(connStr, upperUsername);
                            main.Show();
                        }
                        else if (userRole == "Benh nhan")
                        {
                            // 2. Mở giao diện Bệnh nhân (Yêu cầu 1 - TC#5)
                            // (Chúng ta sẽ tạo PatientForm ở bước tiếp theo)
                            var patientForm = new PatientForm(connStr, upperUsername);
                            patientForm.Show();
                        }
                        else if (userRole == "Ky thuat vien")
                        {
                            // 3. Mở giao diện Kỹ thuật viên (Yêu cầu 1 - TC#4)
                            // (Chúng ta sẽ tạo TechnicianForm ở bước tiếp theo)
                            var ktvForm = new TechnicianForm(connStr, upperUsername);
                            ktvForm.Show();
                        }
                        else if (userRole == "Dieu phoi vien")
                        {
                            // 4. Giao dien Dieu phoi vien (TC#2)
                            var coordinatorForm = new CoordinatorForm(connStr, upperUsername);
                            coordinatorForm.Show();
                        }
                        else if (userRole == "Bac si/Y si")
                        {
                            // 5. Giao dien Bac si / Y si (TC#3)
                            var doctorForm = new DoctorForm(connStr, upperUsername);
                            doctorForm.Show();
                        }
                        else if (userRole == "OLS_TESTER")
                        {
                            // 6. Dành riêng cho U1 -> U8 xem thông báo OLS
                            var olsForm = new NotificationOnlyForm(connStr, upperUsername);
                            olsForm.Show();
                        }
                        else
                        {
                            MessageBox.Show($"Đăng nhập thành công nhưng chưa có giao diện cho vai trò: {userRole}");
                            return; // Tam dung khong an LoginForm
                        }

                        this.Hide(); // Ẩn màn hình đăng nhập
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
