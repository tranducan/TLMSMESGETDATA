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

        List<Tag> tagsError;
        List<string> ListBarcode;
        List<string> ListError;
        List<Tag> tagsbarcode;
        List<string> ListRework;
        List<Tag> tagsRework;
        List<Tag> tags;
        DispatcherTimer timer = new DispatcherTimer();
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
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
            timer.IsEnabled = true;
         
        }
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
                Plc.Instance.Connect("172.16.1.64");

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
                Plc.Instance.Disconnect();
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
            try
            {
                var ListTag = Plc.Instance.ReadTags(tags);
                if (ListTag != null)
                {
                    lb_output.Content = ListTag[0].ItemValue;
                    lb_NG.Content = ListTag[1].ItemValue;
                }
                var barcode = Plc.Instance.ReadTagsToString(tagsbarcode);
                if (barcode != null)
                {
                    lb_barcode.Content = barcode;
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
                    lb_output.Content = ListTag[0].ItemValue;
                    lb_NG.Content = ListTag[1].ItemValue;
                }

            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "LoadDataRealTime ()", ex.Message);
            }

        }
    }
}
