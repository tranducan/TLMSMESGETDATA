using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCSimenNetWrapper
{
    class PlcDriverException : Exception
    {
        public ErrorCode Error { get; private set; }        

        public PlcDriverException(ErrorCode code) 
        {
            this.Error = Error;            
        }
    }
}
