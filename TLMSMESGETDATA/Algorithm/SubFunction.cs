using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLMSMESGETDATA.SQLUpload;

namespace TLMSMESGETDATA
{
    public static class SubFunction
    {
        /* Return : 0 OK
         * Return : 1 QRMES Error
         * Return : 2 QRID Error
         * Return : 3 QMES Error and QRID Error
         * Return : 4 Complete QR
         */
        public static int IsValidationQRCode(string QRMES, string QRID)
        {
            bool IsQRMES = false;
            bool IsQRID = false;
            if (QRMES.Length > 2)
            {
                if (QRMES.StartsWith("s"))
                {
                    if (QRMES.EndsWith("e"))
                    {
                        var QRArray = QRMES.Substring(1, QRMES.Length - 2).Split(';');

                            IsQRMES = true;
                    }
                    else
                    {

                        SystemLog.Output(SystemLog.MSG_TYPE.War, "QR MES nust format start with: e", QRMES);
                        return 1;

                    }
                }
                else
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "QR MES nust format start with: S", QRMES);
                    return 1;

                }

            }
            if (QRID.Length > 3)
            {
                if (QRID.Contains("TL"))
                {
                    IsQRID = true;
                }

            }
            if (IsQRMES && IsQRID)
            {
                return 0;
            }
            else if (IsQRMES == false)
            {
                return 1;
            }
            else if (IsQRID == false)
            {
                return 2;
            }

            return 3;
        }
    }

  
}
