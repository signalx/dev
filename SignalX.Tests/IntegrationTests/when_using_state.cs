namespace SignalXLib.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_using_state
    {
        [TestMethod]
        public void it_should_be_able_to_store_state()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                      signalx.client.getSomethingCompleted2=function (something) {
                                            signalx.server.final(something);
                                       };
                                     signalx.client.getSomethingCompleted=function (something) {
                                            signalx.server.sample(100,'getSomethingCompleted2');
                                       };
                                     signalx.server.sample(10,'getSomethingCompleted');
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                 (request,state) =>
                                 {
                                    
                                    if (state.Data == null)
                                    {
                                        state.Data= request.MessageAs<int>();
                                    }
                                    else
                                    {
                                        state.Data = state.Data * 2;
                                    }
                                  
                                    request.RespondToSender(state.Data);
                                });
                            signalx.Server(
                                "final",
                                (request, state) =>
                                {
                                    result = request.MessageAs<int>();
                                });
                        },
                        () => { assert.AreEqual(result, 20); }
                    );
                });
        }
    }
}