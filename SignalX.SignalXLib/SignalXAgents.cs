namespace SignalXLib.Lib
{
    using System;
    using Newtonsoft.Json;

    class SignalXAgents
    {
        public void SetUpAgents(SignalX SignalX)
        {
            if (SignalX == null)
                throw new ArgumentNullException(nameof(SignalX));
            SignalX.Server(
                SignalX.SIGNALXCLIENTAGENT,
                (request, state) =>
                {
                    try
                    {
                        dynamic str = request.Message.ToString();
                        dynamic response = JsonConvert.DeserializeObject<ResponseAfterScriptRuns>(str);
                        SignalX.Settings.OnResponseAfterScriptRuns?.Invoke(response.Result, request, response.Error);
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
                false,
                false,
                true);

            SignalX.Server(
                SignalX.SIGNALXCLIENTERRORHANDLER,
                (request, state) =>
                {
                    try
                    {
                        dynamic str = request.Message.ToString();

                        SignalX.OnErrorMessageReceivedFromClient?.Invoke(str, request);
                    }
                    catch (Exception e)
                    {
                        SignalX.Settings.ExceptionHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}", e));
                    }
                },
                false,
                false,
                true);

            SignalX.Server(
                SignalX.SIGNALXCLIENTDEBUGHANDLER,
                (request, state) =>
                {
                    try
                    {
                        dynamic str = request.Message.ToString();
                        SignalX.OnDebugMessageReceivedFromClient?.Invoke(str, request);
                    }
                    catch (Exception e)
                    {
                        SignalX.Settings.WarningHandler.ForEach(h => h?.Invoke($"Error while obtaining response from client after server executed script on client : Response was {request?.Message} from sender {request?.Sender}", e));
                    }
                },
                false,
                false,
                true);
        }
    }
}