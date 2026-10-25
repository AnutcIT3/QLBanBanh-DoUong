using System;
using System.Data;
using System.Windows.Forms;
using QLBanBanhvaDoUong.Classes;

// Nếu namespace trong file của bạn khác, GIỮ dòng namespace cũ của bạn
// và chỉ chép phần bên trong lớp frmKhachHang cùng các dòng using ở trên.
namespace QLBanBanhvaDoUong
{
    public partial class frmKhachHang : Form
    {
        private readonly KhachHangData kh = new KhachHangData();

        private bool laQuanLy;
        private bool dangThem;          // true: đang thêm mới, false: đang sửa

        public frmKhachHang()
        {
            InitializeComponent();
        }

        // =====================================================================
        // LOAD FORM
        // =====================================================================
        private void frmKhachHang_Load(object sender, EventArgs e)
        {
            laQuanLy = string.Equals((Session.ChucVu ?? "").Trim(), "Quản lý",
                                     StringComparison.OrdinalIgnoreCase);

            txtMaKH.ReadOnly = true;                // mã tự sinh
            txtDiemTichLuy.ReadOnly = true;         // điểm do hệ thống cộng, không nhập tay

            TaiDuLieu();
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        // Thêm, sửa khách: mọi nhân viên đều làm được (thu ngân thêm khách ngay tại quầy).
        // Xóa khách: chỉ quản lý.
        private void DatNut(bool them, bool sua, bool xoa, bool luu, bool boQua)
        {
            btnThem.Enabled = them;
            btnSua.Enabled = sua;
            btnXoa.Enabled = xoa && laQuanLy;
            btnLuu.Enabled = luu;
            btnBoQua.Enabled = boQua;
        }

        // =====================================================================
        // DANH SÁCH VÀ TÌM KIẾM
        // =====================================================================
        private void TaiDuLieu()
        {
            dgvKhachHang.DataSource = kh.LayKhachHang(txtTimKiem.Text);

            dgvKhachHang.Columns["MaKH"].HeaderText = "Mã KH";
            dgvKhachHang.Columns["HoTen"].HeaderText = "Họ tên";
            dgvKhachHang.Columns["SDT"].HeaderText = "Số điện thoại";
            dgvKhachHang.Columns["DiaChi"].HeaderText = "Địa chỉ";
            dgvKhachHang.Columns["DiemTichLuy"].HeaderText = "Điểm";
            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Gõ mã, tên hoặc số điện thoại là lọc ngay
        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            TaiDuLieu();
        }

        // =====================================================================
        // CHỌN MỘT KHÁCH: hiện thông tin và lịch sử mua
        // =====================================================================
        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgvKhachHang.Rows[e.RowIndex];

            dangThem = false;
            DatNut(false, true, true, false, true);

            txtMaKH.Text = KhachHangData.Chuoi(r.Cells["MaKH"].Value);
            txtHoTen.Text = KhachHangData.Chuoi(r.Cells["HoTen"].Value);
            txtSDT.Text = KhachHangData.Chuoi(r.Cells["SDT"].Value);
            txtDiaChi.Text = KhachHangData.Chuoi(r.Cells["DiaChi"].Value);
            txtDiemTichLuy.Text = KhachHangData.Chuoi(r.Cells["DiemTichLuy"].Value);

            TaiLichSu(txtMaKH.Text);
        }

        private void TaiLichSu(string maKH)
        {
            dgvLichSu.DataSource = kh.LayLichSuMua(maKH);

            dgvLichSu.Columns["MaHD"].HeaderText = "Mã HĐ";
            dgvLichSu.Columns["NgayLap"].HeaderText = "Ngày";
            dgvLichSu.Columns["NgayLap"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvLichSu.Columns["TongTien"].HeaderText = "Tổng tiền";
            dgvLichSu.Columns["TongTien"].DefaultCellStyle.Format = "N0";
            dgvLichSu.Columns["TrangThai"].HeaderText = "Trạng thái";
            dgvLichSu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Bấm đúp một hóa đơn trong lịch sử là mở hóa đơn đó
        private void dgvLichSu_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string maHD = KhachHangData.Chuoi(dgvLichSu.Rows[e.RowIndex].Cells["MaHD"].Value);
            using (var f = new frmHoaDon(maHD))
            {
                f.ShowDialog(this);
            }

            // Hóa đơn có thể vừa được thanh toán hoặc hủy, nạp lại cho đúng
            if (txtMaKH.Text != "") TaiLichSu(txtMaKH.Text);
        }

        // =====================================================================
        // THÊM MỚI, SỬA, BỎ QUA
        // =====================================================================
        private void btnThem_Click(object sender, EventArgs e)
        {
            dangThem = true;
            ResetValue();
            txtMaKH.Text = kh.SinhMaKH();           // hiện mã dự kiến
            DatNut(false, false, false, true, true);
            txtHoTen.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            dangThem = false;
            DatNut(false, true, false, true, true);
            txtHoTen.Focus();
        }

        private void btnBoQua_Click(object sender, EventArgs e)
        {
            dangThem = false;
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        private void ResetValue()
        {
            txtMaKH.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            txtDiaChi.Clear();
            txtDiemTichLuy.Text = "0";
            dgvLichSu.DataSource = null;
        }

        // =====================================================================
        // LƯU
        // =====================================================================
        private void btnLuu_Click(object sender, EventArgs e)
        {
            string hoTen = txtHoTen.Text.Trim();
            string sdt = txtSDT.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();

            if (hoTen == "")
            {
                MessageBox.Show("Bạn phải nhập họ tên khách hàng!");
                txtHoTen.Focus();
                return;
            }
            if (!KhachHangData.SDTHopLe(sdt))
            {
                MessageBox.Show("Số điện thoại phải gồm 9 đến 11 chữ số.");
                txtSDT.Focus();
                return;
            }

            try
            {
                if (dangThem)
                {
                    string maKH = kh.SinhMaKH();    // sinh lại phòng khi máy khác vừa thêm khách

                    if (kh.TrungSDT(sdt, ""))
                    {
                        MessageBox.Show("Số điện thoại này đã thuộc về một khách hàng khác.");
                        txtSDT.Focus();
                        return;
                    }

                    kh.Them(maKH, hoTen, sdt, diaChi);
                    MessageBox.Show("Đã thêm khách hàng " + maKH + ".");
                }
                else
                {
                    string maKH = txtMaKH.Text.Trim();
                    if (maKH == "")
                    {
                        MessageBox.Show("Hãy chọn một khách hàng trong danh sách để sửa.");
                        return;
                    }

                    if (kh.TrungSDT(sdt, maKH))
                    {
                        MessageBox.Show("Số điện thoại này đã thuộc về một khách hàng khác.");
                        txtSDT.Focus();
                        return;
                    }

                    kh.Sua(maKH, hoTen, sdt, diaChi);
                    MessageBox.Show("Đã cập nhật khách hàng " + maKH + ".");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                return;
            }

            dangThem = false;
            TaiDuLieu();
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        // =====================================================================
        // XÓA (chỉ quản lý; khách đã có hóa đơn thì không xóa được)
        // =====================================================================
        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maKH = txtMaKH.Text.Trim();
            if (maKH == "") return;

            try
            {
                int soHD = kh.DemHoaDon(maKH);
                if (soHD > 0)
                {
                    MessageBox.Show("Khách hàng này đã có " + soHD + " hóa đơn nên không thể xóa.");
                    return;
                }

                if (MessageBox.Show("Bạn có muốn xóa khách hàng " + maKH + " không?", "Xóa khách hàng",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                kh.Xoa(maKH);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                return;
            }

            dangThem = false;
            TaiDuLieu();
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}