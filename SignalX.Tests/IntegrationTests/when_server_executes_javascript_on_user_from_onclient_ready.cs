namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using System.Linq;

    [TestClass]
    public class when_server_executes_javascript_on_user_from_onclient_ready
    {
        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;

                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.OnClientReady(
                                () =>
                                {
                                    signalX.RunJavaScriptOnUser(
                                        signalX.Connections.GetAll().First(),
                                        @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                        response => { result = Convert.ToInt32(response); }
                                        );
                                });
                        },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.OnClientReady(
                                () =>
                                {
                                    signalX.RunJavaScriptOnUser(
                                        signalX.Connections.GetAll().First(),
                                        $"return 5*10",
                                        response => { result = Convert.ToInt32(response); }
                                        );
                                });
                        },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                            }));
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_on_client2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.OnClientReady(
                                () =>
                                {
                                    signalX.RunJavaScriptOnUser(
                                        signalX.Connections.GetAll().First(),
                                        @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                        (response, request, error) => { result = Convert.ToInt32(response); }
                                        );
                                });
                        },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.OnClientReady(
                                () =>
                                {
                                    signalX.RunJavaScriptOnUser(
                                        signalX.Connections.GetAll().First(),
                                        $"return 5*10",
                                        (response, request, error) => { result = Convert.ToInt32(response); }
                                        );
                                });
                        },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                            }));
                });
        }

        [TestMethod]
        public void server_should_not_be_able_to_run_complex_javascript_client_without_onclientready()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;

                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.RunJavaScriptOnUser(
                                signalX.Connections.GetAll().First(),
                                @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                response => { result = Convert.ToInt32(response); }
                            );
                        },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_not_be_able_to_run_simple_javascript_on_client_without_onclientready()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.RunJavaScriptOnUser(
                                signalX.Connections.GetAll().First(),
                                $"return 5*10",
                                response => { result = Convert.ToInt32(response); }
                            );
                        },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                            }));
                });
        }

        [TestMethod]
        public void server_should_not_be_able_to_run_complex_javascript_on_client2_without_onclientready()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.RunJavaScriptOnUser(
                                signalX.Connections.GetAll().First(),
                                @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                (response, request, error) => { result = Convert.ToInt32(response); }
                            );
                        },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_not_be_able_to_run_simple_javascript_on_client2_without_onclientready()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () =>
                        {
                            signalX.Settings.ManageUserConnections = true;
                            signalX.RunJavaScriptOnUser(
                                signalX.Connections.GetAll().First(),
                                $"return 5*10",
                                (response, request, error) => { result = Convert.ToInt32(response); }
                            );
                        },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                            }));
                });
        }
    }
}