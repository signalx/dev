namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;

    public class AuthenticatedConnection : PersistentConnection
    {
        SignalX SignalX = SignalX.Instance;
        protected override bool AuthorizeRequest(IRequest request)
        {
           
            if (SignalX.Settings.RequireAuthorizationForPersistentConnections)
            {
                return SignalX.IsAuthenticated(request);
            }
            return base.AuthorizeRequest(request);
        }
    }
}