namespace SignalXLib.TestHelperLib
{
    using Microsoft.Owin.Hosting;
    using SignalXLib.Lib;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
    using sun.awt;
    using BrowserVersion = com.gargoylesoftware.htmlunit.BrowserVersion;
    using WebClient = NHtmlUnit.WebClient;

    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;
        private T _value;

        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
                null, AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public void Dispose()
        {
            if (_domain != null)
            {
                try
                {
                    try
                    {
                        AppDomain.Unload(_domain);
                    }
                    catch (CannotUnloadAppDomainException)
                    {
                        GC.Collect();
                        AppDomain.Unload(_domain);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                _domain = null;
            }
        }
    }
    public class TestHelper
    {
        public static void ExpectNotEqual<T>(T item1, T item2, string message = "")
        {
            if (item1.Equals(item2))
            {
                throw new Exception($"Expected {item1} to be not equal to {item2} " + message);
            }
        }
        public static void ExpectEqual<T>(T item1, T item2, string message = "")
        {
            if (!item1.Equals(item2))
            {
                throw new Exception($"Expected {item1} to be equal to {item2} " + message);
            }
        }
        public static void ExpectTrue(bool expectation,string message="")
        {
            if (!expectation)
            {
                throw  new Exception($"Expected result to be true "+ message);
            }
        }
        public static void ExpectFalse(bool expectation, string message = "")
        {
            if (expectation)
            {
                throw new Exception($"Expected result to be not true " + message);
            }
        }


        public static void SetSignalXInstance(SignalX signalX)
        {
            SignalX = signalX;
        }
      static  SignalX SignalX=null;
     public  static TimeSpan MaxTestWaitTime = TimeSpan.FromSeconds(10);

       public static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }


        //public static void RunScenario(Func<SignalX, ScenarioDefinition> fun)
        //{
        //    Exception eex = null;
        //    try
        //    {
        //        RunScenarioInternal((sx) =>
        //        {
        //            try
        //            {
        //                return fun(sx);
        //            }
        //            catch (Exception e)
        //            {
        //                eex = e;
        //                throw e;
        //            }
        //        });

        //    }
        //    catch (Exception e)
        //    {
        //        if (eex != null)
        //        {
        //            throw eex;
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }

        //}

        public static void RunScenario<T>(Func<SignalX, SignalXTestDefinition<T>> fun)
        {
            using (Isolated<Facade<T>> isolated = new Isolated<Facade<T>>())
            {
                isolated.Value.b = fun;
                isolated.Value.DoSomething();
            }
        }
        public static void RunScenario(Func<SignalX,SignalXTestDefinition> fun )
        {
            using (Isolated<Facade<object>> isolated = new Isolated<Facade<object>>())
            {
                isolated.Value.a = fun;
                isolated.Value.DoSomething();
            }
        }

        internal static void RunScenario<T>(SignalX signalX, SignalXTestDefinition<T> scenarioDefinition)

        {
            TestHelper testHelper = new TestHelper();
            TestHelper.SetSignalXInstance(signalX);
            // script
            var script = scenarioDefinition.Script;
            //setup
            var testObject = TestHelper.SetUpScriptForTest((testData) => script);
            //server
            //test : run and assert
            TestHelper.CheckExpectations(
                operation: () =>
                {
                    scenarioDefinition?.Checks(scenarioDefinition.Expectation);
                },
                url: "http://localhost:" + FreeTcpPort(),
                html: testObject, onClientLoaded: scenarioDefinition.OnClientLoaded, onClientError: scenarioDefinition.OnClientError, browserType: scenarioDefinition.BrowserType, onCheckSucceeded: scenarioDefinition.OnCheckSucceeded, onCheckFailures: scenarioDefinition.OnCheckFailures,
                onFinally: (e) =>
                {
                    signalX.Dispose();
                },
                onAppStart: () =>
                {
                    scenarioDefinition?.Server();
                });

        }

        internal static void RunScenario(SignalX signalX, SignalXTestDefinition scenarioDefinition)
        {
            RunScenario<object>(signalX,scenarioDefinition);
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

        public static void CheckExpectations(Action operation, string url, TestObject html,Action onClientLoaded=null, Action<Exception> onClientError=null, BrowserType browserType = BrowserType.Unknown, Action onCheckSucceeded = null, Action<Exception> onCheckFailures = null, Action<Exception> onFinally = null, Action onAppStart=null)
        {
           
            Thread thread;
            Process browserProcess=null;
            SignalX.Settings.LogAgentMessagesOnClient = true;
            SignalX.Settings.ManageUserConnections = true;
            using (WebApp.Start<Startup>(url))
            {
                onAppStart?.Invoke();
               
                thread = new Thread(() =>
                {
                    try
                    {
                        if (browserType== BrowserType.DefaultSystemBrowser || browserType == BrowserType.Unknown)
                        {
                            browserProcess =System.Diagnostics.Process.Start(url+html.FileName);
                        }
                        if (browserType == BrowserType.EmbededBrowser )
                        {
                            WebClient webClient = new WebClient(new NHtmlUnit.BrowserVersion(BrowserVersion.CHROME))
                            {
                                JavaScriptEnabled = true,
                                ThrowExceptionOnFailingStatusCode = true,
                                ThrowExceptionOnScriptError = true
                            };
                            webClient.GetPage(url + html.FileName.Replace("\\","/"));
                            
                        }

                        TestHelper.AwaitAssert(
                            () =>
                            {
                                if (!SignalX.Settings.HasOneOrMoreConnections)
                                {
                                    throw new Exception("No connection received from any client");
                                }
                            },
                            TimeSpan.FromSeconds(10));


                        onClientLoaded?.Invoke();


                        
                    }
                    catch (Exception e)
                    {
                        onClientError?.Invoke(e);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                TestHelper.AwaitAssert(
                    () => operation(),
                    MaxTestWaitTime,null,
                    (ex) =>
                    {
                        onFinally?.Invoke(ex);
                        try
                        {
                            browserProcess?.Kill();
                        }
                        catch (Exception e)
                        {
                            
                        }

                        try
                        {
                            System.IO.File.Delete(FilePath);
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
                    });
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

            testObject.PageHtml = TestHelper.WrapScriptInHtml(script);
            FileName = "\\index" + Guid.NewGuid() + ".html";
            FilePath = AppDomain.CurrentDomain.BaseDirectory + FileName;
            System.IO.File.WriteAllText(TestHelper.FilePath, testObject.PageHtml);
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

        public static string FileName ;
        public static string FilePath ;

        public static void AwaitAssert(Action operation, Action cleanUpOperation)
        {
            AwaitAssert(operation, null, cleanUpOperation);
        }

        public static TestObject SetupGeneralTest()
        {
            TestHelperLib.TestHelper.MaxTestWaitTime= TimeSpan.FromSeconds(60);
            SignalX = SignalX.Instance();
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

        public static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation = null, Action<Exception> onFinally=null)
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

    //https://bitlush.com/blog/executing-code-in-a-separate-application-domain-using-c-sharp
    public class Facade<T> : MarshalByRefObject
    {
        public Func<SignalX,SignalXTestDefinition> a { set; get; }
        public Func<SignalX,SignalXTestDefinition<T>> b { set; get; }
        public void DoSomething()
        {
            var sx = SignalX.Instance();
            if (a != null)
            {
                var defa = a?.Invoke(sx);
                TestHelper.RunScenario(sx, defa);
            }
            if (b != null)
            {
                var defb = b?.Invoke(sx);
                TestHelper.RunScenario<T>(sx, defb);
            }
        }
    }
}