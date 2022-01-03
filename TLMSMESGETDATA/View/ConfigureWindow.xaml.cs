using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;


namespace TLMSMESGETDATA.View
{
    /// <summary>
    /// Interaction logic for ConfigureWindow.xaml
    /// </summary>
    public partial class ConfigureWindow : Window
    {
        Model.SettingClass SettingClass = null;
        public ConfigureWindow()
        {
            InitializeComponent();
            LoadSettingToUI();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            SettingClass = new Model.SettingClass();
            SettingClass.OfflineServer = txt_serverOffline.Text.Trim();
            SettingClass.userOffline = txt_userOffline.Text.Trim();
            SettingClass.password = passwordBox.Password;
          
            SettingClass.IsStartupWindow = (bool)cb_StartupWindow.IsChecked;
            SettingClass.TechLinkDatabse = txt_database.Text.Trim();
         
           
            try
            {
                SettingClass.timmer = int.Parse(txt_timer.Text.Trim());
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "convert str to int fail", ex.Message);
                SettingClass.timmer = 30000;
            }
            try
            {
                SettingClass.PLCTimeOut = int.Parse(txt_PLCTimeOut.Text.Trim());
            }
            catch (Exception ex)
            {

                SystemLog.Output(SystemLog.MSG_TYPE.Err, "convert timeout to int fail", ex.Message);
                SettingClass.timmer = 3000;
            }
            if ((bool)cb_StartupWindow.IsChecked)
                StartUpWindow.RegistrationStartUp();
            else
                StartUpWindow.DeleteStartUp();

            Algorithm.SaveObject.Save_data(Algorithm.SaveObject.Pathsave, SettingClass);


        }
        private void LoadSettingToUI ()
        {
            if(File.Exists(Algorithm.SaveObject.Pathsave))
            {
                SettingClass = new Model.SettingClass();
                SettingClass = (Model.SettingClass)Algorithm.SaveObject.Load_data(Algorithm.SaveObject.Pathsave);
                if (SettingClass != null)
                {

                 
                    txt_serverOffline.Text = SettingClass.OfflineServer;
                    txt_userOffline.Text = SettingClass.userOffline;
                   
                    cb_StartupWindow.IsChecked = SettingClass.IsStartupWindow;
                    txt_timer.Text = SettingClass.timmer.ToString();
                    txt_PLCTimeOut.Text = SettingClass.PLCTimeOut.ToString();
                    passwordBox.Password = SettingClass.password;
                    txt_database.Text = SettingClass.TechLinkDatabse;
                }

                
            }
        }
    }
}
