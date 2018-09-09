namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_server_receives_message_from_client
    {
        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_ServerSingleAccess_is_used_when_Authentication_fails()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        () => { assert.IsTrue(failed); });
                });
        }

        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_ServerSingleAccess_is_used()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void server_should_block_request_that_fails_authorization_when_ServerAuthorizedSingleAccess_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.ServerAuthorizedSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void server_should_block_request_that_fails_authorization_when_serverAuthorized_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.ServerAuthorized(
                                "sample",
                                request => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_serverAuthorized_is_not_used2()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        () => { assert.IsTrue(failed); });
                });
        }

        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_serverAuthorized_is_not_used()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request) => false);
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void server_cannot_block_request_that_fails_when_no_authorization_is_set()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    bool failed = false;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }
    }
}