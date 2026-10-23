using QLBanBanhvaDoUong.Classes;
using QLBanBanhvaDoUong.Forms.DanhMuc;
using QLBanBanhvaDoUong.Forms.NghiepVu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLBanBanhvaDoUong.Fomrs.HeThong
{
    public partial class frmMain : Form
    {
        private readonly DataProcesser dp = new DataProcesser();
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblXinChao.Text = "Xin chào, " + Session.HoTen;

            // Chỉ quản lý được vào Nhân viên và Thống kê
            bool laQuanLy = Session.ChucVu == "Quản lý";
            nhanVienToolStripMenuItem.Visible = laQuanLy;
            thongKeToolStripMenuItem.Visible = laQuanLy;

            TaiThongKe();
        }
        private void TaiThongKe()
        {
            try
            {
                DataTable dt = dp.DocBang(
                    "SELECT ISNULL(SUM(TongTien),0) FROM HoaDon " +
                    "WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)");
                decimal doanhThu = Convert.ToDecimal(dt.Rows[0][0]);
                lblDoanhThuNgay.Text = "Doanh thu: " + string.Format("{0:N0} đ", doanhThu);
            }
            catch
            {
                lblDoanhThuNgay.Text = "Doanh thu: —";   // chưa kết nối được CSDL thì form vẫn mở
            }
        }
        private void MoForm(Form f)
        {
            using (f) { f.ShowDialog(this); }
            TaiThongKe();
        }

        private void banHangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBanHang bh = new frmBanHang();
            bh.ShowDialog(this);
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Restart();   // quay lại form đăng nhập
            }
        }

        private void sanPhamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSanPham sp = new frmSanPham();
            sp.ShowDialog(this);
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
            MoForm(new frmSanPham("L04"));
        }

        private void cafeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoForm(new frmSanPham("L03"));
        }

        private void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSanPham sp = new frmSanPham();
            sp.ShowDialog(this);
        }
    }
}
