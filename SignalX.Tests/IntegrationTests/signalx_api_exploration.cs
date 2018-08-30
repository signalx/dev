namespace SignalXLib.Tests.IntegrationTests
{
    using System;
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
                    signalx.SetUpClientErrorMessageHandler((error, request) =>
                    {

                    });
                    signalx.SetUpClientDebugMessageHandler((error, request) =>
                    {

                    });
                    SignalX mySignalX = SignalX.Instance;
                    signalx.OnException((message, exception) => { });
                    signalx.OnWarning((message, exception) => { });
                    signalx.OnConnectionEvent((message, exception) => { });
                    signalx.AllowDynamicServer = false;
                    signalx.Settings.RequireAuthorizationForPersistentConnections = false;
                    signalx.Settings.ManageUserConnections = true;
                    ConnectionMapping<string> Connections = signalx.Settings.Connections;
                    bool HasOneOrMoreConnections = signalx.Settings.HasOneOrMoreConnections;
                    HubConfiguration HubConfiguration = signalx.Settings.HubConfiguration;
                    bool LogAgentMessagesOnClient = signalx.Settings.LogAgentMessagesOnClient;
                    ISignalXClientReceiver Receiver = signalx.Settings.Receiver;
                    bool RequireAuthorizationForAllHandlers = signalx.Settings.RequireAuthorizationForAllHandlers;

                    signalx.AuthenticationHandler(request => true);
                    signalx.DisableAllClients();
                    signalx.DisableClient("test");
                    signalx.EnableAllClients();
                    signalx.EnableClient("test");
                    signalx.GetInComingMessageSpeedAsync(TimeSpan.FromSeconds(10));
                    signalx.GetOutGoingMessageSpeedAsync(TimeSpan.FromSeconds(10));
                    signalx.SetGlobalDefaultMessageBufferSize(1000);

                    return new SignalXTestDefinition()
                    {
                        OnAppStarted = () =>
                        {
                            signalx.Server(Guid.NewGuid().ToString(), request => { });
                            signalx.ServerAuthorized(Guid.NewGuid().ToString(), request => { });
                            signalx.ServerAuthorizedSingleAccess(Guid.NewGuid().ToString(), request => { });
                            signalx.ServerSingleAccess(Guid.NewGuid().ToString(), request => { });
                        },
                        TestEvents = new TestEventHandler(
                            () =>
                            {
                                assert.AreEqual(1, (int)signalx.ConnectionCount);
                            })
                    };
                });
        }
    }
}