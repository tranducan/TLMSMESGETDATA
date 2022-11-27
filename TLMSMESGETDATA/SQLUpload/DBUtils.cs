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

        public static SqlConnection GetDBConnection(Model.SettingClass settingClass)
        {
            //string datasource = @"DESKTOP-M6N0IBR\SQLEXPRESS";
            //string database = "ERPSOFT";
            //string username = "dnmdev";
            //string password = "toluen";

            string datasource = "";
            string database = "";
            string username = "";
            string password = "";


            datasource = settingClass.OfflineServer == null ? "172.16.0.12" : settingClass.OfflineServer;
            database = "ERPSOFT";
            username = settingClass.userOffline == null ? "ERPUSER" : settingClass.userOffline;
            password = settingClass.password == null ? "12345" : settingClass.password;

            //string datasource = @"172.16.1.222\LOCALSQL";
            //string database = "MQCMES";
            //string username = "Automation";
            //string password = "12345";


            return DBSQLServerUtils.GetDBConnection(datasource, database, username, password);
        }


        public static SqlConnection GetERPDBConnection()
        {
            //Data Source = LONG; Initial Catalog = TEST; Integrated Security = True
            string datasource = "172.16.0.11";

            string database =  "TLVN2";
            string username = "soft";
            string password = "techlink@!@#";

            if (MainWindow.SettingClass.TechLinkDatabse != null)
                database = MainWindow.SettingClass.TechLinkDatabse;


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
       
        public static SqlConnection GetLocalPLCConnection(string _datasource, string  _database, string _user, string _pass)
        {
            //Data Source = LONG; Initial Catalog = TEST; Integrated Security = True
            string datasource = _datasource;
            string database =_database;
            string username = _user;
            string password = _pass;


            return DBSQLServerUtils.GetLocalPLCConnection(datasource, database, username, password);
        }
    }
}
