using QLBanBanhvaDoUong.Classes;
using QLBanBanhvaDoUong.Forms.NghiepVu;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

// Nếu namespace trong file của bạn khác, GIỮ dòng namespace cũ của bạn
// và chỉ chép phần bên trong lớp frmHoaDon cùng các dòng using ở trên.
namespace QLBanBanhvaDoUong
{
    public partial class frmHoaDon : Form
    {
        private readonly HoaDonData hd = new HoaDonData();
        private readonly ThanhToanData tt = new ThanhToanData();

        private readonly string maHDMoSan;      // mã hóa đơn được truyền vào khi mở form (có thể rỗng)
        private string maHDHienTai = "";        // hóa đơn đang hiển thị
        private decimal tongTien, daTra;

        public frmHoaDon() : this("") { }

        // Các form khác gọi new frmHoaDon("HD004") để mở thẳng một hóa đơn
        public frmHoaDon(string maHD)
        {
            InitializeComponent();
            maHDMoSan = maHD ?? "";
        }

        // =====================================================================
        // LOAD FORM
        // =====================================================================
        private void frmHoaDon_Load(object sender, EventArgs e)
        {
            // Form chỉ để xem: khóa các ô thông tin chung
            txtMaHoaDon.ReadOnly = true;
            txtTenNhanVien.ReadOnly = true;
            txtTenKhachHang.ReadOnly = true;
            txtDiaChi.ReadOnly = true;
            txtDienThoai.ReadOnly = true;
            txtTongTien.ReadOnly = true;
            cboMaNhanVien.DropDownStyle = ComboBoxStyle.DropDown;
            cboMaKhachHang.DropDownStyle = ComboBoxStyle.DropDown;
            cboMaNhanVien.Enabled = false;
            cboMaKhachHang.Enabled = false;

            dtpNgayBan.Format = DateTimePickerFormat.Custom;
            dtpNgayBan.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpNgayBan.Enabled = false;

            // Ô tìm kiếm: gõ để gợi ý mã hóa đơn
            cboTimKiem.DropDownStyle = ComboBoxStyle.DropDown;
            cboTimKiem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboTimKiem.AutoCompleteSource = AutoCompleteSource.ListItems;

            NapDanhSach();
            ResetForm();

            if (maHDMoSan != "")
            {
                cboTimKiem.Text = maHDMoSan;
                TimHoaDon(maHDMoSan);
            }
        }

        private void NapDanhSach()
        {
            cboTimKiem.DataSource = hd.LayDanhSachMaHD();
            cboTimKiem.DisplayMember = "MaHD";
            cboTimKiem.ValueMember = "MaHD";
            cboTimKiem.SelectedIndex = -1;
        }

        // Trạng thái trống: chưa chọn hóa đơn nào
        private void ResetForm()
        {
            maHDHienTai = "";
            tongTien = daTra = 0;

            txtMaHoaDon.Clear();
            cboMaNhanVien.Text = "";
            txtTenNhanVien.Clear();
            cboMaKhachHang.Text = "";
            txtTenKhachHang.Clear();
            txtDiaChi.Clear();
            txtDienThoai.Clear();
            txtTongTien.Text = "0 đ";
            lblBangChu.Text = "Bằng chữ:";
            lblTrangThai.Text = "Trạng thái:";
            lblTrangThai.ForeColor = Color.Black;
            lblDaTra.Text = "Đã trả: 0 đ";
            lblConThieu.Text = "Còn thiếu: 0 đ";
            dgvChiTietHDBan.DataSource = null;
            dgvThanhToan.DataSource = null;

            btnThanhToan.Enabled = false;
            btnHuyHoaDon.Enabled = false;
            btnInHoaDon.Enabled = false;
        }

        // =====================================================================
        // TÌM VÀ HIỂN THỊ HÓA ĐƠN
        // =====================================================================
        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string maHD = cboTimKiem.Text.Trim();
            if (maHD == "")
            {
                MessageBox.Show("Vui lòng nhập hoặc chọn Mã hóa đơn cần tìm!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TimHoaDon(maHD);
        }

        // Chọn một mã trong danh sách là hiện luôn hóa đơn đó
        private void cboTimKiem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            object v = cboTimKiem.SelectedValue;
            if (v != null) TimHoaDon(v.ToString());
        }

        private void TimHoaDon(string maHD)
        {
            DataRow h = hd.LayHoaDon(maHD);
            if (h == null)
            {
                MessageBox.Show("Không tìm thấy mã hóa đơn này!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            maHDHienTai = maHD;

            // Thông tin chung
            txtMaHoaDon.Text = maHD;
            dtpNgayBan.Value = Convert.ToDateTime(h["NgayLap"]);
            cboMaNhanVien.Text = HoaDonData.Chuoi(h["MaNV"]);
            txtTenNhanVien.Text = HoaDonData.Chuoi(h["TenNV"]);
            cboMaKhachHang.Text = HoaDonData.Chuoi(h["MaKH"]);
            txtTenKhachHang.Text = HoaDonData.Chuoi(h["TenKH"]);
            txtDiaChi.Text = HoaDonData.Chuoi(h["DiaChi"]);
            txtDienThoai.Text = HoaDonData.Chuoi(h["SDT"]);

            // Các món
            dgvChiTietHDBan.DataSource = hd.LayChiTiet(maHD);
            DinhDangLuoiChiTiet();

            // Tiền và thanh toán
            tongTien = Convert.ToDecimal(h["TongTien"]);
            daTra = tt.TongDaTra(maHD);
            decimal conThieu = tongTien - daTra;
            string trangThai = HoaDonData.Chuoi(h["TrangThai"]);

            txtTongTien.Text = tongTien.ToString("N0") + " đ";
            lblBangChu.Text = "Bằng chữ: " + HoaDonData.DocSoThanhChu(tongTien);
            lblDaTra.Text = "Đã trả: " + daTra.ToString("N0") + " đ";
            lblConThieu.Text = "Còn thiếu: " + (conThieu > 0 ? conThieu : 0).ToString("N0") + " đ";
            lblTrangThai.Text = "Trạng thái: " + trangThai;
            lblTrangThai.ForeColor =
                trangThai == ThanhToanData.DaThanhToan ? Color.FromArgb(15, 110, 86) :
                trangThai == ThanhToanData.DaHuy ? Color.FromArgb(153, 60, 29) :
                Color.FromArgb(122, 82, 0);

            dgvThanhToan.DataSource = tt.LayThanhToan(maHD);
            DinhDangLuoiThanhToan();

            // Bật tắt nút theo trạng thái
            bool chuaTra = trangThai == ThanhToanData.ChuaThanhToan;
            btnThanhToan.Enabled = chuaTra && conThieu > 0;
            btnHuyHoaDon.Enabled = chuaTra && daTra == 0;
            btnInHoaDon.Enabled = true;
        }

        private void DinhDangLuoiChiTiet()
        {
            dgvChiTietHDBan.Columns["MaSP"].HeaderText = "Mã SP";
            dgvChiTietHDBan.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgvChiTietHDBan.Columns["SoLuong"].HeaderText = "SL";
            dgvChiTietHDBan.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvChiTietHDBan.Columns["MucDuong"].HeaderText = "Đường";
            dgvChiTietHDBan.Columns["MucDa"].HeaderText = "Đá";
            dgvChiTietHDBan.Columns["ThanhTien"].HeaderText = "Thành tiền";
            dgvChiTietHDBan.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvChiTietHDBan.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            dgvChiTietHDBan.Columns["MucDuong"].DefaultCellStyle.Format = "0'%'";
            dgvChiTietHDBan.Columns["MucDa"].DefaultCellStyle.Format = "0'%'";
            dgvChiTietHDBan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void DinhDangLuoiThanhToan()
        {
            dgvThanhToan.Columns["MaPT"].Visible = false;
            dgvThanhToan.Columns["TenPT"].HeaderText = "Phương thức";
            dgvThanhToan.Columns["SoTien"].HeaderText = "Số tiền";
            dgvThanhToan.Columns["SoTien"].DefaultCellStyle.Format = "N0";
            dgvThanhToan.Columns["NgayTT"].HeaderText = "Thời gian";
            dgvThanhToan.Columns["NgayTT"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvThanhToan.Columns["MaGiaoDich"].HeaderText = "Mã giao dịch";
            dgvThanhToan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // =====================================================================
        // NỐI SANG CÁC FORM KHÁC
        // =====================================================================

        // Thêm hóa đơn mới: mở form bán hàng, đóng xong thì nạp lại danh sách
        // và hiện hóa đơn mới nhất nếu có hóa đơn vừa được tạo
        private void btnThemHoaDon_Click(object sender, EventArgs e)
        {
            string truoc = hd.MaHDMoiNhat();

            using (var f = new frmBanHang())
            {
                f.ShowDialog(this);
            }

            NapDanhSach();
            string sau = hd.MaHDMoiNhat();
            if (sau != "" && sau != truoc)
            {
                cboTimKiem.Text = sau;
                TimHoaDon(sau);
            }
            else if (maHDHienTai != "")
            {
                TimHoaDon(maHDHienTai);
            }
        }

        // Thanh toán hóa đơn đang xem
        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (maHDHienTai == "") return;

            using (var f = new frmThanhToan(maHDHienTai))
            {
                f.ShowDialog(this);
            }

            TimHoaDon(maHDHienTai);    // nạp lại để thấy số đã trả và trạng thái mới
        }

        // =====================================================================
        // HỦY HÓA ĐƠN (chỉ hóa đơn chưa trả đồng nào; vẫn giữ bản ghi với trạng thái "Đã hủy")
        // =====================================================================
        private void btnHuyHoaDon_Click(object sender, EventArgs e)
        {
            if (maHDHienTai == "") return;

            if (MessageBox.Show("Bạn có muốn HỦY hóa đơn " + maHDHienTai + " không?", "Xác nhận hủy",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                if (!tt.HuyHoaDon(maHDHienTai))
                {
                    MessageBox.Show("Hóa đơn đã có thanh toán nên không thể hủy.");
                    return;
                }
                MessageBox.Show("Đã hủy hóa đơn.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                TimHoaDon(maHDHienTai);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // =====================================================================
        // IN HÓA ĐƠN (xuất ra file Excel; cần Excel cài trên máy và đã Add Reference COM Excel)
        // =====================================================================
        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            if (maHDHienTai == "")
            {
                MessageBox.Show("Hãy chọn một hóa đơn để in.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                sfd.FileName = "HoaDon_" + maHDHienTai + ".xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                Excel.Application exApp = null;
                Excel.Workbook exBook = null;
                try
                {
                    exApp = new Excel.Application();
                    exBook = exApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                    Excel.Worksheet sh = (Excel.Worksheet)exBook.Worksheets[1];
                    sh.Name = "HoaDon";

                    // Tiêu đề
                    sh.Range["A1:G1"].Merge();
                    sh.Range["A1"].Value = "HÓA ĐƠN BÁN HÀNG";
                    sh.Range["A1"].Font.Size = 18;
                    sh.Range["A1"].Font.Bold = true;
                    sh.Range["A1"].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Thông tin chung
                    sh.Range["A3"].Value = "Mã hóa đơn: " + maHDHienTai;
                    sh.Range["A4"].Value = "Ngày bán: " + dtpNgayBan.Value.ToString("dd/MM/yyyy HH:mm");
                    sh.Range["A5"].Value = "Nhân viên: " + txtTenNhanVien.Text;
                    sh.Range["D3"].Value = "Khách hàng: " + txtTenKhachHang.Text;
                    sh.Range["D4"].Value = "Điện thoại: " + txtDienThoai.Text;
                    sh.Range["D5"].Value = "Địa chỉ: " + txtDiaChi.Text;

                    // Tiêu đề cột
                    string[] tieuDe = { "STT", "Tên sản phẩm", "SL", "Đơn giá", "Đường", "Đá", "Thành tiền" };
                    int hang = 7;
                    for (int c = 0; c < tieuDe.Length; c++)
                        sh.Cells[hang, c + 1] = tieuDe[c];
                    sh.Range["A7:G7"].Font.Bold = true;

                    // Các món
                    DataTable ct = hd.LayChiTiet(maHDHienTai);
                    for (int i = 0; i < ct.Rows.Count; i++)
                    {
                        hang++;
                        DataRow r = ct.Rows[i];
                        sh.Cells[hang, 1] = i + 1;
                        sh.Cells[hang, 2] = HoaDonData.Chuoi(r["TenSP"]);
                        sh.Cells[hang, 3] = Convert.ToInt32(r["SoLuong"]);
                        sh.Cells[hang, 4] = Convert.ToDecimal(r["DonGia"]);
                        sh.Cells[hang, 5] = HoaDonData.PhanTram(r["MucDuong"]);
                        sh.Cells[hang, 6] = HoaDonData.PhanTram(r["MucDa"]);
                        sh.Cells[hang, 7] = Convert.ToDecimal(r["ThanhTien"]);
                    }
                    if (ct.Rows.Count > 0)
                    {
                        sh.Range["D8:D" + hang].NumberFormat = "#,##0";
                        sh.Range["G8:G" + hang].NumberFormat = "#,##0";
                    }

                    // Tổng kết
                    hang += 2;
                    sh.Cells[hang, 6] = "Tổng tiền:";
                    sh.Cells[hang, 7] = tongTien;
                    sh.Cells[hang + 1, 6] = "Đã trả:";
                    sh.Cells[hang + 1, 7] = daTra;
                    sh.Cells[hang + 2, 6] = "Còn thiếu:";
                    sh.Cells[hang + 2, 7] = tongTien - daTra > 0 ? tongTien - daTra : 0;
                    sh.Range["F" + hang + ":F" + (hang + 2)].Font.Bold = true;
                    sh.Range["G" + hang + ":G" + (hang + 2)].NumberFormat = "#,##0";

                    sh.Cells[hang + 4, 1] = lblBangChu.Text;

                    sh.Columns.AutoFit();
                    exBook.SaveAs(sfd.FileName);
                    MessageBox.Show("Đã xuất hóa đơn ra file Excel.", "Thông báo",
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
        private void btnDong_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}