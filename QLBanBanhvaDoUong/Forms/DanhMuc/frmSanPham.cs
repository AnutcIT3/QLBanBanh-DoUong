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
        private static object NullNeuRong(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            dangThem = false;
            DatNut(false, true, false, true, true);
        }

        private void btnBoQua_Click(object sender, EventArgs e)
        {
            dangThem = false;
            ResetValue();
            TaiDuLieu(false);
            DatNut(true, false, false, false, false);
        }

        private void btnLoai_Click(object sender, EventArgs e)
        {
            if (!laQuanLy)
            {
                MessageBox.Show("Chỉ quản lý mới được thêm loại sản phẩm.");
                return;
            }

            string ten = HoiTen("Thêm loại sản phẩm", "Tên loại mới:");
            if (string.IsNullOrWhiteSpace(ten)) return;

            try
            {
                DataTable trung = dp.DocBang("SELECT COUNT(*) FROM LoaiSanPham WHERE TenLoai = @ten",
                    new SqlParameter("@ten", ten));
                if (Convert.ToInt32(trung.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Loại sản phẩm này đã tồn tại.");
                    return;
                }

                // Sinh mã mới dạng L05, L06, ...
                DataTable so = dp.DocBang(
                    "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(MaLoai, 2, 10) AS INT)), 0) + 1 FROM LoaiSanPham");
                string maMoi = "L" + Convert.ToInt32(so.Rows[0][0]).ToString("00");

                dp.CapNhat("INSERT INTO LoaiSanPham (MaLoai, TenLoai) VALUES (@ma, @ten)",
                    new SqlParameter("@ma", maMoi),
                    new SqlParameter("@ten", ten));

                fn.FillComBox(cboLoai, dp.DocBang(SqlLoai), "TenLoai", "MaLoai");
                cboLoai.SelectedValue = maMoi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private static string HoiTen(string tieuDe, string nhan)
        {
            using (var f = new Form())
            using (var lbl = new Label { Text = nhan, Left = 12, Top = 14, AutoSize = true })
            using (var txt = new TextBox { Left = 12, Top = 38, Width = 260 })
            using (var ok = new Button { Text = "OK", Left = 116, Top = 72, Width = 75, DialogResult = DialogResult.OK })
            using (var huy = new Button { Text = "Hủy", Left = 197, Top = 72, Width = 75, DialogResult = DialogResult.Cancel })
            {
                f.Text = tieuDe;
                f.ClientSize = new Size(290, 110);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.AcceptButton = ok;
                f.CancelButton = huy;
                f.Controls.AddRange(new Control[] { lbl, txt, ok, huy });
                return f.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.");
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Excel (*.xlsx)|*.xlsx";
                dlg.FileName = "DanhSachSanPham.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                Excel.Application exApp = null;
                Excel.Workbook exBook = null;
                try
                {
                    exApp = new Excel.Application();
                    exBook = exApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                    Excel.Worksheet exSheet = (Excel.Worksheet)exBook.Worksheets[1];
                    exSheet.Name = "DSSanPham";

                    // Tiêu đề
                    exSheet.Range["B2"].Value = "Danh sách sản phẩm";
                    exSheet.Range["B2"].Font.Size = 25;
                    exSheet.Range["B2"].Font.Name = "Times New Roman";
                    exSheet.Range["B2"].Font.Color = ColorTranslator.ToOle(Color.Red);

                    // Dòng tiêu đề cột
                    string[] tieuDeCot = { "Mã SP", "Tên sản phẩm", "Loại", "Đơn giá bán", "Đơn vị tính" };
                    for (int c = 0; c < tieuDeCot.Length; c++)
                        exSheet.Cells[4, c + 1] = tieuDeCot[c];
                    exSheet.Range["A4:E4"].Font.Size = 13;
                    exSheet.Range["A4:E4"].Font.Name = "Times New Roman";
                    exSheet.Range["A4:E4"].Font.Bold = true;

                    // Dữ liệu
                    int hang = 5;
                    foreach (DataGridViewRow r in dgvSanPham.Rows)
                    {
                        exSheet.Cells[hang, 1] = Chuoi(r.Cells["MaSP"].Value);
                        exSheet.Cells[hang, 2] = Chuoi(r.Cells["TenSP"].Value);
                        exSheet.Cells[hang, 3] = Chuoi(r.Cells["TenLoai"].Value);
                        exSheet.Cells[hang, 4] = Convert.ToDecimal(r.Cells["DonGia"].Value);
                        exSheet.Cells[hang, 5] = Chuoi(r.Cells["DonViTinh"].Value);
                        hang++;
                    }
                   
                    exSheet.Columns.AutoFit();

                    exBook.SaveAs(dlg.FileName);
                    MessageBox.Show("Đã xuất file Excel thành công.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không xuất được Excel: " + ex.Message);
                }
                finally
                {
                    if (exBook != null) exBook.Close(false);
                    if (exApp != null) exApp.Quit();
                }
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
