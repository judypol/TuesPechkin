using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TuesPechkin.Tests
{
    public abstract class TuesPechkinTests
    {
        protected const string TEST_WK_VER = "0.12.3";
        protected const string TEST_URL = "https://www.jd.com";

        // Simulates 1.x.x
        protected static readonly ThreadSafeConverter converter;

        protected static readonly IToolset toolset;
                
        static TuesPechkinTests()
        {
            toolset =
                new RemotingToolset<PdfToolset>(
                    new StaticDeployment(
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wk-ver",
                            TEST_WK_VER)));
            converter = new ThreadSafeConverter(toolset);
        }

        public void TestCleanup()
        {
            toolset.Unload();
        }

        protected static string GetDeploymentPath()
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "wk-ver",
                TEST_WK_VER);
        }

        protected IConverter GetNewConverter()
        {
            return new StandardConverter(GetNewToolset());
        }

        protected IToolset GetNewToolset()
        {
            return new PdfToolset(GetNewDeployment());
        }

        protected static IDeployment GetNewDeployment()
        {
            return new StaticDeployment(GetDeploymentPath());
        }

        protected HtmlToPdfDocument Document(params ObjectSettings[] objects)
        {
            var doc = new HtmlToPdfDocument();
            doc.Objects.AddRange(objects);

            return doc;
        }

        protected ObjectSettings StringObject()
        {
            var html = GetResourceString("TuesPechkin.Tests.Resources.page.html");

            return new ObjectSettings { HtmlText = html };
            //return new ObjectSettings { PageUrl = "http://www.baidu.com" };
        }

        protected ObjectSettings UrlObject()
        {
            return new ObjectSettings { PageUrl = TEST_URL,WebSettings=new WebSettings {
                EnableJavascript=true,
                EnablePlugins=true,
                LoadImages=true,
            },LoadSettings=new LoadSettings { RenderDelay=1500} };
        }

        protected static string GetResourceString(string name)
        {
            if (name == null)
            {
                return null;
            }

            using (var s = new FileStream("Resources/page.html", FileMode.Open))
            {
                if (s == null)
                {
                    return null;
                }

                return new StreamReader(s).ReadToEnd();
            }  
        }
    }
}
