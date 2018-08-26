namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server7
    {
        [Fact]
        public void client_should_support_anonymous_callback2()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m);
							       });
                                });",
                        server: () =>
                        {
                            signalX.Server("ServerHandler", request =>
                            {
                                signalX.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = request.Message as string;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1, finalMessage);
                        });
                });
        }

    }
}