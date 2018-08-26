namespace SignalXLib.Tests
{
    using System;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server1
    {
        [Fact]
        public void server_should_be_able_to_run_complex_javascript_on_client()
        {
            SignalXTester.Run((signalX, assert) =>
            {
                SignalXTester.MaxTestWaitTime = TimeSpan.FromSeconds(20);

                var result = 0;
                return new SignalXTestDefinition(
                    script: "",
                    server: () =>
                    {
                    }, browserType: BrowserType.Unknown, checks: () =>
                    {
                        assert.Equal(result, 3);
                    },
                    events: new TestEventHandler(onClientLoaded: () =>
                    {
                        signalX.RunJavaScriptOnAllClients(@"
                                    var grades = [1,2,3,4,5];
                                    var total = 0;
                                    for(var i = 0; i < grades.length; i++) {
                                        total += grades[i];
                                    }
                                    var avg = total / grades.length;
                                    return avg;
                            ",
                            (response, request, error) =>
                            {
                                result = Convert.ToInt32(response);
                            }, TimeSpan.FromSeconds(10));
                    }));
            });
        }

    }
}