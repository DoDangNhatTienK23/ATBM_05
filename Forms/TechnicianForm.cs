using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class TechnicianForm : Form
    {
        private readonly string _connStr;
        private readonly string _username;
        private TabControl tabControl;

        // Tab 1 Controls
        private DataGridView dgvServices;
        private TextBox txtKetQua;
        private Label lblStatus;

        // Tab 2 Controls
        private TextBox txtMaNV, txtHoTen, txtPhai, txtNgaySinh, txtCMND, txtVaiTro, txtChuyenKhoa;
        private TextBox txtQueQuan, txtSoDT;
        private Label lblProfileStatus;

        public TechnicianForm(string connStr, string username)
        {
            _connStr = connStr;
            _username = username;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text = "Kỹ thuật viên - " + _username;
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIHelper.LightBg;

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };

            var tabServices = new TabPage("  Dịch vụ được phân công (TC#4)  ") { BackColor = UIHelper.LightBg };
            BuildServicesTab(tabServices);

            var tabProfile = new TabPage("  Thông tin cá nhân (TC#5)  ") { BackColor = UIHelper.LightBg };
            BuildProfileTab(tabProfile);

            tabControl.TabPages.Add(tabServices);
            tabControl.TabPages.Add(tabProfile);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            var btnLogout = UIHelper.CreateButton("Đăng xuất", ButtonStyle.Secondary);
            btnLogout.Location = new Point(20, 10);
            btnLogout.Click += (s, e) => { this.Close(); new LoginForm().Show(); };
            pnlBottom.Controls.Add(btnLogout);

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlBottom);
        }

        // ========================================================
        // TAB 1: SERVICES (TC#4)
        // ========================================================
        private void BuildServicesTab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(980, 620);

            var pnlTop = UIHelper.CreateCard(10, 10, 950, 350, "DANH SÁCH CHỈ ĐỊNH DỊCH VỤ");
            dgvServices = UIHelper.CreateGrid();
            dgvServices.Dock = DockStyle.Fill;
            dgvServices.CellClick += DgvServices_CellClick;

            // Bind the event to apply sorting rules AFTER data is loaded
            dgvServices.DataBindingComplete += DgvServices_DataBindingComplete;

            pnlTop.Controls.Add(dgvServices);
            page.Controls.Add(pnlTop);

            var pnlEdit = UIHelper.CreateCard(10, 370, 950, 200, "CẬP NHẬT KẾT QUẢ");
            page.Controls.Add(pnlEdit);

            UIHelper.CreateLabeledInput(pnlEdit, "NHẬP KẾT QUẢ DỊCH VỤ TẠI ĐÂY", 20, 35, 910, out txtKetQua);
            UIHelper.ConfigureMemo(txtKetQua, 80);

            var btnSave = UIHelper.CreateButton("LƯU KẾT QUẢ", ButtonStyle.Success);
            btnSave.Location = new Point(730, 145);
            btnSave.Size = new Size(200, 40);
            btnSave.Click += BtnSave_Click;
            pnlEdit.Controls.Add(btnSave);

            lblStatus = UIHelper.CreateStatusLabel(page);
            lblStatus.Location = new Point(15, 580);

            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    var da = new OracleDataAdapter("SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA FROM BVDBA.VW_KTV_HSBA_DV", conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvServices.DataSource = dt;

                    // Basic grid settings
                    dgvServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvServices.AllowUserToResizeColumns = false;
                    dgvServices.AllowUserToResizeRows = false;
                    dgvServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvServices.ReadOnly = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
        }

        // Apply formatting & restrict sorting here to bypass WinForms auto-reset
        private void DgvServices_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // 1. Disable sort for ALL columns first
            foreach (DataGridViewColumn col in dgvServices.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // 2. Format specific columns safely
            if (dgvServices.Columns.Contains("MAHSBA"))
            {
                dgvServices.Columns["MAHSBA"].HeaderText = "Mã HSBA";
                dgvServices.Columns["MAHSBA"].FillWeight = 22;
            }
            if (dgvServices.Columns.Contains("LOAIDV"))
            {
                dgvServices.Columns["LOAIDV"].HeaderText = "Loại Dịch Vụ";
                dgvServices.Columns["LOAIDV"].FillWeight = 23;
            }
            if (dgvServices.Columns.Contains("NGAYDV"))
            {
                dgvServices.Columns["NGAYDV"].HeaderText = "Ngày Yêu Cầu";
                dgvServices.Columns["NGAYDV"].FillWeight = 15;
                // ONLY enable sort for date column
                dgvServices.Columns["NGAYDV"].SortMode = DataGridViewColumnSortMode.Automatic;
            }
            if (dgvServices.Columns.Contains("MAKTV"))
            {
                dgvServices.Columns["MAKTV"].HeaderText = "Mã KTV";
                dgvServices.Columns["MAKTV"].FillWeight = 10;
            }
            if (dgvServices.Columns.Contains("KETQUA"))
            {
                dgvServices.Columns["KETQUA"].HeaderText = "Kết Quả Hiện Tại";
                dgvServices.Columns["KETQUA"].FillWeight = 30;
            }
        }

        private void DgvServices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtKetQua.Text = dgvServices.Rows[e.RowIndex].Cells["KETQUA"].Value?.ToString() ?? "";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (dgvServices.CurrentRow == null) return;

            string maHSBA = dgvServices.CurrentRow.Cells["MAHSBA"].Value.ToString();
            string loaiDV = dgvServices.CurrentRow.Cells["LOAIDV"].Value.ToString();
            DateTime ngayDV = Convert.ToDateTime(dgvServices.CurrentRow.Cells["NGAYDV"].Value);

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    string sql = "UPDATE BVDBA.VW_KTV_HSBA_DV SET KETQUA = :kq WHERE MAHSBA = :id AND LOAIDV = :type AND NGAYDV = :d";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("kq", txtKetQua.Text);
                        cmd.Parameters.Add("id", maHSBA);
                        cmd.Parameters.Add("type", loaiDV);
                        cmd.Parameters.Add("d", ngayDV);
                        cmd.ExecuteNonQuery();
                        UIHelper.SetStatus(lblStatus, "Đã cập nhật kết quả thành công!", StatusType.Success);
                        LoadServices();
                    }
                }
            }
            catch (Exception ex) { UIHelper.SetStatus(lblStatus, ex.Message, StatusType.Error); }
        }

        // ========================================================
        // TAB 2: PROFILE (TC#5)
        // ========================================================
        private void BuildProfileTab(TabPage page)
        {
            var cardBasic = UIHelper.CreateCard(15, 15, 940, 160, "THÔNG TIN CƠ BẢN (CHÉO)");
            page.Controls.Add(cardBasic);

            UIHelper.CreateLabeledInput(cardBasic, "Mã NV", 20, 40, 150, out txtMaNV);
            UIHelper.CreateLabeledInput(cardBasic, "Họ tên", 190, 40, 250, out txtHoTen);
            UIHelper.CreateLabeledInput(cardBasic, "Phái", 460, 40, 100, out txtPhai);
            UIHelper.CreateLabeledInput(cardBasic, "Ngày sinh", 580, 40, 150, out txtNgaySinh);
            UIHelper.CreateLabeledInput(cardBasic, "CMND/CCCD", 750, 40, 170, out txtCMND);

            UIHelper.CreateLabeledInput(cardBasic, "Vai trò", 20, 100, 250, out txtVaiTro);
            UIHelper.CreateLabeledInput(cardBasic, "Chuyên khoa", 290, 100, 250, out txtChuyenKhoa);

            // Read-only fields
            txtMaNV.ReadOnly = txtHoTen.ReadOnly = txtPhai.ReadOnly = txtNgaySinh.ReadOnly =
            txtCMND.ReadOnly = txtVaiTro.ReadOnly = txtChuyenKhoa.ReadOnly = true;

            var cardEdit = UIHelper.CreateCard(15, 190, 940, 140, "THÔNG TIN LIÊN LẠC (ĐƯỢC SỬA)");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Quê quán", 20, 40, 450, out txtQueQuan);
            UIHelper.CreateLabeledInput(cardEdit, "Số điện thoại", 490, 40, 250, out txtSoDT);

            // Moved button down to avoid overlap & adjusted size
            var btnUpdateProfile = UIHelper.CreateButton("CẬP NHẬT LIÊN LẠC", ButtonStyle.Primary);
            btnUpdateProfile.Location = new Point(560, 90);
            btnUpdateProfile.Size = new Size(180, 35);
            btnUpdateProfile.Click += BtnUpdateProfile_Click;
            cardEdit.Controls.Add(btnUpdateProfile);

            lblProfileStatus = new Label { Location = new Point(20, 350), Size = new Size(600, 30), Font = new Font("Segoe UI", 9.5f) };
            page.Controls.Add(lblProfileStatus);

            LoadProfileData();
        }

        private void LoadProfileData()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    string sql = "SELECT * FROM BVDBA.VW_NV_THONGTIN_CANHAN";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtMaNV.Text = reader["MANV"].ToString();
                            txtHoTen.Text = reader["HOTEN"].ToString();
                            txtPhai.Text = reader["PHAI"].ToString();
                            txtNgaySinh.Text = Convert.ToDateTime(reader["NGAYSINH"]).ToString("dd/MM/yyyy");
                            txtCMND.Text = reader["CMND"].ToString();
                            txtVaiTro.Text = reader["VAITRO"].ToString();
                            txtChuyenKhoa.Text = reader["CHUYENKHOA"].ToString();

                            // Editable fields
                            txtQueQuan.Text = reader["QUEQUAN"].ToString();
                            txtSoDT.Text = reader["SODT"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { UIHelper.SetStatus(lblProfileStatus, "Lỗi: " + ex.Message, StatusType.Error); }
        }

        private void BtnUpdateProfile_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    string sql = "UPDATE BVDBA.VW_NV_THONGTIN_CANHAN SET QUEQUAN = :qq, SODT = :sdt";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("qq", txtQueQuan.Text);
                        cmd.Parameters.Add("sdt", txtSoDT.Text);
                        cmd.ExecuteNonQuery();
                        UIHelper.SetStatus(lblProfileStatus, "Đã cập nhật hồ sơ thành công!", StatusType.Success);
                    }
                }
            }
            catch (Exception ex) { UIHelper.SetStatus(lblProfileStatus, "Lỗi: " + ex.Message, StatusType.Error); }
        }
    }
}
