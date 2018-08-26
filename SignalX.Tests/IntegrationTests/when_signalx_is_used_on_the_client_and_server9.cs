namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server9
    {
        [Fact]
        public void client_should_support_named_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);

                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    var groupName = Guid.NewGuid().ToString();

                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
								signalx.client." + "ClientHandler" + @"=function (m) {
							      signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"' ,function(){});
							    };
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"','" + "ClientHandler" + @"');
                                });",
                        server: () =>
                        {
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll("ClientHandler", request.Message);
                            });

                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1 + message2, finalMessage);
                        });
                });
        }

    }
}