namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class when_signalx_is_used : SignalXAssertionLib
    {
        [TestMethod]
        public async Task Test()
        {
            string message = Guid.NewGuid().ToString();
            TestReceiver receiver = null;
            SignalX signalX = SignalX.CreateInstance(
                instance =>
                {
                    receiver = new TestReceiver();
                    return receiver;
                });
            var signalTestClient = new SignalTestClient(receiver);

            signalX.Server("us", request => { request.RespondToSender(message); });
            signalX.Server(
                "sam",
                async request =>
                {
                    //request.Forward("us");
                    await request.ForwardAsync("us");
                    request.RespondToServer("us", "abc");
                    request.RespondToSender(message);
                });
            signalX.SetSignalXClientAsReady();
            signalX.RespondToServer("sam", "abc");
            await this.WillPassBeforeGivenTime(
                TimeSpan.FromSeconds(1000),
                () => { this.AreEqual(receiver.MessagesReceived.LastOrDefault().Message, message); });
        }
    }
}