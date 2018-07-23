namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;

    public class AuthenticatedConnection : PersistentConnection
    {
        protected override bool AuthorizeRequest(IRequest request)
        {
            if (SignalX.RequireAuthorizationForPersistentConnections)
            {
                return SignalX.IsAuthenticated(request);
            }
            return base.AuthorizeRequest(request);
        }
        
    }
}