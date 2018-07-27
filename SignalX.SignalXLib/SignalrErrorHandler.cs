using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR.Hubs;
    using System.Collections.Generic;

    public partial class SignalXHub
    {
        public class SignalXHubPipelineModule : HubPipelineModule
        {
            protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
            {
                SignalX.ExceptionHandler?.Invoke("OnIncomingError", exceptionContext?.Error);
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnIncomingError.ToString(), exceptionContext?.Error?.Message);
                base.OnIncomingError(exceptionContext, invokerContext);
            }

            protected override void OnAfterConnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnAfterConnect.ToString(), null);
                base.OnAfterConnect(hub);
            }

            public override Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildRejoiningGroups.ToString(), null);
                return base.BuildRejoiningGroups(rejoiningGroups);
            }

            public override Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildReconnect.ToString(), null);
                return base.BuildReconnect(reconnect);
            }

            public override Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildOutgoing.ToString(), null);
                return base.BuildOutgoing(send);
            }

            public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildIncoming.ToString(), null);
                return base.BuildIncoming(invoke);
            }

            public override Func<IHub, bool, Task> BuildDisconnect(Func<IHub, bool, Task> disconnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildDisconnect.ToString(), null);
                return base.BuildDisconnect(disconnect);
            }

            public override Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildConnect.ToString(), null);
                return base.BuildConnect(connect);
            }

            public override Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.BuildAuthorizeConnect.ToString(), null);
                return base.BuildAuthorizeConnect(authorizeConnect);
            }

            protected override bool OnBeforeReconnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeReconnect.ToString(), null);
                return base.OnBeforeReconnect(hub);
            }

            protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeOutgoing.ToString(), null);
                return base.OnBeforeOutgoing(context);
            }

            protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeIncoming.ToString(), null);
                return base.OnBeforeIncoming(context);
            }

            protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeDisconnect.ToString(), null);
                return base.OnBeforeDisconnect(hub, stopCalled);
            }

            protected override bool OnBeforeConnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeConnect.ToString(), null);
                return base.OnBeforeConnect(hub);
            }

            protected override bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnBeforeAuthorizeConnect.ToString(), null);
                return base.OnBeforeAuthorizeConnect(hubDescriptor, request);
            }

            protected override void OnAfterReconnect(IHub hub)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnAfterReconnect.ToString(), null);
                base.OnAfterReconnect(hub);
            }

            protected override void OnAfterOutgoing(IHubOutgoingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnAfterOutgoing.ToString(), null);
                base.OnAfterOutgoing(context);
            }

            protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnAfterIncoming.ToString(), null);
                return base.OnAfterIncoming(result, context);
            }

            protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
            {
                SignalX.ConnectionEventsHandler?.Invoke(ConnectionEvents.OnAfterDisconnect.ToString(), null);
                base.OnAfterDisconnect(hub, stopCalled);
            }
        }
    }
}