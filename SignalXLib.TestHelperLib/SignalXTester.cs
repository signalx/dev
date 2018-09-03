namespace SignalXLib.TestHelperLib
{
    using Microsoft.Owin.Hosting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;
    using SignalXLib.Lib;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    public class SignalXTester
    {
        public static BrowserType DefaultBrowserType = BrowserType.SystemBrowser;
        public static TimeSpan MaxTestTimeSpan = TimeSpan.FromSeconds(30);
        public static TimeSpan MaxTestWaitTimeBeforeChecks = TimeSpan.FromSeconds(3);
        public static bool EmbedeLibraryScripts = true;
        public static TimeSpan MaxWaitTimeForAllExpectedConnectionsToArrive = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Sets this only when none is specified in each test
        /// </summary>
        public static int NumberOfMustRetryNoMatterWhat = 0;

        /// <summary>
        ///     Sets this only when none is specified in each test
        /// </summary>
        public static int NumberOfRetryOnTestFailure = 0;

        public static string CDNSignalX = "https://unpkg.com/signalx";
        public static string CDNjQuery = "https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js";
        public static string CDNSignalR = "http://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.min.js";

        public static Func<ScriptLibraries, string, string> ScriptDownLoadFunction = (name, cdn) =>
        {
            try
            {
                using (var client = new WebClient())
                {
                    string script = client.DownloadString(cdn);
                    if (string.IsNullOrEmpty(script))
                        throw new Exception("Empty source returned from cdn : " + CDNSignalX);
                    else
                        return script.Replace(@"\\""", @"""").Replace(@"\\'", @"'");
                }
            }
            catch (Exception e)
            {
                if (!EmbedeLibraryScripts)
                    throw new Exception("It seems there is an issue connecting to the internet : " + CDNSignalX + " - " + e.Message);
                else
                    throw e;
            }
        };

        internal static string signalxScriptDownload { set; get; }

        internal static string signalRScriptDownload { set; get; }

        internal static string JQueryScriptDownload { set; get; }

        /// <summary>
        ///     Runs test in another app domain
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="numberOfRetryOnTestFailure"></param>
        /// <param name="numberOfMustRetryNoMatterWhat"></param>
        /// <param name="callerName"></param>
        public static void RunAndExpectFailure(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0, [CallerMemberName] string callerName = "")
        {
            RunInternal(fun, true, numberOfRetryOnTestFailure, numberOfMustRetryNoMatterWhat, callerName);
        }

        /// <summary>
        ///     Runs test in another app domain
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="numberOfRetryOnTestFailure"></param>
        /// <param name="numberOfMustRetryNoMatterWhat"></param>
        /// <param name="callerName"></param>
        public static void Run(Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun, int numberOfRetryOnTestFailure = 0, int numberOfMustRetryNoMatterWhat = 0, [CallerMemberName] string callerName = "")
        {
            RunInternal(fun, false, numberOfRetryOnTestFailure, numberOfMustRetryNoMatterWhat, callerName);
        }

        private static void RunInternal(
            Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> fun,
            bool expectFailure,
            int numberOfRetryOnTestFailure = 0,
            int numberOfMustRetryNoMatterWhat = 0,
            [CallerMemberName] string callerName = "")
        {
            numberOfRetryOnTestFailure = numberOfRetryOnTestFailure == 0 ? NumberOfRetryOnTestFailure : numberOfRetryOnTestFailure;
            numberOfMustRetryNoMatterWhat = numberOfMustRetryNoMatterWhat == 0 ? NumberOfMustRetryNoMatterWhat : numberOfMustRetryNoMatterWhat;

            Exception ex = null;
            for (int i = 0; i <= numberOfMustRetryNoMatterWhat; i++)
            {
                int failures = 0;
                bool succeeded = false;
                while (failures <= numberOfRetryOnTestFailure)
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

                if (succeeded)
                {
                }
                else
                {
                    throw expectFailure ? new Exception("Expected tests to fail but it did not") : ex ?? new Exception("Expected tests to pass but it did not");
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

        internal static void Run(ExceptionTracker exceptionTracker, SignalX signalX, SignalXTestDefinition scenarioDefinition)

        {
            // SetSignalXInstance(signalX);
            //setup
            var testObjects = new List<TestObject>();
            // script
            string scriptTags = "";
            if (EmbedeLibraryScripts)
            {
                JQueryScriptDownload = JQueryScriptDownload ?? ScriptDownLoadFunction(ScriptLibraries.JQUERY, CDNjQuery);
                signalRScriptDownload = signalRScriptDownload ?? ScriptDownLoadFunction(ScriptLibraries.SIGNALR, CDNSignalR);
                signalxScriptDownload = signalxScriptDownload ?? ScriptDownLoadFunction(ScriptLibraries.SIGNALX, CDNSignalX);

                scriptTags = @"
                            <script>" + JQueryScriptDownload + @"</script>
							<script>" + signalRScriptDownload + @"</script>
							<script>" + signalxScriptDownload + @"</script>
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

            scriptTags += "<script>" + signalX.ClientErrorSnippet + @"</script>";

            //todo this ovverides settings. so use jquery events on clients
            scriptTags += @"<script>signalx.debug(function(ev,e){ " + signalX.Settings.ClientLogScriptFunction + @"('<div  style=\' color:green\'>'+JSON.stringify(e)+'</div>'); });</script>";
            scriptTags += @"<script>signalx.error(function(ev,e){ " + signalX.Settings.ClientLogScriptFunction + @"('<div style=\' color:red\'>'+JSON.stringify(e)+'</div>'); });</script>";

            for (int i = 0; i < scenarioDefinition.NumberOfClients; i++)
            {
                string script = scenarioDefinition.Script.Count == 1 ? scenarioDefinition.Script[0] : scenarioDefinition.Script[i];

                var testObject = new TestObject();
                testObject.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                testObject.FileName = "\\index" + Guid.NewGuid() + ".html";
                testObject.PageHtml = WrapScriptInHtml(scriptTags, script, "<h1>Signalx tests running ....</h1>");

                testObject = scenarioDefinition.OnClientPrepared == null ? testObject : scenarioDefinition.OnClientPrepared.Invoke(testObject);

                string FilePath = testObject.BaseDirectory + testObject.FileName;
                File.WriteAllText(FilePath, testObject.PageHtml);
                testObjects.Add(testObject);
            }

            scenarioDefinition.TestEvents = scenarioDefinition.TestEvents ?? new TestEventHandler();
            scenarioDefinition.TestEvents.OnAppStarted = () => { scenarioDefinition?.OnAppStarted?.Invoke(); };

            CheckExpectations(
                exceptionTracker,
                signalX,
                scenarioDefinition.NumberOfClients,
                () => { scenarioDefinition?.Checks?.Invoke(); },
                "http://localhost:" + FreeTcpPort(),
                testObjects,
                scenarioDefinition.BrowserType,
                scenarioDefinition.TestEvents);
        }

        internal static void CheckExpectations(ExceptionTracker exceptionTracker, SignalX signalX, int numberOfClients, Action operation, string url, List<TestObject> testObjects, BrowserType browserType = BrowserType.Default, TestEventHandler events = null)
        {
            Thread thread = null;
            var browserProcess = new List<Process>();
            /*

             */
            //https://stackoverflow.com/questions/22538457/put-a-string-with-html-javascript-into-selenium-webdriver
            //https://www.automatetheplanet.com/selenium-webdriver-csharp-cheat-sheet/
            var option = new FirefoxOptions();
            option.AddArgument("--headless");
            var _drivers = new List<IWebDriver>();
            if (browserType == BrowserType.HeadlessBrowser)
                for (int i = 0; i < testObjects.Count; i++)
                    _drivers.Add(
                        new FirefoxDriver(
                            option
                        // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        ));

            try
            {
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
                                    TestObject testObject = testObjects[i];
                                    if (browserType == BrowserType.SystemBrowser)
                                        browserProcess.Add(Process.Start(url + testObject.FileName));

                                    if (browserType == BrowserType.Default || browserType == BrowserType.HeadlessBrowser)
                                    {
                                        _drivers[i].Navigate().GoToUrl(url);
                                        ((IJavaScriptExecutor)_drivers[i]).ExecuteScript($"arguments[0].innerHTML = '{testObject.PageHtml}'");
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
                        MaxWaitTimeForAllExpectedConnectionsToArrive);

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
                        ex =>
                        {
                            //to handle both exceptions
                            if (exceptionTracker.Exception != null)
                                events?.OnFinally?.Invoke(exceptionTracker.Exception);
                            else
                                events?.OnFinally?.Invoke(ex);
                        },
                        events?.OnCheckFailures);
                }
            }
            finally
            {
                QuitAllBrowsers(_drivers, browserProcess, thread, testObjects, browserType);
                signalX.Dispose();
            }
        }

        private static void QuitAllBrowsers(
            List<IWebDriver> _drivers,
            List<Process> browserProcess,
            Thread thread,
            List<TestObject> testObjects,
            BrowserType browserType)
        {
            foreach (IWebDriver _driver in _drivers)
                try
                {
                    _driver.Quit();
                }
                catch (Exception e)
                {
                }

            if (browserType == BrowserType.SystemBrowser)
                foreach (TestObject testObject in testObjects)
                {
                    try
                    {
                        string tabContent = testObject.FileName
                            .Replace("\\\\", "").Replace("//", "")
                            .Replace("\\", "").Replace("/", "");

                        CloseAChromeTab(tabContent);
                    }
                    catch (Exception e)
                    {
                    }

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
        }

        private static void CloseAChromeTab(string check)
        {
            /* Process[] procsChrome = Process.GetProcessesByName("chrome");

             if (procsChrome.Length > 0)
             {
                 foreach (Process proc in procsChrome)
                 {
                     // the chrome process must have a window
                     if (proc.MainWindowHandle == IntPtr.Zero)
                         continue;

                     // to find the tabs we first need to locate something reliable - the 'New Tab' button
                     AutomationElement root = AutomationElement.FromHandle(proc.MainWindowHandle);
                     var SearchBar = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
                     if (SearchBar != null)
                     {
                         bool valuePatternExist = (bool)SearchBar.GetCurrentPropertyValue(AutomationElement.IsValuePatternAvailableProperty);
                         if (valuePatternExist)
                         {
                             ValuePattern val = SearchBar.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                             if (val.Current.Value.ToLower().Contains(check) || val.Current.Value.Contains(check))
                             {
                                 proc.Close();
                                 proc.Kill();
                             }
                         }
                     }
                 }
             }*/
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

        internal static string WrapScriptInHtml(string concatScriptsTagsHtml, string scriptPure, string bodyHtml)
        {
            return @"<!DOCTYPE html>
							<html>
                            <head></head>
							<body>
							" + concatScriptsTagsHtml + @"
							<script>
                               " + scriptPure + @"
							</script>
                              " + bodyHtml + @"
							</body>
							</html>";
        }

        internal static void AwaitAssert(Action operation, Action breakOnExcetipon, TimeSpan maxDuration, Action cleanUpOperation = null, Action<Exception> onFinally = null, Action<Exception> onEveryFailure = null)
        {
            DateTime start = DateTime.Now;
            bool done = false;
            Exception lastException = null;

            while ((DateTime.Now - start).TotalMilliseconds < maxDuration.TotalMilliseconds && !done)
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
                var e = new Exception("Unable to pass within specified miliseconds - " + maxDuration.TotalMilliseconds + "  " + lastException?.Message, lastException);
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