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
using PLCSimenNetWrapper;
using TLMSMESGETDATA.PLC2;
using System.Windows.Threading;
using System.Globalization;
using TLMSMESGETDATA.Model;
using System.ComponentModel;
using TLMSMESGETDATA.Algorithm;
using TLMSMESGETDATA.SQLUpload;

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
        List<Tag> tagsError;
        List<string> ListBarcode;

        List<string> ListError;
        List<Tag> tagsbarcode;
        List<string> ListRework;
        List<Tag> tagsRework;
        List<Tag> tags;

        DispatcherTimer timer = new DispatcherTimer();
        List<MachineItem> ListMachines;
        // this timer calls bgWorker again and again after regular intervals
        System.Windows.Forms.Timer tmrCallBgWorker;
        // this is our worker
        BackgroundWorker bgWorker;
        // this is our worker

        object lockObject = new object();
        List<MachineOperation> machineOperations;
        List<MachineOperation> machineOperationsOld;
        List<Tag> ListTagWrite;
        Stopwatch stopwatch;

        Dictionary<string, DataMQC> keyValuePairsOld = new Dictionary<string, DataMQC>();
        Dictionary<string, DataMQC> keyValuePairsNew = new Dictionary<string, DataMQC>();
        int CountRun = 0;
        System.Windows.Forms.NotifyIcon m_notify = null;
        int CountRefresh = 0;
        SettingClass SettingClass = new SettingClass();

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
            machineOperationsOld = new List<MachineOperation>();
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
                int CountOnline = machineOperations.Where(d => d.Status == ConnectionStates.Online.ToString()).Count();
                int CountOffline = machineOperations.Where(d => d.Status == ConnectionStates.Offline.ToString()).Count();
                lblConnectionState.Text = CountOnline.ToString() + " " + ConnectionStates.Online.ToString() + "|" +
                    CountOffline.ToString() + " " + ConnectionStates.Offline.ToString();
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
                    MachineItem machine = new MachineItem();
                    machine.IP = "172.16.1.145";
                    machine.Line = "L03";
                    //     foreach (var machine in ListMachines)
                    {
                        try
                        {
                            //     machine.IP = 
                            DataMQC MQCIPOld = keyValuePairsOld[machine.IP];
                            DataMQC mQCIP = new DataMQC();
                            mQCIP = GetDataMQCRealtime_QRMES(machine.IP, machine.Line, MQCIPOld);
                            if (mQCIP != null)
                            {
                                Uploaddata uploaddata = new Uploaddata();
                                UploadLocalPLCDB uploadLocalPLCDB = new UploadLocalPLCDB();

                                bool ischange = false;
                                DataMQC mQCIPChanged = new DataMQC();
                                mQCIPChanged = uploaddata.ChangeMQCData(MQCIPOld, mQCIP, out ischange);
                                if (mQCIP.PLC_Barcode != null)
                                {

                                    if (mQCIP.PLC_Barcode.Contains("0010;B01;B01"))
                                    {
                                        if (ischange)
                                        {
                                            if (SettingClass.usingOfftlineServer)
                                            {
                                                DateTime FromDate = DateTime.Now;
                                                var InsertLocal = uploadLocalPLCDB.InsertMQCUpdateRealtime(mQCIPChanged, machine.Line, false, SettingClass);
                                                if (InsertLocal == false)
                                                {
                                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert local data fail", machine.Line);
                                                }
                                                else
                                                {
                                                    keyValuePairsOld[machine.IP] = mQCIP;
                                                    DateTime ToDate = DateTime.Now;
                                                    LocalToServer localToServer = new LocalToServer();
                                                    var Result = localToServer.UploadLocalServertoFactoryDB(mQCIPChanged.PLC_Barcode, FromDate, ToDate, SettingClass);
                                                    if (Result == false)
                                                    {
                                                        SystemLog.Output(SystemLog.MSG_TYPE.War, "insert to local data not success", machine.Line);
                                                    }
                                                }

                                                //var InsertDb = uploaddata.InsertMQCUpdateRealtime(mQCIPChanged, machine.Line, false);
                                                //if (InsertDb == false)
                                                //{
                                                //    SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert remote data fail", machine.Line);
                                                //}
                                            }
                                            else
                                            {
                                                var InsertDb = uploaddata.InsertMQCUpdateRealtime(mQCIPChanged, machine.Line, false);
                                                if (InsertDb == false)
                                                {
                                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert remote data fail", machine.Line);
                                                }
                                                else
                                                {
                                                    keyValuePairsOld[machine.IP] = mQCIP;
                                                }
                                            }

                                        }

                                        var StockAvaiable = uploaddata.QuantityCanRun(mQCIPChanged.PLC_Barcode);
                                        if (StockAvaiable > 0)
                                        {


                                            Plc.Instance.Write("DB151.DBW0", (uint)StockAvaiable);

                                        }
                                        else
                                        {
                                            Plc.Instance.Write("DB151.DBW4", (uint)Math.Abs(StockAvaiable));
                                        }

                                    }
                                    else
                                    {
                                        Plc.Instance.Write("DB151.DBW0", (uint)0);
                                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Line : " + machine.Line, "Barcode Wrong Format- Write stock available = 0 " + mQCIP.PLC_Barcode);
                                    }
                                }
                                else
                                {
                                    Plc.Instance.Write("DB151.DBW0", (uint)0);
                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Line : " + machine.Line, "Barcode == null - Write stock available = 0 " + mQCIP.PLC_Barcode);
                                }
                            }
                            else
                            {
                                SystemLog.Output(SystemLog.MSG_TYPE.War, "Line : " + machine.Line, " DATA MQC = NULL ");
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
            //   ListMachines = pLCData.GetIpMachineRuning();
            ListBarcode = VariablePLC.barcodeaddress();

            tagsbarcode = new List<Tag>();

            ListTagWrite = new List<Tag>();
            Tag tag = new Tag(VariablePLC.AddingAvaiable, "");
            ListTagWrite.Add(tag);
            tag = new Tag(VariablePLC.GapQty, "");
            ListTagWrite.Add(tag);
            foreach (var item in ListBarcode)
            {
                tagsbarcode.Add(new Tag(item, ""));
            }

            ListError = VariablePLC.List38Errors();
            tagsError = new List<Tag>();
            foreach (var item in ListError)
            {
                tagsError.Add(new Tag(item, ""));
            }

            ListRework = VariablePLC.List38Reworks();
            tagsRework = new List<Tag>();
            foreach (var item in ListRework)
            {
                tagsRework.Add(new Tag(item, ""));
            }
            tags = new List<Tag>();

            tag = new Tag(VariablePLC.Good_Products_Total, "");
            tags.Add(tag);
            tag = new Tag(VariablePLC.NG_Products_Total, "");
            tags.Add(tag);
            tag = new Tag(VariablePLC.RW_Products_Total, "");
            tags.Add(tag);
            tag = new Tag(VariablePLC.OnOFF, "");
            tags.Add(tag);
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
            LoadAdress();
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
            m_notify.Text = "PLC To GMES";
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string str = string.Format("PLC To GMES: v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            notiftyBalloonTip(str, 1000);


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
            configureWindow.Show();

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
                CountRun = 1;
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
                LoadDataMQCStarting();
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

                //   LoadAdress();
                CountRun = 1;
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
            stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                if (ListMachines != null)
                {
                    //   foreach (var machine in ListMachines)
                    MachineItem machine = new MachineItem();
                    machine.IP = "172.16.1.145";
                    machine.Line = "L03";
                    {
                        Plc.Instance.Connect(machine.IP, 3000);
                        DataMQC dataMQC = new DataMQC();
                        //  DataMQC dataPLC = new DataMQC();

                        if (Plc.Instance.ConnectionState == ConnectionStates.Online)
                        {// doc barcode truoc
                            List<Tag> TempTag = new List<Tag>();

                            TempTag.Add(new Tag(VariablePLC.FlagKT, ""));
                            TempTag.Add(new Tag(VariablePLC.WriteReadyStart, ""));
                            var ReadTags = Plc.Instance.ReadTags(TempTag);
                            if (ReadTags.Count == 2)
                            {
                                dataMQC.IsChecKQRCode = (bool)ReadTags[0].ItemValue;
                                dataMQC.IsReadyStart = (bool)ReadTags[1].ItemValue;
                            }
                            //      if (dataMQC.IsChecKQRCode == true)
                            {
                                //     if (dataMQC.IsReadyStart == false)
                                {
                                    TempTag = new List<Tag>();
                                    TempTag.Add(new Tag(VariablePLC.NumberCharMESQR, ""));

                                    TempTag.Add(new Tag(VariablePLC.NumberCharIDWorker, ""));
                                    ReadTags = Plc.Instance.ReadTags(TempTag);
                                    if (ReadTags.Count == 2)
                                    {
                                        dataMQC.numberCharQRMES = int.Parse(ReadTags[0].ItemValue.ToString());
                                        dataMQC.numberCharQRID = int.Parse(ReadTags[1].ItemValue.ToString());
                                        List<string> listQRMES = new List<string>();
                                        listQRMES = VariablePLC.barcodeaddressMES();
                                        List<string> listID = new List<string>();
                                        listID = VariablePLC.barcodeaddresID();
                                        List<Tag> listBarcodeMES = new List<Tag>();
                                        List<Tag> listBarcodeID = new List<Tag>();
                                        for (int i = 0; i < dataMQC.numberCharQRMES; i++)
                                        {

                                            listBarcodeMES.Add(new PLCSimenNetWrapper.Tag(listQRMES[i], ""));
                                        }
                                        for (int i = 0; i < dataMQC.numberCharQRID; i++)
                                        {
                                            listBarcodeID.Add(new PLCSimenNetWrapper.Tag(listID[i], ""));
                                        }

                                        var stringMES = Plc.Instance.ReadTagsToString(listBarcodeMES);
                                        var stringID = Plc.Instance.ReadTagsToString(listBarcodeID);
                                        dataMQC.strQRID = stringID;
                                        dataMQC.strQRMES = stringMES;
                                        var ResultValidationQRCode = SubFunction.IsValidationQRCode(stringMES, stringID);
                                        if (ResultValidationQRCode == 0)
                                        {
                                            dataMQC.IsValidQRMQC = true;
                                            dataMQC.IsValidQRID = true;
                                            dataMQC.IsReadyStart = true;
                                            //    Plc.Instance.Write(VariablePLC.WriteReadyStart, true);
                                            Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)0);

                                        }
                                        else if (ResultValidationQRCode == 1)
                                        {
                                            dataMQC.IsValidQRMQC = false;
                                            Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)1);
                                        }
                                        else if (ResultValidationQRCode == 2)
                                        {
                                            dataMQC.IsValidQRID = false;
                                            Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)2);
                                        }
                                        else if (ResultValidationQRCode == 3)
                                        {
                                            dataMQC.IsValidQRID = false;
                                            dataMQC.IsValidQRID = false;
                                            Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)3);
                                        }
                                    }
                                    var ListTag = Plc.Instance.ReadTags(tags);
                                    dataMQC.STARTSTOP = (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                                    dataMQC.Good_Products_Total = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                                    dataMQC.NG_Products_Total = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                                    dataMQC.RW_Products_Total = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                                    //        ma1.ONOFF = (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                                    //        ma1.Status = Plc.Instance.ConnectionState.ToString();
                                }


                                //else
                                //{


                                //    var ListTag = Plc.Instance.ReadTags(tags);

                                //    if (ListTag.Count == 4)
                                //    {
                                //        MachineOperation ma1 = new MachineOperation();
                                //        ma1.Line = line;
                                //        ma1.IP = IP;
                                //        ma1.Output = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                                //        ma1.NG = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                                //        ma1.Rework = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                                //        ma1.ONOFF = (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                                //        ma1.Status = Plc.Instance.ConnectionState.ToString();
                                //        if (dataOld != null)
                                //        {
                                //            #region Running
                                //            if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "False")
                                //            {
                                //                dataMQC = new DataMQC();
                                //                ma1.Lot = dataOld.PLC_Barcode;
                                //                dataMQC.PLC_Barcode = dataOld.PLC_Barcode;
                                //                dataMQC.Good_Products_Total = ma1.Output;
                                //                dataMQC.NG_Products_Total = ma1.NG;
                                //                dataMQC.RW_Products_Total = ma1.Rework;
                                //                dataMQC.STARTSTOP = ma1.ONOFF;

                                //                if (dataOld.PLC_Barcode != null && dataOld.PLC_Barcode.Contains("0010;B01;B01"))
                                //                {
                                //                    dataMQC.NG_Products_NG_ = new int[38];

                                //                    int CountNG = 0;
                                //                    int CountRW = 0;
                                //                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                //                    {
                                //                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                //                        foreach (var item in ListNG)
                                //                        {
                                //                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                //                            CountNG++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                //                    }
                                //                    dataMQC.RW_Products_NG_ = new int[38];
                                //                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                //                    {
                                //                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                //                        foreach (var item in ListRW)
                                //                        {
                                //                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                //                            CountRW++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                //                    }

                                //                }
                                //                else
                                //                {
                                //                    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                    dataMQC.PLC_Barcode = barcode;
                                //                    ma1.Lot = barcode;
                                //                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);
                                //                }

                                //                if (dataMQC.Good_Products_Total == 0 && dataMQC.NG_Products_Total == 0 && dataMQC.RW_Products_Total == 0)
                                //                {
                                //                    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                    dataMQC.PLC_Barcode = barcode;
                                //                    ma1.Lot = barcode;
                                //                }
                                //                machineOperations.Add(ma1);
                                //            }

                                //            #endregion
                                //            #region chuyen giao giua chay va reset
                                //            else if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "True")
                                //            {
                                //                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                ma1.Lot = barcode;


                                //                if (dataOld.PLC_Barcode != null && dataOld.PLC_Barcode.Contains("0010;B01;B01"))
                                //                {
                                //                    dataMQC = new DataMQC();
                                //                    ma1.Lot = barcode;
                                //                    dataMQC.PLC_Barcode = barcode;
                                //                    dataMQC.Good_Products_Total = ma1.Output;
                                //                    dataMQC.NG_Products_Total = ma1.NG;
                                //                    dataMQC.RW_Products_Total = ma1.Rework;
                                //                    dataMQC.STARTSTOP = ma1.ONOFF;


                                //                    dataMQC.NG_Products_NG_ = new int[38];

                                //                    int CountNG = 0;
                                //                    int CountRW = 0;
                                //                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                //                    {
                                //                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                //                        foreach (var item in ListNG)
                                //                        {
                                //                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                //                            CountNG++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                //                    }
                                //                    dataMQC.RW_Products_NG_ = new int[38];
                                //                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                //                    {
                                //                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                //                        foreach (var item in ListRW)
                                //                        {
                                //                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                //                            CountRW++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                //                    }


                                //                    Uploaddata uploaddata = new Uploaddata();
                                //                    uploaddata.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0);
                                //                    if (SettingClass.usingOfftlineServer)
                                //                    {
                                //                        UploadLocalPLCDB uploadLocalPLCDB = new UploadLocalPLCDB();
                                //                        uploadLocalPLCDB.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0, SettingClass);
                                //                    }


                                //                }
                                //                else
                                //                {

                                //                    dataMQC = dataOld;
                                //                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                //                }
                                //                try
                                //                {
                                //                    if (dataOld != null)
                                //                    {
                                //                        Uploaddata uploaddata = new Uploaddata();
                                //                        string model = uploaddata.GetModelFromLot(dataOld.PLC_Barcode);
                                //                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " Reset ");
                                //                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " OP " + dataOld.Good_Products_Total);
                                //                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " NG " + dataOld.NG_Products_Total);
                                //                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " RW " + dataOld.RW_Products_Total);
                                //                    }
                                //                }
                                //                catch (Exception ex)
                                //                {

                                //                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Write record reset not succesfull" + " Line: " + line + " QR code: " + dataOld.PLC_Barcode, ex.Message);
                                //                }
                                //                machineOperations.Add(ma1);
                                //            }

                                //            #endregion
                                //            else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "True")
                                //            {

                                //                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                ma1.Lot = barcode;
                                //                machineOperations.Add(ma1);
                                //                dataMQC = new DataMQC();
                                //                dataMQC.STARTSTOP = ma1.ONOFF;
                                //                dataMQC.PLC_Barcode = barcode;

                                //            }
                                //            else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "False")
                                //            {
                                //                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                ma1.Lot = barcode;
                                //                machineOperations.Add(ma1);
                                //                dataMQC = new DataMQC();
                                //                dataMQC.STARTSTOP = ma1.ONOFF;
                                //                dataMQC.PLC_Barcode = barcode;

                                //            }
                                //            else
                                //            {
                                //                if (dataOld.STARTSTOP == null)
                                //                {
                                //                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "dataOld.STARTSTOP == null", ma1.Lot);
                                //                }
                                //                else if (ma1.ONOFF == null)
                                //                {
                                //                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "ma1.ONOFF == null", ma1.Lot);
                                //                }
                                //                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //                ma1.Lot = barcode;
                                //                machineOperations.Add(ma1);

                                //                if (barcode != null && barcode.Contains("0010;B01;B01"))
                                //                {
                                //                    dataMQC = new DataMQC();
                                //                    ma1.Lot = barcode;
                                //                    dataMQC.PLC_Barcode = barcode;
                                //                    dataMQC.Good_Products_Total = ma1.Output;
                                //                    dataMQC.NG_Products_Total = ma1.NG;
                                //                    dataMQC.RW_Products_Total = ma1.Rework;
                                //                    dataMQC.STARTSTOP = ma1.ONOFF;
                                //                    dataMQC.NG_Products_NG_ = new int[38];
                                //                    int CountNG = 0;
                                //                    int CountRW = 0;
                                //                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                //                    {
                                //                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                //                        foreach (var item in ListNG)
                                //                        {
                                //                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                //                            CountNG++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                //                    }
                                //                    dataMQC.RW_Products_NG_ = new int[38];
                                //                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                //                    {
                                //                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                //                        foreach (var item in ListRW)
                                //                        {
                                //                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                //                            CountRW++;
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                //                    }
                                //                }
                                //                else
                                //                {

                                //                    dataMQC = dataOld;
                                //                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                //                }
                                //            }
                                //        }
                                //        else
                                //        {
                                //            dataMQC = new DataMQC();
                                //            var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                //            ma1.Lot = barcode;
                                //            dataMQC.STARTSTOP = ma1.ONOFF;
                                //            dataMQC.PLC_Barcode = barcode;
                                //            dataMQC.Good_Products_Total = ma1.Output;
                                //            dataMQC.NG_Products_Total = ma1.NG;
                                //            dataMQC.RW_Products_Total = ma1.Rework;
                                //            dataMQC.STARTSTOP = ma1.ONOFF;
                                //            machineOperations.Add(ma1);
                                //            SystemLog.Output(SystemLog.MSG_TYPE.War, "Data old = null: IPMachine :", IP);
                                //        }


                                //    }
                                //    else
                                //    {
                                //        MachineOperation ma1 = new MachineOperation();
                                //        ma1.Line = line;
                                //        ma1.IP = IP;

                                //        ma1.Status = Plc.Instance.ConnectionState.ToString();
                                //        machineOperations.Add(ma1);
                                //        dataMQC = dataOld;
                                //        SystemLog.Output(SystemLog.MSG_TYPE.War, "Readtag fail: IPMachine ", IP);
                                //    }


                                //}
                            }
                            //else
                            //{
                            //    MachineOperation ma1 = new MachineOperation();
                            //    ma1.Line = line;
                            //    ma1.IP = IP;
                            //    ma1.Status = "Waiting";
                            //    machineOperations.Add(ma1);
                            //    dataMQC = dataPLC;
                            //    SystemLog.Output(SystemLog.MSG_TYPE.War, "Machine is offline", IP);
                            //}
                        }





                        //if (Plc.Instance.ConnectionState == ConnectionStates.Online)
                        //{// doc barcode truoc

                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                        //  //  var bar = Plc.Instance.R

                        //        dataMQC = new DataMQC();
                        //        dataMQC.PLC_Barcode = barcode;


                        //        var ListTag = Plc.Instance.ReadTags(tags);
                        //        if (ListTag != null && ListTag.Count == 4)
                        //        {
                        //            MachineOperation ma1 = new MachineOperation();
                        //            ma1.Line = machine.Line;
                        //            ma1.IP = machine.IP;
                        //            ma1.Output = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                        //            ma1.NG = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                        //            ma1.Rework = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                        //            ma1.ONOFF= (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                        //            ma1.Status = Plc.Instance.ConnectionState.ToString();
                        //            ma1.Lot = barcode;
                        //            dataMQC.DateTimeReset = DateTime.Now;
                        //            dataMQC.Good_Products_Total = ma1.Output;
                        //            dataMQC.NG_Products_Total = ma1.NG;
                        //            dataMQC.RW_Products_Total = ma1.Rework;
                        //            dataMQC.STARTSTOP = ma1.ONOFF;

                        //            dataMQC.NG_Products_NG_ = new int[38];
                        //            int CountNG = 0;
                        //            int CountRW = 0;
                        //            machineOperations.Add(ma1);
                        //            if (dataMQC.NG_Products_Total > 0)
                        //            {
                        //                var ListNG = Plc.Instance.ReadTags(tagsError);

                        //                foreach (var item in ListNG)
                        //                {
                        //                    dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                        //                    CountNG ++;
                        //                }
                        //            }
                        //            dataMQC.RW_Products_NG_ = new int[38];
                        //            if (dataMQC.RW_Products_Total > 0)
                        //            {
                        //                var ListRW = Plc.Instance.ReadTags(tagsRework);

                        //                foreach (var item in ListRW)
                        //                {
                        //                    dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                        //                    CountRW++;
                        //                }
                        //            }


                        //    }
                        //    else
                        //    {
                        //        SystemLog.Output(SystemLog.MSG_TYPE.War, "PLC readtag fail :", machine.IP);
                        //    }
                        //    if (dataMQC != null)
                        //    {
                        //        UploadLocalPLCDB uploadLocalPLCDB = new UploadLocalPLCDB();
                        //        if (dataMQC.PLC_Barcode.Contains("0010;B01;B01"))
                        //        {
                        //            Uploaddata uploaddata = new Uploaddata();
                        //            if (CountRun == 0 && cb_GetFirstValues.IsChecked == true)
                        //            {
                        //                if (SettingClass.usingOfftlineServer)
                        //                {
                        //                    DateTime FromDate = DateTime.Now;
                        //                    var InsertLocal = uploadLocalPLCDB.InsertMQCUpdateRealtime(dataMQC, machine.Line, false, SettingClass);
                        //                    if (InsertLocal == false)
                        //                    {
                        //                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Insert local data fail", "");
                        //                    }
                        //                    else
                        //                    {
                        //                        DateTime ToDate = DateTime.Now;
                        //                        LocalToServer localToServer = new LocalToServer();
                        //                        var Result = localToServer.UploadLocalServertoFactoryDB(dataMQC.PLC_Barcode, FromDate, ToDate, SettingClass);
                        //                        if (Result == false)
                        //                        {
                        //                            SystemLog.Output(SystemLog.MSG_TYPE.War, "insert to local data not success", machine.Line);
                        //                        }
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                   var InsertResult = uploaddata.InsertMQCUpdateRealtime(dataMQC, machine.Line, false);
                        //                    if(InsertResult ==false)
                        //                    {
                        //                        SystemLog.Output(SystemLog.MSG_TYPE.War, "InsertMQCUpdateRealtime", "");
                        //                    }
                        //                }
                        //            }

                        //            var StockAvaiable = uploaddata.QuantityCanRun(dataMQC.PLC_Barcode);


                        //            if (StockAvaiable > 0)
                        //            {

                        //                Plc.Instance.Write("DB151.DBW0", (uint)StockAvaiable);

                        //            }
                        //            else
                        //            {

                        //                Plc.Instance.Write("DB151.DBW4", (uint)Math.Abs(StockAvaiable));

                        //            }
                        //        }
                        //        else
                        //        {
                        //            SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode wrong format :", dataMQC.PLC_Barcode);
                        //        }

                        //    }
                        //    else
                        //    {
                        //        SystemLog.Output(SystemLog.MSG_TYPE.War, "Data MQC = null :", machine.IP);
                        //    }
                        //}
                        else
                        {
                            MachineOperation ma1 = new MachineOperation();
                            ma1.Line = machine.Line;
                            ma1.IP = machine.IP;
                            ma1.Status = Plc.Instance.ConnectionState.ToString();
                            machineOperations.Add(ma1);
                            SystemLog.Output(SystemLog.MSG_TYPE.War, "machine is not online", machine.IP);
                        }

                        keyValuePairsOld.Add(machine.IP, dataMQC);

                    }
                    datagridMachines.ItemsSource = machineOperations;
                }
                else
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "list machine = null", "");
                }
                stopwatch.Stop();

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Read PLC fail", ex.Message);
            }
        }

        private DataMQC GetDataMQCRealtime(string IP, string line, DataMQC dataOld)
        {
            DataMQC dataMQC = null;
            try
            {
                Plc.Instance.Connect(IP, SettingClass.PLCTimeOut);

                if (Plc.Instance.ConnectionState == ConnectionStates.Online)
                {// doc barcode truoc

                    var ListTag = Plc.Instance.ReadTags(tags);

                    if (ListTag.Count == 4)
                    {
                        MachineOperation ma1 = new MachineOperation();
                        ma1.Line = line;
                        ma1.IP = IP;
                        ma1.Output = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                        ma1.NG = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                        ma1.Rework = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                        ma1.ONOFF = (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                        ma1.Status = Plc.Instance.ConnectionState.ToString();
                        if (dataOld != null)
                        {
                            #region Running
                            if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "False")
                            {
                                dataMQC = new DataMQC();
                                ma1.Lot = dataOld.PLC_Barcode;
                                dataMQC.PLC_Barcode = dataOld.PLC_Barcode;
                                dataMQC.Good_Products_Total = ma1.Output;
                                dataMQC.NG_Products_Total = ma1.NG;
                                dataMQC.RW_Products_Total = ma1.Rework;
                                dataMQC.STARTSTOP = ma1.ONOFF;

                                if (dataOld.PLC_Barcode != null && dataOld.PLC_Barcode.Contains("0010;B01;B01"))
                                {
                                    dataMQC.NG_Products_NG_ = new int[38];

                                    int CountNG = 0;
                                    int CountRW = 0;
                                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                    {
                                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                        foreach (var item in ListNG)
                                        {
                                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                            CountNG++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                    }
                                    dataMQC.RW_Products_NG_ = new int[38];
                                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                    {
                                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                        foreach (var item in ListRW)
                                        {
                                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                            CountRW++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                    }

                                }
                                else
                                {
                                    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                    dataMQC.PLC_Barcode = barcode;
                                    ma1.Lot = barcode;
                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);
                                }

                                if (dataMQC.Good_Products_Total == 0 && dataMQC.NG_Products_Total == 0 && dataMQC.RW_Products_Total == 0)
                                {
                                    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                    dataMQC.PLC_Barcode = barcode;
                                    ma1.Lot = barcode;
                                }
                                machineOperations.Add(ma1);
                            }

                            #endregion
                           #region chuyen giao giua chay va reset
                            else if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "True")
                            {
                                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                ma1.Lot = barcode;


                                if (dataOld.PLC_Barcode != null && dataOld.PLC_Barcode.Contains("0010;B01;B01"))
                                {
                                    dataMQC = new DataMQC();
                                    ma1.Lot = barcode;
                                    dataMQC.PLC_Barcode = barcode;
                                    dataMQC.Good_Products_Total = ma1.Output;
                                    dataMQC.NG_Products_Total = ma1.NG;
                                    dataMQC.RW_Products_Total = ma1.Rework;
                                    dataMQC.STARTSTOP = ma1.ONOFF;


                                    dataMQC.NG_Products_NG_ = new int[38];

                                    int CountNG = 0;
                                    int CountRW = 0;
                                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                    {
                                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                        foreach (var item in ListNG)
                                        {
                                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                            CountNG++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                    }
                                    dataMQC.RW_Products_NG_ = new int[38];
                                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                    {
                                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                        foreach (var item in ListRW)
                                        {
                                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                            CountRW++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                    }


                                    Uploaddata uploaddata = new Uploaddata();
                                    uploaddata.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0);
                                    if (SettingClass.usingOfftlineServer)
                                    {
                                        UploadLocalPLCDB uploadLocalPLCDB = new UploadLocalPLCDB();
                                        uploadLocalPLCDB.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0, SettingClass);
                                    }


                                }
                                else
                                {

                                    dataMQC = dataOld;
                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                }
                                try
                                {
                                    if (dataOld != null)
                                    {
                                        Uploaddata uploaddata = new Uploaddata();
                                        string model = uploaddata.GetModelFromLot(dataOld.PLC_Barcode);
                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " Reset ");
                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " OP " + dataOld.Good_Products_Total);
                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " NG " + dataOld.NG_Products_Total);
                                        SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " RW " + dataOld.RW_Products_Total);
                                    }
                                }
                                catch (Exception ex)
                                {

                                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Write record reset not succesfull" + " Line: " + line + " QR code: " + dataOld.PLC_Barcode, ex.Message);
                                }
                                machineOperations.Add(ma1);
                            }

                            #endregion
                            else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "True")
                            {

                                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                ma1.Lot = barcode;
                                machineOperations.Add(ma1);
                                dataMQC = new DataMQC();
                                dataMQC.STARTSTOP = ma1.ONOFF;
                                dataMQC.PLC_Barcode = barcode;

                            }
                            else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "False")
                            {
                                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                ma1.Lot = barcode;
                                machineOperations.Add(ma1);
                                dataMQC = new DataMQC();
                                dataMQC.STARTSTOP = ma1.ONOFF;
                                dataMQC.PLC_Barcode = barcode;

                            }
                            else
                            {
                                if (dataOld.STARTSTOP == null)
                                {
                                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "dataOld.STARTSTOP == null", ma1.Lot);
                                }
                                else if (ma1.ONOFF == null)
                                {
                                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "ma1.ONOFF == null", ma1.Lot);
                                }
                                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                ma1.Lot = barcode;
                                machineOperations.Add(ma1);

                                if (barcode != null && barcode.Contains("0010;B01;B01"))
                                {
                                    dataMQC = new DataMQC();
                                    ma1.Lot = barcode;
                                    dataMQC.PLC_Barcode = barcode;
                                    dataMQC.Good_Products_Total = ma1.Output;
                                    dataMQC.NG_Products_Total = ma1.NG;
                                    dataMQC.RW_Products_Total = ma1.Rework;
                                    dataMQC.STARTSTOP = ma1.ONOFF;
                                    dataMQC.NG_Products_NG_ = new int[38];
                                    int CountNG = 0;
                                    int CountRW = 0;
                                    if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                    {
                                        var ListNG = Plc.Instance.ReadTags(tagsError);

                                        foreach (var item in ListNG)
                                        {
                                            dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                            CountNG++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                    }
                                    dataMQC.RW_Products_NG_ = new int[38];
                                    if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                    {
                                        var ListRW = Plc.Instance.ReadTags(tagsRework);

                                        foreach (var item in ListRW)
                                        {
                                            dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                            CountRW++;
                                        }
                                    }
                                    else
                                    {
                                        dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                    }
                                }
                                else
                                {

                                    dataMQC = dataOld;
                                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                }
                            }
                        }
                        else
                        {
                            dataMQC = new DataMQC();
                            var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                            ma1.Lot = barcode;
                            dataMQC.STARTSTOP = ma1.ONOFF;
                            dataMQC.PLC_Barcode = barcode;
                            dataMQC.Good_Products_Total = ma1.Output;
                            dataMQC.NG_Products_Total = ma1.NG;
                            dataMQC.RW_Products_Total = ma1.Rework;
                            dataMQC.STARTSTOP = ma1.ONOFF;
                            machineOperations.Add(ma1);
                            SystemLog.Output(SystemLog.MSG_TYPE.War, "Data old = null: IPMachine :", IP);
                        }


                    }
                    else
                    {
                        MachineOperation ma1 = new MachineOperation();
                        ma1.Line = line;
                        ma1.IP = IP;

                        ma1.Status = Plc.Instance.ConnectionState.ToString();
                        machineOperations.Add(ma1);
                        dataMQC = dataOld;
                        SystemLog.Output(SystemLog.MSG_TYPE.War, "Readtag fail: IPMachine ", IP);
                    }

                }
                else
                {
                    MachineOperation ma1 = new MachineOperation();
                    ma1.Line = line;
                    ma1.IP = IP;
                    ma1.Status = Plc.Instance.ConnectionState.ToString();
                    machineOperations.Add(ma1);
                    dataMQC = dataOld;
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Machine is offline", IP);
                }

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "DataMQC GetDataMQCRealtime line " + line, ex.Message);
            }

            return dataMQC;
        }

        private DataMQC GetDataMQCRealtime_QRMES(string IP, string line, DataMQC dataOld)
        {
            DataMQC dataMQC = new DataMQC();
            DataMQC dataPLC = new DataMQC();
            try
            {
                Plc.Instance.Connect(IP, 3000);

                if (Plc.Instance.ConnectionState == ConnectionStates.Online)
                {// doc barcode truoc
                    List<Tag> TempTag = new List<Tag>();

                    TempTag.Add(new Tag(VariablePLC.FlagKT, ""));
                    TempTag.Add(new Tag(VariablePLC.WriteReadyStart, ""));
                    TempTag.Add(new Tag(VariablePLC.IsReset, ""));
                    var ReadTags = Plc.Instance.ReadTags(TempTag);
                    if (ReadTags.Count == 3)
                    {
                        dataPLC.IsChecKQRCode = (bool)ReadTags[0].ItemValue;
                        dataPLC.IsReadyStart = (bool)ReadTags[1].ItemValue;
                        dataPLC.IsReset = (bool)ReadTags[2].ItemValue;
                    }
                    if (dataPLC.IsChecKQRCode == true)
                    {
                        if (dataPLC.IsReadyStart == false)
                        {
                            TempTag = new List<Tag>();
                            TempTag.Add(new Tag(VariablePLC.NumberCharMESQR, ""));

                            TempTag.Add(new Tag(VariablePLC.NumberCharIDWorker, ""));
                            ReadTags = Plc.Instance.ReadTags(TempTag);
                            if (ReadTags.Count == 2)
                            {
                                dataPLC.numberCharQRMES = int.Parse(ReadTags[0].ItemValue.ToString());
                                dataPLC.numberCharQRID = int.Parse(ReadTags[1].ItemValue.ToString());
                                List<string> listQRMES = new List<string>();
                                listQRMES = VariablePLC.barcodeaddressMES();
                                List<string> listID = new List<string>();
                                listID = VariablePLC.barcodeaddresID();
                                List<Tag> listBarcodeMES = new List<Tag>();
                                List<Tag> listBarcodeID = new List<Tag>();
                                for (int i = 0; i < dataPLC.numberCharQRMES; i++)
                                {

                                    listBarcodeMES.Add(new PLCSimenNetWrapper.Tag(listQRMES[i], ""));
                                }
                                for (int i = 0; i < dataPLC.numberCharQRID; i++)
                                {
                                    listBarcodeID.Add(new PLCSimenNetWrapper.Tag(listID[i], ""));
                                }

                                var stringMES = Plc.Instance.ReadTagsToString(listBarcodeMES);
                                var stringID = Plc.Instance.ReadTagsToString(listBarcodeID);
                                dataPLC.strQRMES = stringMES;
                                dataPLC.strQRID = stringID;
                                var ResultValidationQRCode = SubFunction.IsValidationQRCode(stringMES, stringID);
                                if (ResultValidationQRCode == 0)
                                {
                                    dataPLC.IsValidQRMQC = true;
                                    dataPLC.IsValidQRID = true;
                                    dataPLC.IsReadyStart = true;
                                    Plc.Instance.Write(VariablePLC.WriteReadyStart, true);
                                    Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)0);

                                }
                                else if (ResultValidationQRCode == 1)
                                {
                                    dataPLC.IsValidQRMQC = false;
                                    Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)1);
                                }
                                else if (ResultValidationQRCode == 2)
                                {
                                    dataPLC.IsValidQRID = false;
                                    Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)2);
                                }
                                else if (ResultValidationQRCode == 3)
                                {
                                    dataPLC.IsValidQRID = false;
                                    dataPLC.IsValidQRID = false;
                                    Plc.Instance.Write(VariablePLC.WriteMessage, (Int16)3);
                                }
                            }
                            return dataPLC;

                        }


                        else
                        {


                            var ListTag = Plc.Instance.ReadTags(tags);

                            if (ListTag.Count == 4)
                            {
                                MachineOperation ma1 = new MachineOperation();
                                ma1.Line = line;
                                ma1.IP = IP;
                                ma1.Output = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                                ma1.NG = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                                ma1.Rework = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                                ma1.ONOFF = (ListTag[3].ItemValue.ToString() != null) ? ListTag[3].ItemValue.ToString() : "";
                                ma1.Status = Plc.Instance.ConnectionState.ToString();
                                if (dataOld != null)
                                {
                                    #region Running
                                    // if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "False")
                                    {
                                        dataMQC = new DataMQC();
                                        ma1.Lot = dataOld.strQRMES;
                                        dataMQC.PLC_Barcode = dataOld.strQRMES;
                                        dataMQC.Good_Products_Total = ma1.Output;
                                        dataMQC.NG_Products_Total = ma1.NG;
                                        dataMQC.RW_Products_Total = ma1.Rework;
                                        dataMQC.STARTSTOP = ma1.ONOFF;

                                        if (dataOld.strQRMES != null)
                                        {
                                            dataMQC.NG_Products_NG_ = new int[38];

                                            int CountNG = 0;
                                            int CountRW = 0;
                                            if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                            {
                                                var ListNG = Plc.Instance.ReadTags(tagsError);

                                                foreach (var item in ListNG)
                                                {
                                                    dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                                    CountNG++;
                                                }
                                            }
                                            else
                                            {
                                                dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                            }
                                            dataMQC.RW_Products_NG_ = new int[38];
                                            if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                            {
                                                var ListRW = Plc.Instance.ReadTags(tagsRework);

                                                foreach (var item in ListRW)
                                                {
                                                    dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                                    CountRW++;
                                                }
                                            }
                                            else
                                            {
                                                dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                            }

                                        }
                                        else
                                        {
                                            var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                            dataMQC.PLC_Barcode = barcode;
                                            ma1.Lot = barcode;
                                            SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);
                                        }

                                        //if (dataMQC.Good_Products_Total == 0 && dataMQC.NG_Products_Total == 0 && dataMQC.RW_Products_Total == 0)
                                        //{
                                        //    //var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    //dataMQC.PLC_Barcode = barcode;
                                        //    //ma1.Lot = barcode;
                                        //}
                                        machineOperations.Add(ma1);
                                        //    }

                                        #endregion
                                    //    #region chuyen giao giua chay va reset
                                        //else if (dataOld.STARTSTOP == "False" && ma1.ONOFF == "True")
                                        //{
                                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    ma1.Lot = barcode;


                                        //    if (dataOld.PLC_Barcode != null && dataOld.PLC_Barcode.Contains("0010;B01;B01"))
                                        //    {
                                        //        dataMQC = new DataMQC();
                                        //        ma1.Lot = barcode;
                                        //        dataMQC.PLC_Barcode = barcode;
                                        //        dataMQC.Good_Products_Total = ma1.Output;
                                        //        dataMQC.NG_Products_Total = ma1.NG;
                                        //        dataMQC.RW_Products_Total = ma1.Rework;
                                        //        dataMQC.STARTSTOP = ma1.ONOFF;


                                        //        dataMQC.NG_Products_NG_ = new int[38];

                                        //        int CountNG = 0;
                                        //        int CountRW = 0;
                                        //        if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                        //        {
                                        //            var ListNG = Plc.Instance.ReadTags(tagsError);

                                        //            foreach (var item in ListNG)
                                        //            {
                                        //                dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                        //                CountNG++;
                                        //            }
                                        //        }
                                        //        else
                                        //        {
                                        //            dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                        //        }
                                        //        dataMQC.RW_Products_NG_ = new int[38];
                                        //        if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                        //        {
                                        //            var ListRW = Plc.Instance.ReadTags(tagsRework);

                                        //            foreach (var item in ListRW)
                                        //            {
                                        //                dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                        //                CountRW++;
                                        //            }
                                        //        }
                                        //        else
                                        //        {
                                        //            dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                        //        }


                                        //        Uploaddata uploaddata = new Uploaddata();
                                        //        uploaddata.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0);
                                        //        if (SettingClass.usingOfftlineServer)
                                        //        {
                                        //            UploadLocalPLCDB uploadLocalPLCDB = new UploadLocalPLCDB();
                                        //            uploadLocalPLCDB.InsertToMQC_Realtime(dataOld.PLC_Barcode, line, "", "0", "Reset", 0, SettingClass);
                                        //        }


                                        //    }
                                        //    else
                                        //    {

                                        //        dataMQC = dataOld;
                                        //        SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                        //    }
                                        //    try
                                        //    {
                                        //        if (dataOld != null)
                                        //        {
                                        //            Uploaddata uploaddata = new Uploaddata();
                                        //            string model = uploaddata.GetModelFromLot(dataOld.PLC_Barcode);
                                        //            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " Reset ");
                                        //            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " OP " + dataOld.Good_Products_Total);
                                        //            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " NG " + dataOld.NG_Products_Total);
                                        //            SystemRecord.Output(SystemRecord.MSG_TYPE.Nor, "Line: " + line + " QR code: " + dataOld.PLC_Barcode + " Model: " + model, " RW " + dataOld.RW_Products_Total);
                                        //        }
                                        //    }
                                        //    catch (Exception ex)
                                        //    {

                                        //        SystemLog.Output(SystemLog.MSG_TYPE.Err, "Write record reset not succesfull" + " Line: " + line + " QR code: " + dataOld.PLC_Barcode, ex.Message);
                                        //    }
                                        //    machineOperations.Add(ma1);
                                        //}

                                        //#endregion
                                        //else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "True")
                                        //{

                                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    ma1.Lot = barcode;
                                        //    machineOperations.Add(ma1);
                                        //    dataMQC = new DataMQC();
                                        //    dataMQC.STARTSTOP = ma1.ONOFF;
                                        //    dataMQC.PLC_Barcode = barcode;

                                        //}
                                        //else if (dataOld.STARTSTOP == "True" && ma1.ONOFF == "False")
                                        //{
                                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    ma1.Lot = barcode;
                                        //    machineOperations.Add(ma1);
                                        //    dataMQC = new DataMQC();
                                        //    dataMQC.STARTSTOP = ma1.ONOFF;
                                        //    dataMQC.PLC_Barcode = barcode;

                                        //}
                                        //else
                                        //{
                                        //    if (dataOld.STARTSTOP == null)
                                        //    {
                                        //        SystemLog.Output(SystemLog.MSG_TYPE.Err, "dataOld.STARTSTOP == null", ma1.Lot);
                                        //    }
                                        //    else if (ma1.ONOFF == null)
                                        //    {
                                        //        SystemLog.Output(SystemLog.MSG_TYPE.Err, "ma1.ONOFF == null", ma1.Lot);
                                        //    }
                                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    ma1.Lot = barcode;
                                        //    machineOperations.Add(ma1);

                                        //    if (barcode != null && barcode.Contains("0010;B01;B01"))
                                        //    {
                                        //        dataMQC = new DataMQC();
                                        //        ma1.Lot = barcode;
                                        //        dataMQC.PLC_Barcode = barcode;
                                        //        dataMQC.Good_Products_Total = ma1.Output;
                                        //        dataMQC.NG_Products_Total = ma1.NG;
                                        //        dataMQC.RW_Products_Total = ma1.Rework;
                                        //        dataMQC.STARTSTOP = ma1.ONOFF;
                                        //        dataMQC.NG_Products_NG_ = new int[38];
                                        //        int CountNG = 0;
                                        //        int CountRW = 0;
                                        //        if (dataMQC.NG_Products_Total > dataOld.NG_Products_Total)
                                        //        {
                                        //            var ListNG = Plc.Instance.ReadTags(tagsError);

                                        //            foreach (var item in ListNG)
                                        //            {
                                        //                dataMQC.NG_Products_NG_[CountNG] = int.Parse(item.ItemValue.ToString());
                                        //                CountNG++;
                                        //            }
                                        //        }
                                        //        else
                                        //        {
                                        //            dataMQC.NG_Products_NG_ = dataOld.NG_Products_NG_;

                                        //        }
                                        //        dataMQC.RW_Products_NG_ = new int[38];
                                        //        if (dataMQC.RW_Products_Total > dataOld.RW_Products_Total)
                                        //        {
                                        //            var ListRW = Plc.Instance.ReadTags(tagsRework);

                                        //            foreach (var item in ListRW)
                                        //            {
                                        //                dataMQC.RW_Products_NG_[CountRW] = int.Parse(item.ItemValue.ToString());
                                        //                CountRW++;
                                        //            }
                                        //        }
                                        //        else
                                        //        {
                                        //            dataMQC.RW_Products_NG_ = dataOld.RW_Products_NG_;
                                        //        }
                                        //    }
                                        //    else
                                        //    {

                                        //        dataMQC = dataOld;
                                        //        SystemLog.Output(SystemLog.MSG_TYPE.War, "Barcode is wrong format: IPMachine :", IP);

                                        //    }
                                        //}
                                        //}
                                        //else
                                        //{
                                        //    dataMQC = new DataMQC();
                                        //    var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                                        //    ma1.Lot = barcode;
                                        //    dataMQC.STARTSTOP = ma1.ONOFF;
                                        //    dataMQC.PLC_Barcode = barcode;
                                        //    dataMQC.Good_Products_Total = ma1.Output;
                                        //    dataMQC.NG_Products_Total = ma1.NG;
                                        //    dataMQC.RW_Products_Total = ma1.Rework;
                                        //    dataMQC.STARTSTOP = ma1.ONOFF;
                                        //    machineOperations.Add(ma1);
                                        //    SystemLog.Output(SystemLog.MSG_TYPE.War, "Data old = null: IPMachine :", IP);
                                        //}


                                    }
                                    //else
                                    //{
                                    //    MachineOperation ma1 = new MachineOperation();
                                    //    ma1.Line = line;
                                    //    ma1.IP = IP;

                                    //    ma1.Status = Plc.Instance.ConnectionState.ToString();
                                    //    machineOperations.Add(ma1);
                                    //    dataMQC = dataOld;
                                    //    SystemLog.Output(SystemLog.MSG_TYPE.War, "Readtag fail: IPMachine ", IP);
                                    //}


                                }

                                //else
                                //{
                                //    MachineOperation ma1 = new MachineOperation();
                                //    ma1.Line = line;
                                //    ma1.IP = IP;
                                //    ma1.Status = "Waiting";
                                //    machineOperations.Add(ma1);
                                //    dataMQC = dataPLC;
                                //    SystemLog.Output(SystemLog.MSG_TYPE.War, "Machine is offline", IP);
                            }
                        }

                    }
                    //else
                    //{
                    // if(   dataPLC.IsChecKQRCode == false && dataPLC.IsReadyStart == false)
                    //    {
                    //        if(dataPLC.Good_Products_Total + dataPLC.NG_Products_Total + dataPLC.RW_Products_Total > 0)
                    //        SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Reset", "");
                    //        else
                    //        {
                    //            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Reset choi", "");
                    //        }
                    //    }
                    //}

                    if(dataPLC.IsReset == true)
                    {
                        if (dataPLC.Good_Products_Total + dataPLC.NG_Products_Total + dataPLC.RW_Products_Total > 0)
                            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Reset", "");
                        else
                        {
                            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Reset choi", "");
                        }
                        Plc.Instance.Write(VariablePLC.IsReset, false);
                    }
                }
                else
                {
                    MachineOperation ma1 = new MachineOperation();
                    ma1.Line = line;
                    ma1.IP = IP;
                    ma1.Status = Plc.Instance.ConnectionState.ToString();
                    machineOperations.Add(ma1);
                    dataMQC = dataOld;
                    SystemLog.Output(SystemLog.MSG_TYPE.War, "Machine is offline", IP);
                }

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "DataMQC GetDataMQCRealtime line " + line, ex.Message);
            }

            return dataMQC;
        }

        private void btn_GetValue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> listQRMES = new List<string>();
                listQRMES = VariablePLC.barcodeaddressMES();
                List<string> listID = new List<string>();
                listID = VariablePLC.barcodeaddresID();
                List<Tag> listBarcodeMES = new List<Tag>();
                List<Tag> listBarcodeID = new List<Tag>();

                //List<Tag> QRMES = new List<Tag>();
                //QRMES.Add(new PLCSimenNetWrapper.Tag(VariablePLC.MESQR, ""));
                //List<Tag> QRID = new List<Tag>();
                //QRID.Add(new PLCSimenNetWrapper.Tag(VariablePLC.IDWorker, ""));

                string IPTest = "172.16.1.145";

                Plc.Instance.Connect(IPTest, 3000);
                var PLCStatus = Plc.Instance.ConnectionState;
                if (PLCStatus == ConnectionStates.Online)
                {
                    //  var PlcQRMES = Plc.Instance.ReadTagsToString(QRMES).ToString();
                    //var plcQRID = Plc.Instance.ReadTagsToString(QRID).ToString();
                    var numberMESQR = "";
                    List<Tag> listNumberQR = new List<Tag>();
                    listNumberQR.Add(new PLCSimenNetWrapper.Tag(VariablePLC.NumberCharIDWorker, ""));
                    listNumberQR.Add(new PLCSimenNetWrapper.Tag(VariablePLC.NumberCharMESQR, ""));

                    var listTagNumber = Plc.Instance.ReadTags(listNumberQR);
                    int NumberCharIDWorker = 0;
                    int NumberCharMESQR = 0;
                    if (listTagNumber.Count == 2)
                    {
                        NumberCharIDWorker = int.Parse(listTagNumber[0].ItemValue.ToString());
                        NumberCharMESQR = int.Parse(listTagNumber[1].ItemValue.ToString());
                    }

                    for (int i = 0; i < NumberCharMESQR; i++)
                    {
                        listBarcodeMES.Add(new PLCSimenNetWrapper.Tag(listQRMES[i], ""));
                    }
                    for (int i = 0; i < NumberCharIDWorker; i++)
                    {
                        listBarcodeID.Add(new PLCSimenNetWrapper.Tag(listID[i], ""));
                    }

                    var stringMES = Plc.Instance.ReadTagsToString(listBarcodeMES);
                    var stringID = Plc.Instance.ReadTagsToString(listBarcodeID);

                    // if (plcQRID.Count == 1)
                    txt_IDQR.Text = stringID;//((uint)OvenSV.ItemValue).ConvertToFloat();
                    txt_IDQRMES.Text = stringMES;
                    //  if (PlcQRMES.Count == 1)
                    // txt_IDQRMES.Text = plcQRID;
                }


            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, ex.Source, ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string IPTest = "172.16.1.145";

                Plc.Instance.Connect(IPTest, 3000);
                var PLCStatus = Plc.Instance.ConnectionState;
                if (PLCStatus == ConnectionStates.Online)
                {
                    bool ReadyStart = (txt_readyStart.Text == "1") ? true : false;
                    Int16 messageNum = Int16.Parse(txt_messageNumber.Text.Trim());
                    List<Tag> WriteMessage = new List<Tag>();
                    WriteMessage.Add(new PLCSimenNetWrapper.Tag(VariablePLC.WriteMessage, (Int16)messageNum));
                    WriteMessage.Add(new PLCSimenNetWrapper.Tag(VariablePLC.WriteReadyStart, ReadyStart));
                    Plc.Instance.Write(VariablePLC.OKProduced, (Int16)32);
                    Plc.Instance.Write(VariablePLC.NGProduced, (Int16)15);
                    //  Plc.Instance.Write("DB181.DBW206", messageNum);
                    Plc.Instance.Write(WriteMessage);
                }

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, ex.Source, ex.Message);
            }
        }
    }
}



