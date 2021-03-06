﻿/*!
 * SignalX JavaScript Library v6.0.0-pre
 * https://github.com/signalx
 */
(function ($, window, undefined) {
    
    if (typeof SIGNALR_ROOT !== 'undefined') {
        var SIGNALR_ROOT = "/signalr/hubs";
    }
    var signalx = {};
    var context = {};
    signalx.logConnections = false;

    signalx.logConnections = function (p) {
        signalx.error.logConnections = (p || false) && true;
    };
    signalx.error = function (f) {
        //signalx.error.f = f || signalx.error.f;
        $(document).on('SIGNALX:ERROR', f);
    };
    signalx.error.f = function (o) {
        $(document).trigger('SIGNALX:ERROR',o);
    };

    signalx.debug = function (f) {
        $(document).on('SIGNALX:DEBUG', f);
    };
    signalx.debug.f = function(o) {
        $(document).trigger('SIGNALX:DEBUG', o);
    };
    
    signalx.waitingList = function (n, f) {
        if (n && f) {
            signalx.waitingList.w[n] = f;
        }
    };
    signalx.groupList = signalx.groupList || [];
    signalx.waitingList.w = signalx.waitingList.w || {};
    //debug
    signalx.debug.f ("starting lib");
    if (window.signalx && window.signalx.server && window.signalx.error) {
        signalx.error.f({
            description: "signalx is already included in the page"
        });
        return;
    }
    if (window.signalx) {
        signalx.error.f({
            description: "signalx variable in windows context, i will override it!"
        });
    }
    var handlers = {};
    var haservers = false;
    var clientReceiver = function (owner, message) {
        //debug
        signalx.debug.f ("successfully received server message meant for  " + owner + " handler . Message is : " + JSON.stringify(message));
        context.loadClients();
        var own = signalx.waitingList.w[owner];

        if (!own) {
            signalx.debug.f ("Could not find any defined callback for " + owner);
            own = handlers[owner];
            if (!own) {
                var errMsg = "Could not find specified client handler '" + owner + "' to handle the server response '" + message;
                signalx.error.f({
                    error: errMsg,
                    description: "No client handler registered by the name " + owner
                });
            }
        } else {
            delete signalx.waitingList.w[owner];
        }

        try {
            if (typeof own === "object" && typeof own.resolve === "function") {
                own.resolve(message);
            } else {
                if (own) {
                    own(message);
                }
            }
        } catch (e) {
            signalx.error.f({
                error: e,
                description: "Error while running client handler " + owner + " with the server message " + message
            });
        }
    };
    var chatserversend = function (name, message, retTo, sender, mId, f) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c === 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sender = sender || window.signalxid;
        retTo = retTo || '';
        var messageId = window.signalxidgen();
        var rt = retTo;
        if (typeof retTo === 'function') {
            signalx.waitingList(messageId, retTo);
            rt = messageId;
        }
        if (!retTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }

        signalx.debug.f ("Server on client called  by " + name + " from sender " + sender);

        var respondTo = function (na, mes) {
            clientReceiver(na, mes);
        };
        var respond = function (mes) {
            respondTo(rt, mes);
        };

        var request = {
            respondTo: respondTo,
            RespondTo: respondTo,
            Respond: respond,
            respond: respond,
            message: message,
            replyTo: rt,
            sender: sender,
            messageId: mId,
            Message: message,
            ReplyTo: rt,
            Sender: sender,
            MessageId: mId
        };
        signalx.debug.f ("Sending Message : " + JSON.stringify(request));
        try {
            f(request);
        } catch (e) {
            signalx.error.f({
                error: e,
                description: "Error executing server client",
                context: request
            });
        }

        if (retTo) {
            return messageId;
        } else {
            return deferred.promise();
        }
    };
    var toCamelCase = function (str) {
        return str.charAt(0).toLowerCase() + str.slice(1);
    };
    var toUnCamelCase = function (str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    };
    signalx.server = function (n, f) {
        if (n && f) {
            haservers = true;
            var nname = toCamelCase(n);
            signalx.server[nname] = (function (f, n) {
                return function (message, retTo, sender, messageId) {
                    return chatserversend(n, message, retTo, sender, messageId, f);
                };
            })(f, nname);
            nname = toUnCamelCase(n);
            signalx.server[nname] = (function (f, n) {
                return function (message, retTo, sender, messageId) {
                    return chatserversend(n, message, retTo, sender, messageId, f);
                };
            })(f, nname);
        } else {
            signalx.error.f({
                description: "error registering server handler"
            });
        }
    };

    var hasRun = false;
    var mailBox = [];

    mailBox.run = function () {
        context.loadClients();
        if (signalx.server) {
            while (mailBox.length) {
                var func = mailBox.pop();
                try {
                    func(signalx.server);
                } catch (e) {
                    signalx.error.f({
                        error: e,
                        context: JSON.stringify(func),
                        description: "Error while executing method in signalx.ready"
                    });
                }
            }
        }
    };

    signalx.client = function (name, f) {
        //todo check if is function
        if (name && f) {
            //debug
            signalx.debug.f ("registering handler : " + name);
            handlers[name] = f;
            var camelCase = toCamelCase(name);
            if (camelCase !== name) {
                handlers[camelCase] = f;
            }

            var unCamelCase = toUnCamelCase(name);
            if (unCamelCase !== name) {
                handlers[unCamelCase] = f;
            }
        } else {
            var errMsg = "Please supply a valid client handler name and method";
            signalx.error.f({
                error: errMsg,
                description: errMsg
            });
            throw errMsg;
        }
    };

    context.loadClients = function () {
        for (var key in signalx.client) {
            if (signalx.client.hasOwnProperty(key)) {
                if (!handlers[key]) {
                    handlers[key] = signalx.client[key];
                }

                var camelCase = toCamelCase(key);
                if (camelCase !== key) {
                    if (!handlers[camelCase]) {
                        handlers[camelCase] = signalx.client[key];
                    }
                }

                var unCamelCase = toUnCamelCase(key);
                if (unCamelCase !== key) {
                    if (!handlers[unCamelCase]) {
                        handlers[unCamelCase] = signalx.client[key];
                    }
                }
            }
        }
    };

    var isReady = false;
    signalx.ready = function (f) {
        if (f) {
            mailBox.push(f);
        }
        if (isReady) {
            mailBox.run();
        }
        if (!hasRun) {
            hasRun = true;
            //debug
            signalx.debug.f ("loading signalr script at /signalr/hubs ");
            $.ajax({
                url: "/signalr/hubs",
                dataType: "script",
                success: function () {
                    $(function () {
                      var  chat = $.connection.signalXHub;
                        //debug
                        signalx.debug.f ("successfully loaded signalr script from /signalr/hubs ");
                        chat.client.groupManager = function (groupName, operation) {
                            if (operation === "join") {
                                signalx.debug.f('joined group '+groupName);
                                signalx.groupList.push(groupName);
                            }
                            if (operation === "leave") {
                                signalx.debug.f('left group ' + groupName);
                                signalx.groupList = signalx.groupList.filter(e => e !== groupName); 
                            }
                            signalx.groupNotifications = signalx.groupNotifications || [];
                            for (var gi = 0; gi < signalx.groupNotifications.length; gi++) {
                                typeof signalx.groupNotifications[gi] === "function" && signalx.groupNotifications[gi](groupName, operation);
                            }
                            signalx.groupNotifications = [];
                        };
                        signalx.groups = {
                            join: function (grpName, f) {
                                signalx.debug.f('joining group ' + grpName);
                                signalx.groupNotifications = [];
                                signalx.groupNotifications.push(f);
                                chat.server.joinGroup(grpName);
                            },
                            leave: function (grpName, f) {
                                signalx.debug.f('leaving group ' + grpName);
                                signalx.groupNotifications = [];
                                signalx.groupNotifications.push(f);
                                 chat.server.leaveGroup(grpName);
                            }
                        };
                         chat.client.addMessage = function (message) {
                            try {
                                signalx.debug.f ("successfully loaded signalx script from server : " + message);
                                var server = eval(message);
                                for (var nnn in server) {
                                    if (server.hasOwnProperty(nnn)) {
                                        signalx.server[nnn] = server[nnn];
                                    }
                                }
                                
                                (typeof  chat.server.signalXClientReady === "function") &&  chat.server.signalXClientReady();
                                if (typeof signalx.beforeOthersReady === "function") {
                                    signalx.beforeOthersReady(function (msgResp) {
                                        msgResp && signalx.debug.f("After serve's on ready executes : "+msgResp);
                                       isReady = true;
                                       mailBox.run();
                                    });
                                } else {
                                    isReady = true;
                                    mailBox.run();
                                }
                                
                            } catch (e) {
                                (typeof  chat.server.signalXClientReadyError === "function") &&  chat.server.signalXClientReadyError("Error downloading script from server",e);
                                signalx.error.f({
                                    error: e,
                                    context: message,
                                    description: "Error downloading script from server"
                                });
                                throw e;
                            }
                        };

                         chat.client.broadcastMessage = clientReceiver;


                        var promise = $.connection.hub.start();

                        
                        promise.done(function () {
                            //debug
                            signalx.debug.f ("signalr hub started successfully. Now loading signalx script from hub");
                             chat.server.getMethods();
                        }).fail(function (e) {
                            signalx.error.f({
                                error: e,
                                description: "Error requesting client script from server"
                            });
                        });
                    });
                }
            }).fail(function () {
                //todo log error
                context.loadClients();
                mailBox.run();
                isReady = true;
            });
        }
    };
    window.signalx = signalx;
    signalx.ready(function () {
        for (var key in handlers) {
            if (handlers.hasOwnProperty(key)) {
                //debug
                signalx.debug.f ("Client handlers registered : " + key);
            }
        }
        //debug
        signalx.debug.f ("signalx is all set to start reactive server - client server communications");
    });
    signalx.client.signalx_error = function (message) {
        signalx.debug.f (message);
        signalx.error.f({
            error: message,
            description: "Error occured on the server"
        });
    };
    signalx.getConnection = function () {
        return $.connection.hub;
    };
    signalx.enableConnectionDebugging = function() {
        $.connection.hub.logging = function (o) {
            signalx.debug.f({
                description: o,
                context: 'connection.hub.logging'
            });
        };
        $.connection.logging = function (o) {
            signalx.debug.f({
                description: o,
                context: 'connection.logging'
            });
        };
        $.connection.hub.error(function (input) {
            signalx.error.f({
                error: input,
                description: "Connection.hub error reporting"
            });
        });
    }
    signalx.ready(function() { });
}(window.jQuery, window));
