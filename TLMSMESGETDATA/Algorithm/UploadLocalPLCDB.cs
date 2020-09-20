using System;
using System.Text;
using TLMSMESGETDATA.PLC2;


namespace TLMSMESGETDATA.Algorithm
{
    class UploadLocalPLCDB
    {
      public bool InsertMQCUpdateRealtime(DataMQC data, string Line, bool Isreset, Model.SettingClass setting)
        {
            try
            {
                if (Isreset)
                {
                    if (data.PLC_Barcode.Length == 26)
                    {
                        if(data.Good_Products_Total > 0)
                        InsertToMQC_Realtime(data.PLC_Barcode, Line, "OUTPUT", data.Good_Products_Total.ToString(), "OP", 1,setting);

                        for (int i = 1; i < 39; i++)
                        {
                            if (data.NG_Products_NG_[i - 1] > 0)
                                InsertToMQC_Realtime(data.PLC_Barcode, Line, "NG" + i.ToString(), data.NG_Products_NG_[i - 1].ToString(), "NG", 1,setting);
                        }



                        for (int i = 1; i < 39; i++)
                        {
                            if (data.RW_Products_NG_[i - 1] > 0)
                                InsertToMQC_Realtime(data.PLC_Barcode, Line, "RW" + i.ToString(), data.RW_Products_NG_[i - 1].ToString(), "RW", 1,setting);
                        }

                    }
                    else
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode wrong formart ", data.PLC_Barcode);
                    }
                }
                else
                {
                    if (data.PLC_Barcode.Length == 26)
                    {
                        if (data.Good_Products_Total > 0)
                            InsertToMQC_Realtime(data.PLC_Barcode, Line, "OUTPUT", data.Good_Products_Total.ToString(), "OP", 0,setting);
                        if (data.NG_Products_Total > 0)
                        {
                            for (int i = 1; i < 39; i++)
                            {
                                if (data.NG_Products_NG_[i - 1] > 0)
                                    InsertToMQC_Realtime(data.PLC_Barcode, Line, "NG" + i.ToString(), data.NG_Products_NG_[i - 1].ToString(), "NG", 0,setting);
                            }
                        }
                        if (data.RW_Products_Total > 0)
                        {
                            for (int i = 1; i < 39; i++)
                            {
                                if (data.RW_Products_NG_[i - 1] > 0)
                                    InsertToMQC_Realtime(data.PLC_Barcode, Line, "RW" + i.ToString(), data.RW_Products_NG_[i - 1].ToString(), "RW", 0,setting);
                            }
                        }
                    }
                    else
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode wrong formart ", data.PLC_Barcode);
                    }
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertMQCUpdateRealtime (DataMQC data, string Line, bool Isreset)", ex.Message);

                return false;
            }

            return true;

        }

        public bool InsertMQCMESUpdateRealtime(Model.MQCVariable mQCChanged,string TypeChange, string Line, Model.SettingClass setting)
        {
            try
            {
                if(TypeChange == "OPQTY")
                {
                    InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "OUTPUT", mQCChanged.ListMQCQty[0].ToString(), "OP", setting);
                }
                if(TypeChange == "NGQTY")
                {
                    for (int i = 0; i < mQCChanged.ListNG38.Count; i++)
                    {
                        if(mQCChanged.ListNG38[i] > 0 )
                        InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "NG"+(i+1).ToString(), mQCChanged.ListNG38[i].ToString(), "NG", setting);
                    }
                  
                }
                if (TypeChange == "RWQTY")
                {
                    for (int i = 0; i < mQCChanged.ListRW38.Count; i++)
                    {
                        if (mQCChanged.ListRW38[i] > 0)
                            InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "RW" + (i + 1).ToString(), mQCChanged.ListRW38[i].ToString(), "RW", setting);
                    }

                }
                if (TypeChange == "Reset")
                {
                    InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "Reset", "", "Reset", setting);

                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertMQCUpdateRealtime (DataMQC data, string Line, bool Isreset)", ex.Message);

                return false;
            }

            return true;

        }
        public string GetModelFromLot(string lot, Model.SettingClass setting)
        {
            string model = "";
            try
            {
                
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("select distinct isnull(model,'') from m_ERPMQC_REALTIME where 1=1 ");
                stringBuilder.Append(" and lot = '" + lot + "'");

                
                LocalPLCSql sqlERPCON = new LocalPLCSql();
                model = sqlERPCON.sqlExecuteScalarString(stringBuilder.ToString(), setting);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "GetModelFromLot (string lot)", ex.Message);

                return "";
            }
            return model;
        }
        public string GetModelFromLot(string lot)
        {
            string model = "";
            try
            {
                string sqlQuery = "select distinct TC047 from SFCTC where TC004 = '" + lot.Substring(0, 4) + "' and TC005 = '" + lot.Substring(5, 8) + "'";
                sqlERPCON sqlERPCON = new sqlERPCON();
                model = sqlERPCON.sqlExecuteScalarString(sqlQuery);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "GetModelFromLot (string lot)", ex.Message);

                return "";
            }
            return model;
        }
        public bool InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge, Model.SettingClass setting)
        {
            try
            {
                string datetimeserno = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serno = lot + "-" + datetimeserno;
                string site = "B01";
                string factory = "TECHLINK";
                string process = "MQC";

                string status = "";
                string date_ = DateTime.Now.ToString("yyyy-MM-dd");
                string time_ = DateTime.Now.ToString("HH:mm:ss");
                string sqlQuerry = "";

                // string model = GetModelFromLot(lot,setting);
                string model = GetModelFromLot(lot);// lay model tren server, neu ko lay dc thi lay o local
                if(model=="")
                {
                    model = GetModelFromLot(lot,setting);
                }
                sqlQuerry += "insert into m_ERPMQC_REALTIME (serno, lot, model, site, factory, line, process,item,inspectdate,inspecttime, data, judge,status,remark ) values( '";
                sqlQuerry += serno + "', '" + lot + "', '" + model + "', '" + site + "', '" + factory + "', '" + line +
               "', '" + process + "', '" + item + "', '" + date_ + "', '" + time_ + "', '" + data + "', '" + judge.ToString() + "', '" + status + "', '" + Remark + "' )";
                LocalPLCSql localPLC = new LocalPLCSql();
                return localPLC.sqlExecuteNonQuery(sqlQuerry, false,setting);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }

        }


        public bool InsertToMQCMES_Realtime(string QRMES,string QRID, string line, string item, string data, string Remark, Model.SettingClass setting)
        {
            try
            {
                string datetimeserno = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serno = QRMES;
                string site = "B01";
                string factory = "TECHLINK";
                string process = "MQC";

                string status = "";
                string date_ = DateTime.Now.ToString("yyyy-MM-dd");
                string time_ = DateTime.Now.ToString("HH:mm:ss");
                string sqlQuerry = "";

                // string model = GetModelFromLot(lot,setting);
                QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(QRMES);
                QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat(QRID);
                sqlQuerry += "insert into m_ERPMQC_REALTIME (serno, lot, model, site, factory, line, process,item,inspectdate,inspecttime, data, judge,status,remark ) values( '";
                sqlQuerry += serno + "', '" + qRMQC_MES.PO + "', '" + qRMQC_MES.Product + "', '" + site + "', '" + factory + "', '" + line +
               "', '" + process + "', '" + item + "', '" + date_ + "', '" + time_ + "', '" + data + "', '" + qRIDMES.ID + "', '" + status + "', '" + Remark + "' )";
                LocalPLCSql localPLC = new LocalPLCSql();
                return localPLC.sqlExecuteNonQuery(sqlQuerry, false, setting);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }

        }
    }
}
