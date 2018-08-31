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
                SignalX SignalX = SignalX.Instance;
                SignalX.Settings.RequireAuthorizationForAllHandlers = false;
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
                   var messageId = SignalXExtensions.GenerateUniqueNameId();
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
                    (request) =>
                    {
                        SignalX.RespondToAll(string.IsNullOrEmpty(request.ReplyTo) ? "Myclient" : request.ReplyTo, request.MessageId + ":" + request.Sender + " sent me this message : " + request.Message + " and asked me to reply to " + request.ReplyTo);
                    });

                SignalX.Server("Sample3", (request) => request.RespondToSender(request.ReplyTo));

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