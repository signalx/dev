using Microsoft.VisualStudio.TestTools.UnitTesting;

//https://blogs.msdn.microsoft.com/devops/2018/01/30/mstest-v2-in-assembly-parallel-test-execution/
[assembly: Parallelize(Workers = 20, Scope = ExecutionScope.MethodLevel)]

namespace SignalXLib.Tests
{
    using System;
    using System.IO;
    using SignalXLib.TestHelperLib;

    public class ScriptSource
    {
        public static Func<ScriptLibraries, string, string> ScriptDownLoadFunction = (script, cdn) =>
        {
            string jquery = "jquery-1.9.0.min.js";
            string signalr = "jquery.signalr-2.2.0.min.js";
            string baseDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName + "\\SignalX.JS\\";
            switch (script)
            {
                case ScriptLibraries.JQUERY:
                    return File.ReadAllText(baseDir + $"lib\\{jquery}");

                case ScriptLibraries.SIGNALR:
                    return File.ReadAllText(baseDir + $"lib\\{signalr}");

                case ScriptLibraries.SIGNALX:
                    return File.ReadAllText(baseDir + "index.js");

                default:
                    throw new FileLoadException("Cannot find file for " + cdn);
            }
        };
    }
}