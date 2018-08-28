namespace SignalXLib.TestHelperLib
{
    using SignalXLib.Lib;
    using System;

    public class IsolationContainer : MarshalByRefObject
    {
        public Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> MethodBody { set; get; }
        public Func<SignalX, SignalXTestDefinition> MethodBody2 { set; get; }

        public void DoSomething()
        {
            SignalX sx = SignalX.Instance;
            sx.Settings.LogAgentMessagesOnClient = true;
            if (this.MethodBody != null )
            {
                SignalXTester.Run(sx, (this.MethodBody)?.Invoke(sx, new SignalXAssertionLib()));
            }
            if (this.MethodBody2 != null)
            {
                SignalXTester.Run(sx, (this.MethodBody2)?.Invoke(sx));
            }
        }
    }
}