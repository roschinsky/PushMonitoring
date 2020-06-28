using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TRoschinsky.Common;

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
        private List<JournalEntry> logEntries = new List<JournalEntry>();
        public IReadOnlyList<JournalEntry> LogEntries { get { return logEntries.AsReadOnly(); } }
        public int LastNotifcationsSuccessful { get; private set; }
        public int LastNotifcationsToSend { get { return monitoringConfig != null ? monitoringConfig.PushInterfaces.Count : 0; } }
        public string LastCheckResult { get; private set; }
        public int LastChecksSuccessful { get; private set; }
        public int LastChecksFailed { get; private set; }


        public Monitoring(string xmlConfigString)
        {
            logEntries.Add(new JournalEntry("Starting with XML config string."));
            monitoringConfig = new MonitoringConfig(xmlConfigString);
            logEntries.AddRange(monitoringConfig.Log);
            Initialize();
        }

        public Monitoring(FileInfo xmlConfig)
        {
            logEntries.Add(new JournalEntry("Starting with XML config file."));
            monitoringConfig = new MonitoringConfig(xmlConfig);
            logEntries.AddRange(monitoringConfig.Log);
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

                notificationBody += String.Format("\n\nChecks done. {2} (S:{0}/F:{1})", LastChecksSuccessful, LastChecksFailed, !sendNotification ? "It's all fine!" : "Some issues detected!");

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
            catch (Exception ex)
            {
                logEntries.Add(new JournalEntry("Unexpected error when running checks.", ex));
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
                    Notification notify = null;
                    logEntries.Add(new JournalEntry(String.Format("Notification via {0}...", pushInterface.Value.Name)));
                    if (pushInterface.Value == typeof(NotificationPushover))
                    {
                        notify = new NotificationPushover(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if(notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                    else if (pushInterface.Value == typeof(NotificationPushalot))
                    {
                        notify = new NotificationPushalot(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if (notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                    else if (pushInterface.Value == typeof(NotificationTelegram))
                    {
                        notify = new NotificationTelegram(pushInterface.Key, notificationBody, monitoringName, sendWithHighPrio, false);
                        if (notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }

                    if (notify != null)
                    {
                        logEntries.AddRange(notify.Log);
                    }
                    logEntries.Add(new JournalEntry(String.Format("...was processed!"), !(notify != null && notify.NotificationSuccessfulSend)));
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
            catch (Exception ex)
            {
                // Something went wrong but we don't care; feature will be disabled
                logEntries.Add(new JournalEntry("Unable to read log file.", ex));
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
            catch (Exception ex)
            {
                // Something went wrong but we don't care; feature will be disabled by next read
                logEntries.Add(new JournalEntry("Unable to write log file.", ex));
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
