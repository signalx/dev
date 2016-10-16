using Microsoft.AspNet.SignalR;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace SignalXLib.Lib
{
    public static class SignalXAppBuilderExtensions
    {
        public static void UseSignalX(
            this IAppBuilder app, SignalX signalXOptions=null)
        {
            signalXOptions = signalXOptions ?? new SignalX(new HubConfiguration() { EnableDetailedErrors = true});
            // Turn cross domain on 
            var hubConfig = signalXOptions.HubConfiguration;// new HubConfiguration {EnableDetailedErrors = true, EnableJSONP = true};

            app.MapSignalR(hubConfig);

            if (signalXOptions.UiFolder==null) return;
            var fileSystem = new PhysicalFileSystem(signalXOptions.UiFolder);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };

            app.UseFileServer(options);
        }
    }
}