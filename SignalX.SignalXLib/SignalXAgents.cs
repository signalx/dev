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
                        var response = JsonConvert.DeserializeObject<ResponseAfterScriptRuns>(JsonConvert.SerializeObject(request.Message));
                        SignalX.OnResponseAfterScriptRuns ?.Invoke(response.Result, request, response.Error);
                    }
                    catch (Exception e)
                    {
                        SignalX.OnResponseAfterScriptRuns?.Invoke(null, request, e.Message);
                    }

                    SignalX.OnResponseAfterScriptRuns = null;
                }, false, false, true);
        }
    }
}