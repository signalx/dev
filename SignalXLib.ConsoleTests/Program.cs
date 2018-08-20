using System;

namespace SignalXLib.ConsoleTests
{
    using Microsoft.Owin.Hosting;
    using SignalXLib.TestHelperLib;
    using Xunit;
    using TestObject = SignalXLib.TestHelperLib.TestObject;

    internal class Program
    {
        private static void Main(string[] args)
        {
            TestObject testObject = TestHelper.SetupGeneralTest();

            var url = "http://localhost:44111";
            using (WebApp.Start<Startup>(url))
            {
                System.Diagnostics.Process.Start(url);
                while (true)
                {
                    Console.WriteLine("Press any key to run the test");
                    Console.ReadKey();
                    try
                    {
                        Assert.Equal(testObject.Message, testObject.FinalMessage);
                        Assert.Equal(testObject.Message, testObject.FinalMessage2);
                        Assert.Equal(testObject.Message, testObject.FinalMessage3);
                        Assert.Equal(testObject.Message, testObject.FinalMessage4);
                        Assert.True(testObject.VerifiedJoinedGroup, "verifiedJoinedGroup");
                        Assert.True(testObject.VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                        Console.WriteLine("tests succeeded!");
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("press any key to try again");
                    }
                }
            }

            Console.WriteLine("press any key to exit!");
            Console.ReadKey();
        }
    }
}