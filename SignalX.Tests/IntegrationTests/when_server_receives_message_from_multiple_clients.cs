namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System.Collections.Generic;
    using System.Diagnostics;

    [TestClass]
    public class when_server_receives_message_from_multiple_clients
    {
       static int numberOfRety = 1;

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups5()
        {
            SignalXTester.RunAndExpectFailure((signalx, assert) =>
            {
                SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                SignalXTester.MaxTestWaitTimeBeforeChecks = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                                assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () => { assert.AreNotEqual(0, counter); });
            }, numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups4a()
        {
            SignalXTester.RunAndExpectFailure((signalx, assert) =>
            {
                SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                                assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () =>
                    {
                        assert.AreEqual(1, counter);
                       
                    });
            }, numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups4()
        {
            SignalXTester.Run((signalx, assert) =>
            {
                SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                                assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () => { assert.AreEqual(0, counter); });
            }, numberOfRety);
        }
        
        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups2()
        {
            SignalXTester.Run((signalx, assert) =>
            {
                //SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                              signalx.groups.join('groupA');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () => { assert.AreEqual(2, counter); });
            }, numberOfRety);
        }
        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups3()
        {
            SignalXTester.RunAndExpectFailure((signalx, assert) =>
            {
                SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                              signalx.groups.join('groupB');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                              signalx.groups.join('groupA');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () => { assert.AreEqual(4, counter); });
            }, numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups()
        {
            SignalXTester.Run((signalx, assert) =>
            {
                //SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                int counter = 0;
                return new SignalXTestDefinition(
                    script: new List<string>()
                    {
                        @"
                             signalx.ready(function (server) {
                                 signalx.groups.join('groupB');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                        @"
                             signalx.ready(function (server) {
                               signalx.groups.join('groupA');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                    },
                    onAppStarted: () =>
                    {
                        signalx.Server("sample",
                            request =>
                            {
                                request.RespondToAllInGroup("handler", "done", "groupA");
                            });
                        signalx.Server("final",
                            request =>
                            {
                                counter++;
                            });
                    },
                    checks: () => { assert.AreEqual(2, counter); });
            }, numberOfRety);
        }

        [TestMethod]
        public void server_should_be_able_to_send_message_to_clients_in_same_group()
        {
            SignalXTester.Run((signalx, assert) =>
            {
                signalx.SetUpClientErrorMessageHandler((error, request) =>
                {

                });
                signalx.SetUpClientDebugMessageHandler((error, request) =>
                {

                });
                int counter = 0;
                    return new SignalXTestDefinition(
                        script: new List<string>()
                        {
                            @"
                             signalx.ready(function (server) {
                                signalx.groups.join('groupA');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });",
                            @"
                             signalx.ready(function (server) {
                               signalx.groups.join('groupA');
                                    signalx.client.handler=function(){
                                        signalx.server.final(); 
                                      }
                                     signalx.server.sample(100,'handler');
                                   });"
                        },
                        onAppStarted: () =>
                        {
                            signalx.Server("sample",
                                request =>
                                {
                                    request.RespondToAllInGroup("handler", "done","groupA");
                                });
                            signalx.Server("final",
                                request =>
                                {
                                    counter++;
                                });
                        },
                        checks: () =>
                        {
                            assert.AreEqual(4, counter);
                        });
                }, numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond()
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
                        checks: () => { assert.AreEqual(numberOfClients, counter); });
                }, numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond2()
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
                        checks: () =>
                        {
                            assert.AreEqual(numberOfClients, counter);
                        })
                    {
                        
                    };
                }, numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond3()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int counter = 0;
                    return new SignalXTestDefinition(
                        script: new List<string>()
                        {
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });"
                        },
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { counter++; });
                        },
                        checks: () => { assert.AreNotEqual(2, counter); });
                }, numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond4()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int counter = 0;
                    return new SignalXTestDefinition(
                        script: new List<string>()
                        {
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });"
                        },
                        onAppStarted: () =>
                        {
                            signalx.ServerSingleAccess(
                                "sample",
                                request => { counter++; });
                        },
                        checks: () => { assert.AreEqual(2, counter); });
                }, numberOfRety);
        }
    }
}