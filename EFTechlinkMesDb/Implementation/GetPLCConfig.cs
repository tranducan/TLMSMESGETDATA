using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb.Interface;
using EFTechlinkMesDb.Model;

namespace EFTechlinkMesDb.Implementation
{
    public class GetPLCConfig : IGetPLCConfig
    {
        public List<m_ipPLC> GetPLCConfigWithProcessFilter(string filterProcess, string version)
        {
            using (var context = new EFTechLinkMESModel())
            {
                var pLCConfig = context.m_ipPLC.Where(d=> d.isactive == true && d.process == filterProcess && d.MQCversion == version)
                                                .ToList();

                return pLCConfig;
            };
        }

        public List<m_ipPLC> GetAllPLCConfig()
        {
            using (var context = new EFTechLinkMESModel())
            {
                var pLCConfig = context.m_ipPLC.Where(d => d.isactive == true)
                                                .ToList();

                return pLCConfig;
            };
        }
    }
}
