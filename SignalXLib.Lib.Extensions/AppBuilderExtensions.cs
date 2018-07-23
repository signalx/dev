﻿namespace SignalXLib.Lib.Extensions
{
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using System;

    public static class SignalXAppBuilderExtensions
    {
        public static string UseSignalXFileSystem(
            this IAppBuilder app, string folder = "")
        {
            var uiFolder = AppDomain.CurrentDomain.BaseDirectory + folder;
            var fileSystem = new PhysicalFileSystem(uiFolder);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };
            app.UseFileServer(options);
            return uiFolder;
        }
    }
}