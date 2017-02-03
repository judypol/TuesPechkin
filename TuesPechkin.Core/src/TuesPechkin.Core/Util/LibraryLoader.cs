using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TuesPechkin.Core.Util;

namespace TuesPechkin.Core.Util
{
    public class LibraryLoader
    {
        private readonly ILibraryLoaderLogic logic;

        private LibraryLoader(ILibraryLoaderLogic logic)
        {
            this.logic = logic;
        }

        private readonly object syncLock = new object();
        private readonly Dictionary<string, IntPtr> loadedAssemblies = new Dictionary<string, IntPtr>();

        public IntPtr LoadLibrary(string fileName, string platformName = null)
        {
            fileName = FixUpLibraryName(fileName);
            lock (syncLock)
            {
                if (!loadedAssemblies.ContainsKey(fileName))
                {
                    if (platformName == null)
                        platformName = SystemOsManager.GetPlatformName();
                    Tracer.Trace("Current platform: " + platformName);
                    //IntPtr dllHandle = CheckExecutingAssemblyDomain(fileName, platformName);
                    //if (dllHandle == IntPtr.Zero)
                    //    dllHandle = CheckCurrentAppDomain(fileName, platformName);
                    //if (dllHandle == IntPtr.Zero)
                    //    dllHandle = CheckWorkingDirecotry(fileName, platformName);
                    var dllHandle = logic.LoadLibrary(fileName);

                    if (dllHandle != IntPtr.Zero)
                        loadedAssemblies[fileName] = dllHandle;
                    else
                        throw new DllNotFoundException(string.Format("Failed to find library \"{0}\" for platform {1}.", fileName, platformName));
                }
                return loadedAssemblies[fileName];
            }
        }

        //private IntPtr CheckExecutingAssemblyDomain(string fileName, string platformName)
        //{
        //    var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    return InternalLoadLibrary(baseDirectory, platformName, fileName);
        //}

        //private IntPtr CheckCurrentAppDomain(string fileName, string platformName)
        //{
        //    var baseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        //    return InternalLoadLibrary(baseDirectory, platformName, fileName);
        //}

        //private IntPtr CheckWorkingDirecotry(string fileName, string platformName)
        //{
        //    var baseDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
        //    return InternalLoadLibrary(baseDirectory, platformName, fileName);
        //}

        //private IntPtr InternalLoadLibrary(string baseDirectory, string platformName, string fileName)
        //{
        //    var fullPath = Path.Combine(baseDirectory, Path.Combine(platformName, fileName));
        //    return File.Exists(fullPath) ? logic.LoadLibrary(fullPath) : IntPtr.Zero;
        //}

        public bool FreeLibrary(string fileName)
        {
            fileName = FixUpLibraryName(fileName);
            lock (syncLock)
            {
                if (!IsLibraryLoaded(fileName))
                {
                    Tracer.Trace($"Failed to free library \"{fileName}\" because it is not loaded");
                    return false;
                }
                if (logic.FreeLibrary(loadedAssemblies[fileName]))
                {
                    loadedAssemblies.Remove(fileName);
                    return true;
                }
                return false;
            }
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string name)
        {
            return logic.GetProcAddress(dllHandle, name);
        }

        public bool IsLibraryLoaded(string fileName)
        {
            fileName = FixUpLibraryName(fileName);
            lock (syncLock)
                return loadedAssemblies.ContainsKey(fileName);
        }

        private string FixUpLibraryName(string fileName)
        {
            return logic.FixUpLibraryName(fileName);

        }

        #region Singleton

        private static LibraryLoader instance;

        public static LibraryLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    var operatingSystem = SystemOsManager.GetOperatingSystem();
                    switch (operatingSystem)
                    {
                        case OperatingSystem.Windows:
                            Tracer.Trace("Current OS: Windows");
                            instance = new LibraryLoader(new WindowsLibraryLoaderLogic());
                            break;
                        case OperatingSystem.Linux:
                            Tracer.Trace("Current OS: Unix");
                            instance = new LibraryLoader(new UnixLibraryLoaderLogic());
                            break;
                        case OperatingSystem.MacOSX:
                            throw new Exception("Unsupported operation system");
                        default:
                            throw new Exception("Unsupported operation system");
                    }
                }
                return instance;
            }
        }

        #endregion
    }
}
