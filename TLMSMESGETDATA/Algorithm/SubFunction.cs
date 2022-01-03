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
                        if (QRArray.Count() == 10)
                        {
                            IsQRMES = true;
                        }
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
        public static void InsertTargettoSOTDb(string QRMES, string Product, int Output, int Scrap)
        {
            try
            {

                SQLUpload.SQLQRUpdate sQLQR = new SQLQRUpdate();
                DataTable dtQRRecord = sQLQR.GetQuanityFromQRMES(QRMES);
                if (dtQRRecord.Rows.Count == 0)
                {
                    SQLERPTarget sQLERPTarget = new SQLERPTarget();
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(@" USE [SOT]

DECLARE @DATE varchar(8) =  _DATE
DECLARE @PRODCODE nchar(100) =  _PRODCODE
DECLARE @OUTPUT numeric(18,0) = _OUTPUT
DECLARE @SCRAP numeric(18,0) =  _SCRAP

IF (NOT EXISTS(SELECT TOP 1 1 FROM dbo.DAILYTARGET WHERE DATE = @DATE AND PRODCODE = @PRODCODE))
BEGIN
INSERT INTO [dbo].[DAILYTARGET]
           ([DATE]
           ,[PRODCODE]
           ,[OUTPUT]
           ,[SCRAP])
     VALUES
           (@DATE
           ,@PRODCODE
           ,@OUTPUT
           ,@SCRAP)

 END
 ELSE
 UPDATE [dbo].[DAILYTARGET] SET OUTPUT = OUTPUT + @OUTPUT, SCRAP = SCRAP+ @SCRAP
 WHERE DATE = @DATE AND PRODCODE = @PRODCODE
");
                    stringBuilder.Replace("_DATE", "'" + DateTime.Now.ToString("yyyyMMdd") + "'");
                    stringBuilder.Replace("_PRODCODE", "'" + Product + "'");
                    stringBuilder.Replace("_OUTPUT", Output.ToString());
                    stringBuilder.Replace("_SCRAP", Scrap.ToString());

                    sQLERPTarget.sqlExecuteNonQuery(stringBuilder.ToString(), false);
                }
            }

            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "InsertTargettoSOTDb(string QRMES, string Product, int Output, int Scrap)", ex.Message);
            }

        }
    }

  
}
