using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Techlink.Common.QRFormat
{
    public class QRCodeMES
    {
        public string PO { get; set; }
        public string Product { get; set; }
        public string lot { get; set; }
        public string Unit { get; set; }
        public int quantity { get; set; }
        public string dateTime { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
    }
}
