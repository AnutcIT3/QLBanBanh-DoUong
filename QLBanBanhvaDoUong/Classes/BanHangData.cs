using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBanBanhvaDoUong.Classes
{
    internal class BanHangData
    {
        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT (sửa câu lệnh ở đây, form không phải sửa)
        // =====================================================================
        private const string SqlLoai =
            "SELECT MaLoai, TenLoai FROM LoaiSanPham ORDER BY MaLoai";

        private const string SqlKhachHang =
            "SELECT MaKH, HoTen + ISNULL(N' - ' + SDT, N'') AS TenHienThi " +
            "FROM KhachHang ORDER BY HoTen";

        // Chỉ lấy sản phẩm đang bán; điều kiện lọc được nối thêm trong LaySanPham
        private const string SqlSanPham =
            "SELECT sp.MaSP, sp.TenSP, lsp.TenLoai, sp.DonGia, sp.DonViTinh, sp.MaLoai, sp.HinhAnh " +
            "FROM SanPham sp JOIN LoaiSanPham lsp ON lsp.MaLoai = sp.MaLoai " +
            "WHERE sp.TrangThai = 1";

        private const string SqlMaHDKeTiep =
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(MaHD, 3, 10) AS INT)), 0) + 1 FROM HoaDon";

        // Mã loại là đồ uống, có chọn mức đường và đá (Trà = L02, Cà phê = L03).
        // Nếu thêm loại đồ uống mới thì thêm mã của nó vào đây.
        private static readonly string[] loaiDoUong = { "L02", "L03" };

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================

        // Danh sách loại, có thêm dòng "Tất cả" (mã rỗng) ở đầu
        public DataTable LayLoai()
        {
            DataTable dt = dp.DocBang(SqlLoai);
            ThemDongDau(dt, "MaLoai", "", "TenLoai", "Tất cả");
            return dt;
        }

        // Danh sách khách hàng, có thêm dòng "Khách lẻ" (mã rỗng) ở đầu
        public DataTable LayKhachHang()
        {
            DataTable dt = dp.DocBang(SqlKhachHang);
            ThemDongDau(dt, "MaKH", "", "TenHienThi", "Khách lẻ");
            return dt;
        }

        // Sản phẩm đang bán, lọc theo loại (rỗng = tất cả) và từ khóa (mã hoặc tên)
        public DataTable LaySanPham(string maLoai, string tuKhoa)
        {
            string sql = SqlSanPham;
            var ps = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(maLoai))
            {
                sql += " AND sp.MaLoai = @loai";
                ps.Add(new SqlParameter("@loai", maLoai));
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                sql += " AND (sp.MaSP LIKE @k OR sp.TenSP LIKE @k)";
                ps.Add(new SqlParameter("@k", "%" + tuKhoa.Trim() + "%"));
            }
            sql += " ORDER BY sp.TenSP";

            return dp.DocBang(sql, ps.ToArray());
        }

        // Mã hóa đơn kế tiếp dạng HD001, HD002, ...
        public string SinhMaHD()
        {
            DataTable dt = dp.DocBang(SqlMaHDKeTiep);
            return "HD" + Convert.ToInt32(dt.Rows[0][0]).ToString("000");
        }

        // =====================================================================
        // LƯU HÓA ĐƠN (một giao dịch: hóa đơn và các dòng chi tiết cùng lưu hoặc cùng không)
        // maKH rỗng = khách lẻ (lưu NULL). Trigger trong CSDL tự tính TongTien.
        // =====================================================================
        public void LuuHoaDon(string maHD, string maNV, string maKH, DataTable gio)
        {
            var sb = new StringBuilder();
            var ps = new List<SqlParameter>();

            sb.Append("SET XACT_ABORT ON; BEGIN TRAN; ");
            sb.Append("INSERT INTO HoaDon (MaHD, NgayLap, MaNV, MaKH, TrangThai) " +
                      "VALUES (@hd, GETDATE(), @nv, @kh, N'Chưa thanh toán'); ");
            ps.Add(new SqlParameter("@hd", maHD));
            ps.Add(new SqlParameter("@nv", maNV));
            ps.Add(new SqlParameter("@kh", string.IsNullOrEmpty(maKH) ? (object)DBNull.Value : maKH));

            for (int i = 0; i < gio.Rows.Count; i++)
            {
                DataRow r = gio.Rows[i];
                sb.AppendFormat(
                    "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia, MucDuong, MucDa) " +
                    "VALUES (@hd, @sp{0}, @sl{0}, @gia{0}, @duong{0}, @da{0}); ", i);
                ps.Add(new SqlParameter("@sp" + i, r["MaSP"].ToString()));
                ps.Add(new SqlParameter("@sl" + i, (int)r["SoLuong"]));
                ps.Add(new SqlParameter("@gia" + i, (decimal)r["DonGia"]));
                ps.Add(new SqlParameter("@duong" + i, SqlDbType.TinyInt) { Value = r["MucDuong"] });
                ps.Add(new SqlParameter("@da" + i, SqlDbType.TinyInt) { Value = r["MucDa"] });
            }
            sb.Append("COMMIT TRAN;");

            dp.CapNhat(sb.ToString(), ps.ToArray());
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================

        // Tạo bảng giỏ hàng rỗng (mỗi sản phẩm chỉ một dòng)
        public static DataTable TaoGioHang()
        {
            var gio = new DataTable();
            gio.Columns.Add("MaSP", typeof(string));
            gio.Columns.Add("TenSP", typeof(string));
            gio.Columns.Add("SoLuong", typeof(int));
            gio.Columns.Add("DonGia", typeof(decimal));
            gio.Columns.Add("MucDuong", typeof(int));
            gio.Columns.Add("MucDa", typeof(int));
            gio.Columns.Add("ThanhTien", typeof(decimal), "SoLuong * DonGia");
            //gio.PrimaryKey = new[] { gio.Columns["MaSP"] };
            return gio;
        }

        // Tổng tiền của giỏ hàng
        public static decimal TinhTong(DataTable gio)
        {
            object tong = gio.Compute("SUM(ThanhTien)", "");
            return tong == DBNull.Value ? 0 : Convert.ToDecimal(tong);
        }

        // Loại này có chọn mức đường và đá không
        public static bool LaDoUong(string maLoai)
        {
            return Array.IndexOf(loaiDoUong, maLoai) >= 0;
        }

        // "50%" -> 50
        public static int DocPhanTram(string s)
        {
            int n;
            return int.TryParse((s ?? "").Replace("%", "").Trim(), out n) ? n : 100;
        }

        // Giá trị ô lưới -> chuỗi, NULL thành chuỗi rỗng
        public static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }

        private static void ThemDongDau(DataTable dt, string cotMa, string ma, string cotTen, string ten)
        {
            DataRow r = dt.NewRow();
            r[cotMa] = ma;
            r[cotTen] = ten;
            dt.Rows.InsertAt(r, 0);
        }
    }
}
