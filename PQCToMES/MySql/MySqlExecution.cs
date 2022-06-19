using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PQCToMES.MySql
{
    public class MySqlExecution
    {
        static string connectionString;
        public MySqlExecution(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public MySqlConnection conn = DbUtilsMySql.GetDBConnection(connectionString);
       
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
                    conn.Close();
                    return true;
                }
                else
                {
                    conn.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
        }
    }
}
