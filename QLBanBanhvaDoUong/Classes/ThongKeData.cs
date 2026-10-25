using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QLBanBanhvaDoUong.Classes
{
    // Các câu lệnh SELECT thống kê dùng riêng cho frmThongKe.
    // Đặt file này trong thư mục Classes, cạnh NhanVienData.cs.
    //
    // Quy ước: tuNgay và denNgay là hai ngày ĐỀU ĐƯỢC TÍNH (giờ trong ngày bị bỏ qua).
    // Bên trong, điều kiện là NgayLap >= tuNgay và NgayLap < denNgay + 1 ngày,
    // nên hóa đơn lập lúc 23:59 của ngày cuối vẫn được tính.
    internal class ThongKeData
    {
        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT
        // Doanh thu chỉ tính hóa đơn "Đã thanh toán".
        // =====================================================================
        private const string SqlLoai =
            "SELECT MaLoai, TenLoai FROM LoaiSanPham ORDER BY MaLoai";

        // Doanh thu theo ngày: Ky hiện dạng dd/MM/yyyy, sắp xếp theo ngày thật
        private const string SqlDoanhThuNgay =
            "SELECT CONVERT(VARCHAR(10), Ngay, 103) AS Ky, COUNT(*) AS SoHoaDon, SUM(TongTien) AS DoanhThu " +
            "FROM (SELECT CAST(NgayLap AS DATE) AS Ngay, TongTien FROM HoaDon " +
            "      WHERE TrangThai = N'Đã thanh toán' AND NgayLap >= @tu AND NgayLap < @den) x " +
            "GROUP BY Ngay ORDER BY Ngay";

        // Doanh thu theo tháng: Ky hiện dạng MM/yyyy
        private const string SqlDoanhThuThang =
            "SELECT RIGHT('0' + CAST(Thang AS VARCHAR(2)), 2) + '/' + CAST(Nam AS VARCHAR(4)) AS Ky, " +
            "       COUNT(*) AS SoHoaDon, SUM(TongTien) AS DoanhThu " +
            "FROM (SELECT YEAR(NgayLap) AS Nam, MONTH(NgayLap) AS Thang, TongTien FROM HoaDon " +
            "      WHERE TrangThai = N'Đã thanh toán' AND NgayLap >= @tu AND NgayLap < @den) x " +
            "GROUP BY Nam, Thang ORDER BY Nam, Thang";

        // Sản phẩm bán chạy: {0} là điều kiện lọc loại (có thể rỗng)
        private const string SqlBanChay =
            "SELECT TOP (@top) " +
            "       ROW_NUMBER() OVER (ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.ThanhTien) DESC) AS Hang, " +
            "       sp.TenSP, SUM(ct.SoLuong) AS SoLuong, SUM(ct.ThanhTien) AS DoanhThu " +
            "FROM ChiTietHoaDon ct " +
            "JOIN HoaDon h ON h.MaHD = ct.MaHD " +
            "JOIN SanPham sp ON sp.MaSP = ct.MaSP " +
            "WHERE h.TrangThai = N'Đã thanh toán' AND h.NgayLap >= @tu AND h.NgayLap < @den {0}" +
            "GROUP BY sp.MaSP, sp.TenSP " +
            "ORDER BY Hang";

        // Tiền thu được theo phương thức thanh toán (tính theo ngày trả tiền)
        private const string SqlPhuongThuc =
            "SELECT pt.TenPT, COUNT(*) AS SoLan, SUM(t.SoTien) AS TongThu " +
            "FROM ThanhToan t JOIN PhuongThucThanhToan pt ON pt.MaPT = t.MaPT " +
            "WHERE t.NgayTT >= @tu AND t.NgayTT < @den " +
            "GROUP BY pt.MaPT, pt.TenPT " +
            "ORDER BY SUM(t.SoTien) DESC";

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================

        // Danh sách loại, có thêm dòng "Tất cả" (mã rỗng) ở đầu
        public DataTable LayLoai()
        {
            DataTable dt = dp.DocBang(SqlLoai);
            DataRow r = dt.NewRow();
            r["MaLoai"] = "";
            r["TenLoai"] = "Tất cả";
            dt.Rows.InsertAt(r, 0);
            return dt;
        }

        // Cột trả về: Ky, SoHoaDon, DoanhThu
        public DataTable DoanhThu(DateTime tuNgay, DateTime denNgay, bool theoThang)
        {
            return dp.DocBang(theoThang ? SqlDoanhThuThang : SqlDoanhThuNgay,
                Tu(tuNgay), Den(denNgay));
        }

        // Cột trả về: Hang, TenSP, SoLuong, DoanhThu. maLoai rỗng = tất cả loại.
        public DataTable BanChay(DateTime tuNgay, DateTime denNgay, string maLoai, int top)
        {
            bool loc = !string.IsNullOrEmpty(maLoai);
            string sql = string.Format(SqlBanChay, loc ? "AND sp.MaLoai = @loai " : "");

            var ps = new List<SqlParameter>
            {
                Tu(tuNgay), Den(denNgay),
                new SqlParameter("@top", SqlDbType.Int) { Value = top }
            };
            if (loc) ps.Add(new SqlParameter("@loai", maLoai));

            return dp.DocBang(sql, ps.ToArray());
        }

        // Cột trả về: TenPT, SoLan, TongThu
        public DataTable PhuongThuc(DateTime tuNgay, DateTime denNgay)
        {
            return dp.DocBang(SqlPhuongThuc, Tu(tuNgay), Den(denNgay));
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================
        private static SqlParameter Tu(DateTime d)
        {
            return new SqlParameter("@tu", SqlDbType.DateTime) { Value = d.Date };
        }

        private static SqlParameter Den(DateTime d)
        {
            return new SqlParameter("@den", SqlDbType.DateTime) { Value = d.Date.AddDays(1) };
        }
    }
}