namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.SignalR.Owin;

    public class SignalX : IDisposable
    {
        internal static object padlock = new object();
        static readonly SignalXAgents SignalXAgents = new SignalXAgents();
        internal static string SIGNALXCLIENTAGENT = "SIGNALXCLIENTAGENT";
        internal static string SIGNALXCLIENTREADY = "SIGNALXCLIENTREADY";
        internal static string SIGNALXCLIENTERRORHANDLER = "SIGNALXCLIENTERRORHANDLER";
        internal static string SIGNALXCLIENTDEBUGHANDLER = "SIGNALXCLIENTDEBUGHANDLER";

        public readonly string ClientDebugSnippet = @"
                    signalx.debug(function(ev,e){ signalx.ready(function(){ signalx.server." + SIGNALXCLIENTDEBUGHANDLER + @"(JSON.stringify(e),function(MSG){  });  }); });
                 ";

        public readonly string ClientErrorSnippet = @"
                      //signalx error collector
                        ;window.onerror = function (msg, url, lineNo, columnNo, error) {
                            var string = msg.toLowerCase();
                            var substring = 'script error';
                            if (string.indexOf(substring) > -1){
		                        signalx.ready(function(){ signalx.server." + SIGNALXCLIENTERRORHANDLER + @"('Script Error: See Browser Console for Detail',function(MSG){  });  });
                            } else {
                                var message = [
                                    'Message: ' + msg,
                                    'URL: ' + url,
                                    'Line: ' + lineNo,
                                    'Column: ' + columnNo,
                                    'Error object: ' + JSON.stringify(error)
                                ].join(' - ');
                                signalx.ready(function(){ signalx.server." + SIGNALXCLIENTERRORHANDLER + @"(message,function(MSG){  });  });
                            }
                            return false;
                        };
                    signalx.error(function(ev,e){ signalx.ready(function(){ signalx.server." + SIGNALXCLIENTERRORHANDLER + @"(JSON.stringify(e),function(MSG){  });  }); });

                 ";

        public SignalXAdvanced Advanced {set;get;}

        //todo rename this to reflect what is actually happening!
        public void SetSignalXClientAsReady()
        {
            this.Advanced.Trace("Client is ready...");
            this.Settings.HasOneOrMoreConnections = true;
            this.ConnectionCount++;
        }

        public SignalXSettings Settings = new SignalXSettings();
        protected internal ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>> SignalXServers { set; get; }

        internal ConcurrentDictionary<string, ClientDetails> SignalXClientDetails { set; get; }

        internal ConcurrentDictionary<string, ServerHandlerDetails> SignalXServerExecutionDetails { set; get; }

        SignalX()
        {
            this.NullHubCallerContext = new HubCallerContext(new ServerRequest(new ConcurrentDictionary<string, object>()), Guid.NewGuid().ToString());

            Advanced = new SignalXAdvanced();
            this.Advanced.Trace("Initializing Framework SIgnalX ...");
            this.SignalXServers = new ConcurrentDictionary<string, Action<SignalXRequest, SignalXServerState>>();
            this.SignalXClientDetails = new ConcurrentDictionary<string, ClientDetails>();
            this.SignalXServerExecutionDetails = new ConcurrentDictionary<string, ServerHandlerDetails>();

            this.Connections = new ConnectionMapping<string>();
            this.OnErrorMessageReceivedFromClient = new List<Action<string, SignalXRequest>>();
            this.OnDebugMessageReceivedFromClient = new List<Action<string, SignalXRequest>>();
            this.OnClientReady = new List<Action<SignalXRequest>>();
            this.Receiver = ReceiverCreator != null ? ReceiverCreator(this) : new DefaultSignalRClientReceiver();
            this.Advanced.Trace("SIgnalX framework initialized");
        }
        [Obsolete("Not intended for client access")]
        internal ISignalXClientReceiver Receiver { set; get; }
        /// <summary>
        ///     Is used only if SignalX.ManageUserConnections has been set to true, otherwise
        ///     Connections remains empty, even if there has been connections
        /// </summary>
        public ConnectionMapping<string> Connections { internal set; get; }

        internal List<Action<string, SignalXRequest>> OnErrorMessageReceivedFromClient { set; get; }

        internal List<Action<string, SignalXRequest>> OnDebugMessageReceivedFromClient { set; get; }

        //needs more care to refactor
        internal Action<dynamic, SignalXRequest, string> OnResponseAfterScriptRuns { set; get; }

        internal List<Action<SignalXRequest>> OnClientReady { set; get; }

        static SignalX instance { set; get; }

        public ulong ConnectionCount { get; internal set; }

        internal static Func<SignalX, ISignalXClientReceiver> ReceiverCreator = null;

        public static SignalX CreateInstance(Func<SignalX, ISignalXClientReceiver> receiverCreator)
        {
            lock (padlock)
            {
                if (instance != null)
                {
                    throw new Exception("SignalX Instance has already been created. Try using SignalX.Instance to get instance");
                }

                ReceiverCreator = receiverCreator;
                return Instance;
            }
        }


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
            this.SignalXClientDetails = null;
            this.SignalXServerExecutionDetails = null;
            ReceiverCreator = null;

        }

        public void SetUpClientErrorMessageHandler(Action<string, SignalXRequest> handler)
        {
            this.Advanced.Trace($"Registering {nameof(this.SetUpClientErrorMessageHandler)}...");
            this.OnErrorMessageReceivedFromClient.Add(handler);
        }

        public void SetUpClientDebugMessageHandler(Action<string, SignalXRequest> handler)
        {
            this.Advanced.Trace($"Registering {nameof(this.SetUpClientDebugMessageHandler)}...");
            this.OnDebugMessageReceivedFromClient.Add(handler);
        }

        public HubCallerContext NullHubCallerContext;

        public void RespondToServer(
            string handler,
            dynamic message,
            object sender = null,
            string replyTo = null,
            List<string> groupList = null)
        {
            RespondToServer(this.NullHubCallerContext, null,null,handler, message, sender, replyTo, groupList);
        }

        public void RespondToServer(
            
            HubCallerContext context ,
            IHubCallerConnectionContext<dynamic> clients ,
            IGroupManager groups ,
            string handler,
            dynamic message,
            object sender=null,
            string replyTo=null,
            List<string> groupList=null)
        {
            Task.Factory.StartNew(
                () =>
                {
                    var messageId = Guid.NewGuid().ToString();
                    SendMessageToServer(context, clients, groups, handler, message, replyTo, sender, messageId, groupList, true);
                }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default); 
        }


        [Obsolete("Not intended for client use")]
        internal void SendMessageToServer(
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groups,
            string handler,
            dynamic message,
            string replyTo,
            object sender,
            string messageId,
            List<string> groupList,bool isInternalCall)
        {
            Advanced.Trace($"Sending message {message} to server from {messageId} having groups {string.Join(",", groupList ?? new List<string>())} with reply to {replyTo}");

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

                if (string.IsNullOrEmpty(handler) || !this.SignalXServers.ContainsKey(handler))
                {
                    string e = "Error request for unknown server name " + handler;
                    this.Advanced.Trace(e);
                    this.Settings.ExceptionHandler.ForEach(h => h?.Invoke(e, new Exception(e)));
                    this.RespondToUser(context?.ConnectionId, replyTo, e);
                    return;
                }

                var request = new SignalXRequest(this, replyTo, sender, messageId, message, context?.ConnectionId, handler, context?.User, groupList, context?.Request,context,clients,groups);

                if (!this.CanProcess(context, handler, request, isInternalCall))
                {
                    this.Settings.WarningHandler.ForEach(h => h?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required"));
                    return;
                }

                this.CallServer(request);
            }
            catch (Exception e)
            {
                error = "An error occured on the server while processing message " + message + " with id " +
                    messageId + " received from  " + sender + " [ user = " + user?.Identity?.Name + "] for a response to " + replyTo + " - ERROR: " +
                    e?.Message;
                this.Advanced.Trace(error, e);
                if (!string.IsNullOrEmpty(context?.ConnectionId))
                    this.RespondToUser(context?.ConnectionId, "signalx_error", error);

                if (!string.IsNullOrEmpty(replyTo) && !string.IsNullOrEmpty(context?.ConnectionId))
                    this.RespondToUser(context?.ConnectionId, replyTo, error);

                //todo possible infinite loop here. Need to fix!!!
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
            this.Advanced.Trace($"Checking if {name} can be sent a message", data);

            if (!string.IsNullOrEmpty(name))
            {
                if (this.Settings.DisabledAllClients)
                {
                    if (!this.SignalXClientDetails.ContainsKey(name) ||
                        this.SignalXClientDetails.ContainsKey(name) && this.SignalXClientDetails[name].Disabled)
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
                    if (this.SignalXClientDetails.ContainsKey(name) && this.SignalXClientDetails[name].Disabled)
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
            }
            else
            {
                //todo fix bug when name is null and still allowing message sending, especially calls origination from within server
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
            this.Advanced.Trace($"Responding to all {replyTo}...", responseData);
            RespondToAllInGroup(replyTo, responseData, null);
        }

        public void RespondToAllInGroup(string replyTo, dynamic responseData, string groupName)
        {
            this.Advanced.Trace($"Responding to all {replyTo} in group {groupName}...", responseData);
            if (!AllowToSend(replyTo, responseData))
                return;
            if (this.Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref this.Settings.OutGoingCounter);

            this.Receiver.ReceiveByGroup(replyTo, responseData, groupName);
        }
    }
}