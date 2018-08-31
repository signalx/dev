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
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;
    using WebClient = NHtmlUnit.WebClient;

    public class SignalXTester
    {
        public static TimeSpan MaxTestTimeSpan = TimeSpan.FromMinutes(1);
        public static TimeSpan MaxTestWaitTimeBeforeChecks = TimeSpan.FromSeconds(5);

        public static void RunAndExpectFailure(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0, [CallerMemberName] string callerName = "")
        {
            RunInternal(fun, true, numberOfRetryOnTestFailure, numberOfMustRetryNoMatterWhat, callerName);

        }

        public static void Run(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0, [CallerMemberName] string callerName = "")
        {
            RunInternal(fun, false, numberOfRetryOnTestFailure, numberOfMustRetryNoMatterWhat, callerName);
        }
        static void RunInternal(
            Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun,
            bool expectFailure,
            int numberOfRetryOnTestFailure = 0,
            int numberOfMustRetryNoMatterWhat = 0, [CallerMemberName] string callerName = "")
        {
            Exception ex = null;
            for (int i = 0; i <= numberOfMustRetryNoMatterWhat; i++)
            {
                var failures = 0;
                var succeeded = false;
                while (failures <= numberOfRetryOnTestFailure)
                {
                    try
                    {
                        using (var isolated = new Isolated<IsolationContainer>())
                        {
                            isolated.Value.MethodBody = fun;
                            isolated.Value.TestName = callerName;
                            isolated.Value.DoSomething();
                        }

                        if (expectFailure)
                        {
                            failures++;
                        }
                        else
                        {
                            succeeded = true;
                            break;

                        }
                    }
                    catch (Exception e)
                    {
                        ex = e;
                        if (!expectFailure)
                        {
                            failures++;
                        }
                        else
                        {
                            succeeded = true;
                            break;
                        }

                    }
                }

                if (succeeded)
                {

                }
                else
                {
                    throw expectFailure ?
                        new Exception("Expected tests to fail but it did not") :
                        ex ?? new Exception("Expected tests to pass but it did not");
                }
            }
        }

        //  private static SignalX SignalX;

        // internal static string FileName;
        //internal static string FilePath;



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


        internal static void Run(ExceptionTracker exceptionTracker, SignalX signalX, SignalXTestDefinition scenarioDefinition)

        {
            // SetSignalXInstance(signalX);
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

            scriptTags += "<script>" + signalX.ClientErrorSnippet + @"</script><script>" + signalX.ClientDebugSnippet + @"</script>"
                ;

            for (int i = 0; i < scenarioDefinition.NumberOfClients; i++)
            {
                string script = scenarioDefinition.Script.Count == 1 ?
                    scenarioDefinition.Script[0] :
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

            CheckExpectations(exceptionTracker, signalX, scenarioDefinition.NumberOfClients,
                () =>
                {
                    scenarioDefinition?.Checks?.Invoke();
                },
                "http://localhost:" + FreeTcpPort(),
                testObjects,
                scenarioDefinition.BrowserType,
                scenarioDefinition.TestEvents);
        }
        internal static void CheckExpectations(ExceptionTracker exceptionTracker, SignalX signalX, int numberOfClients, Action operation, string url, List<TestObject> testObjects, BrowserType browserType = BrowserType.Default, TestEventHandler events = null)
        {
            Thread thread;
            Process browserProcess = null;

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
                        }
                        catch (Exception e)
                        {
                            events?.OnClientError?.Invoke(new AggregateException(e, exceptionTracker.Exception));
                        }

                    });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();


                // SpinWait.SpinUntil(() => (!SignalX.Settings.HasOneOrMoreConnections || SignalX.CurrentNumberOfConnections < (ulong)numberOfClients), MaxTestWaitTimeBeforeChecks);

                AwaitAssert(
                () =>
                {

                    if (!signalX.Settings.HasOneOrMoreConnections)
                        throw new Exception("No connection received from any client.This may also be caused by a slow connection " + exceptionTracker?.Exception?.Message);

                    if (signalX.ConnectionCount < (ulong)numberOfClients)
                        throw new Exception($"Wait timeout for expected number of clients {numberOfClients} to show up.This may also be caused by a slow connection " + exceptionTracker?.Exception?.Message);

                },
                () =>
                {
                    if (exceptionTracker.Exception != null) throw exceptionTracker.Exception;
                },
                TimeSpan.FromSeconds(15));

                new SignalXAssertionLib().WaitForSomeTime(MaxTestWaitTimeBeforeChecks);


                events?.OnClientLoaded?.Invoke();
                //wait for some time before checks to allow side effects to occur
                AwaitAssert(
                    () =>
                    {
                        operation();
                        //if we got this far, then we made it
                        events?.OnCheckSucceeded?.Invoke();
                    },
                    () =>
                    {
                        if (exceptionTracker.Exception != null) throw exceptionTracker.Exception;
                    },
                    MaxTestTimeSpan,
                    null,
                    onFinally: ex =>
                    {

                        try
                        {
                            //to handle both exceptions
                            if (exceptionTracker.Exception != null)
                            {
                                events?.OnFinally?.Invoke(exceptionTracker.Exception);

                            }
                            else
                            {
                                events?.OnFinally?.Invoke(ex);
                            }
                            signalX.Dispose();
                        }
                        catch (Exception e)
                        {
                            signalX.Dispose();
                            throw;
                        }
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
                    }, onEveryFailure: events?.OnCheckFailures);
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


        internal static string WrapScriptInHtml(string script, string concatScriptsTags, string bodyHtml)
        {

            return @"<!DOCTYPE html>
							<html>
                            <head></head>
							<body
                            " + bodyHtml + @"
							" + concatScriptsTags + @"
							<script>
                              
                               " + script + @"
                             
							</script>
							</body>
							</html>";
        }

        internal static void AwaitAssert(Action operation, Action breakOnExcetipon, Action cleanUpOperation)
        {
            AwaitAssert(operation, breakOnExcetipon, null, cleanUpOperation);
        }



        internal static void AwaitAssert(Action operation, Action breakOnExcetipon, TimeSpan? maxDuration = null, Action cleanUpOperation = null, Action<Exception> onFinally = null, Action<Exception> onEveryFailure = null)
        {
            maxDuration = maxDuration ?? TimeSpan.FromSeconds(5);
            DateTime start = DateTime.Now;
            var done = false;
            Exception lastException = null;

            while ((DateTime.Now - start).TotalMilliseconds < maxDuration.Value.TotalMilliseconds && !done)
            {

                try
                {
                    try
                    {
                        breakOnExcetipon?.Invoke();
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        break;
                    }
                    operation();
                    done = true;
                }
                catch (Exception e)
                {
                    lastException = e;
                    onEveryFailure?.Invoke(e);
                }
                // Thread.Sleep(0);
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
            }

            cleanUpOperation?.Invoke();
            if (!done)
            {
                var e = new Exception("Unable to pass within specified miliseconds - " + maxDuration.Value.TotalMilliseconds + "  " + lastException?.Message, lastException);
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