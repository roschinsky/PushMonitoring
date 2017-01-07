using System;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes.CheckTypes
{
    public class CheckWinUptime : CheckByValue
    {
        public CheckWinUptime(object input)
            : base(input, "h")
        {

        }

        public override void ExecuteCheck()
        {
            // TODO: Implement check
            Output = "check not implemented yet";
            LastCheck = DateTime.Now;
        }
    }
}
