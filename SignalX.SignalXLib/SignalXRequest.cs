﻿using System;

namespace SignalXLib.Lib
{
    using System.Security.Principal;

    public class SignalXRequest
    {
        SignalX SignalX;
        public SignalXRequest(SignalX signalX,string replyTo, object sender, string messageId, dynamic message,  string connectionId, string handler, IPrincipal principalUser)
        {
            SignalX = signalX ?? throw new ArgumentNullException(nameof(signalX));
            ReplyTo = replyTo;
            Sender = sender;
            MessageId = messageId;
            Message = message;
            UserId = principalUser?.Identity?.Name;
            ConnectionId = connectionId;
            this.Handler = handler;
            this.PrincipalUser = principalUser;
        }

        public string UserId { get; set; }

        public string ReplyTo { get; }
        public object Sender { get; }
        public string MessageId { get; }
        public dynamic Message { get; }
        //[Obsolete("Will soon be removed in subsequent versions. Please obtain UserId from PrincipalUser instead")]
        //public string UserId { get; }
        public string ConnectionId { get; }
        public IPrincipal PrincipalUser { get;  }

        public string Handler { get; private set; }

        public void RespondToAll(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(ReplyTo))
                SignalX.RespondToAll(ReplyTo, response, groupName);
        }

        public void RespondToAll(string replyTo, object response, string groupName = null)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondToAll(replyTo, response, groupName);
        }

        public void RespondToUser(string userId, string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondToUser(userId, replyTo, response);
        }

        /// <summary>
        /// Reply to a specific client 
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="response"></param>
        public void RespondToUser(string connectionId, object response)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));
            if (!string.IsNullOrEmpty(ReplyTo))
                SignalX.RespondToUser(connectionId, ReplyTo, response);
        }

        /// <summary>
        /// Reply to the handler that the client has specified
        /// </summary>
        /// <param name="response"></param>
        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(UserId))
                SignalX.RespondToUser(UserId, ReplyTo, response);
        }

        /// <summary>
        /// Reply to every other clients but the sender
        /// </summary>
        /// <param name="response"></param>
        /// <param name="groupName"></param>
        public void RespondToOthers(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(ConnectionId))
                SignalX.RespondToOthers(ConnectionId, ReplyTo, response, groupName);
        }
    }
}