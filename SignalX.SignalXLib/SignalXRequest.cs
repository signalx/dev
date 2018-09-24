namespace SignalXLib.Lib
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    public class SignalXRequest
    {
        private readonly SignalX SignalX;

        public string CorrelationId { get; }

        internal SignalXRequest(string correlationId,
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
            IGroupManager groupsManager, object messageObject)
        {
            CorrelationId = correlationId;
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
            MessageObject = messageObject;
        }

        public void Receive<T>(Action<T> operation)
        {
            var message = MessageAs<T>();
            if (message != null) { operation(message); }
        }

        public T DeserializeObject<T>(string data)
        {
            return this.SignalX.Serializer.DeserializeObject<T>(data, CorrelationId);
        }

        public string SerializeObject(object data)
        {
            return this.SignalX.Serializer.SerializeObject(data, CorrelationId);
        }

        public T MessageAs<T>(string message = null)
        {
            try
            {
                if (MessageObject != null)
                {
                    SignalX.Advanced.Trace(CorrelationId, $"Message object present , so casting that o type {typeof(T).FullName}...");
                    return (T)MessageObject;
                }

                SignalX.Advanced.Trace(CorrelationId, $"Deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");

                if (string.IsNullOrEmpty(message ?? this.MessageAsJsonString))
                {
                    return default(T);
                }
                return SignalX.Serializer.DeserializeObject<T>(message ?? this.MessageAsJsonString, CorrelationId);
            }
            catch (Exception e)
            {
                SignalX.Advanced.Trace(CorrelationId, e, $"Error deserializing message string {message ?? this.MessageAsJsonString} to type {typeof(T).FullName}...");
                this.SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke("MessageSerializationError", e));

                return default(T);
            }
        }

        private HubCallerContext Context { get; }

        private IHubCallerConnectionContext<dynamic> Clients { get; }

        private IGroupManager GroupsManager { get; }

        public string ReplyTo { get; }

        public List<string> Groups { get; }

        public object Sender { get; }

        public string MessageId { get; }

        private string MessageAsJson { get; }

        public string MessageAsJsonString => this.MessageObject == null ? this.MessageAsJson : SignalX.Serializer.SerializeObject(this.MessageObject, CorrelationId);

        private object MessageObject { get; }

        public string User { get; }

        public IPrincipal PrincipalUser { get; }

        public IRequest Request { get; }

        public string Handler { get; }

        public void RespondToAllInGroup(string replyTo, object response, string groupName)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAllInGroup(replyTo, response, groupName, CorrelationId);
        }

        public void RespondToAll(string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            this.SignalX.RespondToAll(replyTo, response, CorrelationId);
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
                this.SignalX.RespondToUser(user, this.ReplyTo, response, CorrelationId);
        }

        /// <summary>
        ///     Reply to the handler that the client has specified
        /// </summary>
        /// <param name="response"></param>
        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(this.User))
                this.SignalX.RespondToUser(this.User, this.ReplyTo, response, CorrelationId);
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
            Task task = this.SignalX.SendMessageToServer(CorrelationId, this.Context, this.Clients, this.GroupsManager, handler, message ?? this.MessageAsJsonString, replyTo ?? this.ReplyTo, this.Sender, this.MessageId, this.Groups, true, message == null ? this.MessageObject : null);
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
                this.SignalX.RespondToOthers(this.User, this.ReplyTo, response, groupName, CorrelationId);
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
                groupList, CorrelationId);
        }
    }
}