using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Configuration;

namespace FileTransferService
{
    public partial class FileTransferService : ServiceBase
    {
        public string WatchDirectory = ConfigurationManager.AppSettings["WatchFolder"].ToString();
        public string MoveToDirectory = ConfigurationManager.AppSettings["MoveToFolder"].ToString();
        private int PollingSeconds = int.Parse(ConfigurationManager.AppSettings["SecondsToCheck"].ToString());
        private int eventId = 1;

        public FileTransferService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("FileTransferService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "FileTransferService", "Application");
            }
            eventLog1.Source = "FileTransferService";
            eventLog1.Log = "Application";

            
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("File Transfer Service Started", EventLogEntryType.Information, eventId++);

            if (!Directory.Exists(WatchDirectory))
            {
                eventLog1.WriteEntry("Directory Does Not Exist: " + WatchDirectory, EventLogEntryType.Error, eventId++);
                Directory.CreateDirectory(WatchDirectory);
            }
            if (!Directory.Exists(MoveToDirectory))
            {
                eventLog1.WriteEntry("Directory Does Not Exist: " + MoveToDirectory, EventLogEntryType.Error, eventId++);
                Directory.CreateDirectory(MoveToDirectory);
            }

            Timer timer = new Timer();
            timer.Interval = (1000 * PollingSeconds); //10 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

        }


        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            //ADD TRANSFER FILES
            try
            {
                foreach (string file in Directory.GetFiles(WatchDirectory))
                {
                    FileInfo f = new FileInfo(file);
                    eventLog1.WriteEntry("Move File: " + f.FullName + " to " + MoveToDirectory, EventLogEntryType.Information, eventId++);
                    f.MoveTo(MoveToDirectory + "\\" + f.Name);
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error Transferring Files: " + ex.Message + " | " + ex.StackTrace, EventLogEntryType.Error, eventId++);
            }

           
        }

        protected override void OnStop()
        {
        }
    }
}
