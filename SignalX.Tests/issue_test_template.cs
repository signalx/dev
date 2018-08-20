namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class issue_test_template
    {
        [Fact]
        public void it_should_call_server_from_client()
        {
            TestHelper.MaxTestWaitTime = TimeSpan.FromSeconds(20);
            var scenario = new ScenarioDefinition(
                script: @"signalx.ready(function (server) {
							      server.myServer('abc',function (message) {							         
							       });
                                 });",
                server: () =>
                {
                    SignalX.Server("myServer",
                        request =>
                        {
                            SignalX.RespondToAll(request.ReplyTo, request.Message);
                        });
                },
                checks: () =>
                {
                    Assert.True(true);
                }
            );
            TestHelper.RunScenario(scenario);
        }
    }
}