// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open Owin
open Microsoft.Owin
open SignalXLib.Lib
open SignalXLib.Lib.Extensions
open Microsoft.Owin.Hosting

type public Startup() =
        member x.Configuration (app:IAppBuilder) =
                                      app.UseSignalX() |> ignore
                                      app.UseSignalXFileSystem() |> ignore

[<EntryPoint>]
let main argv =
    let url="http://localhost:44111"
    use server=WebApp.Start<Startup>(url)
    SignalX.Server("Sample",fun request -> request.RespondToUser("Myclient",request.Message ))
    SignalX.Server("Sample2",fun message sender replyTo -> SignalX.RespondToAll(replyTo,(message.ToString()+sender.ToString())))
    SignalX.Server("Sample3",fun request -> request.RespondToUser(request.ReplyTo,""))
    System.Diagnostics.Process.Start(url)
    Console.ReadLine()
    0 // return an integer exit code