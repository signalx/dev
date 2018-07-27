using Microsoft.Owin.Hosting;
using SignalXLib.Lib;
using System;
using System.Threading.Tasks;

namespace ConsoleApplicationCSharp
{
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var url = "http://localhost:44111";

            using (WebApp.Start(url))
            {
                SignalX.RequireAuthorizationForAllHandlers = false;
                SignalX.AuthenticationHandler((r) => true);

                SignalX.OnWarning(
                    (m, e) =>
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(e);
                    });
                SignalX.OnException(
                    (m, e) =>
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(e);
                    });
                SignalX.Server("Sample", request =>
               {
                   var messageId = Guid.NewGuid().ToString();
                   SignalX.RespondToAll("Myclient", messageId + " - " + request.Message + ": Thank you for sending me the message ");
                   SignalX.RespondToAll("Myclient", messageId + " - " + request.Message + ": Hang on i'm not done yet");
                   Task.Delay(TimeSpan.FromMilliseconds(1000)).ContinueWith(x =>
                   {
                       SignalX.RespondToAll("Myclient", messageId + " - " + request.Message + ": So im almost done");
                   });
                   Task.Delay(TimeSpan.FromMilliseconds(1000)).ContinueWith(x =>
                   {
                       SignalX.RespondToAll("Myclient", messageId + " - " + request.Message + ": Im' done!");
                   });
               });
                SignalX.Server(
                    "GetInComingMessageSpeed",
                    (request) =>
                    {
                        SignalX.GetInComingMessageSpeedAsync(TimeSpan.FromSeconds(10)).ContinueWith(
                 x =>
                 {
                     SignalX.RespondToAll(request.ReplyTo, x.Result);
                 });
                    });
                SignalX.Server(
                    "GetOutGoingMessageSpeed",
                    (request) =>
                    {
                        SignalX.GetOutGoingMessageSpeedAsync(TimeSpan.FromSeconds(10)).ContinueWith(
                            x =>
                            {
                                SignalX.RespondToAll(request.ReplyTo, x.Result);
                            });
                    });
                SignalX.Server("DisableAllClients", (request) => { SignalX.DisableAllClients(); });
                SignalX.Server("Sample2",
                    (message, sender, replyTo, messageId) =>
                    {
                        SignalX.RespondToAll(string.IsNullOrEmpty(replyTo) ? "Myclient" : replyTo, messageId + ":" + sender + " sent me this message : " + message + " and asked me to reply to " + replyTo);
                    });

                SignalX.Server("Sample3", (request) => request.RespondToAll(request.ReplyTo));

                System.Diagnostics.Process.Start(url);

                foreach (int i in Enumerable.Range(0, 100000))
                {
                    SignalX.RespondToAll("sa", "you");
                }

                Console.ReadLine();
            }
        }
    }
}