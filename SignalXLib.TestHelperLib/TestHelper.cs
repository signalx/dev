namespace SignalXLib.TestHelperLib
{
    using Microsoft.Owin.Hosting;
    using NHtmlUnit;
    using SignalXLib.Lib;
    using SignalXLib.Tests;
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
    using BrowserVersion = com.gargoylesoftware.htmlunit.BrowserVersion;

    public class TestHelper
    {
        public static void CheckExpectations(Action operation, string url, string html)
        {
            Thread thread;
            using (WebApp.Start<Startup>(url))
            {
                thread = new Thread(() =>
                {
                    try
                    {
                        WebClient webClient = new WebClient(new NHtmlUnit.BrowserVersion(BrowserVersion.CHROME))
                        {
                            JavaScriptEnabled = true,
                            ThrowExceptionOnFailingStatusCode = true,
                            ThrowExceptionOnScriptError = true
                        };
                        webClient.GetPage(url);
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
        private static void KillTheThread(Thread thread)
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

        public TestObject SetUpScriptForTest(Func<TestObject, string> scriptFunc)
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

        public static TestObject SetupGeneralTest()
        {
            string groupName = Guid.NewGuid().ToString();
            var groupWatcher = "groupWatcher";
            var groupWatcher2 = "groupWatcher2";
            var clientGroupReceiver = "groupClient";
            TestHelper TestHelper = new TestHelper();
            //SET UP CLIENT
            TestObject testObject = TestHelper.SetUpScriptForTest(
                (testData) => @"
                                var promise = {}
								signalx.client." + testData.clientHandler + @"=function (m) {
							      signalx.server." + testData.testServerFeedbackHandler + @"(m,function(m2){
                                     signalx.server." + testData.testServerFeedbackHandler2 + @"(m2,function(m3){
                                          promise = signalx.server." + testData.testServerFeedbackHandler3 + @"(m3);
                                          promise.then(function(m4){
                                             signalx.server." + testData.testServerFeedbackHandler4 + @"(m4);
                                          });
                                     });
                                  });
							    };
                                signalx.client." + clientGroupReceiver + @"=function(c){
                                  signalx.server." + groupWatcher2 + @"(c,function(){});
                                };
                               signalx.ready(function (server) {
                                  signalx.groups.join('" + groupName + @"',function(c){
                                             signalx.server." + groupWatcher + @"(c);
                                          });
							      server." + testData.serverHandler + @"('" + testData.message + @"','" + testData.clientHandler + @"');
                                });");

            testObject.verifiedJoinedGroup = false;
            testObject.verifiedJoinedGroup2 = false;
            //SET UP SERVER
            SignalX.Server(
                groupWatcher,
                request =>
                {
                    testObject.verifiedJoinedGroup = request.Message as string == groupName;
                    SignalX.RespondToAll(clientGroupReceiver, request.Message, groupName);
                });
            SignalX.Server(groupWatcher2,
                request =>
                {
                    testObject.verifiedJoinedGroup2 = request.Message as string == groupName;
                });
            SignalX.Server(testObject.serverHandler,
                request =>
                {
                    SignalX.RespondToAll(testObject.clientHandler, testObject.message);
                });
            SignalX.Server(
                testObject.testServerFeedbackHandler,
                request =>
                {
                    testObject.finalMessage = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage);
                });
            SignalX.Server(
                testObject.testServerFeedbackHandler2,
                request =>
                {
                    testObject.finalMessage2 = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage2);
                });
            SignalX.Server(
                testObject.testServerFeedbackHandler3,
                request =>
                {
                    testObject.finalMessage3 = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage3);
                });
            SignalX.Server(testObject.testServerFeedbackHandler4,
                request =>
                {
                    testObject.finalMessage4 = request.Message as string;
                });

            return testObject;
        }

        public static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation = null)
        {
            maxDuration = maxDuration ?? TimeSpan.FromSeconds(5);
            var start = DateTime.Now;
            bool done = false;
            Exception lastException = null;
            while ((DateTime.Now - start).TotalMilliseconds < maxDuration.Value.TotalMilliseconds && !done)
            {
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                try
                {
                    operation();
                    done = true;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            cleanUpOperation?.Invoke();
            if (!done)
            {
                throw new Exception("Unable to pass withing specified miliseconds" + maxDuration.Value.TotalMilliseconds + "  " + lastException?.Message, lastException);
            }
        }
    }
}