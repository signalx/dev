using System;

namespace SignalXLib.Lib
{
    public class SignalXRequest
    {
        public SignalXRequest(string replyTo, object sender, string messageId, object message)
        {
            ReplyTo = replyTo;
            Sender = sender;
            MessageId = messageId;
            Message = message;
        }

        public string ReplyTo { get; }
        public object Sender { get; }
        public string MessageId { get; }
        public object Message { get; }
        public void Respond(object response)
        {
            if (!string.IsNullOrEmpty(ReplyTo))
                SignalX.RespondTo(ReplyTo, response);
        }
        public void RespondTo(string replyTo, object response)
        {
            if (replyTo == null) throw new ArgumentNullException(nameof(replyTo));
            SignalX.RespondTo(replyTo, response);
        }
    }
}