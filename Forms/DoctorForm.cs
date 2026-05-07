using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Forms
{
    public class DoctorForm : Form
    {
        private readonly string _connStr;
        private readonly string _username;

        private TabControl tabControl;

        // HSBA tab
        private DataGridView dgvHSBA;
        private TextBox txtHSBA_Ma, txtChanDoan, txtDieuTri, txtKetLuan;
        private Label lblStatusHSBA;

        // Benh nhan tab
        private DataGridView dgvBenhNhan;
        private TextBox txtBN_Ma, txtBN_TienSu, txtBN_TienSuGD, txtBN_DiUng;
        private Label lblStatusBN;

        // HSBA_DV tab
        private DataGridView dgvHSBADV;
        private TextBox txtDV_MAHSBA, txtDV_LoaiDV, txtDV_MAKTV;
        private DateTimePicker dtpDV_Ngay;
        private Label lblStatusDV;

        // DONTHUOC tab
        private DataGridView dgvDonThuoc;
        private TextBox txtDT_MAHSBA, txtDT_TenThuoc, txtDT_LieuDung;
        private DateTimePicker dtpDT_Ngay;
        private Label lblStatusDT;

        public DoctorForm(string connStr, string username)
        {
            _connStr = connStr;
            _username = username;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            Text = "Bác sĩ / Y sĩ - " + _username;
            Size = new Size(1180, 780);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UIHelper.LightBg;

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };

            var tabHSBA = new TabPage("  HSBA của tôi  ") { BackColor = UIHelper.LightBg };
            var tabBN = new TabPage("  Bệnh nhân liên quan  ") { BackColor = UIHelper.LightBg };
            var tabDV = new TabPage("  HSBA_DV  ") { BackColor = UIHelper.LightBg };
            var tabDT = new TabPage("  Đơn thuốc  ") { BackColor = UIHelper.LightBg };

            BuildHSBATab(tabHSBA);
            BuildBenhNhanTab(tabBN);
            BuildHSBADVTab(tabDV);
            BuildDonThuocTab(tabDT);
            tabControl.TabPages.Add(tabHSBA);
            tabControl.TabPages.Add(tabBN);
            tabControl.TabPages.Add(tabDV);
            tabControl.TabPages.Add(tabDT);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = Color.White };
            var btnLogout = UIHelper.CreateButton("Đăng xuất", ButtonStyle.Secondary);
            btnLogout.Location = new Point(15, 10);
            btnLogout.Click += (s, e) => { Close(); new LoginForm().Show(); };
            pnlBottom.Controls.Add(btnLogout);
            
            Controls.Add(pnlBottom);
            Controls.Add(tabControl);
            
        }

        private void BuildHSBATab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1140, 650);

            var cardGrid = UIHelper.CreateCard(10, 10, 1120, 300, "DANH SÁCH HSBA");
            dgvHSBA = UIHelper.CreateGrid();
            dgvHSBA.Dock = DockStyle.Fill;
            dgvHSBA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHSBA.CellClick += (s, e) => FillHSBAFromGrid();
            cardGrid.Controls.Add(dgvHSBA);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1120, 280, "CẬP NHẬT CHẨN ĐOÁN / ĐIỀU TRỊ / KẾT LUẬN");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã HSBA", 15, 30, 140, out txtHSBA_Ma);
            txtHSBA_Ma.ReadOnly = true;

            UIHelper.CreateLabeledInput(cardEdit, "Chẩn đoán", 15, 80, 1080, out txtChanDoan);
            UIHelper.ConfigureMemo(txtChanDoan, 45);

            UIHelper.CreateLabeledInput(cardEdit, "Điều trị", 15, 140, 1080, out txtDieuTri);
            UIHelper.ConfigureMemo(txtDieuTri, 45);

            UIHelper.CreateLabeledInput(cardEdit, "Kết luận", 15, 200, 1080, out txtKetLuan);
            UIHelper.ConfigureMemo(txtKetLuan, 45);

            var btnUpdate = UIHelper.CreateButton("Cập nhật HSBA", ButtonStyle.Primary);
            btnUpdate.Location = new Point(940, 35);
            btnUpdate.Size = new Size(155, 32);
            btnUpdate.Click += (s, e) => UpdateHSBA();
            cardEdit.Controls.Add(btnUpdate);

            lblStatusHSBA = new Label { Location = new Point(15, 610), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusHSBA);

            LoadHSBA();
        }

        private void BuildBenhNhanTab(TabPage page)
        {
            // NOTE: Trước đây phần "CẬP NHẬT TIỀN SỬ / DỊ ỨNG" bị che do chiều cao AutoScrollMinSize quá nhỏ
            // so với tổng height thực tế của các control (cardEdit cao ~230 + status label ở y=560).
            // Tăng AutoScrollMinSize để luôn đủ không gian, tránh bị che / cắt.

           
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1140, 820);

            var cardGrid = UIHelper.CreateCard(10, 10, 1120, 300, "BỆNH NHÂN LIÊN QUAN");
            dgvBenhNhan = UIHelper.CreateGrid();
            dgvBenhNhan.Dock = DockStyle.Fill;
            dgvBenhNhan.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBenhNhan.CellClick += (s, e) => FillBenhNhanFromGrid();
            cardGrid.Controls.Add(dgvBenhNhan);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 330, 1120, 300, "CẬP NHẬT TIỀN SỬ / DỊ ỨNG ");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã BN", 15, 30, 140, out txtBN_Ma);
            txtBN_Ma.ReadOnly = true;

            UIHelper.CreateLabeledInput(cardEdit, "Tiền sử bệnh", 15, 80, 1080, out txtBN_TienSu);
            UIHelper.ConfigureMemo(txtBN_TienSu, 40);
          
            UIHelper.CreateLabeledInput(cardEdit, "Tiền sử bệnh GĐ", 15, 140, 1080, out txtBN_TienSuGD);
            UIHelper.ConfigureMemo(txtBN_TienSuGD, 40);

            UIHelper.CreateLabeledInput(cardEdit, "Dị ứng thuốc", 15, 200, 1080, out txtBN_DiUng);
            UIHelper.ConfigureMemo(txtBN_DiUng, 40);

            var btnUpdate = UIHelper.CreateButton("Cập nhật bệnh nhân", ButtonStyle.Primary);
            btnUpdate.Location = new Point(940, 35);
            btnUpdate.Size = new Size(155, 32);
            btnUpdate.Click += (s, e) => UpdateBenhNhan();
            cardEdit.Controls.Add(btnUpdate);

            // Đặt status nằm sát cardEdit để không bị đè lên bởi AutoScroll + panel bottom
            lblStatusBN = new Label
            {
                Location = new Point(15, cardEdit.Bottom + 10),
                Size = new Size(800, 22),
                Font = new Font("Segoe UI", 8.5f)
            };
            page.Controls.Add(lblStatusBN);

            LoadBenhNhan();
        }

        private void BuildHSBADVTab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1140, 560);

            var cardGrid = UIHelper.CreateCard(10, 10, 1120, 300, "DANH SÁCH HSBA_DV");
            dgvHSBADV = UIHelper.CreateGrid();
            dgvHSBADV.Dock = DockStyle.Fill;
            dgvHSBADV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHSBADV.CellClick += (s, e) => FillHSBADVFromGrid();
            cardGrid.Controls.Add(dgvHSBADV);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1120, 180, "THÊM / XÓA HSBA_DV");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã HSBA", 15, 30, 140, out txtDV_MAHSBA);
            UIHelper.CreateLabeledInput(cardEdit, "Loại DV", 170, 30, 260, out txtDV_LoaiDV);
            var lblNgay = new Label { Text = "Ngày DV", Location = new Point(450, 30), AutoSize = true, ForeColor = UIHelper.TextMuted, Font = new Font("Segoe UI", 8.5f) };
            dtpDV_Ngay = new DateTimePicker { Location = new Point(450, 48), Size = new Size(130, 26), Format = DateTimePickerFormat.Short };
            cardEdit.Controls.Add(lblNgay);
            cardEdit.Controls.Add(dtpDV_Ngay);
            UIHelper.CreateLabeledInput(cardEdit, "Mã KTV", 595, 30, 120, out txtDV_MAKTV);

            var btnInsert = UIHelper.CreateButton("Thêm DV", ButtonStyle.Success);
            btnInsert.Location = new Point(760, 45);
            btnInsert.Size = new Size(100, 32);
            btnInsert.Click += (s, e) => InsertHSBADV();
            cardEdit.Controls.Add(btnInsert);

            var btnDelete = UIHelper.CreateButton("Xóa DV", ButtonStyle.Danger);
            btnDelete.Location = new Point(870, 45);
            btnDelete.Size = new Size(100, 32);
            btnDelete.Click += (s, e) => DeleteHSBADV();
            cardEdit.Controls.Add(btnDelete);

            lblStatusDV = new Label { Location = new Point(15, 510), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusDV);

            LoadHSBADV();
        }

        private void BuildDonThuocTab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1140, 580);

            var cardGrid = UIHelper.CreateCard(10, 10, 1120, 300, "DANH SÁCH ĐƠN THUỐC");
            dgvDonThuoc = UIHelper.CreateGrid();
            dgvDonThuoc.Dock = DockStyle.Fill;
            dgvDonThuoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDonThuoc.CellClick += (s, e) => FillDonThuocFromGrid();
            cardGrid.Controls.Add(dgvDonThuoc);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1120, 200, "THÊM / CẬP NHẬT / XÓA ĐƠN THUỐC");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã HSBA", 15, 30, 140, out txtDT_MAHSBA);
            var lblNgay = new Label { Text = "Ngày ĐT", Location = new Point(170, 30), AutoSize = true, ForeColor = UIHelper.TextMuted, Font = new Font("Segoe UI", 8.5f) };
            dtpDT_Ngay = new DateTimePicker { Location = new Point(170, 48), Size = new Size(120, 26), Format = DateTimePickerFormat.Short };
            cardEdit.Controls.Add(lblNgay);
            cardEdit.Controls.Add(dtpDT_Ngay);
            UIHelper.CreateLabeledInput(cardEdit, "Tên thuốc", 305, 30, 260, out txtDT_TenThuoc);
            UIHelper.CreateLabeledInput(cardEdit, "Liều dùng", 580, 30, 520, out txtDT_LieuDung);

            var btnInsert = UIHelper.CreateButton("Thêm", ButtonStyle.Success);
            btnInsert.Location = new Point(760, 85);
            btnInsert.Size = new Size(80, 32);
            btnInsert.Click += (s, e) => InsertDonThuoc();
            cardEdit.Controls.Add(btnInsert);

            var btnUpdate = UIHelper.CreateButton("Cập nhật liều", ButtonStyle.Primary);
            btnUpdate.Location = new Point(850, 85);
            btnUpdate.Size = new Size(110, 32);
            btnUpdate.Click += (s, e) => UpdateDonThuoc();
            cardEdit.Controls.Add(btnUpdate);

            var btnDelete = UIHelper.CreateButton("Xóa", ButtonStyle.Danger);
            btnDelete.Location = new Point(970, 85);
            btnDelete.Size = new Size(70, 32);
            btnDelete.Click += (s, e) => DeleteDonThuoc();
            cardEdit.Controls.Add(btnDelete);

            lblStatusDT = new Label { Location = new Point(15, 530), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusDT);

            LoadDonThuoc();
        }

        private void LoadHSBA()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                using (var da = new OracleDataAdapter("SELECT * FROM BVDBA.HSBA ORDER BY MAHSBA", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvHSBA.DataSource = dt;
                }
                UIHelper.SetStatus(lblStatusHSBA, "Đã tải danh sách HSBA.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusHSBA, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void LoadBenhNhan()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                using (var da = new OracleDataAdapter("SELECT * FROM BVDBA.BENHNHAN ORDER BY MABN", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvBenhNhan.DataSource = dt;
                }
                UIHelper.SetStatus(lblStatusBN, "Đã tải danh sách bệnh nhân.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusBN, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void LoadHSBADV()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                using (var da = new OracleDataAdapter("SELECT * FROM BVDBA.HSBA_DV ORDER BY MAHSBA, NGAYDV", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvHSBADV.DataSource = dt;
                }
                UIHelper.SetStatus(lblStatusDV, "Đã tải danh sách HSBA_DV.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDV, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void LoadDonThuoc()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                using (var da = new OracleDataAdapter("SELECT * FROM BVDBA.DONTHUOC ORDER BY MAHSBA, NGAYDT", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvDonThuoc.DataSource = dt;
                }
                UIHelper.SetStatus(lblStatusDT, "Đã tải danh sách đơn thuốc.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDT, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void FillHSBAFromGrid()
        {
            if (dgvHSBA.CurrentRow == null) return;
            txtHSBA_Ma.Text = dgvHSBA.CurrentRow.Cells["MAHSBA"].Value?.ToString();
            txtChanDoan.Text = dgvHSBA.CurrentRow.Cells["CHANDOAN"].Value?.ToString();
            txtDieuTri.Text = dgvHSBA.CurrentRow.Cells["DIEUTRI"].Value?.ToString();
            txtKetLuan.Text = dgvHSBA.CurrentRow.Cells["KETLUAN"].Value?.ToString();
        }

        private void FillBenhNhanFromGrid()
        {
            if (dgvBenhNhan.CurrentRow == null) return;
            txtBN_Ma.Text = dgvBenhNhan.CurrentRow.Cells["MABN"].Value?.ToString();
            txtBN_TienSu.Text = dgvBenhNhan.CurrentRow.Cells["TIENSUBENH"].Value?.ToString();
            txtBN_TienSuGD.Text = dgvBenhNhan.CurrentRow.Cells["TIENSUBENHGD"].Value?.ToString();
            txtBN_DiUng.Text = dgvBenhNhan.CurrentRow.Cells["DIUNGTHUOC"].Value?.ToString();
        }

        private void FillHSBADVFromGrid()
        {
            if (dgvHSBADV.CurrentRow == null) return;
            txtDV_MAHSBA.Text = dgvHSBADV.CurrentRow.Cells["MAHSBA"].Value?.ToString();
            txtDV_LoaiDV.Text = dgvHSBADV.CurrentRow.Cells["LOAIDV"].Value?.ToString();
            txtDV_MAKTV.Text = dgvHSBADV.CurrentRow.Cells["MAKTV"].Value?.ToString();
            if (DateTime.TryParse(dgvHSBADV.CurrentRow.Cells["NGAYDV"].Value?.ToString(), out var d))
                dtpDV_Ngay.Value = d;
        }

        private void FillDonThuocFromGrid()
        {
            if (dgvDonThuoc.CurrentRow == null) return;
            txtDT_MAHSBA.Text = dgvDonThuoc.CurrentRow.Cells["MAHSBA"].Value?.ToString();
            txtDT_TenThuoc.Text = dgvDonThuoc.CurrentRow.Cells["TENTHUOC"].Value?.ToString();
            txtDT_LieuDung.Text = dgvDonThuoc.CurrentRow.Cells["LIEUDUNG"].Value?.ToString();
            if (DateTime.TryParse(dgvDonThuoc.CurrentRow.Cells["NGAYDT"].Value?.ToString(), out var d))
                dtpDT_Ngay.Value = d;
        }

        private void UpdateHSBA()
        {
            if (string.IsNullOrWhiteSpace(txtHSBA_Ma.Text))
            {
                UIHelper.SetStatus(lblStatusHSBA, "Vui lòng chọn HSBA.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new OracleCommand("BVDBA.SP_CAP_NHAT_HSBA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_mahsba", OracleDbType.Varchar2).Value = txtHSBA_Ma.Text.Trim();
                        cmd.Parameters.Add("p_chandoan", OracleDbType.NVarchar2).Value = txtChanDoan.Text.Trim();
                        cmd.Parameters.Add("p_dieutri", OracleDbType.NVarchar2).Value = txtDieuTri.Text.Trim();
                        cmd.Parameters.Add("p_ketluan", OracleDbType.NVarchar2).Value = txtKetLuan.Text.Trim();
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBA();
                UIHelper.SetStatus(lblStatusHSBA, "Đã cập nhật HSBA.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusHSBA, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void UpdateBenhNhan()
        {
            if (string.IsNullOrWhiteSpace(txtBN_Ma.Text))
            {
                UIHelper.SetStatus(lblStatusBN, "Vui lòng chọn bệnh nhân.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"UPDATE BVDBA.BENHNHAN
                        SET TIENSUBENH = :ts, TIENSUBENHGD = :tsgd, DIUNGthuoc = :du
                        WHERE MABN = :mabn";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("ts", txtBN_TienSu.Text.Trim());
                        cmd.Parameters.Add("tsgd", txtBN_TienSuGD.Text.Trim());
                        cmd.Parameters.Add("du", txtBN_DiUng.Text.Trim());
                        cmd.Parameters.Add("mabn", txtBN_Ma.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadBenhNhan();
                UIHelper.SetStatus(lblStatusBN, "Đã cập nhật bệnh nhân.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusBN, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void InsertHSBADV()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"INSERT INTO BVDBA.HSBA_DV
                        (MAHSBA, LOAIDV, NGAYDV, MAKTV)
                        VALUES (:mahsba, :ldv, :ngay, :maktv)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mahsba", txtDV_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ldv", txtDV_LoaiDV.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDV_Ngay.Value.Date);
                        cmd.Parameters.Add("maktv", txtDV_MAKTV.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBADV();
                UIHelper.SetStatus(lblStatusDV, "Đã thêm dịch vụ.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDV, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void DeleteHSBADV()
        {
            if (string.IsNullOrWhiteSpace(txtDV_MAHSBA.Text) || string.IsNullOrWhiteSpace(txtDV_LoaiDV.Text))
            {
                UIHelper.SetStatus(lblStatusDV, "Vui lòng chọn dịch vụ cần xóa.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = "DELETE FROM BVDBA.HSBA_DV WHERE MAHSBA = :mahsba AND LOAIDV = :ldv AND NGAYDV = :ngay";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mahsba", txtDV_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ldv", txtDV_LoaiDV.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDV_Ngay.Value.Date);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBADV();
                UIHelper.SetStatus(lblStatusDV, "Đã xóa dịch vụ.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDV, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void InsertDonThuoc()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"INSERT INTO BVDBA.DONTHUOC
                        (MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG)
                        VALUES (:mahsba, :ngay, :tenthuoc, :lieudung)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mahsba", txtDT_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDT_Ngay.Value.Date);
                        cmd.Parameters.Add("tenthuoc", txtDT_TenThuoc.Text.Trim());
                        cmd.Parameters.Add("lieudung", txtDT_LieuDung.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadDonThuoc();
                UIHelper.SetStatus(lblStatusDT, "Đã thêm đơn thuốc.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDT, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void UpdateDonThuoc()
        {
            if (string.IsNullOrWhiteSpace(txtDT_MAHSBA.Text) || string.IsNullOrWhiteSpace(txtDT_TenThuoc.Text))
            {
                UIHelper.SetStatus(lblStatusDT, "Vui lòng chọn đơn thuốc.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"UPDATE BVDBA.DONTHUOC
                        SET LIEUDUNG = :lieu
                        WHERE MAHSBA = :mahsba AND NGAYDT = :ngay AND TENTHUOC = :tenthuoc";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("lieu", txtDT_LieuDung.Text.Trim());
                        cmd.Parameters.Add("mahsba", txtDT_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDT_Ngay.Value.Date);
                        cmd.Parameters.Add("tenthuoc", txtDT_TenThuoc.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadDonThuoc();
                UIHelper.SetStatus(lblStatusDT, "Đã cập nhật liều dùng.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDT, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void DeleteDonThuoc()
        {
            if (string.IsNullOrWhiteSpace(txtDT_MAHSBA.Text) || string.IsNullOrWhiteSpace(txtDT_TenThuoc.Text))
            {
                UIHelper.SetStatus(lblStatusDT, "Vui lòng chọn đơn thuốc.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = "DELETE FROM BVDBA.DONTHUOC WHERE MAHSBA = :mahsba AND NGAYDT = :ngay AND TENTHUOC = :tenthuoc";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mahsba", txtDT_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDT_Ngay.Value.Date);
                        cmd.Parameters.Add("tenthuoc", txtDT_TenThuoc.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadDonThuoc();
                UIHelper.SetStatus(lblStatusDT, "Đã xóa đơn thuốc.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDT, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }
    }
}
