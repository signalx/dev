using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class SignalXIntegrationTest
    {
        [TestMethod]
        public void it_should_be_able_to_communicate_back_and_forth_with_the_client()
        {
            //SET UP CLIENT
            TestObject testObject = TestHelper.SetUpScriptForTest((testData) => @"
                                var promise = {}
								signalx.client." + testData.clientHandler + @"=function (m) {
							      signalx.server." + testData.testServerFeedbackHandler + @"(m,function(m2){
                                     signalx.server." + testData.testServerFeedbackHandler2 + @"(m2,function(m3){
                                          promise = signalx.server." + testData.testServerFeedbackHandler3 + @"(m3);
                                          promise.then(function(m4){
                                             signalx.server." + testData.testServerFeedbackHandler4 + @"(m4);
                                          });
                                     });
                                  });
							    };
                               signalx.ready(function (server) {
							      server." + testData.serverHandler + @"('" + testData.message + @"','" + testData.clientHandler + @"');
                                });");
            Assert.AreNotEqual(testObject.message, testObject.finalMessage);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage2);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage3);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage4);

            //SET UP SERVER
            SignalX.Server(testObject.serverHandler, request =>
             {
                 SignalX.RespondToAll(testObject.clientHandler, testObject.message);
             });
            SignalX.Server(testObject.testServerFeedbackHandler, request =>
            {
                testObject.finalMessage = request.Message as string;
                SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage);
            });
            SignalX.Server(testObject.testServerFeedbackHandler2, request =>
            {
                testObject.finalMessage2 = request.Message as string;
                SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage2);
            });
            SignalX.Server(testObject.testServerFeedbackHandler3, request =>
            {
                testObject.finalMessage3 = request.Message as string;
                SignalX.RespondToAll(request.ReplyTo, testObject.finalMessage3);
            });
            SignalX.Server(testObject.testServerFeedbackHandler4, request =>
            {
                testObject.finalMessage4 = request.Message as string;
            });

            Assert.AreNotEqual(testObject.message, testObject.finalMessage);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage2);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage3);
            Assert.AreNotEqual(testObject.message, testObject.finalMessage4);

            //ASSERT
            TestHelper.CheckExpectations(
                  () =>
                  {
                      Assert.AreEqual(testObject.message, testObject.finalMessage);
                      Assert.AreEqual(testObject.message, testObject.finalMessage2);
                      Assert.AreEqual(testObject.message, testObject.finalMessage3);
                      Assert.AreEqual(testObject.message, testObject.finalMessage4);
                  }, "http://localhost:44111");

            TestHelper.CheckExpectations(
                () =>
                {
                    Assert.AreEqual(testObject.message, testObject.finalMessage);
                    Assert.AreEqual(testObject.message, testObject.finalMessage2);
                    Assert.AreEqual(testObject.message, testObject.finalMessage3);
                    Assert.AreEqual(testObject.message, testObject.finalMessage4);
                }, "http://localhost:44111");
        }
    }
}