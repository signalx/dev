namespace SignalXLib.Tests
{
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class TEST_CLASS_TEMPLATE
    {
        [Fact]
        public void TEST_TEMPLATE()
        {
            TestHelper.RunScenario(
                (signalx) =>
                {
                    var actual = 0;
                    return new SignalXTestDefinition(
                        script: "",
                        server: () => { },
                        checks: () => { TestHelper.ExpectEqual(10000 ,actual); },
                        onClientLoaded: () =>
                        {
                            signalx.RunJavaScriptOnAllClients($"return 100*100",
                                (answer) =>
                                {
                                    actual = (int)answer;
                                });
                        },
                        onClientError: (e) => { throw e; },
                        browserType: BrowserType.Unknown,
                        onCheckSucceeded: () => { },
                        onCheckFailures: (e) => { }
                    );
                });
        }
    }
}