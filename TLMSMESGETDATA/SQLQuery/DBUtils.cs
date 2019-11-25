using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace TLMSMESGETDATA
{
    class DBUtils
    {

        public static SqlConnection GetDBConnection()
        {
            //Data Source=LONG;Initial Catalog=TEST;Integrated Security=True
            //string datasource = "Long";
            //string database = "TEST";
            //string username = "SQLUSER";
            //string password = "12345";

            //string datasource = @"DESKTOP-83OVHRC\SQLEXPRESS";
            //string database = "TLSOFT";
            //string username = "SQLUSER";
            //string password = "12345";

            string datasource = "172.16.0.12";
            string database = "ERPSOFT";
            string username = "ERPUSER";
            string password = "12345";



            return DBSQLServerUtils.GetDBConnection(datasource, database, username, password);
        }
        public static SqlConnection GetERPDBConnection()
        {
            //Data Source = LONG; Initial Catalog = TEST; Integrated Security = True
            string datasource = "172.16.0.11";
            string database = "TECHLINK";
            string username = "soft";
            string password = "techlink@!@#";

            //string datasource = "Long";
            //string database = "TECHLINK";
            //string username = "SQLUSER";
            //string password = "12345";
            //string datasource = @"DESKTOP-83OVHRC\SQLEXPRESS";
            //string database = "TECHLINK";
            //string username = "SQLUSER";
            //string password = "12345";


            return DBSQLServerUtils.GetERPDBConnection(datasource, database, username, password);
        }
        public static SqlConnection GetSFTDBConnection()
        {
            //Data Source = LONG; Initial Catalog = TEST; Integrated Security = True
            string datasource = "172.16.0.11";
            string database = "SFT_TECHLINK";
            string username = "soft";
            string password = "techlink@!@#";


            return DBSQLServerUtils.GetSFTDBConnection(datasource, database, username, password);
        }
        public static SqlConnection GetERPTargetBConnection()
        {
            //Data Source = LONG; Initial Catalog = TEST; Integrated Security = True
            string datasource = "172.16.0.11";
            string database = "SOT";
            string username = "sa";
            string password = "dsc@123";


            return DBSQLServerUtils.GetSFTDBConnection(datasource, database, username, password);
        }
    }
}
