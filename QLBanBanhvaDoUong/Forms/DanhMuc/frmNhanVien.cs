using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLBanBanhvaDoUong.Classes;

// Nếu namespace trong file của bạn khác, GIỮ dòng namespace cũ của bạn
// và chỉ chép phần bên trong lớp frmNhanVien cùng các dòng using ở trên.
namespace QLBanBanhvaDoUong
{
    public partial class frmNhanVien : Form
    {
        private readonly NhanVienData nv = new NhanVienData();
        private bool dangThem;          // true: đang thêm mới, false: đang sửa

        public frmNhanVien()
        {
            InitializeComponent();
        }

        // =====================================================================
        // LOAD FORM (chỉ quản lý được vào)
        // =====================================================================
        private void frmNhanVien_Load(object sender, EventArgs e)
        {
            if (!string.Equals((Session.ChucVu ?? "").Trim(), NhanVienData.QuanLy,
                               StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Chỉ quản lý mới được vào chức năng này.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            txtMaNV.ReadOnly = true;                   // mã tự sinh
            txtMatKhau.UseSystemPasswordChar = true;

            cboGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ" });

            cboChucVu.DropDownStyle = ComboBoxStyle.DropDownList;
            cboChucVu.Items.AddRange(new object[] { "Quản lý", "Thu ngân", "Pha chế" });

            dtpNgaySinh.Format = DateTimePickerFormat.Short;
            dtpNgaySinh.MaxDate = DateTime.Today;

            TaiDuLieu();
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        // Nút Đặt lại mật khẩu bật cùng lúc với nút Sửa (tức là khi đã chọn một nhân viên)
        private void DatNut(bool them, bool sua, bool xoa, bool luu, bool boQua)
        {
            btnThem.Enabled = them;
            btnSua.Enabled = sua;
            btnSua.Enabled = xoa;
            btnLuu.Enabled = luu;
            btnBoQua.Enabled = boQua;
            btnDatLaiMK.Enabled = sua;
        }

        // =====================================================================
        // DANH SÁCH VÀ TÌM KIẾM
        // =====================================================================
        private void TaiDuLieu()
        {
            dgvNhanVien.DataSource = nv.LayNhanVien(txtTimKiem.Text);

            dgvNhanVien.Columns["MaNV"].HeaderText = "Mã NV";
            dgvNhanVien.Columns["HoTen"].HeaderText = "Họ tên";
            dgvNhanVien.Columns["GioiTinh"].HeaderText = "Giới tính";
            dgvNhanVien.Columns["NgaySinh"].HeaderText = "Ngày sinh";
            dgvNhanVien.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvNhanVien.Columns["SDT"].HeaderText = "Số điện thoại";
            dgvNhanVien.Columns["ChucVu"].HeaderText = "Chức vụ";
            dgvNhanVien.Columns["TrangThai"].HeaderText = "Đang làm";
            dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            TaiDuLieu();
        }

        // =====================================================================
        // CHỌN MỘT NHÂN VIÊN
        // =====================================================================
        private void dgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgvNhanVien.Rows[e.RowIndex];

            dangThem = false;
            DatNut(false, true, true, false, true);

            txtMaNV.Text = NhanVienData.Chuoi(r.Cells["MaNV"].Value);
            txtHoTen.Text = NhanVienData.Chuoi(r.Cells["HoTen"].Value);
            cboGioiTinh.SelectedIndex = cboGioiTinh.FindStringExact(NhanVienData.Chuoi(r.Cells["GioiTinh"].Value));
            txtSDT.Text = NhanVienData.Chuoi(r.Cells["SDT"].Value);
            cboChucVu.SelectedIndex = cboChucVu.FindStringExact(NhanVienData.Chuoi(r.Cells["ChucVu"].Value));
            chkDangLam.Checked = Convert.ToBoolean(r.Cells["TrangThai"].Value);
            txtMatKhau.Clear();                        // không bao giờ hiện mật khẩu; để trống = giữ nguyên

            object ns = r.Cells["NgaySinh"].Value;
            if (ns != null && ns != DBNull.Value)
            {
                DateTime d = Convert.ToDateTime(ns);
                dtpNgaySinh.Value = d > dtpNgaySinh.MaxDate ? dtpNgaySinh.MaxDate : d;
            }
        }

        // =====================================================================
        // THÊM MỚI, SỬA, BỎ QUA
        // =====================================================================
        private void btnThem_Click(object sender, EventArgs e)
        {
            dangThem = true;
            ResetValue();
            txtMaNV.Text = nv.SinhMaNV();              // hiện mã dự kiến
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
            txtMaNV.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            txtMatKhau.Clear();
            cboGioiTinh.SelectedIndex = -1;
            cboChucVu.SelectedIndex = -1;
            dtpNgaySinh.Value = new DateTime(2000, 1, 1);
            chkDangLam.Checked = true;
        }

        // =====================================================================
        // LƯU
        // =====================================================================
        private void btnLuu_Click(object sender, EventArgs e)
        {
            string hoTen = txtHoTen.Text.Trim();
            string sdt = txtSDT.Text.Trim();
            string mk = txtMatKhau.Text;
            bool dangLam = chkDangLam.Checked;

            if (hoTen == "")
            {
                MessageBox.Show("Bạn phải nhập họ tên nhân viên!");
                txtHoTen.Focus(); return;
            }
            if (cboGioiTinh.SelectedIndex < 0)
            {
                MessageBox.Show("Bạn phải chọn giới tính!");
                cboGioiTinh.Focus(); return;
            }
            if (dtpNgaySinh.Value.Date > DateTime.Today.AddYears(-16))
            {
                MessageBox.Show("Nhân viên phải đủ 16 tuổi.");
                dtpNgaySinh.Focus(); return;
            }
            if (!KhachHangData.SDTHopLe(sdt))
            {
                MessageBox.Show("Số điện thoại phải gồm 9 đến 11 chữ số.");
                txtSDT.Focus(); return;
            }
            if (cboChucVu.SelectedIndex < 0)
            {
                MessageBox.Show("Bạn phải chọn chức vụ!");
                cboChucVu.Focus(); return;
            }

            // Mật khẩu: thêm mới thì bắt buộc, sửa thì để trống là giữ nguyên
            if (dangThem && mk == "")
            {
                MessageBox.Show("Bạn phải nhập mật khẩu cho nhân viên mới!");
                txtMatKhau.Focus(); return;
            }
            if (mk != "" && mk.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.");
                txtMatKhau.Focus(); return;
            }

            string gioiTinh = cboGioiTinh.SelectedItem.ToString();
            string chucVu = cboChucVu.SelectedItem.ToString();
            bool laQuanLy = chucVu == NhanVienData.QuanLy;

            try
            {
                string maNV = dangThem ? nv.SinhMaNV() : txtMaNV.Text.Trim();   // sinh lại phòng khi máy khác vừa thêm
                if (maNV == "")
                {
                    MessageBox.Show("Hãy chọn một nhân viên trong danh sách để sửa.");
                    return;
                }

                if (nv.TrungSDT(sdt, dangThem ? "" : maNV))
                {
                    MessageBox.Show("Số điện thoại này đã thuộc về một nhân viên khác.");
                    txtSDT.Focus(); return;
                }

                // Không tự hạ quyền hoặc cho nghỉ việc chính mình
                if (!dangThem && maNV == Session.MaNV && (!laQuanLy || !dangLam))
                {
                    MessageBox.Show("Bạn không thể tự đổi chức vụ hoặc cho chính mình nghỉ việc.");
                    return;
                }

                // Luôn phải còn ít nhất một quản lý đang làm việc, nếu không sẽ không ai đăng nhập quản trị được
                int conLai = nv.SoQuanLyKhac(dangThem ? "" : maNV) + ((laQuanLy && dangLam) ? 1 : 0);
                if (conLai == 0)
                {
                    MessageBox.Show("Hệ thống phải còn ít nhất một quản lý đang làm việc.");
                    return;
                }

                if (dangThem)
                {
                    nv.Them(maNV, hoTen, gioiTinh, dtpNgaySinh.Value, sdt, chucVu, mk, dangLam);
                    MessageBox.Show("Đã thêm nhân viên " + maNV + ".");
                }
                else
                {
                    nv.Sua(maNV, hoTen, gioiTinh, dtpNgaySinh.Value, sdt, chucVu, dangLam, mk);
                    MessageBox.Show("Đã cập nhật nhân viên " + maNV + ".");
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
        // XÓA: nhân viên đã lập hóa đơn thì chuyển sang "nghỉ việc" thay vì xóa
        // =====================================================================
        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            if (maNV == "") return;

            if (maNV == Session.MaNV)
            {
                MessageBox.Show("Bạn không thể xóa chính mình.");
                return;
            }

            try
            {
                // Không xóa hoặc cho nghỉ quản lý cuối cùng
                if (nv.LaQuanLyDangLam(maNV) && nv.SoQuanLyKhac(maNV) == 0)
                {
                    MessageBox.Show("Hệ thống phải còn ít nhất một quản lý đang làm việc.");
                    return;
                }

                if (nv.DemHoaDon(maNV) > 0)
                {
                    if (MessageBox.Show("Nhân viên này đã lập hóa đơn nên không thể xóa.\nChuyển sang trạng thái nghỉ việc?",
                        "Xóa nhân viên", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                    nv.DatTrangThai(maNV, false);
                }
                else
                {
                    if (MessageBox.Show("Bạn có muốn xóa nhân viên " + maNV + " không?", "Xóa nhân viên",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                    nv.Xoa(maNV);
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
        // ĐẶT LẠI MẬT KHẨU cho nhân viên quên
        // =====================================================================
        private void btnDatLaiMK_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            if (maNV == "")
            {
                MessageBox.Show("Hãy chọn một nhân viên trong danh sách.");
                return;
            }

            string mkMoi;
            if (!HoiMatKhauMoi("Đặt lại mật khẩu cho " + maNV, out mkMoi)) return;

            try
            {
                nv.DatLaiMatKhau(maNV, mkMoi);
                MessageBox.Show("Đã đặt lại mật khẩu cho " + maNV + ".", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // Hộp thoại nhập mật khẩu mới hai lần (dựng bằng code, không cần thêm form)
        private static bool HoiMatKhauMoi(string tieuDe, out string matKhau)
        {
            matKhau = "";
            using (var f = new Form())
            using (var l1 = new Label { Text = "Mật khẩu mới:", Left = 12, Top = 16, AutoSize = true })
            using (var t1 = new TextBox { Left = 120, Top = 12, Width = 170, UseSystemPasswordChar = true })
            using (var l2 = new Label { Text = "Nhập lại:", Left = 12, Top = 48, AutoSize = true })
            using (var t2 = new TextBox { Left = 120, Top = 44, Width = 170, UseSystemPasswordChar = true })
            using (var ok = new Button { Text = "Lưu", Left = 134, Top = 84, Width = 75, DialogResult = DialogResult.OK })
            using (var huy = new Button { Text = "Hủy", Left = 215, Top = 84, Width = 75, DialogResult = DialogResult.Cancel })
            {
                f.Text = tieuDe;
                f.ClientSize = new Size(310, 124);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.AcceptButton = ok;
                f.CancelButton = huy;
                f.Controls.AddRange(new Control[] { l1, t1, l2, t2, ok, huy });

                // Mật khẩu ngắn hoặc nhập lại không khớp thì không cho đóng hộp thoại
                ok.Click += (s, ev) =>
                {
                    if (t1.Text.Length < 6)
                    {
                        MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.");
                        t1.Focus();
                        f.DialogResult = DialogResult.None;
                    }
                    else if (t1.Text != t2.Text)
                    {
                        MessageBox.Show("Mật khẩu nhập lại không khớp.");
                        t2.Clear();
                        t2.Focus();
                        f.DialogResult = DialogResult.None;
                    }
                };

                if (f.ShowDialog() != DialogResult.OK) return false;
                matKhau = t1.Text;
                return true;
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}