namespace SignalXLib.TestHelperLib
{
    using SignalXLib.Lib;
    using System;

    public class IsolationContainer : MarshalByRefObject
    {
        public Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> MethodBody { set; get; }

        public void DoSomething()
        {
            SignalX sx = SignalX.Instance();
            if (this.MethodBody != null)
            {
                SignalXTester.Run(sx, this.MethodBody?.Invoke(sx, new SignalXAssertionLib()));
            }
        }
    }
}