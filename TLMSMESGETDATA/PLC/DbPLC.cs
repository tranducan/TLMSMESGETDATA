using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;

namespace TLMSMESGETDATA.PLC
{
    public class DbPLC
    {
        public class DbPLC_2
        {
            public static readonly DbPLC.DbPLC_2 storage = new DbPLC.DbPLC_2();

            public static DbPLC_2 GetStorage()
            {
                return storage;
            }
            public string Good_Products_Total { get; set; }

            //3
            public string NG_Products_Total { get; set; }

            public string[] NG_Products_NG_ = new string[38];

            //4
            public string RW_Products_Total { get; set; }
            public string[] RW_Products_NG_ = new string[38];

            //8
            public string PLC_Barcode { get; set; }
        }
        public void READPLC(Plc plc)
        {
            string d = "";
            DbPLC.DbPLC_2 va = DbPLC.DbPLC_2.GetStorage();
            object DB7 = new object();
  
            for (int iwork = 0; iwork < 25; iwork++)
            {
                if (iwork < 10)
                {string tem  = "DB7.DBB10" + iwork;
                    va.PLC_Barcode = plc.Read("DB7.DBB10" + iwork).ToString(); //testing
                    char c = Convert.ToChar(int.Parse(va.PLC_Barcode));
                    d = d + c.ToString();
                }
                else
                {
                    va.PLC_Barcode = plc.Read("DB7.DBB1" + iwork).ToString(); //testing
                    char c = Convert.ToChar(int.Parse(va.PLC_Barcode));
                    d = d + c.ToString();
                }
            }
            va.PLC_Barcode = d;
            va.Good_Products_Total = plc.Read("DB2.DBW2").ToString();
            va.NG_Products_Total = plc.Read("DB3.DBW0").ToString(); //NG
            va.RW_Products_Total = plc.Read("DB4.DBW0").ToString(); //RW

            //vậy nó chạy xong . 
            for (int i = 1; i < 38; i++) //NG
            {
                va.NG_Products_NG_[i] = (plc.Read("DB3.DBW" + (i * 2 + 2)).ToString());
            }
            for (int i = 1; i < 38; i++) //RW
            {
                va.RW_Products_NG_[i] = (plc.Read("DB4.DBW" + (i * 2 + 2)).ToString());
            }//bỏ cái debug dùm e
        }
    }
}
