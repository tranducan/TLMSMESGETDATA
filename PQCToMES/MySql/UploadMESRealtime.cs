using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb;

namespace PQCToMES.MySql
{
    public class UploadMESRealtime
    {
        public static string connectionString = "";

        public UploadMESRealtime(string _connectionString)
        {
            connectionString = _connectionString;
        }
        
        public bool PushPQCDataToMESRealtime(PQCMesData pQCMesData)
        {
            try
            {
                StringBuilder command = new StringBuilder();
                command.Append(@"INSERT INTO t_mqc_realtime(

    serno,
    lot,
    model,
    site,
    factory,
    line,
    process,
    item,
    inspectdate,
    inspecttime,
    data,
    judge,
    status,
    remark,
    inspector
    )
    VALUES( ");
                command.Append("'" + pQCMesData.POCode + "',");
                command.Append("'" + pQCMesData.LotNumber + "',");
                command.Append("'" + pQCMesData.Model + "',");
                command.Append("'" + pQCMesData.Site + "',");
                command.Append("'" + "TechLink" + "',");
                command.Append("'" + pQCMesData.Line + "',");
                command.Append("'" + pQCMesData.Process + "',");
                command.Append("'" + pQCMesData.Attribute + "',");
                command.Append("'" + pQCMesData.InspectDateTime.ToString("yyyy-MM-dd") + "',");
                command.Append("'" + pQCMesData.InspectDateTime.ToString("HH:mm:ss") + "',");
                command.Append("'" + pQCMesData.Quantity + "',");
                command.Append("'" + "1" + "',");
                command.Append("'" + pQCMesData.Flag + "',");
                command.Append("'" + "" + "',");
                command.Append("'" + pQCMesData.Inspector + "'");
                command.Append(" ) ");
                MySqlExecution mySqlExecution = new MySqlExecution(connectionString);
                var result = mySqlExecution.sqlExecuteNonQuery(command.ToString(), false);
                return result;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
