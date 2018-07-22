using Owin;
using SignalXLib.Lib;

namespace SignalXLib.TestsNuget
{
    using SignalXLib.Lib.Extensions;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSignalX( new SignalX());
            app.UseSignalXFileSystem("");
        }
    }
}