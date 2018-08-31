namespace SignalXLib.Lib
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;

    //use GlobalHost.HubPipeline.RequireAuthentication(); to lock sown all your hubs

    public partial class SignalXHub : Hub
    {
        readonly SignalX signalX = SignalX.Instance;

        public void Send(string handler, dynamic message, string replyTo, dynamic sender, string messageId)
        {
            this.signalX.SendMessageToServer(this.Context, this.Clients, this.Groups, handler, message, replyTo, sender, messageId);
        }

        public void JoinGroup(string groupName)
        {
            this.signalX.JoinGroup(this.Context, this.Clients, this.Groups, groupName);
        }

        public void LeaveGroup(string groupName)
        {
            this.signalX.LeaveGroup(this.Context, this.Clients, this.Groups, groupName);
        }

        public void GetMethods()
        {
            this.signalX.RespondToScriptRequest(this.Context, this.Clients, this.Groups);
        }

        public void SignalXClientReady()
        {
            this.signalX.Settings.HasOneOrMoreConnections = true;
            this.signalX.ConnectionCount++;
        }

        public void SignalXClientReadyError(string message, string error)
        {
        }

        public override Task OnConnected()
        {
            this.signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnConnected.ToString(), null));
            string name = this.Context?.User?.Identity?.Name;

            if (name != null)
                this.signalX.Settings.Connections?.Add(this.signalX, name, this.Context?.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this.signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnDisconnected.ToString(), null));
            string name = this.Context?.User?.Identity?.Name;

            if (name != null)
                this.signalX.Settings.Connections?.Remove(this.signalX, name, this.Context?.ConnectionId);

            try
            {
                if (this.signalX.ConnectionCount != 0)
                    this.signalX.ConnectionCount--;
            }
            catch (Exception)
            {
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            this.signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnReconnected.ToString(), null));
            string name = this.Context?.User?.Identity?.Name;

            if (name != null)
                if (this.signalX.Settings.Connections != null && !this.signalX.Settings.Connections.GetConnections(name).Contains(this.Context?.ConnectionId))
                    this.signalX.Settings.Connections?.Add(this.signalX, name, this.Context?.ConnectionId);
            this.signalX.ConnectionCount++;
            return base.OnReconnected();
        }
    }
}