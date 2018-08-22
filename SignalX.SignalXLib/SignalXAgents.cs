namespace SignalXLib.Lib
{
    using System;
    using Newtonsoft.Json;

    internal class SignalXAgents
    {
        public SignalXAgents()
        {
            SignalX.Server(SignalX.SIGNALXCLIENTAGENT,
                (request, state) =>
                {
                    try
                    {
                        var str = request.Message.ToString();
                        var response = JsonConvert.DeserializeObject<ResponseAfterScriptRuns>(str);
                        SignalX.OnResponseAfterScriptRuns ?.Invoke(response.Result, request, response.Error);
                        /*
                          todo check why this is not working as dynamic
                          todo maybe needs to specify/check serialization for hub 
                          SignalX.OnResponseAfterScriptRuns ?.Invoke(request.Message.Result, request, request.Message.Error);
                   
                         */

                    }
                    catch (Exception e)
                    {
                        SignalX.OnResponseAfterScriptRuns?.Invoke(null, request, e.Message);
                    }
                    //removed coz of possibility of result aggregation from clients
                   // SignalX.OnResponseAfterScriptRuns = null;
                }, false, false, true);
        }
    }
}