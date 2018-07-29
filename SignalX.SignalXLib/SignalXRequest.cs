using System;

namespace SignalXLib.Lib
{
    public class SignalXRequest
    {
        public SignalXRequest(string replyTo, object sender, string messageId, object message, string userId, string connectionId, string handler)
        {
            ReplyTo = replyTo;
            Sender = sender;
            MessageId = messageId;
            Message = message;
            UserId = userId;
            ConnectionId = connectionId;
            this.Handler = handler;
        }

        public string ReplyTo { get; }
        public object Sender { get; }
        public string MessageId { get; }
        public object Message { get; }
        public string UserId { get; }
        public string ConnectionId { get; }

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

        public void RespondToUser(string connectionId, object response)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));
            if (!string.IsNullOrEmpty(ReplyTo))
                SignalX.RespondToUser(connectionId, ReplyTo, response);
        }

        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(ConnectionId))
                SignalX.RespondToUser(ConnectionId, ReplyTo, response);
        }

        public void RespondToOthers(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(ConnectionId))
                SignalX.RespondToOthers(ConnectionId, ReplyTo, response, groupName);
        }
    }
}