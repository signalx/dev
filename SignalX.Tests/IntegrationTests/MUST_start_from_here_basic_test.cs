namespace SignalXLib.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_sending_message_to_server
    {
        public class SampleClass
        {
            public string Data { set; get; }
        }
        [TestMethod]
        public void server_should_not_deserialize_object_sent()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2({Data : something});
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<int>(100, request.MessageAs<int>(), "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100/10);
                                });
                            signalx.Server(
                                "sample2",
                                request =>
                                {
                                    assert.AreEqual<int>(10, Convert.ToInt32(request.MessageAs<SampleClass>().Data));
                                    request.RespondToServer("sample3", request.MessageAs<SampleClass>());
                                });
                            signalx.Server(
                                "sample3",
                                request =>
                                {
                                    assert.AreEqual(JsonConvert.SerializeObject(request.MessageAs<SampleClass>()), request.MessageAsJsonString);
                                    assert.AreEqual(10, Convert.ToInt32(request.MessageAs<SampleClass>().Data));
                                });
                        });
                });
        }
        
    }
}