namespace SignalXLib.Lib
{
    using System;
    using Newtonsoft.Json;

    class SignalXAgents
    {
        public void SetUpAgents(SignalX SignalX)
        {
            SignalX?.Advanced.Trace("Setting up agents ...");
            if (SignalX == null)
                throw new ArgumentNullException(nameof(SignalX));

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTREADY} ...");
            SignalX.Server(
                SignalX.SIGNALXCLIENTREADY,
                (request, state) =>
                {
                    foreach (Action<SignalXRequest> action in SignalX.OnClientReady)
                        try
                        {
                            action.Invoke(request);
                            request.RespondToSender("");
                        }
                        catch (Exception e)
                        {
                            string error = $"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}";
                            SignalX.Advanced.Trace(e, error);
                            if (SignalX.Settings.ContinueClientExecutionWhenAnyServerOnClientReadyFails)
                                request.RespondToSender("Error while executing server ready, but server has allowed client to continue execution regardless");
                            else
                                SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke(error, e));
                        }
                },
                requireAuthorization: false,
                isSingleWriter: false,
                allowDynamicServerForThisInstance: true);

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTAGENT} ...");
            SignalX.Server(
                SignalX.SIGNALXCLIENTAGENT,
                (request, state) =>
                {
                    try
                    {
                        dynamic str = request.Message.ToString();
                        dynamic response = JsonConvert.DeserializeObject<ResponseAfterScriptRuns>(str);
                        SignalX.OnResponseAfterScriptRuns?.Invoke(response.Result, request, response.Error);
                        /*
                          todo check why this is not working as dynamic
                          todo maybe needs to specify/check serialization for hub
                          SignalX.OnResponseAfterScriptRuns ?.Invoke(request.Message.Result, request, request.Message.Error);

                         */
                    }
                    catch (Exception e)
                    {
                        SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}", e));
                    }

                    //removed coz of possibility of result aggregation from clients
                    // SignalX.OnResponseAfterScriptRuns = null;
                },
                requireAuthorization: false,
                isSingleWriter: false,
                allowDynamicServerForThisInstance: true);

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTERRORHANDLER} ...");
            SignalX.Server(
                SignalX.SIGNALXCLIENTERRORHANDLER,
                (request, state) =>
                {
                    foreach (Action<string, SignalXRequest> action in SignalX.OnErrorMessageReceivedFromClient)
                        try
                        {
                            dynamic str = request.Message.ToString();

                            action?.Invoke(str, request);
                        }
                        catch (Exception e)
                        {
                            SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}", e));
                        }
                },
                requireAuthorization: false,
                isSingleWriter: false,
                allowDynamicServerForThisInstance: true);

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTDEBUGHANDLER} ...");
            SignalX.Server(
                SignalX.SIGNALXCLIENTDEBUGHANDLER,
                (request, state) =>
                {
                    foreach (Action<string, SignalXRequest> action in SignalX.OnDebugMessageReceivedFromClient)
                        try
                        {
                            dynamic str = request.Message.ToString();
                            action?.Invoke(str, request);
                        }
                        catch (Exception e)
                        {
                            SignalX.Settings.WarningHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}", e));
                        }
                },
                requireAuthorization: false,
                isSingleWriter: false,
                allowDynamicServerForThisInstance: true);
        }
    }
}