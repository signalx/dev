namespace SignalXLib.TestHelperLib
{
    using SignalXLib.Lib;
    using System;

    public class IsolationFacade : MarshalByRefObject
    {
        public Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> a { set; get; }

        public void DoSomething()
        {
            SignalX sx = SignalX.Instance();
            if (this.a != null)
            {
                SignalXTestDefinition defa = this.a?.Invoke(sx, new SignalXAssertionLib());
                SignalXTester.Run(sx, defa);
            }
        }
    }
}