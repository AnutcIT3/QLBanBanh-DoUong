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
        private void CapNhatTong()
        {
            tongTien = BanHangData.TinhTong(gio);
            lblTongTien.Text = "Tổng tiền: " + tongTien.ToString("N0") + " đ";
        }

        private void DatLaiForm()
        {
            gio.Clear();
            cboKhachHang.SelectedIndex = 0;
            nudSoLuong.Value = 1;
            CapNhatTong();
            HienMaHoaDon();
        }
        private void TaiSanPham()
        {
            string maLoai = cboLoai.SelectedValue == null ? "" : cboLoai.SelectedValue.ToString();
            dgvSanPham.DataSource = bh.LaySanPham(maLoai,txtTimKiem.Text); // Gọi hàm từ đối tượng bh

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

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (dangNap) return;
            TaiSanPham();
        }
        private void HienMaHoaDon()
        {
            try { lblMaHD.Text = "Hóa đơn: " + bh.SinhMaHD(); }
            catch { lblMaHD.Text = "Hóa đơn: —"; }
        }

        private void cboLoai_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dangNap) return;
            TaiSanPham();
        }

        private void dgvSanPham_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSanPham.CurrentRow == null)
            {
                HienThiAnh(null);
                cboDa.Enabled = false;
                rdo100.Enabled = rdo50.Enabled = rdo25.Enabled = false;
                return;
            }

            DataGridViewRow r = dgvSanPham.CurrentRow;
            bool laDoUong = BanHangData.LaDoUong(BanHangData.Chuoi(r.Cells["MaLoai"].Value));
            rdo100.Enabled = laDoUong;
            rdo50.Enabled = laDoUong;
            rdo25.Enabled = laDoUong;
            cboDa.Enabled = laDoUong;
            cboDa.Enabled = laDoUong;

            string anh = BanHangData.Chuoi(r.Cells["HinhAnh"].Value);
            HienThiAnh(anh == "" ? null : Path.Combine(imgFolder, anh));
        }

        private void dgvSanPham_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ThemMon();
        }
        private void HienThiAnh(string path)
        {
            if (picAnh.Image != null) { picAnh.Image.Dispose(); picAnh.Image = null; }
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            using (var img = Image.FromStream(ms))
                picAnh.Image = new Bitmap(img);
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            ThemMon();
        }
        private void ThemMon()
        {
            if (dgvSanPham.CurrentRow == null)
            {
                MessageBox.Show("Hãy chọn một sản phẩm.");
                return;
            }

            DataGridViewRow r = dgvSanPham.CurrentRow;
            string ma = BanHangData.Chuoi(r.Cells["MaSP"].Value);
            string ten = BanHangData.Chuoi(r.Cells["TenSP"].Value);
            decimal gia = Convert.ToDecimal(r.Cells["DonGia"].Value);
            bool laDoUong = BanHangData.LaDoUong(BanHangData.Chuoi(r.Cells["MaLoai"].Value));
            int soLuong = (int)nudSoLuong.Value;

            // Bánh thì không có mức đường và đá (để trống)
            object duong = laDoUong ? LayMucDuongChon() : DBNull.Value; ;
            object da = laDoUong ? (object)BanHangData.DocPhanTram(cboDa.Text) : DBNull.Value;

            DataRow dong = gio.Rows.Find(ma);
            if (dong == null)
            {
                gio.Rows.Add(ma, ten, soLuong, gia, duong, da);
            }
            else if (Equals(dong["MucDuong"], duong) && Equals(dong["MucDa"], da))
            {
                dong["SoLuong"] = (int)dong["SoLuong"] + soLuong;
            }
            else
            {
                MessageBox.Show("Món này đã có trong giỏ với mức đường hoặc đá khác.\n" +
                                "Hãy xóa dòng cũ, hoặc chọn cùng mức để cộng thêm số lượng.");
                return;
            }

            nudSoLuong.Value = 1;
            CapNhatTong();
        }
        private object LayMucDuongChon()
        {
            if (rdo100.Checked) return 100;
            if (rdo50.Checked) return 50;
            if (rdo25.Checked) return 25;
            return 100; // Mặc định 100% nếu không nút nào được chọn
        }
        private DataRow DongGioDangChon()
        {
            if (dgvGioHang.CurrentRow == null) return null;
            return gio.Rows.Find(BanHangData.Chuoi(dgvGioHang.CurrentRow.Cells["MaSP"].Value));
        }

        private void btnCong_Click(object sender, EventArgs e)
        {
            DataRow dong = DongGioDangChon();
            if (dong == null) return;
            dong["SoLuong"] = (int)dong["SoLuong"] + 1;
            CapNhatTong();
        }

        private void btnTru_Click(object sender, EventArgs e)
        {
            DataRow dong = DongGioDangChon();
            if (dong == null) return;
            if ((int)dong["SoLuong"] > 1)
                dong["SoLuong"] = (int)dong["SoLuong"] - 1;
            CapNhatTong();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            DataRow dong = DongGioDangChon();
            if (dong == null)
            {
                MessageBox.Show("Hãy chọn một dòng trong giỏ hàng.");
                return;
            }
            gio.Rows.Remove(dong);
            CapNhatTong();
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            if (gio.Rows.Count > 0 &&
                MessageBox.Show("Xóa toàn bộ giỏ hàng?", "Làm mới",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            DatLaiForm();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (gio.Rows.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống.");
                return;
            }

            try
            {
                string maHD = bh.SinhMaHD();
                string maKH = cboKhachHang.SelectedValue == null ? "" : cboKhachHang.SelectedValue.ToString();

                bh.LuuHoaDon(maHD, Session.MaNV, maKH, gio);

                MessageBox.Show("Đã lưu hóa đơn " + maHD + ".\nTổng tiền: " + tongTien.ToString("N0") + " đ",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (var f = new frmThanhToan(maHD)) { f.ShowDialog(this); }

                DatLaiForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được hóa đơn: " + ex.Message);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (gio.Rows.Count > 0 &&
               MessageBox.Show("Giỏ hàng chưa được lưu. Bạn vẫn muốn thoát?", "Thoát",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            Close();
        }
    }
}
