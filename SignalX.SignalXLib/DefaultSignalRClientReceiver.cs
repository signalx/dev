using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR.Hubs;

    public partial class SignalX
    {
        internal class DefaultSignalRClientReceiver : ISignalXClientReceiver
        {
            /// <summary>
            /// Called by framework
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="clientName"></param>
            /// <param name="message"></param>
            public void Receive(string userId, string clientName, dynamic message)
            {
                IHubContext hubContext=  GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();
                hubContext.Clients.User(userId).broadcastMessage(clientName, message);
            }
            /// <summary>
            /// Called by framework
            /// </summary>
            /// <param name="clientName"></param>
            /// <param name="message"></param>
            public void Receive(string clientName, dynamic message)
            {
                IHubContext hubContext= GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();
              hubContext.Clients.All.broadcastMessage(clientName, message);
            }
            /// <summary>
            /// Called by framework
            /// </summary>
            /// <param name="clientName"></param>
            /// <param name="message"></param>
            /// <param name="excludedConnection"></param>
            public void ReceiveAsOther(string clientName, dynamic message, string excludedConnection)
            {
                IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();
                hubContext.Clients.AllExcept(excludedConnection).broadcastMessage(clientName, message);
            }
            /// <summary>
            /// Called by framework
            /// </summary>
            /// <param name="methods"></param>
            /// <param name="clients"></param>
            public void ReceiveScripts(string methods, IHubCallerConnectionContext<dynamic> clients)
            {
               clients?.Caller?.addMessage(methods);
            }

            /// <summary>
            /// Call this to send message to server
            /// </summary>
            /// <param name="context"></param>
            /// <param name="clients"></param>
            /// <param name="handler"></param>
            /// <param name="message"></param>
            /// <param name="replyTo"></param>
            /// <param name="sender"></param>
            /// <param name="messageId"></param>
            public void SendMessageToServer(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, string handler, object message, string replyTo, object sender, string messageId)
            {
                SignalX.Send(context, clients, handler, message, replyTo, sender, messageId);
            }
        }
    }
}