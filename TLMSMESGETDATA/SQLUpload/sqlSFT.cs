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
   public class sqlSFT
    {

        public SqlConnection conn = DBUtils.GetSFTDBConnection(); //get from user database

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
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
               // MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                conn.Close();
                return String.Empty;
            }
            //    conn.Close();

        }
        public void getComboBoxData(string sql, ref ComboBox cmb)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                adapter.Dispose();
                cmd.Dispose();
                cmb.Items.Clear();

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    cmb.Items.Add(row[0].ToString());
                }
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
               // MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            conn.Close();
        }
        public void getComboBoxData(string sql, ref ComboBox cmb, ref ComboBox cmb2)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                adapter.Dispose();
                cmd.Dispose();
                cmb.Items.Clear();
                cmb2.Items.Clear();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (cmb.Items != null && cmb.Items.Contains(row[0].ToString()) == false)
                        cmb.Items.Add(row[0].ToString());
                    if (cmb2.Items != null && cmb2.Items.Contains(row[1].ToString()) == false)
                        cmb2.Items.Add(row[1].ToString());
                }
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
              //  MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            conn.Close();
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
            {//
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", ex.Message);
            //    MessageBox.Show(ex.Message, "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Database Responce", "");

                  //  MessageBox.Show("Not successful!", "Database Responce", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
