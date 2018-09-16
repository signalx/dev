namespace SignalXLib.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_server_receives_message_from_multiple_clients
    {
        static readonly int numberOfRety = 0;

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups5()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    SignalXTester.MaxTestWaitTimeBeforeChecks = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
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
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) =>
                                {
                                    request.RespondToAllInGroup("handler", "done", "groupA");
                                    assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                                });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreNotEqual(0, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups4a()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
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
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) =>
                                {
                                    request.RespondToAllInGroup("handler", "done", "groupA");
                                    assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                                });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(1, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups4()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
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
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) =>
                                {
                                    request.RespondToAllInGroup("handler", "done", "groupA");
                                    assert.WaitForSomeTime(TimeSpan.FromSeconds(10));
                                });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(0, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups2()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    //SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
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
                              signalx.groups.join('groupA',function(){ signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                   });
                                    
                                   });"
                        },
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) => { request.RespondToAllInGroup("handler", "done", "groupA"); });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(2, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups3()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
                        {
                            @"
                             signalx.ready(function (server) {
                              signalx.groups.join('groupB',function(){

                                    signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                    });
                                   });",
                            @"
                             signalx.ready(function (server) {
                              signalx.groups.join('groupA',function(){
                                        signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                    });

                                   });"
                        },
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) => { request.RespondToAllInGroup("handler", "done", "groupA"); });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(4, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_not_be_able_to_send_message_to_clients_in_different_groups()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    //SignalXTester.MaxTestTimeSpan = TimeSpan.FromSeconds(30);
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
                        {
                            @"
                             signalx.ready(function (server) {
                                 signalx.groups.join('groupB',function(){

                                    signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                      });
                                   });",
                            @"
                             signalx.ready(function (server) {
                               signalx.groups.join('groupA',function(){

                                    signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                    });
                                   });"
                        },
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) => { request.RespondToAllInGroup("handler", "done", "groupA"); });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(2, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void server_should_be_able_to_send_message_to_clients_in_same_group()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    SignalXTester.MaxTestTimeSpan = TimeSpan.FromMinutes(1);
                    return new SignalXTestDefinition(
                        new List<string>
                        {
                            @"
                             signalx.ready(function (server) {
                               signalx.groups.join('groupA',function(){

                                    signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                    });
                                   });",
                            @"
                             signalx.ready(function (server) {
                               signalx.groups.join('groupA',function(){

                                    signalx.client.handler=function(){
                                        signalx.server.final();
                                      }
                                     signalx.server.sample(100,'handler');
                                    });
                                   });"
                        },
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                (request) =>
                                {
                                    request.RespondToAllInGroup("handler", "done", "groupA");
                                });
                            signalx.Server(
                                "final",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(4, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int numberOfClients = 4;
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                        2,
                        () =>
                        {
                            signalx.Server(ServerType.SingleAccess,
                                "sample",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(numberOfClients, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond2()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int numberOfClients = 4;
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   }); ",
                        numberOfClients,
                        () =>
                        {
                            signalx.Server(ServerType.SingleAccess,
                                "sample",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(numberOfClients, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond3()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
                        {
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });"
                        },
                        () =>
                        {
                            signalx.Server(ServerType.SingleAccess,
                                "sample",
                                (request) => { counter++; });
                        },
                        () => { assert.AreNotEqual(2, counter); });
                },
                numberOfRety);
        }

        [TestMethod]
        public void all_clients_should_respond4()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int counter = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        new List<string>
                        {
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });",
                            @"signalx.ready(function (server) {
                                     signalx.server.sample(100);
                                   });"
                        },
                        () =>
                        {
                            signalx.Server(ServerType.SingleAccess,
                                "sample",
                                (request) => { counter++; });
                        },
                        () => { assert.AreEqual(2, counter); });
                },
                numberOfRety);
        }
    }
}