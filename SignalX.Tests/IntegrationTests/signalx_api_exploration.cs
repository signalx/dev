namespace SignalXLib.Tests.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNet.SignalR;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class signalx_api_exploration
    {
        [TestMethod]
        public void Test()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    signalx.SetUpClientErrorMessageHandler((error, request) => { });
                    signalx.SetUpClientDebugMessageHandler((error, request) => { });
                    SignalX mySignalX = SignalX.Instance;
                    signalx.OnException((message, exception) => { });
                    signalx.OnWarning((message, exception) => { });
                    signalx.OnConnectionEvent((message, exception) => { });
                    signalx.AllowDynamicServer = false;
                    signalx.Settings.RequireAuthorizationForPersistentConnections = false;
                    signalx.Settings.ManageUserConnections = true;
                    ConnectionMapping<string> Connections = signalx.Connections;
                    bool HasOneOrMoreConnections = signalx.Settings.HasOneOrMoreConnections;
                    HubConfiguration HubConfiguration = signalx.Settings.HubConfiguration;
                    bool LogAgentMessagesOnClient = signalx.Settings.LogAgentMessagesOnClient;
                    // ISignalXClientReceiver Receiver = signalx.Settings.Receiver;
                    bool RequireAuthorizationForAllHandlers = signalx.Settings.RequireAuthorizationForAllHandlers;

                    signalx.AuthenticationHandler(request => Task.FromResult(true));
                    signalx.DisableAllClients();
                    signalx.DisableClient("test");
                    signalx.EnableAllClients();
                    signalx.EnableClient("test");
                    signalx.GetInComingMessageSpeedAsync(TimeSpan.FromSeconds(10));
                    signalx.GetOutGoingMessageSpeedAsync(TimeSpan.FromSeconds(10));
                    signalx.SetGlobalDefaultMessageBufferSize(1000);
                    signalx.AuthenticationHandler(r => Task.FromResult(true));
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition("")
                    {
                        OnAppStarted = () =>
                        {
                            signalx.Server(SignalXExtensions.GenerateUniqueNameId().Replace("-", ""), request => { });
                            signalx.Server(ServerType.Authorized, SignalXExtensions.GenerateUniqueNameId().Replace("-", ""), request => { });
                            signalx.Server(ServerType.AuthorizedSingleAccess, SignalXExtensions.GenerateUniqueNameId().Replace("-", ""), request => { });
                            signalx.Server(ServerType.SingleAccess, SignalXExtensions.GenerateUniqueNameId().Replace("-", ""), request => { });
                        },
                        TestEvents = new TestEventHandler(
                            () => { assert.AreEqual(1, (int)signalx.ConnectionCount); })
                    };
                });
        }

        [TestMethod]
        public void can_register_to_receive_internal_trace_messages()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    int traceCount = 0;
                    signalx.Advanced.OnTrace(
                        (s, m, e, l) =>
                        {
                            if (traceCount == 0)
                                traceCount++;
                        });
                    signalx.Advanced.OnTrace(
                        (c, s, m, e) =>
                        {
                            if (traceCount == 1)
                                traceCount++;
                        });
                    signalx.Advanced.OnTrace(
                        (m, e) =>
                        {
                            if (traceCount == 2)
                                traceCount++;
                        });

                    signalx.Advanced.OnTrace(
                        m =>
                        {
                            if (traceCount == 3)
                                traceCount++;
                        });

                    return new SignalXTestDefinition(
                        @"",
                        () => { },
                        () => { assert.AreEqual(4, traceCount); });
                });
        }
    }
}