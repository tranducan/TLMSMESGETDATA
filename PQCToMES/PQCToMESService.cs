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

namespace PQCToMES
{
    public partial class PQCToMESService : ServiceBase
    {
        private int eventId = 1;
        private IDataContextAction DataContext = new DataContextAction();
        public PQCToMESService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart.");
            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            var pQCDatas = DataContext.SelectTop100NotProcess("");
            foreach (var item in pQCDatas)
            {
                try
                {
                    var transferResult = DataContext.UpdateFlagTransferingSuccessful(item.PQCMesDataId);
                    if (transferResult)
                    {
                        eventLog1.WriteEntry("Transfering to MES database successful: " + item.POCode, EventLogEntryType.Information);
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
