 using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBanBanhvaDoUong.Classes
{
    internal class DataProcesser
    {
        string strConnect = "Data Source=.\\SQLEXPRESS;Initial Catalog=QLBanBanhDoUong;Integrated Security=True;";
        SqlConnection conn = null;

        //Hàm mở kết nối
        private void KetNoiCSDL()
        {
            conn = new SqlConnection(strConnect);
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        private void DongKetNoi()
        {
            if (conn.State != ConnectionState.Closed)
                conn.Close();
            conn.Dispose();
        }

        //Thực thi câu lệnh select 
        public DataTable DocBang(string sql, params SqlParameter[] ps)
        {
            DataTable dt = new DataTable();
            try
            {
                KetNoiCSDL();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                if (ps != null) da.SelectCommand.Parameters.AddRange(ps);
                da.Fill(dt);
            }
            finally
            {
                DongKetNoi();
            }
            return dt;
        }

        public void CapNhat(string sql, params SqlParameter[] ps)
        {
            try
            {
                KetNoiCSDL();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (ps != null) cmd.Parameters.AddRange(ps);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                DongKetNoi();
            }
        }

    }
}
