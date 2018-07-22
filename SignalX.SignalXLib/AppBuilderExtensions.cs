using Microsoft.AspNet.SignalR;

using Owin;

namespace SignalXLib.Lib
{
    public static class SignalXAppBuilderExtensions
    {
        public static void UseSignalX(
            this IAppBuilder app, SignalX signalXOptions = null)
        {
            signalXOptions = signalXOptions ?? new SignalX(new HubConfiguration() { EnableDetailedErrors = true });
            // Turn cross domain on 
            var hubConfig = signalXOptions.HubConfiguration;// new HubConfiguration {EnableDetailedErrors = true, EnableJSONP = true};

            GlobalHost.HubPipeline.AddModule(new SignalXHub.SignalrErrorHandler());
            app.MapSignalR(hubConfig);

           // if (signalXOptions.UiFolder == null) return;
          
        }
    }
}