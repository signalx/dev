namespace SignalXLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_signalx_is_used: SignalXAssertionLib
    {
        [TestMethod]
        public async Task Test()
        {
            var message = Guid.NewGuid().ToString();
            TestReceiver receiver=null;
            var signalX = SignalX.CreateInstance(
                (instance) =>
                {
                    receiver= new TestReceiver();
                    return receiver;
                });
            signalX.Server("us", (request) =>
            {
                request.RespondToSender(message);
            });
            signalX.Server("sam",(request) =>
            {
                request.Forward("us");
                request.ForwardAsync("us");
                request.RespondToSender(message);
            });
            signalX.SetSignalXClientAsReady();
            signalX.RespondToServer("sam","abc");
            await WillPassBeforeGivenTime(TimeSpan.FromSeconds(1000),
                () =>
                {
                    AreEqual(receiver.LastMessageReceived.Message, message);
                });
            
        }
    }
}