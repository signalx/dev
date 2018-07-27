using System;
using System.Threading.Tasks;

namespace SignalXLib.Tests
{
    using Microsoft.Owin.Hosting;
    using System.Threading;

    public class TestHelper
    {
        public static void CheckExpectations(Action operation, string url)
        {
            using (WebApp.Start<Startup>(url))
            {
                var webClient = new NHtmlUnit.WebClient(new NHtmlUnit.BrowserVersion(com.gargoylesoftware.htmlunit.BrowserVersion.CHROME))
                {
                    JavaScriptEnabled = true,
                    ThrowExceptionOnFailingStatusCode = true,
                    ThrowExceptionOnScriptError = true
                };
                var thread = new Thread(() => webClient.GetPage(url).Initialize());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                TestHelper.AwaitAssert(
                    () => operation(),
                    TimeSpan.FromSeconds(1000));
                thread?.Abort();
            }
        }

        public static TestObject SetUpScriptForTest(Func<TestObject, string> scriptFunc)
        {
            var testObject = new TestObject() { };
            testObject.finalMessage = null;
            testObject.finalMessage2 = null;
            testObject.finalMessage3 = null;
            testObject.finalMessage4 = null;
            testObject.message = Guid.NewGuid().ToString();
            testObject.clientHandler = "myclientHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.serverHandler = "myServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.testServerFeedbackHandler = "myTestServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.testServerFeedbackHandler2 = "myTestServerHandler2" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.testServerFeedbackHandler3 = "myTestServerHandler3" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.testServerFeedbackHandler4 = "myTestServerHandler4" + Guid.NewGuid().ToString().Replace("-", "");
            var script = scriptFunc(testObject);

            testObject.indexPage = TestHelper.WrapScriptInHtml(script);

            System.IO.File.WriteAllText(TestHelper.FilePath, testObject.indexPage);
            return testObject;
        }

        public static string WrapScriptInHtml(string script)
        {
            return @"<!DOCTYPE html>
							<html>
							<body>
							<script src='https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js'></script>
							<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js'></script>
							<script src='https://unpkg.com/signalx'></script>
							<script>
                               " + script + @"
							</script>
							</body>
							</html>";
        }

        public static string FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\index.html";

        public static void AwaitAssert(Action operation, Action cleanUpOperation)
        {
            AwaitAssert(operation, null, cleanUpOperation);
        }

        public static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation = null)
        {
            maxDuration = maxDuration ?? TimeSpan.FromSeconds(5);
            var start = DateTime.Now;
            bool done = false;
            while ((DateTime.Now - start).TotalMilliseconds < maxDuration.Value.TotalMilliseconds && !done)
            {
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                try
                {
                    operation();
                    done = true;
                }
                catch (Exception)
                {
                }
            }
            cleanUpOperation?.Invoke();
            if (!done)
            {
                throw new Exception("Unable to pass withing specified miliseconds" + maxDuration.Value.TotalMilliseconds);
            }
        }
    }
}