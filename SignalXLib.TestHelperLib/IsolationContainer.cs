namespace SignalXLib.TestHelperLib
{
    using SignalXLib.Lib;
    using System;

    public class IsolationContainer : MarshalByRefObject
    {
        public Func<SignalX, SignalXAssertionLib, SignalXTestDefinition> MethodBody { set; get; }
        public Func<SignalX, SignalXTestDefinition> MethodBody2 { set; get; }

        public string TestName { get; set; }

        public void DoSomething()
        {
            SignalX sx = SignalX.Instance;
            sx.Settings.ReceiveErrorMessagesFromClient = true;
            sx.Settings.ReceiveDebugMessagesFromClient = true;
            sx.Settings.ManageUserConnections = true;
            sx.Settings.LogAgentMessagesOnClient = true;
            sx.SetUpClientErrorMessageHandler((error, request) =>
            {
                throw new Exception($"Failed because an error has occurred on the client side script during a test '{TestName.Replace("_"," ")}'  {error}");
            });
            ExceptionTracker exceptionTracker= new ExceptionTracker()
            {
                Exception=null
            };
            sx.OnException(
                (e) =>
                {
                    exceptionTracker.Exception= e;
                });
            if (this.MethodBody != null )
            {
                SignalXTester.Run( exceptionTracker,  sx, (this.MethodBody)?.Invoke(sx, new SignalXAssertionLib()));
            }
            if (this.MethodBody2 != null)
            {
                SignalXTester.Run(exceptionTracker,sx, (this.MethodBody2)?.Invoke(sx));
            }
        }
    }

    public class ExceptionTracker
    {
        public Exception Exception { get; set; }
    }
}