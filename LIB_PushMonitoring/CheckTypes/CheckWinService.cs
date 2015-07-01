using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckWinService : CheckByState
    {
        public CheckWinService(string serviceName)
            : base(serviceName) { }

        public override void ExecuteCheck()
        {
            CheckSuccessful = false;

            try
            {
                if (input == null || String.IsNullOrWhiteSpace((string)input))
                {
                    Output = "no or empty service name given";
                    return;
                }

                List<ServiceController> services = new List<ServiceController>();
                services.AddRange(ServiceController.GetServices());

                ServiceController svcCtrl = services.Find(s => s.ServiceName == (string)input || s.DisplayName == (string)input);

                if (svcCtrl != null)
                {
                    Output = String.Format("state is {0}", svcCtrl.Status);
                    if (svcCtrl.Status == ServiceControllerStatus.Running)
                    {
                        CheckSuccessful = true;
                    }
                }
                else
                {
                    Output = String.Format("service not found");
                }
            }
            catch (Exception ex)
            {
                Output = String.Format("error '{0}'", ex.Message);
            }

            LastCheck = DateTime.Now;
        }

        protected override string GetName()
        {
            if (input != null && !String.IsNullOrWhiteSpace((string)input))
            {
                return String.Format("SERVICE {0}", (string)input);
            }
            else
            {
                return "service unknown";
            }
        }
    }
}
