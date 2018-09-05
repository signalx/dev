namespace SignalXLib.Lib
{
    using System.Collections.Generic;
    using System.Threading;

    class ServerHandlerDetails
    {
        public ServerHandlerDetails(bool requiresAuthorization, bool isSingleWriter, List<string> allowedGroups)
        {
            this.RequiresAuthorization = requiresAuthorization;
            this.IsSingleWriter = isSingleWriter;
            this.SingleWriter = new ReaderWriterLockSlim();
            this.AllowedGroups = allowedGroups;
        }

        internal bool RequiresAuthorization { get; }

        public bool IsSingleWriter { get; }

        public ReaderWriterLockSlim SingleWriter { set; get; }

        public SignalXServerState State { get; set; }
        public List<string> AllowedGroups { get; set; }
    }
}