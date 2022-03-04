namespace DiscordRPC

open System
open System.Threading
open DiscordRPC
open DiscordRPC.Logging
open DiscordRPC.Message
open InfoFetcher

module Main =
    let onReadyEventHandler (_: ReadyMessage) =
        printfn "Ready"
        ()
        
    let rec setPresence (client: DiscordRpcClient) =
        let assets = Assets ()
        assets.LargeImageKey <- "icon"
        
        let button = Button ()
        button.Label <- "Watch on YouTube"
        button.Url <- "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
        
        let presence = RichPresence ()
        presence.Timestamps <- Timestamps (DateTime.Now, Nullable DateTime.UtcNow)
        presence.Assets <- assets
        presence.Buttons <- [| button |]
        
        
        match getTrackInfo () with
        | Yes res ->
            presence.Details <- res.Title
            assets.LargeImageText <- $"{res.Album} - {res.Artist}"
            
            client.SetPresence presence
        | No -> ()
        
        Async.Sleep 3000 |> Async.RunSynchronously
        setPresence client
        
    let checkInitialized (client: DiscordRpcClient) =
        match client.IsInitialized with
        | true -> setPresence client
        | false -> printfn "Stopped"
    
    [<EntryPoint>]
    let main argv = 
        let mutable client = new DiscordRpcClient "948624514546270208" 
        client.Logger <- ConsoleLogger (LogLevel.Warning, true)
        client.OnReady.Add onReadyEventHandler
        
        match client.Initialize () with
        | true -> printfn "success"
        | false -> printfn "failure"
        
        checkInitialized client
        
        0
