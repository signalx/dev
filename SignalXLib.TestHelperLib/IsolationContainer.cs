namespace SignalXLib.TestHelperLib
{
    using System;
    using SignalXLib.Lib;

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
            sx.Settings.ClientLogScriptFunction = @"
(function(html){
var appendHtml= function (el, str) {
  var div = document.createElement('div');
  div.innerHTML = str;
  while (div.children.length > 0) {
    el.appendChild(div.children[0]);
  }
}
 appendHtml(document.body,html);
})";
            sx.SetUpClientErrorMessageHandler(
                (error, request) => { throw new Exception($"Failed because an error has occurred on the client side script during a test '{this.TestName.Replace("_", " ")}'  {error}"); });
            var exceptionTracker = new ExceptionTracker
            {
                Exception = null
            };
            sx.OnException(
                (m, e) =>
                {
                    exceptionTracker.Exception = new Exception(m, e);
                    exceptionTracker.Context = m;
                });
            if (this.MethodBody != null)
                SignalXTester.Run(exceptionTracker, sx, this.MethodBody?.Invoke(sx, new SignalXAssertionLib()));
            if (this.MethodBody2 != null)
                SignalXTester.Run(exceptionTracker, sx, this.MethodBody2?.Invoke(sx));
        }
    }
}