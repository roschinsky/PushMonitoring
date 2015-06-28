using System;
using System.Collections.Generic;
using System.IO;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class Monitoring is a set of checks, defined by configuration file or programmatic 
    /// setup, that will execute the checks and, if needed, initiate notifications.
    /// </summary>
    public class Monitoring
    {
        private MonitoringConfig monitoringConfig;

        private List<Check> checks = new List<Check>();
        public IReadOnlyList<Check> Checks { get { return checks.AsReadOnly(); } }
        public int LastNotifcationsSuccessful { get; private set; }
        public int LastNotifcationsToSend { get { return monitoringConfig != null ? monitoringConfig.PushInterfaces.Count : 0; } }
        public string LastCheckResult { get; private set; }


        public Monitoring(string xmlConfigString)
        {
            monitoringConfig = new MonitoringConfig(xmlConfigString);
            if (monitoringConfig != null && monitoringConfig.ConfigReadSuccessfully)
            {
                checks.AddRange(monitoringConfig.Checks);
                RunChecks();
            }
        }

        public Monitoring(FileInfo xmlConfig)
        {
            monitoringConfig = new MonitoringConfig(xmlConfig);
            if (monitoringConfig != null && monitoringConfig.ConfigReadSuccessfully)
            {
                checks.AddRange(monitoringConfig.Checks);
                RunChecks();
            }
        }

        public void RunChecks()
        {
            try
            {
                bool sendNotification = false;
                string checkName = String.Empty;
                string notificationBody = String.Format("Executed checks on '{0}' @ {1}\n", System.Environment.MachineName, DateTime.Now);

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
                        }

                        notificationBody += "\n" + check.ToString();
                    }
                    catch (Exception ex)
                    {
                        sendNotification = true;
                        notificationBody += String.Format("\nCheck {0} failed with: {1}", checkName, ex.Message);
                    }
                }

                // Processing of checks done so now let's see if we have to push out some notifications
                if (sendNotification || monitoringConfig.NotifyEverRun)
                {
                    LastNotifcationsSuccessful = SendNotification(notificationBody);
                }

                // Set last result
                LastCheckResult = notificationBody;
            }
            catch (Exception)
            {
                // Todo: Implement error handling
            }
        }

        public int SendNotification(string notificationBody)
        {
            int successfulNotifications = 0;
            string monitoringName = String.Format("PM from {0}", monitoringConfig.Name);

            if (monitoringConfig.PushInterfaces != null)
            {
                foreach (KeyValuePair<string, Type> pushInterface in monitoringConfig.PushInterfaces)
                {
                    if (pushInterface.Value == typeof(NotificationPushover))
                    {
                        Notification notify = new NotificationPushover(pushInterface.Key, notificationBody, monitoringName);
                        if(notify.NotificationSuccessfulSend) { successfulNotifications++; }
                    }
                    else if (pushInterface.Value == typeof(NotificationPushalot))
                    {
                        Notification notify = new NotificationPushalot(pushInterface.Key, notificationBody, monitoringName);
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
    }
}
