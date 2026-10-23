using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBanBanhvaDoUong.Classes
{
    internal class ThanhToanData
    {
        public const string ChuaThanhToan = "Chưa thanh toán";
        public const string DaThanhToan = "Đã thanh toán";
        public const string DaHuy = "Đã hủy";

        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT
        // =====================================================================
        private const string SqlHoaDon =
            "SELECT h.MaHD, h.NgayLap, h.TongTien, h.TrangThai, " +
            "       nv.HoTen AS TenNV, ISNULL(kh.HoTen, N'Khách lẻ') AS TenKH " +
            "FROM HoaDon h " +
            "JOIN NhanVien nv ON nv.MaNV = h.MaNV " +
            "LEFT JOIN KhachHang kh ON kh.MaKH = h.MaKH " +
            "WHERE h.MaHD = @hd";

        private const string SqlPhuongThuc =
            "SELECT MaPT, TenPT FROM PhuongThucThanhToan ORDER BY MaPT";

        private const string SqlThanhToan =
            "SELECT t.MaPT, pt.TenPT, t.SoTien, t.NgayTT, t.MaGiaoDich " +
            "FROM ThanhToan t JOIN PhuongThucThanhToan pt ON pt.MaPT = t.MaPT " +
            "WHERE t.MaHD = @hd ORDER BY t.NgayTT";

        private const string SqlTongDaTra =
            "SELECT ISNULL(SUM(SoTien), 0) FROM ThanhToan WHERE MaHD = @hd";

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================

        // Thông tin hóa đơn; trả về null nếu không có mã này
        public DataRow LayHoaDon(string maHD)
        {
            DataTable dt = dp.DocBang(SqlHoaDon, new SqlParameter("@hd", maHD));
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public DataTable LayPhuongThuc()
        {
            return dp.DocBang(SqlPhuongThuc);
        }

        // Các lần thanh toán đã ghi của hóa đơn
        public DataTable LayThanhToan(string maHD)
        {
            return dp.DocBang(SqlThanhToan, new SqlParameter("@hd", maHD));
        }

        public decimal TongDaTra(string maHD)
        {
            DataTable dt = dp.DocBang(SqlTongDaTra, new SqlParameter("@hd", maHD));
            return Convert.ToDecimal(dt.Rows[0][0]);
        }

        // =====================================================================
        // GHI THANH TOÁN (một giao dịch)
        // Khóa chính ThanhToan là (MaHD, MaPT): nếu phương thức này đã có trên hóa đơn
        // thì cộng dồn số tiền, chưa có thì thêm dòng mới.
        // Trả đủ thì hóa đơn tự chuyển sang "Đã thanh toán".
        // =====================================================================
        public void GhiThanhToan(string maHD, string maPT, decimal soTien, string maGiaoDich)
        {
            const string sql =
                "SET XACT_ABORT ON; BEGIN TRAN; " +
                "IF EXISTS (SELECT 1 FROM ThanhToan WHERE MaHD = @hd AND MaPT = @pt) " +
                "    UPDATE ThanhToan SET SoTien = SoTien + @st, NgayTT = GETDATE(), " +
                "           MaGiaoDich = ISNULL(@gd, MaGiaoDich) WHERE MaHD = @hd AND MaPT = @pt " +
                "ELSE " +
                "    INSERT INTO ThanhToan (MaHD, MaPT, SoTien, NgayTT, MaGiaoDich) " +
                "    VALUES (@hd, @pt, @st, GETDATE(), @gd); " +
                "UPDATE HoaDon SET TrangThai = N'Đã thanh toán' " +
                "WHERE MaHD = @hd AND TrangThai = N'Chưa thanh toán' " +
                "  AND (SELECT ISNULL(SUM(SoTien), 0) FROM ThanhToan WHERE MaHD = @hd) >= TongTien; " +
                "COMMIT TRAN;";

            var st = new SqlParameter("@st", SqlDbType.Decimal) { Precision = 14, Scale = 0, Value = soTien };
            var gd = new SqlParameter("@gd", SqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(maGiaoDich) ? (object)DBNull.Value : maGiaoDich.Trim()
            };

            dp.CapNhat(sql,
                new SqlParameter("@hd", maHD),
                new SqlParameter("@pt", maPT),
                st, gd);
        }

        // Hủy hóa đơn chưa trả đồng nào. Trả về false nếu đã có thanh toán.
        public bool HuyHoaDon(string maHD)
        {
            if (TongDaTra(maHD) > 0) return false;

            dp.CapNhat(
                "UPDATE HoaDon SET TrangThai = N'Đã hủy' WHERE MaHD = @hd AND TrangThai = N'Chưa thanh toán'",
                new SqlParameter("@hd", maHD));
            return true;
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================

        // "1.250.000" hoặc "1,250,000" -> 1250000
        public static bool DocTien(string s, out decimal tien)
        {
            return decimal.TryParse((s ?? "").Replace(".", "").Replace(",", "").Trim(), out tien);
        }

        public static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }
    }
}
