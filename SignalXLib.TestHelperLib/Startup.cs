namespace SignalXLib.TestHelperLib
{
    using Owin;
    using SignalXLib.Lib;
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