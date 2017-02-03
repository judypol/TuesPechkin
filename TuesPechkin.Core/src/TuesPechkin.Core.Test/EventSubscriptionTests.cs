using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TuesPechkin.Tests
{
    public class EventSubscriptionTests : TuesPechkinTests
    {
        private void RunConversion(Action<IConverter> subscribe, IDocument document = null)
        {
            subscribe(converter);
            converter.Convert(document ?? Document(UrlObject(), StringObject()));
        }

        [Fact]
        public void BeginDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(c => c.Begin += (s, a) => count++);

            Assert.True(count > 0);
        }

        [Fact]
        public void ErrorDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(
                c => c.Warning += (s, a) => count++,
                Document(new ObjectSettings
                {
                    PageUrl = "nonexistent.website.com",
                    LoadSettings =
                    {
                        ErrorHandling = LoadSettings.ContentErrorHandling.Abort
                    }
                }));

            Assert.True(count > 0);
        }

        [Fact]
        public void FinishDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(c => c.Finish += (s, a) => count++);

            Assert.True(count > 0);
        }

        [Fact]
        public void PhaseChangeDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(c => c.PhaseChange += (s, a) => count++);

            Assert.True(count > 0);
        }

        [Fact]
        public void ProgressChangeDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(c => c.ProgressChange += (s, a) => count++);

            Assert.True(count > 0);
        }

        [Fact]
        public void WarningDoesNotBlockConversion()
        {
            var count = 0;

            RunConversion(
                c => c.Warning += (s, a) => count++,
                Document(new ObjectSettings 
                { 
                    PageUrl = "nonexistent.website.com",
                    LoadSettings =
                    {
                        ErrorHandling = LoadSettings.ContentErrorHandling.Abort
                    }
                }));

            Assert.True(count > 0);
        }
    }
}
