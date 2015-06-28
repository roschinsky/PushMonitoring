using System;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    class CheckHttp : CheckByValue
    {
        private const string valueUnitToken = "Statecode";


        public CheckHttp(object url, double fromStateCode, double toStateCode)
            : base(url, fromStateCode, toStateCode, valueUnitToken) { }

        public CheckHttp(string url)
            : base(url, valueUnitToken) { }


    }
}
