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
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.Equal<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.Equal<int>(700, request.Message); });
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
                                    assert.Equal<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.Equal<int>(700, request.Message); });
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
                                    assert.Equal<string>("GetSomething", request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.Equal<int>(700, request.Message); });
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
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll("ClientHandler", request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.Equal(message1 + message2, finalMessage); });
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
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.Equal(message1 + message2, finalMessage); });
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
                        () =>
                        {
                            signalX.Server("ServerHandler", request => { signalX.RespondToAll(request.ReplyTo, request.Message); });

                            signalX.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.Equal(message1, finalMessage); });
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
                        () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = request.Message as string; });
                        },
                        () => { assert.Equal(message1 + message2, finalMessage); },
                        new TestEventHandler(
                            () => { },
                            e => { throw e; },
                            () => { },
                            e => { }),
                        browserType: BrowserType.Default);
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
                        () =>
                        {
                            signalx.Server("ServerHandler", request => { signalx.RespondToAll(request.ReplyTo, request.Message); });

                            signalx.Server("TestServerFeedbackHandler", request => { finalMessage = message3; });
                        },
                        () => { assert.Equal(message3, finalMessage); },
                        browserType: BrowserType.Default);
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
                            assert.NotEqual(message1, finalMessage);
                            assert.Equal(message3, finalMessage);
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
                            assert.Equal(message1, finalMessage);
                            assert.NotEqual(message3, finalMessage);
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
                        () => { assert.Equal(50, result); },
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
                    string groupName = Guid.NewGuid().ToString();
                    string groupWatcher = "groupWatcher";
                    string groupWatcher2 = "groupWatcher2";
                    string clientGroupReceiver = "groupClient";
                    var testObject = new TestObject();
                    testObject.FinalMessage = null;
                    testObject.FinalMessage2 = null;
                    testObject.FinalMessage3 = null;
                    testObject.FinalMessage4 = null;
                    testObject.Message = Guid.NewGuid().ToString();
                    testObject.ClientHandler = "myclientHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    testObject.ServerHandler = "myServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    testObject.TestServerFeedbackHandler = "myTestServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                    testObject.TestServerFeedbackHandler2 = "myTestServerHandler2" + Guid.NewGuid().ToString().Replace("-", "");
                    testObject.TestServerFeedbackHandler3 = "myTestServerHandler3" + Guid.NewGuid().ToString().Replace("-", "");
                    testObject.TestServerFeedbackHandler4 = "myTestServerHandler4" + Guid.NewGuid().ToString().Replace("-", "");
                    assert.NotEqual(testObject.Message, testObject.FinalMessage);
                    assert.NotEqual(testObject.Message, testObject.FinalMessage2);
                    assert.NotEqual(testObject.Message, testObject.FinalMessage3);
                    assert.NotEqual(testObject.Message, testObject.FinalMessage4);
                    testObject.VerifiedJoinedGroup = false;
                    testObject.VerifiedJoinedGroup2 = false;
                    return new SignalXTestDefinition(
                        @"
                                var promise = {}
								signalx.client." + testObject.ClientHandler + @"=function (m) {
							      signalx.server." + testObject.TestServerFeedbackHandler + @"(m,function(m2){
                                     signalx.server." + testObject.TestServerFeedbackHandler2 + @"(m2,function(m3){
                                          promise = signalx.server." + testObject.TestServerFeedbackHandler3 + @"(m3);
                                          promise.then(function(m4){
                                             signalx.server." + testObject.TestServerFeedbackHandler4 + @"(m4);
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
							      server." + testObject.ServerHandler + @"('" + testObject.Message + @"','" + testObject.ClientHandler + @"');
                                });",
                        () =>
                        {
                            //SET UP SERVER
                            signalX.Server(
                                groupWatcher,
                                request =>
                                {
                                    testObject.VerifiedJoinedGroup = request.Message as string == groupName;
                                    signalX.RespondToAll(clientGroupReceiver, request.Message, groupName);
                                });
                            signalX.Server(
                                groupWatcher2,
                                request => { testObject.VerifiedJoinedGroup2 = request.Message as string == groupName; });
                            signalX.Server(
                                testObject.ServerHandler,
                                request => { signalX.RespondToAll(testObject.ClientHandler, testObject.Message); });
                            signalX.Server(
                                testObject.TestServerFeedbackHandler,
                                request =>
                                {
                                    testObject.FinalMessage = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, testObject.FinalMessage);
                                });
                            signalX.Server(
                                testObject.TestServerFeedbackHandler2,
                                request =>
                                {
                                    testObject.FinalMessage2 = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, testObject.FinalMessage2);
                                });
                            signalX.Server(
                                testObject.TestServerFeedbackHandler3,
                                request =>
                                {
                                    testObject.FinalMessage3 = request.Message as string;
                                    signalX.RespondToAll(request.ReplyTo, testObject.FinalMessage3);
                                });
                            signalX.Server(
                                testObject.TestServerFeedbackHandler4,
                                request => { testObject.FinalMessage4 = request.Message as string; });
                        },
                        () =>
                        {
                            assert.Equal(testObject.Message, testObject.FinalMessage);
                            assert.Equal(testObject.Message, testObject.FinalMessage2);
                            assert.Equal(testObject.Message, testObject.FinalMessage3);
                            assert.Equal(testObject.Message, testObject.FinalMessage4);
                            assert.IsTrue(testObject.VerifiedJoinedGroup, "verifiedJoinedGroup");
                            assert.IsTrue(testObject.VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                        },
                        new TestEventHandler(() => { }),
                        browserType: BrowserType.Default);
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
                        () => { assert.Equal(result, 3); },
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
                            }),
                        browserType: BrowserType.Default);
                });
        }
    }
}