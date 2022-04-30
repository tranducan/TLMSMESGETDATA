using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EFTechlinkMesDb;
using EFTechlinkMesDb.Interface;
using EFTechlinkMesDb.Implementation;
using EFTechlinkMesDb.Model;
using Techlink.Common.QRFormat;
using Serilog;
using Techlink.Common.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using ProcessData.PLCSimmens;
using Techlink.Common.Process.PQC;
using Techlink.Common.Shared;

namespace PQCMES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDataContextAction dataContextAction = new DataContextAction();
        private IGetPLCConfig getPLCConfig = new GetPLCConfig();
        private IGetPQCQRRecord getPQCQRRecord = new GetPQCQRRecord();
        List<MachineOperation> machineOperations = new List<MachineOperation>();
        System.Windows.Forms.NotifyIcon m_notify = null;
        Serilog.Core.Logger Logger;
        BackgroundWorker bgWorker;
        System.Windows.Forms.Timer tmrCallBgWorker;
        Stopwatch stopwatch = new Stopwatch();
        object lockObject = new object();
        System.Threading.Timer tmrEnsureWorkerGetsCalled;
        int CountRefresh = 0;
        public static SettingClass settingClass = new SettingClass();
        Dictionary<string, PQCVariables> keyValuePairs = new Dictionary<string, PQCVariables>();
        List<m_ipPLC> listPLC = new List<m_ipPLC>();
        
        public MainWindow()
        {
            InitializeComponent();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_connect.Content = "Starting";
                btn_disconnect.Content = "Stop";
                btn_connect.IsEnabled = false;
                btn_disconnect.IsEnabled = true;
                if (System.IO.File.Exists(SaveObject.Pathsave))
                    settingClass = (SettingClass)SaveObject.Load_data(SaveObject.Pathsave) ?? new SettingClass();
                if (settingClass.PLCTimeOut == 0)
                    settingClass.PLCTimeOut = 2000;
                if (settingClass.timmer == 0)
                    settingClass.timmer = 500;

                tmrCallBgWorker.Interval = settingClass.timmer;
                tmrCallBgWorker.Start();
                LoadDataAtStartingStage();
            }
            catch (Exception ex)
            {
                Logger.Error("Load data PQC in starting stage got a trouble", ex);
            }
        }

        private void btn_disconnect_Click(object sender, RoutedEventArgs e)
        {
                tmrCallBgWorker.Stop();
                btn_disconnect.Content = "Stopping";
                btn_connect.Content = "Start";
                btn_connect.IsEnabled = true;
                btn_disconnect.IsEnabled = false;
        }

        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            //var PLCConfigLoaded = getPLCConfig.GetPLCConfigWithProcessFilter("PQC");
            //datagridMachines.ItemsSource = PLCConfigLoaded;
            var QRTest = QRConverting.GetQRCodeMQCStation("s;B51122010017;BMH1458724S01;5OM8EZYGQ9C1;64;13/02/2022;;BMH1458724S012022212;;JO20220213015513800e");
            var QrMes = "s;B51122010017;BMH1458724S01;5OM8EZYGQ9C1;64;13/02/2022;;BMH1458724S012022212;;JO20220213015513800e";
            var QRTest2 = QRConverting.GetQRCodePQCStation(QrMes);
            var validation = QRValidation.IsValidationQRCodeForPQC(QrMes, "TL-8022");
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string connectionString = @"data source=DESKTOP-M6N0IBR\SQLEXPRESS;initial catalog=ERPSOFT;persist security info=True;user id=dnmdev;password=toluen;MultipleActiveResultSets=True;App=EntityFramework";
            Logger = new LoggerConfiguration()
                .WriteTo.File("C:/logs/myapp.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.MSSqlServer(connectionString, "EntryPQCMesData", schemaName: "Logging")
                .CreateLogger();
            Logger.Information("Initialing completed");

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem itemConfig = new System.Windows.Forms.MenuItem();
            itemConfig.Index = 0;
            itemConfig.Text = "Configure";
            itemConfig.Click += ItemConfig_Click;
            menu.MenuItems.Add(itemConfig);

            System.Windows.Forms.MenuItem itemExit = new System.Windows.Forms.MenuItem();
            itemExit.Index = 1;
            itemExit.Text = "Exit";
            itemExit.Click += ItemExit_Click;
            menu.MenuItems.Add(itemExit);

            m_notify = new System.Windows.Forms.NotifyIcon();
            m_notify.Icon = Properties.Resources.icon2;
            m_notify.Visible = true;
            m_notify.DoubleClick += (object send, EventArgs args) => { this.Show(); this.WindowState = WindowState.Normal; this.ShowInTaskbar = true; };
            m_notify.ContextMenu = menu;
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            m_notify.Text = string.Format("PQC To MES: v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);

            string str = string.Format("PQC To MES: v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            notiftyBalloonTip(str, 1000);

        }

        private void notiftyBalloonTip(string message, int showTime, string title = null)
        {
            if (m_notify == null)
                return;
            if (title == null)
                m_notify.BalloonTipTitle = "PQC To GMES";
            else
                m_notify.BalloonTipTitle = title;
            m_notify.BalloonTipText = message;
            m_notify.ShowBalloonTip(showTime);
        }

        private void ItemExit_Click(object sender, EventArgs e)
        {
            Logger.Information( "PQC To MES is exiting");
            this.Close();
            Environment.Exit(0);
        }

        private void ItemConfig_Click(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBackgroundWorker();
        }

        private void LoadBackgroundWorker()
        {
            tmrCallBgWorker = new System.Windows.Forms.Timer();//Timer for do task
            tmrCallBgWorker.Tick += new EventHandler(tmrCallBgWorker_Tick);
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bg_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bgWorker.WorkerReportsProgress = true;

        }

        private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                CountRefresh++;
                List<string> keyList = new List<string>(keyValuePairs.Keys);
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    MachineOperation machine = new MachineOperation();
                    machine.IP = keyList[i];
                    var pqcVariable = keyValuePairs[machine.IP];
                    if(pqcVariable.Connection == -1)
                    {
                        machine.Output = 0;
                        machine.NG = 0;
                        machine.Rework = 0;
                        machine.Status = "No Connection";
                    }
                    else
                    {
                        if (pqcVariable.ListMQCQty.Count == 3)
                        {
                            machine.Output = pqcVariable.ListMQCQty[0];
                            machine.NG = pqcVariable.ListMQCQty[1];
                            machine.Rework = pqcVariable.ListMQCQty[2];
                            machine.Status = "Waiting";
                        }
                    }
                    machineOperations.Add(machine);
                }

                datagridMachines.ItemsSource = machineOperations;
                datagridMachines.Items.Refresh();

                lblReadTime.Text = "Circle-time: " + stopwatch.ElapsedMilliseconds.ToString() + " ms";


                if (CountRefresh == 60)
                {
                    CountRefresh = 0;
                }
            }
            catch (Exception ex)
            {

                Logger.Error(ex, "Exception at bg_RunWorkerCompleted ");
            }
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                machineOperations = new List<MachineOperation>();
                var listMachines = getPLCConfig.GetPLCConfigWithProcessFilter("PQC", "V3");
                foreach (var machine in listMachines)
                {
                    PQCVariables pQCVariablesOld = new PQCVariables();
                    if (keyValuePairs.ContainsKey(machine.IPPLC))
                        pQCVariablesOld = keyValuePairs[machine.IPPLC];

                    var pqcRealtime = GetPQCVariablesRealtime(machine.IPPLC, pQCVariablesOld);
                    if(pqcRealtime.Connection == 0)
                    {
                        if (keyValuePairs.ContainsKey(machine.IPPLC))
                            keyValuePairs[machine.IPPLC] = pqcRealtime;
                        else
                        {
                            keyValuePairs.Add(machine.IPPLC, pqcRealtime);
                        }
                        bool isChanged = false;
                        List<TypechangedEnum> listChanged = new List<TypechangedEnum>();
                        var pqcChanged = GetVariableDiscrepancy(pqcRealtime, pQCVariablesOld, ref isChanged, ref listChanged);

                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private PQCVariables GetPQCVariablesRealtime(string IP, PQCVariables variablesOld)
        {
            PQCVariables pQCVariables = new PQCVariables();
            try
            {
                int ConnectionPLC = -1; //default
                ReadTags readTags = new ReadTags(IP, 0, 0, out ConnectionPLC);
                WriteTags writeTags = new WriteTags(IP, 0, 0, out ConnectionPLC);
                pQCVariables.Connection = ConnectionPLC;
                if (ConnectionPLC == 0)
                {
                    pQCVariables.DicSPLCtatus = readTags.ReadStatusPLC();
                    if(pQCVariables.DicSPLCtatus[TagSubcribed.FlagKT] == true)
                    {
                        if(pQCVariables.DicSPLCtatus[TagSubcribed.WriteReadyStart] == false)
                        {
                            var qRMes = readTags.ReadAreaByteToString(183, 0, 300);
                            pQCVariables.QRMES = qRMes.Trim();
                            var qRID = readTags.ReadAreaByteToString(182, 0, 300);
                            pQCVariables.QRID = qRID.Trim();
                            var qRvalidation = QRValidation.IsValidationQRCodeForPQC(qRMes, qRID);
                            writeTags.WriteDinttoPLC(qRvalidation, 181, 206, 2);

                            if (qRvalidation == 0)
                            {
                                writeTags.WritebittoPLC(true, 181, 204, 1);
                                writeTags.WriteQRMESto(184, 0, 300, qRMes);
                                var qRCodeMES = QRConverting.GetQRCodePQCStation(qRMes);
                                var qRRecord = getPQCQRRecord.GetPQCQRRecordWithFilterQRcode(qRMes);

                                pQCVariables.ListMQCQty = readTags.ReadQuantity();
                                pQCVariables.ListQtyProduced = readTags.ReadQuantityProduced();
                                pQCVariables.ListNG38 = readTags.ReadAreaIntToListInt(3, 4, 76);
                                pQCVariables.ListRW38 = readTags.ReadAreaIntToListInt(4, 4, 76);

                                if (!(qRRecord is null))
                                {
                                    if(qRCodeMES.quantity > (qRRecord.OutputQty+qRRecord.NGQty+qRRecord.RWQty)
                                        && qRCodeMES.quantity > (qRRecord.TotalQty))
                                    {
                                        writeTags.WriteMQCProducedQuantitytoPLC(
                                            (Int16)qRRecord.OutputQty, (Int16)qRRecord.NGQty, (Int16)qRRecord.RWQty);
                                        writeTags.WritebittoPLC(true, 181, 204, 1);
                                    }
                                    else
                                    {
                                        //Write to PLC values to know QR production finished
                                        writeTags.WriteDinttoPLC(4, 181, 206, 2);
                                    }
                                }
                                else
                                {
                                    writeTags.WriteMQCProducedQuantitytoPLC(0, 0, 0);
                                    writeTags.WritebittoPLC(true, 181, 204, 1);//Write FlagKT to PLC
                                    //23/05/2021 : Add to write QR to PLC
                                    writeTags.WriteQRMESto(184, 0, 300, qRMes);
                                }          
                            }
                            else
                            {
                                writeTags.WritebittoPLC(false, 181, 204, 1);
                            }
                        }
                    }
                    else
                    {
                        pQCVariables = variablesOld;
                    }

                    readTags.Diconnect();
                    writeTags.Diconnect();
                }
                return pQCVariables;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Read data from PLC fail");
                throw ex;
            }
        }

        private PQCVariables GetVariableDiscrepancy(
            PQCVariables pqcReadtime, PQCVariables pqcOld, ref bool ischanged, ref List<TypechangedEnum> typeChange)
        {
            PQCVariables variables = new PQCVariables();
            variables.QRMES = pqcReadtime.QRMES;
            variables.QRID = pqcReadtime.QRID;

            try
            {
                if (pqcReadtime.DicSPLCtatus.ContainsKey(TagSubcribed.IsReset) && pqcOld.DicSPLCtatus.ContainsKey(TagSubcribed.IsReset))
                {
                    if (pqcReadtime.DicSPLCtatus[TagSubcribed.IsReset] != pqcOld.DicSPLCtatus[TagSubcribed.IsReset])
                    {
                        ischanged = true;
                        typeChange.Add(TypechangedEnum.Reset);
                        return variables;
                    }
                }

                if (pqcReadtime.DicSPLCtatus.ContainsKey(TagSubcribed.FlagKT) && pqcOld.DicSPLCtatus.ContainsKey(TagSubcribed.FlagKT))
                {
                    if (pqcReadtime.DicSPLCtatus[TagSubcribed.FlagKT] != pqcOld.DicSPLCtatus[TagSubcribed.FlagKT])
                    {
                        ischanged = true;
                        typeChange.Add(TypechangedEnum.QRchecking);
                        return variables;
                    }

                    if (pqcReadtime.QRMES != pqcOld.QRMES)
                    {
                        ischanged = true;
                        typeChange.Add(TypechangedEnum.QRMesDiscrepancy);
                        return variables;
                    }

                    if (pqcReadtime.QRID != pqcOld.QRID)
                    {
                        ischanged = true;
                        typeChange.Add(TypechangedEnum.QRIdDiscrepancy);
                        return variables;
                    }

                    if (pqcOld.DicSPLCtatus[TagSubcribed.FlagKT])
                    {
                        if (pqcReadtime.ListMQCQty[0] > pqcOld.ListMQCQty[0])
                        {                       
                            variables.ListMQCQty.Add(pqcReadtime.ListMQCQty[0] - pqcOld.ListMQCQty[0]);
                            ischanged = true;
                            typeChange.Add(TypechangedEnum.OutputChanged);
                        }
                        else
                        {
                            variables.ListMQCQty.Add(0); // Add default ouput Qty of variableChanged = 0
                        }

                        if (pqcReadtime.ListMQCQty[1] > pqcOld.ListMQCQty[1])
                        {
                            variables.ListMQCQty.Add(pqcReadtime.ListMQCQty[1] - pqcOld.ListMQCQty[1]);
                            ischanged = true;
                            typeChange.Add(TypechangedEnum.NGChanged);
                        }
                        else
                        {
                            variables.ListMQCQty.Add(0); // Add default NG Qty of variableChanged = 0
                        }

                        if (pqcReadtime.ListMQCQty[2] > pqcOld.ListMQCQty[2])
                        {
                            variables.ListMQCQty.Add(pqcReadtime.ListMQCQty[2] - pqcOld.ListMQCQty[2]);
                            ischanged = true;
                            typeChange.Add(TypechangedEnum.RWChanged);
                        }
                        else
                        {
                            variables.ListMQCQty.Add(0); // Add default RW Qty of variableChanged = 0
                        }

                        return variables;

                    }
                }
            }
            catch (Exception ex)
            {

                Logger.Error("Error happen while comparison between PLC Readtime and PLC old values", ex);
            }
            return variables;
        }

        private void tmrCallBgWorker_Tick(object sender, EventArgs e)
        {
            if (Monitor.TryEnter(lockObject))
            {
                try
                {
                    if (!bgWorker.IsBusy)
                        bgWorker.RunWorkerAsync();
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }

            } 
            else
            {
               // as the bgworker is busy we will start a timer that will try to call the bgworker again after some time
                tmrEnsureWorkerGetsCalled = new System.Threading.Timer(new TimerCallback(tmrEnsureWorkerGetsCalled_Callback), null, 0, 10);
            }
        }

        private void tmrEnsureWorkerGetsCalled_Callback(object obj)
        {
            // this timer was started as the bgworker was busy before now it will try to call the bgworker again
            if (Monitor.TryEnter(lockObject))
            {
                try
                {
                    if (!bgWorker.IsBusy)
                        bgWorker.RunWorkerAsync();
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
                tmrEnsureWorkerGetsCalled = null;
            }
        }

        private void LoadDataAtStartingStage()
        {
            try
            {
                stopwatch.Start();
                listPLC = getPLCConfig.GetPLCConfigWithProcessFilter("PQC", "V3");
                if ( listPLC.Count > 0)
                {
                    keyValuePairs = new Dictionary<string, PQCVariables>();
                    foreach (var machine in listPLC)
                    {
                        PQCVariables pQCVariables = new PQCVariables();
                        ReadTags readTagsPlc = new ReadTags(machine.IPPLC, 0, 0, out var comectiomResult);
                        if(comectiomResult == 0)
                        {
                            var statusPLC = readTagsPlc.ReadStatusPLC();
                            pQCVariables.DicSPLCtatus = statusPLC;
                            if (statusPLC.Count == 4)
                            {
                                if (statusPLC[TagSubcribed.FlagKT])
                                {
                                    var qrMES = readTagsPlc.ReadAreaByteToString(183, 0, 300);
                                    pQCVariables.QRMES = qrMES;
                                    var qrId = readTagsPlc.ReadAreaByteToString(182, 0, 300);
                                    pQCVariables.QRID = qrId;
                                    var validatedQrsCode = QRValidation.IsValidationQRCodeForPQC(qrMES, qrId);

                                    ///Write validation code to PLC, base on the code,
                                    ///PLC will do logic at PLC side
                                    readTagsPlc.WriteDinttoPLC(validatedQrsCode, 181, 206, 2);
                                    if(validatedQrsCode == 0)
                                    {
                                        ///Write ReadyStart Bit to PLC, if true that means PLC is able
                                        ///to start inspection
                                        readTagsPlc.WritebittoPLC(true, 181, 204, 1);
                                        readTagsPlc.WriteQRMESto(184, 0, 300, qrMES);
                                    }
                                    else
                                    {
                                        ///Write ReadyStart Bit to PLC, if false that means PLC is not able
                                        ///to start inspection
                                        readTagsPlc.WritebittoPLC(false, 181, 204, 1);
                                    }

                                    var qrMESConverted = QRConverting.GetQRCodePQCStation(qrMES);

                                    ///Get records of the QR code in ERPSoft database
                                    var qRCodeRecorded = getPQCQRRecord.GetPQCQRRecordWithFilterQRcode(qrMES);
                                    if (qRCodeRecorded is null)
                                    {
                                        readTagsPlc.WriteMQCProducedQuantitytoPLC(0, 0, 0);
                                    }
                                    else
                                    {
                                        if (qrMESConverted.quantity > (qRCodeRecorded.OutputQty + qRCodeRecorded.NGQty + qRCodeRecorded.RWQty)
                                            || qrMESConverted.quantity > qRCodeRecorded.TotalQty)
                                        {
                                            ///Write quanity of QR code recored in database
                                            readTagsPlc.WriteMQCProducedQuantitytoPLC(
                                                (short)qRCodeRecorded.OutputQty,
                                                (short)qRCodeRecorded.NGQty, 
                                                (short)qRCodeRecorded.RWQty);
                                        }
                                        else
                                        {
                                            ///Write to PLC the values to know the QR code is completed. 
                                            ///PLC will stop for the production
                                            readTagsPlc.WriteDinttoPLC(4, 181, 206, 2);
                                        }
                                    }

                                    var listQty = readTagsPlc.ReadQuantity();
                                    pQCVariables.ListMQCQty = listQty;
                                    var listQtyProduced = readTagsPlc.ReadQuantityProduced();
                                    pQCVariables.ListQtyProduced = listQtyProduced;
                                    var list38NG = readTagsPlc.ReadAreaIntToListInt(3, 4, 76);
                                    pQCVariables.ListNG38 = list38NG;
                                    var list38RW = readTagsPlc.ReadAreaIntToListInt(4, 4, 76);
                                    pQCVariables.ListRW38 = list38RW;
                                }
                            }
                        }
                        keyValuePairs.Add(machine.IPPLC, pQCVariables);
                        readTagsPlc.Diconnect();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Error happen while trying to get PQC variables at a first starting", ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmrCallBgWorker.Tick -= new EventHandler(tmrCallBgWorker_Tick);
            bgWorker.DoWork -= new DoWorkEventHandler(bg_DoWork);
            bgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            Environment.Exit(0);
        }
    }
}
