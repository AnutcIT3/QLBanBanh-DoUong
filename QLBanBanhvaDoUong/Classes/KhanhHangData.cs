using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace QLBanBanhvaDoUong.Classes
{
    // Các câu lệnh SELECT và hàm ghi dữ liệu dùng riêng cho frmKhachHang.
    // Đặt file này trong thư mục Classes, cạnh HoaDonData.cs.
    internal class KhachHangData
    {
        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT
        // =====================================================================
        // Tìm theo mã, họ tên hoặc số điện thoại
        private const string SqlKhachHang =
            "SELECT MaKH, HoTen, SDT, DiaChi, DiemTichLuy FROM KhachHang " +
            "WHERE (MaKH LIKE @k OR HoTen LIKE @k OR SDT LIKE @k) ORDER BY MaKH";

        // Các hóa đơn của một khách
        private const string SqlLichSu =
            "SELECT MaHD, NgayLap, TongTien, TrangThai FROM HoaDon " +
            "WHERE MaKH = @kh ORDER BY NgayLap DESC";

        private const string SqlMaKHKeTiep =
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(MaKH, 3, 10) AS INT)), 0) + 1 FROM KhachHang";

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================
        public DataTable LayKhachHang(string tuKhoa)
        {
            return dp.DocBang(SqlKhachHang,
                new SqlParameter("@k", "%" + (tuKhoa ?? "").Trim() + "%"));
        }

        public DataTable LayLichSuMua(string maKH)
        {
            return dp.DocBang(SqlLichSu, new SqlParameter("@kh", maKH));
        }

        // Mã khách kế tiếp dạng KH01, KH02, ...
        public string SinhMaKH()
        {
            DataTable dt = dp.DocBang(SqlMaKHKeTiep);
            return "KH" + Convert.ToInt32(dt.Rows[0][0]).ToString("00");
        }

        // Số điện thoại này đã thuộc về khách khác chưa (bỏ qua chính khách đang sửa)
        public bool TrungSDT(string sdt, string maKHBoQua)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return false;

            DataTable dt = dp.DocBang(
                "SELECT COUNT(*) FROM KhachHang WHERE SDT = @sdt AND MaKH <> @ma",
                new SqlParameter("@sdt", sdt.Trim()),
                new SqlParameter("@ma", maKHBoQua ?? ""));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public int DemHoaDon(string maKH)
        {
            DataTable dt = dp.DocBang("SELECT COUNT(*) FROM HoaDon WHERE MaKH = @ma",
                new SqlParameter("@ma", maKH));
            return Convert.ToInt32(dt.Rows[0][0]);
        }

        // =====================================================================
        // HÀM GHI DỮ LIỆU
        // =====================================================================
        public void Them(string maKH, string hoTen, string sdt, string diaChi)
        {
            dp.CapNhat(
                "INSERT INTO KhachHang (MaKH, HoTen, SDT, DiaChi) VALUES (@ma, @ten, @sdt, @dc)",
                ThamSo(maKH, hoTen, sdt, diaChi));
        }

        public void Sua(string maKH, string hoTen, string sdt, string diaChi)
        {
            dp.CapNhat(
                "UPDATE KhachHang SET HoTen = @ten, SDT = @sdt, DiaChi = @dc WHERE MaKH = @ma",
                ThamSo(maKH, hoTen, sdt, diaChi));
        }

        // Chỉ xóa được khách chưa có hóa đơn (gọi DemHoaDon trước)
        public void Xoa(string maKH)
        {
            dp.CapNhat("DELETE FROM KhachHang WHERE MaKH = @ma", new SqlParameter("@ma", maKH));
        }

        private static SqlParameter[] ThamSo(string maKH, string hoTen, string sdt, string diaChi)
        {
            return new[]
            {
                new SqlParameter("@ma", maKH),
                new SqlParameter("@ten", hoTen.Trim()),
                new SqlParameter("@sdt", NullNeuRong(sdt)),
                new SqlParameter("@dc", NullNeuRong(diaChi))
            };
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================
        public static object NullNeuRong(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
        }

        // Để trống hoặc 9 đến 11 chữ số
        public static bool SDTHopLe(string sdt)
        {
            return string.IsNullOrWhiteSpace(sdt) || Regex.IsMatch(sdt.Trim(), @"^\d{9,11}$");
        }

        public static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }
    }
}