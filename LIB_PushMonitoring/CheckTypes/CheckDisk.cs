using System;
using System.IO;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckDisk : CheckByValue
    {
        private const string valueUnitToken = "%";
        public bool CheckMultipleDrives { get { return this.Input != null && this.Input.GetType() == typeof(string) && !String.IsNullOrWhiteSpace((string)this.Input); } }


        public CheckDisk(object driveLetter, double minPercentageFree, double maxPercentageFree)
            : base(driveLetter, minPercentageFree, maxPercentageFree, valueUnitToken) { }

        public CheckDisk(object driveLetter)
            : base(driveLetter, valueUnitToken) { }

        public CheckDisk(double minPercentageFree, double maxPercentageFree)
            : base(null, minPercentageFree, maxPercentageFree, valueUnitToken) { }

        public CheckDisk()
            : base(null, valueUnitToken) { }

        public override void ExecuteCheck()
        {
            long freeSpace = 0;
            long totalSpace = 0;

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach(DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    string volumeLabel = String.IsNullOrWhiteSpace(drive.VolumeLabel) ? String.Empty : String.Format(" ({0})", drive.VolumeLabel);

                    if (CheckMultipleDrives)
                    {
                        if (drive.Name == (string)Input)
                        {
                            freeSpace = drive.AvailableFreeSpace;
                            totalSpace = drive.TotalSize;
                            Output = String.Format("has {0} of {1} free space{2}.",
                                CheckDisk.BytesToString(drive.TotalFreeSpace), CheckDisk.BytesToString(drive.TotalSize), volumeLabel);
                            break;
                        }
                    }
                    else
                    {
                        freeSpace += drive.AvailableFreeSpace;
                        totalSpace += drive.TotalSize;
                        Output += String.Format("\n\t- {0} has {1} of {2} free space{3}.",
                            drive.Name, CheckDisk.BytesToString(drive.TotalFreeSpace), CheckDisk.BytesToString(drive.TotalSize), volumeLabel);
                    }
                }
            }

            CurrentValue = totalSpace > 0 ? 100 * freeSpace / totalSpace : 0;
            LastCheck = DateTime.Now;
        }

        protected override string GetName()
        {
            if (CheckMultipleDrives)
            {
                return String.Format("DRIVE {0}", this.Input);
            }
            else
            {
                return String.Format("DRIVES");
            }
        }

        private static String BytesToString(long byteCount)
        {
            try
            {
                string[] suf = { "B", "KB", "MB", "GB", "TB", "PB" };
                if (byteCount == 0)
                {
                    return "0" + suf[0];
                }
                long bytes = Math.Abs(byteCount);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return (Math.Sign(byteCount) * num).ToString() + suf[place];
            }
            catch
            {
                return "<N/A>";
            }
        }
    }
}
