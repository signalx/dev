namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISignalXClientReceiver
    {
        void Receive(string user, string clientName, object message);

        void ReceiveByGroup(string clientName, object message, string groupName = null);

        void ReceiveAsOther(string clientName, object message, string excludedConnection, string groupName = null);

        void ReceiveScripts(string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients);

        void ReceiveInGroupManager(string operation, string userId, string groupName, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups);

        Task RequestScripts(
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups);

        Task SendMessageToServer(
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            string message,
            string replyTo,
            object sender,
            string messageId,
            List<string> groupList);
    }
}