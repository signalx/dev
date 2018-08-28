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
        public void all_clients_should_respond2()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    var numberOfClients = 4;
                    int counter = 0;
                    return new SignalXTestDefinition(
                        script: @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                        numberOfClients: 2,
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { counter++; });
                        },
                        checks: () =>
                        {
                            assert.AreEqual(numberOfClients, counter);
                        });
                });
        }


        [TestMethod]
        public void all_clients_should_respond()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var numberOfClients = 4;
                    int counter = 0;
                    return new SignalXTestDefinition(
                        script: @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        numberOfClients: numberOfClients,
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { counter++; });
                        },
                        checks: () => { assert.AreEqual(numberOfClients, counter); });

                });
        }



        [TestMethod]
        public void server_should_not_block_request_that_fails_authorization_when_ServerSingleAccess_is_used_when_Authentication_fails()
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
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.IsTrue(failed);
                        });
                });
        }



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
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestWaitTime);
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
                        onAppStarted: () =>
                        {
                            signalx.ServerAuthorizedSingleAccess(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestWaitTime);
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
                        onAppStarted: () =>
                        {
                            signalx.ServerAuthorized(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestWaitTime);
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
                    signalx.AuthenticationHandler(request => false);
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                        
                            assert.IsTrue(failed);
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
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestWaitTime);
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
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request => { failed = true; });
                        },
                        checks: () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestWaitTime);
                            assert.IsFalse(failed);
                        });
                });
        }
    }
}