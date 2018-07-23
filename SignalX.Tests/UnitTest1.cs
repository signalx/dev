﻿using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalXLib.Lib;
using System;

namespace SignalXLib.Tests
{
    using System.Threading;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var url = "http://localhost:44111";
            using (WebApp.Start<Startup>(url))
            {
                string finalMessage = null;
                string message = Guid.NewGuid().ToString();
                string clientHandler = "myclientHandler" + Guid.NewGuid().ToString().Replace("-", "");
                string serverHandler = "myServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                string testServerFeedbackHandler = "myTestServerHandler" + Guid.NewGuid().ToString().Replace("-", "");
                var indexPage = TestHelper.GetIndexPages(message, clientHandler, serverHandler, testServerFeedbackHandler);

                foreach (var index in indexPage)
                {
                    System.IO.File.WriteAllText(TestHelper.FilePath, index);
                    SignalX.Server(serverHandler, request =>
                     {
                         SignalX.RespondToAll(clientHandler, message);
                     });
                    SignalX.Server(testServerFeedbackHandler, request =>
                    {
                        finalMessage = request.Message as string;
                    });

                    var t = new Thread(
                      () =>
                      {
                          var Runner = new StepRunner.Form1()
                          {
                              url = url
                          };
                          Runner.ShowDialog();
                      });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();

                    TestHelper.AwaitAssert(() =>
                    Assert.AreEqual(message, finalMessage),
                    TimeSpan.FromSeconds(1000),
                    () => { });
                }
            }
        }
    }
}