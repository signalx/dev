using Owin;
using SignalXLib.Lib;

namespace ConsoleApplicationCSharp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSignalX( new SignalX(""));
        }
    }
}