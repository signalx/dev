namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using Xunit;

    public class SignalXIntegrationTest1
    {

        public SignalXIntegrationTest1()
        {
            TestHelper.MaxTestWaitTime = TimeSpan.FromSeconds(20);
        }


        /// <summary>
        /// Last server setup will run
        /// </summary>
        [Fact]
        public void can_allow_dynamic_server_on_serverside2()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.testServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            SignalX.AllowDynamicServer = true;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });
            //this server will be ignored
            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });
            //this server will run
            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = message1;
            });
           

            //ASSERT
            TestHelper.CheckExpectationsExpectingFailures(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.indexPage);
        }


        [Fact]
        public void can_allow_dynamic_server_on_serverside()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.testServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            SignalX.AllowDynamicServer = true;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = message1;
            });
            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.indexPage);
        }



        [Fact]
        public void client_dont_need_to_specify_any_argument_to_call_the_server()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message3 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.testServerFeedbackHandler + @"();
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = message3;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message3, finalMessage), "http://localhost:44111", testObject.indexPage);
        }



        [Fact]
        public void client_dont_need_to_specify_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.testServerFeedbackHandler + @"(m +'" + message2 + @"');
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.indexPage);
        }




        [Fact]
        public void client_should_support_anonymous_callback2()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {							          
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectationsExpectingFailures(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.indexPage);
        }



        [Fact]
        public void client_should_support_anonymous_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"',function (m) {
							          signalx.server." + testData.testServerFeedbackHandler + @"(m +'" + message2 + @"' ,function(){});
							       });
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(request.ReplyTo, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.indexPage);
        }

         [Fact]
        public void client_should_support_named_callback()
        {
            TestHelper TestHelper = new TestHelper();
            var message1 = Guid.NewGuid().ToString();
            var message2 = Guid.NewGuid().ToString();
            var groupName = Guid.NewGuid().ToString();
            //SET UP CLIENT
            TestObject testObject =
                TestHelper.SetUpScriptForTest((testData) => @"
								signalx.client." + testData.clientHandler + @"=function (m) {
							      signalx.server." + testData.testServerFeedbackHandler + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + message1 + @"','" + testData.clientHandler + @"');
                                });");

            string finalMessage = null;
            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
            {
                SignalX.RespondToAll(testObject.clientHandler, request.Message);
            });

            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                finalMessage = request.Message as string;
            });

            //ASSERT
            TestHelper.CheckExpectations(() => Assert.Equal(message1 + message2, finalMessage), "http://localhost:44111", testObject.indexPage);
        }

        [Fact]
        public void COMPLETE_TEST_it_should_be_able_to_communicate_back_and_forth_with_the_client()

        {
            TestObject testObject = TestHelper.SetupGeneralTest();

            Assert.NotEqual(testObject.message, testObject.finalMessage);
            Assert.NotEqual(testObject.message, testObject.finalMessage2);
            Assert.NotEqual(testObject.message, testObject.finalMessage3);
            Assert.NotEqual(testObject.message, testObject.finalMessage4);

            //ASSERT
            TestHelper.CheckExpectations(
                  () =>
                  {
                      Assert.Equal(testObject.message, testObject.finalMessage);
                      Assert.Equal(testObject.message, testObject.finalMessage2);
                      Assert.Equal(testObject.message, testObject.finalMessage3);
                      Assert.Equal(testObject.message, testObject.finalMessage4);
                      Assert.True(testObject.verifiedJoinedGroup, "verifiedJoinedGroup");
                      Assert.True(testObject.verifiedJoinedGroup2, "verifiedJoinedGroup2");
                  }, "http://localhost:44111", testObject.indexPage);
        }
    }
}