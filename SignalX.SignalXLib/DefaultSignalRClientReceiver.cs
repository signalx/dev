namespace SignalXLib.Lib
{
    using System.Collections.Generic;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.SignalR.Infrastructure;

    class DefaultSignalRClientReceiver : ISignalXClientReceiver
    {
        // SignalX SignalX= Lib.SignalX.Instance;

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="user"></param>
        /// <param name="replyTo"></param>
        /// <param name="message"></param>
        public void Receive(
            string user,
            string replyTo,
            dynamic message
            //, string groupName = null
        )
        {
            //if (!string.IsNullOrEmpty(groupName))
            //{
            //    IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

            //    hubContext.Clients.Group(groupName).broadcastMessage(replyTo, message);

            //}
            //else
            {
                IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

                hubContext.Clients.Client(user).broadcastMessage(replyTo, message);
            }
        }

        /// <summary>
        ///     Called by framework
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="message"></param>
        /// <param name="groupName"></param>
        public void ReceiveByGroup(string clientName, dynamic message, string groupName)
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
        /// <param name="clientName"></param>
        /// <param name="message"></param>
        /// <param name="excludedConnection"></param>
        public void ReceiveAsOther(string clientName, dynamic message, string excludedConnection, string groupName)
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
        /// <param name="contextConnectionId"></param>
        /// <param name="methods"></param>
        /// <param name="context"></param>
        /// <param name="groups"></param>
        /// <param name="clients"></param>
        public void ReceiveScripts(string contextConnectionId, string methods, HubCallerContext context, IGroupManager groups, IHubCallerConnectionContext<dynamic> clients)
        {
            clients.Caller?.addMessage(methods);
        }

        /// <summary>
        /// Notifies client using its callback that the group has been added successfully
        /// signalx.groups.join(name, callback : function(groupName){ })
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        /// <param name="context"></param>
        /// <param name="clients"></param>
        /// <param name="groups"></param>
        public void ReceiveInGroupManager(string operation,string userId, dynamic groupName, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            clients.Caller?.groupManager(groupName, operation);
        }

        public void RequestScripts(SignalX SignalX, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
        {
            SignalX.RespondToScriptRequest(context, clients, groups);
        }

        /// <summary>
        ///     Call this to send message to server
        /// </summary>
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
        public void SendMessageToServer(SignalX SignalX, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, dynamic message, string replyTo, object sender, string messageId, List<string> groupList)
        {
            SignalX.SendMessageToServer(context, clients, groups, handler, message, replyTo, sender, messageId, groupList);
        }
    }
}