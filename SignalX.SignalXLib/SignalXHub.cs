using Microsoft.AspNet.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace SignalXLib.Lib
{
    //use GlobalHost.HubPipeline.RequireAuthentication(); to lock sown all your hubs

    public partial class SignalXHub : Hub
    {
        SignalX SignalX = Lib.SignalX.Instance();
        public void Send(string handler, dynamic message, string replyTo, dynamic sender, string messageId)
        {
            SignalX.SendMessageToServer(Context, Clients, Groups, handler, message, replyTo, sender, messageId);
        }

        public void JoinGroup(string groupName)
        {
            SignalX.JoinGroup(Context, Clients, Groups, groupName);
        }

        public void LeaveGroup(string groupName)
        {
            SignalX.LeaveGroup(Context, Clients, Groups, groupName);
        }

        public void GetMethods()
        {
            SignalX.RespondToScriptRequest(Context, Clients, Groups);
        }

        public override Task OnConnected()
        {
            SignalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnConnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Settings.Connections?.Add(SignalX,name, Context?.ConnectionId);
            
            SignalX.Settings.HasOneOrMoreConnections = true;

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            SignalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnDisconnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Settings.Connections?.Remove(SignalX,name, Context?.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            SignalX.Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnReconnected.ToString(), null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                if (SignalX.Settings.Connections != null && !SignalX.Settings.Connections.GetConnections(name).Contains(Context?.ConnectionId))
                {
                    SignalX.Settings.Connections?.Add(SignalX,name, Context?.ConnectionId);
                }

            return base.OnReconnected();
        }
    }
}