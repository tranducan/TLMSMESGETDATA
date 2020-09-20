using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.Model
{
    class MachineOperation
    {   public string Line { get; set; }
        public string IP { get; set; }
        public string QR { get; set; }
        public string Lot { get; set; }
        public string product { get; set; }
        public string Inspector { get; set; }
        public string Status{ get; set; }
        public double Output { get; set; }
        public double NG { get; set; }
        public double Rework { get; set; }
       

    }
}
