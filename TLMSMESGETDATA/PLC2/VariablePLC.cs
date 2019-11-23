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
            for (int i = 100; i < 125; i++)
            {
                list.Add(barcodeStart + i.ToString());
            }
            return list;
        }


    }
}
