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
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                   }); ",
                        () => { },
                        () => { assert.AreEqual(10000, actual); },
                        new TestEventHandler(
                            () =>
                            {
                                signalx.RunJavaScriptOnAllClients(
                                    "return 100*100",
                                    answer => { actual = answer.MessageAs<int>(); });
                                signalx.RespondToServer("hala", "");
                            })
                    );
                },
                1);
        }
    }
}