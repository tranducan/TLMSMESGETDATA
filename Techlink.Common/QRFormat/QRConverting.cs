using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Techlink.Common.QRFormat
{
    public static class QRConverting
    {
        public static QRCodeMES GetQRCodePQCStation(string QRCode)
        {
            QRCodeMES qRCodeMES = new QRCodeMES();
            if (string.IsNullOrEmpty(QRCode))
                throw new ArgumentNullException("QR code is empty");

            QRCode = QRCode.Trim();
            if (QRCode.Substring(0, 1) == "s" && QRCode.Substring((QRCode.Length - 1), 1) == "e")
            {
                var QRArray = QRCode.Substring(1, QRCode.Length - 2).Split(';');
                    qRCodeMES.PO = QRArray[1];
                    qRCodeMES.Product = QRArray[2];
                    qRCodeMES.lot = QRArray[3];
                    qRCodeMES.buff1 = QRArray[3];
                    qRCodeMES.quantity = int.Parse(QRArray[4], NumberStyles.AllowThousands);
                    qRCodeMES.dateTime = QRArray[5];
                    qRCodeMES.buff2 = QRArray[7];
                    qRCodeMES.buff3 = QRArray[8];
                    qRCodeMES.buff4 = QRArray[9];
                    qRCodeMES.buff5 = QRArray.Count() == 11? QRArray[10] : "";
                
            }

            return qRCodeMES;
        }

        public static QRCodeMES GetQRCodeMQCStation(string QRCode)
        {
            QRCodeMES qRCodeMES = new QRCodeMES();
            if (string.IsNullOrEmpty(QRCode))
                throw new ArgumentNullException("QR code is empty");

            QRCode = QRCode.Trim();
            if (QRCode.Substring(0, 1) == "s" && QRCode.Substring((QRCode.Length - 1), 1) == "e")
            {
                var QRArray = QRCode.Substring(1, QRCode.Length - 2).Split(';');

                    qRCodeMES.PO = QRArray[1];
                    qRCodeMES.Product = QRArray[2];
                    qRCodeMES.lot = QRArray[3];
                    qRCodeMES.buff1 = QRArray[3];
                    qRCodeMES.quantity = int.Parse(QRArray[4], NumberStyles.AllowThousands);
                    qRCodeMES.dateTime = QRArray[5];
                    qRCodeMES.buff2 = QRArray[7];
                    qRCodeMES.buff3 = QRArray[8];
                    qRCodeMES.buff4 = QRArray[9];

                
            }

            return qRCodeMES;
        }
    }
}
