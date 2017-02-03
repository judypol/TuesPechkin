using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TuesPechkin.Core.Util;

namespace TuesPechkin
{
    [Serializable]
    public sealed class TempFolderDeployment : IDeployment
    {
        public string Path { get; private set; }

        public TempFolderDeployment()
        {
            // Scope it to the application
            //Path = System.IO.Path.Combine(
            //    System.IO.Path.GetTempPath(),
            //    AppDomain.CurrentDomain.BaseDirectory.GetHashCode().ToString());
            var os = SystemOsManager.GetOperatingSystem();
            if(os==OperatingSystem.Windows)
            {
                Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                Directory.GetCurrentDirectory().GetHashCode().ToString());

                // Scope it by bitness, too
                Path = System.IO.Path.Combine(
                    Path,
                    IntPtr.Size.ToString());
            }
            else
            {
                Path =System.IO.Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
                Console.WriteLine($"TempFolderDeployment:{Path}");
            }
            
        }
    }
}
