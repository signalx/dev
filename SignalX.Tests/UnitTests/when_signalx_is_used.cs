namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    
    [TestClass]
    public class when_signalx_is_used
    {
        [TestMethod]
        public void Test()
        {
            var receiver = new TestReceiver(SignalX.Instance());
            SignalX.Instance().Settings.Receiver = receiver;
        }
    }
}