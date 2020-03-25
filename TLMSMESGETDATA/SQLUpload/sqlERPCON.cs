using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace TLMSMESGETDATA
{
    public class sqlERPCON
    {
        public SqlConnection conn = DBUtils.GetERPDBConnection(); //get from user database
             
        public string sqlExecuteScalarString(string sql)
        {
            String outstring;
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                outstring = cmd.ExecuteScalar().ToString();
                conn.Close();
                return outstring;
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                conn.Close();
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "sqlExecuteScalarString(string sql)", ex.Message);
                return String.Empty;
            }
        //    conn.Close();

        }
       
       
        public void sqlDataAdapterFillDatatable(string sql, ref DataTable dt)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                {
                    cmd.CommandText = sql;
                    cmd.Connection = conn;
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
             //   MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }
        public bool sqlExecuteNonQuery(string sql, bool result_message_show)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                int response = cmd.ExecuteNonQuery();
                if (response >= 1)
                {
                    if (result_message_show) { MessageBox.Show("Successful!", "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    conn.Close();
                    return true;
                }
                else
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce","");
                 //   MessageBox.Show("Not successful!", "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    conn.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Not successful!" + System.Environment.NewLine + ex.Message
                //                , "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
                conn.Close();
                return false;
            }
        }

      
    }
}
