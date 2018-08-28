namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_signalx_is_used_on_the_client_and_server
    {
        [TestMethod]
        public void client_can_receive_data_from_handler()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample('GetSomething','handler');
                                     signalx.client.handler(function (something) {
                                            signalx.server.sample2(something*7);
                                       });
                                   }); ",
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(700, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void client_can_receive_data_from_callback()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample('GetSomething',function (something) {
                                            signalx.server.sample2(something*7);
                                       });
                                   }); ",
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(700, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void client_can_receive_data_from_promise()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     var getSomethingCompletedPromise = signalx.server.sample('GetSomething');
                                     getSomethingCompletedPromise.done(function (something) {
                                            signalx.server.sample2(something*7);
                                       });
                                   }); ",
                        onAppStarted: () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(700, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void client_should_support_named_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message2 = Guid.NewGuid().ToString();
                    string groupName = Guid.NewGuid().ToString();

                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
								signalx.client." + "ClientHandler" + @"=function (m) {
							      signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"','" + "ClientHandler" + @"');
                                });",
                        onAppStarted: () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll("ClientHandler", request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        checks: () => { assert.AreEqual(message1 + message2, finalMessage); });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message2 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m +'" + message2 + @"' ,function(){});
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        checks: () => { assert.AreEqual(message1 + message2, finalMessage); });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m);
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        checks: () => { assert.AreEqual(message1, finalMessage); });
                });
        }

        [TestMethod]
        public void client_dont_need_to_specify_callback_when_passing_back_only_a_message()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message2 = Guid.NewGuid().ToString();
                    string finalMessage = "";
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
							          signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"');
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        checks: () => { assert.AreEqual(message1 + message2, finalMessage); },
                        events: new TestEventHandler(
                            () => { },
                            e => { throw e; },
                            () => { },
                            e => { })
                        );
                });
        }

        [TestMethod]
        public void client_dont_need_to_specify_any_argument_to_call_the_server()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message3 = Guid.NewGuid().ToString();
                    string finalMessage = "";
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                        },
                        checks: () => { assert.AreEqual(message3, finalMessage); }
                        );
                });
        }

        [TestMethod]
        public void will_not_allow_dynamic_server_on_serverside_when_not_set()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message3 = Guid.NewGuid().ToString();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalX.AllowDynamicServer = false;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                            try
                            {
                                //this server will run
                                signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message1; });
                            }
                            catch (Exception e)
                            {
                            }
                        },
                        checks: () =>
                        {
                            assert.AreNotEqual(message1, finalMessage);
                            assert.AreEqual(message3, finalMessage);
                        });
                });
        }

        /// <summary>
        ///     Last server setup will run
        /// </summary>
        [TestMethod]
        public void can_allow_dynamic_server_on_serverside_when_set_to_do_so()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = Guid.NewGuid().ToString();
                    string message3 = Guid.NewGuid().ToString();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        onAppStarted: () =>
                        {
                            signalX.AllowDynamicServer = true;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                            //this server will run
                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message1; });
                        },
                        checks: () =>
                        {
                            assert.AreEqual(message1, finalMessage);
                            assert.AreNotEqual(message3, finalMessage);
                        },
                        events: new TestEventHandler(
                            () => { },
                            e => { throw e; },
                            () =>
                            {
                                int t = 10;
                            },
                            e => { }));
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;

                    return new SignalXTestDefinition(
                        "",
                        onAppStarted: () => { },
                        checks: () => { assert.AreEqual(50, result); },
                        events: new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnAllClients(
                                    $"return 5*10",
                                    (response, request, error) => { result = Convert.ToInt32(response); },
                                    TimeSpan.FromSeconds(15));
                            }));
                });
        }

        [TestMethod]
        public void DIRTY_SANITY_TEST_COMPLICATED_SETUP_SCENARIO()

        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string groupName = Guid.NewGuid().ToString();
                    string groupWatcher = "groupWatcher";
                    string groupWatcher2 = "groupWatcher2";
                    string clientGroupReceiver = "groupClient";
                   // var testObject = new TestObject();
                    string FinalMessage = null;
                    string FinalMessage2 = null;
                    string FinalMessage3 = null;
                    string FinalMessage4 = null;
                    var Message = Guid.NewGuid().ToString();
                    var ClientHandler = "myclientHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    string ServerHandler = "myServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    string TestServerFeedbackHandler = "myTestServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    string TestServerFeedbackHandler2 = "myTestServerHandler2" + Guid.NewGuid().ToString().Replace("-", "");
                    string TestServerFeedbackHandler3 = "myTestServerHandler3" + Guid.NewGuid().ToString().Replace("-", "");
                    string TestServerFeedbackHandler4 = "myTestServerHandler4" + Guid.NewGuid().ToString().Replace("-", "");
                    assert.AreNotEqual(Message, FinalMessage);
                    assert.AreNotEqual(Message, FinalMessage2);
                    assert.AreNotEqual(Message, FinalMessage3);
                    assert.AreNotEqual(Message, FinalMessage4);
                    var VerifiedJoinedGroup = false;
                    var VerifiedJoinedGroup2 = false;
                    return new SignalXTestDefinition(
                        @"
                                var promise = {}
								signalx.client." + ClientHandler + @"=function (m) {
							      signalx.server." + TestServerFeedbackHandler + @"(m,function(m2){
                                     signalx.server." + TestServerFeedbackHandler2 + @"(m2,function(m3){
                                          promise = signalx.server." + TestServerFeedbackHandler3 + @"(m3);
                                          promise.then(function(m4){
                                             signalx.server." + TestServerFeedbackHandler4 + @"(m4);
                                          });
                                     });
                                  });
							    };
                                signalx.client." + clientGroupReceiver + @"=function(c){
                                  signalx.server." + groupWatcher2 + @"(c,function(){});
                                };
                               signalx.ready(function (server) {
                                  signalx.groups.join('" + groupName + @"',function(c){
                                             signalx.server." + groupWatcher + @"(c);
                                          });
							      server." + ServerHandler + @"('" + Message + @"','" + ClientHandler + @"');
                                });",
                        onAppStarted: () =>
                        {
                            //SET UP SERVER
                            signalX.Server(
                                groupWatcher,
                                request =>
                                {
                                    VerifiedJoinedGroup = request.Message as string == groupName;
                                    signalX.RespondToAll(clientGroupReceiver, request.Message, groupName);
                                });
                            signalX.Server(
                                groupWatcher2,
                                request => { VerifiedJoinedGroup2 = request.Message as string == groupName; });
                            signalX.Server(
                                ServerHandler,
                                request => { signalX.RespondToAll(ClientHandler, Message); });
                            signalX.Server(
                                TestServerFeedbackHandler,
                                request =>
                                {
                                    FinalMessage = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, FinalMessage);
                                });
                            signalX.Server(
                                TestServerFeedbackHandler2,
                                request =>
                                {
                                    FinalMessage2 = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, FinalMessage2);
                                });
                            signalX.Server(
                                TestServerFeedbackHandler3,
                                request =>
                                {
                                    FinalMessage3 = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, FinalMessage3);
                                });
                            signalX.Server(
                                TestServerFeedbackHandler4,
                                request => { FinalMessage4 = request.Message as string; });
                        },
                        checks: () =>
                        {
                            assert.AreEqual(Message, FinalMessage);
                            assert.AreEqual(Message, FinalMessage2);
                            assert.AreEqual(Message, FinalMessage3);
                            assert.AreEqual(Message, FinalMessage4);
                            assert.IsTrue(VerifiedJoinedGroup, "verifiedJoinedGroup");
                            assert.IsTrue(VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                        },
                        events: new TestEventHandler(() => { })
                        );
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_on_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    int result = 0;
                    return new SignalXTestDefinition(
                        "",
                        onAppStarted: () => { },
                        checks: () => { assert.AreEqual(result, 3); },
                        events: new TestEventHandler(
                            () =>
                            {
                                signalX.RunJavaScriptOnAllClients(
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
                },1);
        }
    }
}