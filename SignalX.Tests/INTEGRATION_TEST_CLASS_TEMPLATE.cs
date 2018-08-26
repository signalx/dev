namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    
    [TestClass]
    public class INTEGRATION_TEST_CLASS_TEMPLATE
    {
        [TestMethod]
        public void TEST_TEMPLATE()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    var actual = 0;
                    return new SignalXTestDefinition(
                        script: "",
                        server: () => { },
                        browserType: BrowserType.Unknown,
                        checks: () => { assert.Equal(10000, actual); },
                        events: new TestEventHandler(onClientLoaded: () =>
                             {
                                 signalx.RunJavaScriptOnAllClients($"return 100*100",
                                     (answer) =>
                                     {
                                         actual = (int)answer;
                                     });
                             },
                            onClientError: (e) => { throw e; },
                            onCheckSucceeded: () => { },
                            onCheckFailures: (e) => { }));
                });
        }
    }
}