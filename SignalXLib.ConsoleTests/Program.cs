using System;

namespace SignalXLib.ConsoleTests
{
    using Microsoft.Owin.Hosting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;
    using TestObject = SignalXLib.TestHelperLib.TestObject;

    internal class Program
    {
        /// <summary>
        /// NOTE : this is not complete
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
           // TestObject testObject = SignalXTester.SetupGeneralTest();

            var url = "http://localhost:44111";
            using (WebApp.Start<Startup>(url))
            {
                SignalX SignalX = SignalX.Instance();
                System.Diagnostics.Process.Start(url);
                while (true)
                {
                    Console.WriteLine("Press any key to run the test");
                    Console.ReadKey();
                    try
                    {
                   
                        Console.WriteLine("tests succeeded!");
                        SignalX.RunJavaScriptOnAllClients("alert('yo');for(i=0;i<10000;i++) {console.log(Math.pow(999, Math.pow(9, 2)));}",
                            (response, request, error) =>
                            {
                           var     result = Convert.ToInt32(response);
                            }, TimeSpan.FromSeconds(5));
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