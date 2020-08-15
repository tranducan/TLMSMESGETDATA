using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using TLMSMESGETDATA.Model;

namespace TLMSMESGETDATA.PLC2
{
    class PLCData
    {
       public List<MachineItem> GetIpMachineRuning()
        {
            List<MachineItem> machineItems = new List<MachineItem>();
            DataTable dt = new DataTable();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("select factory, line, modelPLC, IPPLC from m_ipPLC where isactive = 'True'");
            stringBuilder.Append(" and process ='" + "MQC" + "' ");
            stringBuilder.Append(" order by line ");
            sqlCON sqlERPCON = new sqlCON();

            sqlERPCON.sqlDataAdapterFillDatatable(stringBuilder.ToString(), ref dt);
            machineItems = (from DataRow dr in dt.Rows
                            select new MachineItem()
                            {
                                Location = dr["factory"].ToString(),
                                Line = dr["line"].ToString(),
                                Model = dr["modelPLC"].ToString(),
                                IP = dr["IPPLC"].ToString(),
                                IsEnable = true                              
                            }).ToList();

            return machineItems;


        }

    }
}
