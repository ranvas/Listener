//сделать нормальную трассировку
using Server.Model;
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
using Microsoft.Win32;
using System.Xml;


namespace ListenerService
{
    public partial class ListenerService : ServiceBase
    {
        private Server.Model.MyServer server;
        ////Путь к файлу xml. В нем лежат описания функций и изменяемые переменные библиотеки
        //private string ApiXmlFilePath
        //{
        //    get
        //    {
        //        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ListenerService");
        //        return (string)key.GetValue("servicePath") + @"\API.xml";
        //    }
        //}
        public ListenerService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("ListenerServiceSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ListenerServiceSource", "ListenerServiceLog");
            }
            ListenerServiceLog.Source = "ListenerServiceSource";
            ListenerServiceLog.Log = "ListenerServiceLog";
            server = new Server.Model.MyServer();
            server.ServerError += logging;
            server.ServerTracing += logging;
        }

        void logging(object sender, ServerEventArgs e)
        {
            ListenerServiceLog.WriteEntry(e.text);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ListenerServiceLog.WriteEntry("Таймер сработал");
        }
        
        
        protected override void OnStart(string[] args)
        {
            //определить адрес XML-файла конфигурации
            //string XMLFile = ApiXmlFilePath;
            //XmlDocument doc = new XmlDocument();
            try
            {
                ////открыть XML-файл и считать строку подключения
                //doc.Load(XMLFile);
                //string connectionString = doc.SelectSingleNode(".//connectionString").Attributes["name"].Value;
                ////поместить в реестр полученные значения
                //RegistryKey key = Registry.CurrentUser.CreateSubKey("baily");
                //key.SetValue("connectionString", connectionString);
                //key.SetValue("XML", XMLFile);
                //string test = (string)key.GetValue("XML");
                server.start();
            }
            catch(Exception ex)
            {
                ListenerServiceLog.WriteEntry(ex.Message + (ex.InnerException) ?? " " + ex.InnerException.Message);
                server.stop();
            }
            ListenerServiceLog.WriteEntry("Стартануло");
        }

        protected override void OnStop()
        {
            server.stop();
            ListenerServiceLog.WriteEntry("Остановилось");
        }


    }
}
