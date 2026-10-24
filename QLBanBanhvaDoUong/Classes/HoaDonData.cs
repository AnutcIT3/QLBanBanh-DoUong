using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBanBanhvaDoUong.Classes
{
    internal class HoaDonData
    {
        private readonly DataProcesser dp = new DataProcesser();

        // =====================================================================
        // CÁC CÂU LỆNH SELECT
        // =====================================================================
        private const string SqlDanhSachMa =
            "SELECT MaHD FROM HoaDon ORDER BY NgayLap DESC, MaHD DESC";

        private const string SqlMaMoiNhat =
            "SELECT TOP 1 MaHD FROM HoaDon ORDER BY NgayLap DESC, MaHD DESC";

        // Thông tin chung của một hóa đơn (khách lẻ thì tên là "Khách lẻ", địa chỉ và SĐT để trống)
        private const string SqlHoaDon =
            "SELECT h.MaHD, h.NgayLap, h.MaNV, nv.HoTen AS TenNV, h.MaKH, " +
            "       ISNULL(kh.HoTen, N'Khách lẻ') AS TenKH, kh.DiaChi, kh.SDT, " +
            "       h.TongTien, h.TrangThai " +
            "FROM HoaDon h " +
            "JOIN NhanVien nv ON nv.MaNV = h.MaNV " +
            "LEFT JOIN KhachHang kh ON kh.MaKH = h.MaKH " +
            "WHERE h.MaHD = @hd";

        // Các món trong hóa đơn
        private const string SqlChiTiet =
            "SELECT ct.MaSP, sp.TenSP, ct.SoLuong, ct.DonGia, ct.MucDuong, ct.MucDa, ct.ThanhTien " +
            "FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP " +
            "WHERE ct.MaHD = @hd ORDER BY sp.TenSP";

        // =====================================================================
        // HÀM ĐỌC DỮ LIỆU
        // =====================================================================

        // Danh sách mã hóa đơn, mới nhất ở đầu
        public DataTable LayDanhSachMaHD()
        {
            return dp.DocBang(SqlDanhSachMa);
        }

        // Mã hóa đơn mới nhất, rỗng nếu chưa có hóa đơn nào
        public string MaHDMoiNhat()
        {
            DataTable dt = dp.DocBang(SqlMaMoiNhat);
            return dt.Rows.Count == 0 ? "" : dt.Rows[0][0].ToString();
        }

        // Thông tin chung; trả về null nếu không có mã này
        public DataRow LayHoaDon(string maHD)
        {
            DataTable dt = dp.DocBang(SqlHoaDon, new SqlParameter("@hd", maHD));
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public DataTable LayChiTiet(string maHD)
        {
            return dp.DocBang(SqlChiTiet, new SqlParameter("@hd", maHD));
        }

        // =====================================================================
        // HÀM PHỤ
        // =====================================================================
        public static string Chuoi(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v.ToString();
        }

        // 50 -> "50%", NULL -> "" (dùng cho mức đường và đá của bánh)
        public static string PhanTram(object v)
        {
            return (v == null || v == DBNull.Value) ? "" : v + "%";
        }

        private static readonly string[] chuSo =
            { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

        private static readonly string[] donViNhom =
            { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ", "tỷ tỷ" };

        // 1250000 -> "Một triệu hai trăm năm mươi nghìn đồng"
        public static string DocSoThanhChu(decimal soTien)
        {
            long n = (long)Math.Round(soTien, MidpointRounding.AwayFromZero);
            if (n == 0) return "Không đồng";

            bool am = n < 0;
            if (am) n = -n;

            // Tách thành các nhóm ba chữ số, nhóm thấp nhất ở đầu danh sách
            var nhom = new List<int>();
            while (n > 0)
            {
                nhom.Add((int)(n % 1000));
                n /= 1000;
            }

            var kq = new List<string>();
            for (int i = nhom.Count - 1; i >= 0; i--)
            {
                if (nhom[i] == 0) continue;

                // Nhóm đứng sau một nhóm lớn hơn thì đọc đủ "không trăm", "lẻ"
                bool docDayDu = i < nhom.Count - 1;
                string chu = DocBaChuSo(nhom[i], docDayDu);
                if (donViNhom[i] != "") chu += " " + donViNhom[i];
                kq.Add(chu);
            }

            string s = string.Join(" ", kq);
            if (am) s = "âm " + s;
            return char.ToUpper(s[0]) + s.Substring(1) + " đồng";
        }

        private static string DocBaChuSo(int g, bool docDayDu)
        {
            int tram = g / 100, chuc = g % 100 / 10, dv = g % 10;
            var p = new List<string>();

            if (tram > 0 || docDayDu)
                p.Add(chuSo[tram] + " trăm");

            if (chuc == 0)
            {
                if (dv > 0)
                {
                    if (tram > 0 || docDayDu) p.Add("lẻ");
                    p.Add(chuSo[dv]);
                }
            }
            else if (chuc == 1)
            {
                p.Add("mười");
                if (dv == 5) p.Add("lăm");
                else if (dv > 0) p.Add(chuSo[dv]);
            }
            else
            {
                p.Add(chuSo[chuc] + " mươi");
                if (dv == 1) p.Add("mốt");
                else if (dv == 4) p.Add("tư");
                else if (dv == 5) p.Add("lăm");
                else if (dv > 0) p.Add(chuSo[dv]);
            }

            return string.Join(" ", p);
        }
    }
}
