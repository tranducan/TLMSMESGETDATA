using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace TLMSMESGETDATA.SQLUpload
{
    class SQLQRUpdate
    {
        public DataTable GetQuanityFromQRMES(string QRMES)
        {
            DataTable dt = new DataTable();
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" select * from dbo.m_MQCQR_Record where 1=1 ");
                stringBuilder.Append(" and  QR ='" + QRMES + "' ");
                sqlCON sqlCON = new sqlCON();
                sqlCON.sqlDataAdapterFillDatatable(stringBuilder.ToString(), ref dt);
                return dt;

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err,"Get quantity From QRMES error", ex.Message);

            }
            return dt;
        }
       public void UpdateOrInsertQRRecordTable(Model.MQCVariable qCVariable,  string line)
        {
            try
            {

        
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("begin tran");
            stringBuilder.Append(" declare @QRMES varchar(100) set @QRMES = '" + qCVariable.QRMES + "' ");
            stringBuilder.Append(" declare @StartDate datetime2(7) set @StartDate = GETDATE() ");
            stringBuilder.Append(" declare @EndDate datetime2(7) set @EndDate = GETDATE() ");
            stringBuilder.Append(" declare @OPQty int set @OPQty = "+ qCVariable.ListMQCQty[0]);
            stringBuilder.Append(" declare @NGQty int set @NGQty = " + qCVariable.ListMQCQty[1]);
            stringBuilder.Append(" declare @RWQty int set @RWQty =" + qCVariable.ListMQCQty[2]);
            stringBuilder.Append(" declare @Line varchar(10) set @Line = '" + line +"' ");
            stringBuilder.Append(@" select * from m_MQCQR_Record where QR = @QRMES
	IF NOT EXISTS (SELECT * FROM m_MQCQR_Record (NOLOCK) WHERE QR = @QRMES)
	Begin
			INSERT INTO m_MQCQR_Record(QR,StartDate,EndDate,OutputQty,NGQty,RWQty,TotalQty,LastUpdated,Line,TL01,TL02)
		VALUES (@QRMES, @StartDate, NULL, @OPQty, @NGQty,@RWQty, @OPQty+@NGQty+@RWQty,@StartDate,@Line,NUll,NULL)
	END
	ELSE
	BEGIN
	     Update m_MQCQR_Record set OutputQty = OutputQty+@OPQty,NGQty =NGQty +@NGQty, RWQty = RWQty+@RWQty,
		 TotalQty = TotalQty+@OPQty+@NGQty+@RWQty,LastUpdated = @StartDate where QR = @QRMES
	END
	select @@ROWCOUNT
	select * from m_MQCQR_Record where QR = @QRMES ");

            stringBuilder.Append(" commit ");

            sqlCON sqlCON = new sqlCON();
            sqlCON.sqlExecuteNonQuery(stringBuilder.ToString(), false);
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "UpdateOrInsertQRRecordTable", ex.Message);
            }

        }
    }
}
