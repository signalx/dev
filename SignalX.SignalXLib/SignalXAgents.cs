namespace SignalXLib.Lib
{
    using System;
    using Newtonsoft.Json;

    internal class SignalXAgents
    {
        public void SetUpAgents(SignalX SignalX)
        {
            if (SignalX == null)
                throw new ArgumentNullException(nameof(SignalX));
            SignalX.Server(SignalX.SIGNALXCLIENTAGENT,
                (request, state) =>
                {
                    try
                    {
                        var str = request.Message.ToString();
                        var response = JsonConvert.DeserializeObject<ResponseAfterScriptRuns>(str);
                        SignalX.Settings.OnResponseAfterScriptRuns ?.Invoke(response.Result, request, response.Error);
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
                }, requireAuthorization: false, isSingleWriter: false, allowDynamicServerForThisInstance: true);
        }
    }
}