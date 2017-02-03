using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TuesPechkin.Core.Util
{
    public class SystemOsManager
    {
        public static string GetPlatformName()
        {
            return IntPtr.Size == sizeof(int) ? "x86" : "x64";
        }

        public static OperatingSystem GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OperatingSystem.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OperatingSystem.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OperatingSystem.MacOSX;
            else
                return OperatingSystem.Unknown;
        }
    }

    public enum OperatingSystem
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }
}
