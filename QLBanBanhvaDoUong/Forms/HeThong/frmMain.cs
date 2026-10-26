using QLBanBanhvaDoUong.Classes;
using QLBanBanhvaDoUong.Forms.DanhMuc;
using QLBanBanhvaDoUong.Forms.NghiepVu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace QLBanBanhvaDoUong.Fomrs.HeThong
{
    public partial class frmMain : Form
    {
        private readonly DataProcesser dp = new DataProcesser();
        private readonly string imgFolder = Path.Combine(Application.StartupPath, "Images");

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblXinChao.Text = "Xin chào, " + Session.HoTen;

            // Chỉ quản lý mới thấy mục Nhân viên và Thống kê
            bool laQuanLy = string.Equals(Session.ChucVu, "Quản lý", StringComparison.OrdinalIgnoreCase);
            nhanVienToolStripMenuItem.Visible = laQuanLy;
            thongKeToolStripMenuItem.Visible = laQuanLy;
            picTop1.SizeMode = PictureBoxSizeMode.Zoom;
            picTop2.SizeMode = PictureBoxSizeMode.Zoom;

            // Cập nhật số liệu và danh sách
            CapNhatTrangThai();
        }

        private void CapNhatTrangThai()
        {
            TaiDoanhThuNgay();
            TaiSanPhamNoiBat();
        }

        // 1. Tải tổng doanh thu trong ngày
        private void TaiDoanhThuNgay()
        {
            try
            {
                DataTable dt = dp.DocBang(
                    "SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon " +
                    "WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)");

                decimal doanhThu = Convert.ToDecimal(dt.Rows[0][0]);
                lblDoanhThuNgay.Text = "Doanh thu hôm nay: " + string.Format("{0:N0} đ", doanhThu);
            }
            catch
            {
                lblDoanhThuNgay.Text = "Doanh thu hôm nay: —";
            }
        }

        // 2. Tải Top 5 sản phẩm nổi bật/bán chạy lên DataGridView (dgvSanPhamNoiBat)
        private void TaiSanPhamNoiBat()
        {
            try
            {
                // Truy vấn lấy Top 5 sản phẩm bán chạy nhất (bao gồm cả cột HinhAnh)
                string sql = "SELECT TOP 5 sp.TenSP, lsp.TenLoai, sp.DonGia, " +
                             "ISNULL(SUM(ct.SoLuong), 0) AS DaBan, sp.HinhAnh " +
                             "FROM SanPham sp " +
                             "JOIN LoaiSanPham lsp ON lsp.MaLoai = sp.MaLoai " +
                             "LEFT JOIN ChiTietHoaDon ct ON ct.MaSP = sp.MaSP " +
                             "GROUP BY sp.TenSP, lsp.TenLoai, sp.DonGia, sp.HinhAnh " +
                             "ORDER BY DaBan DESC";

                DataTable dt = dp.DocBang(sql);
                dgvSanPhamNoiBat.DataSource = dt;

                // Ẩn cột HinhAnh trên DataGridView
                if (dgvSanPhamNoiBat.Columns["HinhAnh"] != null)
                    dgvSanPhamNoiBat.Columns["HinhAnh"].Visible = false;

                dgvSanPhamNoiBat.Columns["TenSP"].HeaderText = "Tên sản phẩm";
                dgvSanPhamNoiBat.Columns["TenLoai"].HeaderText = "Loại sản phẩm";
                dgvSanPhamNoiBat.Columns["DonGia"].HeaderText = "Đơn giá (VNĐ)";
                dgvSanPhamNoiBat.Columns["DonGia"].DefaultCellStyle.Format = "N0";
                dgvSanPhamNoiBat.Columns["DaBan"].HeaderText = "Đã bán";
                dgvSanPhamNoiBat.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // --- HIỂN THỊ HÌNH ẢNH 2 SẢN PHẨM CAO NHẤT ---
                // Reset ảnh cũ
                XoaAnh(picTop1);
                XoaAnh(picTop2);

                if (dt != null && dt.Rows.Count > 0)
                {
                    // Sản phẩm Top 1
                    string anhTop1 = dt.Rows[0]["HinhAnh"].ToString();
                    if (!string.IsNullOrEmpty(anhTop1))
                        picTop1.Image = DocAnhAnToan(Path.Combine(imgFolder, anhTop1));

                    // Sản phẩm Top 2 (nếu có)
                    if (dt.Rows.Count > 1)
                    {
                        string anhTop2 = dt.Rows[1]["HinhAnh"].ToString();
                        if (!string.IsNullOrEmpty(anhTop2))
                            picTop2.Image = DocAnhAnToan(Path.Combine(imgFolder, anhTop2));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được sản phẩm nổi bật: " + ex.Message);
            }
        }
        private Image DocAnhAnToan(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(path)))
                using (var img = Image.FromStream(ms))
                {
                    return new Bitmap(img);
                }
            }
            catch
            {
                return null;
            }
        }

        private void XoaAnh(PictureBox pic)
        {
            if (pic.Image != null)
            {
                pic.Image.Dispose();
                pic.Image = null;
            }
        }

        private void MoForm(Form f)
        {
            using (f)
            {
                f.ShowDialog(this);
            }
            CapNhatTrangThai(); // Tự động load lại doanh thu sau khi đóng các form nghiệp vụ
        }

        // =====================================================================
        // SỰ KIỆN MENU
        // =====================================================================
        private void banHangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmBanHang());
        }

        private void sanPhamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham());
        }

        private void banhNgotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham("L01"));
        }

        private void traToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham("L02"));
        }

        private void banhManToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham("L01"));
        }

        private void cafeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham("L03"));
        }

        private void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham());
        }

        private void hoaDonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmHoaDon());
        }

        private void khachHangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmKhachHang());
        }

        private void nhanVienToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmNhanVien());
        }

        private void thongKeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmThongKe());
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống?", "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Restart(); // Khởi động lại ứng dụng về màn hình đăng nhập
            }
        }
    }
}