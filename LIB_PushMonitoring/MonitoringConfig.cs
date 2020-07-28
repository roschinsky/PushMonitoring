using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TRoschinsky.Common;
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
        public List<JournalEntry> Log = new List<JournalEntry>();

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
                            catch (Exception exNode)
                            {
                                Log.Add(new JournalEntry(String.Format("Failed to process config node '{0}'.", node.Name), exNode));
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Add(new JournalEntry("Failed to process config.", ex));
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
                                case "CheckWinTaks":
                                    // TODO: Implement setup of CheckWinTask config
                                    break;
                                case "CheckFileState":
                                    input = checkNode.Attributes["FilePath"].Value;
                                    checks.Add(new CheckFileState(input));
                                    break;
                                case "CheckFileContent":
                                    input = checkNode.Attributes["FilePath"].Value;
                                    string inputPeekFrom = checkNode.Attributes["PeekFrom"].Value;
                                    string inputPeekTo = checkNode.Attributes["PeekTo"].Value;
                                    CheckByProperties.Comparison inputCompareType;
                                    switch(checkNode.Attributes["CompareType"].Value.ToLower())
                                    {
                                        case "hash":
                                            inputCompareType = CheckByProperties.Comparison.Hash;
                                            break;

                                        case "startswith":
                                            inputCompareType = CheckByProperties.Comparison.StartsWith;
                                            break;

                                        case "endswith":
                                            inputCompareType = CheckByProperties.Comparison.EndsWith;
                                            break;

                                        case "contains":
                                            inputCompareType = CheckByProperties.Comparison.Contains;
                                            break;

                                        case "length":
                                            inputCompareType = CheckByProperties.Comparison.Length;
                                            break;

                                        default:
                                            inputCompareType = CheckByProperties.Comparison.CompleteMatch;
                                            break;
                                    }
                                    string inputExpectedValue = checkNode.Attributes["ExpectedValue"].Value;
                                    checks.Add(new CheckFileContent(input, int.Parse(inputPeekFrom), int.Parse(inputPeekTo), inputCompareType, inputExpectedValue));
                                    break;
                                case "CheckHomematic":
                                    string host = checkNode.Attributes["Host"].Value;
                                    input = checkNode.Attributes["CCU2Device"].Value;
                                    string inputExpectedValue = checkNode.Attributes["ExpectedValue"].Value;
                                    checks.Add(new CheckHomematic(input, inputExpectedValue));
                                    break;

                                default:
                                    break;
                            }
                        }

                    }
                    catch (Exception exNode)
                    {
                        // A specific check wasn't interpreted well... log, ignore and just go on
                        Log.Add(new JournalEntry(String.Format("Failed to process config check node '{0}'.", checkNode.Name), exNode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private XmlDocument GetXmlDocument(XmlReader xmlReader)
        {
            try
            {
                if (xmlReader != null)
                {
                    XmlDocument config = new XmlDocument();
                    config.Load(xmlReader);
                    xmlReader.Close();
                    return config;
                }
            }
            catch (Exception ex)
            {
                Log.Add(new JournalEntry("Failed to load XML document.", ex));
            }
            return null;
        }

    }
}
