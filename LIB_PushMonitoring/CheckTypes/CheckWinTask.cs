using System;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    class CheckWinTask : CheckByState
    {
        public CheckWinTask(string taskName)
            : base(taskName) { }

        public override void ExecuteCheck()
        {
            // TODO: Implement check
            Output = "check not implemented yet";
            LastCheck = DateTime.Now;
        }

        protected override string GetName()
        {
            if (input != null && !String.IsNullOrWhiteSpace((string)input))
            {
                return String.Format("TASK {0}", (string)input);
            }
            else
            {
                return "task unknown";
            }
        }
    }
}
