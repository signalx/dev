namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    //use GlobalHost.HubPipeline.RequireAuthentication(); to lock sown all your hubs

    public partial class SignalXHub : Hub
    {
        private readonly SignalX signalX = SignalX.Instance;

        //not async because it already behaves as async from client clide
        public async Task Send(string handler, string message, string replyTo, dynamic sender, string messageId, List<string> groups)
        {
            this.signalX.Advanced.Trace(messageId, $"Received message from client {handler}...");
            Task task = this.signalX.SendMessageToServer(messageId, this.Context, this.Clients, this.Groups, handler, message, replyTo, sender, messageId, groups, false, null);
            await task.ConfigureAwait(false);
        }

        //not async because it already behaves as async from client clide
        public void JoinGroup(string groupName)
        {
            var correlationId = Guid.NewGuid().ToString();
            this.signalX.JoinGroup(Context.ConnectionId + "_" + correlationId, this.Context, this.Clients, this.Groups, groupName);
        }

        //not async because it already behaves as async from client clide
        public void LeaveGroup(string groupName)
        {
            var correlationId = Guid.NewGuid().ToString();
            this.signalX.LeaveGroup(Context.ConnectionId + "_" + correlationId, this.Context, this.Clients, this.Groups, groupName);
        }

        public async Task GetMethods()
        {  var correlationId = Guid.NewGuid().ToString();

            this.signalX.Advanced.Trace(Context.ConnectionId + "_" + correlationId, "Sending methods to client...");
          
            await  this.signalX.RespondToScriptRequest(Context.ConnectionId + "_" + correlationId,  this.Context, this.Clients, this.Groups).ConfigureAwait(false);
        }

        public void SignalXClientReady()
        {
            this.signalX.SetSignalXClientAsReady();
        }

        public void SignalXClientReadyError(string message, string error)
        {
        }

        public override Task OnConnected()
        {
            this.signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnConnected.ToString(), null));
            string name = this.Context?.User?.Identity?.Name ?? this.Context?.ConnectionId;

            if (name != null)
                this.signalX.Connections?.Add(this.signalX, name, this.Context?.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this.signalX.Advanced.Trace(Context.ConnectionId + "_" + Guid.NewGuid(), "Client is disconnected");
            this.signalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnDisconnected.ToString(), null));
            string name = this.Context?.User?.Identity?.Name ?? this.Context?.ConnectionId;

            if (name != null)
                this.signalX.Connections?.Remove(this.signalX, name, this.Context?.ConnectionId);

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
            string name = this.Context?.User?.Identity?.Name ?? this.Context?.ConnectionId;

            if (name != null)
                if (this.signalX.Connections != null && !this.signalX.Connections.GetByKey(name).Contains(this.Context?.ConnectionId))
                    this.signalX.Connections?.Add(this.signalX, name, this.Context?.ConnectionId);
            this.signalX.ConnectionCount++;
            return base.OnReconnected();
        }
    }
}