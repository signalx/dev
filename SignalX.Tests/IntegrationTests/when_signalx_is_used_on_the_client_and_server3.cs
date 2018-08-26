namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server3
    {
        /// <summary>
        /// Last server setup will run
        /// </summary>
        [Fact]
        public void can_allow_dynamic_server_on_serverside_when_set_to_do_so()
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
                            signalX.AllowDynamicServer = true;
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
                            //this server will run
                            signalX.Server("TestServerFeedbackHandler", request =>
                            {
                                finalMessage = message1;
                            });
                        },
                        checks: () =>
                        {
                            assert.Equal(message1, finalMessage);
                            assert.NotEqual(message3, finalMessage);
                        },
                        events: new TestEventHandler(onClientLoaded: () =>
                            {
                            },
                            onClientError: (e) =>
                            {
                                throw e;
                            },
                            onCheckSucceeded: () =>
                            {
                                var t = 10;
                            },
                            onCheckFailures: (e) =>
                            {
                            }));
                });
        }

    }
}