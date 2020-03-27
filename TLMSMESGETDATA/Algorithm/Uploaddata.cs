using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLMSMESGETDATA.PLC2;

namespace TLMSMESGETDATA.Algorithm
{
   class Uploaddata
    {
        public DataMQC ChangeMQCData(DataMQC dataOld, DataMQC dataNew, out bool IsChange )
        {
            DataMQC _dataRaise = new DataMQC();
            bool _ischange = false;

            try
            {
                dataNew.DateTimeReset = dataOld.DateTimeReset;
                if (dataNew.Good_Products_Total > dataOld.Good_Products_Total || dataNew.NG_Products_Total > dataOld.NG_Products_Total
                         || dataNew.RW_Products_Total > dataOld.RW_Products_Total)
                {

                    _ischange = true;
                    _dataRaise.Good_Products_Total = dataNew.Good_Products_Total - dataOld.Good_Products_Total;
                    _dataRaise.NG_Products_Total = dataNew.NG_Products_Total - dataOld.NG_Products_Total;
                    _dataRaise.RW_Products_Total = dataNew.RW_Products_Total - dataOld.RW_Products_Total;
                    _dataRaise.PLC_Barcode = dataNew.PLC_Barcode != "" ? dataNew.PLC_Barcode : dataOld.PLC_Barcode;
                    for (int i = 0; i < 38; i++)
                    {
                        if (_dataRaise.NG_Products_Total > 0)
                        {
                            _dataRaise.NG_Products_NG_[i] = dataNew.NG_Products_NG_[i] - dataOld.NG_Products_NG_[i];
                        }
                        if (_dataRaise.RW_Products_Total > 0)
                        {
                            _dataRaise.RW_Products_NG_[i] = dataNew.RW_Products_NG_[i] - dataOld.RW_Products_NG_[i];
                        }
                    }
                }
                else if(dataNew.STARTSTOP != dataOld.STARTSTOP)
                {
                    _ischange = true;
                    _dataRaise.Good_Products_Total = dataNew.Good_Products_Total - dataOld.Good_Products_Total;
                    _dataRaise.NG_Products_Total = dataNew.NG_Products_Total - dataOld.NG_Products_Total;
                    _dataRaise.RW_Products_Total = dataNew.RW_Products_Total - dataOld.RW_Products_Total;
                    _dataRaise.PLC_Barcode = dataNew.PLC_Barcode != "" ? dataNew.PLC_Barcode : dataOld.PLC_Barcode;
                    for (int i = 0; i < 38; i++)
                    {
                        if (_dataRaise.NG_Products_Total > 0)
                        {
                            _dataRaise.NG_Products_NG_[i] = dataNew.NG_Products_NG_[i] - dataOld.NG_Products_NG_[i];
                        }
                        if (_dataRaise.RW_Products_Total > 0)
                        {
                            _dataRaise.RW_Products_NG_[i] = dataNew.RW_Products_NG_[i] - dataOld.RW_Products_NG_[i];
                        }
                    }
                }

                else
                {
                    _ischange = false;
                    _dataRaise = dataNew;

                }


                IsChange = _ischange;
            }


            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.War, "ChangeMQCData :" + dataOld.PLC_Barcode, ex.Message);
                IsChange = false;
            }
            return _dataRaise;
        }

        public bool InsertMQCUpdateRealtime (DataMQC data, string Line, bool Isreset)
        {

           
                try
                {
                if (Isreset)
                {
                    if ( data.PLC_Barcode.Length == 26)
                    {
                     
                              InsertToMQC_Realtime(data.PLC_Barcode, Line, "OUTPUT", data.Good_Products_Total.ToString(), "OP",1);
                      
                            for (int i = 1; i < 39; i++)
                            {
                            if (data.NG_Products_NG_[i - 1] > 0)
                                InsertToMQC_Realtime(data.PLC_Barcode, Line, "NG" + i.ToString(), data.NG_Products_NG_[i - 1].ToString(), "NG",1);
                            }
                        
                    
                        
                            for (int i = 1; i < 39; i++)
                            {
                                if (data.RW_Products_NG_[i - 1] > 0)
                                    InsertToMQC_Realtime(data.PLC_Barcode, Line, "RW" + i.ToString(), data.RW_Products_NG_[i - 1].ToString(), "RW",1);
                            }
                        
                    }
                    else
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode wrong formart ", data.PLC_Barcode);
                    }
                }
                else
                {
                    if ( data.PLC_Barcode.Length == 26)
                    {
                        if (data.Good_Products_Total > 0)
                            InsertToMQC_Realtime(data.PLC_Barcode, Line, "OUTPUT", data.Good_Products_Total.ToString(), "OP",0);
                        if (data.NG_Products_Total > 0)
                        {
                            for (int i = 1; i < 39; i++)
                            {
                                if (data.NG_Products_NG_[i - 1] > 0)
                                    InsertToMQC_Realtime(data.PLC_Barcode, Line, "NG" + i.ToString(), data.NG_Products_NG_[i - 1].ToString(), "NG",0);
                            }
                        }
                        if (data.RW_Products_Total > 0)
                        {
                            for (int i = 1; i < 39; i++)
                            {
                                if (data.RW_Products_NG_[i - 1] > 0)
                                    InsertToMQC_Realtime(data.PLC_Barcode, Line, "RW" + i.ToString(), data.RW_Products_NG_[i - 1].ToString(), "RW",0);
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
        public string GetModelFromLot (string lot)
        { 
            string model = "";
            try
            {
                string sqlQuery = "select distinct TC047 from SFCTC where TC004 = '" + lot.Substring(0,4) + "' and TC005 = '" + lot.Substring(5,8) + "'";
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
        public bool InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)
        {
            try
            {
                string datetimeserno = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serno = lot+ "-" + datetimeserno;
                string site = "B01";
                string factory = "TECHLINK";
                string process = "MQC";
              
                string status = "";
                string date_ = DateTime.Now.ToString("yyyy-MM-dd");
                string time_ = DateTime.Now.ToString("HH:mm:ss");
                string sqlQuerry = "";

                string model = GetModelFromLot(lot);
                sqlQuerry += "insert into m_ERPMQC_REALTIME (serno, lot, model, site, factory, line, process,item,inspectdate,inspecttime, data, judge,status,remark ) values( '";
                sqlQuerry += serno + "', '" + lot + "', '" + model + "', '" + site + "', '" + factory + "', '" + line +
               "', '" + process + "', '" +item + "', '"+ date_+ "', '" + time_ + "', '"+  data + "', '"+ judge.ToString() + "', '"+ status + "', '" + Remark + "' )";
                sqlCON sqlCON = new sqlCON();
                return sqlCON.sqlExecuteNonQuery(sqlQuerry, false);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }

        }

        public bool IsExtinstDataReset(string line, DateTime dateTime)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" select ISNULL(COUNT(data),'0') from m_ERPMQC_REALTIME where 1=1 and judge = '1' ");
                stringBuilder.Append(" and  line = '" + line + "' ");
                stringBuilder.Append(" and cast(inspectdate as datetime) + CAST (inspecttime as datetime) > '" + dateTime.ToString("yyyyMMdd hh:mm:ss") + "' ");
                sqlCON sqlCON = new sqlCON();
             var temp =   sqlCON.sqlExecuteScalarString(stringBuilder.ToString());
                try
                {
                    int count = int.Parse(temp.Trim());
                    if (count > 0)
                        return true;
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, " convert to int", ex.Message);
                    return false;
                }



            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, " IsExtinstDataReset(string line, DateTime dateTime)", ex.Message);
                return false;
            }
            return false;
        }
     public   double QuantityCanRun(string lot)
        {
            double ReadyQty = 0;
            try
            {
                if (lot.Length == 26)
                {
                    sqlERPCON data = new sqlERPCON();

                    string lottam = lot;
                    double StockERP = double.Parse(data.sqlExecuteScalarString("select isnull(sum(TA011+TA012),'0') from SFCTA where ( TA001+ '-'+RTRIM(TA002)+';'+ TA003 +';'+TA004 +';'+TA006 ) = '" + lot + "' "));
                    var PlanERP = double.Parse(data.sqlExecuteScalarString("select (isnull (TA010,'0')) from SFCTA where ( TA001+ '-'+RTRIM(TA002)+';'+ TA003 +';'+TA004 +';'+TA006 ) = '" + lot + "' "));
                    sqlCON sqlCON = new sqlCON();
                    double StockRealtime = double.Parse(sqlCON.sqlExecuteScalarString("select isnull(sum(cast( isnull(data,'0') as int)),0) from m_ERPMQC_REALTIME  where lot = '" + lot + "' and remark != 'RW'"));
                    var MAXSYSTEM = StockERP >= StockRealtime ? StockERP : StockRealtime;
                    ReadyQty = (PlanERP - MAXSYSTEM);
                }
        }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "QuantityCanRun(string lot)", ex.Message);
            }
            return ReadyQty;

        }


    }
}
