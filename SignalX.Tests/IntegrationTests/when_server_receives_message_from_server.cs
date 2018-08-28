namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_server_receives_message_from_server
    {
        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_ServerSingleAccess_is_used()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    
                    bool failed = false;
                    signalx.AuthenticationHandler(request => false);
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
                            assert.WaitFor(SignalXTester.MaxTestWaitTime);
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
                    signalx.AuthenticationHandler(request => false);
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
                            assert.WaitFor(SignalXTester.MaxTestWaitTime);
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
                    signalx.AuthenticationHandler(request => false);
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
                            assert.WaitFor(SignalXTester.MaxTestWaitTime);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_serverAuthorized_is_not_used()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                   
                    bool failed = false;
                    signalx.AuthenticationHandler(request => false);
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
                            assert.WaitFor(SignalXTester.MaxTestWaitTime);
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
                            assert.WaitFor(SignalXTester.MaxTestWaitTime);
                            assert.IsFalse(failed);
                        });
                });
        }
    }
}