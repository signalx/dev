
signalx.client.myclient = function(message) {
    console.log(message);
};

signalx.ready(function (server) {
    server.sample("RR 1 Hi, its sam");
});
signalx.ready(function (server) {
    server.sample("RR 1 Hi, its sam");
});

try {
    signalx.server.sample("fhj fhfjh jgk jg");
} catch (e) {

}

setTimeout(function() {
    signalx.server.sample("fhj fhfjh jgk jg");
}, 5000);