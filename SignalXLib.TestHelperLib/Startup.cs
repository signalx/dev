using Owin;
using SignalXLib.Lib;

namespace SignalXLib.Tests
{
    using SignalXLib.Lib.Extensions;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseSignalX();
            app.UseSignalXFileSystem();
        }
    }
}