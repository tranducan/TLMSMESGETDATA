using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.PLC2
{
    class DB1
    {
        public string Good_Products_Total { get; set; }
        public string NG_Products_Total { get; set; }

        public string[] NG_Products_NG_ = new string[38];

        //4
        public string RW_Products_Total { get; set; }
        public string[] RW_Products_NG_ = new string[38];

        //8
        public string PLC_Barcode { get; set; }
    }
}
