namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server6
    {
        [Fact]
        public void client_dont_need_to_specify_callback_when_passing_back_only_a_message()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var message1 = Guid.NewGuid().ToString();
                    var message2 = Guid.NewGuid().ToString();
                    var finalMessage = "";
                    return new SignalXTestDefinition(
                        script: @"
                               signalx.ready(function (server) {
							      server." + "ServerHandler" + @"('" + message1 + @"',function (m) {
							          signalx.server." + "TestServerFeedbackHandler" + @"(m +'" + message2 + @"');
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
                                finalMessage = request.Message as string;
                            });
                        },
                        browserType: BrowserType.Unknown,
                        checks: () => { assert.Equal(message1 + message2, finalMessage); },
                        events: new TestEventHandler(onClientLoaded: () =>
                            {
                            },
                            onClientError: (e) => { throw e; },
                            onCheckSucceeded: () => { },
                            onCheckFailures: (e) => { }));
                });
        }

    }
}