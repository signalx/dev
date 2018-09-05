namespace SignalXLib.Lib
{
    using System.Collections.Generic;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public interface ISignalXClientReceiver
    {
        void Receive(string user, string clientName, dynamic message);

        void ReceiveByGroup(string clientName, dynamic message, string groupName = null);

        void ReceiveAsOther(string clientName, dynamic message, string excludedConnection, string groupName = null);

        void ReceiveScripts(string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients);

        void ReceiveInGroupManager(string operation, string userId, dynamic groupName, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups);

        void RequestScripts(
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups);

        void SendMessageToServer(
            SignalX SignalX,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            dynamic message,
            string replyTo,
            object sender,
            string messageId,
            List<string> groupList);
    }
}