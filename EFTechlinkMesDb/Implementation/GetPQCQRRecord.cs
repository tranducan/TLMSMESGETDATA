using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTechlinkMesDb.Interface;
using EFTechlinkMesDb.Model;

namespace EFTechlinkMesDb.Implementation
{
    public class GetPQCQRRecord : IGetPQCQRRecord
    {
        public PQCQRRecord GetPQCQRRecordWithFilterQRcode(string QRCode)
        {
            using (var context = new EFTechLinkMESModel())
            {
                var pQCQRecord= context.PQCQRRecords.Where(d => d.QR == QRCode)
                                                .FirstOrDefault();

                return pQCQRecord;
            };
        }
    }
}
