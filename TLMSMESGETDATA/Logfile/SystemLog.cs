using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMSMESGETDATA
{
    public class SystemLog
    {
        static SystemLog s_myInstance = null;
        string m_startUpPath = "";
        public enum MSG_TYPE
        {
            Nor,
            Err,
            War
        };
        private SystemLog()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "\\Logfile";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dirPath);
            if (dir.Exists == false)
                dir.Create();
            m_startUpPath = dirPath + "\\Log_";
        }

        public static void Output(MSG_TYPE msgType, string name, string str)
        {
            if (s_myInstance == null)
                s_myInstance = new SystemLog();
            s_myInstance.logout(msgType, name, str);
        }
        public static void Output(MSG_TYPE msgType, string name, string format, params object[] args)
        {
            if (s_myInstance == null)
                s_myInstance = new SystemLog();
            s_myInstance.logout(msgType, name, string.Format(format, args));
        }
        private void logout(MSG_TYPE msgType, string name, string str)
        {
            string filePath = m_startUpPath + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string output = name + " : " + str;
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath, true))
                {
                    file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + output);
                }
            }
            catch (Exception) { }
            
            EventBroker.AsyncSend(EventBroker.EventID.etLog, new EventBroker.EventParam(this, (int)msgType, output));
        }


    }
}
