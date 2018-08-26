namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server2
    {
        [Fact]
        public void server_should_be_able_to_run_simple_javascript_on_client()
        {
            SignalXTester.Run(
                (signalX, assert) =>
                {
                    SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(200);
                    var result = 0;

                    return new SignalXTestDefinition(
                        script: "",
                        server: () =>
                        {
                        },
                        checks: () =>
                        {
                            assert.Equal(50, result);
                        },
                        events: new TestEventHandler(onClientLoaded: () =>
                        {
                            signalX.RunJavaScriptOnAllClients($"return 5*10",
                                (response, request, error) =>
                                {
                                    result = Convert.ToInt32(response);
                                }, TimeSpan.FromSeconds(15));
                        }));
                });
        }

    }
}