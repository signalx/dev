namespace SignalXLib.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public class SignalXRequest
    {
        readonly SignalX SignalX;

        internal SignalXRequest(
            string correlationId,
            SignalX signalX,
            string replyTo,
            object sender,
            string messageId,
            string message,
            string user,
            string handler,
            IPrincipal principalUser,
            List<string> groups,
            IRequest request,
            HubCallerContext context,
            IHubCallerConnectionContext<dynamic> clients,
            IGroupManager groupsManager,
            object messageObject)
        {
            this.CorrelationId = correlationId;
            this.Context = context;
            this.Clients = clients;
            this.GroupsManager = groupsManager;
            this.SignalX = signalX ?? throw new ArgumentNullException(nameof(signalX));
            this.ReplyTo = replyTo;
            this.Sender = sender;
            this.MessageId = messageId;
            this.MessageAsJson = message;
            //UserId = principalUser?.Identity?.Name;
            this.User = user;
            this.Handler = handler;
            this.PrincipalUser = principalUser;
            this.Groups = groups ?? new List<string>();
            this.Request = request;
            this.MessageObject = messageObject;
        }

        public string CorrelationId { get; }

        HubCallerContext Context { get; }

        IHubCallerConnectionContext<dynamic> Clients { get; }

        IGroupManager GroupsManager { get; }

        public string ReplyTo { get; }

        public List<string> Groups { get; }

        public object Sender { get; }

        public string MessageId { get; }

        string MessageAsJson { get; }

        public string MessageAsJsonString => this.MessageObject == null ? this.MessageAsJson : this.SignalX.Serializer.SerializeObject(this.MessageObject, this.CorrelationId);

        object MessageObject { get; }

        public string User { get; }

        public IPrincipal PrincipalUser { get; }

        public IRequest Request { get; }

        public string Handler { get; }

        public void Receive<T>(Action<T> operation)
        {
            var message = this.MessageAs<T>();
            if (message != null)
                operation(message);
        }

        public T DeserializeObject<T>(string data)
        {
            return this.SignalX.Serializer.DeserializeObject<T>(data, this.CorrelationId);
        }

        public string SerializeObject(object data)
        {
            return this.SignalX.Serializer.SerializeObject(data, this.CorrelationId);
        }

        public T MessageAs<T>(string message = null)
        {
            try
            {
                if (this.MessageObject != null)
                {
                    this.SignalX.Advanced.Trace(this.CorrelationId, $"Message object present , so casting that o type {typeof(T).FullName}...");
                    return (T)this.MessageObject;
                }

                this.SignalX.Advanced.Trace(this.CorrelationId, $"Deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");

                if (string.IsNullOrEmpty(message ?? this.MessageAsJsonString))
                    return default(T);
                return this.SignalX.Serializer.DeserializeObject<T>(message ?? this.MessageAsJsonString, this.CorrelationId);
            }
            catch (Exception e)
            {
                this.SignalX.Advanced.Trace(this.CorrelationId, e, $"Error deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");
                this.SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke("MessageSerializationError", e));

                return default(T);
            }
        }

        public void RespondToAllInGroup(string replyTo, object response, string groupName)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAllInGroup(replyTo, response, groupName, this.CorrelationId);
        }

        public void RespondToAll(string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAll(replyTo, response, this.CorrelationId);
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
                this.SignalX.RespondToUser(user, this.ReplyTo, response, this.CorrelationId);
        }

        /// <summary>
        ///     Reply to the handler that the client has specified
        /// </summary>
        /// <param name="response"></param>
        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(this.User))
                this.SignalX.RespondToUser(this.User, this.ReplyTo, response, this.CorrelationId);
        }

        /// <summary>
        ///     Forward request to  another server asynchronously The original message and 'reply to' is forwarded if none is
        ///     provided
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        /// <param name="replyTo"></param>
        public async Task ForwardAsync(string handler, string message = null, string replyTo = null)
        {
            Task task = this.SignalX.SendMessageToServer(this.CorrelationId, this.Context, this.Clients, this.GroupsManager, handler, message ?? this.MessageAsJsonString, replyTo ?? this.ReplyTo, this.Sender, this.MessageId, this.Groups, true, message == null ? this.MessageObject : null);
            await task.ConfigureAwait(false);
        }

        /// <summary>
        ///     Reply to every other clients but the sender
        /// </summary>
        /// <param name="response"></param>
        /// <param name="groupName"></param>
        public void RespondToOthers(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(this.User))
                this.SignalX.RespondToOthers(this.User, this.ReplyTo, response, groupName, this.CorrelationId);
        }

        public void RespondToServer(
            string handler,
            object message,
            object sender = null,
            string replyTo = null,
            List<string> groupList = null)
        {
            this.SignalX.RespondToServer(
                handler,
                message,
                sender,
                replyTo,
                groupList,
                this.CorrelationId);
        }
    }
}