using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLMSMESGETDATA.PLC2;
using TLMSMESGETDATA.Algorithm;



namespace TLMSMESGETDATA.SQLUpload
{
  public   class Upload2Mes
    {
        public  bool IsSendData2MES(DataMQC dataMQC, string line)
        {
            try
            {
                if (dataMQC.Good_Products_Total > 0)
                    InsertRowDataMQC(dataMQC, dataMQC.Good_Products_Total.ToString(), line, "OUTPUT");
                for (int i = 1; i < 39; i++)
                {
                    if (dataMQC.NG_Products_NG_[i - 1] > 0)
                        InsertRowDataMQC(dataMQC, dataMQC.NG_Products_NG_[i - 1].ToString(), line, "NG" + i.ToString());

                }

                for (int i = 1; i < 39; i++)
                {
                    if (dataMQC.RW_Products_NG_[i - 1] > 0)
                        InsertRowDataMQC(dataMQC, dataMQC.RW_Products_NG_[i - 1].ToString(), line, "RW" + i.ToString());
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "IsSendData2MES(DataMQC dataMQC, string line, string IP)", ex.Message);
            }
          
            return false;
        }
        public  bool InsertRowDataMQC (DataMQC dataMQC,string data, string line,string item)
        {
            try
            {
                string datetimeserno = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serno = dataMQC.strQRMES;
                QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(dataMQC.strQRMES);
                QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat(dataMQC.strQRID);
                string site = "B01";
                string factory = "TECHLINK";
                string process = "MQC";

                string status = "";
                string date_ = DateTime.Now.ToString("yyyy-MM-dd");
                string time_ = DateTime.Now.ToString("HH:mm:ss");
                string sqlQuerry = "";
                string Remark= "";
                if (item == "OUTPUT")
                    Remark = "OP";
                else if(item.Contains("NG"))
                    Remark = "NG";
                else if (item.Contains("RW"))
                    Remark = "RW";
                else if (item.Contains("Reset"))
                    Remark = "Reset";
                // string model = GetModelFromLot(lot,setting);
                string model = qRMQC_MES.Product;// lay model tren server, neu ko lay dc thi lay o local
               
                sqlQuerry += "insert into t_mqc_realtime (serno, lot, model, site, factory, line, process,item,inspectdate,inspecttime, data, judge,status,remark,inspector ) values( '";
                sqlQuerry += serno + "', '" + qRMQC_MES.PO + "', '" + model + "', '" + site + "', '" + factory + "', '" + line +
               "', '" + process + "', '" + item + "', '" + date_ + "', '" + time_ + "', '" + data + "', '" + "" + "', '" + status + "', '" + Remark + "' , '"+ qRIDMES.ID + "' )";
                MysqlMES localPLC = new MysqlMES();
                return localPLC.sqlExecuteNonQuery(sqlQuerry, false);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }
         
        }
    }
}
