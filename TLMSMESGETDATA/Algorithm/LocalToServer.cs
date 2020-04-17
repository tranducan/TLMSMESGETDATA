using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TLMSMESGETDATA.Algorithm
{
  public  class LocalToServer
    {
        public DataTable GetDataTable(string lot, DateTime dtFrom, DateTime dtTo, Model.SettingClass settingClass)
        {
            DataTable dt = new DataTable();
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" select * from m_ERPMQC_REALTIME where 1=1 and item !='' and status ='' ");
                stringBuilder.Append(" and lot = '" + lot + "' ");
                stringBuilder.Append(" and cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '" + dtFrom.ToString("yyyyMMdd HH:mm:ss") + "'");
                stringBuilder.Append(" and  cast (inspectdate as datetime) + cast (inspecttime as datetime) <= '" + dtTo.ToString("yyyyMMdd HH:mm:ss") + "'");
                LocalPLCSql localPLCSql = new LocalPLCSql();
                localPLCSql.sqlDataAdapterFillDatatable(stringBuilder.ToString(), ref dt, settingClass);


            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "DataTable GetDataTable", ex.Message);
            }
            return dt;
        }
        public bool UpdateStatusLocalDatabase(string serno, string Status, Model.SettingClass settingClass)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" update m_ERPMQC_REALTIME set status ='" + Status + "' where serno ='" + serno + "' ");
                LocalPLCSql localPLCSql = new LocalPLCSql();
              return  localPLCSql.sqlExecuteNonQuery(stringBuilder.ToString(), false, settingClass);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "UpdateStatusLocalDatabase", ex.Message);
                return false;
            }
            
        }
        public bool InsertToFactoryDatabase(DataTable dataTable,Model.SettingClass settingClass)
        {
            try
            {
                int countInsertOK = 0;
                StringBuilder stringCommon = new StringBuilder();
                stringCommon.Append(" insert into m_ERPMQC_REALTIME ( ");
                
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    {
                        if (i < dataTable.Columns.Count - 1)
                            stringCommon.Append(dataTable.Columns[i].ColumnName + ",");
                        else stringCommon.Append(dataTable.Columns[i].ColumnName + ") values ( ");
                    }
                }
                for (int i = 0;  i< dataTable.Rows.Count; i++)
                {
                    StringBuilder strValues = new StringBuilder();
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                      string  valueCell = dataTable.Rows[i][j].ToString();
                        if (j < dataTable.Columns.Count - 1) strValues.Append(" '" + valueCell + "',");
                        else strValues.Append("'" + valueCell + "')");
                    }

                    string StrInsert = stringCommon.ToString() + strValues.ToString();
                    sqlCON sqlCON = new sqlCON();
                  var resultInsert =  sqlCON.sqlExecuteNonQuery(StrInsert, false);
                    if(resultInsert)
                    {
                        UpdateStatusLocalDatabase(dataTable.Rows[i][0].ToString(), "OK", settingClass);
                           countInsertOK++;

                    }
                }
                if(dataTable.Rows.Count > countInsertOK)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert data has problem: ", dataTable.Rows.Count.ToString() + ">" + countInsertOK.ToString());
                }
                else if(dataTable.Rows.Count < countInsertOK)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert data has problem: ", dataTable.Rows.Count.ToString() + "<" + countInsertOK.ToString());
                }
                else
                {
                    return true;
                }
            }
               
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToFactoryDatabase", ex.Message);
            }
            return false;
        }
        public bool UploadLocalServertoFactoryDB(string lot, DateTime dtFrom, DateTime dtTo, Model.SettingClass settingClass)
        {
            try
            {
                var dtLotUpload = GetDataTable(lot, dtFrom, dtTo, settingClass);
                if (dtLotUpload.Rows.Count > 0)
                {
                    var ResultUpload = InsertToFactoryDatabase(dtLotUpload, settingClass);
                    return ResultUpload;
                }
                else return true;
            }
            catch (Exception ex) 
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "UploadLocalServertoFactoryDB", ex.Message);
            }
            return false;
        }

    }
}
