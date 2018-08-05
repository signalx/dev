var $sx = {
    myTestServerHandler609adb6628184fa8b1abad76c785afc5: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('myTestServerHandler609adb6628184fa8b1abad76c785afc5', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    GroupWatcher: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('GroupWatcher', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    MyTestServerHandler390f1ff448cd146bebaf61ddee4aa7cd0: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('MyTestServerHandler390f1ff448cd146bebaf61ddee4aa7cd0', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    MyTestServerHandler2f380fb3460fe48b380b0388b178842a8: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('MyTestServerHandler2f380fb3460fe48b380b0388b178842a8', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    myTestServerHandler43c13ad01141646dd893cae02c1227416: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('myTestServerHandler43c13ad01141646dd893cae02c1227416', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    myServerHandler08efac7cc20342f09d594a2ee9f23620: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('myServerHandler08efac7cc20342f09d594a2ee9f23620', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    myTestServerHandler2f380fb3460fe48b380b0388b178842a8: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('myTestServerHandler2f380fb3460fe48b380b0388b178842a8', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    groupWatcher: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('groupWatcher', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    MyServerHandler08efac7cc20342f09d594a2ee9f23620: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('MyServerHandler08efac7cc20342f09d594a2ee9f23620', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    MyTestServerHandler43c13ad01141646dd893cae02c1227416: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('MyTestServerHandler43c13ad01141646dd893cae02c1227416', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    groupWatcher2: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('groupWatcher2', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    MyTestServerHandler609adb6628184fa8b1abad76c785afc5: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('MyTestServerHandler609adb6628184fa8b1abad76c785afc5', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    myTestServerHandler390f1ff448cd146bebaf61ddee4aa7cd0: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('myTestServerHandler390f1ff448cd146bebaf61ddee4aa7cd0', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
    GroupWatcher2: function (m, repTo, sen, msgId) {
        var deferred = $.Deferred();
        window.signalxidgen = window.signalxidgen || function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        };
        window.signalxid = window.signalxid || window.signalxidgen();
        sen = sen || window.signalxid;
        repTo = repTo || '';
        var messageId = window.signalxidgen();
        var rt = repTo;
        if (typeof repTo === 'function') {
            signalx.waitingList(messageId, repTo);
            rt = messageId;
        }
        if (!repTo) {
            signalx.waitingList(messageId, deferred);
            rt = messageId;
        }
        chat.server.send('GroupWatcher2', m, rt, sen, messageId);
        if (repTo) {
            return messageId
        } else {
            return deferred.promise();
        }
    },
};
$sx;