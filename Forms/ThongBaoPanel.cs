using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleAdminApp.Helpers;

namespace OracleAdminApp.Forms
{
    // ================================================================
    // ThongBaoPanel - Tab hien thi thong bao OLS
    // Dung chung cho: CoordinatorForm, DoctorForm,
    //                 TechnicianForm, PatientForm
    //
    // OLS tu dong loc du lieu theo nhan cua user dang dang nhap.
    // Chi can SELECT * FROM BVDBA.THONGBAO la Oracle tra ve
    // dung cac dong ma user do co quyen doc.
    // ================================================================
    public class ThongBaoPanel : UserControl
    {
        private readonly string _connStr;

        private DataGridView dgvThongBao;
        private Label lblStatus;
        private Button btnRefresh;
        private Label lblInfo;

        public ThongBaoPanel(string connStr)
        {
            _connStr = connStr;
            InitializeLayout();
            LoadThongBao();
        }

        private void InitializeLayout()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = UIHelper.LightBg;
            this.Padding   = new Padding(10);

            // Header
            var header = UIHelper.CreateSectionHeader(
                "Thong bao",
                "Cac thong bao duoc gui den ban (Oracle Label Security tu dong loc theo quyen)");
            this.Controls.Add(header);

            // Panel toolbar (Dock Top)
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 45,
                BackColor = Color.Transparent
            };

            btnRefresh           = UIHelper.CreateButton("Lam moi", ButtonStyle.Secondary);
            btnRefresh.Size      = new Size(100, 34);
            btnRefresh.Location  = new Point(0, 5);
            btnRefresh.ForeColor = UIHelper.TextDark;
            btnRefresh.Click    += (s, e) => LoadThongBao();
            pnlTop.Controls.Add(btnRefresh);

            lblInfo = new Label
            {
                Location  = new Point(115, 12),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = UIHelper.TextMuted
            };
            pnlTop.Controls.Add(lblInfo);

            this.Controls.Add(pnlTop);

            // Grid (Dock Fill)
            var pnlGrid = new Panel
            {
                Dock        = DockStyle.Fill,
                BackColor   = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvThongBao      = UIHelper.CreateGrid();
            dgvThongBao.Dock = DockStyle.Fill;
            dgvThongBao.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name       = "MATHONGBAO",
                HeaderText = "Ma TB",
                FillWeight = 8
            });
            dgvThongBao.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name       = "NOIDUNG",
                HeaderText = "Noi dung thong bao",
                FillWeight = 55
            });
            dgvThongBao.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name       = "NGAYGIOGIO",
                HeaderText = "Ngay gio",
                FillWeight = 17
            });
            dgvThongBao.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name       = "DIADIEM",
                HeaderText = "Dia diem",
                FillWeight = 20
            });

            // Wrap text cho cot Noi dung
            dgvThongBao.Columns["NOIDUNG"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvThongBao.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // To mau xen ke ro rang hon
            dgvThongBao.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 255);
            dgvThongBao.RowTemplate.Height = 50;

            pnlGrid.Controls.Add(dgvThongBao);
            lblStatus = UIHelper.CreateStatusLabel(pnlGrid);

            // Thứ tự add: Fill truoc, Top sau
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlTop);
            this.Controls.Add(header);
        }

        // ================================================================
        // LoadThongBao: SELECT tu BVDBA.THONGBAO
        // Oracle OLS tu loc theo nhan cua session hien tai
        // Khong can WHERE gi them - he thong tu bao mat row-level
        // ================================================================
        public void LoadThongBao()
        {
            UIHelper.SetStatus(lblStatus, "Dang tai thong bao...", StatusType.Info);
            dgvThongBao.Rows.Clear();

            try
            {
                const string sql = @"
                    SELECT MATHONGBAO,
                           NOIDUNG,
                           TO_CHAR(NGAYGIO, 'DD/MM/YYYY HH24:MI') AS NGAYGIOGIO,
                           DIADIEM
                    FROM   BVDBA.THONGBAO
                    ORDER  BY NGAYGIO DESC";

                using (var conn = new OracleConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd    = new OracleCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvThongBao.Rows.Add(
                                reader["MATHONGBAO"].ToString(),
                                reader["NOIDUNG"].ToString(),
                                reader["NGAYGIOGIO"].ToString(),
                                reader["DIADIEM"].ToString()
                            );
                        }
                    }
                }

                int count = dgvThongBao.Rows.Count;
                UIHelper.SetStatus(lblStatus,
                    count + " thong bao duoc gui den ban.",
                    StatusType.Success);

                lblInfo.Text = count == 0
                    ? "Ban khong co thong bao nao."
                    : "Hien thi " + count + " thong bao phu hop voi quyen cua ban.";
            }
            catch (Exception ex)
            {
                UIHelper.SetStatus(lblStatus, "Loi: " + ex.Message, StatusType.Error);
            }
        }
    }
}
