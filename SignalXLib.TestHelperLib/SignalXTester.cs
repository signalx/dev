namespace SignalXLib.TestHelperLib
{
    using com.gargoylesoftware.htmlunit;
    using Microsoft.Owin.Hosting;
    using SignalXLib.Lib;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
    using WebClient = NHtmlUnit.WebClient;

    public class SignalXTester
    {
        private static SignalX SignalX;
        public static TimeSpan MaxTestWaitTime = TimeSpan.FromSeconds(20);

        public static string FileName;
        public static string FilePath;

        public static void SetSignalXInstance(SignalX signalX)
        {
            SignalX = signalX;
        }

        public static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static void Run(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun)
        {
            using (var isolated = new Isolated<IsolationFacade>())
            {
                isolated.Value.a = fun;
                isolated.Value.DoSomething();
            }
        }

        internal static void Run(SignalX signalX, SignalXTestDefinition scenarioDefinition)

        {
            var testHelper = new SignalXTester();
            SetSignalXInstance(signalX);
            // script
            string script = scenarioDefinition.Script;
            //setup
            TestObject testObject = SetUpScriptForTest(testData => script);
            //server
            //test : run and assert

            scenarioDefinition.TestEvents = scenarioDefinition.TestEvents ?? new TestEventHandler();
            scenarioDefinition.TestEvents.OnAppStarted = () => { scenarioDefinition?.Server(); };
            scenarioDefinition.TestEvents.OnFinally = (e) => { signalX.Dispose(); };
            CheckExpectations(
                () => { scenarioDefinition?.Checks(); },
                "http://localhost:" + FreeTcpPort(),
                testObject,
                scenarioDefinition.BrowserType,
                scenarioDefinition.TestEvents);
        }

        public static void CheckExpectationsExpectingFailures(Action operation, string url, TestObject html)
        {
            try
            {
                CheckExpectations(operation, url, html);
                throw new Exception("Expected test to fail but it passed");
            }
            catch (Exception e)
            {
            }
        }

        public static void CheckExpectations(Action operation, string url, TestObject testObject, BrowserType browserType = BrowserType.Unknown, TestEventHandler events = null)
        {
            Thread thread;
            Process browserProcess = null;
            SignalX.Settings.LogAgentMessagesOnClient = true;
            SignalX.Settings.ManageUserConnections = true;
            using (WebApp.Start<Startup>(url))
            {
                events?.OnAppStarted?.Invoke();

                thread = new Thread(
                    () =>
                    {
                        try
                        {
                            if (browserType == BrowserType.Unknown ||browserType == BrowserType.DefaultSystemBrowser )
                                browserProcess = Process.Start(url + testObject.FileName);
                            if (browserType == BrowserType.HeadlessBrowser)
                            {
                                var webClient = new WebClient(new NHtmlUnit.BrowserVersion(BrowserVersion.CHROME))
                                {
                                    JavaScriptEnabled = true,
                                    ThrowExceptionOnFailingStatusCode = true,
                                    ThrowExceptionOnScriptError = true
                                };
                                webClient.GetPage(url + testObject.FileName.Replace("\\", "/"));
                            }

                            AwaitAssert(
                                () =>
                                {
                                    if (!SignalX.Settings.HasOneOrMoreConnections)
                                        throw new Exception("No connection received from any client");
                                },
                                TimeSpan.FromSeconds(10));

                            events?.OnClientLoaded?.Invoke();
                        }
                        catch (Exception e)
                        {
                            events?.OnClientError?.Invoke(e);
                        }
                    });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                AwaitAssert(
                    () =>
                    {
                        operation();
                        //if we got this far, then we made it
                        events?.OnCheckSucceeded?.Invoke();
                    },
                    MaxTestWaitTime,
                    null,
                    ex =>
                    {
                        events?.OnFinally?.Invoke(ex);
                        try
                        {
                            browserProcess?.Kill();
                        }
                        catch (Exception e)
                        {
                        }

                        try
                        {
                            File.Delete(FilePath);
                        }
                        catch (Exception e)
                        {
                        }

                        try
                        {
                            KillTheThread(thread);
                        }
                        catch (Exception e)
                        {
                        }
                    }, events?.OnCheckFailures);
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

        public static TestObject SetUpScriptForTest(Func<TestObject, string> scriptFunc)
        {
            var testObject = new TestObject();
            testObject.FinalMessage = null;
            testObject.FinalMessage2 = null;
            testObject.FinalMessage3 = null;
            testObject.FinalMessage4 = null;
            testObject.Message = Guid.NewGuid().ToString();
            testObject.ClientHandler = "myclientHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.ServerHandler = "myServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.TestServerFeedbackHandler = "myTestServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.TestServerFeedbackHandler2 = "myTestServerHandler2" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.TestServerFeedbackHandler3 = "myTestServerHandler3" + Guid.NewGuid().ToString().Replace("-", "");
            testObject.TestServerFeedbackHandler4 = "myTestServerHandler4" + Guid.NewGuid().ToString().Replace("-", "");
            string script = scriptFunc(testObject);

            testObject.PageHtml = WrapScriptInHtml(script);
            FileName = "\\index" + Guid.NewGuid() + ".html";
            FilePath = AppDomain.CurrentDomain.BaseDirectory + FileName;
            File.WriteAllText(FilePath, testObject.PageHtml);
            testObject.FileName = FileName;
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

        public static void AwaitAssert(Action operation, Action cleanUpOperation)
        {
            AwaitAssert(operation, null, cleanUpOperation);
        }

        public static TestObject SetupGeneralTest()
        {
            MaxTestWaitTime = TimeSpan.FromSeconds(60);
            SignalX = SignalX.Instance();
            string groupName = Guid.NewGuid().ToString();
            string groupWatcher = "groupWatcher";
            string groupWatcher2 = "groupWatcher2";
            string clientGroupReceiver = "groupClient";
            var TestHelper = new SignalXTester();
            //SET UP CLIENT
            TestObject testObject = SetUpScriptForTest(
                testData => @"
                                var promise = {}
								signalx.client." + testData.ClientHandler + @"=function (m) {
							      signalx.server." + testData.TestServerFeedbackHandler + @"(m,function(m2){
                                     signalx.server." + testData.TestServerFeedbackHandler2 + @"(m2,function(m3){
                                          promise = signalx.server." + testData.TestServerFeedbackHandler3 + @"(m3);
                                          promise.then(function(m4){
                                             signalx.server." + testData.TestServerFeedbackHandler4 + @"(m4);
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
							      server." + testData.ServerHandler + @"('" + testData.Message + @"','" + testData.ClientHandler + @"');
                                });");

            testObject.VerifiedJoinedGroup = false;
            testObject.VerifiedJoinedGroup2 = false;
            //SET UP SERVER
            SignalX.Server(
                groupWatcher,
                request =>
                {
                    testObject.VerifiedJoinedGroup = request.Message as string == groupName;
                    SignalX.RespondToAll(clientGroupReceiver, request.Message, groupName);
                });
            SignalX.Server(
                groupWatcher2,
                request => { testObject.VerifiedJoinedGroup2 = request.Message as string == groupName; });
            SignalX.Server(
                testObject.ServerHandler,
                request => { SignalX.RespondToAll(testObject.ClientHandler, testObject.Message); });
            SignalX.Server(
                testObject.TestServerFeedbackHandler,
                request =>
                {
                    testObject.FinalMessage = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.FinalMessage);
                });
            SignalX.Server(
                testObject.TestServerFeedbackHandler2,
                request =>
                {
                    testObject.FinalMessage2 = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.FinalMessage2);
                });
            SignalX.Server(
                testObject.TestServerFeedbackHandler3,
                request =>
                {
                    testObject.FinalMessage3 = request.Message as string;
                    SignalX.RespondToAll(request.ReplyTo, testObject.FinalMessage3);
                });
            SignalX.Server(
                testObject.TestServerFeedbackHandler4,
                request => { testObject.FinalMessage4 = request.Message as string; });

            return testObject;
        }

        public static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation = null, Action<Exception> onFinally = null, Action<Exception> onEveryFailure = null)
        {
            maxDuration = maxDuration ?? TimeSpan.FromSeconds(5);
            DateTime start = DateTime.Now;
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
                    onEveryFailure?.Invoke(e);
                }
            }

            cleanUpOperation?.Invoke();
            if (!done)
            {
                var e = new Exception("Unable to pass withing specified miliseconds" + maxDuration.Value.TotalMilliseconds + "  " + lastException?.Message, lastException);
                onFinally?.Invoke(e);
                throw e;
            }
            else
            {
                onFinally?.Invoke(null);
            }
        }
    }
}