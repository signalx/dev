using System.Threading;

namespace SignalXLib.Lib
{
    internal class ServerHandlerDetails
    {
        public ServerHandlerDetails(bool requiresAuthorization, bool isSingleWriter)
        {
            this.RequiresAuthorization = requiresAuthorization;
            this.IsSingleWriter = isSingleWriter;
            this.SingleWriter = new ReaderWriterLockSlim();
        }

        internal bool RequiresAuthorization { private set; get; }

        public bool IsSingleWriter { get; private set; }

        public ReaderWriterLockSlim SingleWriter {set;get;}
        public SignalXServerState State { get; set; }
    }
}