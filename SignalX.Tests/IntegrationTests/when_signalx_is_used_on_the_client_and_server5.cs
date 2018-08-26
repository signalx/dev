namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server5
    {
        [Fact]
        public void client_dont_need_to_specify_any_argument_to_call_the_server()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message3 = Guid.NewGuid().ToString();
                    var finalMessage = "";
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server.ServerHandler('" + message1 + @"',function (m) {
							          signalx.server.TestServerFeedbackHandler ();
							       });
                                });",
                        server: () =>
                        {
                            signalx.Server("ServerHandler", request =>
                            {
                                signalx.RespondToAll(request.ReplyTo, request.Message);
                            });

                            signalx.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message3;
                            });
                        },
                        browserType: BrowserType.Unknown,
                        checks: () => { assert.Equal(message3, finalMessage); }
                    );
                });
        }

    }
}