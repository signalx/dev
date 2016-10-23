using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalXLib.Lib
{
    public class SignalXHub : Hub
    {
        public void Send(string handler, object message, string replyTo, object sender, string messageId)
        {
            try
            {
                SignalX._signalXServers[handler].Invoke(message, sender, replyTo, messageId, Context?.User?.Identity?.Name, Context?.ConnectionId);
            }
            catch (Exception e)
            {
                var error = "An error occured on the server while processing message " + message + " with id " +
                            messageId + " received from " + sender + " for a response to " + replyTo + " - ERROR: " +
                            e.Message;
                SignalX.RespondToAll("signalx_error", error);
                if (!string.IsNullOrEmpty(replyTo))
                {
                    SignalX.RespondToAll(replyTo, error);
                }
                SignalX.ExceptionHandler?.Invoke(new Exception(error, e));
            }
          
        }

        public void GetMethods()
        {
            var methods = SignalX._signalXServers.Aggregate("var $sx= {", (current, signalXServer) => current + (signalXServer.Key + @":function(m,repTo,sen,msgId){ var deferred = $.Deferred(); window.signalxidgen=window.signalxidgen||function(){return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);    return v.toString(16);})}; window.signalxid=window.signalxid||window.signalxidgen();   sen=sen||window.signalxid;repTo=repTo||''; var messageId=window.signalxidgen(); var rt=repTo; if(typeof repTo==='function'){ signalx.waitingList(messageId,repTo);rt=messageId;  }  if(!repTo){ signalx.waitingList(messageId,deferred);rt=messageId;  }  chat.server.send('" + signalXServer.Key + "',m,rt,sen,messageId); if(repTo){return messageId}else{ return deferred.promise();}   },")) +"}; $sx; ";

            Clients.All.addMessage(methods);
        }

        public override Task OnConnected()
        {
            string name = Context?.User?.Identity?.Name;

            if(name!=null)
            SignalX.Connections?.Add(name, Context?.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                SignalX.Connections?.Remove(name, Context?.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context?.User?.Identity?.Name;

            if (name != null)
                if (SignalX.Connections !=null && !SignalX.Connections.GetConnections(name).Contains(Context?.ConnectionId))
            {
                SignalX.Connections?.Add(name, Context?.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}