using QLBanBanhvaDoUong.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLBanBanhvaDoUong.Forms.NghiepVu
{
    public partial class frmThanhToan : Form
    {
        private readonly ThanhToanData tt = new ThanhToanData();
        private readonly Function fn = new Function();
        private readonly string maHD;
        private readonly string imgFolder = Path.Combine(Application.StartupPath, "Images");

        // Tài khoản ngân hàng nhận tiền của cửa hàng (dùng để tạo mã VietQR). Sửa cho đúng tài khoản của bạn.
        private const string NganHang = "MB";                  // Mã ngân hàng: MB, VCB, TCB, ACB, VPB, ICB...
        private const string SoTaiKhoan = "0356123456";        // Số tài khoản
        private const string TenTaiKhoan = "DELICACY BAKERY";  // Tên chủ tài khoản, viết hoa không dấu

        // Chờ người dùng gõ xong số tiền rồi mới tải lại mã QR, khỏi gọi mạng sau mỗi phím
      

        private decimal tongTien, daTra, conThieu;
        private bool dangNap;

        private void frmThanhToan_Load(object sender, EventArgs e)
        {
            dangNap = true;
            fn.FillComBox(cboPhuongThuc, tt.LayPhuongThuc(), "TenPT", "MaPT");
            cboPhuongThuc.SelectedIndex = 0;
            dangNap = false;

            // Gọi API VietQR qua HTTPS cần TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

            picQR.SizeMode = PictureBoxSizeMode.Zoom;
            picQR.LoadCompleted += picQR_LoadCompleted;
            tmrQR.Tick += tmrQR_Tick;
            FormClosed += (s, ev) => tmrQR.Dispose();

            HienTheoPhuongThuc();
            LamMoiSoLieu();

        }

        public frmThanhToan() : this("") { }

        private void cboPhuongThuc_SelectedIndexChanged(object sender, EventArgs e)
        {
            string maPT = cboPhuongThuc.SelectedValue == null ? "" : cboPhuongThuc.SelectedValue.ToString();
            bool laTienMat = maPT == "TM";

            pnlTienMat.Visible = laTienMat;
            pnlChuyenKhoan.Visible = !laTienMat;
            HienQR(maPT);
        }



        // frmBanHang mở form này với mã hóa đơn vừa lưu
        public frmThanhToan(string maHD)
        {
            InitializeComponent();
            this.maHD = maHD;
        }
        // Đọc số liệu hóa đơn và các lần thanh toán, rồi cập nhật giao diện
        private void LamMoiSoLieu()
        {
            DataRow h = tt.LayHoaDon(maHD);
            if (h == null)
            {
                MessageBox.Show("Không tìm thấy hóa đơn " + maHD + ".");
                Close();
                return;
            }

            tongTien = Convert.ToDecimal(h["TongTien"]);
            daTra = tt.TongDaTra(maHD);
            conThieu = tongTien - daTra;
            string trangThai = ThanhToanData.Chuoi(h["TrangThai"]);

            lblDonHang.Text = "Đơn hàng: " + maHD + "  |  Khách: " + ThanhToanData.Chuoi(h["TenKH"]);
            lblTongTien.Text = "Tổng tiền: " + tongTien.ToString("N0") + " đ";
            lblDaTra.Text = "Đã trả: " + daTra.ToString("N0") + " đ";
            lblConThieu.Text = "Còn thiếu: " + (conThieu > 0 ? conThieu : 0).ToString("N0") + " đ";
            lblTrangThai.Text = "Trạng thái: " + trangThai;

            dgvThanhToan.DataSource = tt.LayThanhToan(maHD);
            dgvThanhToan.Columns["MaPT"].Visible = false;
            dgvThanhToan.Columns["TenPT"].HeaderText = "Phương thức";
            dgvThanhToan.Columns["SoTien"].HeaderText = "Số tiền";
            dgvThanhToan.Columns["SoTien"].DefaultCellStyle.Format = "N0";
            dgvThanhToan.Columns["NgayTT"].HeaderText = "Thời gian";
            dgvThanhToan.Columns["NgayTT"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvThanhToan.Columns["MaGiaoDich"].HeaderText = "Mã giao dịch";
            dgvThanhToan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Chỉ cho nhập thanh toán khi hóa đơn còn nợ và chưa bị hủy
            bool choPhepTra = trangThai == ThanhToanData.ChuaThanhToan && conThieu > 0;
            cboPhuongThuc.Enabled = choPhepTra;
            txtSoTien.Enabled = choPhepTra;
            pnlTienMat.Enabled = choPhepTra;
            pnlChuyenKhoan.Enabled = choPhepTra;
            btnThemTT.Enabled = choPhepTra;
            btnHuyHD.Enabled = trangThai == ThanhToanData.ChuaThanhToan && daTra == 0;

            txtSoTien.Text = choPhepTra ? conThieu.ToString("0") : "";
            txtKhachDua.Clear();
            txtMaGiaoDich.Clear();
            lblTienTraLai.Text = "Tiền thối: 0 đ";

            HienQR(MaPTDangChon());
        }
        private void HienTheoPhuongThuc()
        {
            string maPT = cboPhuongThuc.SelectedValue == null ? "" : cboPhuongThuc.SelectedValue.ToString();
            bool laTienMat = maPT == "TM";

            pnlTienMat.Visible = laTienMat;
            pnlChuyenKhoan.Visible = !laTienMat;
            HienQR(maPT);
        }
        private void HienQR(string maPT)
        {
            tmrQR.Stop();
            picQR.CancelAsync();                               // hủy lần tải trước nếu còn dở

            bool hien = (maPT == "CK" || maPT == "VI");
            picQR.Visible = hien;
            lblQRTrangThai.Visible = hien;
            if (!hien) return;

            XoaAnhQR();

            if (maPT == "VI")
            {
                string file = Path.Combine(imgFolder, "QR.png");
                if (File.Exists(file)) picQR.Image = DocAnh(file);
                lblQRTrangThai.Text = "Quét mã ví điện tử của cửa hàng.";
                return;
            }

            decimal soTien;
            if (!ThanhToanData.DocTien(txtSoTien.Text, out soTien) || soTien <= 0)
            {
                lblQRTrangThai.Text = "Nhập số tiền để tạo mã QR.";
                return;
            }
            HienThiVietQR(maHD, soTien);
        }
        private void HienThiVietQR(string maHD, decimal soTien)
        {
            try
            {
                string noiDung = maHD;                         // nội dung chuyển khoản là mã hóa đơn

                string urlQR = "https://img.vietqr.io/image/" + NganHang + "-" + SoTaiKhoan + "-compact.png" +
                               "?amount=" + soTien.ToString("0", CultureInfo.InvariantCulture) +
                               "&addInfo=" + Uri.EscapeDataString(noiDung) +
                               "&accountName=" + Uri.EscapeDataString(TenTaiKhoan);

                lblQRTrangThai.Text = "Đang tải mã QR...";
                picQR.LoadAsync(urlQR);                        // tải ngầm, form không bị đứng
            }
            catch (Exception ex)
            {
                lblQRTrangThai.Text = "Không tạo được mã QR: " + ex.Message;
            }
        }
        private void picQR_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) return;

            if (e.Error != null)
            {
                XoaAnhQR();
                lblQRTrangThai.Text = "Không tải được mã QR. Kiểm tra kết nối Internet.";
                return;
            }

            decimal soTien;
            ThanhToanData.DocTien(txtSoTien.Text, out soTien);
            lblQRTrangThai.Text = "Quét mã để chuyển " + soTien.ToString("N0") + " đ, nội dung " + maHD + ".";
        }

        private void XoaAnhQR()
        {
            if (picQR.Image != null) { picQR.Image.Dispose(); picQR.Image = null; }
        }

        // Tính tiền thối mỗi khi sửa số tiền trả hoặc số tiền khách đưa
        private void txtKhachDua_TextChanged(object sender, EventArgs e)
        {
            CapNhatTienThoi();
        }

        private void btnThemTT_Click(object sender, EventArgs e)
        {
            decimal soTien;
            if (!ThanhToanData.DocTien(txtSoTien.Text, out soTien) || soTien <= 0)
            {
                MessageBox.Show("Số tiền thanh toán không hợp lệ.");
                txtSoTien.Focus();
                return;
            }
            if (soTien > conThieu)
            {
                MessageBox.Show("Số tiền vượt quá số còn thiếu (" + conThieu.ToString("N0") + " đ).");
                txtSoTien.Text = conThieu.ToString("0");
                return;
            }

            string maPT = cboPhuongThuc.SelectedValue.ToString();
            string thongBaoThoi = "";

            if (maPT == "TM")
            {
                // Để trống ô "khách đưa" nghĩa là khách đưa đúng số tiền
                decimal khachDua;
                if (txtKhachDua.Text.Trim() != "")
                {
                    if (!ThanhToanData.DocTien(txtKhachDua.Text, out khachDua) || khachDua < soTien)
                    {
                        MessageBox.Show("Số tiền khách đưa chưa đủ.");
                        txtKhachDua.Focus();
                        return;
                    }
                    if (khachDua > soTien)
                        thongBaoThoi = "\nTiền thối: " + (khachDua - soTien).ToString("N0") + " đ";
                }
            }

            try
            {
                tt.GhiThanhToan(maHD, maPT, soTien, txtMaGiaoDich.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không ghi được thanh toán: " + ex.Message);
                return;
            }

            LamMoiSoLieu();

            if (conThieu <= 0)
            {
                MessageBox.Show("Hóa đơn " + maHD + " đã thanh toán đủ." + thongBaoThoi,
                    "Thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Đã ghi thanh toán " + soTien.ToString("N0") + " đ." +
                                "\nCòn thiếu " + conThieu.ToString("N0") + " đ." + thongBaoThoi,
                    "Thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnHuyHD_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Hủy hóa đơn " + maHD + "?", "Hủy hóa đơn",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                if (!tt.HuyHoaDon(maHD))
                {
                    MessageBox.Show("Hóa đơn đã có thanh toán nên không thể hủy.");
                    return;
                }
                DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtSoTien_TextChanged(object sender, EventArgs e)
        {
            CapNhatTienThoi();
            tmrQR.Stop();                                      // tải lại mã QR sau khi ngừng gõ
            tmrQR.Start();
        }

        private void CapNhatTienThoi()
        {
            decimal soTien, khachDua;
            if (ThanhToanData.DocTien(txtSoTien.Text, out soTien) &&
                ThanhToanData.DocTien(txtKhachDua.Text, out khachDua) && khachDua >= soTien)
                lblTienTraLai.Text = "Tiền trả lại: " + (khachDua - soTien).ToString("N0") + " đ";
            else
                lblTienTraLai.Text = "Tiền trả lại: 0 đ";
        }
        private static Image DocAnh(string path)
        {
            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            using (var img = Image.FromStream(ms))
                return new Bitmap(img);
        }
        // =====================================================================
        // MÃ QR: chuyển khoản dùng VietQR theo đúng số tiền đang nhập,
        // ví điện tử dùng ảnh QR cố định của cửa hàng (file Images\QR.png)
        // =====================================================================
        private string MaPTDangChon()
        {
            return cboPhuongThuc.SelectedValue == null ? "" : cboPhuongThuc.SelectedValue.ToString();
        }

        private void tmrQR_Tick(object sender, EventArgs e)
        {
            tmrQR.Stop();
            HienQR(MaPTDangChon());
        }
    }
}
