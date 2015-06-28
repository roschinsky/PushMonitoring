using System;
using System.Net;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckIcmp : CheckByValue
    {
        public CheckIcmp(object input)
            : base(input, "ms")
        {

        }
    }
}
