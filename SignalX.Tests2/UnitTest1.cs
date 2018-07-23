using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalXLib.Lib;
using System;

namespace SignalXLib.TestsNuget
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var url = "http://localhost:44111";

            using (WebApp.Start(url))
            {
                try
                {
                    SignalX.Server("Sample", request => request.RespondToUser("Myclient", "yooo server : " + request.Message));
                    Console.WriteLine("quiting server in next 5 minute");
                    Reader.ReadLine(TimeSpan.FromMinutes(5));
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("session ended.");
                }
            }
        }
    }
}