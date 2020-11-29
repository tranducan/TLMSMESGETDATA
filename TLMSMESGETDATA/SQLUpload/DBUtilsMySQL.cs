using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace TLMSMESGETDATA.SQLUpload
{
    class DBUtilsMySQL
    {
        public static MySqlConnection GetDBConnection()
        {


            string datasource = @"172.16.0.22";
            string port = "3306";
            string database = "mes_2";
            string username = "guest";
            string password = "guest@123";


            return DbMySQLConnect.GetDBConnection(datasource, port, database, username, password);
        }
    }
}
