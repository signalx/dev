namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public class SignalXRequest
    {
        readonly SignalX SignalX;

        internal SignalXRequest(
            SignalX signalX, 
            string replyTo,
            object sender, 
            string messageId, 
            dynamic message,
            string user, 
            string handler,
            IPrincipal principalUser, 
            List<string> groups, 
            IRequest request,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groupsManager)
        {
            Context = context;
            Clients = clients;
            GroupsManager = groupsManager;
            this.SignalX = signalX ?? throw new ArgumentNullException(nameof(signalX));
            this.ReplyTo = replyTo;
            this.Sender = sender;
            this.MessageId = messageId;
            this.Message = message;
            //UserId = principalUser?.Identity?.Name;
            this.User = user;
            this.Handler = handler;
            this.PrincipalUser = principalUser;
            this.Groups = groups ?? new List<string>();
            this.Request = request;
        }

        HubCallerContext Context { set; get; }
            IHubCallerConnectionContext<dynamic> Clients { set; get; }
        IGroupManager GroupsManager { set; get; }


        // public string UserId { get; set; }

        public string ReplyTo { get; }

        public List<string> Groups { get; }

        public object Sender { get; }

        public string MessageId { get; }

        public dynamic Message { get; }

        //[Obsolete("Will soon be removed in subsequent versions. Please obtain UserId from PrincipalUser instead")]
        //public string UserId { get; }
        /// <summary>
        ///     This user is actually connection id
        /// </summary>
        public string User { get; }

        public IPrincipal PrincipalUser { get; }

        public IRequest Request { get; }

        public string Handler { get; }

       

        public void RespondToAllInGroup(string replyTo, object response, string groupName)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAllInGroup(replyTo, response, groupName);
        }

        public void RespondToAll(string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAll(replyTo, response);
        }


        /// <summary>
        ///     Reply to a specific client
        /// </summary>
        /// <param name="user"></param>
        /// <param name="response"></param>
        public void RespondToUser(string user, object response)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!string.IsNullOrEmpty(this.ReplyTo))
                this.SignalX.RespondToUser(user, this.ReplyTo, response);
        }

        /// <summary>
        ///     Reply to the handler that the client has specified
        /// </summary>
        /// <param name="response"></param>
        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(this.User))
                this.SignalX.RespondToUser(this.User, this.ReplyTo, response);
        }

      
        /// <summary>
        /// Forward request To Server and wait till server completes. The original message and 'reply to' is forwarded if none is provided
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        /// <param name="replyTo"></param>
        public void Forward(string handler,dynamic message=null, string replyTo=null )
        {
            this.SignalX.SendMessageToServer(Context,Clients,GroupsManager,handler,message??Message,replyTo??ReplyTo,Sender, MessageId,Groups,true);
        }

        /// <summary>
        /// Forward request to  server asynchronously The original message and 'reply to' is forwarded if none is provided
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        /// <param name="replyTo"></param>
        public Task ForwardAsync(string handler, dynamic message = null, string replyTo = null)
        {
           return  Task.Factory.StartNew(
                () =>
                {
                    this.SignalX.SendMessageToServer(Context, Clients, GroupsManager, handler, message ?? Message, replyTo ?? ReplyTo, Sender, MessageId, Groups, true);
                }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        /// <summary>
        ///     Reply to every other clients but the sender
        /// </summary>
        /// <param name="response"></param>
        /// <param name="groupName"></param>
        public void RespondToOthers(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(this.User))
                this.SignalX.RespondToOthers(this.User, this.ReplyTo, response, groupName);
        }
    }
}