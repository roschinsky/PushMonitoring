using System;
using System.IO;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    class CheckFileState : CheckByState
    {
        public CheckFileState(string filePath)
            : base(filePath) { }

        public override void ExecuteCheck()
        {
            try
            {
                if (input == null || String.IsNullOrWhiteSpace(input as string))
                {
                    Output = "no or empty file name given";
                    return;
                }

                FileInfo fileInfo = new FileInfo(input as string);
                if (fileInfo != null)
                {
                    if (fileInfo.Exists)
                    {
                        Output = String.Format("file exists; created on {0} and modified on {1}", fileInfo.CreationTime, fileInfo.LastWriteTime);
                        CheckSuccessful = true;
                    }
                    else
                    {
                        Output = "file is not existing";
                    }
                }
                else
                {
                    Output = "file not accessible";
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
    }
}
