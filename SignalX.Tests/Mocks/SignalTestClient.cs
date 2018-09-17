namespace SignalXLib.Tests
{
    using SignalXLib.Lib;

    public class SignalTestClient
    {
        public SignalTestClient(ISignalXClientReceiver receiver)
        {
            this.Receiver = receiver;
            this.SignalX = SignalX.Instance;
            this.Receiver.RequestScripts(this.SignalX, this.SignalX.NullHubCallerContext, null, null);
        }

        private SignalX SignalX { get; }

        private ISignalXClientReceiver Receiver { get; }
    }
}