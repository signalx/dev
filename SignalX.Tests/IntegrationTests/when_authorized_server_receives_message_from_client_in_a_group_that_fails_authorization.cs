﻿namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_authorized_server_receives_message_from_client_in_a_group_that_fails_authorization
    {
        [TestMethod]
        public void group_authorized_server_should_block_request_that_fails_authorization_when_ServerAuthorizedSingleAccess_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request ) => { return request.Groups.Contains("groupB"); });
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                             signalx.groups.join('groupA',function(){ signalx.server.sample(100); });
                                   }); ",
                        () =>
                        {
                            signalx.Server(ServerType.AuthorizedSingleAccess,
                                "sample",
                                (request) => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void group_authorized_server_should_block_request_that_fails_authorization_when_serverAuthorized_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request ) => { return request.Groups.Contains("groupB"); });
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                             signalx.groups.join('groupA',function(){  signalx.server.sample(100); });
                                   }); ",
                        () =>
                        {
                            signalx.Server(ServerType.Authorized,
                                "sample",
                                (request) => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsFalse(failed);
                        });
                });
        }

        [TestMethod]
        public void group_authorized_server_should_allow_request_that_passes_authorization_when_ServerAuthorizedSingleAccess_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request ) => { return request.Groups.Contains("groupB"); });
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                             signalx.groups.join('groupB',function(){ signalx.server.sample(100); });
                                   }); ",
                        () =>
                        {
                            signalx.Server(ServerType.AuthorizedSingleAccess,
                                "sample",
                                (request) => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsTrue(failed);
                        });
                });
        }

        [TestMethod]
        public void group_authorized_server_should_allow_request_that_passes_authorization_when_serverAuthorized_is_used()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    bool failed = false;
                    signalx.AuthenticationHandler((request ) => { return request.Groups.Contains("groupB"); });
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                             signalx.groups.join('groupB',function(){ signalx.server.sample(100); });
                                   }); ",
                        () =>
                        {
                            signalx.Server(ServerType.Authorized,
                                "sample",
                                (request) => { failed = true; });
                        },
                        () =>
                        {
                            assert.WaitForSomeTime(SignalXTester.MaxTestTimeSpan);
                            assert.IsTrue(failed);
                        });
                });
        }
    }
}