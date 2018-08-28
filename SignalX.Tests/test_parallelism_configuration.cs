using Microsoft.VisualStudio.TestTools.UnitTesting;

//https://blogs.msdn.microsoft.com/devops/2018/01/30/mstest-v2-in-assembly-parallel-test-execution/
[assembly: Parallelize(Workers = 100, Scope = ExecutionScope.MethodLevel)]
namespace SignalXLib.Tests
{
}