namespace SignalXLib.Lib
{
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;

    public class AuthenticatedConnection : PersistentConnection
    {
        private readonly SignalX SignalX = SignalX.Instance;

        protected override bool AuthorizeRequest(IRequest request)
        {
            //todo - this could potentially deadlock because eof the .Result 
            if (this.SignalX.Settings.RequireAuthorizationForPersistentConnections)
                return this.SignalX.IsAuthenticated(request, null).Result;
            return base.AuthorizeRequest(request);
        }
    }
}