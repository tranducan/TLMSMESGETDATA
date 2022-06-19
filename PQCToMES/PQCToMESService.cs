using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using EFTechlinkMesDb.Interface;
using EFTechlinkMesDb.Implementation;
using EFTechlinkMesDb.Model;
using PQCToMES.MySql;
using System.Configuration;

namespace PQCToMES
{
    public partial class PQCToMESService : ServiceBase
    {
        private int eventId = 1;
        private IDataContextAction DataContext = new DataContextAction();
        private int topQuery = 0;
        private int setInterval = 10000;
        private string mySqlConnection = "";
        public PQCToMESService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("PQCToMES"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "PQCToMES", "PQCApplication");
            }
            eventLog1.Source = "PQCToMES";
            eventLog1.Log = "PQCApplication";

            setInterval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
            topQuery = int.Parse(ConfigurationManager.AppSettings["Topquery"]);
            mySqlConnection = ConfigurationManager.AppSettings["MySqlConnection"];
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart.");
            Timer timer = new Timer();
            timer.Interval = setInterval; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            var pQCDatas = DataContext.SelectTopNotProcess("", topQuery);
            foreach (var item in pQCDatas)
            {
                try
                {
                    Upload2MESMySql upload2MES = new Upload2MESMySql(mySqlConnection);
                    UploadMESRealtime uploadMESRealtime = new UploadMESRealtime(mySqlConnection);


                    if (bool.Parse(ConfigurationManager.AppSettings["IsInsertMQCRealtime"]))
                    {
                        var result = uploadMESRealtime.PushPQCDataToMESRealtime(item);
                        if (result)
                        {
                            var transferResult = DataContext.UpdateFlagTransferingSuccessful(item.PQCMesDataId);
                            if (transferResult)
                            {
                                eventLog1.WriteEntry("Transfering to MES database successful: " + item.POCode, EventLogEntryType.Information);
                            }
                        }
                    }
                    if (bool.Parse(ConfigurationManager.AppSettings["IsInsertPQCData"]))
                    {
                        upload2MES.PushPQCDataToMESSql(item);
                    }
                }
                catch (Exception ex)
                {

                    eventLog1.WriteEntry("Transfering to MES database failed: " + item.POCode + " |message : "+ ex.InnerException, EventLogEntryType.Error);
                }
            }
           
        }
        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }
    }
}
