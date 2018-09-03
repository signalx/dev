﻿namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;

    [TestClass]
    public class when_client_sends_message_to_server
    {
        public when_client_sends_message_to_server()
        {
            SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
            SignalXTester.EmbedeLibraryScripts = true;
        }

        [TestMethod]
        public void server_response_with_upperlevel_api_to_all_must_work_with_a_named_handler()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    //SignalXTester.MaxTestTimeSpan= TimeSpan.FromHours(1);
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.client.getSomethingCompleted=function (something) {
                                            signalx.server.sample2(10);
                                       };
                                     signalx.server.sample(100,'getSomethingCompleted');
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    signalx.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_response_with_upperlevel_api_to_all_must_work_with_a_promise()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     var getSomethingCompletedPromise = signalx.server.sample(100);
                                     getSomethingCompletedPromise.done(function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    signalx.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_response_with_upperlevel_api_to_all_must_work_with_a_callback()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function(something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    signalx.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_reply_with_upperlevel_api_to_others_should_exclude_sender()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, request.Message, "server must get the correct message");
                                    signalx.RespondToOthers(request.User, request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_can_reply_with_upperlevel_api_to_client_using_RespondToUser_replyto()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, request.Message, "server must get the correct message");
                                    signalx.RespondToUser(request.User, request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(10, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void server_can_reply_with_upperlevel_api_to_client_using_RespondToAll_replyto()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<int>(100, request.Message, "server must get the correct message");
                                    signalx.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual(10, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void server_response_to_all_must_work_with_a_named_handler()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     signalx.client.getSomethingCompleted=function (something) {
                                            signalx.server.sample2(10);
                                       };
                                     signalx.server.sample(100,'getSomethingCompleted');
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    request.RespondToSender(100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_response_to_all_must_work_with_a_promise()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                     var getSomethingCompletedPromise = signalx.server.sample(100);
                                     getSomethingCompletedPromise.done(function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    request.RespondToSender(100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_response_to_all_must_work_with_a_callback()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function(something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, (int)request.Message, "server must get the correct message");
                                    request.RespondToSender(100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_reply_to_others_should_exclude_sender()
        {
            SignalXTester.RunAndExpectFailure(
                (signalx, assert) =>
                {
                    int result = 0;
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual(100, request.Message, "server must get the correct message");
                                    request.RespondToOthers(100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { result = (int)request.Message; });
                        },
                        () => { assert.AreEqual(result, 10); }
                    );
                });
        }

        [TestMethod]
        public void server_can_reply_to_client_using_RespondToSender()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<int>(100, request.Message, "server must get the correct message");
                                    request.RespondToSender(100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(10, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void server_can_reply_to_client_using_RespondToUser_replyto()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<int>(100, request.Message, "server must get the correct message");
                                    request.RespondToUser(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(10, request.Message); });
                        });
                });
        }

        [TestMethod]
        public void server_can_reply_to_client_using_RespondToAll_replyto()
        {
            SignalXTester.Run(
                (signalx, assert) =>
                {
                    SignalXTester.ScriptDownLoadFunction = ScriptSource.ScriptDownLoadFunction;
                    SignalXTester.EmbedeLibraryScripts = true;
                    return new SignalXTestDefinition(
                        @"signalx.ready(function (server) {
                                        signalx.server.sample(100,function (something) {
                                            signalx.server.sample2(10);
                                       });
                                   }); ",
                        () =>
                        {
                            signalx.Server(
                                "sample",
                                request =>
                                {
                                    assert.AreEqual<int>(100, request.Message, "server must get the correct message");
                                    request.RespondToAll(request.ReplyTo, 100);
                                });
                            signalx.Server(
                                "sample2",
                                request => { assert.AreEqual<int>(10, request.Message); });
                        });
                });
        }
    }
}