using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TuesPechkin.Tests
{
    public class GeneralTests : TuesPechkinTests
    {
        [Fact]
        public void BubblesExceptionsFromSyncedThread()
        {
            var toolset = new BogusToolset();
            var converter = new ThreadSafeConverter(toolset);

            try
            {
               var result= converter.Convert(Document(StringObject()));
            }
            catch (NotImplementedException) { }

            toolset.Unload(); // Needed for testing framework to succeed
        }

        //[Fact]
        //public void ConvertsAfterAppDomainRecycles()
        //{
        //    // arrange
        //    var domain1 = this.GetAppDomain("testing_unload_1");
        //    byte[] result1 = null;
        //    var domain2 = this.GetAppDomain("testing_unload_2");
        //    byte[] result2 = null;
        //    CrossAppDomainDelegate callback = () =>
        //    {
        //        var dllPath = AppDomain.CurrentDomain.GetData("dllpath") as string;

        //        var converter =
        //            new ThreadSafeConverter(
        //                new RemotingToolset<PdfToolset>(
        //                    new StaticDeployment(dllPath)));

        //        var document = new HtmlToPdfDocument 
        //        { 
        //            Objects = 
        //            { 
        //                new ObjectSettings { PageUrl = "www.google.com" } 
        //            } 
        //        };

        //        AppDomain.CurrentDomain.SetData("result", converter.Convert(document));
        //    };

        //    // act
        //    domain1.SetData("dllpath", GetDeploymentPath());
        //    domain1.DoCallBack(callback);
        //    result1 = domain1.GetData("result") as byte[];
        //    AppDomain.Unload(domain1);

        //    domain2.SetData("dllpath", GetDeploymentPath());
        //    domain2.DoCallBack(callback);
        //    result2 = domain2.GetData("result") as byte[];
        //    AppDomain.Unload(domain2);

        //    // assert
        //    Assert.IsNotNull(result1);
        //    Assert.IsNotNull(result2);
        //}

        [Fact]
        public void HandlesConcurrentThreads()
        {
            int numberOfTasks = 10;
            int completed = 0;

            var tasks = Enumerable.Range(0, numberOfTasks).Select(i => new Task(() =>
            {
                Debug.WriteLine(String.Format("#{0} started", i + 1));
                Assert.NotNull(converter.Convert(Document(StringObject())));
                completed++;
                Debug.WriteLine(String.Format("#{0} completed", i + 1));
            }));

            Parallel.ForEach(tasks, task => task.Start());

            while (completed < numberOfTasks)
            {
                // tried using Task.WaitAll but it blocked the test engine
                Thread.Sleep(100);
            }
        }

        [Fact]
        public void TwoSequentialConversionsFromString()
        {
            byte[] result = null;

            result = converter.Convert(Document(StringObject()));

            Assert.NotNull(result);

            result = converter.Convert(Document(StringObject()));

            Assert.NotNull(result);
        }

        [Fact]
        public void MultipleObjectConversionFromString()
        {            
            // See: https://github.com/wkhtmltopdf/wkhtmltopdf/issues/1790

            var result = converter.Convert(Document("Hey girl", "What's up"));

            Assert.NotNull(result);
        }

        [Fact]
        public void TwoSequentialConversionsFromUrl()
        {
            byte[] result = null;

            result = converter.Convert(Document(UrlObject()));

            using (var fs = new FileStream("www.pdf", FileMode.Create))
            {
                fs.Write(result, 0, result.Length);
            }
                Assert.NotNull(result);

            //result = converter.Convert(Document(UrlObject()));

            //Assert.NotNull(result);
        }

        [Fact]
        public void MultipleObjectConversionFromUrl()
        {
            var result = converter.Convert(Document(UrlObject(), UrlObject()));

            Assert.NotNull(result);
        }

        [Fact]
        public void ResultIsPdf()
        {
            string html = GetResourceString("PechkinTests.Resources.page.html");

            byte[] ret = converter.Convert(Document(StringObject()));

            Assert.NotNull(ret);

            byte[] right = Encoding.UTF8.GetBytes("%PDF");

            Assert.True(right.Length <= ret.Length);

            byte[] test = new byte[right.Length];
            Array.Copy(ret, 0, test, 0, right.Length);

            for (int i = 0; i < right.Length; i++)
            {
                Assert.Equal(right[i], test[i]);
            }
        }

        [Fact]
        public void ReturnsResultFromFile()
        {
            string html = GetResourceString("PechkinTests.Resources.page.html");

            string fn = string.Format("{0}.html", Path.GetTempFileName());
            FileStream fs = new FileStream(fn, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(html);

            sw.Flush();

            byte[] ret = converter.Convert(new HtmlToPdfDocument
            {
                Objects = { 
                    new ObjectSettings { PageUrl = fn } 
                }
            });

            Assert.NotNull(ret);

            File.Delete(fn);
        }

        [Fact]
        public void ReturnsResultFromString()
        {
            var document = new HtmlToPdfDocument("<p>some html</p>");

            var result = converter.Convert(document);

            Assert.NotNull(result);
        }

       // [Fact]
       // public void UnloadsWkhtmltoxWhenAppDomainUnloads()
       // {
       //     // arrange
       //     var domain = GetAppDomain("testing_unload");

       //     // act
       //     domain.DoCallBack(() =>
       //     {
       //         var converter =
       //             new ThreadSafeConverter(
       //                 new RemotingToolset<PdfToolset>(
							//new WinAnyCPUEmbeddedDeployment(
       //                         new StaticDeployment(Path.GetTempPath()))));

       //         var document = new HtmlToPdfDocument("<p>some html</p>");

       //         converter.Convert(document);
       //     });
       //     AppDomain.Unload(domain);

       //     // assert
       //     Assert.IsFalse(Process
       //         .GetCurrentProcess()
       //         .Modules
       //         .Cast<ProcessModule>()
       //         .Any(m => m.ModuleName == "wkhtmltox.dll"));
       // }
    }
}