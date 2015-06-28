﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TRoschinsky.Lib.PushMonitoring;

namespace TRoschinsky.App.PushMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo configFile;

            try
            {
                Console.WriteLine(" *** {0} ({1}) ***", GetAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title).Replace("CMD_", String.Empty), GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright));
                Console.WriteLine(" {0}\n", GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a.Description));

                if(args != null && args.Length == 1)
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
            
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void Exceute(FileInfo configFile)
        {
            Monitoring monitor = new Monitoring(configFile);
            Console.WriteLine(monitor.LastCheckResult);
        }

        #region Helper

        private static string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }

        #endregion
    }
}
