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

namespace TLMSMESGETDATA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        BackgroundWorker bgSendMailWorker;
        object lockObject = new object();
        List<MachineOperation> machineOperations;
        List<Plc> listPLC = new List<Plc>();
        List<Tag> ListTagValueBasis;
        List<Tag> ListTagNG;
        List<Tag> ListTagRework;
        string barcodeRuning = "";
        string barcodeNew ="";
        Stopwatch stopwatch;
        int CountNGOld = 0 ; bool IsChangeCountNG = false;
        int CountReworkOld = 0; bool IsChangeCountRW = false;


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
            SystemLog.Output(SystemLog.MSG_TYPE.War, Title, "Started " );
            machineOperations = new List<MachineOperation>();
            //timer.Interval = TimeSpan.FromMilliseconds(100);
            //timer.Tick += timer_Tick;
            //timer.IsEnabled = true;



        }
        #region backgroundworker
        private void LoadBackgroundWorker()
        {   // this timer calls bgWorker again and again after regular intervals
            tmrCallBgWorker = new System.Windows.Forms.Timer();//Timer for do task
            tmrCallBgWorker.Tick += new EventHandler(tmrCallBgWorker_Tick);
            tmrCallBgWorker.Interval = 100;

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

            datagridMachines.ItemsSource = machineOperations;
            datagridMachines.Items.Refresh();
          
            lblReadTime.Text = "Circle-time: " + stopwatch.ElapsedMilliseconds.ToString() + " ms";
            int CountOnline = machineOperations.Where(d => d.Status == ConnectionStates.Online.ToString()).Count();
            int CountOffline = machineOperations.Where(d => d.Status == ConnectionStates.Offline.ToString()).Count();
            lblConnectionState.Text = CountOnline.ToString() + " " + ConnectionStates.Online.ToString() + "|" +
                CountOffline.ToString() + " " + ConnectionStates.Offline.ToString();
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            // does a job like writing to serial communication, webservices etc
            var worker = sender as BackgroundWorker;
            stopwatch = new Stopwatch();

            stopwatch.Start();
            if (ListMachines != null)
            {
                machineOperations = new List<MachineOperation>();
                foreach (var machine in ListMachines)
                {
                    Plc.Instance.Connect(machine.IP);
                    if (Plc.Instance.ConnectionState == ConnectionStates.Online)
                    {
                        var ListTag = Plc.Instance.ReadTags(tags);
                        if (ListTag != null && ListTag.Count == 3)
                        {
                            MachineOperation ma1 = new MachineOperation();
                            ma1.IP = machine.IP;
                            ma1.Output = (ListTag[0].ItemValue.ToString() != null) ? double.Parse(ListTag[0].ItemValue.ToString()) : 0;
                            ma1.NG = (ListTag[1].ItemValue.ToString() != null) ? double.Parse(ListTag[1].ItemValue.ToString()) : 0;
                            ma1.Rework = (ListTag[2].ItemValue.ToString() != null) ? double.Parse(ListTag[2].ItemValue.ToString()) : 0;
                            ma1.Status = Plc.Instance.ConnectionState.ToString();

                            // Xac dinh co thay doi NG, RW ko ?
                          if(CountNGOld < ma1.NG)
                            {
                               var  ListNG= Plc.Instance.ReadTags(tagsError);
                           //     ListTagNG = ListNG.Where(d => (int)d.ItemValue > 0).ToList();
                                CountNGOld =(int) ma1.NG;
                            }
                          if(CountReworkOld < ma1.Rework)
                            {
                               // var ListRW = 
                               // ListTagRework = Plc.Instance.ReadTags(tagsRework);
                                CountReworkOld = (int)ma1.Rework;
                            }
                            if (ma1.Output + ma1.NG + ma1.Rework == 0 || barcodeRuning == "")
                                barcodeRuning = Plc.Instance.ReadTagsToString(tagsbarcode);
                            if (barcodeRuning != null || barcodeRuning != ""  /*&& barcodeRuning.Length == 24*/)
                                ma1.Lot = barcodeRuning;
                            else
                            {
                                ma1.Lot = "";
                            }
                            machineOperations.Add(ma1);
                            ma1 = null;
                        }
                    }
                    else if(Plc.Instance.ConnectionState == ConnectionStates.Offline)
                    {
                        MachineOperation ma1 = new MachineOperation();
                        ma1.IP = machine.IP;
                        ma1.Status = Plc.Instance.ConnectionState.ToString();

                        machineOperations.Add(ma1);
                        ma1 = null;
                    }
                }
                stopwatch.Stop();

                //     barcodeRuning = Plc.Instance.ReadTagsToString(tagsbarcode);

                Plc.Instance.Disconnect();
                    
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
        void timer_Tick(object sender, EventArgs e)
        {
            btn_connect.IsEnabled = Plc.Instance.ConnectionState == ConnectionStates.Offline;
       if(Plc.Instance.ConnectionState == ConnectionStates.Offline)
            {
                ElipsStatus.Fill = Brushes.Black;
            }
        else    if (Plc.Instance.ConnectionState == ConnectionStates.Online)
            {
                ElipsStatus.Fill = Brushes.Green;
            }
            else if (Plc.Instance.ConnectionState == ConnectionStates.Connecting)
            {
                ElipsStatus.Fill = Brushes.Yellow;
            }
            btn_disconnect.IsEnabled = Plc.Instance.ConnectionState != ConnectionStates.Offline;
            lblConnectionState.Text = Plc.Instance.ConnectionState.ToString();
            LoadDataRealTime();
            // statusbar
            lblReadTime.Text = Plc.Instance.CycleReadTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }
        public void LoadAdress()
        {
            ListMachines = new List<MachineItem>();
            PLCData pLCData = new PLCData();
            ListMachines = pLCData.GetIpMachineRuning();
            ListBarcode = VariablePLC.barcodeaddress();

            tagsbarcode = new List<Tag>();
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

            Tag tag = new Tag(VariablePLC.Good_Products_Total, "");
            tags.Add(tag);
            tag = new Tag(VariablePLC.NG_Products_Total, "");
            tags.Add(tag);
            tag = new Tag(VariablePLC.RW_Products_Total, "");
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

        }

        private void Window_Closed(object sender, EventArgs e)
        {

            if (m_timerEvent != null)
                EventBroker.RemoveTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent);
            EventBroker.RemoveObserver(EventBroker.EventID.etLog, m_observerLog);
            EventBroker.Relase();
        }

        private void Btn_test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tmrCallBgWorker.Start();
                btn_connect.Content = "Starting";
                btn_disconnect.Content = "Stop";
                btn_connect.IsEnabled = false;
                btn_disconnect.IsEnabled = true;
                //   Plc.Instance.Connect("172.16.1.64");

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);

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
                MessageBox.Show(exc.Message);
            }
        }

        private void Btn_getData_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            Plc.Instance.Connect("172.16.1.64");
            try
            {
                var ListTag = Plc.Instance.ReadTags(tags);
                if (ListTag != null)
                {
                  
                }
                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                if (barcode != null)
                {
                   
                }


                //var ListTagError = Plc.Instance.ReadTags(tagsError);

                //var ListTagRework = Plc.Instance.ReadTags(tagsRework);
                stopwatch.Stop();
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Tack-time", stopwatch.ElapsedMilliseconds.ToString() + " ms");
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Tack-time", ex.Message);
            }
        
        }
        public void LoadDataRealTime ()
        {
            try
            {
                var ListTag = Plc.Instance.ReadTags(tags);
                if (ListTag != null)
                {
                
                  
                }

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "LoadDataRealTime ()", ex.Message);
            }

        }
    }
}
