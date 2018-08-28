namespace SignalXLib.TestHelperLib
{
    using com.gargoylesoftware.htmlunit;
    using Microsoft.Owin.Hosting;
    using SignalXLib.Lib;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
    using WebClient = NHtmlUnit.WebClient;

    public class SignalXTester
    {
        public static TimeSpan MaxTestWaitTime = TimeSpan.FromMinutes(1);

        public static void RunAndExpectFailure(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0)
        {
            try
            {
                RunInternal(fun,true, numberOfRetryOnTestFailure, numberOfMustRetryNoMatterWhat );
                throw new Exception("Expected run to fail , but it succeeded");
            }
            catch (Exception e)
            {
            }
        }

        public static void Run(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0)
        {
            RunInternal(fun,false,  numberOfRetryOnTestFailure ,  numberOfMustRetryNoMatterWhat );
        }
        static void RunInternal(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, bool expectFailure, int numberOfRetryOnTestFailure=0, int numberOfMustRetryNoMatterWhat=0)
        {
            for (int i = 0; i < numberOfMustRetryNoMatterWhat+1; i++)
            {
                var failures = 0;
                var succeeded = false;
                while (failures<=numberOfRetryOnTestFailure && !succeeded)
                {
                    try
                    {
                        using (var isolated = new Isolated<IsolationContainer>())
                        {
                            isolated.Value.MethodBody = fun;
                            isolated.Value.DoSomething();
                        }

                        succeeded = true;
                    }
                    catch (Exception e)
                    {
                        if (!expectFailure)
                        {
                            failures++;
                        }
                        break;
                    }
                }
            }
        }

        private static SignalX SignalX;

        // internal static string FileName;
        //internal static string FilePath;

        internal static void SetSignalXInstance(SignalX signalX)
        {
            SignalX = signalX;
        }

        internal static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        internal static string signalxScriptDownload { set; get; }
        internal static string signalRScriptDownload { set; get; }
        internal static string JQueryScriptDownload { set; get; }
        public static string CDNSignalX = "https://unpkg.com/signalx";
        public static string CDNjQuery = "https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js";
        public static string CDNSignalR = "https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js";
        
        static string DownLoadScript(string cdn)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var script = client.DownloadString(cdn);
                    if (string.IsNullOrEmpty(script))
                    {
                        throw new Exception("Empty source returned from cdn : " + CDNSignalX);
                    }
                    else
                    {
                        return script.Replace(@"\\""", @"""").Replace(@"\\'", @"'");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("It seems there is an issue connecting to the internet : " + CDNSignalX + " - " + e.Message);
            }
        }


        internal static void Run(SignalX signalX, SignalXTestDefinition scenarioDefinition)

        {
            SetSignalXInstance(signalX);
            //setup
            var testObjects = new List<TestObject>();
            // script
            var scriptTags = "";
            if (scenarioDefinition.EmbedeLibraryScripts)
            {
                JQueryScriptDownload = JQueryScriptDownload ?? DownLoadScript(CDNjQuery);
                signalRScriptDownload = signalRScriptDownload ?? DownLoadScript(CDNSignalR);
                signalxScriptDownload = signalxScriptDownload ?? DownLoadScript(CDNSignalX);

                scriptTags = @"
                            <script >" + JQueryScriptDownload + @"</script>
							<script >" + signalRScriptDownload + @"</script>
							<script >" + signalxScriptDownload + @"</script>
                            ";
            }
            else
            {
                scriptTags = @"
                            <script src='" + CDNjQuery + @"'></script>
							<script src='" + CDNSignalR + @"'></script>
							<script src='" + CDNSignalX + @"'></script>
                            ";
            }
             

            for (int i = 0; i < scenarioDefinition.NumberOfClients; i++)
            {
                string script = scenarioDefinition.Script.Count==1? 
                    scenarioDefinition.Script[0]: 
                    scenarioDefinition.Script[i];

                var testObject = new TestObject() { };
                testObject.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                testObject.FileName = "\\index" + Guid.NewGuid() + ".html";
                testObject.PageHtml = WrapScriptInHtml(script, scriptTags, "<h1>Signalx tests running ....</h1>");

                testObject = scenarioDefinition.OnClientPrepared == null ? 
                    testObject : 
                    scenarioDefinition.OnClientPrepared.Invoke(testObject);

                var FilePath = testObject.BaseDirectory + testObject.FileName;
                File.WriteAllText(FilePath, testObject.PageHtml);
                testObjects.Add(testObject);
            }

            scenarioDefinition.TestEvents = scenarioDefinition.TestEvents ?? new TestEventHandler();
            scenarioDefinition.TestEvents.OnAppStarted = () => { scenarioDefinition?.OnAppStarted?.Invoke(); };
            scenarioDefinition.TestEvents.OnFinally = (e) => { signalX.Dispose(); };
            CheckExpectations(scenarioDefinition.NumberOfClients,
                () => { scenarioDefinition?.Checks?.Invoke(); },
                "http://localhost:" + FreeTcpPort(),
                testObjects,
                scenarioDefinition.BrowserType,
                scenarioDefinition.TestEvents);
        }

       

        internal static void CheckExpectations(int numberOfClients, Action operation, string url, List<TestObject> testObjects, BrowserType browserType = BrowserType.Default, TestEventHandler events = null)
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
                            for (int i = 0; i < testObjects.Count; i++)
                            {
                                var testObject = testObjects[i];
                                if (browserType == BrowserType.Default || browserType == BrowserType.SystemBrowser)
                                {
                                    browserProcess = Process.Start(url + testObject.FileName);
                                }

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
                            }

                            AwaitAssert(
                                () =>
                                {
                                    if (!SignalX.Settings.HasOneOrMoreConnections || SignalX.CurrentNumberOfConnections < (ulong)numberOfClients)
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
                        foreach (TestObject testObject in testObjects)
                        {
                            try
                            {
                                File.Delete(testObject.BaseDirectory + testObject.FileName);
                            }
                            catch (Exception e)
                            {
                            }
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

       
        internal static string WrapScriptInHtml(string script,string concatScriptsTags, string bodyHtml)
        {

            return @"<!DOCTYPE html>
							<html>
                            <head></head>
							<body
                            "+bodyHtml+@"
							"+concatScriptsTags+@"
							<script>
                               " + script + @"
							</script>
							</body>
							</html>";
        }

        internal static void AwaitAssert(Action operation, Action cleanUpOperation)
        {
            AwaitAssert(operation, null, cleanUpOperation);
        }

        internal static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation = null, Action<Exception> onFinally = null, Action<Exception> onEveryFailure = null)
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