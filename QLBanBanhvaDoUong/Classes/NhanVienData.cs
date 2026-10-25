using System;
using System.Data;
using System.Data.SqlClient;

namespace QLBanBanhvaDoUong.Classes
{
    // Các câu lệnh SELECT và hàm ghi dữ liệu dùng riêng cho frmNhanVien.
    // Đặt file này trong thư mục Classes, cạnh KhachHangData.cs.
    // YÊU CẦU: bảng NhanVien phải có cột TrangThai (xem lệnh ALTER TABLE trong phần hướng dẫn).
    internal class NhanVienData
    {
        public const string QuanLy = "Quản lý";

        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT (không lấy cột mật khẩu ra lưới)
        // =====================================================================
        private const string SqlNhanVien =
            "SELECT MaNV, HoTen, GioiTinh, NgaySinh, SDT, ChucVu, TrangThai FROM NhanVien " +
            "WHERE (MaNV LIKE @k OR HoTen LIKE @k OR SDT LIKE @k) ORDER BY MaNV";

        private const string SqlMaNVKeTiep =
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(MaNV, 3, 10) AS INT)), 0) + 1 FROM NhanVien";

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================
        public DataTable LayNhanVien(string tuKhoa)
        {
            return dp.DocBang(SqlNhanVien,
                new SqlParameter("@k", "%" + (tuKhoa ?? "").Trim() + "%"));
        }

        // Mã nhân viên kế tiếp dạng NV01, NV02, ...
        public string SinhMaNV()
        {
            DataTable dt = dp.DocBang(SqlMaNVKeTiep);
            return "NV" + Convert.ToInt32(dt.Rows[0][0]).ToString("00");
        }

        // Số điện thoại đã thuộc nhân viên khác chưa (bỏ qua chính nhân viên đang sửa)
        public bool TrungSDT(string sdt, string maNVBoQua)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return false;

            DataTable dt = dp.DocBang(
                "SELECT COUNT(*) FROM NhanVien WHERE SDT = @sdt AND MaNV <> @ma",
                new SqlParameter("@sdt", sdt.Trim()),
                new SqlParameter("@ma", maNVBoQua ?? ""));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        // Số quản lý đang làm việc, không tính nhân viên maNVBoQua
        public int SoQuanLyKhac(string maNVBoQua)
        {
            DataTable dt = dp.DocBang(
                "SELECT COUNT(*) FROM NhanVien WHERE ChucVu = N'Quản lý' AND TrangThai = 1 AND MaNV <> @ma",
                new SqlParameter("@ma", maNVBoQua ?? ""));
            return Convert.ToInt32(dt.Rows[0][0]);
        }

        // Nhân viên này hiện là quản lý đang làm việc?
        public bool LaQuanLyDangLam(string maNV)
        {
            DataTable dt = dp.DocBang(
                "SELECT COUNT(*) FROM NhanVien WHERE MaNV = @ma AND ChucVu = N'Quản lý' AND TrangThai = 1",
                new SqlParameter("@ma", maNV));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public int DemHoaDon(string maNV)
        {
            DataTable dt = dp.DocBang("SELECT COUNT(*) FROM HoaDon WHERE MaNV = @ma",
                new SqlParameter("@ma", maNV));
            return Convert.ToInt32(dt.Rows[0][0]);
        }

        // =====================================================================
        // HÀM GHI DỮ LIỆU
        // Cột TenDangNhap bắt buộc có giá trị và không trùng; vì form đăng nhập dùng mã nhân viên
        // nên ở đây gán TenDangNhap = MaNV.
        // =====================================================================
        public void Them(string maNV, string hoTen, string gioiTinh, DateTime ngaySinh,
                         string sdt, string chucVu, string matKhau, bool dangLam)
        {
            dp.CapNhat(
                "INSERT INTO NhanVien (MaNV, HoTen, GioiTinh, NgaySinh, SDT, ChucVu, TenDangNhap, MatKhau, TrangThai) " +
                "VALUES (@ma, @ten, @gt, @ns, @sdt, @cv, @ma, @mk, @tt)",
                new SqlParameter("@ma", maNV),
                new SqlParameter("@ten", hoTen.Trim()),
                new SqlParameter("@gt", gioiTinh),
                new SqlParameter("@ns", SqlDbType.Date) { Value = ngaySinh.Date },
                new SqlParameter("@sdt", NullNeuRong(sdt)),
                new SqlParameter("@cv", chucVu),
                new SqlParameter("@mk", matKhau),
                new SqlParameter("@tt", dangLam));
        }

        // matKhauMoi rỗng thì giữ nguyên mật khẩu cũ
        public void Sua(string maNV, string hoTen, string gioiTinh, DateTime ngaySinh,
                        string sdt, string chucVu, bool dangLam, string matKhauMoi)
        {
            bool doiMatKhau = !string.IsNullOrEmpty(matKhauMoi);
            string sql =
                "UPDATE NhanVien SET HoTen = @ten, GioiTinh = @gt, NgaySinh = @ns, SDT = @sdt, " +
                "ChucVu = @cv, TrangThai = @tt" + (doiMatKhau ? ", MatKhau = @mk" : "") +
                " WHERE MaNV = @ma";

            var ps = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@ma", maNV),
                new SqlParameter("@ten", hoTen.Trim()),
                new SqlParameter("@gt", gioiTinh),
                new SqlParameter("@ns", SqlDbType.Date) { Value = ngaySinh.Date },
                new SqlParameter("@sdt", NullNeuRong(sdt)),
                new SqlParameter("@cv", chucVu),
                new SqlParameter("@tt", dangLam)
            };
            if (doiMatKhau) ps.Add(new SqlParameter("@mk", matKhauMoi));

            dp.CapNhat(sql, ps.ToArray());
        }

        public void DatLaiMatKhau(string maNV, string matKhauMoi)
        {
            dp.CapNhat("UPDATE NhanVien SET MatKhau = @mk WHERE MaNV = @ma",
                new SqlParameter("@mk", matKhauMoi),
                new SqlParameter("@ma", maNV));
        }

        // Cho nghỉ việc (false) hoặc đi làm lại (true)
        public void DatTrangThai(string maNV, bool dangLam)
        {
            dp.CapNhat("UPDATE NhanVien SET TrangThai = @tt WHERE MaNV = @ma",
                new SqlParameter("@tt", dangLam),
                new SqlParameter("@ma", maNV));
        }

        // Chỉ xóa được nhân viên chưa lập hóa đơn nào (gọi DemHoaDon trước)
        public void Xoa(string maNV)
        {
            dp.CapNhat("DELETE FROM NhanVien WHERE MaNV = @ma", new SqlParameter("@ma", maNV));
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================
        public static object NullNeuRong(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
        }

        public static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }
    }
}