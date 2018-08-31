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
        public void
            client_cannot_receive_data_from_handler_when_handler_is_not_well_formed_and_handler_is_defined_before_call_to_server
            ()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.client.handler(function (something) {
                                            signalx.server.sample2(something*7);
                                       });
                                     signalx.server.sample('GetSomething','handler');
                                   }); ",
                        () =>
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
        public void
            client_cannot_receive_data_from_handler_when_handler_is_not_well_formed_and_handler_is_not_defined_before_call_to_server
            ()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample('GetSomething','handler');
                                     signalx.client.handler(function (something) {
                                            signalx.server.sample2(something*7);
                                       });
                                   }); ",
                        () =>
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
        public void client_can_receive_data_from_handler_when_handler_is_well_formed_and_handler_is_not_defined_before_call_to_server()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.server.sample('GetSomething','handler');
                                     signalx.client.handler=function (something) {
                                            signalx.server.sample2(something*7);
                                       };
                                   }); ",
                        () =>
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
        public void client_can_receive_data_from_handler_when_handler_is_well_formed_and_handler_is_defined_before_call_to_server()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                      signalx.client.handler=function (something) {
                                            signalx.server.sample2(something*7);
                                       };
                                     signalx.server.sample('GetSomething','handler');
                                   }); ",
                        () =>
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
                        () =>
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
                        () =>
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
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message2 = SignalXExtensions.GenerateUniqueNameId();
                    string groupName = SignalXExtensions.GenerateUniqueNameId();

                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
								signalx.client." + "ClientHandler" + @"=function (m) {
							      signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"','" + "ClientHandler" + @"');
                                });",
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll("ClientHandler", request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.AreEqual(message1 + message2, finalMessage); });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message2 = SignalXExtensions.GenerateUniqueNameId();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m +'" + message2 + @"' ,function(){});
							       });
                                });",
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.AreEqual(message1 + message2, finalMessage); });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m);
							       });
                                });",
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.AreEqual(message1, finalMessage); });
                });
        }

        [TestMethod]
        public void client_dont_need_to_specify_callback_when_passing_back_only_a_message()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message2 = SignalXExtensions.GenerateUniqueNameId();
                    string finalMessage = "";
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
							          signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"');
							       });
                                });",
                        () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.AreEqual(message1 + message2, finalMessage); },
                        new TestEventHandler(
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
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message3 = SignalXExtensions.GenerateUniqueNameId();
                    string finalMessage = "";
                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                        },
                        () => { assert.AreEqual(message3, finalMessage); }
                    );
                });
        }

        [TestMethod]
        public void will_not_allow_dynamic_server_on_serverside_when_not_set()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message3 = SignalXExtensions.GenerateUniqueNameId();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        () =>
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
                        () =>
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
                    string message1 = SignalXExtensions.GenerateUniqueNameId();
                    string message3 = SignalXExtensions.GenerateUniqueNameId();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        () =>
                        {
                            signalX.AllowDynamicServer = true;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                            //this server will run
                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = message1; });
                        },
                        () =>
                        {
                            assert.AreEqual(message1, finalMessage);
                            assert.AreNotEqual(message3, finalMessage);
                        },
                        new TestEventHandler(
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
                        () => { },
                        () => { assert.AreEqual(50, result); },
                        new TestEventHandler(
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
                    string groupName = SignalXExtensions.GenerateUniqueNameId();
                    string groupWatcher = "groupWatcher";
                    string groupWatcher2 = "groupWatcher2";
                    string clientGroupReceiver = "groupClient";
                    // var testObject = new TestObject();
                    string FinalMessage = null;
                    string FinalMessage2 = null;
                    string FinalMessage3 = null;
                    string FinalMessage4 = null;
                    string Message = SignalXExtensions.GenerateUniqueNameId();
                    string ClientHandler = "myclientHandler" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    string ServerHandler = "myServerHandler" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    string TestServerFeedbackHandler = "myTestServerHandler" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    string TestServerFeedbackHandler2 = "myTestServerHandler2" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    string TestServerFeedbackHandler3 = "myTestServerHandler3" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    string TestServerFeedbackHandler4 = "myTestServerHandler4" + SignalXExtensions.GenerateUniqueNameId().Replace("-", "");
                    assert.AreNotEqual(Message, FinalMessage);
                    assert.AreNotEqual(Message, FinalMessage2);
                    assert.AreNotEqual(Message, FinalMessage3);
                    assert.AreNotEqual(Message, FinalMessage4);
                    bool VerifiedJoinedGroup = false;
                    bool VerifiedJoinedGroup2 = false;
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
                        () =>
                        {
                            //SET UP SERVER
                            signalX.Server(
                                groupWatcher,
                                request =>
                                {
                                    VerifiedJoinedGroup = request.Message as string == groupName;
                                    signalX.RespondToAllInGroup(clientGroupReceiver, request.Message, groupName);
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
                        () =>
                        {
                            assert.AreEqual(Message, FinalMessage);
                            assert.AreEqual(Message, FinalMessage2);
                            assert.AreEqual(Message, FinalMessage3);
                            assert.AreEqual(Message, FinalMessage4);
                            assert.IsTrue(VerifiedJoinedGroup, "verifiedJoinedGroup");
                            assert.IsTrue(VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                        },
                        new TestEventHandler(() => { })
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
                        () => { },
                        () => { assert.AreEqual(result, 3); },
                        new TestEventHandler(
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
                },
                1);
        }
    }
}