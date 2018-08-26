namespace SignalXLib.Tests
{
    using System;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using SignalXLib.Lib;

    public class TestReceiver : ISignalXClientReceiver
    {
        SignalX SignalX;
        public TestReceiver(SignalX signalX)
        {
            SignalX = signalX ?? throw new ArgumentNullException(nameof(signalX));
            this.LastMessageReceived = new TestMessageModel();
        }

        public TestMessageModel LastMessageReceived { set; get; }

        public void Receive(string userId, string clientName, dynamic message)
        {
            this.LastMessageReceived = new TestMessageModel();
            this.LastMessageReceived.UserId = userId;
            this.LastMessageReceived.ClientName = clientName;
            this.LastMessageReceived.Message = message;
        }

        public void ReceiveByGroup(string clientName, dynamic message, string groupName = null)
        {
            this.LastMessageReceived = new TestMessageModel();
            this.LastMessageReceived.GroupName = groupName;
            this.LastMessageReceived.ClientName = clientName;
            this.LastMessageReceived.Message = message;
        }

        public void ReceiveAsOther(string clientName, dynamic message, string excludedConnection, string groupName = null)
        {
            this.LastMessageReceived = new TestMessageModel();
            this.LastMessageReceived.GroupName = groupName;
            this.LastMessageReceived.ExcludedConnection = excludedConnection;
            this.LastMessageReceived.ClientName = clientName;
            this.LastMessageReceived.Message = message;
        }

        public void ReceiveScripts(string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients)
        {
            this.LastMessageReceived = new TestMessageModel();

            this.LastMessageReceived.Script = script;
        }

        public void ReceiveInGroupManager(string userId, dynamic message, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            this.LastMessageReceived = new TestMessageModel();
            this.LastMessageReceived.UserId = userId;
            this.LastMessageReceived.Message = message;
        }

        public void RequestScripts(SignalX SignalX,HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            SignalX.RespondToScriptRequest(context, clients, groups);
        }

        public void SendMessageToServer(SignalX SignalX,HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, dynamic message, string replyTo, object sender, string messageId)
        {
            SignalX.SendMessageToServer(context, clients, groups, handler, message, replyTo, sender, messageId);
        }
    }
}