using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using SysPath = System.IO.Path;
using TuesPechkin.Core.Util;

namespace TuesPechkin
{
    [Serializable]
    public class AnyCPUEmbeddedDeployment : EmbeddedDeployment
    {
        Stream stream = null;
        OperatingSystem os;
        public AnyCPUEmbeddedDeployment(IDeployment physical)
            : base(physical)
        {
            os = SystemOsManager.GetOperatingSystem();
            if(os==OperatingSystem.Windows)
            {
                if (IntPtr.Size == 8)
                {
                    stream = this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("TuesPecnkin.Core.Wkhtmltox.AnyCpu.wkhtmltox_64.dll.gz");
                }
                else
                {
                    stream = this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("TuesPecnkin.Core.Wkhtmltox.AnyCpu.wkhtmltox_32.dll.gz");
                }
            }
            else if(os==OperatingSystem.Linux)
            {
                if(IntPtr.Size==8)
                {
                    stream = this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("TuesPecnkin.Core.Wkhtmltox.AnyCpu.wkhtmltox_64.so.gz");
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            
        }
        private bool deployed;
        public override string Path
        {
            get
            {
                string path = "";
                if (os == OperatingSystem.Windows)
                {
                    path= System.IO.Path.Combine(base.Path,
                        this.GetType().GetTypeInfo().Assembly.GetName().Version.ToString());
                }
                else if (os == OperatingSystem.Linux)
                {
                    path= System.IO.Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
                    Console.WriteLine($"AnyCpu-Path{path}");
                }

                if (!deployed)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    foreach (var nameAndContents in GetContents())
                    {
                        var filename = System.IO.Path.Combine(path, nameAndContents.Key);

                        if (!File.Exists(filename))
                        {
                            base.WriteStreamToFile(filename, nameAndContents.Value);
                        }
                    }

                    deployed = true;
                }

                return path;
            }
        }

        protected override IEnumerable<KeyValuePair<string, Stream>> GetContents()
        {
            string key = WkhtmltoxBindings.DLLNAME;
            if(os==OperatingSystem.Linux)
            {
                key = key + ".so";
            }else if (os == OperatingSystem.Windows)
            {
                key += ".dll";
            }
            var gzip= new GZipStream(stream,CompressionMode.Decompress);
            return new[]
            { 
                new KeyValuePair<string, Stream>(
                    key: key,
                    value: gzip)
            };
        }
    }
}
