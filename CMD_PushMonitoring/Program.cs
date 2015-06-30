﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TRoschinsky.Lib.PushMonitoring;

namespace TRoschinsky.App.PushMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteIntro();
            Initialize(args);
            
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void Initialize(string[] args)
        {
            FileInfo configFile;

            try
            {
                // Extract arguments and setup PushMonitoring instance
                if (args != null && args.Length == 1)
                {
                    configFile = new FileInfo(args[0]);
                    if (configFile != null && configFile.Exists)
                    {
                        Console.WriteLine(" ...using config file given \"{0}\"", configFile.Name);
                        Exceute(configFile);
                    }
                    else
                    {
                        Console.WriteLine(" ...could not find/access config file given \"{0}\"", configFile.Name);
                    }
                }
                else
                {
                    configFile = new FileInfo(Properties.Settings.Default.ConfigFileDefaultPath);
                    if (configFile != null && configFile.Exists)
                    {
                        Console.WriteLine(" ...using default config file \"{0}\"", configFile.Name);
                        Exceute(configFile);
                    }
                    else
                    {
                        Console.WriteLine(" ...could not find/access default config file \"{0}\"", configFile.Name);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("...an unexpected error occurred: {0}", ex.Message);
            }
        }

        private static void Exceute(FileInfo configFile)
        {
            Monitoring monitor = new Monitoring(configFile);
            if (monitor.Checks.Count > 0)
            {
                Console.WriteLine(" ...{0}", monitor.LastCheckResult);
            }
            else
            {
                Console.WriteLine(" ...there are no checks defined. Please examine configuration at <checksToRun>!");
            }
        }

        #region Helper

        private static void WriteIntro()
        {
            Console.WriteLine();
            Console.WriteLine(" *** {0} ({1}) ***", GetAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title).Replace("CMD_", String.Empty), GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright));
            Console.WriteLine(" -------------------------------------------------------------------------");
            string description = GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a.Description);
            foreach (string line in new List<string>(Regex.Split(description, @"(?<=\G.{74})", RegexOptions.Singleline)))
            {
                Console.WriteLine(" {0}", line);
            }
            Console.WriteLine();
        }

        private static string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }

        #endregion
    }
}
