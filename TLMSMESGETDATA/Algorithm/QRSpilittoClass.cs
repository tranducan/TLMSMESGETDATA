using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.Algorithm
{

    //s;B511-20040008;BMH1257060S05;PSC;5,000;18/06/2020;;202011;;21202020202020e

    public static class QRSpilittoClass
    {
        public static QRMQC_MES QRstring2MQCFormat(string QRstr)
        {
            QRMQC_MES qRMQC_MES = new QRMQC_MES();
            if(QRstr.Length > 0)
            {
              //  if(QRstr.Substring(0,1)=="s" && QRstr.Substring((QRstr.Length-1),1)== "e")
                {
                    var QRArray = QRstr.Substring(1, QRstr.Length - 2).Split(';');
                    if(QRArray.Count() == 10)
                    {
                        qRMQC_MES.PO = QRArray[1];
                        qRMQC_MES.Product = QRArray[2];
                        qRMQC_MES.Unit = QRArray[3];
                        qRMQC_MES.quantity = int.Parse(QRArray[4],NumberStyles.AllowThousands);
                        qRMQC_MES.dateTime = QRArray[5];
                        qRMQC_MES.str1 = QRArray[6];
                        qRMQC_MES.str2 = QRArray[7];
                        qRMQC_MES.str3 = QRArray[8];
                        qRMQC_MES.str4 = QRArray[9];
                        return qRMQC_MES;
                    }

                }
               
            }
            return null;
        }

        /// <summary>
        /// /44Z9D075C2O1;TL-0079;Nguyễn Thị Bích Kiều;
        /// </summary>
        /// <param name="QRstr"></param>
        /// <returns></returns>
        public static QRIDMES QRstring2IDFormat(string QRstr)
        {
            QRIDMES qRIDMES = new QRIDMES();
            if (QRstr.Length > 0)
            {
                  var QRArray = QRstr.Split(';');
                    if (QRArray.Count() ==4)
                    {
                    qRIDMES.str1 = QRArray[0];
                    qRIDMES.ID = QRArray[1];
                    qRIDMES.FullName = QRArray[2];
                    return qRIDMES;
                }

             
               
            }
            return qRIDMES;
        }
    }
public    class QRMQC_MES
    {
        public string PO { get; set; }
        public string Product { get; set; }
        public string Unit { get; set; }
        public int quantity { get; set; }
        public string dateTime { get; set; }
        public string str1 { get; set; }
        public string str2 { get; set; }
        public string str3 { get; set; }
        public string str4 { get; set; }
    }

    public class QRIDMES
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string str1 { get; set; }
        
    }
}
