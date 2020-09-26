using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLMSMESGETDATA.Algorithm;
using TLMSMESGETDATA.Model;

namespace TLMSMESGETDATA.SQLUpload
{
 public   class Upload2MESMySQL
    {
        public bool InsertRowMQVariables(Model.MQCVariable mQCVariable, string data, string line, string item)
        {
            try
            {
                string datetimeserno = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serno = mQCVariable.QRMES;
                QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(mQCVariable.QRMES);
                QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat(mQCVariable.QRID);
                string site = "B01";
                string factory = "TECHLINK";
                string process = "MQC";

                string status = "";
                string date_ = DateTime.Now.ToString("yyyy-MM-dd");
                string time_ = DateTime.Now.ToString("HH:mm:ss");
                string sqlQuerry = "";
                string Remark = "";
                if (item == "OUTPUT")
                    Remark = "OP";
                else if (item.Contains("NG"))
                    Remark = "NG";
                else if (item.Contains("RW"))
                    Remark = "RW";
                else if (item.Contains("Reset"))
                    Remark = "Reset";
                // string model = GetModelFromLot(lot,setting);
                string model = qRMQC_MES.Product;// lay model tren server, neu ko lay dc thi lay o local

                sqlQuerry += "insert into t_mqc_realtime (serno, lot, model, site, factory, line, process,item,inspectdate,inspecttime, data, judge,status,remark,inspector ) values( '";
                sqlQuerry += serno + "', '" + qRMQC_MES.PO + "', '" + model + "', '" + site + "', '" + factory + "', '" + line +
               "', '" + process + "', '" + item + "', '" + date_ + "', '" + time_ + "', '" + data + "', '" + "" + "', '" + status + "', '" + Remark + "' , '" + qRIDMES.ID + "' )";
                MysqlMES mysqlMES = new MysqlMES();
                return mysqlMES.sqlExecuteNonQuery(sqlQuerry, false);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }

        }
    
    
    
    }
}
