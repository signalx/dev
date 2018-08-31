namespace SignalXLib.Lib
{
    using System.Threading;

    class ServerHandlerDetails
    {
        public ServerHandlerDetails(bool requiresAuthorization, bool isSingleWriter)
        {
            this.RequiresAuthorization = requiresAuthorization;
            this.IsSingleWriter = isSingleWriter;
            this.SingleWriter = new ReaderWriterLockSlim();
        }

        internal bool RequiresAuthorization { get; }

        public bool IsSingleWriter { get; }

        public ReaderWriterLockSlim SingleWriter { set; get; }

        public SignalXServerState State { get; set; }
    }
}