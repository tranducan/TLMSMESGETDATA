using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQCToMES.MySql
{
    class DbConnection
    {
        public static MySqlConnection
                GetDBConnection(string datasource, string port, string database, string username, string password)
        {
            string connString = @"Server = " + datasource + ";Port=" + port + ";Database= " + database + ";Uid=" + username + ";Pwd=" + password;
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
    }
}
