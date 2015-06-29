using System;
using System.Net;
using System.Net.NetworkInformation;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckIcmp : CheckByValue
    {
        private const string valueUnitToken = "ms";
        private const int pingTimeOutDefault = 3000;


        public CheckIcmp(string host, double minResponseTimeMs, double maxResponseTimeMs)
            : base(host, minResponseTimeMs, maxResponseTimeMs, valueUnitToken) { }

        public CheckIcmp(string host)
            : base(host, 0, 1000, valueUnitToken) { }

        public override void ExecuteCheck()
        {
            int timeOut = pingTimeOutDefault;
            if((int)maxValue > pingTimeOutDefault)
            {
                timeOut  = (int)maxValue;
            }

            try
            {
                if (input == null || String.IsNullOrWhiteSpace((string)input))
                {
                    Output = "no valid host given";
                    CurrentValue = -1;
                    return;
                }

                Ping ping = new Ping();
                PingReply reply = ping.Send((string)input, timeOut);

                if (reply.Status == IPStatus.Success)
                {
                    Output = String.Format("answer in {0:N0}ms", reply.RoundtripTime);
                    CurrentValue = reply.RoundtripTime;
                }
                else
                {
                    Output = String.Format("no answer due to status {0}", reply.Status);
                    CurrentValue = -1;
                }
            }
            catch (PingException exPing)
            {
                Output = String.Format("no answer within {0:N0}ms. Result: {1}", timeOut, exPing.Message);
                CurrentValue = -2;
            }
            catch (Exception ex)
            {
                Output = String.Format("failed. Error: {0}", ex.Message);
                CurrentValue = -3;
            }

            LastCheck = DateTime.Now;
        }

        protected override string GetName()
        {
            if (input != null && !String.IsNullOrWhiteSpace((string)input))
            {
                return String.Format("PING {0}", (string)input);
            }
            else
            {
                return "host unknown";
            }
        }


    }
}
