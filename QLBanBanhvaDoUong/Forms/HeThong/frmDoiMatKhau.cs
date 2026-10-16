using QLBanBanhvaDoUong.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLBanBanhvaDoUong.Forms.HeThong
{
    public partial class frmDoiMatKhau : Form
    {
        private readonly DataProcesser dp = new DataProcesser();
        public frmDoiMatKhau()
        {
            InitializeComponent();
        }
        public frmDoiMatKhau(string maNV) : this()
        {
            txtMaNV.Text = maNV;
        }

        private void txtMaNV_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmDoiMatKhau_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Session.MaNV))
            {
                txtMaNV.Text = Session.MaNV;
                txtMaNV.ReadOnly = true;
                txtMKcu.Focus();
            }
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            string cu = txtMKcu.Text;
            string moi = txtMKmoi.Text;
            string nhapLai = txtNhapLai.Text;

            if (maNV == "" || cu == "" || moi == "" || nhapLai == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }
            if (moi.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự.");
                txtMKmoi.Focus();
                return;
            }
            if (moi != nhapLai)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp.");
                txtNhapLai.Clear();
                txtNhapLai.Focus();
                return;
            }
            if (moi == cu)
            {
                MessageBox.Show("Mật khẩu mới phải khác mật khẩu cũ.");
                txtMKmoi.Focus();
                return;
            }

            try
            {
                // Kiểm tra mật khẩu cũ (phân biệt hoa thường)
                DataTable dt = dp.DocBang(
                    "SELECT COUNT(*) FROM NhanVien " +
                    "WHERE MaNV = @ma AND MatKhau = @cu COLLATE Latin1_General_CS_AS",
                    new SqlParameter("@ma", maNV),
                    new SqlParameter("@cu", cu));

                if (Convert.ToInt32(dt.Rows[0][0]) == 0)
                {
                    MessageBox.Show("Mã nhân viên hoặc mật khẩu cũ không đúng.");
                    txtMKcu.Clear();
                    txtMKcu.Focus();
                    return;
                }

                dp.CapNhat(
                    "UPDATE NhanVien SET MatKhau = @moi WHERE MaNV = @ma",
                    new SqlParameter("@moi", moi),
                    new SqlParameter("@ma", maNV));

                MessageBox.Show("Đổi mật khẩu thành công.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;   // đóng form
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
