using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalXLib.Lib
{
    using System.Collections.Generic;
    using Microsoft.AspNet.SignalR.Hubs;

    public class SignalXHub : Hub
    {
        public void Send(string handler, object message, string replyTo, object sender, string messageId)
        {
            try
            {
                SignalX._signalXServers[handler].Invoke(message, sender, replyTo, messageId, Context?.User?.Identity?.Name, Context?.ConnectionId);
            }
            catch (Exception e)
            {
                var error = "An error occured on the server while processing message " + message + " with id " +
                            messageId + " received from " + sender + " for a response to " + replyTo + " - ERROR: " +
                            e.Message;
                SignalX.RespondToAll("signalx_error", error);
                if (!string.IsNullOrEmpty(replyTo))
                {
                    SignalX.RespondToAll(replyTo, error);
                }
                SignalX.ExceptionHandler?.Invoke(new Exception(error, e));
            }
          
        }

        public void GetMethods()
        {
            var methods = SignalX._signalXServers.Aggregate("var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred(); window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})}; window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  chat.server.send('" + signalXServer.Key + "',m,rt,sen,messageId); if(repTo){return messageId}else{ return deferred.promise();}   },")) +"}; $sx; ";
            Clients.All.addMessage(methods);
        }

        public override Task OnConnected()
        {
            SignalX.ConnectionEventsHandler?.Invoke("OnConnected", null);
            string name = Context?.User?.Identity?.Name;

            if(name!=null)
            SignalX.Connections?.Add(name, Context?.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            SignalX.ConnectionEventsHandler?.Invoke("OnDisconnected", null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Connections?.Remove(name, Context?.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            SignalX.ConnectionEventsHandler?.Invoke("OnReconnected", null);
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                if (SignalX.Connections !=null && !SignalX.Connections.GetConnections(name).Contains(Context?.ConnectionId))
            {
                SignalX.Connections?.Add(name, Context?.ConnectionId);
            }

            return base.OnReconnected();
        }


        public class SignalrErrorHandler : HubPipelineModule
        {
            protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
            {
                SignalX.ExceptionHandler?.Invoke(exceptionContext?.Error);
                base.OnIncomingError(exceptionContext, invokerContext);
            }

            protected override void OnAfterConnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnAfterConnect", null);
                base.OnAfterConnect(hub);
            }
            
            public override Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildRejoiningGroups", null);
                return base.BuildRejoiningGroups(rejoiningGroups);
            }

            public override Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildReconnect", null);
                return base.BuildReconnect(reconnect);
            }

            public override Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildOutgoing", null);
                return base.BuildOutgoing(send);
            }

            public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildIncoming", null);
                return base.BuildIncoming(invoke);
            }

            public override Func<IHub, bool, Task> BuildDisconnect(Func<IHub, bool, Task> disconnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildDisconnect", null);
                return base.BuildDisconnect(disconnect);
            }

            public override Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildConnect", null);
                return base.BuildConnect(connect);
            }

            public override Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke("BuildAuthorizeConnect", null);
                return base.BuildAuthorizeConnect(authorizeConnect);
            }

            protected override bool OnBeforeReconnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeReconnect", null);
                return base.OnBeforeReconnect(hub);
            }

            protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeOutgoing", null);
                return base.OnBeforeOutgoing(context);
            }

            protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeIncoming", null);
                return base.OnBeforeIncoming(context);
            }

            protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeDisconnect", null);
                return base.OnBeforeDisconnect(hub, stopCalled);
            }

            protected override bool OnBeforeConnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeConnect", null);
                return base.OnBeforeConnect(hub);
            }

            protected override bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnBeforeAuthorizeConnect", null);
                return base.OnBeforeAuthorizeConnect(hubDescriptor, request);
            }

            protected override void OnAfterReconnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnAfterReconnect", null);
                base.OnAfterReconnect(hub);
            }

            protected override void OnAfterOutgoing(IHubOutgoingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnAfterOutgoing", null);
                base.OnAfterOutgoing(context); 
            }

            protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnAfterIncoming", null);
                return base.OnAfterIncoming(result, context);
            }

            protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
            {
                SignalX.ConnectionEventsHandler?.Invoke("OnAfterDisconnect", null);
                base.OnAfterDisconnect(hub, stopCalled);
            }
        }
    }


}