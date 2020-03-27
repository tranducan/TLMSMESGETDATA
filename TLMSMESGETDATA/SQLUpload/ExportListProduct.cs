using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA.SQLUpload
{
 public  static class ExportListProduct
    {
    public  static  void exportcsvToPLC(string linkListProduct)
        {
            try
            {

                string path = linkListProduct;
                File.Delete(path + "ListProduct.csv");
                StringBuilder sql = new StringBuilder();
                sql.Append(@"
select TA001+'-'+RTRIM(TA002)+';'+TA003+';'+TA004+';'+ RTRIM(TA006),  RTRIM(TC047),RTRIM(TA010),  RTRIM(TA011), RTRIM(TA012)  ,TA001,TA002,TA003,TA004, TA006 from SFCTA 
left join SFCTC on TA001 = TC004 and TA002 = TC005
where 1=1
and TA004 = 'B01'
and TA011+TA012 <TA010
group by TA001,TA002,TA003,TA004, TA006, TA010, TA011,TA012, TC047
");
                DataTable dtshow = new DataTable();
                sqlERPCON data = new sqlERPCON();
                data.sqlDataAdapterFillDatatable(sql.ToString(), ref dtshow);

                StringBuilder builder = new StringBuilder();
                int rowcount = dtshow.Rows.Count;
                int columncount = dtshow.Columns.Count;
                List<string> cols = new List<string>();

                // builder.AppendLine(string.Join("\t", cols.ToArray()));
                for (int i = 0; i < rowcount; i++)
                {
                    cols = new List<string>();
                    for (int j = 0; j < 5; j++) //Chỉ lay 4 cọt đâu thôi, yêu cầu của Đức
                    {
                        cols.Add(dtshow.Rows[i][j].ToString() + @",");
                    }
                    builder.AppendLine(string.Join("", cols.ToArray()));
                }
                System.IO.File.WriteAllText(path + "ListProduct.csv", builder.ToString());
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "exportcsvToPLC()", ex.Message);
            }
        }
    }
}
