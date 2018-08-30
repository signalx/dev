namespace SignalXLib.Lib
{
    using System;
    using System.Security.Principal;
    using System.Threading;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public class SignalX : IDisposable
    {
        internal static object padlock = new object();
        static readonly SignalXAgents SignalXAgents = new SignalXAgents();
        internal static string SIGNALXCLIENTAGENT = "SIGNALXCLIENTAGENT"; 
        internal static string SIGNALXCLIENTERRORHANDLER = "SIGNALXCLIENTERRORHANDLER"; 
        internal static string SIGNALXCLIENTDEBUGHANDLER = "SIGNALXCLIENTDEBUGHANDLER";
        public readonly string ClientDebugSnippet = @"                   
                    signalx.debug(function(e){ signalx.ready(function(){ signalx.server." + SignalX.SIGNALXCLIENTDEBUGHANDLER + @"(JSON.stringify(e),function(MSG){  });  }); });
                 ";

        public readonly string ClientErrorSnippet = @"
                    signalx.error(function(e){ signalx.ready(function(){ signalx.server." + SignalX.SIGNALXCLIENTERRORHANDLER + @"(JSON.stringify(e),function(MSG){  });  }); });
                    
                 ";

        internal Action<string, SignalXRequest> OnErrorMessageReceivedFromClient { set; get; }

        public void SetUpClientErrorMessageHandler(Action<string, SignalXRequest> handler)
        {
            OnErrorMessageReceivedFromClient = handler;
        }


        internal Action<string, SignalXRequest> OnDebugMessageReceivedFromClient { set; get; }

        public void SetUpClientDebugMessageHandler(Action<string, SignalXRequest> handler)
        {
            OnDebugMessageReceivedFromClient = handler;
        }
        public SignalXSettings Settings = new SignalXSettings();

        SignalX()
        {
        }

        static SignalX instance { set; get; }

        public ulong ConnectionCount { get; internal set; }

        public static SignalX Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                lock (padlock)
                {
                    if (instance != null)
                        return instance;
                    instance = new SignalX();
                    SignalXAgents.SetUpAgents(instance);
                }

                return instance;
            }
        }

        /// <summary>
        ///     Powerful when this is true, you will be able to change behaviour of a server by name at run time
        /// </summary>
        public bool AllowDynamicServer
        {
            set => this.Settings.AllowDynamicServerInternal = value;
        }

        public void Dispose()
        {
            instance = null;
            this.Settings.Dispose();
        }

        [Obsolete("Not intended for client use")]
        public void SendMessageToServer(
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            dynamic message,
            string replyTo,
            object sender,
            string messageId)
        {
            IPrincipal user = context?.User;
            string error = "";
            try
            {
                if (this.Settings.StartCountingInComingMessages)
                    Interlocked.Increment(ref this.Settings.InComingCounter);

                this.Settings.ConnectionEventsHandler.ForEach(
                    h => h?.Invoke(
                        ConnectionEvents.SignalXIncomingRequest.ToString(),
                        new
                        {
                            handler = handler,
                            message = message,
                            replyTo = replyTo,
                            sender = sender,
                            messageId = messageId
                        }));

                if (string.IsNullOrEmpty(handler) || !this.Settings.SignalXServers.ContainsKey(handler))
                {
                    string e = "Error request for unknown server name " + handler;
                    this.Settings.ExceptionHandler.ForEach(h => h?.Invoke(e, new Exception(e)));
                    this.RespondToUser(context?.ConnectionId, replyTo, e);
                    return;
                }

                if (!this.CanProcess(context, handler))
                {
                    this.Settings.WarningHandler.ForEach(h => h?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required"));
                    return;
                }

                var request = new SignalXRequest(this, replyTo, sender, messageId, message, context?.ConnectionId, handler, context?.User);

                this.CallServer(request);
            }
            catch (Exception e)
            {
                error = "An error occured on the server while processing message " + message + " with id " +
                    messageId + " received from  " + sender + " [ user = " + user.Identity.Name + "] for a response to " + replyTo + " - ERROR: " +
                    e.Message;
                this.RespondToUser(context.ConnectionId, "signalx_error", error);
                if (!string.IsNullOrEmpty(replyTo))
                    this.RespondToUser(context.ConnectionId, replyTo, error);
                this.Settings.ExceptionHandler.ForEach(h => h?.Invoke(error, e));
            }

            this.Settings.ConnectionEventsHandler.ForEach(
                h => h?.Invoke(
                    ConnectionEvents.SignalXIncomingRequestCompleted.ToString(),
                    new
                    {
                        handler = handler,
                        message = message,
                        replyTo = replyTo,
                        sender = sender,
                        messageId = messageId,
                        error = error
                    }));
        }

        internal bool AllowToSend(string name, dynamic data)
        {
            if (this.Settings.DisabledAllClients)
            {
                if (!this.Settings.SignalXClientDetails.ContainsKey(name) ||
                    this.Settings.SignalXClientDetails.ContainsKey(name) && this.Settings.SignalXClientDetails[name].Disabled)
                {
                    this.Settings.WarningHandler.ForEach(
                        h => h?.Invoke(
                            "DataSendingNotActivated",
                            new
                            {
                                Name = name,
                                Data = data,
                                Issue = $"Sending message to client {name} has been disabled. DisabledAllClients is activated"
                            }));
                    return false;
                }
            }
            else
            {
                if (this.Settings.SignalXClientDetails.ContainsKey(name) && this.Settings.SignalXClientDetails[name].Disabled)
                {
                    this.Settings.WarningHandler.ForEach(
                        h => h?.Invoke(
                            "DataSendingNotActivated",
                            new
                            {
                                Name = name,
                                Data = data,
                                Issue = $"Sending message to client {name} has been disabled"
                            }));
                    return false;
                }
            }

            if (!this.Settings.HasOneOrMoreConnections)
            {
                this.Settings.WarningHandler.ForEach(
                    h => h?.Invoke(
                        "DataSendingNotActivated",
                        new
                        {
                            UserId = "",
                            Name = name,
                            Data = data,
                            Issue = "Cannot send message to client since no client connection has been obtained"
                        }));
                return false;
            }

            return true;
        }
        public void RespondToAll(string replyTo, dynamic responseData)
        {
            RespondToAllInGroup(replyTo, responseData, null);
        }


        public void RespondToAllInGroup(string replyTo, dynamic responseData, string groupName )
        {
            if (!AllowToSend(replyTo, responseData))
                return;
            if (this.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref this.Settings.OutGoingCounter);

            this.Settings.Receiver.ReceiveByGroup(replyTo, responseData, groupName);
        }
    }
}