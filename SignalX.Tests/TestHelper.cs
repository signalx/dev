using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalXLib.Tests
{
    public class TestHelper
    {
        public static List<string> GetIndexPages(string message, string clientHandler, string serverHandler, string testServerFeedbackHandler)
        {
            var result = new List<string>();

            result.Add(@"<!DOCTYPE html>
							<html>
							<body>
							<script src='https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js'></script>     
							<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js'></script>
							<script src='https://unpkg.com/signalx'></script>
							<script>
								signalx.client." + clientHandler + @"=function (m) {
							      signalx.server." + testServerFeedbackHandler + @"(m,function(){});
							    };							    
                               signalx.ready(function (server) {
							      server." + serverHandler + @"('" + message + @"','" + clientHandler + @"');                                   
                               });
							</script>
							</body>
							</html>");
            return result;
        }
        public static string FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\index.html";

        public static void AwaitAssert(Action operation,  Action cleanUpOperation )
        {
            AwaitAssert( operation,  null,  cleanUpOperation );
        }

        public static void AwaitAssert(Action operation, TimeSpan? maxDuration = null, Action cleanUpOperation=null)
        {
            maxDuration = maxDuration ?? TimeSpan.FromSeconds(5);
            var start = DateTime.Now;
            bool done = false;
            while ((DateTime.Now - start).TotalMilliseconds < maxDuration.Value.TotalMilliseconds && !done)
            {
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                try
                {
                    operation();
                    done = true;
                }
                catch (Exception)
                {
                }
            }
            cleanUpOperation?.Invoke();
            if (!done)
            {
                throw new Exception("Unable to pass withing specified miliseconds" + maxDuration.Value.TotalMilliseconds);
            }

        }

    }
}