using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class Monitoring is a set of checks, defined by configuration file or programmatic 
    /// setup, that will execute the checks and, if needed, initiate notifications.
    /// </summary>
    public class Monitoring
    {
        private MonitoringConfig monitoringConfig;
        private DateTime lastOverrideExecution = DateTime.MinValue;
        private List<Check> checks = new List<Check>();
        public IReadOnlyList<Check> Checks { get { return checks.AsReadOnly(); } }
        public int LastNotifcationsSuccessful { get; private set; }
        public int LastNotifcationsToSend { get { return monitoringConfig != null ? monitoringConfig.PushInterfaces.Count : 0; } }
        public string LastCheckResult { get; private set; }
        public int LastChecksSuccessful { get; private set; }
        public int LastChecksFailed { get; private set; }


        public Monitoring(string xmlConfigString)
        {
            monitoringConfig = new MonitoringConfig(xmlConfigString);
            Initialize();
        }

        public Monitoring(FileInfo xmlConfig)
        {
            monitoringConfig = new MonitoringConfig(xmlConfig);
            Initialize();
        }

        private void Initialize()
        {
            if (monitoringConfig != null && monitoringConfig.ConfigReadSuccessfully)
            {
                // Add checks from configuration if there are any
                if (monitoringConfig.Checks != null && monitoringConfig.Checks.Length > 0)
                {
                    checks.AddRange(monitoringConfig.Checks);
                    // Run checks if there are any
                    if (checks.Count > 0)
                    {
                        ReadLogfile();
                        RunChecks();
                        WriteLogfile();
                    }
                }
            }
        }

        public void RunChecks()
        {
            LastChecksSuccessful = LastChecksFailed = 0;
            LastCheckResult = String.Empty;

            try
            {
                bool sendNotification = false;
                string checkName = String.Empty;
                string notificationBody = String.Format("executed checks{2} on '{0}' @ {1}\n", System.Environment.MachineName, DateTime.Now, OverrideRunNeeded() ? " by interval" : String.Empty);

                // Processing of checks by executing them and obtaining results
                foreach (Check check in checks)
                {
                    try
                    {
                        checkName = check.Name;
                        check.ExecuteCheck();
                        if (check.NotifyRequired)
                        {
                            sendNotification = true;
                            LastChecksFailed++;
                        }
                        else
                        {
                            LastChecksSuccessful++;
                        }

                        notificationBody += "\n" + check.ToString();
                    }
                    catch (Exception ex)
                    {
                        sendNotification = true;
                        LastChecksFailed++;
                        notificationBody += String.Format("\n[!] Check {0} failed with {1}", checkName, ex.Message);
                    }
                }

                notificationBody += String.Format("\n\n Checks done. {2} (S:{0}/F:{1})", LastChecksSuccessful, LastChecksFailed, !sendNotification ? "It's all fine!" : "Some issues detected!");

                // Processing of checks done so now let's see if we have to push out some notifications
                if (sendNotification || OverrideRunNeeded() || monitoringConfig.NotifyEverRun)
                {
//#if RELEASE
                    LastNotifcationsSuccessful = SendNotification(notificationBody, (LastChecksFailed > 0));
//#endif
                }

                // Set last result
                LastCheckResult = notificationBody;
            }
            catch (Exception)
            {
                // Todo: Implement error handling
            }
        }

        public int SendNotification(string notificationBody, bool sendWithHighPrio)
        {
            int successfulNotifications = 0;
            string monitoringName = String.Format("PM from {0}", monitoringConfig.Name);

            if (monitoringConfig.PushInterfaces != null)
            {
                foreach (KeyValuePair<string, Type> pushInterface in monitoringConfig.PushInterfaces)
                {
                    if (pushInterface.Value == typeof(NotificationPushover))
                    {
                        Notification notify = new NotificationPushover(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if(notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                    else if (pushInterface.Value == typeof(NotificationPushalot))
                    {
                        Notification notify = new NotificationPushalot(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if (notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                    else if (pushInterface.Value == typeof(NotificationTelegram))
                    {
                        Notification notify = new NotificationTelegram(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if (notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                }
            }

            return successfulNotifications;
        }

        public void AddCheck(Check check)
        {
            checks.Add(check);
        }

        private void ReadLogfile()
        {
            try
            {
                if (monitoringConfig.LogFile != null && monitoringConfig.LogFile.Exists)
                {
                    string[] lines = File.ReadAllLines(monitoringConfig.LogFile.FullName, Encoding.UTF8);
                    if (lines != null && lines.Length > 0)
                    {
                        lastOverrideExecution = new DateTime(long.Parse(lines[0]));
                        return;
                    }
                }
            }
            catch (Exception)
            { 
                // Something went wrong but we don't care; feature will be disabled
            }
        }

        private void WriteLogfile()
        {
            try
            {
                if (monitoringConfig.LogFile != null)
                {
                    string notificationLog = OverrideRunNeeded() || lastOverrideExecution == DateTime.MinValue ? DateTime.Now.Ticks.ToString() : lastOverrideExecution.Ticks.ToString();
                    notificationLog += String.Format("\n*** Results ***\n{0}", LastCheckResult);
                    File.WriteAllText(monitoringConfig.LogFile.FullName, notificationLog, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // Something went wrong but we don't care; feature will be disabled by next read
            }
        }

        private bool OverrideRunNeeded()
        {
            bool result = false;
            if (!String.IsNullOrWhiteSpace(monitoringConfig.IntervallType) && monitoringConfig.Intervall > 0 && lastOverrideExecution > DateTime.MinValue)
            {

                TimeSpan lastOverrideExecutionSince = DateTime.Now - lastOverrideExecution;

                switch (monitoringConfig.IntervallType.ToLower())
                {
                    case "day":
                    case "days":
                        result = lastOverrideExecutionSince.TotalDays > monitoringConfig.Intervall;
                        break;
                    case "hour":
                    case "hours":
                        result = lastOverrideExecutionSince.TotalHours > monitoringConfig.Intervall;
                        break;
                    case "minute":
                    case "minutes":
                        result = lastOverrideExecutionSince.TotalMinutes > monitoringConfig.Intervall;
                        break;

                    default:
                        break;
                }
            }
            return result;
        }
    }
}
