namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server8
    {
        [Fact]
        public void client_should_support_anonymous_callback()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    string finalMessage = null;
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
                                            server.TestServerFeedbackHandler(m +'" + message2 + @"' ,function(){});
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
                            assert.Equal(message1 + message2, finalMessage);
                        });
                });
        }

    }
}