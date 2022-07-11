namespace MediaPlayerRPC

open System
open DiscordRPC
open DiscordRPC.Logging
open DiscordRPC.Message
open InfoFetcher
open Windows.Media.Control

module Presence =
    let client = new DiscordRpcClient "948624514546270208"
    let assets = Assets ()
    let button = Button ()
    let presence = RichPresence ()
    
    let onReadyEventHandler (_: ReadyMessage) =
        printfn "Client ready"
    
    client.Logger <- ConsoleLogger (LogLevel.Warning, true)
    client.OnReady.Add onReadyEventHandler
    client.SkipIdenticalPresence <- true
    
    match client.Initialize () with
    | true -> printfn "Connection initialised successfully"
    | false -> printfn "Connection failed!"
    
    assets.LargeImageKey <- "icon"
    
    button.Label <- "Play on YouTube"
            
    presence.Assets <- assets
    presence.Buttons <- [| button |]

    let clearPresence () =
        client.Deinitialize ()

    let setPresence () =
        match getTrackInfo () with
        | Yes res ->
            try
                client.Initialize ()
            with
                | :? DiscordRPC.Exceptions.UninitializedException -> true
            |> ignore 
            
            printfn $"{DateTime.Now + res.StartTime}"
//            presence.Timestamps <- Timestamps ( DateTime.UtcNow + res.CurrentTime)
            presence.Details <- $"{res.Artist} - {res.Title}"
            assets.LargeImageText <- $"{res.Album}"
            button.Url <- $"https://www.youtube.com/results?search_query={res.Artist.Replace (' ', '+')}+{res.Title.Replace (' ', '+')}"
            
            match res.PlaybackStatus with
            | GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing ->
                assets.SmallImageKey <- "play"
                assets.SmallImageText <- "Playing"
            | GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused ->
                assets.SmallImageKey <- "pause"
                assets.SmallImageText <- "Paused"
            | _ -> ()
            
            client.SetPresence presence
        | No -> clearPresence ()
        
        Async.Sleep 1000 |> Async.RunSynchronously
