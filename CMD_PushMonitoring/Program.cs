using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TRoschinsky.Common;
using TRoschinsky.Lib.PushMonitoring;

namespace TRoschinsky.App.PushMonitoring
{
    /// <summary>
    /// The class Program is a simple command line wrapper, calling the check and notification 
    /// logic provided by TRoschinsky.Lib.PushMonitoring.Monitoring library. It can optionally 
    /// handle a specified configuration file from a command line parameter.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entrance when program is started; writes headline and short description to output, runs
        /// initialization-method to execute logic and stops in DEBUG mode until key was pressed.
        /// </summary>
        /// <param name="args">Multiple strings passed from command line to the application</param>
        static void Main(string[] args)
        {
            try
            {
                WriteIntro();
                Execute(Initialize(args));
            }
            catch (Exception ex)
            {
                Console.WriteLine(" ...an unexpected error occurred: {0}\n", ex.Message);
            }
            
#if DEBUG
            Console.WriteLine("\n < press any key to continue >");
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// Initialization by checking command line parameter if one was entered, obtaining path 
        /// to config file and testing for existance of file.
        /// </summary>
        /// <param name="args">Multiple strings passed from command line to the application</param>
        private static FileInfo Initialize(string[] args)
        {
            FileInfo configFile = null;

            try
            {
                // Extract arguments and setup PushMonitoring instance
                if (args != null && args.Length == 1)
                {
                    configFile = new FileInfo(args[0]);
                    if (configFile != null && configFile.Exists)
                    {
                        Console.WriteLine(" ...using config file given \"{0}\"", configFile.Name);
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
                    }
                    else
                    {
                        Console.WriteLine(" ...could not find/access default config file \"{0}\"", configFile.Name);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(" ...an unexpected error occurred: {0}", ex.Message);
            }

            return configFile;
        }

        private static void Execute(FileInfo configFile)
        {
            if (configFile != null)
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

                Console.WriteLine();
                Console.WriteLine(" Extended Log:");
                foreach(JournalEntry entry in monitor.LogEntries)
                {
                    Console.WriteLine(entry);
                }
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
