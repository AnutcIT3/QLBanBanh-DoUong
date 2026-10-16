using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBanBanhvaDoUong.Classes
{
    internal class Session
    {
        public static string MaNV { get; set; } = "";
        public static string HoTen { get; set; } = "";
        public static string ChucVu { get; set; } = "";
        public static void Clear()
        {
            MaNV = "";
            HoTen = "";
            ChucVu = "";
        }
    }
}
