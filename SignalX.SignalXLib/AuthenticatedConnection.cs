namespace SignalXLib.Lib
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;

    public class AuthenticatedConnection : PersistentConnection
    {
        private readonly SignalX SignalX = SignalX.Instance;

        protected override bool AuthorizeRequest(IRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            //todo - this could potentially deadlock because eof the .Result 
            if (this.SignalX.Settings.RequireAuthorizationForPersistentConnections)
                return this.SignalX.IsAuthenticated(correlationId,request, null).Result;
            return base.AuthorizeRequest(request);
        }
    }
}