using System;

namespace SignalXLib.Lib
{
    using System.Threading;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

 
    public  class SignalX : IDisposable
    {
        private SignalX()
        {
           
        }
        static SignalX instance { set; get; }
        public static object padlock=new object();
        
        
        public static SignalX Instance()
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
 
        public SignalXSettings Settings = new SignalXSettings();
        
        static SignalXAgents SignalXAgents = new SignalXAgents();




        public void Dispose()
        {
            instance = null;
            Settings.Dispose();
        }

        

        /// <summary>
        /// Powerful when this is true, you will be able to redefine a server by name at run time
        /// </summary>
        public  bool AllowDynamicServer
        {
            set { Settings. AllowDynamicServerInternal = value; }
        }
        internal static string SIGNALXCLIENTAGENT = "SIGNALXCLIENTAGENT";//+Guid.NewGuid().ToString().Replace("-","");

        public void SendMessageToServer( HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients, IGroupManager groups, string handler, dynamic message, string replyTo, object sender, string messageId)
        {
            var user = context?.User;
            string error = "";
            try
            {
                if (Settings.StartCountingInComingMessages)
                    Interlocked.Increment(ref Settings.InComingCounter);

                Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXIncomingRequest.ToString(), new
                {
                    handler = handler,
                    message = message,
                    replyTo = replyTo,
                    sender = sender,
                    messageId = messageId
                });

                if (string.IsNullOrEmpty(handler) || !Settings.SignalXServers.ContainsKey(handler))
                {
                    var e = "Error request for unknown server name " + handler;
                    Settings.ExceptionHandler?.Invoke(e, new Exception(e));
                    this.RespondToUser(context?.ConnectionId, replyTo, e);
                    return;
                }

                if (!this.CanProcess(context, handler))
                {
                    Settings.WarningHandler?.Invoke("RequireAuthentication", "User attempting to connect has not been authenticated when authentication is required");
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
                {
                    this.RespondToUser(context.ConnectionId, replyTo, error);
                }
                this.Settings.ExceptionHandler?.Invoke(error, e);
            }
            Settings.ConnectionEventsHandler?.Invoke(ConnectionEvents.SignalXIncomingRequestCompleted.ToString(), new
            {
                handler = handler,
                message = message,
                replyTo = replyTo,
                sender = sender,
                messageId = messageId,
                error = error
            });
        }

        internal  bool AllowToSend( string name, dynamic data)
        {
            if (Settings.DisabledAllClients)
            {
                if (!Settings.SignalXClientDetails.ContainsKey(name) ||
                    (Settings.SignalXClientDetails.ContainsKey(name) && Settings.SignalXClientDetails[name].Disabled))
                {
                    Settings.WarningHandler?.Invoke("DataSendingNotActivated",
                        new
                        {
                            Name = name,
                            Data = data,
                            Issue = $"Sending message to client {name} has been disabled. DisabledAllClients is activated"
                        });
                    return false;
                }
            }
            else
            {
                if (Settings.SignalXClientDetails.ContainsKey(name) && Settings.SignalXClientDetails[name].Disabled)
                {
                    Settings.WarningHandler?.Invoke("DataSendingNotActivated",
                        new
                        {
                            Name = name,
                            Data = data,
                            Issue = $"Sending message to client {name} has been disabled"
                        });
                    return false;
                }
            }

            if (!Settings.HasOneOrMoreConnections)
            {
                Settings.WarningHandler?.Invoke("DataSendingNotActivated",
                    new
                    {
                        UserId = "",
                        Name = name,
                        Data = data,
                        Issue = "Cannot send message to client since no client connection has been obtained"
                    });
                return false;
            }
            return true;
        }

        public  void RespondToAll( string name, dynamic data, string groupName = null)
        {
            if (!AllowToSend(name, data))
            {
                return;
            }
            if (Settings.StartCountingOutGoingMessages)
                Interlocked.Increment(ref Settings.OutGoingCounter);

          this.Settings.  Receiver.ReceiveByGroup(name, data, groupName);
        }
    }
}