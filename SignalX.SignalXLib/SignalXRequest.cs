using System;

namespace SignalXLib.Lib
{
    using System.Security.Principal;

    public class SignalXRequest
    {
        SignalX SignalX;
        public SignalXRequest(SignalX signalX,string replyTo, object sender, string messageId, dynamic message,  string user, string handler, IPrincipal principalUser)
        {
            SignalX = signalX ?? throw new ArgumentNullException(nameof(signalX));
            ReplyTo = replyTo;
            Sender = sender;
            MessageId = messageId;
            Message = message;
            //UserId = principalUser?.Identity?.Name;
            this.User = user;
            this.Handler = handler;
            this.PrincipalUser = principalUser;
        }

       // public string UserId { get; set; }

        public string ReplyTo { get; }
        public object Sender { get; }
        public string MessageId { get; }
        public dynamic Message { get; }
        //[Obsolete("Will soon be removed in subsequent versions. Please obtain UserId from PrincipalUser instead")]
        //public string UserId { get; }
        /// <summary>
        /// This user is actually connection id
        /// </summary>
        public string User { get; }
        public IPrincipal PrincipalUser { get;  }

        public string Handler { get; private set; }

        //public void RespondToAll(object response, string groupName = null)
        //{
        //    if (!string.IsNullOrEmpty(ReplyTo))
        //        SignalX.RespondToAll(ReplyTo, response, groupName);
        //}

        public void RespondToAllInGroup(string replyTo, object response, string groupName )
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondToAllInGroup(replyTo, response, groupName);
        }
        public void RespondToAll(string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondToAll(replyTo, response);
        }

        public void RespondToUser(string userId, string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondToUser(userId, replyTo, response);
        }

        /// <summary>
        /// Reply to a specific client 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="response"></param>
        public void RespondToUser(string user, object response)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!string.IsNullOrEmpty(ReplyTo))
                SignalX.RespondToUser(user, ReplyTo, response);
        }

        /// <summary>
        /// Reply to the handler that the client has specified
        /// </summary>
        /// <param name="response"></param>
        public void RespondToSender(object response)
        {
            if (!string.IsNullOrEmpty(this.User))
                SignalX.RespondToUser(this.User, ReplyTo, response);
          
        }

        /// <summary>
        /// Reply to every other clients but the sender
        /// </summary>
        /// <param name="response"></param>
        /// <param name="groupName"></param>
        public void RespondToOthers(object response, string groupName = null)
        {
            if (!string.IsNullOrEmpty(this.User))
                SignalX.RespondToOthers(this.User, ReplyTo, response, groupName);
        }
    }
}