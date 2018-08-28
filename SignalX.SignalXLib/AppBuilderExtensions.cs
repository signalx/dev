﻿using Microsoft.AspNet.SignalR;

using Owin;

namespace SignalXLib.Lib
{
    using System;

    public static class SignalXAppBuilderExtensions
    {
        static SignalX SignalX = SignalX.Instance;
        /// <summary>
        /// If you already did MapSignalR in your app, then you done need to use this!
        /// </summary>
        public static void UseSignalX(
            this IAppBuilder app)
        {
            // Turn cross domain on
            var hubConfig = SignalX.Settings.HubConfiguration;// new HubConfiguration {EnableDetailedErrors = true, EnableJSONP = true};

            try
            {
                GlobalHost.HubPipeline.AddModule(new SignalXHub.SignalXHubPipelineModule());
            }
            catch (Exception e)
            {
                SignalX.Settings.ExceptionHandler.ForEach(h=>h?.Invoke("Unable to add SignalXHubPipelineModule to HubPipeline module. Possibly because it has already been added previously. See exception for more details", e));
            }

            app.MapSignalR(hubConfig);

            // if (signalXOptions.UiFolder == null) return;
        }
    }
}