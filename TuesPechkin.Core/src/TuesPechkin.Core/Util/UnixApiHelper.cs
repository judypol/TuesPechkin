using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TuesPechkin.Core.Util
{
    internal class UnixLibraryLoaderLogic : ILibraryLoaderLogic
    {
        public IntPtr LoadLibrary(string fileName)
        {
            var libraryHandle = IntPtr.Zero;

            try
            {
                libraryHandle = dlopen(fileName, RTLD_NOW);
                if (libraryHandle != IntPtr.Zero)
                    Tracer.Trace($"Successfully loaded native library \"{fileName}\", handle = {libraryHandle}.");
                else
                    Tracer.Trace($"Failed to load native library \"{fileName}\".\r\nCheck windows event log.");

                Console.WriteLine($"dlopen:{libraryHandle}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var lastError = UnixGetLastError();
                Tracer.Trace($"Failed to load native library \"{fileName}\".\r\nLast Error:{lastError}\r\nCheck inner exception and\\or windows event log.\r\nInner Exception: {e.ToString()}");
            }

            return libraryHandle;
        }

        public bool FreeLibrary(IntPtr libraryHandle)
        {
            return dlclose(libraryHandle) != 0;
        }

        public IntPtr GetProcAddress(IntPtr libraryHandle, string functionName)
        {
            UnixGetLastError(); // Clearing previous errors
            Tracer.Trace($"Trying to load native function \"{functionName}\" from the library with handle {libraryHandle}...");
            var functionHandle = UnixGetProcAddress(libraryHandle, functionName);
            var errorPointer = UnixGetLastError();
            if (errorPointer != IntPtr.Zero)
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errorPointer));
            if (functionHandle != IntPtr.Zero && errorPointer == IntPtr.Zero)
                Tracer.Trace($"Successfully loaded native function \"{functionName}\", function handle = {functionHandle}.");
            else
                Tracer.Trace($"Failed to load native function \"{functionName}\", function handle = {functionHandle}, error pointer = {errorPointer}");
            return functionHandle;
        }

        public string FixUpLibraryName(string fileName)
        {
            //var file = fileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!fileName.EndsWith(".so", StringComparison.OrdinalIgnoreCase))
                    fileName += ".so";
                //if (!fileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
                //    fileName = "lib" + fileName;
            }
            return fileName;
        }

        const int RTLD_NOW = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so", EntryPoint = "dlclose")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr UnixGetProcAddress(IntPtr handle, String symbol);

        [DllImport("libdl.so", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr UnixGetLastError();
    }
}
