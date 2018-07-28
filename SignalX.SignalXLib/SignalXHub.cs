using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalXLib.Lib
{
    using System.Threading;

    //use GlobalHost.HubPipeline.RequireAuthentication(); to lock sown all your hubs

    public partial class SignalXHub : Hub
    {
        public void Send(string handler, object message, string replyTo, object sender, string messageId)
        {
            SignalX.Send(Context, Clients, handler, message, replyTo, sender, messageId);
        }

        public void GetMethods()
        {
            SignalX.GetMethods(Context, Clients);
        }

        public override Task OnConnected()
        {
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnConnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Connections?.Add(name, Context?.ConnectionId);
            SignalX.HasHadAConnection = true;
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnDisconnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Connections?.Remove(name, Context?.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnReconnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                if (SignalX.Connections != null && !SignalX.Connections.GetConnections(name).Contains(Context?.ConnectionId))
                {
                    SignalX.Connections?.Add(name, Context?.ConnectionId);
                }

            return base.OnReconnected();
        }
    }
}