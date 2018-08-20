namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using System;
    using Xunit;

    public class issue_test_template
    {
        [Fact]
        public void it_should_call_server_from_client()
        {
            TestHelper.MaxTestWaitTime = TimeSpan.FromSeconds(200);
            var result = 0;
            var scenario = new ScenarioDefinition(
                script: "",
                server: () =>
                {
                },
                checks: () =>
                {
                    Assert.Equal(50, result);
                },
                onClientLoaded: () =>
                {
                    SignalX.RunJavaScriptOnAllClients($"return 5*10",
                        (response, request, error) =>
                        {
                            result = Convert.ToInt32(response);
                        }, TimeSpan.FromSeconds(5));
                }
            );
            TestHelper.RunScenario(scenario);
        }
    }
}