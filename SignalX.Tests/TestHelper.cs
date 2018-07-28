using System;
using System.Threading.Tasks;

namespace SignalXLib.Tests
{
    using System.Diagnostics;
    using System.Security.Permissions;
    using Microsoft.Owin.Hosting;
    using System.Threading;
    using Jint;
    using NHtmlUnit;
    using StepRunner;
    using BrowserVersion = com.gargoylesoftware.htmlunit.BrowserVersion;

    public class TestHelper
    {
      

        public  void CheckExpectations(Action operation, string url, string html, bool useNHtmlUnit=true)
        { 
             Thread thread;
            using (WebApp.Start<Startup>(url))
            {
                thread = new Thread(() =>
                {
                    try
                    {

                        if (useNHtmlUnit)
                        {
                            WebClient webClient = new WebClient(new NHtmlUnit.BrowserVersion(BrowserVersion.CHROME))
                            {
                                JavaScriptEnabled = true,
                                ThrowExceptionOnFailingStatusCode = true,
                                ThrowExceptionOnScriptError = true
                            };
                            webClient.GetPage(url);
                        }
                        else
                        {
                            Form1 dlg = new Form1(html);
                            dlg.ShowDialog();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

              
                TestHelper.AwaitAssert(
                    () => operation(),
                    TimeSpan.FromSeconds(1000));
               
               
                 KillTheThread(thread);
            }
        }
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
         void KillTheThread(Thread thread)
        {
            try
            {
                thread?.Interrupt();
                thread?.Abort();
                thread = null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
              
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
                            <head></head>
							<body
                            <h1> SignalX in motion .....</h1>
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