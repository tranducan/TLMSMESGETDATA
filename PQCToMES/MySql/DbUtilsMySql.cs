using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQCToMES.MySql
{
    class DbUtilsMySql
    {
        public static MySqlConnection GetDBConnection()
        {

            string datasource = @"127.0.0.1";
            string port = "3306";
            string database = "mes_interface";
            string username = "root";
            string password = "0000";

            return DbConnection.GetDBConnection(datasource, port, database, username, password);
        }

        public static MySqlConnection GetDBConnection(string connectionString)
        {

            return DbConnection.GetDBConnection(connectionString);
        }
    }
}
