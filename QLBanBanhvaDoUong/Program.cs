using QLBanBanhvaDoUong.Fomrs.HeThong;
using QLBanBanhvaDoUong.Forms.HeThong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLBanBanhvaDoUong
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var login = new frmDangNhap())
            {
                if (login.ShowDialog() != DialogResult.OK) return;   // đóng hoặc bấm Thoát thì thoát chương trình
            }
            Application.Run(new frmMain());
        }
    }
}
