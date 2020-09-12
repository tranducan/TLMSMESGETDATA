using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace TLMSMESGETDATA.SQLUpload
{
    class MysqlMES
    {
        public MySqlConnection conn = DBUtilsMySQL.GetDBConnection();

        public string sqlExecuteScalarString(string sql)
        {

            String outstring;
            try
            {
               MySqlCommand cmd = new MySqlCommand(sql, conn);
                conn.Open();
                outstring = cmd.ExecuteScalar().ToString();
                conn.Close();
                return outstring;
            }
            catch (Exception ex)
            {

                //   MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
                return String.Empty;
            }


        }

        public void sqlDataAdapterFillDatatable(string sql, ref DataTable dt)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
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
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                int response = cmd.ExecuteNonQuery();
                if (response >= 1)
                {
                    if (result_message_show) { MessageBox.Show("Successful!", "Database Responce"); }
                    conn.Close();
                    return true;
                }
                else
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", "");
                    // MessageBox.Show("Not successful!", "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
