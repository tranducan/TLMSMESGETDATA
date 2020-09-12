using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.ClassMysql
{
    class ClassMySqlConnect
    {
        private const string MysqlConnStr = "Server={0};Port={1};Database={2};Uid={3};Pwd={4};";
        private static object serverIP;
        private static object serverPort;
        private static object database;
        private static object m_userName;
        private static object password;
        public static MySqlConnection StringConnection()
        {
            MySqlConnection mysqlConn = null;
            serverIP = "127.0.0.1";
            serverPort = "3306";
            database = "mes_interface";
            m_userName = "root";
            password = "0000";
            string connStr = string.Format(MysqlConnStr, serverIP, serverPort, database, m_userName, password);
            mysqlConn = new MySqlConnection(connStr);
            return mysqlConn;
        }
    }
}
