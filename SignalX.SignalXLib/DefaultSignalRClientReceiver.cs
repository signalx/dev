namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.SignalR.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class DefaultSignalRClientReceiver : ISignalXClientReceiver
    {
        // SignalX SignalX= Lib.SignalX.Instance;

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="user"></param>
        /// <param name="replyTo"></param>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        public void Receive(
            string user,
            string replyTo,
            object message,
            string correlationId)
        {
            IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            hubContext.Clients.Client(user).broadcastMessage(replyTo, message);
        }

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="clientName"></param>
        /// <param name="message"></param>
        /// <param name="groupName"></param>
        public void ReceiveByGroup(string correlationId, string clientName, object message, string groupName = null)
        {
            IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();
            if (!string.IsNullOrEmpty(groupName))
                hubContext.Clients.Group(groupName).broadcastMessage(clientName, message);
            else
                hubContext.Clients.All.broadcastMessage(clientName, message);
        }

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="clientName"></param>
        /// <param name="message"></param>
        /// <param name="excludedConnection"></param>
        /// <param name="groupName"></param>
        public void ReceiveAsOther(string correlationId, string clientName, object message, string excludedConnection, string groupName = null)
        {
            IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();
            if (!string.IsNullOrEmpty(groupName))
                hubContext.Clients.Group(groupName, excludedConnection).broadcastMessage(clientName, message);
            else
                hubContext.Clients.AllExcept(excludedConnection).broadcastMessage(clientName, message);
        }

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="contextConnectionId"></param>
        /// <param name="methods"></param>
        /// <param name="context"></param>
        /// <param name="groups"></param>
        /// <param name="clients"></param>
        public void ReceiveScripts(string correlationId, string contextConnectionId, string methods, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients)
        {
            clients.Caller?.addMessage(methods);
        }

        /// <summary>
        ///     Notifies client using its callback that the group has been added successfully
        ///     signalx.groups.join(name, callback : function(groupName){ })
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="operation"></param>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        /// <param name="context"></param>
        /// <param name="clients"></param>
        /// <param name="groups"></param>
        public void ReceiveInGroupManager(string correlationId, string operation, string userId, string groupName, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            clients.Caller?.groupManager(groupName, operation);
        }

        public async Task RequestScripts(SignalX SignalX, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string correlationId)
        {
            correlationId = correlationId ?? Guid.NewGuid().ToString();
            await SignalX.RespondToScriptRequest(correlationId, context, clients, groups).ConfigureAwait(false);
        }

        /// <summary>
        ///     Call this to send message to server
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="SignalX"></param>
        /// <param name="context"></param>
        /// <param name="clients"></param>
        /// <param name="groups"></param>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        /// <param name="replyTo"></param>
        /// <param name="sender"></param>
        /// <param name="messageId"></param>
        /// <param name="groupList"></param>
        public async Task SendMessageToServer(string correlationId, SignalX SignalX, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, object message, string replyTo, object sender, string messageId, List<string> groupList)
        {
            await SignalX.SendMessageToServer(correlationId, context, clients, groups, handler, message?.ToString(), replyTo, sender, messageId, groupList, false, message);
        }
    }
}