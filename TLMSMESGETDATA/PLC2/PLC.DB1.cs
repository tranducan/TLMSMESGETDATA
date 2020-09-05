using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.PLC2
{
    class DataMQC
    {
        public double Good_Products_Total { get; set; }
        public double NG_Products_Total { get; set; }

        public int[] NG_Products_NG_ = new int[38];

        //4
        public double RW_Products_Total { get; set; }
        public int[] RW_Products_NG_ = new int[38];

        //8
        public string PLC_Barcode { get; set; }
        public string STARTSTOP { get; set; }
        public DateTime DateTimeReset { get; set; }
        public bool IsChecKQRCode { get; set; }
        public bool IsReadyStart { get; set; }
        public bool IsReset { get; set; }
        public bool IsValidQRID { get; set; }
        public bool IsValidQRMQC { get; set; }
        public int numberCharQRMES { get; set; }
        public int numberCharQRID { get; set; }
        public string strQRMES { get; set; }
        public string strQRID { get; set; }
    }
}
