namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using Xunit;
    using Assert = Xunit.Assert;

    public class SignalXIntegrationTest1
    {
        public SignalXIntegrationTest1()
        {
            TestHelper.MaxTestWaitTime = TimeSpan.FromSeconds(20);
        }

        [Fact]
        public void server_should_be_able_to_run_javascript_on_client()
        {
            TestHelper.MaxTestWaitTime = TimeSpan.FromSeconds(200);
            var result = 0;
            var scenario = new ScenarioDefinition(
                script: "",
                server: () =>
                {
                },
                checks: () =>
                {
                    Assert.Equal(50, result);
                },
                onClientLoaded: () =>
                {
                    SignalX.RunJavaScriptOnAllClients($"return 5*10",
                        (response, request, error) =>
                        {
                            result = Convert.ToInt32(response);
                        }, TimeSpan.FromSeconds(5));
                }
            );
            TestHelper.RunScenario(scenario);
        }

        /// <summary>
        /// Last server setup will run
        /// </summary>
        [Fact]
        [Timeout(1)]
        public void can_allow_dynamic_server_on_serverside2()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.TestServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            SignalX.AllowDynamicServer = true;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });
            //this server will be ignored
            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });
            //this server will run
            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = message1;
            });

            //ASSERT
            TestHelper.CheckExpectationsExpectingFailures(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void can_allow_dynamic_server_on_serverside()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.TestServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            SignalX.AllowDynamicServer = true;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = message1;
            });
            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void client_dont_need_to_specify_any_argument_to_call_the_server()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.TestServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void client_dont_need_to_specify_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.TestServerFeedbackHandler + @"(m +'" + message2 + @"');
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void client_should_support_anonymous_callback2()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectationsExpectingFailures(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void client_should_support_anonymous_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.TestServerFeedbackHandler + @"(m +'" + message2 + @"' ,function(){});
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void client_should_support_named_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            var groupName = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
								signalx.client." + testData.ClientHandler + @"=function (m) {
							      signalx.server." + testData.TestServerFeedbackHandler + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + testData.ServerHandler + @"('" + message1 + @"','" + testData.ClientHandler + @"');
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.ServerHandler, request =>
            {
                SignalX.RespondToAll(testObject.ClientHandler, request.Message);
            });

            SignalX.Server(testObject.TestServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.IndexPage);
        }

        [Fact]
        [Timeout(1)]
        public void COMPLETE_TEST_it_should_be_able_to_communicate_back_and_forth_with_the_client()

        {
            TestObject testObject = TestHelper.SetupGeneralTest();

            Assert.NotEqual(testObject.Message, testObject.FinalMessage);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage2);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage3);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage4);

            //ASSERT
            TestHelper.CheckExpectations(
                  () =>
                  {
                      Assert.Equal(testObject.Message, testObject.FinalMessage);
                      Assert.Equal(testObject.Message, testObject.FinalMessage2);
                      Assert.Equal(testObject.Message, testObject.FinalMessage3);
                      Assert.Equal(testObject.Message, testObject.FinalMessage4);
                      Assert.True(testObject.VerifiedJoinedGroup, "verifiedJoinedGroup");
                      Assert.True(testObject.VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                  }, "http://localhost:44111", testObject.IndexPage);
        }
    }
}