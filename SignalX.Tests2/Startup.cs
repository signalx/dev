using Owin;
using SignalXLib.Lib;

namespace SignalXLib.TestsNuget
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSignalX( new SignalX(""));
        }
    }
}