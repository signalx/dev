namespace SignalXLib.Tests.IntegrationTests
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
                    int actual = 0;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     
                                   }); ",
                        onAppStarted: () => { },
                        checks: () => { assert.AreEqual(10000, actual); },
                        events: new TestEventHandler(
                            () =>
                            {
                                signalx.RunJavaScriptOnAllClients(
                                    $"return 100*100",
                                    answer => { actual = (int)answer; });
                            })
                        );
                },1);
        }
    }
}