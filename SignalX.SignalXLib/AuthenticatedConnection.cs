namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;

    public class AuthenticatedConnection : PersistentConnection
    {
        readonly SignalX SignalX = SignalX.Instance;

        protected override bool AuthorizeRequest(IRequest request)
        {
            if (this.SignalX.Settings.RequireAuthorizationForPersistentConnections)
                return this.SignalX.IsAuthenticated(request);
            return base.AuthorizeRequest(request);
        }
    }
}