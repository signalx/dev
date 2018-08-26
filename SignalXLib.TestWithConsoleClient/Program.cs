﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace SignalXLib.TestWithConsoleClient
{
    using Microsoft.AspNet.SignalR.Client;

    internal class Program
    {
        private static void Main(string[] args)
        {
            //Set connection
            var connection = new HubConnection("http://127.0.0.1:8088/");
            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("CustomHub");
            //Start connection

            connection.Start().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                        task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }

            }).Wait();

            myHub.Invoke<string>("Send", "HELLO World ").ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error calling send: {0}",
                        task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine(task.Result);
                }
            });

            myHub.On<string>("addMessage", param => {
                Console.WriteLine(param);
            });

            myHub.Invoke<string>("DoSomething", "I'm doing something!!!").Wait();


            Console.Read();
            connection.Stop();
        }
    }
}
