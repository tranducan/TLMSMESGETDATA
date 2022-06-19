using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTechlinkMesDb.Interface
{
    public interface IDataContextAction
    {
        PQCMesData Select(string Model);
        bool Insert(PQCMesData pQCMesData);

        List<PQCMesData> SelectTopNotProcess(string flag, int topQuery);

        bool UpdateFlagTransferingSuccessful(long ID);

    }
}
