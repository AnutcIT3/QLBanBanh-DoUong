using QLBanBanhvaDoUong.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace QLBanBanhvaDoUong.Forms.DanhMuc
{
    public partial class frmSanPham : Form
    {
        private readonly DataProcesser dp = new DataProcesser();
        private readonly Function fn = new Function();
        private const string SqlLoai = "SELECT MaLoai, TenLoai FROM LoaiSanPham ORDER BY MaLoai";
        private readonly string maLoaiLoc;
        private readonly string imgFolder = Path.Combine(Application.StartupPath, "Images");

        private bool laQuanLy;
        private bool dangThem;                                 // true: đang thêm mới, false: đang sửa
        private string anhDuocChon = null;                     // đường dẫn ảnh vừa chọn, chưa lưu
        private string tenAnhHienTai = "";
        public frmSanPham() : this(null) { }
        public frmSanPham(string maLoaiLoc)
        {
            InitializeComponent();
            this.maLoaiLoc = maLoaiLoc;
        }
        private void txtTenSanPham_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmSanPham_Load(object sender, EventArgs e)
        {
            laQuanLy = string.Equals((Session.ChucVu ?? "").Trim(), "Quản lý",
                                     StringComparison.OrdinalIgnoreCase);

            fn.FillComBox(cboLoai, dp.DocBang(SqlLoai), "TenLoai", "MaLoai");
            cboLoai.SelectedIndex = -1;
            if (maLoaiLoc != null)
            {
                cboLoai.SelectedValue = maLoaiLoc;
                Text = "Sản phẩm - " + cboLoai.Text;           // tiêu đề form theo loại đang xem
            }

            TaiDuLieu(false);

            // Ban đầu chỉ bấm được Thêm mới (và chỉ khi là quản lý)
            DatNut(true, false, false, false, false);

            lblNhanVien.Text = "Nhân viên: " + Session.HoTen;
        }
        private void DatNut(bool them, bool sua, bool xoa, bool luu, bool boQua)
        {
            btnThem.Enabled = them && laQuanLy;
            btnSua.Enabled = sua && laQuanLy;
            btnXoa.Enabled = xoa && laQuanLy;
            btnLuu.Enabled = luu && laQuanLy;
            btnBoQua.Enabled = boQua;
        }
        private void TaiDuLieu(bool timKiem)
        {
            string sql =
                "SELECT sp.MaSP, sp.TenSP, lsp.TenLoai, sp.DonGia, sp.DonViTinh, " +
                "       sp.TrangThai, sp.MaLoai, sp.HinhAnh " +
                "FROM SanPham sp JOIN LoaiSanPham lsp ON lsp.MaLoai = sp.MaLoai " +
                "WHERE 1 = 1";
            var ps = new List<SqlParameter>();

            if (maLoaiLoc != null)
            {
                sql += " AND sp.MaLoai = @locLoai";
                ps.Add(new SqlParameter("@locLoai", maLoaiLoc));
            }

            if (timKiem)
            {
                if (txtMaSP.Text.Trim() != "")
                {
                    sql += " AND sp.MaSP LIKE @ma";
                    ps.Add(new SqlParameter("@ma", "%" + txtMaSP.Text.Trim() + "%"));
                }
                if (txtTenSP.Text.Trim() != "")
                {
                    sql += " AND sp.TenSP LIKE @ten";
                    ps.Add(new SqlParameter("@ten", "%" + txtTenSP.Text.Trim() + "%"));
                }
                if (maLoaiLoc == null && cboLoai.SelectedValue != null)
                {
                    sql += " AND sp.MaLoai = @loai";
                    ps.Add(new SqlParameter("@loai", cboLoai.SelectedValue.ToString()));
                }
                decimal gia;
                if (txtDonGia.Text.Trim() != "" && DocGia(txtDonGia.Text, out gia))
                {
                    sql += " AND sp.DonGia = @gia";
                    ps.Add(new SqlParameter("@gia", gia));
                }
            }

            sql += " ORDER BY sp.MaSP";
            dgvSanPham.DataSource = dp.DocBang(sql, ps.ToArray());
            DinhDangLuoi();
        }
        private static bool DocGia(string s, out decimal gia)
        {
            return decimal.TryParse((s ?? "").Replace(".", "").Replace(",", "").Trim(), out gia);
        }
        private void DinhDangLuoi()
        {
            dgvSanPham.Columns["MaSP"].HeaderText = "Mã SP";
            dgvSanPham.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgvSanPham.Columns["TenLoai"].HeaderText = "Loại";
            dgvSanPham.Columns["DonGia"].HeaderText = "Đơn giá bán";
            dgvSanPham.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvSanPham.Columns["DonViTinh"].HeaderText = "ĐVT";
            dgvSanPham.Columns["TrangThai"].HeaderText = "Đang bán";
            dgvSanPham.Columns["MaLoai"].Visible = false;
            dgvSanPham.Columns["HinhAnh"].Visible = false;
            dgvSanPham.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        // CHỌN DÒNG TRÊN LƯỚI
        private void dgvSanPham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgvSanPham.Rows[e.RowIndex];

            dangThem = false;
            DatNut(false, true, true, false, true);

            txtMaSP.Text = Chuoi(r.Cells["MaSP"].Value);
            txtMaSP.ReadOnly = true;                           // không cho đổi mã khi đang sửa
            txtTenSP.Text = Chuoi(r.Cells["TenSP"].Value);
            cboLoai.SelectedValue = Chuoi(r.Cells["MaLoai"].Value);
            txtDonGia.Text = Convert.ToDecimal(r.Cells["DonGia"].Value).ToString("0");
            txtDonViTinh.Text = Chuoi(r.Cells["DonViTinh"].Value);

            anhDuocChon = null;
            tenAnhHienTai = Chuoi(r.Cells["HinhAnh"].Value);
            HienThiAnh(tenAnhHienTai == "" ? null : Path.Combine(imgFolder, tenAnhHienTai));
        }
        private static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }
        private void HienThiAnh(string path)
        {
            // Giải phóng bộ nhớ của ảnh cũ nếu đang hiển thị trên PictureBox
            if (picAnh.Image != null)
            {
                picAnh.Image.Dispose();
                picAnh.Image = null;
            }

            // Nếu đường dẫn trống hoặc file không tồn tại thì dừng
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            // Đọc ảnh qua MemoryStream để tránh khóa (lock) file ảnh trong ổ cứng
            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            using (var img = Image.FromStream(ms))
            {
                picAnh.Image = new Bitmap(img);
            }
        }
        private string LuuAnh(string maSP)
        {
            if (anhDuocChon == null) return tenAnhHienTai;     // giữ ảnh cũ

            Directory.CreateDirectory(imgFolder);
            string tenFile = maSP + Path.GetExtension(anhDuocChon).ToLower();
            string dich = Path.Combine(imgFolder, tenFile);
            if (Path.GetFullPath(anhDuocChon) != Path.GetFullPath(dich))
            {
                File.Copy(anhDuocChon, dich, true);            // ảnh đã đọc qua bộ nhớ nên ghi đè được
            }
            return tenFile;
        }

        private void btnAnh_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*";
                dlg.Title = "Chọn ảnh để hiển thị";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                anhDuocChon = dlg.FileName;
                HienThiAnh(anhDuocChon);
            }
        }
        private void ResetValue()
        {
            txtMaSP.Clear();
            txtMaSP.ReadOnly = false;
            txtTenSP.Clear();
            txtDonGia.Clear();
            txtDonViTinh.Clear();
            cboLoai.SelectedIndex = -1;
            if (maLoaiLoc != null) cboLoai.SelectedValue = maLoaiLoc;
            anhDuocChon = null;
            tenAnhHienTai = "";
            HienThiAnh(null);
            txtMaSP.Focus();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            dangThem = true;
            ResetValue();
            DatNut(true, false, false, true, true);
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            TaiDuLieu(true);
            if (dgvSanPham.Rows.Count == 0)
                MessageBox.Show("Không tìm thấy sản phẩm nào.");
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string ma = txtMaSP.Text.Trim();
            if (ma == "") return;

            try
            {
                DataTable daBan = dp.DocBang("SELECT COUNT(*) FROM ChiTietHoaDon WHERE MaSP = @ma",
                    new SqlParameter("@ma", ma));

                if (Convert.ToInt32(daBan.Rows[0][0]) > 0)
                {
                    // Đã bán rồi thì không xóa được, chỉ chuyển sang ngừng bán
                    if (MessageBox.Show("Sản phẩm đã có trong hóa đơn nên không thể xóa.\nChuyển sang trạng thái ngừng bán?",
                        "Xóa mặt hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                    dp.CapNhat("UPDATE SanPham SET TrangThai = 0 WHERE MaSP = @ma", new SqlParameter("@ma", ma));
                }
                else
                {
                    if (MessageBox.Show("Bạn có muốn xóa không?", "Xóa mặt hàng",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                    dp.CapNhat("DELETE FROM SanPham WHERE MaSP = @ma", new SqlParameter("@ma", ma));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                return;
            }

            dangThem = false;
            TaiDuLieu(false);
            ResetValue();
            DatNut(true, false, false, false, false);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string ma = txtMaSP.Text.Trim();

            if (ma == "")
            {
                MessageBox.Show("Bạn phải nhập mã sản phẩm!");
                txtMaSP.Focus(); return;
            }
            if (txtTenSP.Text.Trim() == "")
            {
                MessageBox.Show("Bạn phải nhập tên sản phẩm!");
                txtTenSP.Focus(); return;
            }
            if (cboLoai.SelectedValue == null)
            {
                MessageBox.Show("Bạn phải chọn loại sản phẩm!");
                cboLoai.Focus(); return;
            }
            decimal gia;
            if (!DocGia(txtDonGia.Text, out gia) || gia < 0)
            {
                MessageBox.Show("Đơn giá bán không hợp lệ!");
                txtDonGia.Focus(); return;
            }
            if (txtDonViTinh.Text.Trim() == "")
            {
                MessageBox.Show("Bạn phải nhập đơn vị tính!");
                txtDonViTinh.Focus(); return;
            }

            try
            {
                if (dangThem)
                {
                    DataTable kt = dp.DocBang("SELECT COUNT(*) FROM SanPham WHERE MaSP = @ma",
                        new SqlParameter("@ma", ma));
                    if (Convert.ToInt32(kt.Rows[0][0]) > 0)
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại. Mời bạn nhập mã khác!");
                        txtMaSP.Focus(); return;
                    }

                    dp.CapNhat(
                        "INSERT INTO SanPham (MaSP, TenSP, MaLoai, DonGia, DonViTinh, HinhAnh) " +
                        "VALUES (@ma, @ten, @loai, @gia, @dvt, @anh)",
                        ThamSo(ma, gia));
                    MessageBox.Show("Đã thêm sản phẩm.");
                }
                else
                {
                    dp.CapNhat(
                        "UPDATE SanPham SET TenSP=@ten, MaLoai=@loai, DonGia=@gia, DonViTinh=@dvt, " +
                        "HinhAnh=@anh WHERE MaSP=@ma",
                        ThamSo(ma, gia));
                    MessageBox.Show("Đã cập nhật sản phẩm.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                return;
            }

            // Load lại dữ liệu và đưa form về trạng thái ban đầu
            dangThem = false;
            TaiDuLieu(false);
            ResetValue();
            DatNut(true, false, false, false, false);
        }
        private SqlParameter[] ThamSo(string ma, decimal gia)
        {
            return new[]
            {
                new SqlParameter("@ma", ma),
                new SqlParameter("@ten", txtTenSP.Text.Trim()),
                new SqlParameter("@loai", cboLoai.SelectedValue.ToString()),
                new SqlParameter("@gia", gia),
                new SqlParameter("@dvt", txtDonViTinh.Text.Trim()),
                new SqlParameter("@anh", NullNeuRong(LuuAnh(ma)))
            };
        }
    }
}
