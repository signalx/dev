 using Microsoft.VisualStudio.TestTools.UnitTesting;
[assembly: Parallelize(Workers = 20, Scope = ExecutionScope.MethodLevel)]
namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    //https://blogs.msdn.microsoft.com/devops/2018/01/30/mstest-v2-in-assembly-parallel-test-execution/
    [TestClass]
 
    public class when_signalx_is_used_on_the_client_and_server
    {
        [TestMethod]
        public void client_should_support_named_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);

                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    var groupName = Guid.NewGuid().ToString();

                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
								signalx.client." + "ClientHandler" + @"=function (m) {
							      signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"','" + "ClientHandler" + @"');
                                });",
                        server: () =>
                        {
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll("ClientHandler", request.Message);
                            });

                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1 + message2, finalMessage);
                        });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m +'" + message2 + @"' ,function(){});
							       });
                                });",
                        server: () =>
                        {
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1 + message2, finalMessage);
                        });
                });
        }

        [TestMethod]
        public void client_should_support_anonymous_callback2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m);
							       });
                                });",
                        server: () =>
                        {
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1, finalMessage);
                        });
                });
        }

        [TestMethod]
        public void client_dont_need_to_specify_callback_when_passing_back_only_a_message()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    var finalMessage = "";
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
							          signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"');
							       });
                                });",
                        server: () =>
                        {
                            signalx.Server("ServerHandler", request =>
                            {
                                signalx.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalx.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        browserType: BrowserType.Unknown,
                        checks: () => { assert.Equal(message1 + message2, finalMessage); },
                        events: new TestEventHandler(onClientLoaded: () =>
                            {
                            },
                            onClientError: (e) => { throw e; },
                            onCheckSucceeded: () => { },
                            onCheckFailures: (e) => { }));
                });
        }


        [TestMethod]
        public void client_dont_need_to_specify_any_argument_to_call_the_server()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message3 = Guid.NewGuid().ToString();
                    var finalMessage = "";
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server.ServerHandler('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        server: () =>
                        {
                            signalx.Server("ServerHandler", request =>
                            {
                                signalx.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalx.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message3;
                            });
                        },
                        browserType: BrowserType.Unknown,
                        checks: () => { assert.Equal(message3, finalMessage); }
                    );
                });
        }

        [TestMethod]
        public void will_not_allow_dynamic_server_on_serverside_when_not_set()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);
                    var message1 = Guid.NewGuid().ToString();
                    var message3 = Guid.NewGuid().ToString();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        server: () =>
                        {
                            signalX.AllowDynamicServer = false;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message3;
                            });
                            try
                            {
                                //this server will run
                                signalX.Server("TestServerFeedbackHandler", request =>
                                {
                                    finalMessage = message1;
                                });
                            }
                            catch (Exception e)
                            {
                            }
                        },
                        checks: () =>
                        {
                            assert.NotEqual(message1, finalMessage);
                            assert.Equal(message3, finalMessage);
                        });
                });
        }

        /// <summary>
        /// Last server setup will run
        /// </summary>
        [TestMethod]
        public void can_allow_dynamic_server_on_serverside_when_set_to_do_so()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);
                    var message1 = Guid.NewGuid().ToString();
                    var message3 = Guid.NewGuid().ToString();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        server: () =>
                        {
                            signalX.AllowDynamicServer = true;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message3;
                            });
                            //this server will run
                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message1;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1, finalMessage);
                            assert.NotEqual(message3, finalMessage);
                        },
                        events: new TestEventHandler(onClientLoaded: () =>
                        {
                        },
                            onClientError: (e) =>
                            {
                                throw e;
                            },
                            onCheckSucceeded: () =>
                            {
                                var t = 10;
                            },
                            onCheckFailures: (e) =>
                            {
                            }));
                });
        }

        [TestMethod]
        public void server_should_be_able_to_run_simple_javascript_on_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);
                    var result = 0;

                    return new SignalXTestDefinition(
                        script: "",
                        server: () =>
                        {
                        },
                        checks: () =>
                        {
                            assert.Equal(50, result);
                        },
                        events: new TestEventHandler(onClientLoaded: () =>
                        {
                            signalX.RunJavaScriptOnAllClients($"return 5*10",
                                (response, request, error) =>
                                {
                                    result = Convert.ToInt32(response);
                                }, TimeSpan.FromSeconds(15));
                        }));
                });
        }

        [TestMethod]
        public void DIRTY_SANITY_TEST()

        {
            TestObject testObject = SignalXTester.SetupGeneralTest();

            Assert.AreNotEqual(testObject.Message, testObject.FinalMessage);
            Assert.AreNotEqual(testObject.Message, testObject.FinalMessage2);
            Assert.AreNotEqual(testObject.Message, testObject.FinalMessage3);
            Assert.AreNotEqual(testObject.Message, testObject.FinalMessage4);

            //ASSERT
            SignalXTester.CheckExpectations(
                () =>
                {
                    Assert.AreEqual(testObject.Message, testObject.FinalMessage);
                    Assert.AreEqual(testObject.Message, testObject.FinalMessage2);
                    Assert.AreEqual(testObject.Message, testObject.FinalMessage3);
                    Assert.AreEqual(testObject.Message, testObject.FinalMessage4);
                    Assert.IsTrue(testObject.VerifiedJoinedGroup, "verifiedJoinedGroup");
                    Assert.IsTrue(testObject.VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                }, "http://localhost:" + SignalXTester.FreeTcpPort(), testObject);
        }
        [TestMethod]
        public void server_should_be_able_to_run_complex_javascript_on_client()
        {
            SignalXTester.Run((signalX, assert) =>
            {
                SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(20);

                var result = 0;
                return new SignalXTestDefinition(
                    script: "",
                    server: () =>
                    {
                    }, browserType: BrowserType.Unknown, checks: () =>
                    {
                        assert.Equal(result, 3);
                    },
                    events: new TestEventHandler(onClientLoaded: () =>
                    {
                        signalX.RunJavaScriptOnAllClients(@"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                            (response, request, error) =>
                            {
                                result = Convert.ToInt32(response);
                            }, TimeSpan.FromSeconds(10));
                    }));
            });
        }

    }
}