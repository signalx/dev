namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;

    [TestClass]
    public class when_server_executes_javascript_on_user
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
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                    response => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(10));
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
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    "return 5*10",
                                    response => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(15));
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
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    @"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                    (response, request, error) => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(10));
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
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    "return 5*10",
                                    (response, request, error) => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(15));
                            }));
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_client_having_wrong_script()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    @"
                                    var grades = [1,2,3,4,5,6];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                    response => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(10));
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client_having_wrong_script()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    "return 5*10*20",
                                    response => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(15));
                            }));
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_on_client_having_wrong_script2()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    @"
                                    var grades = [1,2,3,4,5,6];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                                    (response, request, error) => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(10));
                            })
                    );
                },
                1);
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client_having_wrong_script2()
        {
            SignalXTester.RunAndExpectFailure(
                (signalX, assert) =>
                {
                    int result = 0;

                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        "",
                        () => { signalX.Settings.ManageUserConnections = true; },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnUser(
                                    signalX.Connections.FirstOrDefault(),
                                    "return 5*10*20",
                                    (response, request, error) => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(15));
                            }));
                });
        }
    }
}