using Microsoft.Office.Interop.Excel;
using QLBanBanhvaDoUong.Classes;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;

// Nếu namespace trong file của bạn khác, GIỮ dòng namespace cũ của bạn
// và chỉ chép phần bên trong lớp frmThongKe cùng các dòng using ở trên.
//
// Control Chart nằm trong Toolbox, nhóm Data. Kéo vào form là Visual Studio tự thêm tham chiếu
// System.Windows.Forms.DataVisualization.
namespace QLBanBanhvaDoUong
{
    public partial class frmThongKe : Form
    {
        private readonly ThongKeData tk = new ThongKeData();
        private readonly Function fn = new Function();

        private bool dangNap;
        // Kết quả của lần xem gần nhất, dùng để xuất Excel
        private DataTable dtDoanhThu, dtBanChay, dtPhuongThuc;

        public frmThongKe()
        {
            InitializeComponent();
        }

        // =====================================================================
        // LOAD FORM (chỉ quản lý được vào)
        // =====================================================================
        private void frmThongKe_Load(object sender, EventArgs e)
        {
            if (!string.Equals((Session.ChucVu ?? "").Trim(), "Quản lý",
                               StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Chỉ quản lý mới được vào chức năng này.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            dangNap = true;

            // Mặc định xem từ đầu tháng đến hôm nay
            DateTime homNay = DateTime.Today;
            dtpTuNgay.Format = DateTimePickerFormat.Short;
            dtpDenNgay.Format = DateTimePickerFormat.Short;
            dtpTuNgay.Value = new DateTime(homNay.Year, homNay.Month, 1);
            dtpDenNgay.Value = homNay;

            rdoNgay.Checked = true;

            fn.FillComBox(cboLoai, tk.LayLoai(), "TenLoai", "MaLoai");
            cboLoai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoai.SelectedIndex = 0;

            nudTop.Minimum = 1;
            nudTop.Maximum = 50;
            nudTop.Value = 10;

            CauHinhBieuDo();

            dangNap = false;

            XemBaoCao();
        }

        // Dựng sẵn các biểu đồ: loại biểu đồ, tiêu đề, định dạng trục
        private void CauHinhBieuDo()
        {
            // Doanh thu: cột
            chartDoanhThu.Series.Clear();
            chartDoanhThu.Legends.Clear();
            chartDoanhThu.Titles.Clear();
            chartDoanhThu.Titles.Add("Doanh thu");
            Series s1 = chartDoanhThu.Series.Add("Doanh thu");
            s1.ChartType = SeriesChartType.Column;
            s1.LabelFormat = "N0";
            chartDoanhThu.ChartAreas[0].AxisY.LabelStyle.Format = "N0";
            chartDoanhThu.ChartAreas[0].AxisX.MajorGrid.Enabled = false;

            // Bán chạy: cột ngang, hạng 1 ở trên cùng
            chartBanChay.Series.Clear();
            chartBanChay.Legends.Clear();
            chartBanChay.Titles.Clear();
            chartBanChay.Titles.Add("Sản phẩm bán chạy (theo số lượng)");
            Series s2 = chartBanChay.Series.Add("Số lượng");
            s2.ChartType = SeriesChartType.Bar;
            s2.IsValueShownAsLabel = true;
            chartBanChay.ChartAreas[0].AxisX.IsReversed = true;
            chartBanChay.ChartAreas[0].AxisX.Interval = 1;
            chartBanChay.ChartAreas[0].AxisX.MajorGrid.Enabled = false;

            // Phương thức thanh toán: tròn, có chú giải bên phải
            chartPhuongThuc.Series.Clear();
            chartPhuongThuc.Legends.Clear();
            chartPhuongThuc.Titles.Clear();
            chartPhuongThuc.Titles.Add("Tiền thu theo phương thức");
            Legend lg = chartPhuongThuc.Legends.Add("lgPhuongThuc");
            lg.Docking = Docking.Right;
            Series s3 = chartPhuongThuc.Series.Add("Tiền thu");
            s3.ChartType = SeriesChartType.Pie;
            s3.Legend = "lgPhuongThuc";
            s3.IsValueShownAsLabel = true;
            s3.Label = "#PERCENT{P0}";
            s3.LegendText = "#VALX";
        }

        // =====================================================================
        // XEM BÁO CÁO: nạp cả ba tab theo khoảng ngày đang chọn
        // =====================================================================
        private void btnXem_Click(object sender, EventArgs e)
        {
            XemBaoCao();
        }

        private void XemBaoCao()
        {
            if (dtpTuNgay.Value.Date > dtpDenNgay.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
                dtpTuNgay.Focus();
                return;
            }

            try
            {
                TaiDoanhThu();
                TaiBanChay();
                TaiPhuongThuc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lấy được số liệu: " + ex.Message);
            }
        }

        // Đổi cách xem hoặc điều kiện lọc thì chỉ nạp lại tab liên quan
        private void rdoNgay_CheckedChanged(object sender, EventArgs e)
        {
            if (dangNap || !((RadioButton)sender).Checked) return;
            TaiDoanhThu();
        }

        private void cboLoai_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dangNap) return;
            TaiBanChay();
        }

        private void nudTop_ValueChanged(object sender, EventArgs e)
        {
            if (dangNap) return;
            TaiBanChay();
        }

        // ---------- Tab 1: doanh thu ----------
        private void TaiDoanhThu()
        {
            bool theoThang = rdoThang.Checked;
            DataTable dt = tk.DoanhThu(dtpTuNgay.Value, dtpDenNgay.Value, theoThang);
            dtDoanhThu = dt;

            dgvDoanhThu.DataSource = dt;
            dgvDoanhThu.Columns["Ky"].HeaderText = theoThang ? "Tháng" : "Ngày";
            dgvDoanhThu.Columns["SoHoaDon"].HeaderText = "Số hóa đơn";
            dgvDoanhThu.Columns["DoanhThu"].HeaderText = "Doanh thu";
            dgvDoanhThu.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
            dgvDoanhThu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            decimal tong = 0;
            int soHD = 0;
            Series s = chartDoanhThu.Series[0];
            s.Points.Clear();
            foreach (DataRow r in dt.Rows)
            {
                decimal doanhThu = Convert.ToDecimal(r["DoanhThu"]);
                tong += doanhThu;
                soHD += Convert.ToInt32(r["SoHoaDon"]);
                s.Points.AddXY(r["Ky"].ToString(), doanhThu);
            }

            // Ít cột thì ghi số trên đầu cột, nhiều cột thì để trục tự giãn nhãn cho khỏi chồng chữ
            s.IsValueShownAsLabel = dt.Rows.Count <= 12;
            chartDoanhThu.ChartAreas[0].AxisX.Interval = dt.Rows.Count > 15 ? 0 : 1;
            chartDoanhThu.Titles[0].Text = theoThang ? "Doanh thu theo tháng" : "Doanh thu theo ngày";

            lblTongDoanhThu.Text = "Tổng doanh thu: " + tong.ToString("N0") + " đ";
            lblSoHoaDon.Text = "Số hóa đơn: " + soHD;
            lblTrungBinh.Text = "Trung bình mỗi hóa đơn: " + (soHD > 0 ? tong / soHD : 0).ToString("N0") + " đ";
        }

        // ---------- Tab 2: bán chạy ----------
        private void TaiBanChay()
        {
            string maLoai = cboLoai.SelectedValue == null ? "" : cboLoai.SelectedValue.ToString();
            DataTable dt = tk.BanChay(dtpTuNgay.Value, dtpDenNgay.Value, maLoai, (int)nudTop.Value);
            dtBanChay = dt;

            dgvBanChay.DataSource = dt;
            dgvBanChay.Columns["Hang"].HeaderText = "Hạng";
            dgvBanChay.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgvBanChay.Columns["SoLuong"].HeaderText = "Số lượng";
            dgvBanChay.Columns["DoanhThu"].HeaderText = "Doanh thu";
            dgvBanChay.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
            dgvBanChay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Series s = chartBanChay.Series[0];
            s.Points.Clear();
            foreach (DataRow r in dt.Rows)
                s.Points.AddXY(r["TenSP"].ToString(), Convert.ToInt32(r["SoLuong"]));
        }

        // ---------- Tab 3: phương thức thanh toán ----------
        private void TaiPhuongThuc()
        {
            DataTable dt = tk.PhuongThuc(dtpTuNgay.Value, dtpDenNgay.Value);
            dtPhuongThuc = dt;

            dgvPhuongThuc.DataSource = dt;
            dgvPhuongThuc.Columns["TenPT"].HeaderText = "Phương thức";
            dgvPhuongThuc.Columns["SoLan"].HeaderText = "Số lần";
            dgvPhuongThuc.Columns["TongThu"].HeaderText = "Tổng thu";
            dgvPhuongThuc.Columns["TongThu"].DefaultCellStyle.Format = "N0";
            dgvPhuongThuc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            decimal tong = 0;
            Series s = chartPhuongThuc.Series[0];
            s.Points.Clear();
            foreach (DataRow r in dt.Rows)
            {
                decimal thu = Convert.ToDecimal(r["TongThu"]);
                tong += thu;
                s.Points.AddXY(r["TenPT"].ToString(), thu);
            }

            lblTongThu.Text = "Tổng đã thu: " + tong.ToString("N0") + " đ";
        }

        // =====================================================================
        // XUẤT EXCEL: một file có ba trang (cần Excel cài trên máy và đã Add Reference COM Excel)
        // =====================================================================
        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dtDoanhThu == null)
            {
                MessageBox.Show("Hãy bấm Xem báo cáo trước.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                sfd.FileName = "ThongKe_" + dtpTuNgay.Value.ToString("ddMMyyyy") + "_" +
                               dtpDenNgay.Value.ToString("ddMMyyyy") + ".xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                string khoang = "Từ " + dtpTuNgay.Value.ToString("dd/MM/yyyy") +
                                " đến " + dtpDenNgay.Value.ToString("dd/MM/yyyy");

                Excel.Application exApp = null;
                Excel.Workbook exBook = null;
                try
                {
                    exApp = new Excel.Application();
                    exBook = exApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);

                    Excel.Worksheet sh1 = (Excel.Worksheet)exBook.Worksheets[1];
                    sh1.Name = "Doanh thu";
                    GhiBang(sh1, rdoThang.Checked ? "DOANH THU THEO THÁNG" : "DOANH THU THEO NGÀY", khoang,
                        dtDoanhThu,
                        new[] { rdoThang.Checked ? "Tháng" : "Ngày", "Số hóa đơn", "Doanh thu" },
                        new string[] { null, "#,##0", "#,##0" });

                    Excel.Worksheet sh2 = (Excel.Worksheet)exBook.Worksheets.Add(Type.Missing, sh1);
                    sh2.Name = "Bán chạy";
                    GhiBang(sh2, "SẢN PHẨM BÁN CHẠY", khoang,
                        dtBanChay,
                        new[] { "Hạng", "Tên sản phẩm", "Số lượng", "Doanh thu" },
                        new string[] { null, null, "#,##0", "#,##0" });

                    Excel.Worksheet sh3 = (Excel.Worksheet)exBook.Worksheets.Add(Type.Missing, sh2);
                    sh3.Name = "Phương thức TT";
                    GhiBang(sh3, "TIỀN THU THEO PHƯƠNG THỨC THANH TOÁN", khoang,
                        dtPhuongThuc,
                        new[] { "Phương thức", "Số lần", "Tổng thu" },
                        new string[] { null, "#,##0", "#,##0" });

                    sh1.Activate();
                    exBook.SaveAs(sfd.FileName);
                    MessageBox.Show("Đã xuất báo cáo ra file Excel.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không xuất được Excel: " + ex.Message);
                }
                finally
                {
                    if (exBook != null) exBook.Close(false);
                    if (exApp != null) exApp.Quit();
                }
            }
        }

        // Ghi một DataTable vào một trang tính: tiêu đề ở A1, khoảng ngày ở A2, bảng bắt đầu từ hàng 4
        private static void GhiBang(Excel.Worksheet sh, string tieuDe, string khoang,
                                    DataTable dt, string[] tenCot, string[] dinhDang)
        {
            sh.Range["A1"].Value = tieuDe;
            sh.Range["A1"].Font.Size = 16;
            sh.Range["A1"].Font.Bold = true;
            sh.Range["A2"].Value = khoang;

            for (int c = 0; c < tenCot.Length; c++)
                sh.Cells[4, c + 1] = tenCot[c];
            sh.Range[sh.Cells[4, 1], sh.Cells[4, tenCot.Length]].Font.Bold = true;

            int soHang = dt.Rows.Count;
            int soCot = tenCot.Length;
            if (soHang > 0)
            {
                // Ghi cả khối một lần cho nhanh
                object[,] mang = new object[soHang, soCot];
                for (int r = 0; r < soHang; r++)
                    for (int c = 0; c < soCot; c++)
                        mang[r, c] = dt.Rows[r][c] == DBNull.Value ? "" : dt.Rows[r][c];

                Excel.Range vung = sh.Range[sh.Cells[5, 1], sh.Cells[4 + soHang, soCot]];
                vung.Value2 = mang;

                for (int c = 0; c < soCot; c++)
                    if (dinhDang[c] != null)
                        sh.Range[sh.Cells[5, c + 1], sh.Cells[4 + soHang, c + 1]].NumberFormat = dinhDang[c];
            }

            sh.Columns.AutoFit();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}