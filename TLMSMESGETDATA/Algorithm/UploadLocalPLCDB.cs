using System;
using System.Text;
using TLMSMESGETDATA.PLC2;
using EFTechlinkMesDb.Interface;
using EFTechlinkMesDb.Implementation;
using EFTechlinkMesDb.Model;
using EFTechlinkMesDb;

namespace TLMSMESGETDATA.Algorithm
{
    class UploadLocalPLCDB
    {
    
        private IDataContextAction dataContextAction = new DataContextAction();
        public bool InsertMQCMESUpdateRealtime(Model.MQCVariable mQCChanged,string TypeChange, string Line, Model.SettingClass setting)
        {
            try
            {
                if(TypeChange == "OPQTY")
                {
                    if (mQCChanged.ListMQCQty[0] > 30)
                        SystemLog.Output(SystemLog.MSG_TYPE.War, "insert Output quantity seems like abnormal", "Output : " + mQCChanged.ListMQCQty[0].ToString());
                    //InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "OUTPUT", mQCChanged.ListMQCQty[0].ToString(), "OP");
                    var pQCMESData = MappedtoPQCMesData(mQCChanged.QRMES, mQCChanged.QRID, Line, "OUTPUT", mQCChanged.ListMQCQty[0], "OP");
                    dataContextAction.Insert(pQCMESData);
                }
                if(TypeChange == "NGQTY")
                {
                    for (int i = 0; i < mQCChanged.ListNG38.Count; i++)
                    {
                        if (mQCChanged.ListNG38[i] > 10)
                            SystemLog.Output(SystemLog.MSG_TYPE.War, "insert NG quantity seems like abnormal", "NG" + (i + 1).ToString()+": " + mQCChanged.ListNG38[i].ToString());
                        if (mQCChanged.ListNG38[i] > 0 )
                        {
                            var pQCMESData = MappedtoPQCMesData(mQCChanged.QRMES, mQCChanged.QRID, Line, "NG" + (i + 1).ToString(), mQCChanged.ListNG38[i], "NG");
                            dataContextAction.Insert(pQCMESData);
                        }
                        //InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "NG"+(i+1).ToString(), mQCChanged.ListNG38[i].ToString(), "NG");
                    }
                  
                }
                if (TypeChange == "RWQTY")
                {
                    for (int i = 0; i < mQCChanged.ListRW38.Count; i++)
                    {
                        if (mQCChanged.ListRW38[i] > 10)
                            SystemLog.Output(SystemLog.MSG_TYPE.War, "insert RW quantity seems like abnormal", "RW" + (i + 1).ToString() + ": " + mQCChanged.ListRW38[i].ToString());
                        if (mQCChanged.ListRW38[i] > 0)
                        {
                            var pQCMESData = MappedtoPQCMesData(mQCChanged.QRMES, mQCChanged.QRID, Line, "RW" + (i + 1).ToString(), mQCChanged.ListRW38[i], "RW");
                            dataContextAction.Insert(pQCMESData);
                        }
                            //InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "RW" + (i + 1).ToString(), mQCChanged.ListRW38[i].ToString(), "RW");
                    }

                }
                if (TypeChange == "Reset")
                {
                    var pQCMESData = MappedtoPQCMesData(mQCChanged.QRMES, mQCChanged.QRID, Line, "Reset", 0, "Reset");
                    dataContextAction.Insert(pQCMESData);
                    //InsertToMQCMES_Realtime(mQCChanged.QRMES, mQCChanged.QRID, Line, "Reset", "", "Reset");
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertMQCUpdateRealtime (DataMQC data, string Line, bool Isreset)", ex.Message);

                return false;
            }

            return true;

        }
       
        public bool InsertToMQCMES_Realtime(string QRMES,string QRID, string line, string item, string data, string Remark)
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
                sqlCON localPLC = new sqlCON();
                return localPLC.sqlExecuteNonQuery(sqlQuerry, false);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertToMQC_Realtime(string lot, string line, string item, string data, string Remark, int judge)", ex.Message);
                return false;
            }

        }

        PQCMesData MappedtoPQCMesData(string QRMES, string QRID, string line, string item, decimal Quanlity, string attributeType)
        {
            QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(QRMES);
            QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat(QRID);

            PQCMesData qRMQC_ = new PQCMesData()
            {
                POCode = QRMES,
                Inspector = qRIDMES.ID,
                Model = qRMQC_MES.Product,
                Site = "TechLink",
                Line = line,
                Attribute = item,
                Quantity = Quanlity,
                AttributeType = attributeType,
                Flag = "",
                InspectDateTime = DateTime.Now,
                LotNumber = qRMQC_MES.lot,
                Process = "PQC",
            };

            return qRMQC_;
        }

    }
}
