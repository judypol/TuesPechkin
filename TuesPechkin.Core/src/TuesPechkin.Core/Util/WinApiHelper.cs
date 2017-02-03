using System;
using System.Runtime.InteropServices;
using TuesPechkin.Core.Util;

namespace TuesPechkin
{
    internal class WindowsLibraryLoaderLogic : ILibraryLoaderLogic
    {
        public IntPtr LoadLibrary(string fileName)
        {
            var libraryHandle = IntPtr.Zero;

            try
            {
                Tracer.Trace($"Trying to load native library \"{fileName}\"...");
                libraryHandle = WindowsLoadLibrary(fileName);
                if (libraryHandle != IntPtr.Zero)
                    Tracer.Trace($"Successfully loaded native library \"{fileName}\", handle = {libraryHandle}.");
                else
                    Tracer.Trace($"Failed to load native library \"{fileName}\".\r\nCheck windows event log.");
            }
            catch (Exception e)
            {
                var lastError = WindowsGetLastError();
                Tracer.Trace($"Failed to load native library \"{fileName}\".\r\nLast Error:{lastError}\r\nCheck inner exception and\\or windows event log.\r\nInner Exception: {e.ToString()}");
            }

            return libraryHandle;
        }

        public bool FreeLibrary(IntPtr libraryHandle)
        {
            try
            {
                Tracer.Trace($"Trying to free native library with handle {libraryHandle} ...");
                var isSuccess = WindowsFreeLibrary(libraryHandle);
                if (isSuccess)
                    Tracer.Trace($"Successfully freed native library with handle {libraryHandle}.");
                else
                    Tracer.Trace($"Failed to free native library with handle {libraryHandle}.\r\nCheck windows event log.");
                return isSuccess;
            }
            catch (Exception e)
            {
                var lastError = WindowsGetLastError();
                Tracer.Trace($"Failed to free native library with handle {libraryHandle}.\r\nLast Error:{lastError}\r\nCheck inner exception and\\or windows event log.\r\nInner Exception: {e.ToString()}");
                return false;
            }
        }

        public IntPtr GetProcAddress(IntPtr libraryHandle, string functionName)
        {
            try
            {
                Tracer.Trace($"Trying to load native function \"{functionName}\" from the library with handle {libraryHandle}...");
                var functionHandle = WindowsGetProcAddress(libraryHandle, functionName);
                if (functionHandle != IntPtr.Zero)
                    Tracer.Trace($"Successfully loaded native function \"{functionName}\", function handle = {functionHandle}.");
                else
                    Tracer.Trace($"Failed to load native function \"{functionName}\", function handle = {functionHandle}");
                return functionHandle;
            }
            catch (Exception e)
            {
                var lastError = WindowsGetLastError();
                Tracer.Trace($"Failed to free native library with handle {libraryHandle}.\r\nLast Error:{lastError}\r\nCheck inner exception and\\or windows event log.\r\nInner Exception: {e.ToString()}");
                return IntPtr.Zero;
            }
        }

        public string FixUpLibraryName(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName) && !fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return fileName + ".dll";
            return fileName;
        }

        [DllImport("kernel32", EntryPoint = "LoadLibrary", CallingConvention = CallingConvention.Winapi,
            SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern IntPtr WindowsLoadLibrary(string dllPath);

        [DllImport("kernel32", EntryPoint = "FreeLibrary", CallingConvention = CallingConvention.Winapi,
            SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern bool WindowsFreeLibrary(IntPtr handle);

        [DllImport("kernel32", EntryPoint = "GetProcAddress", CallingConvention = CallingConvention.Winapi,
            SetLastError = true)]
        private static extern IntPtr WindowsGetProcAddress(IntPtr handle, string procedureName);

        private static int WindowsGetLastError()
        {
            return Marshal.GetLastWin32Error();
        }
    }
}