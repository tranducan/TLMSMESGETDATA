using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;




namespace TLMSMESGETDATA.PLC

{
   public class IPPLC
    {
        public DbPLC IPPLC_MQC(string IP)
        {
            DbPLC con = new DbPLC();
            try
            {
                using (var plc_MQC = new Plc(CpuType.S71200, IP, 0, 1))
                {
                    plc_MQC.Open();
                  
                    con.READPLC(plc_MQC);
                
                    plc_MQC.Close();
                }
                return con;
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err,  "void IPPLC_MQC(string IP)", ex.Message);
                return null;
            }
         

        }
    }
}
