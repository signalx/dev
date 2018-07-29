namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public partial class SignalX
    {
        public interface ISignalXClientReceiver
        {
            void Receive(string userId, string clientName, dynamic message);

            void Receive(string clientName, dynamic message, string groupName = null);

            void ReceiveAsOther(string clientName, dynamic message, string excludedConnection, string groupName = null);

            void ReceiveScripts(string contextConnectionId, string script, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients);

            void ReceiveInGroupManager(string userId, dynamic message, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups);

            void RequestScripts(HubCallerContext context,
                IHubCallerConnectionContext<dynamic> clients,
                IGroupManager groups);

            void SendMessageToServer(
                HubCallerContext context,
                IHubCallerConnectionContext<dynamic> clients,
                IGroupManager groups,
                string handler,
                object message,
                string replyTo,
                object sender,
                string messageId);
        }
    }
}