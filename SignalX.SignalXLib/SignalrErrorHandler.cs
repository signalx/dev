namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public partial class SignalXHub
    {
        public class SignalXHubPipelineModule : HubPipelineModule
        {
            readonly SignalX SignalX = SignalX.Instance;

            protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
            {
                this.SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke("OnIncomingError", exceptionContext?.Error));
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnIncomingError.ToString(), exceptionContext?.Error?.Message));
                base.OnIncomingError(exceptionContext, invokerContext);
            }

            protected override void OnAfterConnect(IHub hub)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnAfterConnect.ToString(), null));
                base.OnAfterConnect(hub);
            }

            public override Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildRejoiningGroups.ToString(), null));
                return base.BuildRejoiningGroups(rejoiningGroups);
            }

            public override Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildReconnect.ToString(), null));
                return base.BuildReconnect(reconnect);
            }

            public override Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildOutgoing.ToString(), null));
                return base.BuildOutgoing(send);
            }

            public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h.Invoke(ConnectionEvents.BuildIncoming.ToString(), null));
                return base.BuildIncoming(invoke);
            }

            public override Func<IHub, bool, Task> BuildDisconnect(Func<IHub, bool, Task> disconnect)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildDisconnect.ToString(), null));
                return base.BuildDisconnect(disconnect);
            }

            public override Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildConnect.ToString(), null));
                return base.BuildConnect(connect);
            }

            public override Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.BuildAuthorizeConnect.ToString(), null));
                return base.BuildAuthorizeConnect(authorizeConnect);
            }

            protected override bool OnBeforeReconnect(IHub hub)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeReconnect.ToString(), null));
                return base.OnBeforeReconnect(hub);
            }

            protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeOutgoing.ToString(), null));
                return base.OnBeforeOutgoing(context);
            }

            protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeIncoming.ToString(), null));
                return base.OnBeforeIncoming(context);
            }

            protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeDisconnect.ToString(), null));
                return base.OnBeforeDisconnect(hub, stopCalled);
            }

            protected override bool OnBeforeConnect(IHub hub)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeConnect.ToString(), null));
                return base.OnBeforeConnect(hub);
            }

            protected override bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnBeforeAuthorizeConnect.ToString(), null));
                return base.OnBeforeAuthorizeConnect(hubDescriptor, request);
            }

            protected override void OnAfterReconnect(IHub hub)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnAfterReconnect.ToString(), null));
                base.OnAfterReconnect(hub);
            }

            protected override void OnAfterOutgoing(IHubOutgoingInvokerContext context)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnAfterOutgoing.ToString(), null));
                base.OnAfterOutgoing(context);
            }

            protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnAfterIncoming.ToString(), null));
                return base.OnAfterIncoming(result, context);
            }

            protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
            {
                this.SignalX.Settings.ConnectionEventsHandler.ForEach(h => h?.Invoke(ConnectionEvents.OnAfterDisconnect.ToString(), null));
                base.OnAfterDisconnect(hub, stopCalled);
            }
        }
    }
}