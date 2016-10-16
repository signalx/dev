using SignalXLib.Lib;
using System;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace ConsoleApplicationCSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var url = "http://localhost:44111";


            using (WebApp.Start(url))
            {
                 SignalX.Server("Sample", request =>
                {
                    var messageId = Guid.NewGuid().ToString();
                    SignalX.RespondTo("Myclient", messageId +" - "+ request.Message + ": Thank you for sending me the message " );
                    SignalX.RespondTo("Myclient", messageId + " - " + request.Message + ": Hang on i'm not done yet");
                    Task.Delay(TimeSpan.FromMilliseconds(1000)).ContinueWith(x =>
                    {
                        SignalX.RespondTo("Myclient", messageId + " - " + request.Message + ": So im almost done");
                    });
                    Task.Delay(TimeSpan.FromMilliseconds(1000)).ContinueWith(x =>
                    {
                        SignalX.RespondTo("Myclient", messageId + " - " + request.Message + ": Im' done!");
                    });
                });
                
                SignalX.Server("Sample2", (message,sender, replyTo,messageId) => SignalX.RespondTo(string.IsNullOrEmpty(replyTo) ? "Myclient" : replyTo, messageId+":"+ sender + " sent me this message : " + message + " and asked me to reply to " + replyTo));


                SignalX.Server("Sample3", (request) => request.Respond(request.ReplyTo));


                System.Diagnostics.Process.Start(url);
                Console.ReadLine();
            }

           
        }
    }
}