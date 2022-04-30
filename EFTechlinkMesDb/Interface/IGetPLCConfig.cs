using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb.Model;

namespace EFTechlinkMesDb.Interface
{
    public interface IGetPLCConfig
    {
        List<m_ipPLC> GetAllPLCConfig();

        List<m_ipPLC> GetPLCConfigWithProcessFilter(string filterProcess, string version);
    }
}
