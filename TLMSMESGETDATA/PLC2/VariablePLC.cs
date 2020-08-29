using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.PLC2
{
    class VariablePLC
    {
        public const string Good_Products_Total = "DB2.DBW2";
        public const string NG_Products_Total = "DB3.DBW0";
        public const string RW_Products_Total = "DB4.DBW0";
        public const string barcode1 = "DB7.DBB100";
        public const string AddingAvaiable = "DB1.DBD6";
        public const string GapQty = "DB1.DBD2";
        public const string OnOFF = "DB9.DBX0.2";
        public const string RealQty = "DB151.DBW0";
        public const string GapValueQty = "DB151.DBW4";
        public const string FlagKT = "DB7.DBX0.2";
        public const string MESQR = "DB180.DBD256";
        public const string IDWorker = "DB180.DBD0";
        public const string NumberCharMESQR = "DB181.DBW202";
        public const string NumberCharIDWorker = "DB181.DBW200";
        public const string WriteReadyStart = "DB181.DBX204.0";
        public const string WriteMessage = "DB181.DBW206";
        public const string OKProduced = "DB6.DBW4";
        public const string NGProduced = "DB6.DBW6";
        public static List<string> List38Errors()
        {
            string barcodeStart = "DB3.DBW";
            List<string> list = new List<string>();
            for (int i = 1; i < 38; i++)
            {
                list.Add(barcodeStart + (i * 2 + 2).ToString());
            }
            return list;
        }
        public static List<string> List38Reworks()
        {
            string barcodeStart = "DB4.DBW";
            List<string> list = new List<string>();
            for (int i = 1; i < 38; i++)
            {
                list.Add(barcodeStart + (i * 2 + 2).ToString());
            }
            return list;
        }
        public static List<string> barcodeaddress()
        {
            string barcodeStart = "DB7.DBB";
            List<string> list = new List<string>();
            for (int i = 100; i < 126; i++)
            {
                list.Add(barcodeStart + i.ToString());
            }
            return list;
        }

        public static List<string> barcodeaddresID()
        {
            string barcodeStart = "DB181.DBB";
            List<string> list = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(barcodeStart + i.ToString());
            }
            return list;
        }
        public static List<string> barcodeaddressMES()
        {
            string barcodeStart = "DB181.DBB";
            List<string> list = new List<string>();
            for (int i = 100; i < 200; i++)
            {
                list.Add(barcodeStart + i.ToString());
            }
            return list;
        }
    }
}
