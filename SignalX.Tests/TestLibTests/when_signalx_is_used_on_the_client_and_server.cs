namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class WHEN_TEST_LIBRARY_TEST_is_used_to_test
    {
        [TestMethod]
        public void it_can_pass()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_can_fail_with_RunAndExpectFailure_having_no_exceptions()
        {
            try
            {
                SignalXTester.RunAndExpectFailure(
                    (signalx, assert) =>
                    {
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;

                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                Assert.Fail();
            }
            catch (Exception e)
            {
            }
        }

        [TestMethod]
        public void it_can_pass_with_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    throw new Exception("signal x test exception thrown");
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_can_fail_with_error_in_Run()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        throw new Exception("signal x test exception thrown");
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_onAppStarted_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { throw new Exception("signal x test exception thrown"); },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_checks_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(20);
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { throw new Exception(); },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_OnFinally_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { throw new Exception("signal x test exception thrown"); },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_OnCheckSucceeded_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(20);
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { throw new Exception("signal x test exception thrown"); },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_OnCheckFailures_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {

                        int count = 0;
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () =>
                            {
                                count++;
                                if (count == 1)
                                    throw new Exception("signal x test exception thrown");
                            },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { throw new Exception("signal x test exception thrown"); },
                                OnClientLoaded = () => { },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_fail_when_OnClientLoaded_fails()
        {
            try
            {
                SignalXTester.Run(
                    (signalx, assert) =>
                    {
                        SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                        SignalXTester.EmbedeLibraryScripts = true;
                        return new SignalXTestDefinition(
                            @"",
                            () => { },
                            () => { },
                            new TestEventHandler
                            {
                                OnFinally = e => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = e => { },
                                OnClientLoaded = () => { throw new Exception("signal x test exception thrown"); },
                                OnClientError = e => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                    throw e;
            }
        }

        [TestMethod]
        public void it_should_pass_when_onAppStarted_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { throw new Exception("signal x test exception thrown"); },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_checks_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(20);

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { throw new Exception("signal x test exception thrown"); },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnFinally_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { throw new Exception("signal x test exception thrown"); },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnCheckSucceeded_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(20);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { throw new Exception("signal x test exception thrown"); },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnCheckFailures_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int count = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () =>
                        {
                            count++;
                            if (count == 1)
                                throw new Exception("signal x test exception thrown");
                        },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { throw new Exception("signal x test exception thrown"); },
                            OnClientLoaded = () => { },
                            OnClientError = e => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnClientLoaded_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { },
                        new TestEventHandler
                        {
                            OnFinally = e => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = e => { },
                            OnClientLoaded = () => { throw new Exception("signal x test exception thrown"); },
                            OnClientError = e => { }
                        });
                });
        }
    }
}