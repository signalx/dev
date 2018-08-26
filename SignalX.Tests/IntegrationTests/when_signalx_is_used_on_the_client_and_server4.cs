namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server4
    {
        [Fact]
        public void will_not_allow_dynamic_server_on_serverside_when_not_set()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);
                    var message1 = Guid.NewGuid().ToString();
                    var message3 = Guid.NewGuid().ToString();

                    string finalMessage = null;

                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server.ServerHandler ('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        server: () =>
                        {
                            signalX.AllowDynamicServer = false;
                            //SET UP SERVER
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });
                            //this server will be ignored
                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message3;
                            });
                            try
                            {
                                //this server will run
                                signalX.Server("TestServerFeedbackHandler", request =>
                                {
                                    finalMessage = message1;
                                });
                            }
                            catch (Exception e)
                            {
                            }
                        },
                        checks: () =>
                        {
                            assert.NotEqual(message1, finalMessage);
                            assert.Equal(message3, finalMessage);
                        });
                });
        }

    }
}