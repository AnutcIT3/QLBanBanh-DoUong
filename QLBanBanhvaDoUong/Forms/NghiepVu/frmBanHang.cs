using QLBanBanhvaDoUong.Classes;
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

namespace QLBanBanhvaDoUong.Forms.NghiepVu
{
    public partial class frmBanHang : Form
    {
        private readonly BanHangData bh = new BanHangData();    // SELECT và hàm phụ của form này
        private readonly Function fn = new Function();
        private readonly string imgFolder = Path.Combine(Application.StartupPath, "Images");

        private DataTable gio;          // giỏ hàng (chưa lưu vào CSDL)
        private bool dangNap;           // true khi đang nạp ComboBox để khỏi chạy sự kiện lọc
        private decimal tongTien;
        public frmBanHang()
        {
            InitializeComponent();
        }

        private void frmBanHang_Load(object sender, EventArgs e)
        {
            lblNhanVien.Text = "Nhân viên: " + Session.HoTen;

            dangNap = true;

            fn.FillComBox(cboLoai, bh.LayLoai(), "TenLoai", "MaLoai");
            cboLoai.SelectedIndex = 0;

            fn.FillComBox(cboKhachHang, bh.LayKhachHang(), "TenHienThi", "MaKH");
            cboKhachHang.SelectedIndex = 0;

            // Mặc định chọn 100% đường
            rdo100.Checked = true;

            // Nạp dữ liệu mức đá vào ComboBox
            cboDa.Items.Clear();
            cboDa.Items.AddRange(new object[] { "0%", "30%", "50%", "100%" });
            cboDa.SelectedItem = "100%";

            dangNap = false;

            // Thiết lập mặc định cho ô số lượng NumericUpDown
            nudSoLuong.Minimum = 1;
            nudSoLuong.Maximum = 50;
            nudSoLuong.Value = 1;

            TaoGioHang();
            TaiSanPham();
            HienMaHoaDon();
        }
        private void TaoGioHang()
        {
            gio = BanHangData.TaoGioHang(); // Gọi hàm tĩnh từ BanHangData
            dgvGioHang.DataSource = gio;

            dgvGioHang.Columns["MaSP"].HeaderText = "Mã SP";
            dgvGioHang.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgvGioHang.Columns["SoLuong"].HeaderText = "SL";
            dgvGioHang.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvGioHang.Columns["MucDuong"].HeaderText = "Đường";
            dgvGioHang.Columns["MucDa"].HeaderText = "Đá";
            dgvGioHang.Columns["ThanhTien"].HeaderText = "Thành tiền";

            dgvGioHang.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvGioHang.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            dgvGioHang.Columns["MucDuong"].DefaultCellStyle.Format = "0'%'";
            dgvGioHang.Columns["MucDa"].DefaultCellStyle.Format = "0'%'";
            dgvGioHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            CapNhatTong();
        }
        private void TaiSanPham()
        {
            string maLoai = cboLoai.SelectedValue == null ? "" : cboLoai.SelectedValue.ToString();
            dgvSanPham.DataSource = bh.LaySanPham(maLoai, txtTimKiem.Text); // Gọi hàm từ đối tượng bh

            dgvSanPham.Columns["MaSP"].HeaderText = "Mã SP";
            dgvSanPham.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgvSanPham.Columns["TenLoai"].HeaderText = "Loại";
            dgvSanPham.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvSanPham.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvSanPham.Columns["DonViTinh"].HeaderText = "ĐVT";
            dgvSanPham.Columns["MaLoai"].Visible = false;
            dgvSanPham.Columns["HinhAnh"].Visible = false;
            dgvSanPham.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
