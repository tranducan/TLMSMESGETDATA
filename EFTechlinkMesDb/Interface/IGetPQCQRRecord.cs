using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb.Model;

namespace EFTechlinkMesDb.Interface
{
    public interface IGetPQCQRRecord
    {
        PQCQRRecord GetPQCQRRecordWithFilterQRcode(string QRCode);
    }
}
