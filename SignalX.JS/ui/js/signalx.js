(function ($, window, undefined) {
    var signalx = {};
    signalx.error = function (f) {
        signalx.error.f = f || signalx.error.f;
    };
    signalx.error.f = function (o) { console.error(o); };

    signalx.debug = function (f) {
        signalx.debug.f = f;
    };
    var debuging = function (o) {
        signalx.debug.f && signalx.debug.f(o);
    };
    signalx.waitingList = function (n, f) {
        if (n && f) {
        signalx.waitingList.w[n] = f;
        }
    };
    signalx.waitingList.w = signalx.waitingList.w || {};
    //debug
    debuging("starting lib");
    if (window.signalx && window.signalx.server && window.signalx.error) {
        signalx.error.f({
            description: "signalx is already included in the page"
        });
        return;
    } else if (window.signalx) {
        signalx.error.f({
            description: "signalx variable in windows context, i will override it!"
        });
    }
    var handlers = {};
    signalx.server = false;
    var hasRun = false;
    var mailBox = [];

    mailBox.run = function () {
        if (signalx.server) {
            while (mailBox.length) {
                var func = mailBox.pop();
                try {
                    func(signalx.server);
                } catch (e) {
                    signalx.error.f({
                        error: e,
                        description: "Error while executing method in signalx.ready"
                    });
                }
            }
        }
    };
    var toCamelCase = function (str) {
        return str.charAt(0).toLowerCase() + str.slice(1);
    };
    var toUnCamelCase = function (str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    };
    signalx.client = function (name, f) {
        //todo check if is function
        if (name && f) {
            //debug
            debuging("registering handler : " + name);
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

    signalx.ready = function (f) {
        f && mailBox.push(f);
        mailBox.run();
        if (!hasRun) {
            hasRun = true;

            //debug
            debuging("loading signalr script at /signalr/hubs ");
            $.ajax({
                url: "/signalr/hubs",
                dataType: "script",
                success: function () {
                    $(function () {
                        var chat = $.connection.signalXHub;
                        //debug
                        debuging("successfully loaded signalr script from /signalr/hubs ");
                        for (var key in signalx.client) {
                            if (signalx.client.hasOwnProperty(key)) {
                                handlers[key] = signalx.client[key];
                                var camelCase = toCamelCase(key);
                                if (camelCase !== key) {
                                    handlers[camelCase] = signalx.client[key];
                                }

                                var unCamelCase = toUnCamelCase(key);
                                if (unCamelCase !== key) {
                                    handlers[unCamelCase] = signalx.client[key];
                                }
                            }
                        }
                        chat.client.addMessage = function (message) {
                            try {
                                //debug
                                debuging("successfully loaded signalx script from server : " + message);
                                signalx.server = eval(message);
                                mailBox.run();
                            } catch (e) {
                                signalx.error.f({
                                    error: e,
                                    description: "Error downloading script from server"
                                });
                                throw e;
                            }
                        };

                        chat.client.broadcastMessage = function (owner, message) {
                            //debug
                            debuging("successfully received server message meant for  " + owner + " handler . Message is : " + message);

                            var own = signalx.waitingList.w[owner];

                            if (!own) {
                                debuging("Could not find any defined callback for " + owner);
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
                                    own && own(message);
                                }
                            } catch (e) {
                                signalx.error.f({
                                    error: e,
                                    description: "Error while running client handler " + owner + " with the server message " + message
                                });
                            }
                        };
                        var promise = $.connection.hub.start();
                        promise.done(function () {
                            //debug
                            debuging("signalr hub started successfully. Now loading signalx script from hub");
                            chat.server.getMethods();
                        }).fail(function (e) {
                            signalx.error.f({
                                error: e,
                                description: "Error requesting client script from server"
                            });
                        });
                    });
                }
            });
        }
    };
    window.signalx = signalx;
    setTimeout(function () {
        signalx.ready(function () {
            for (var key in handlers) {
                if (handlers.hasOwnProperty(key)) {
                    //debug
                    debuging("Client handlers registered : " + key);
                }
            }

            //debug
            debuging("signalx is all set to start reactive server - client server communications");
        });
    }, 0);
    signalx.client.signalx_error = function (message) {
        debuging(message);
        signalx.error.f({
            error: message,
            description: "Error occured on the server"
        });
    };
}(window.jQuery, window));