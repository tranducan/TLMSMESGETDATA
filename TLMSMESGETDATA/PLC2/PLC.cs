﻿using S7.Net;
using PLCSimenNetWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace TLMSMESGETDATA.PLC2
{
    class Plc
    {
        #region Singleton

        // For implementation refer to: http://geekswithblogs.net/BlackRabbitCoder/archive/2010/05/19/c-system.lazylttgt-and-the-singleton-design-pattern.aspx        
        private static readonly Lazy<Plc> _instance = new Lazy<Plc>(() => new Plc());

        public static Plc Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        #endregion

        #region Public properties

        public ConnectionStates ConnectionState { get { return plcDriver != null ? plcDriver.ConnectionState : ConnectionStates.Offline; } }

        public DB1 Db1 { get; set; }

        public TimeSpan CycleReadTime { get; private set; }

        #endregion

        #region Private fields

        IPlcSyncDriver plcDriver;

        System.Timers.Timer timer = new System.Timers.Timer();

        public DateTime lastReadTime;

        #endregion

        #region Constructor

        private Plc()
        {
          
            timer.Interval = 100; // ms
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
            lastReadTime = DateTime.Now;
        }

        #endregion

        #region Event handlers

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return;
            }

            timer.Enabled = false;
            CycleReadTime = DateTime.Now - lastReadTime;
            try
            {
             //   RefreshTags();
            }
            finally
            {
                timer.Enabled = true;
                lastReadTime = DateTime.Now;
            }
        }

        #endregion

        #region Public methods

        public void Connect(string ipAddress)
        {
            if (!IsValidIp(ipAddress))
            {
                throw new ArgumentException("Ip address is not valid");
            }
            plcDriver = new S7NetPlcDriver(CpuType.S71200, ipAddress, 0, 1);
            plcDriver.Connect();
        }

        public void Disconnect()
        {
            if (plcDriver == null || this.ConnectionState == ConnectionStates.Offline)
            {
                return;
            }
            plcDriver.Disconnect();
        }

        public void Write(string name, object value)
        {
            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return;
            }
            Tag tag = new Tag(name, value);
            List<Tag> tagList = new List<Tag>();
            tagList.Add(tag);
            plcDriver.WriteItems(tagList);
        }
        public void Read(string name, object value)
        {
            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return;
            }
            Tag tag = new Tag(name, value);
            List<Tag> tagList = new List<Tag>();
            tagList.Add(tag);
            var tem = plcDriver.ReadItems(tagList);
        }
        public List<Tag> ReadTags(List<Tag> tagsinput)
        {


            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return null;
            }

            List<Tag> tags = plcDriver.ReadItems(tagsinput);
            return tags;

        }
        public string ReadTagsToString(List<Tag> tagsinput)
        {
            string barcode = "";
            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return null;
            }
            List<Tag> tags = plcDriver.ReadItems(tagsinput);
            foreach (var item in tags)
            {
                char c = Convert.ToChar(int.Parse(item.ItemValue.ToString()));
                barcode += c;
            }
            return barcode;
        }

        public void Write(List<Tag> tags)
        {
            if (plcDriver == null || plcDriver.ConnectionState != ConnectionStates.Online)
            {
                return;
            }
            plcDriver.WriteItems(tags);
        }

        #endregion        

        #region Private methods

        private bool IsValidIp(string addr)
        {
            IPAddress ip;
            bool valid = !string.IsNullOrEmpty(addr) && IPAddress.TryParse(addr, out ip);
            return valid;
        }

        private void RefreshTags()
        {
            plcDriver.ReadClass(Db1, 2);

        }

        #endregion


    }
}
