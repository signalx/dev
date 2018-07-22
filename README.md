![sxs](https://cloud.githubusercontent.com/assets/2102748/18841414/a912f0bc-83df-11e6-81ca-608ac62cac47.png) 

[![NuGet version](https://badge.fury.io/nu/signalx.svg)](https://badge.fury.io/nu/signalx)

[![npm version](https://badge.fury.io/js/signalx.svg)](https://badge.fury.io/js/signalx)

[![Bower version](https://badge.fury.io/bo/signalx.svg)](https://badge.fury.io/bo/signalx)

# SignalX
Simplifying sigalr front and backend  setups

No more worrying about setup, cases, etc, just simple javascript to .NET lambda as a server


Backend (F#) :-

	SignalX.Server("Sample",fun request -> request.RespondToAll("response"))	
	
Backend (C#) :-

	SignalX.Server("Sample",request => request.RespondToAll("response"))	
	
	
FrontEnd :-
    
    signalx.server.sample("Hey",function(response){ console.log(response);});

	
	
Download the simple complete linqpad samples:

Run linqpad as administrator to avoid getting exception such as  "Access to the path 'C:\Program Files (x86)\LINQPad5\index.html' is denied."

https://signalx.github.io/LinqPadSamples/signalx_callback.linq

https://signalx.github.io/LinqPadSamples/signalx_handler.linq

https://signalx.github.io/LinqPadSamples/signalx_promise.linq

https://signalx.github.io/LinqPadSamples/signalx_registering_server_on_client.linq
	

you can download linqpad here https://www.linqpad.net/

	
	
	
	
MORE INFORMATION
==================================================================

Backend (F#) :-

    open System
    open Owin
    open Microsoft.Owin
    open SignalXLib.Lib
    open Microsoft.Owin.Hosting
	
    type public Startup() =
        member x.Configuration (app:IAppBuilder) = app.UseSignalX( new SignalX()) |> ignore
		
    [<EntryPoint>]
    let main argv = 
    let url="http://localhost:44111"
    use server=WebApp.Start<Startup>(url)
	SignalX.Server("Sample",fun request -> request.RespondToAll(request.ReplyTo))	
	
	

Backend (C#) :-

    public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseSignalX(new SignalX());
		}
	}
	internal class Program
	{
		private static void Main(string[] args)
		{
			var url = "http://localhost:44111";
			using (WebApp.Start<Startup>(url))
			{
			   SignalX.Server("Sample", (request) => request.RespondToAll(request.ReplyTo));
			}
		}
	}
	
FrontEnd :-
	
Include scripts
----------------------------------------------------------------

      <script src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js"></script>     
      <script src="https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js"></script>
      <script src="https://unpkg.com/signalx"></script>


Report debug information
=========================================================

      signalx.debug(function (o) { console.log(o); });
      signalx.error(function (o) { console.log(o); });
 
Do things only when connection is ready
=========================================================
 
    signalx.ready(function (server) {
      console.log("signalx is ready");
    });
 
    signalx.ready(function (server) {
       server.sample("GetSomething",function(something){ console.log(something);});
    });
 
Do things from anywhere, specify a callback
=========================================================

    signalx.server.sample("GetSomething",function(something){ console.log(something);});
 
Register handler
=========================================================

    signalx.server.sample("GetSomething","getSomethingCompleted");
 
    signalx.client.getSomethingCompleted = function (something) {
        console.log(something);
     };
 
 
Return a promise
=========================================================

    var getSomethingCompletedPromise = signalx.server.sample("GetSomething");
 
    getSomethingCompletedPromise.done(function (something) {
        console.log(something);
    });
 
 
 
 
 
 
 
 
