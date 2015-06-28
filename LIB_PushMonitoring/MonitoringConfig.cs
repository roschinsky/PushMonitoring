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
                                        FileInfo LogFile = new FileInfo(node.InnerText);
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
                            double minValue = 0;
                            double maxValue = 0;

                            switch (checkNode.Name)
                            {
                                case "CheckDisk":
                                    input = checkNode.Attributes["Drive"] != null ? checkNode.Attributes["Drive"].Value : null;
                                    if (!input.Contains(":")) { input += ":"; }
                                    if (!input.Contains("\\")) { input += "\\"; }
                                    minValue = checkNode.Attributes["MinPercentageFree"] != null ? double.Parse(checkNode.Attributes["MinPercentageFree"].Value) : double.MinValue;
                                    maxValue = checkNode.Attributes["MaxPercentageFree"] != null ? double.Parse(checkNode.Attributes["MaxPercentageFree"].Value) : double.MaxValue;
                                    checks.Add(new CheckDisk(input, minValue, maxValue));
                                    break;
                                case "CheckHttp":
                                    input = checkNode.Attributes["Url"].Value;
                                    minValue = checkNode.Attributes["StateCodeFrom"] != null ? double.Parse(checkNode.Attributes["StateCodeFrom"].Value) : double.MinValue;
                                    maxValue = checkNode.Attributes["StateCodeTo"] != null ? double.Parse(checkNode.Attributes["StateCodeTo"].Value) : double.MaxValue;
                                    checks.Add(new CheckHttp(input, minValue, maxValue));
                                    break;
                                case "CheckIcmp":
                                    // TODO: Implement setup of CheckIcmp config
                                    break;
                                case "CheckWinService":
                                    // TODO: Implement setup of CheckWinService config
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
