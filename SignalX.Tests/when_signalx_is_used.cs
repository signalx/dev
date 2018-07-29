namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using Xunit;

    public class when_signalx_is_used
    {
        [Fact]
        public void Test()
        {
            var receiver = new TestReceiver();
            SignalX.Receiver = receiver;
        }
    }
}