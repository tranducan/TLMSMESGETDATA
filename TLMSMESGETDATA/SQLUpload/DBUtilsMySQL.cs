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


            string datasource = @"127.0.0.1";
            string port = "3306";
            string database = "mes_2";
            string username = "root";
            string password = "";


            return DbMySQLConnect.GetDBConnection(datasource, port, database, username, password);
        }
    }
}
