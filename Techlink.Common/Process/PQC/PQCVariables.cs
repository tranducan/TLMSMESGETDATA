using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Techlink.Common.Process.PQC
{
    public class PQCVariables
    {
        public PQCVariables()
        {
            Connection = -1;
            QRMES = "";
            QRID = "";
            ListMQCQty = new List<int>();
            ListQtyProduced = new List<int>();
            ListNG38 = new List<int>();
            ListRW38 = new List<int>();
            DicSPLCtatus = new Dictionary<string, bool>();
        }

        public int Connection { get; set; }
        public string QRMES { get; set; }
        public string QRID { get; set; }
        public List<int> ListMQCQty { get; set; }
        public List<int> ListQtyProduced { get; set; }

        public List<int> ListNG38 { get; set; }
        public List<int> ListRW38 { get; set; }
        public Dictionary<string, bool> DicSPLCtatus { get; set; }
    }
}
