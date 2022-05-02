using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb;
using EFTechlinkMesDb.Model;

namespace PQCToMES.MySql
{
    public class Upload2MESMySql
    {
        public bool PushPQCDataToMESSql(PQCMesData pQCMesData)
        {
            try
            {
                StringBuilder command = new StringBuilder();
                command.Append(@"INSERT INTO processhistory_pqcmesdata(

    POCode,
    LotNumber,
    Model,
    Site,
    Line,
    PROCESS,
    ATTRIBUTE,
    AttributeType,
    Quantity,
    Flag,
    inspector,
    InspectDateTime,
    LastTimeModified
    )
    VALUES( ");

                command.Append("'" + pQCMesData.POCode + "',");
                command.Append("'" + pQCMesData.LotNumber + "',");
                command.Append("'" + pQCMesData.Model + "',");
                command.Append("'" + pQCMesData.Site + "',");
                command.Append("'" + pQCMesData.Line + "',");
                command.Append("'" + pQCMesData.Process + "',");
                command.Append("'" + pQCMesData.Attribute + "',");
                command.Append("'" + pQCMesData.AttributeType + "',");
                command.Append("'" + pQCMesData.Quantity + "',");
                command.Append("'" + pQCMesData.Flag + "',");
                command.Append("'" + pQCMesData.Inspector + "',");
                command.Append("'" + pQCMesData.InspectDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "',");
                command.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                command.Append(" ) ");

                MySqlExecution mySqlExecution = new MySqlExecution();
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
