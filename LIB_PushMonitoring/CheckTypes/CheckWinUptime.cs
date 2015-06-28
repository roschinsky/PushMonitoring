using System;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes.CheckTypes
{
    public class CheckWinUptime : CheckByValue
    {
        public CheckWinUptime(object input)
            : base(input, "h")
        {

        }
    }
}
