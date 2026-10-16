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
    public partial class frmDangNhap : Form
    {
        private readonly DataProcesser dp = new DataProcesser();
        public frmDangNhap()
        {
            InitializeComponent();
        }

        private void txtMaNV_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string tk = txtMaNV.Text.Trim();
            string mk = txtMK.Text;

            if (tk == "" || mk == "")
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản và mật khẩu.");
                return;
            }

            try
            {
                DataTable dt = dp.DocBang(
                    "SELECT MaNV, HoTen, ChucVu FROM NhanVien " +
                    "WHERE MaNV = @tk " +
                    "AND MatKhau = @mk COLLATE Latin1_General_CS_AS",   // phân biệt hoa thường
                    new SqlParameter("@tk", tk),
                    new SqlParameter("@mk", mk));

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Sai tên tài khoản hoặc mật khẩu.");
                    txtMK.Clear();
                    txtMK.Focus();
                    return;
                }

                Session.MaNV = dt.Rows[0]["MaNV"].ToString();
                Session.HoTen = dt.Rows[0]["HoTen"].ToString();
                Session.ChucVu = dt.Rows[0]["ChucVu"].ToString();

                DialogResult = DialogResult.OK;   // đóng form và báo đăng nhập thành công
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối được CSDL:\n" + ex.Message);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnDoiMK_Click(object sender, EventArgs e)
        {
            using (var f = new frmDoiMatKhau(txtMaNV.Text.Trim()))
            {
                f.ShowDialog(this);
            }
            txtMK.Clear();
            txtMK.Focus();
        }
    }
}
