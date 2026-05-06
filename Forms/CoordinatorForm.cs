using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class CoordinatorForm : Form
    {
        private readonly string _connStr;
        private readonly string _username;

        private TabControl tabControl;

        // Tab BENHNHAN
        private DataGridView dgvBenhNhan;
        private TextBox txtMaBN, txtTenBN, txtSoNha, txtTenDuong, txtQuanHuyen, txtTinhTP;
        private TextBox txtTienSu, txtTienSuGD, txtDiUng, txtCCCD, txtOracleUser;
        private ComboBox cmbPhai;
        private DateTimePicker dtpNgaySinh;
        private Label lblStatusBN;

        // Tab HSBA
        private DataGridView dgvHSBA;
        private TextBox txtMaHSBA, txtHSBA_MABN, txtMABS, txtMaKhoa;
        private DateTimePicker dtpHSBA_Ngay;
        private Label lblStatusHSBA;

        // Tab HSBA_DV
        private DataGridView dgvHSBADV;
        private TextBox txtDV_MAHSBA, txtDV_LoaiDV, txtDV_MAKTV;
        private DateTimePicker dtpDV_NgayDV;
        private Label lblStatusDV;

        public CoordinatorForm(string connStr, string username)
        {
            _connStr = connStr;
            _username = username;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            Text = "Điều phối viên - " + _username;
            Size = new Size(1150, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UIHelper.LightBg;

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };

            var tabBN = new TabPage("  Bệnh nhân (TC#2)  ") { BackColor = UIHelper.LightBg };
            var tabHSBA = new TabPage("  Hồ sơ bệnh án  ") { BackColor = UIHelper.LightBg };
            var tabDV = new TabPage("  Điều phối KTV (HSBA_DV)  ") { BackColor = UIHelper.LightBg };

            BuildBenhNhanTab(tabBN);
            BuildHSBATab(tabHSBA);
            BuildHSBADVTab(tabDV);

            tabControl.TabPages.Add(tabBN);
            tabControl.TabPages.Add(tabHSBA);
            tabControl.TabPages.Add(tabDV);

            var tabThongBao = new TabPage("  Thông báo  ") { BackColor = UIHelper.LightBg };
            tabThongBao.Controls.Add(new ThongBaoPanel(_connStr));
            tabControl.TabPages.Add(tabThongBao);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = Color.White };
            var btnLogout = UIHelper.CreateButton("Đăng xuất", ButtonStyle.Secondary);
            btnLogout.Location = new Point(15, 10);
            btnLogout.Click += (s, e) => { Close(); new LoginForm().Show(); };
            pnlBottom.Controls.Add(btnLogout);

            Controls.Add(tabControl);
            Controls.Add(pnlBottom);
        }

        private void BuildBenhNhanTab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1120, 730);

            var cardGrid = UIHelper.CreateCard(10, 10, 1090, 300, "DANH SÁCH BỆNH NHÂN");
            dgvBenhNhan = UIHelper.CreateGrid();
            dgvBenhNhan.Dock = DockStyle.None;
            const int headerHeight = 28;
            dgvBenhNhan.Location = new Point(0, headerHeight);
            dgvBenhNhan.Size = new Size(cardGrid.Width - 2, cardGrid.Height - headerHeight - 2);
            dgvBenhNhan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvBenhNhan.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBenhNhan.CellClick += (s, e) => FillBenhNhanFromGrid();
            cardGrid.Controls.Add(dgvBenhNhan);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1090, 360, "THÊM / CẬP NHẬT BỆNH NHÂN");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã BN", 15, 30, 120, out txtMaBN);
            UIHelper.CreateLabeledInput(cardEdit, "Họ tên", 150, 30, 230, out txtTenBN);
            UIHelper.CreateLabeledCombo(cardEdit, "Phái", 395, 30, 90, out cmbPhai);
            cmbPhai.Items.AddRange(new object[] { "Nam", "Nu" });
            cmbPhai.SelectedIndex = 0;
            var lblNgaySinh = new Label { Text = "Ngày sinh", Location = new Point(500, 30), AutoSize = true, ForeColor = UIHelper.TextMuted, Font = new Font("Segoe UI", 8.5f) };
            dtpNgaySinh = new DateTimePicker { Location = new Point(500, 48), Size = new Size(150, 26), Format = DateTimePickerFormat.Short };
            cardEdit.Controls.Add(lblNgaySinh);
            cardEdit.Controls.Add(dtpNgaySinh);
            UIHelper.CreateLabeledInput(cardEdit, "CCCD", 665, 30, 150, out txtCCCD);
            UIHelper.CreateLabeledInput(cardEdit, "Oracle User", 830, 30, 200, out txtOracleUser);

            UIHelper.CreateLabeledInput(cardEdit, "Số nhà", 15, 90, 90, out txtSoNha);
            UIHelper.CreateLabeledInput(cardEdit, "Tên đường", 120, 90, 200, out txtTenDuong);
            UIHelper.CreateLabeledInput(cardEdit, "Quận/Huyện", 335, 90, 160, out txtQuanHuyen);
            UIHelper.CreateLabeledInput(cardEdit, "Tỉnh/TP", 510, 90, 160, out txtTinhTP);

            UIHelper.CreateLabeledInput(cardEdit, "Tiền sử bệnh", 15, 150, 1015, out txtTienSu);
            UIHelper.ConfigureMemo(txtTienSu, 42);
            UIHelper.CreateLabeledInput(cardEdit, "Tiền sử bệnh GĐ", 15, 210, 1015, out txtTienSuGD);
            UIHelper.ConfigureMemo(txtTienSuGD, 42);
            UIHelper.CreateLabeledInput(cardEdit, "Dị ứng thuốc", 15, 270, 1015, out txtDiUng);
            UIHelper.ConfigureMemo(txtDiUng, 42);

            var btnNew = UIHelper.CreateButton("Nhập mới", ButtonStyle.Secondary);
            btnNew.Size = new Size(90, 32);
            btnNew.Click += (s, e) => ClearBenhNhanInputs();
            cardEdit.Controls.Add(btnNew);

            var btnInsert = UIHelper.CreateButton("Thêm", ButtonStyle.Success);
            btnInsert.Size = new Size(80, 32);
            btnInsert.Click += (s, e) => InsertBenhNhan();
            cardEdit.Controls.Add(btnInsert);

            var btnUpdate = UIHelper.CreateButton("Cập nhật", ButtonStyle.Primary);
            btnUpdate.Size = new Size(90, 32);
            btnUpdate.Click += (s, e) => UpdateBenhNhan();
            cardEdit.Controls.Add(btnUpdate);

            var btnY = txtDiUng.Bottom + 6;
            btnUpdate.Location = new Point(cardEdit.Width - 20 - btnUpdate.Width, btnY);
            btnInsert.Location = new Point(btnUpdate.Left - 10 - btnInsert.Width, btnY);
            btnNew.Location = new Point(btnInsert.Left - 10 - btnNew.Width, btnY);

            lblStatusBN = new Label { Location = new Point(15, 690), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusBN);

            LoadBenhNhan();
        }

        private void BuildHSBATab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1120, 590);

            var cardGrid = UIHelper.CreateCard(10, 10, 1090, 300, "DANH SÁCH HSBA");
            dgvHSBA = UIHelper.CreateGrid();
            dgvHSBA.Dock = DockStyle.None;
            const int hsbaHeaderHeight = 28;
            dgvHSBA.Location = new Point(0, hsbaHeaderHeight);
            dgvHSBA.Size = new Size(cardGrid.Width - 2, cardGrid.Height - hsbaHeaderHeight - 2);
            dgvHSBA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvHSBA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHSBA.CellClick += (s, e) => FillHSBAFromGrid();
            cardGrid.Controls.Add(dgvHSBA);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1090, 210, "TẠO / CẬP NHẬT HSBA");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã HSBA", 15, 30, 140, out txtMaHSBA);
            UIHelper.CreateLabeledInput(cardEdit, "Mã BN", 170, 30, 120, out txtHSBA_MABN);

            var lblNgay = new Label { Text = "Ngày", Location = new Point(310, 30), AutoSize = true, ForeColor = UIHelper.TextMuted, Font = new Font("Segoe UI", 8.5f) };
            dtpHSBA_Ngay = new DateTimePicker { Location = new Point(310, 48), Size = new Size(130, 26), Format = DateTimePickerFormat.Short };
            cardEdit.Controls.Add(lblNgay);
            cardEdit.Controls.Add(dtpHSBA_Ngay);

            UIHelper.CreateLabeledInput(cardEdit, "Mã BS", 455, 30, 120, out txtMABS);
            UIHelper.CreateLabeledInput(cardEdit, "Mã khoa", 590, 30, 120, out txtMaKhoa);

            var btnNew = UIHelper.CreateButton("Nhập mới", ButtonStyle.Secondary);
            btnNew.Location = new Point(760, 45);
            btnNew.Size = new Size(90, 32);
            btnNew.Click += (s, e) => ClearHSBAInputs();
            cardEdit.Controls.Add(btnNew);

            var btnInsert = UIHelper.CreateButton("Thêm HSBA", ButtonStyle.Success);
            btnInsert.Location = new Point(860, 45);
            btnInsert.Size = new Size(110, 32);
            btnInsert.Click += (s, e) => InsertHSBA();
            cardEdit.Controls.Add(btnInsert);

            var btnUpdate = UIHelper.CreateButton("Cập nhật BS/Khoa", ButtonStyle.Primary);
            btnUpdate.Location = new Point(760, 90);
            btnUpdate.Size = new Size(210, 32);
            btnUpdate.Click += (s, e) => UpdateHSBA_Assign();
            cardEdit.Controls.Add(btnUpdate);

            lblStatusHSBA = new Label { Location = new Point(15, 540), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusHSBA);

            LoadHSBA();
        }

        private void BuildHSBADVTab(TabPage page)
        {
            page.AutoScroll = true;
            page.AutoScrollMinSize = new Size(1120, 560);

            var cardGrid = UIHelper.CreateCard(10, 10, 1090, 300, "DANH SÁCH HSBA_DV");
            dgvHSBADV = UIHelper.CreateGrid();
            dgvHSBADV.Dock = DockStyle.None;
            const int dvHeaderHeight = 28;
            dgvHSBADV.Location = new Point(0, dvHeaderHeight);
            dgvHSBADV.Size = new Size(cardGrid.Width - 2, cardGrid.Height - dvHeaderHeight - 2);
            dgvHSBADV.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvHSBADV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHSBADV.CellClick += (s, e) => FillHSBADVFromGrid();
            cardGrid.Controls.Add(dgvHSBADV);
            page.Controls.Add(cardGrid);

            var cardEdit = UIHelper.CreateCard(10, 320, 1090, 180, "CẬP NHẬT KỸ THUẬT VIÊN");
            page.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "Mã HSBA", 15, 30, 140, out txtDV_MAHSBA);
            UIHelper.CreateLabeledInput(cardEdit, "Loại DV", 170, 30, 260, out txtDV_LoaiDV);
            var lblNgay = new Label { Text = "Ngày DV", Location = new Point(450, 30), AutoSize = true, ForeColor = UIHelper.TextMuted, Font = new Font("Segoe UI", 8.5f) };
            dtpDV_NgayDV = new DateTimePicker { Location = new Point(450, 48), Size = new Size(130, 26), Format = DateTimePickerFormat.Short };
            cardEdit.Controls.Add(lblNgay);
            cardEdit.Controls.Add(dtpDV_NgayDV);
            UIHelper.CreateLabeledInput(cardEdit, "Mã KTV", 595, 30, 120, out txtDV_MAKTV);

            var btnUpdate = UIHelper.CreateButton("Cập nhật KTV", ButtonStyle.Primary);
            btnUpdate.Location = new Point(760, 45);
            btnUpdate.Size = new Size(140, 32);
            btnUpdate.Click += (s, e) => UpdateHSBADV_Assign();
            cardEdit.Controls.Add(btnUpdate);

            lblStatusDV = new Label { Location = new Point(15, 510), Size = new Size(800, 22), Font = new Font("Segoe UI", 8.5f) };
            page.Controls.Add(lblStatusDV);

            LoadHSBADV();
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

        private void FillBenhNhanFromGrid()
        {
            if (dgvBenhNhan.CurrentRow == null) return;
            txtMaBN.Text = dgvBenhNhan.CurrentRow.Cells["MABN"].Value?.ToString();
            txtTenBN.Text = dgvBenhNhan.CurrentRow.Cells["TENBN"].Value?.ToString();
            cmbPhai.Text = dgvBenhNhan.CurrentRow.Cells["PHAI"].Value?.ToString();
            txtCCCD.Text = dgvBenhNhan.CurrentRow.Cells["CCCD"].Value?.ToString();
            txtSoNha.Text = dgvBenhNhan.CurrentRow.Cells["SONHA"].Value?.ToString();
            txtTenDuong.Text = dgvBenhNhan.CurrentRow.Cells["TENDUONG"].Value?.ToString();
            txtQuanHuyen.Text = dgvBenhNhan.CurrentRow.Cells["QUANHUYEN"].Value?.ToString();
            txtTinhTP.Text = dgvBenhNhan.CurrentRow.Cells["TINHTHANH"].Value?.ToString();
            txtTienSu.Text = dgvBenhNhan.CurrentRow.Cells["TIENSUBENH"].Value?.ToString();
            txtTienSuGD.Text = dgvBenhNhan.CurrentRow.Cells["TIENSUBENHGD"].Value?.ToString();
            txtDiUng.Text = dgvBenhNhan.CurrentRow.Cells["DIUNGTHUOC"].Value?.ToString();
            txtOracleUser.Text = dgvBenhNhan.CurrentRow.Cells["ORACLE_USERNAME"].Value?.ToString();

            if (DateTime.TryParse(dgvBenhNhan.CurrentRow.Cells["NGAYSINH"].Value?.ToString(), out var d))
                dtpNgaySinh.Value = d;
        }

        private void FillHSBAFromGrid()
        {
            if (dgvHSBA.CurrentRow == null) return;
            txtMaHSBA.Text = dgvHSBA.CurrentRow.Cells["MAHSBA"].Value?.ToString();
            txtHSBA_MABN.Text = dgvHSBA.CurrentRow.Cells["MABN"].Value?.ToString();
            txtMABS.Text = dgvHSBA.CurrentRow.Cells["MABS"].Value?.ToString();
            txtMaKhoa.Text = dgvHSBA.CurrentRow.Cells["MAKHOA"].Value?.ToString();

            if (DateTime.TryParse(dgvHSBA.CurrentRow.Cells["NGAY"].Value?.ToString(), out var d))
                dtpHSBA_Ngay.Value = d;
        }

        private void FillHSBADVFromGrid()
        {
            if (dgvHSBADV.CurrentRow == null) return;
            txtDV_MAHSBA.Text = dgvHSBADV.CurrentRow.Cells["MAHSBA"].Value?.ToString();
            txtDV_LoaiDV.Text = dgvHSBADV.CurrentRow.Cells["LOAIDV"].Value?.ToString();
            txtDV_MAKTV.Text = dgvHSBADV.CurrentRow.Cells["MAKTV"].Value?.ToString();

            if (DateTime.TryParse(dgvHSBADV.CurrentRow.Cells["NGAYDV"].Value?.ToString(), out var d))
                dtpDV_NgayDV.Value = d;
        }

        private void ClearBenhNhanInputs()
        {
            txtMaBN.Text = "";
            txtTenBN.Text = "";
            cmbPhai.SelectedIndex = 0;
            dtpNgaySinh.Value = DateTime.Today;
            txtCCCD.Text = "";
            txtSoNha.Text = "";
            txtTenDuong.Text = "";
            txtQuanHuyen.Text = "";
            txtTinhTP.Text = "";
            txtTienSu.Text = "";
            txtTienSuGD.Text = "";
            txtDiUng.Text = "";
            txtOracleUser.Text = "";
        }

        private void ClearHSBAInputs()
        {
            txtMaHSBA.Text = "";
            txtHSBA_MABN.Text = "";
            dtpHSBA_Ngay.Value = DateTime.Today;
            txtMABS.Text = "";
            txtMaKhoa.Text = "";
        }

        private void InsertBenhNhan()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"INSERT INTO BVDBA.BENHNHAN
                        (MABN, TENBN, PHAI, NGAYSINH, CCCD, SONHA, TENDUONG, QUANHUYEN, TINHTHANH,
                         TIENSUBENH, TIENSUBENHGD, DIUNGthuoc, ORACLE_USERNAME)
                        VALUES (:mabn, :ten, :phai, :ngay, :cccd, :sn, :td, :qh, :tp, :ts, :tsgd, :du, :ou)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mabn", txtMaBN.Text.Trim());
                        cmd.Parameters.Add("ten", txtTenBN.Text.Trim());
                        cmd.Parameters.Add("phai", cmbPhai.Text);
                        cmd.Parameters.Add("ngay", dtpNgaySinh.Value.Date);
                        cmd.Parameters.Add("cccd", txtCCCD.Text.Trim());
                        cmd.Parameters.Add("sn", txtSoNha.Text.Trim());
                        cmd.Parameters.Add("td", txtTenDuong.Text.Trim());
                        cmd.Parameters.Add("qh", txtQuanHuyen.Text.Trim());
                        cmd.Parameters.Add("tp", txtTinhTP.Text.Trim());
                        cmd.Parameters.Add("ts", txtTienSu.Text.Trim());
                        cmd.Parameters.Add("tsgd", txtTienSuGD.Text.Trim());
                        cmd.Parameters.Add("du", txtDiUng.Text.Trim());
                        cmd.Parameters.Add("ou", string.IsNullOrWhiteSpace(txtOracleUser.Text) ? (object)DBNull.Value : txtOracleUser.Text.Trim().ToUpper());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadBenhNhan();
                UIHelper.SetStatus(lblStatusBN, "Đã thêm bệnh nhân.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusBN, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void UpdateBenhNhan()
        {
            if (string.IsNullOrWhiteSpace(txtMaBN.Text))
            {
                UIHelper.SetStatus(lblStatusBN, "Vui lòng chọn bệnh nhân cần cập nhật.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"UPDATE BVDBA.BENHNHAN SET
                        TENBN = :ten, PHAI = :phai, NGAYSINH = :ngay, CCCD = :cccd,
                        SONHA = :sn, TENDUONG = :td, QUANHUYEN = :qh, TINHTHANH = :tp,
                        TIENSUBENH = :ts, TIENSUBENHGD = :tsgd, DIUNGthuoc = :du,
                        ORACLE_USERNAME = :ou
                        WHERE MABN = :mabn";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("ten", txtTenBN.Text.Trim());
                        cmd.Parameters.Add("phai", cmbPhai.Text);
                        cmd.Parameters.Add("ngay", dtpNgaySinh.Value.Date);
                        cmd.Parameters.Add("cccd", txtCCCD.Text.Trim());
                        cmd.Parameters.Add("sn", txtSoNha.Text.Trim());
                        cmd.Parameters.Add("td", txtTenDuong.Text.Trim());
                        cmd.Parameters.Add("qh", txtQuanHuyen.Text.Trim());
                        cmd.Parameters.Add("tp", txtTinhTP.Text.Trim());
                        cmd.Parameters.Add("ts", txtTienSu.Text.Trim());
                        cmd.Parameters.Add("tsgd", txtTienSuGD.Text.Trim());
                        cmd.Parameters.Add("du", txtDiUng.Text.Trim());
                        cmd.Parameters.Add("ou", string.IsNullOrWhiteSpace(txtOracleUser.Text) ? (object)DBNull.Value : txtOracleUser.Text.Trim().ToUpper());
                        cmd.Parameters.Add("mabn", txtMaBN.Text.Trim());
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

        private void InsertHSBA()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = @"INSERT INTO BVDBA.HSBA
                        (MAHSBA, MABN, NGAY, MABS, MAKHOA)
                        VALUES (:mahsba, :mabn, :ngay, :mabs, :makhoa)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mahsba", txtMaHSBA.Text.Trim());
                        cmd.Parameters.Add("mabn", txtHSBA_MABN.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpHSBA_Ngay.Value.Date);
                        cmd.Parameters.Add("mabs", txtMABS.Text.Trim());
                        cmd.Parameters.Add("makhoa", txtMaKhoa.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBA();
                UIHelper.SetStatus(lblStatusHSBA, "Đã thêm HSBA.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusHSBA, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void UpdateHSBA_Assign()
        {
            if (string.IsNullOrWhiteSpace(txtMaHSBA.Text))
            {
                UIHelper.SetStatus(lblStatusHSBA, "Vui lòng chọn HSBA cần cập nhật.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = "UPDATE BVDBA.HSBA SET MABS = :mabs, MAKHOA = :makhoa WHERE MAHSBA = :mahsba";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("mabs", txtMABS.Text.Trim());
                        cmd.Parameters.Add("makhoa", txtMaKhoa.Text.Trim());
                        cmd.Parameters.Add("mahsba", txtMaHSBA.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBA();
                UIHelper.SetStatus(lblStatusHSBA, "Đã cập nhật bác sĩ/khoa.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusHSBA, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }

        private void UpdateHSBADV_Assign()
        {
            if (string.IsNullOrWhiteSpace(txtDV_MAHSBA.Text) || string.IsNullOrWhiteSpace(txtDV_LoaiDV.Text))
            {
                UIHelper.SetStatus(lblStatusDV, "Vui lòng chọn HSBA_DV cần cập nhật.", StatusType.Warning);
                return;
            }

            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    const string sql = "UPDATE BVDBA.HSBA_DV SET MAKTV = :maktv WHERE MAHSBA = :mahsba AND LOAIDV = :ldv AND NGAYDV = :ngay";
                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("maktv", txtDV_MAKTV.Text.Trim());
                        cmd.Parameters.Add("mahsba", txtDV_MAHSBA.Text.Trim());
                        cmd.Parameters.Add("ldv", txtDV_LoaiDV.Text.Trim());
                        cmd.Parameters.Add("ngay", dtpDV_NgayDV.Value.Date);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHSBADV();
                UIHelper.SetStatus(lblStatusDV, "Đã cập nhật KTV.", StatusType.Success);
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatusDV, "Lỗi: " + ex.Message, StatusType.Error);
            }
        }
    }
}
