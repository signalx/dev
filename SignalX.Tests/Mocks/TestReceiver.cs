namespace SignalXLib.Tests
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using SignalXLib.Lib;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TestReceiver : ISignalXClientReceiver
    {
        public TestReceiver()
        {
            this.MessagesReceived = new List<TestMessageModel>();
        }

        public List<TestMessageModel> MessagesReceived { set; get; }

        public void Receive(string userId, string clientName, object message)
        {
            this.MessagesReceived.Add(
                new TestMessageModel
                {
                    UserId = userId,
                    ClientName = clientName,
                    Message = message
                });
        }

        public void ReceiveByGroup(string clientName, object message, string groupName = null)
        {
            this.MessagesReceived.Add(
                new TestMessageModel
                {
                    GroupName = groupName,
                    ClientName = clientName,
                    Message = message
                });
        }

       

        public void ReceiveAsOther(string clientName, object message, string excludedConnection, string groupName = null)
        {
            this.MessagesReceived.Add(
                new TestMessageModel
                {
                    GroupName = groupName,
                    ClientName = clientName,
                    Message = message,
                    ExcludedConnection = excludedConnection
                });
        }

        public void ReceiveScripts(string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients)
        {
            this.MessagesReceived.Add(
                new TestMessageModel
                {
                    Script = script
                });
        }

        public void ReceiveInGroupManager(string operation, string userId, string message, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            this.MessagesReceived.Add(
                new TestMessageModel
                {
                    UserId = userId,
                    Message = message
                });
        }

        public async Task RequestScripts(SignalX SignalX, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
         await   SignalX.RespondToScriptRequest(context, clients, groups).ConfigureAwait(false);
        }

        public async Task SendMessageToServer(
            SignalX signalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            string message,
            string replyTo,
            object sender,
            string messageId,
            List<string> groupList)
        {
            await signalX.RespondToServer(context ?? signalX.NullHubCallerContext, clients, groups, handler, message, sender, replyTo ?? signalX.NullHubCallerContext.ConnectionId, groupList);
        }
    }
}