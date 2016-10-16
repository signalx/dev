using Owin;
using SignalXLib.Lib;

namespace SignalXLib.Tests
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSignalX( new SignalX("/ui"));
        }
    }
}