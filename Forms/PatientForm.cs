using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    public class PatientForm : Form
    {
        private readonly string _connStr;
        private readonly string _username;

        // Controls cho thong tin ca nhan
        private TextBox txtMaBN, txtTenBN, txtPhai, txtNgaySinh, txtCCCD;
        private TextBox txtSoNha, txtTenDuong, txtQuanHuyen, txtTinhTP;
        private TextBox txtTienSu, txtTienSuGD, txtDiUng;
        private Button btnUpdate, btnLogout, btnThongBao;
        private Label lblStatus;

        public PatientForm(string connStr, string username)
        {
            _connStr = connStr;
            _username = username;
            InitializeLayout();
            LoadPatientData();
        }

        private void InitializeLayout()
        {
            this.Text = "Thong tin Benh nhan - " + _username;
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIHelper.LightBg;

            var header = UIHelper.CreateSectionHeader("HO SO CA NHAN BENH NHAN", "Xem va cap nhat thong tin lien lac, tien su benh");
            this.Controls.Add(header);

            // Card 1: Thong tin co ban (Read-only)
            var cardBasic = UIHelper.CreateCard(20, 70, 740, 150, "THONG TIN DINH DANH (KHONG THE SUA)");
            this.Controls.Add(cardBasic);

            UIHelper.CreateLabeledInput(cardBasic, "Ma Benh Nhan", 15, 35, 150, out txtMaBN);
            UIHelper.CreateLabeledInput(cardBasic, "Ho va Ten", 180, 35, 300, out txtTenBN);
            UIHelper.CreateLabeledInput(cardBasic, "Phai", 500, 35, 100, out txtPhai);
            UIHelper.CreateLabeledInput(cardBasic, "Ngay Sinh", 15, 90, 200, out txtNgaySinh);
            UIHelper.CreateLabeledInput(cardBasic, "So CCCD", 230, 90, 250, out txtCCCD);

            txtMaBN.ReadOnly = txtTenBN.ReadOnly = txtPhai.ReadOnly = txtNgaySinh.ReadOnly = txtCCCD.ReadOnly = true;

            // Card 2: Thong tin lien lac & Tien su (Editable)
            var cardEdit = UIHelper.CreateCard(20, 230, 740, 300, "THONG TIN CHI TIET (CO THE CAP NHAT)");
            this.Controls.Add(cardEdit);

            UIHelper.CreateLabeledInput(cardEdit, "So nha", 15, 35, 100, out txtSoNha);
            UIHelper.CreateLabeledInput(cardEdit, "Ten duong", 130, 35, 200, out txtTenDuong);
            UIHelper.CreateLabeledInput(cardEdit, "Quan/Huyen", 350, 35, 170, out txtQuanHuyen);
            UIHelper.CreateLabeledInput(cardEdit, "Tinh/TP", 540, 35, 170, out txtTinhTP);

            UIHelper.CreateLabeledInput(cardEdit, "Tien su benh", 15, 95, 700, out txtTienSu);
            UIHelper.CreateLabeledInput(cardEdit, "Tien su benh gia dinh", 15, 155, 700, out txtTienSuGD);
            UIHelper.CreateLabeledInput(cardEdit, "Di ung thuoc", 15, 215, 700, out txtDiUng);

            // Nút bấm & Trạng thái
            btnUpdate = UIHelper.CreateButton("CAP NHAT THONG TIN", ButtonStyle.Primary);
            btnUpdate.Location = new Point(560, 550);
            btnUpdate.Size = new Size(200, 40);
            btnUpdate.Click += BtnUpdate_Click;
            this.Controls.Add(btnUpdate);

            btnLogout = UIHelper.CreateButton("Dang xuat", ButtonStyle.Secondary);
            btnLogout.Location = new Point(20, 550);
            btnLogout.Click += (s, e) => { this.Close(); new LoginForm().Show(); };
            this.Controls.Add(btnLogout);

            btnThongBao = UIHelper.CreateButton("Xem thong bao", ButtonStyle.Primary);
            btnThongBao.Location = new Point(230, 550);
            btnThongBao.Size = new Size(180, 40);
            btnThongBao.Click += (s, e) =>
            {
                var f = new Form
                {
                    Text = "Thong bao - " + _username,
                    Size = new Size(950, 550),
                    StartPosition = FormStartPosition.CenterParent
                };
                f.Controls.Add(new ThongBaoPanel(_connStr));
                f.ShowDialog();
            };
            this.Controls.Add(btnThongBao);

            lblStatus = new Label { Location = new Point(230, 560), Size = new Size(320, 25), Font = new Font("Segoe UI", 9f) };
            this.Controls.Add(lblStatus);
        }

        private void LoadPatientData()
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    string sql = "SELECT * FROM BVDBA.VW_BN_THONGTIN_CANHAN";
                    using (var cmd = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtMaBN.Text = reader["MABN"].ToString();
                            txtTenBN.Text = reader["TENBN"].ToString();
                            txtPhai.Text = reader["PHAI"].ToString();
                            txtNgaySinh.Text = Convert.ToDateTime(reader["NGAYSINH"]).ToString("dd/MM/yyyy");
                            txtCCCD.Text = reader["CCCD"].ToString();
                            txtSoNha.Text = reader["SONHA"].ToString();
                            txtTenDuong.Text = reader["TENDUONG"].ToString();
                            txtQuanHuyen.Text = reader["QUANHUYEN"].ToString();
                            txtTinhTP.Text = reader["TINHTHANH"].ToString();
                            txtTienSu.Text = reader["TIENSUBENH"].ToString();
                            txtTienSuGD.Text = reader["TIENSUBENHGD"].ToString();
                            txtDiUng.Text = reader["DIUNGthuoc"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { UIHelper.SetStatus(lblStatus, "Loi tai du lieu: " + ex.Message, StatusType.Error); }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    string sql = @"UPDATE BVDBA.VW_BN_THONGTIN_CANHAN SET 
                                   SONHA = :sn, TENDUONG = :td, QUANHUYEN = :qh, TINHTHANH = :tp,
                                   TIENSUBENH = :ts, TIENSUBENHGD = :tsgd, DIUNGthuoc = :du";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("sn", txtSoNha.Text);
                        cmd.Parameters.Add("td", txtTenDuong.Text);
                        cmd.Parameters.Add("qh", txtQuanHuyen.Text);
                        cmd.Parameters.Add("tp", txtTinhTP.Text);
                        cmd.Parameters.Add("ts", txtTienSu.Text);
                        cmd.Parameters.Add("tsgd", txtTienSuGD.Text);
                        cmd.Parameters.Add("du", txtDiUng.Text);

                        cmd.ExecuteNonQuery();
                        UIHelper.SetStatus(lblStatus, "Cap nhat thanh cong!", StatusType.Success);
                    }
                }
            }
            catch (Exception ex) { UIHelper.SetStatus(lblStatus, "Loi cap nhat: " + ex.Message, StatusType.Error); }
        }
    }
}