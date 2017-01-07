using System;
using System.IO;

namespace TRoschinsky.Lib.PushMonitoring.CheckTypes
{
    public class CheckFileContent : CheckByProperties
    {
        private const int maxFileSizeInMB = 500;

        public CheckFileContent(string filePath, int peekFromOffset, int peekToOffset, Comparison compareAs, string expectedContent)
            : base(filePath, new Tuple<object, object, object, object>(peekFromOffset, peekToOffset, null, compareAs), expectedContent)
        {
            this.compareType = (Comparison)searchFor.Item4;
        }

        public CheckFileContent(string filePath, string searchForStartString, string searchForEndString, Comparison compareAs, string expectedContent)
            : base(filePath, new Tuple<object, object, object, object>(searchForStartString, searchForEndString, null, compareAs), expectedContent)
        {
            this.compareType = (Comparison)searchFor.Item4;
        }

        public CheckFileContent(string filePath, Comparison compareAs, string expectedContent)
            : base(filePath, new Tuple<object, object, object, object>(null, null, null, compareAs), expectedContent)
        {
            this.compareType = (Comparison)searchFor.Item4;
        }

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
                        int fileSizeInMB = Convert.ToInt32(fileInfo.Length / 1024 / 1024);

                        if (fileSizeInMB > maxFileSizeInMB)
                        {
                            Output = String.Format("file exceeds limit of {0} MB with a total size of {1} MB. Consider using 'CheckFileState' for size monitoring");
                            return;
                        }

                        string fileContent = File.ReadAllText(fileInfo.FullName);

                        if (SearchFor.Item1 != null && SearchFor.Item2 != null)
                        {
                            if (SearchFor.Item1.GetType() == typeof(int) && SearchFor.Item2.GetType() == typeof(int))
                            {
                                CurrentValue = fileContent.Substring((int)SearchFor.Item1, ((int)SearchFor.Item2 - (int)SearchFor.Item1));
                                Output = String.Format("content between 0x{0:x} and 0x{1:x} {3} (by '{2}') the given value", SearchFor.Item1, SearchFor.Item2, CompareType, (NotifyRequired ? "mismatches" : "matches"));
                            }
                            else if (SearchFor.Item1.GetType() == typeof(string) && SearchFor.Item2.GetType() == typeof(string))
                            {
                                int indexStart = fileContent.IndexOf(SearchFor.Item1 as string);
                                int indexEnd = fileContent.IndexOf(SearchFor.Item2 as string, indexStart);
                                CurrentValue = fileContent.Substring(indexStart, (indexEnd - indexStart));
                                Output = String.Format("content between search pattern at 0x{0:x} and 0x{1:x} {3} (by '{2}') the given value", indexStart, indexEnd, CompareType, (NotifyRequired ? "mismatches" : "matches"));
                            }
                            else
                            {
                                Output = String.Format("seems the desired check type is not avilable... please check configuration");
                                return;
                            }
                        }
                        else
                        {
                            CurrentValue = fileContent;
                            Output = String.Format("the whole file {1} (by '{0}') the given value", CompareType, (NotifyRequired ? "mismatches" : "matches"));
                        }
                    }
                    else
                    {
                        CurrentValue = Output = "file is not existing";
                    }
                }
                else
                {
                    CurrentValue = Output = "file not accessible";
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
