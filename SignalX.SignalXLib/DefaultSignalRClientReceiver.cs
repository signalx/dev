﻿using Microsoft.AspNet.SignalR;
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
                IHubContext hubContext = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<SignalXHub>();

                hubContext.Clients.User(userId).broadcastMessage(clientName, message);
            }

            /// <summary>
            /// Called by framework
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
            /// Called by framework
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
            /// Called by framework
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

            public void ReceiveInGroupManager(string userId, dynamic message, HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
            {
                clients.Caller?.groupManager(message);
            }

            public void RequestScripts(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups)
            {
                SignalX.RespondToScriptRequest(context, clients, groups);
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
            public void SendMessageToServer(HubCallerContext context, IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, dynamic message, string replyTo, object sender, string messageId)
            {
                SignalX.SendMessageToServer(context, clients, groups, handler, message, replyTo, sender, messageId);
            }
        }
    }
}