using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TRoschinsky.Lib.PushMonitoring.CheckTypes;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class MonitoringConfig reads and holds all configuration values 
    /// that are needed for proper execution of PushMonitoring. One can use 
    /// XML based configuration data from string or file (as FileInfo).
    /// </summary>
    class MonitoringConfig
    {
        public string Name { get; private set; }
        public FileInfo LogFile { get; private set; }
        public string IntervallType { get; private set; }
        public int Intervall { get; private set; }
        private List<Check> checks = new List<Check>();
        public Check[] Checks { get { return checks.ToArray(); } }
        public Dictionary<string, Type> pushInterfaces = new Dictionary<string, Type>();
        public Dictionary<string, Type> PushInterfaces { get { return pushInterfaces; } }
        public bool NotifyEverRun { get; private set; }

        public bool ConfigReadSuccessfully { get; private set; }

        public MonitoringConfig(string xmlConfigString)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xmlConfigString), new XmlReaderSettings() { IgnoreComments = true });
            ConfigReadSuccessfully = ParseConfig(GetXmlDocument(reader));
        }

        public MonitoringConfig(FileInfo xmlConfigFile)
        {
            XmlReader reader = XmlReader.Create(xmlConfigFile.FullName, new XmlReaderSettings() { IgnoreComments = true });
            ConfigReadSuccessfully = ParseConfig(GetXmlDocument(reader));
        }

        private bool ParseConfig(XmlDocument xmlConfig)
        {
            try
            {
                // TODO: Implement parser
                if(xmlConfig != null && xmlConfig.HasChildNodes)
                {
                    XmlNode xmlConfigPushMon = xmlConfig.GetElementsByTagName("configPushMonitoring")[0];
                    if (xmlConfigPushMon != null)
                    {
                        Name = xmlConfigPushMon.Attributes["Name"] != null ? xmlConfigPushMon.Attributes["Name"].Value : "Unknown";
                        NotifyEverRun = xmlConfigPushMon.Attributes["NotifyEverRun"] != null ? bool.Parse(xmlConfigPushMon.Attributes["NotifyEverRun"].Value) : false;

                        foreach (XmlNode node in xmlConfigPushMon)
                        {
                            try
                            {
                                switch (node.Name)
                                {
                                    case "logFile":
                                        LogFile = new FileInfo(node.InnerText);
                                        break;

                                    case "sendWithoutError":
                                        IntervallType = node.Attributes["IntervallType"].Value;
                                        Intervall = int.Parse(node.Attributes["Intervall"].Value);
                                        break;

                                    case "sendInterfaces":
                                        ParseConfigInterfaces(node);
                                        break;

                                    case "checksToRun":
                                        ParseConfigChecks(node);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO: Implement error handling
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ParseConfigChecks(XmlNode checksNode)
        {
            try
            {
                foreach(XmlNode checkNode in checksNode)
                {
                    try
                    {

                        if (checkNode.Name.StartsWith("Check"))
                        {
                            string input = null;

                            switch (checkNode.Name)
                            {
                                case "CheckDisk":
                                    if (checkNode.Attributes["Drive"] != null)
                                    {
                                        string drive = checkNode.Attributes["Drive"] != null ? checkNode.Attributes["Drive"].Value : null;
                                        if (!drive.Contains(":")) { drive += ":"; }
                                        if (!drive.Contains("\\")) { drive += "\\"; }

                                        if(checkNode.Attributes["MinPercentageFree"] != null && checkNode.Attributes["MaxPercentageFree"] != null)
                                        {
                                            checks.Add(new CheckDisk(drive, double.Parse(checkNode.Attributes["MinPercentageFree"].Value), double.Parse(checkNode.Attributes["MaxPercentageFree"].Value)));
                                        }
                                        else
                                        {
                                            checks.Add(new CheckDisk(drive));
                                        }
                                    }
                                    else
                                    {
                                        if (checkNode.Attributes["MinPercentageFree"] != null && checkNode.Attributes["MaxPercentageFree"] != null)
                                        {
                                            checks.Add(new CheckDisk(double.Parse(checkNode.Attributes["MinPercentageFree"].Value), double.Parse(checkNode.Attributes["MaxPercentageFree"].Value)));
                                        }
                                        else
                                        {
                                            checks.Add(new CheckDisk());
                                        }
                                    }
                                    break;
                                case "CheckHttp":
                                    input = checkNode.Attributes["Url"].Value;
                                    if(checkNode.Attributes["StateCodeFrom"] != null && checkNode.Attributes["StateCodeTo"] != null)
                                    {
                                        checks.Add(new CheckHttp(input, double.Parse(checkNode.Attributes["StateCodeFrom"].Value), double.Parse(checkNode.Attributes["StateCodeTo"].Value)));
                                    }
                                    else
                                    {
                                        checks.Add(new CheckHttp(input));
                                    }
                                    break;
                                case "CheckIcmp":
                                    input = checkNode.Attributes["Host"].Value;
                                    if(checkNode.Attributes["MinResponseTimeMs"] != null && checkNode.Attributes["MaxResponseTimeMs"] != null)
                                    {
                                        checks.Add(new CheckIcmp(input, double.Parse(checkNode.Attributes["MinResponseTimeMs"].Value), double.Parse(checkNode.Attributes["MaxResponseTimeMs"].Value)));
                                    }
                                    else
                                    {
                                        checks.Add(new CheckIcmp(input));
                                    }
                                    break;
                                case "CheckWinService":
                                    input = checkNode.Attributes["ServiceName"].Value;
                                    checks.Add(new CheckWinService(input));
                                    break;
                                case "CheckWinUptime":
                                    // TODO: Implement setup of CheckWinUptime config
                                    break;

                                default:
                                    break;
                            }
                        }

                    }
                    catch
                    {
                        // A specific check wasn't interpreted well... so right now we'll just ignore and go on
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Implement exception handling
                throw;
            }
        }

        private void ParseConfigInterfaces(XmlNode interfacesNode)
        {
            try
            {
                foreach (XmlNode interfaceNode in interfacesNode)
                {
                    if(interfaceNode.Name == "sendTo")
                    {
                        if(interfaceNode.Attributes["Type"].Value == "Pushalot")
                        {
                            pushInterfaces.Add(interfaceNode.Attributes["ApiKey"].Value, typeof(NotificationPushalot));
                        }
                        else if (interfaceNode.Attributes["Type"].Value == "Pushover")
                        {
                            pushInterfaces.Add(interfaceNode.Attributes["ApiKey"].Value, typeof(NotificationPushover));
                        }
                        else if (interfaceNode.Attributes["Type"].Value == "Telegram")
                        {
                            pushInterfaces.Add(interfaceNode.Attributes["ApiKey"].Value, typeof(NotificationTelegram));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Implement exception handling
                throw;
            }
        }

        private XmlDocument GetXmlDocument(XmlReader xmlReader)
        {
            if (xmlReader != null)
            {
                XmlDocument config = new XmlDocument();
                config.Load(xmlReader);
                xmlReader.Close();
                return config;
            }
            else
            {
                return null;
            }
        }

    }
}
