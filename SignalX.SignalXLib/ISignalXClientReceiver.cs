namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR.Hubs;

    public partial class SignalX
    {
        public interface ISignalXClientReceiver
        {
            void Receive(string userId, string clientName, dynamic message);
            void Receive(string clientName, dynamic message);
            void ReceiveAsOther(string clientName, dynamic message, string excludedConnection);

            void ReceiveScripts(string script, IHubCallerConnectionContext<dynamic> clients);

            void SendMessageToServer(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, string handler, object message, string replyTo, object sender, string messageId);
        }
    }
}