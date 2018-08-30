namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.TestHelperLib;
    using System;

    [TestClass]
    public class when_test_library_is_used
    {
        [TestMethod]
        public void it_can_pass()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
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
                        return new SignalXTestDefinition(
                            @"",
                            onAppStarted: () =>
                            {
                            },
                            checks: () => { },
                            events: new TestEventHandler()
                            {
                                OnFinally = (e) => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = (e) => { },
                                OnClientLoaded = () => { },
                                OnClientError = (e) => { }
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
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
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
                        return new SignalXTestDefinition(
                            @"",
                            onAppStarted: () =>
                            {
                            },
                            checks: () => { },
                            events: new TestEventHandler()
                            {
                                OnFinally = (e) => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = (e) => { },
                                OnClientLoaded = () => { },
                                OnClientError = (e) => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                                return new SignalXTestDefinition(
                                    @"",
                                    onAppStarted: () =>
                                    {
                                        throw new Exception("signal x test exception thrown");
                                    },
                                    checks: () => { },
                                    events: new TestEventHandler()
                                    {
                                        OnFinally = (e) => { },
                                        OnCheckSucceeded = () => { },
                                        OnCheckFailures = (e) => { },
                                        OnClientLoaded = () => { },
                                        OnClientError = (e) => { }
                                    });
                            });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                              return new SignalXTestDefinition(
                                  @"",
                                  onAppStarted: () =>
                                  {
                                  },
                                  checks: () => { throw new Exception(); },
                                  events: new TestEventHandler()
                                  {
                                      OnFinally = (e) => { },
                                      OnCheckSucceeded = () => { },
                                      OnCheckFailures = (e) => { },
                                      OnClientLoaded = () => { },
                                      OnClientError = (e) => { }
                                  });
                          });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                   return new SignalXTestDefinition(
                       @"",
                       onAppStarted: () =>
                       {
                       },
                       checks: () => { },
                       events: new TestEventHandler()
                       {
                           OnFinally = (e) => { throw new Exception("signal x test exception thrown"); },
                           OnCheckSucceeded = () => { },
                           OnCheckFailures = (e) => { },
                           OnClientLoaded = () => { },
                           OnClientError = (e) => { }
                       });
               });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                        return new SignalXTestDefinition(
                            @"",
                            onAppStarted: () =>
                            {
                            },
                            checks: () => { },
                            events: new TestEventHandler()
                            {
                                OnFinally = (e) => { },
                                OnCheckSucceeded = () => { throw new Exception("signal x test exception thrown"); },
                                OnCheckFailures = (e) => { },
                                OnClientLoaded = () => { },
                                OnClientError = (e) => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                        var count = 0;
                        return new SignalXTestDefinition(
                            @"",
                            onAppStarted: () =>
                            {
                            },
                            checks: () =>
                            {
                                count++;
                                if (count == 1)
                                {
                                    throw new Exception("signal x test exception thrown");
                                }
                            },
                            events: new TestEventHandler()
                            {
                                OnFinally = (e) => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = (e) => { throw new Exception("signal x test exception thrown"); },
                                OnClientLoaded = () => { },
                                OnClientError = (e) => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
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
                        return new SignalXTestDefinition(
                            @"",
                            onAppStarted: () =>
                            {
                            },
                            checks: () => { },
                            events: new TestEventHandler()
                            {
                                OnFinally = (e) => { },
                                OnCheckSucceeded = () => { },
                                OnCheckFailures = (e) => { },
                                OnClientLoaded = () => { throw new Exception("signal x test exception thrown"); },
                                OnClientError = (e) => { }
                            });
                    });
                throw new AggregateException();
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    throw e;
                }
            }
        }

        [TestMethod]
        public void it_should_pass_when_onAppStarted_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                            throw new Exception("signal x test exception thrown");
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
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

                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () =>
                        {
                            throw new Exception("signal x test exception thrown");
                        },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnFinally_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { throw new Exception("signal x test exception thrown"); },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
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
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { throw new Exception("signal x test exception thrown"); },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnCheckFailures_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    var count = 0;
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () =>
                        {
                            count++;
                            if (count == 1)
                            {
                                throw new Exception("signal x test exception thrown");
                            }
                        },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { throw new Exception("signal x test exception thrown"); },
                            OnClientLoaded = () => { },
                            OnClientError = (e) => { }
                        });
                });
        }

        [TestMethod]
        public void it_should_pass_when_OnClientLoaded_fails_in_RunAndExpectFailure()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"",
                        onAppStarted: () =>
                        {
                        },
                        checks: () => { },
                        events: new TestEventHandler()
                        {
                            OnFinally = (e) => { },
                            OnCheckSucceeded = () => { },
                            OnCheckFailures = (e) => { },
                            OnClientLoaded = () => { throw new Exception("signal x test exception thrown"); },
                            OnClientError = (e) => { }
                        });
                });
        }
    }
}