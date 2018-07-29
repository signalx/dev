namespace SignalXLib.Lib
{
    public enum ConnectionEvents
    {
        OnAfterDisconnect,

        SignalXIncomingRequest,

        SignalXIncomingRequestCompleted,

        SignalXRequestForMethods,

        SignalXRequestForMethodsCompleted,

        OnConnected,

        OnDisconnected,

        OnReconnected,

        OnIncomingError,

        OnAfterConnect,

        BuildRejoiningGroups,

        BuildReconnect,

        BuildOutgoing,

        BuildIncoming,

        BuildDisconnect,

        BuildConnect,

        BuildAuthorizeConnect,

        OnBeforeReconnect,

        OnBeforeOutgoing,

        OnBeforeIncoming,

        OnBeforeDisconnect,

        OnBeforeConnect,

        OnBeforeAuthorizeConnect,

        OnAfterReconnect,

        OnAfterOutgoing,

        OnAfterIncoming,
        SignalXRequestAuthorizationFailed,
        SignalXGroupJoin,
        SignalXGroupLeave
    }
}