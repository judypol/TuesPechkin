using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TuesPechkin.Core.ConsoleTest
{
    public class TuesPechkinePdf
    {
        public static IConverter converter = new ThreadSafeConverter(
            new RemotingToolset<PdfToolset>(
                    new AnyCPUEmbeddedDeployment(new TempFolderDeployment()))
            );
        public void CreatePdf(string filename)
        {
            //var toolset =
            //    new RemotingToolset<PdfToolset>(
            //        new AnyCPUEmbeddedDeployment(new TempFolderDeployment()));
            //var converter = new ThreadSafeConverter(toolset);

            byte[] result = null;
            lock (converter)
            {
                result = converter.Convert(Document(UrlObject()));
            }
            
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                fs.Write(result, 0, result.Length);
            }
        }
        protected HtmlToPdfDocument Document(params ObjectSettings[] objects)
        {
            var doc = new HtmlToPdfDocument();
            //doc.GlobalSettings.PaperSize = new PechkinPaperSize("297", "420");
            doc.Objects.AddRange(objects);

            return doc;
        }
        protected ObjectSettings UrlObject()
        {
            return new ObjectSettings
            {
                PageUrl = "https://www.shcem.com/",
                WebSettings = new WebSettings
                {
                    EnableJavascript = true,
                    EnablePlugins = true,
                    LoadImages = true,
                    DefaultEncoding="utf-8"
                },
                HeaderSettings=new HeaderSettings { CenterText="我是头部！"},
                FooterSettings=new FooterSettings { CenterText="我是尾部！"},
                LoadSettings = new LoadSettings { RenderDelay = 1500 }
            };
        }
    }
}
