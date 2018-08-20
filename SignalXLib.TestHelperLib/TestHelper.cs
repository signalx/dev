namespace SignalXLib.TestHelperLib
{
    using Microsoft.Owin.Hosting;
    using NHtmlUnit;
    using SignalXLib.Lib;
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
  
    using BrowserVersion = com.gargoylesoftware.htmlunit.BrowserVersion;

    public class TestHelper
    {
     public  static TimeSpan MaxTestWaitTime = TimeSpan.FromSeconds(10);


        public static void RunScenario(ScenarioDefinition scenarioDefinition)
        {
            // script
            var script = scenarioDefinition.Script;
            //setup
            var testObject = TestHelper.SetUpScriptForTest((testData) => script);
            //server
            scenarioDefinition?.Server();
            //test : run and assert
            TestHelper.CheckExpectations(
                () =>
                {
                    scenarioDefinition?.Checks();
                },
                "http://localhost:44111",
                testObject.IndexPage);
        }


        public static void CheckExpectationsExpectingFailures(Action operation, string url, string html)
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

                     
                        // System.Diagnostics.Process.Start(url);
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
                    MaxTestWaitTime);
   }
                KillTheThread(thread);
         
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
            var testObject = new TestObject() { };
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
            var script = scriptFunc(testObject);

            testObject.IndexPage = TestHelper.WrapScriptInHtml(script);

            System.IO.File.WriteAllText(TestHelper.FilePath, testObject.IndexPage);
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
            SignalX.Server(groupWatcher2,
                request =>
                {
                    testObject.VerifiedJoinedGroup2 = request.Message as string == groupName;
                });
            SignalX.Server(testObject.ServerHandler,
                request =>
                {
                    SignalX.RespondToAll(testObject.ClientHandler, testObject.Message);
                });
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
            SignalX.Server(testObject.TestServerFeedbackHandler4,
                request =>
                {
                    testObject.FinalMessage4 = request.Message as string;
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