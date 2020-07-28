using System;
using System.IO;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckHomematic : CheckByProperties
    {
        private string xmlApiDefaultPath = "addons/xmlapi";

        private string xmlApiMethodDevice = "devicelist";
        private string xmlApiMethodStateAll = "statelist";
        private string xmlApiMethodStateSingle = "state";
        private string xmlApiMethodStateSet = "statechange";
        private string xmlApiMethodVariable = "sysvarlist";
        private string xmlApiMethodVariableSet = "statechange";
        private string xmlApiMethodMessage = "systemNotification";
        private string xmlApiMethodMessageSet = "systemNotificationClear";

        public CheckHomematic(string filePath, Comparison compareAs, string expectedContent)
            : base(filePath, new Tuple<object, object, object, object>(null, null, null, compareAs), expectedContent)
        {
            this.compareType = (Comparison)searchFor.Item4;
        }

        public override void ExecuteCheck()
        {
            try
            {
                if (input == null || String.IsNullOrWhiteSpace(input as string))
                {
                    Output = "no or empty CCU2 address given";
                    return;
                }

                XmlDocument result = new XmlDocument();

                if (hmUrl != null)
                {
                    string strErg = null;
                    using(System.Net.Http.HttpClient apiClient = new System.Net.Http.HttpClient()) {
                        strErg = apiClient.GetStringAsync($"{hmUrl}{xmlApiDefaultPath}/{apiMethod}.cgi?{parameter1}={parameterValue1}&{parameter2}={parameterValue2}")
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    if(strErg != null) {
                        result.LoadXml(strErg);
                    }

                    if (result != null && result.DocumentElement != null)
                    {
                        return result;
                    }
                }

                LastCheck = DateTime.Now;
            }
            catch (Exception ex)
            {
                Output = String.Format("error '{0}'", ex.Message);
            }
        }

        protected override string GetName()
        {
            if (input != null && !String.IsNullOrWhiteSpace(input as string))
            {
                return String.Format("FILE {0}", input as string);
            }
            else
            {
                return "unknown";
            }
        }

#region Common helper

        /// <summary>
        /// Converts UNIX timestamp to valid DateTime
        /// </summary>
        /// <param name="timeStamp">UNIX timestamp</param>
        /// <returns>DateTime object representing the given UNIX timestamp</returns>
        public static DateTime TimeStampToDateTime(long timeStamp)
        {
            if (timeStamp > 1)
            {
                return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).AddSeconds(timeStamp);
            }

            return DateTime.MinValue;
        }

#endregion        
    }
}
