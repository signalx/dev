namespace SignalXLib.Lib
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public interface ISignalXClientReceiver
    {
        void Receive(string user, string clientName, object message, string correlationId);

        void ReceiveByGroup(string correlationId, string clientName, object message, string groupName = null);

        void ReceiveAsOther(string correlationId, string clientName, object message, string excludedConnection, string groupName = null);

        void ReceiveScripts(string correlationId, string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients);

        void ReceiveInGroupManager(string correlationId, string operation, string userId, string groupName, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups);

        Task RequestScripts(
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string correlationId);

        Task SendMessageToServer(
            string correlationId,
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            object message,
            string replyTo,
            object sender,
            string messageId,
            List<string> groupList);
    }
}