using System.Text;
using System.Threading.Tasks;

namespace SignalXLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    [TestClass]
    public class async_void_test
    {
        [TestMethod]
        public void EnsureNoAsyncVoidTests()
        {
          //  AssertExtensions.AssertNoAsyncVoidMethods(GetType().Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalXLib.Lib.SignalX).Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalXLib.TestHelperLib.SignalXTester).Assembly);
            AssertExtensions.AssertNoAsyncVoidMethods(typeof(SignalXLib.Lib.Extensions.SignalXAppBuilderExtensions).Assembly);
        }
    }
}
