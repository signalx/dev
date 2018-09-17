namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SignalXLib.Lib;
    using SignalXLib.TestHelperLib;
    using SignalXAppBuilderExtensions = SignalXLib.Lib.Extensions.SignalXAppBuilderExtensions;

    [TestClass]
    public class async_void_test
    {
        [TestMethod]
        public void EnsureNoAsyncVoidTests()
        {
            //  AssertExtensions.AssertNoAsyncVoidMethods(GetType().Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalX).Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalXTester).Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalXAppBuilderExtensions).Assembly);
        }
    }
}