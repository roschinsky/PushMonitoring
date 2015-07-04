using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    class CheckHttp : CheckByValue
    {
        private const string valueUnitToken = "Statecode";
        private const int responseContentCutLength = 25;
        public string NotificationWebResponse { get; private set; }


        public CheckHttp(string url, double fromStateCode, double toStateCode)
            : base(url, fromStateCode, toStateCode, valueUnitToken) { }

        public CheckHttp(string url)
            : base(url, 200, 226, valueUnitToken) { }

        public override void ExecuteCheck()
        {
            Stopwatch requestTimer = new Stopwatch();

            try
            {
                if (input == null || String.IsNullOrWhiteSpace((string)input))
                {
                    Output = "no valid URL given";
                    CurrentValue = 0;
                    return;
                }

                requestTimer.Start();
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create((string)input);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    NotificationWebResponse = reader.ReadToEnd();
                }
                CurrentValue = (double)webResponse.StatusCode;
                webResponse.Close();
                requestTimer.Stop();
                Output = String.Format("successful request in {0:N0}ms", requestTimer.ElapsedMilliseconds);
            }
            catch (WebException exWeb)
            {
                if (requestTimer.IsRunning)
                {
                    requestTimer.Stop();
                }
                using (StreamReader reader = new StreamReader(exWeb.Response.GetResponseStream()))
                {
                    NotificationWebResponse = reader.ReadToEnd();
                }
                Output = String.Format("request issue in {0:N0}ms: {1}", requestTimer.ElapsedMilliseconds, exWeb.Message);
                CurrentValue = (double)exWeb.Status;
            }
            catch (Exception ex)
            {
                if(requestTimer.IsRunning)
                {
                    requestTimer.Stop();
                }
                Output = String.Format("issue in {0:N0}ms: {1}", requestTimer.ElapsedMilliseconds, ex.Message);
                CurrentValue = 0;
            }
            
            if(!String.IsNullOrWhiteSpace(NotificationWebResponse))
            {
                string cleanResponse = Regex.Replace(NotificationWebResponse, @"\s+", " ").Trim();
                Output += String.Format(" (Response: '{0}{1}')",
                    cleanResponse.Length > responseContentCutLength ? cleanResponse.Substring(0, responseContentCutLength) : cleanResponse,
                    cleanResponse.Length > responseContentCutLength ? "..." : String.Empty);
            }

            LastCheck = DateTime.Now;
        }

        protected override string GetName()
        {
            if(input != null && !String.IsNullOrWhiteSpace((string)input))
            {
                return String.Format("GET {0}", (string)input);
            }
            else
            {
                return "URL unknown";
            }
        }
    }
}
