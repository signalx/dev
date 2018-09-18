namespace SignalXLib.Lib
{
    using Newtonsoft.Json;
    using System;

    internal class SignalXAgents
    {
        public void SetUpAgents(SignalX SignalX)
        {
            SignalX?.Advanced.Trace("Setting up agents ...");
            if (SignalX == null)
                throw new ArgumentNullException(nameof(SignalX));

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTREADY} ...");
            SignalX.Server(
                ServerType.Dynamic,
                SignalX.SIGNALXCLIENTREADY,
                (request, state) =>
                {
                    SignalX?.Advanced.Trace($"Running all client ready handlers  '{request?.MessageAsJsonString}'  ...");
                    foreach (Action<SignalXRequest> action in SignalX.OnClientReady)
                    {
 try
                        {
                            action.Invoke(request);
                            request.RespondToSender("");
                        }
                        catch (Exception e)
                        {
                            string error = $"Error while obtaining response from client after server executed script on client : Response was {request?.MessageAsJsonString} from sender {request?.Sender}";
                            SignalX.Advanced.Trace(e, error);
                            if (SignalX.Settings.ContinueClientExecutionWhenAnyServerOnClientReadyFails)
                                request.RespondToSender("Error while executing server ready, but server has allowed client to continue execution regardless");
                            else
                                SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke(error, e));
                        }
                    }
                       
                });

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTAGENT} ...");
            SignalX.Server(
                ServerType.Dynamic,
                SignalX.SIGNALXCLIENTAGENT,
                (request, state) =>
                {
                    SignalX?.Advanced.Trace($"Received message from client agent '{request?.MessageAsJsonString}' ...");
                    try
                    {
                        var response = request.MessageAs<ResponseAfterScriptRuns>();
                        response.Request = request;
                        SignalX.OnResponseAfterScriptRuns?.Invoke(response);
                        
                    }
                    catch (Exception e)
                    {
                        SignalX.Advanced.Trace(e, $"Error while handling client agent with message {request?.MessageAsJsonString}...");
                        SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.MessageAsJsonString} from sender {request?.Sender}", e));
                    }

                    //removed coz of possibility of result aggregation from clients
                    // SignalX.OnResponseAfterScriptRuns = null;
                });

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTERRORHANDLER} ...");
            SignalX.Server(
                ServerType.Dynamic,
                SignalX.SIGNALXCLIENTERRORHANDLER,
                (request, state) =>
                {
                    SignalX?.Advanced.Trace($"Running client error handlers with message '{request?.MessageAsJsonString}' ...");

                    foreach (Action<string, SignalXRequest> action in SignalX.OnErrorMessageReceivedFromClient)
                        try
                        {
                            action?.Invoke(request.MessageAsJsonString, request);
                        }
                        catch (Exception e)
                        {
                            SignalX.Advanced.Trace(e);
                            SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.MessageAsJsonString} from sender {request?.Sender}", e));
                        }
                });

            SignalX.Advanced.Trace($"Setting up agent {SignalX.SIGNALXCLIENTDEBUGHANDLER} ...");
            SignalX.Server(
                ServerType.Dynamic,
                SignalX.SIGNALXCLIENTDEBUGHANDLER,
                (request, state) =>
                {
                    SignalX?.Advanced.Trace($"Running client debug handlers with message '{request?.MessageAsJsonString}' ...");

                    foreach (Action<string, SignalXRequest> action in SignalX.OnDebugMessageReceivedFromClient)
                    {
                    try
                        {
                            action?.Invoke(request.MessageAsJsonString, request);
                        }
                        catch (Exception e)
                        {
                            SignalX.Advanced.Trace(e);
                            SignalX.Settings.WarningHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.MessageAsJsonString} from sender {request?.Sender}", e));
                        }
                    }

                });
        }
    }
}