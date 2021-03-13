using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using TLMSMESGETDATA.PLC;
using System.Diagnostics;
using TLMSMESGETDATA.PLC2;
using System.Windows.Threading;
using System.Globalization;
using TLMSMESGETDATA.Model;
using System.ComponentModel;
using TLMSMESGETDATA.Algorithm;
using TLMSMESGETDATA.SQLUpload;
using System.Data;

namespace TLMSMESGETDATA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum PLCStatusOperation
    {
        Staring, Stoped, Reset, FinishedLot
    }
    public enum PLCStatus
    {
        Online, Offline
    }
    public partial class MainWindow : Window
    {
        EventBroker.EventObserver m_observerLog = null;

        EventBroker.EventParam m_timerEvent = null;

        FlowDocument m_flowDoc = null;
        System.Threading.Timer tmrEnsureWorkerGetsCalled;



        DispatcherTimer timer = new DispatcherTimer();
        List<MachineItem> ListMachines;
        // this timer calls bgWorker again and again after regular intervals
        System.Windows.Forms.Timer tmrCallBgWorker;
        // this is our worker
        BackgroundWorker bgWorker;
        // this is our worker

        object lockObject = new object();
        List<MachineOperation> machineOperations;

        Stopwatch stopwatch = new Stopwatch();


        Dictionary<string, MQCVariable> DicMQCVariableIP = new Dictionary<string, MQCVariable>();

        System.Windows.Forms.NotifyIcon m_notify = null;
        int CountRefresh = 0;
        public static SettingClass SettingClass = new SettingClass();

        public MainWindow()
        {
            InitializeComponent();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(); //AssemblyVersion을 가져온다.
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
            m_flowDoc = richTextBox.Document;
            m_flowDoc.LineHeight = 2;
            richTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            m_observerLog = new EventBroker.EventObserver(OnReceiveLog);
            EventBroker.AddObserver(EventBroker.EventID.etLog, m_observerLog);
            SystemLog.Output(SystemLog.MSG_TYPE.War, Title, "Started ");
            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Record- Reset", DateTime.Now.ToString("dd-MM-yyyy"));
            machineOperations = new List<MachineOperation>();
            string Qrmes = "s;WO2020110948;BMH1257070S03;1257070S03;400;26/11/2020;;BMH1257070S0320201126;;JO2020111777e";
            string QrId = "TL-10940";
            var ResultValidationQRCode = SubFunction.IsValidationQRCode(Qrmes, QrId);
            //QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat("s;WO2020110948;BMH1257070S03;1257070S03;400;26/11/2020;;BMH1257070S0320201126;;JO2020111777e");
            // QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat("TL-xxxxx");
            //timer.Interval = TimeSpan.FromMilliseconds(100);
            //timer.Tick += timer_Tick;
            //timer.IsEnabled = true;

        }
        #region backgroundworker
        private void LoadBackgroundWorker()
        {   // this timer calls bgWorker again and again after regular intervals
            tmrCallBgWorker = new System.Windows.Forms.Timer();//Timer for do task
            tmrCallBgWorker.Tick += new EventHandler(tmrCallBgWorker_Tick);


            // this is our worker
            bgWorker = new BackgroundWorker();

            // work happens in this method
            bgWorker.DoWork += new DoWorkEventHandler(bg_DoWork);
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bgWorker.WorkerReportsProgress = true;

        }
        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {



        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                CountRefresh++;


                datagridMachines.ItemsSource = machineOperations;
                datagridMachines.Items.Refresh();

                lblReadTime.Text = "Circle-time: " + stopwatch.ElapsedMilliseconds.ToString() + " ms";


                if (CountRefresh == 60)
                {
                    if (SettingClass.PathListProduct != null && SettingClass.PathListProduct != "")
                        ExportListProduct.exportcsvToPLC(SettingClass.PathListProduct);
                    CountRefresh = 0;
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "update UI error", ex.Message);
            }


        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            // does a job like writing to serial communication, webservices etc
            var worker = sender as BackgroundWorker;
            stopwatch = new Stopwatch();
            try
            {

                stopwatch.Start();
                //    if (ListMachines != null)
                {
                    machineOperations = new List<MachineOperation>();
                    //MachineItem machine = new MachineItem();
                    //machine.IP = "172.16.1.145";
                    //machine.Line = "L03";
                    LoadAdress();//load list PLC machine
                    foreach (var machine in ListMachines)
                    {
                        try
                        {
                            //     machine.IP = 
                            MQCVariable mQCOld = new MQCVariable();
                            if (DicMQCVariableIP.ContainsKey(machine.IP))
                                mQCOld = DicMQCVariableIP[machine.IP];


                            MQCVariable mQCPLC = GetMQCVariableRealtime(machine.IP, mQCOld);
                            MQCVariable mQCChanged = new MQCVariable();
                            if (mQCPLC.Connection == 0)
                            {
                                bool isChanged = false;
                                List<string> listChanged = new List<string>();
                                //   if (mQCPLC.DicSPLCtatus[VariablePLC.FlagKT] == true)
                                {
                                    mQCChanged = GetMQCVariableDisCrepancy(mQCPLC, mQCOld, ref isChanged, ref listChanged);
                                    if (DicMQCVariableIP.ContainsKey(machine.IP))
                                        DicMQCVariableIP[machine.IP] = mQCPLC;
                                    else
                                    {
                                        DicMQCVariableIP.Add(machine.IP, mQCPLC);
                                    }
                                    if (listChanged.Contains("Reset"))
                                    {
                                        //MachineOperation operation = new MachineOperation();
                                        //operation.IP = machine.IP;
                                        //operation.Line = machine.Line;
                                        //operation.Lot = qRMQC_MES.PO;
                                        //operation.Inspector = qRIDMES.FullName;
                                        //operation.product = qRMQC_MES.Product;

                                        //operation.Output = mQCPLC.ListMQCQty[0];
                                        //operation.NG = mQCPLC.ListMQCQty[1];
                                        //operation.Rework = mQCPLC.ListMQCQty[2];
                                        //operation.Status = "Reset";
                                        SQLUpload.SQLQRUpdate sQLQRUpdate = new SQLQRUpdate();
                                        sQLQRUpdate.UpdateOrInsertQRRecordTable(mQCOld, machine.Line);

                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "QR MES Reset", mQCOld.QRMES);
                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "QR Inspector", mQCOld.QRID);
                                        if (mQCOld.ListMQCQty.Count() == 3)
                                        {
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Ouput Quantity", mQCOld.ListMQCQty[0].ToString());
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "NG Quantity", mQCOld.ListMQCQty[1].ToString());
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "RW Quantity", mQCOld.ListMQCQty[2].ToString());
                                        }
                                        else
                                        {
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Ouput Quantity: 0", "PLC can't get data from PLC");
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "NG Quantity: 0", "PLC can't get data from PLC");
                                            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "RW Quantity: 0", "PLC can't get data from PLC");
                                        }
                                        return;
                                    }
                                    UploadPLCtoDatabase(mQCChanged, listChanged, machine.Line);

                                    QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(mQCPLC.QRMES);
                                    QRIDMES qRIDMES = QRSpilittoClass.QRstring2IDFormat(mQCPLC.QRID);

                                    MachineOperation operation = new MachineOperation();
                                    operation.QR = mQCPLC.QRMES;
                                    operation.IP = machine.IP;
                                    operation.Line = machine.Line;
                                    operation.Lot = qRMQC_MES.PO;
                                    operation.Inspector = qRIDMES.ID;
                                    operation.product = qRMQC_MES.Product;
                                    if (mQCPLC.ListMQCQty.Count == 3)
                                    {
                                        operation.Output = mQCPLC.ListMQCQty[0];
                                        operation.NG = mQCPLC.ListMQCQty[1];
                                        operation.Rework = mQCPLC.ListMQCQty[2];
                                        operation.Status = "Updating";
                                    }
                                    else if (mQCPLC.ListMQCQty.Count == 0)
                                    {
                                        operation.Output = 0;
                                        operation.NG = 0;
                                        operation.Rework = 0;
                                        operation.Status = "Waiting";
                                    }
                                    //if (listChanged.Contains("Reset"))
                                    //{
                                    //    operation.Status = "Reset";
                                    //    //SQLUpload.SQLQRUpdate sQLQRUpdate = new SQLQRUpdate();
                                    //    //sQLQRUpdate.UpdateOrInsertQRRecordTable(mQCPLC, machine.Line);
                                    //    SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "QR MES Reset", mQCPLC.QRMES);
                                    //    SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "QR Inspector", mQCPLC.QRID);
                                    //    SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Ouput Quantity", mQCPLC.ListMQCQty[0].ToString());
                                    //    SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "NG Quantity", mQCPLC.ListMQCQty[1].ToString());
                                    //    SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "RW Quantity", mQCPLC.ListMQCQty[2].ToString());

                                    //}
                                    //else
                                    //{
                                    //    operation.Status = "Updating";
                                    //}

                                    machineOperations.Add(operation);
                                }
                            }
                            else
                            {
                                MachineOperation operation = new MachineOperation();
                                operation.IP = machine.IP;
                                operation.Line = machine.Line;
                                Sharp7.S7Client client = new Sharp7.S7Client();
                                operation.Status = client.ErrorText(mQCPLC.Connection);
                                machineOperations.Add(operation);
                            }
                        }
                        catch (Exception ex)
                        {

                            SystemLog.Output(SystemLog.MSG_TYPE.Err, "PLC IP : " + machine.IP, ex.Message);
                        }

                    }

                }


                stopwatch.Stop();
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Read PLC fail", ex.Message);
            }

            System.Threading.Thread.Sleep(100);
        }
        public void UploadPLCtoDatabase(MQCVariable mqcchanged, List<string> listChanged, string line)
        {
            try
            {

                var ResultValidationQRCode = SubFunction.IsValidationQRCode(mqcchanged.QRMES, mqcchanged.QRID);
                if (ResultValidationQRCode == 0)
                {

                    UploadLocalPLCDB uploadLocal = new UploadLocalPLCDB();
                    Upload2Mes upload2Mes = new Upload2Mes();
                    if (listChanged.Contains("OPQTY"))
                    {
                        uploadLocal.InsertMQCMESUpdateRealtime(mqcchanged, "OPQTY", line, SettingClass);

                        upload2Mes.IsSendData2MES(mqcchanged, line);
                        //Upload OP QTY change
                    }
                    if (listChanged.Contains("NGQTY"))
                    {
                        uploadLocal.InsertMQCMESUpdateRealtime(mqcchanged, "NGQTY", line, SettingClass);
                        upload2Mes.IsSendData2MES(mqcchanged, line);
                    }
                    if (listChanged.Contains("RWQTY"))
                    {
                        uploadLocal.InsertMQCMESUpdateRealtime(mqcchanged, "RWQTY", line, SettingClass);
                        upload2Mes.IsSendData2MES(mqcchanged, line);
                    }

                    if (listChanged.Contains("Reset"))
                    {
                        uploadLocal.InsertMQCMESUpdateRealtime(mqcchanged, "Reset", line, SettingClass);

                    }
                }
                else
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "QR barcode are wrong format", mqcchanged.QRMES + "|", mqcchanged.QRID);
                }
            }


            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "UploadPLCtoDatabase", ex.Message);
            }  
       
        }
        public MQCVariable GetMQCVariableDisCrepancy(MQCVariable mQCPLC, MQCVariable mQCOld, ref bool Ischanged, ref List<string> typeChange)
        {
            MQCVariable qCVariableChanged = new MQCVariable();
            qCVariableChanged.QRMES = mQCPLC.QRMES;
            qCVariableChanged.QRID = mQCPLC.QRID;
            
            try
            {
                if (mQCPLC.DicSPLCtatus.ContainsKey(VariablePLC.IsReset) && mQCOld.DicSPLCtatus.ContainsKey(VariablePLC.IsReset))
                {
                    if (mQCPLC.DicSPLCtatus[VariablePLC.IsReset] != mQCOld.DicSPLCtatus[VariablePLC.IsReset])
                    {
                        Ischanged = true;
                        typeChange.Add("Reset");

                        return qCVariableChanged;
                    }
                }
                if (mQCPLC.DicSPLCtatus.ContainsKey(VariablePLC.FlagKT) && mQCOld.DicSPLCtatus.ContainsKey(VariablePLC.FlagKT))
                {
                    if (mQCPLC.DicSPLCtatus[VariablePLC.FlagKT] != mQCOld.DicSPLCtatus[VariablePLC.FlagKT])
                {
                    Ischanged = true;
                    typeChange.Add("QRCheck");
                }
                    if (mQCOld.DicSPLCtatus[VariablePLC.FlagKT] == true)
                    {
                        qCVariableChanged.ListMQCQty.Add(0);
                        qCVariableChanged.ListMQCQty.Add(0);
                        qCVariableChanged.ListMQCQty.Add(0);
                        Ischanged = false;
                        typeChange = new List<string>(); ;

                        if (mQCPLC.QRMES != mQCOld.QRMES)
                        {
                            Ischanged = true;
                            typeChange.Add("QR_MES");
                        }
                        if (mQCPLC.QRID != mQCOld.QRID)
                        {
                            Ischanged = true;
                            typeChange.Add("QR_ID");
                        }

                        if (mQCPLC.ListMQCQty[0] > mQCOld.ListMQCQty[0])
                        {
                            qCVariableChanged.ListMQCQty[0] = mQCPLC.ListMQCQty[0] - mQCOld.ListMQCQty[0];
                            Ischanged = true;
                            typeChange.Add("OPQTY");
                        }
                        if (mQCPLC.ListMQCQty[1] > mQCOld.ListMQCQty[1])
                        {
                            qCVariableChanged.ListMQCQty[1] = mQCPLC.ListMQCQty[1] - mQCOld.ListMQCQty[1];
                            for (int i = 0; i < mQCPLC.ListNG38.Count; i++)
                            {
                                qCVariableChanged.ListNG38.Add(mQCPLC.ListNG38[i] - mQCOld.ListNG38[i]);
                            }
                            Ischanged = true;
                            typeChange.Add("NGQTY");
                        }
                        if (mQCPLC.ListMQCQty[2] > mQCOld.ListMQCQty[2])
                        {
                            qCVariableChanged.ListMQCQty[2] = mQCPLC.ListMQCQty[2] - mQCOld.ListMQCQty[2];
                            for (int i = 0; i < mQCPLC.ListRW38.Count; i++)
                            {
                                qCVariableChanged.ListRW38.Add(mQCPLC.ListRW38[i] - mQCOld.ListRW38[i]);
                            }
                            Ischanged = true;
                            typeChange.Add("RWQTY");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "GetMQCVariableDisCrepancy", ex.Message);
            }
            return qCVariableChanged;
        }
 
        void tmrCallBgWorker_Tick(object sender, EventArgs e)
        {
            if (Monitor.TryEnter(lockObject))
            {
                try
                {
                    // if bgworker is not busy the call the worker
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
        void tmrEnsureWorkerGetsCalled_Callback(object obj)
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
        #endregion

        public void LoadAdress()
        {

      
            ListMachines = new List<MachineItem>();
            PLCData pLCData = new PLCData();
       //     ListMachines.Add(new MachineItem { IP = "172.16.1.145", Line = "L03", IsEnable = true });
             ListMachines = pLCData.GetIpMachineRuning();


          
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_timerEvent == null)
            {
                m_timerEvent = new EventBroker.EventParam(this, 0);
                EventBroker.AddTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent, 3960000, true);//66분에 한번씩
                //EventBroker.AddTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent, 20000, true);//66분에 한번씩
            }
            LoadBackgroundWorker();
           // LoadAdress();
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
            m_notify.Icon = Properties.Resources.Proycontec_Robots_Robot_screen_settings;
            m_notify.Visible = true;
            m_notify.DoubleClick += (object send, EventArgs args) => { this.Show(); this.WindowState = WindowState.Normal; this.ShowInTaskbar = true; };
            m_notify.ContextMenu = menu;
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            m_notify.Text = string.Format("PLC To GMES: v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            
            string str = string.Format("PLC To GMES: v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            notiftyBalloonTip(str, 1000);

//            LoadDataMQCStarting(); 
            //  StartTimerGetPLCData();
          



        }

        private void ItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Environment.Exit(0);
            SystemLog.Output(SystemLog.MSG_TYPE.Err, "PLC To GMES", e.ToString());
        }

        private void ItemConfig_Click(object sender, EventArgs e)
        {
            View.ConfigureWindow configureWindow = new View.ConfigureWindow();
            configureWindow.Closed += ConfigureWindow_Closed;
            configureWindow.Show();

        }

        private void ConfigureWindow_Closed(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(SaveObject.Pathsave))
                SettingClass = (SettingClass)SaveObject.Load_data(SaveObject.Pathsave);

        }

        private void notiftyBalloonTip(string message, int showTime, string title = null)
        {
            if (m_notify == null)
                return;
            if (title == null)
                m_notify.BalloonTipTitle = "PLC To GMES";
            else
                m_notify.BalloonTipTitle = title;
            m_notify.BalloonTipText = message;
            m_notify.ShowBalloonTip(showTime);
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState.Minimized.Equals(WindowState))
                this.Hide();
            base.OnStateChanged(e);
        }

        private void OnReceiveLog(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if (param == null)
                return;
            SystemLog.MSG_TYPE type = (SystemLog.MSG_TYPE)param.ParamInt;
            if (type == SystemLog.MSG_TYPE.Err)
                Output(param.ParamString, Brushes.Yellow);
            else if (type == SystemLog.MSG_TYPE.War)
                Output(param.ParamString, Brushes.YellowGreen);
            else
                Output(param.ParamString, Brushes.LightGray);
        }

        private void Output(string msg, Brush brush = null, bool isBold = false)
        {
            if (richTextBox.Dispatcher.Thread == Thread.CurrentThread)
                addMessage(msg, brush, false);
            else
                richTextBox.Dispatcher.BeginInvoke(new Action(delegate { addMessage(msg, brush, false); }));
        }

        private void addMessage(string msg, Brush brush, bool isBold)
        {
            Paragraph newExternalParagraph = new Paragraph();
            newExternalParagraph.Inlines.Add(new Bold(new Run(DateTime.Now.ToString("HH:mm:ss.fff "))));

            if (isBold)
                newExternalParagraph.Inlines.Add(new Bold(new Run(msg/* + Environment.NewLine*/)));
            else
                newExternalParagraph.Inlines.Add(new Run(msg/* + Environment.NewLine*/));

            if (brush == null)
                newExternalParagraph.Foreground = Brushes.White;
            else
                newExternalParagraph.Foreground = brush;
            m_flowDoc.Blocks.Add(newExternalParagraph);
            while (m_flowDoc.Blocks.Count >= 1000)
            {
                m_flowDoc.Blocks.Remove(m_flowDoc.Blocks.FirstBlock);
            }
            if (!richTextBox.IsSelectionActive)
                richTextBox.ScrollToEnd();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmrCallBgWorker.Tick -= new EventHandler(tmrCallBgWorker_Tick);
            bgWorker.DoWork -= new DoWorkEventHandler(bg_DoWork);
            bgWorker.ProgressChanged -= BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);

            if (m_timerEvent != null)
                EventBroker.RemoveTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent);
            EventBroker.RemoveObserver(EventBroker.EventID.etLog, m_observerLog);
            EventBroker.Relase();
            Environment.Exit(0);


        }
        private void StartTimerGetPLCData()
        {
            try
            {
                tmrCallBgWorker.Interval = SettingClass.timmer > 0 ? SettingClass.timmer : 500;
                tmrCallBgWorker.Start();
                btn_connect.Content = "Starting";
                btn_disconnect.Content = "Stop";
                btn_connect.IsEnabled = false;
                btn_disconnect.IsEnabled = true;

                SettingClass = (SettingClass)SaveObject.Load_data(SaveObject.Pathsave);
                if (SettingClass.PLCTimeOut == 0)
                    SettingClass.PLCTimeOut = 3000;
                if (SettingClass.timmer == 0)
                    SettingClass.timmer = 30000;
                //       LoadDataMQCStarting();
            
                //    SettingClass = (SettingClass) SaveObject.Load_data(SaveObject.Pathsave);


            }
            catch (Exception exc)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "get data first test", exc.Message);

            }
        }
        private void Btn_test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                btn_connect.Content = "Starting";
                btn_disconnect.Content = "Stop";
                btn_connect.IsEnabled = false;
                btn_disconnect.IsEnabled = true;
              
                if(System.IO.File.Exists(SaveObject.Pathsave))
                SettingClass = (SettingClass)SaveObject.Load_data(SaveObject.Pathsave);
                if (SettingClass == null)
                    SettingClass = new SettingClass();
                if (SettingClass.PLCTimeOut == 0)
                    SettingClass.PLCTimeOut = 2000;
                if (SettingClass.timmer == 0)
                    SettingClass.timmer = 10000;
                LoadDataMQCStarting();
                tmrCallBgWorker.Interval = SettingClass.timmer > 0 ? SettingClass.timmer : 500;
                tmrCallBgWorker.Start();

                //   LoadAdress();
            
                //    SettingClass = (SettingClass) SaveObject.Load_data(SaveObject.Pathsave);


            }
            catch (Exception exc)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "get data first test", exc.Message);

            }
        }

        private void Btn_disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tmrCallBgWorker.Stop();
                btn_disconnect.Content = "Stopping";
                btn_connect.Content = "Start";
                btn_connect.IsEnabled = true;
                btn_disconnect.IsEnabled = false;

            }
            catch (Exception exc)
            {
                //   MessageBox.Show(exc.Message);
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "disconnect", exc.Message);
            }
        }
        public void LoadDataMQCStarting()
        {
            
            try
            {
                stopwatch.Start();
                LoadAdress();
                if (ListMachines.Count > 0)
                {
                    DicMQCVariableIP = new Dictionary<string, MQCVariable>();

                    foreach (var machine in ListMachines)
                    {
                        int ConnectionPLC = -1;
                        MQCVariable mQCVariable = new MQCVariable();
                        QRMQC_MES qRMQC_MES = new QRMQC_MES();
                        QRIDMES qRIDMES = new QRIDMES();
                        Sharp.ReadVariablePLC pLC = new Sharp.ReadVariablePLC(machine.IP, 0, 0, out ConnectionPLC);
                        mQCVariable.Connection = ConnectionPLC;
                        if (ConnectionPLC == 0)
                        {
                            Dictionary<string, bool> DicStatusPLC = new Dictionary<string, bool>();

                            DicStatusPLC = pLC.ReadStatusPLCMQC();
                            mQCVariable.DicSPLCtatus = DicStatusPLC;

                            if (DicStatusPLC.Count == 4)
                            {
                                if (DicStatusPLC[VariablePLC.FlagKT])
                                {
                                    string QRMES = pLC.ReadAreaByteToString(183, 0, 300);
                                    mQCVariable.QRMES = QRMES;

                                    string QRID = pLC.ReadAreaByteToString(182, 0, 300);
                                    mQCVariable.QRID = QRID;

                                    var ResultValidationQRCode = SubFunction.IsValidationQRCode(QRMES, QRID);
                                    pLC.WriteDinttoPLC(ResultValidationQRCode, 181, 206, 2);//write message error to PLC
                                    if (ResultValidationQRCode == 0)
                                    {
                                        pLC.WritebittoPLC(true, 181, 204, 1);//Write FlagKT to PLC

                                    }
                                    else
                                    {
                                        pLC.WritebittoPLC(false, 181, 204, 1);//Write FlagKT to PLC

                                    }

                                   qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(mQCVariable.QRMES);
                                  //  get values from db and write value PLC
                                    SQLUpload.SQLQRUpdate sQLQR = new SQLQRUpdate();
                                    DataTable dtQRRecord = sQLQR.GetQuanityFromQRMES(mQCVariable.QRMES);

                                    if (dtQRRecord.Rows.Count == 1)
                                    {
                                        var OutputQty = Int16.Parse(dtQRRecord.Rows[0]["OutputQty"].ToString());
                                        var NGQty =Int16.Parse(dtQRRecord.Rows[0]["NGQty"].ToString());
                                        var RWQty = Int16.Parse(dtQRRecord.Rows[0]["RWQty"].ToString());
                                        var Total = Int16.Parse(dtQRRecord.Rows[0]["TotalQty"].ToString());
                                        if (qRMQC_MES.quantity > (OutputQty + NGQty + RWQty) && qRMQC_MES.quantity > Total)
                                        {
                                            pLC.WriteMQCProducedQuantitytoPLC(OutputQty, NGQty, RWQty);
                                        }
                                        else
                                        {
                                            //Write to PLC values to know QR production finished
                                            pLC.WriteDinttoPLC(4, 181, 206, 2);//Completed QR MES
                                        }
                                    }
                                    else if (dtQRRecord.Rows.Count == 0)
                                    {
                                        pLC.WriteMQCProducedQuantitytoPLC(0, 0, 0);
                                    }

                                    List<int> ListMQCQty = pLC.ReadQuantityMQC();
                                    mQCVariable.ListMQCQty = ListMQCQty;
                                    List<int> ListMQCProduced = pLC.ReadQuantityMQCProduced();
                                    mQCVariable.ListQtyProduced = ListMQCProduced;
                                    List<int> ListNG38 = pLC.ReadAreaIntToListInt(3, 4, 76);
                                    mQCVariable.ListNG38 = ListNG38;
                                    List<int> ListRW38 = pLC.ReadAreaIntToListInt(4, 4, 76);
                                    mQCVariable.ListRW38 = ListRW38;


                                }
                            }
                        }
                               
                            DicMQCVariableIP.Add(machine.IP, mQCVariable);
                            pLC.Diconnect();
                        qRMQC_MES = new QRMQC_MES();
                         qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(mQCVariable.QRMES);
                        qRIDMES = QRSpilittoClass.QRstring2IDFormat(mQCVariable.QRID);
                        MachineOperation operation = new MachineOperation();
                        operation.IP = machine.IP;
                        operation.Line = machine.Line;
                        operation.Lot = qRMQC_MES.PO;
                        operation.Inspector = qRIDMES.ID;
                        operation.product = qRMQC_MES.Product;

                        if (mQCVariable.ListMQCQty.Count == 3)
                        {
                            operation.Output = mQCVariable.ListMQCQty[0];
                            operation.NG = mQCVariable.ListMQCQty[1];
                            operation.Rework = mQCVariable.ListMQCQty[2];
                            operation.Status = "Updating";
                        }
                        else if (mQCVariable.ListMQCQty.Count == 0)
                        {
                            operation.Output = 0;
                            operation.NG = 0;
                            operation.Rework = 0;
                            operation.Status = "Waiting";
                        }
                        machineOperations.Add(operation);

                    }
                    datagridMachines.ItemsSource = machineOperations;
                    }
                
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Read PLC fail", ex.Message);
            }
        }

        private MQCVariable GetMQCVariableRealtime(string IP, MQCVariable mQCOld)
        {
            MQCVariable qCVariable = new MQCVariable();
            try
            {
                int ConnectionPLC = -1;
                Sharp.ReadVariablePLC pLC = new Sharp.ReadVariablePLC(IP, 0, 0, out ConnectionPLC);
                qCVariable.Connection = ConnectionPLC;
                if (ConnectionPLC == 0)
                {
                    qCVariable.DicSPLCtatus = pLC.ReadStatusPLCMQC();
                  
                    if (qCVariable.DicSPLCtatus[VariablePLC.FlagKT] == true)
                    {
                        if(qCVariable.DicSPLCtatus[VariablePLC.WriteReadyStart]== false)
                        { 
                            string QRMES = pLC.ReadAreaByteToString(183, 0, 300);
                        qCVariable.QRMES = QRMES.Trim();

                            string QRID = pLC.ReadAreaByteToString(182, 0, 300);
                        qCVariable.QRID = QRID.Trim();

                        var ResultValidationQRCode = SubFunction.IsValidationQRCode(qCVariable.QRMES, qCVariable.QRID);
                         pLC.WriteDinttoPLC(ResultValidationQRCode, 181, 206, 2);//write message error to PLC
                            if (ResultValidationQRCode == 0)
                            {
                                SQLUpload.SQLQRUpdate sQLQR = new SQLQRUpdate();
                                DataTable dtQRRecord = sQLQR.GetQuanityFromQRMES(qCVariable.QRMES);

                                QRMQC_MES qRMQC_MES = QRSpilittoClass.QRstring2MQCFormat(qCVariable.QRMES);
                                if (dtQRRecord.Rows.Count == 1)
                                {
                                    var OutputQty = Int16.Parse(dtQRRecord.Rows[0]["OutputQty"].ToString());
                                    var NGQty = Int16.Parse(dtQRRecord.Rows[0]["NGQty"].ToString());
                                    var RWQty = Int16.Parse(dtQRRecord.Rows[0]["RWQty"].ToString());
                                    var Total = Int16.Parse(dtQRRecord.Rows[0]["TotalQty"].ToString());
                                    if (qRMQC_MES.quantity > (OutputQty + NGQty + RWQty) && qRMQC_MES.quantity > Total)
                                    {
                                        pLC.WriteMQCProducedQuantitytoPLC(OutputQty, NGQty, RWQty);
                                        pLC.WritebittoPLC(true, 181, 204, 1);//Write FlagKT to PLC

                                    }
                                    else
                                    {
                                        //Write to PLC values to know QR production finished
                                        pLC.WriteDinttoPLC(3, 181, 206, 2);//Completed QR MES
                                    }
                                }
                                else if (dtQRRecord.Rows.Count == 0)
                                {
                                    pLC.WriteMQCProducedQuantitytoPLC(0, 0, 0);
                                    pLC.WritebittoPLC(true, 181, 204, 1);//Write FlagKT to PLC

                                }

                            }
                            else
                            {
                                pLC.WritebittoPLC(false, 181, 204, 1);//Write FlagKT to PLC

                            }

                            //get values from db and write value PLC
                         


                            qCVariable.ListMQCQty = pLC.ReadQuantityMQC();
                           
                            qCVariable.ListQtyProduced = pLC.ReadQuantityMQCProduced();
                            
                            qCVariable.ListNG38 = pLC.ReadAreaIntToListInt(3, 4, 76);                       
                            qCVariable.ListRW38 = pLC.ReadAreaIntToListInt(4, 4, 76);


                        }
                        else
                        {
                            
                            qCVariable.QRMES = mQCOld.QRMES;                         
                            qCVariable.QRID = mQCOld.QRID;
                            qCVariable.ListMQCQty = pLC.ReadQuantityMQC();
                            if(qCVariable.ListMQCQty[1] > mQCOld.ListMQCQty[1])
                            {
                                qCVariable.ListNG38 = pLC.ReadAreaIntToListInt(3, 4, 76);

                            }
                            else
                            {
                                qCVariable.ListNG38 = mQCOld.ListNG38;
                            }

                            if (qCVariable.ListMQCQty[2] > mQCOld.ListMQCQty[2])
                            {
                                qCVariable.ListRW38 = pLC.ReadAreaIntToListInt(4, 4, 76);

                            }
                            else
                            {
                                qCVariable.ListRW38 = mQCOld.ListRW38;
                            }
                        }
                    }

                   


                    pLC.Diconnect();
                }
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Get MQC variable realtime fail", ex.Message);
            }
            return qCVariable;
        }
     

       

        private void btn_GetValue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SystemLog.Output(SystemLog.MSG_TYPE.War, "Start read data from PLC", "START");
                stopwatch = new Stopwatch();
                MachineItem machine = new MachineItem();
                machine.IP = "172.16.1.145";
                machine.Line = "L03";
                int ConnectionPLC = -1;
                Sharp.ReadVariablePLC pLC = new Sharp.ReadVariablePLC(machine.IP, 0, 0, out ConnectionPLC);
                MQCVariable mQCVariable = new MQCVariable();
                mQCVariable.DicSPLCtatus = pLC.ReadStatusPLCMQC();
               
                mQCVariable.QRMES = pLC.ReadAreaByteToString(183, 0, 300);
                mQCVariable.QRID = pLC.ReadAreaByteToString(182, 0, 300);
                mQCVariable.ListMQCQty = pLC.ReadQuantityMQC();
                mQCVariable.ListNG38 = pLC.ReadAreaIntToListInt(3, 4, 76);
                mQCVariable.ListRW38 = pLC.ReadAreaIntToListInt(4, 4, 76);
                mQCVariable.ListQtyProduced = pLC.ReadQuantityMQCProduced();
                SystemLog.Output(SystemLog.MSG_TYPE.War, "ConnectionPLC", ConnectionPLC.ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, VariablePLC.FlagKT, mQCVariable.DicSPLCtatus[VariablePLC.FlagKT].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, VariablePLC.IsReset, mQCVariable.DicSPLCtatus[VariablePLC.IsReset].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, VariablePLC.OnOFF, mQCVariable.DicSPLCtatus[VariablePLC.OnOFF].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, VariablePLC.WriteReadyStart, mQCVariable.DicSPLCtatus[VariablePLC.WriteReadyStart].ToString());

                SystemLog.Output(SystemLog.MSG_TYPE.War, "QR MES", mQCVariable.QRMES);
                SystemLog.Output(SystemLog.MSG_TYPE.War, "QR ID", mQCVariable.QRID);

                SystemLog.Output(SystemLog.MSG_TYPE.War, "Output", mQCVariable.ListMQCQty[0].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, "NG", mQCVariable.ListMQCQty[1].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, "RW", mQCVariable.ListMQCQty[2].ToString());

                for (int i = 0; i < mQCVariable.ListNG38.Count; i++)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "NG"+(i+1).ToString(), mQCVariable.ListNG38[i].ToString());
                }
                for (int i = 0; i < mQCVariable.ListRW38.Count; i++)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "RW" + (i + 1).ToString(), mQCVariable.ListRW38[i].ToString());
                }

                SystemLog.Output(SystemLog.MSG_TYPE.War, "Output Produced", mQCVariable.ListQtyProduced[0].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, "NG Produced", mQCVariable.ListQtyProduced [1].ToString());
                SystemLog.Output(SystemLog.MSG_TYPE.War, "RW Produced", mQCVariable.ListQtyProduced[2].ToString());
              


                SystemLog.Output(SystemLog.MSG_TYPE.War, "Start read data from PLC", "END");
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, ex.Source, ex.Message);
            }
        }

      


       
    }
}



