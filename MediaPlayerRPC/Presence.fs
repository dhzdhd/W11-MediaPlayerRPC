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
    let mutable oldState = No
    
    let onReadyEventHandler (_: ReadyMessage) =
        printfn "Client ready"
    
    client.Logger <- ConsoleLogger (LogLevel.Warning, true)
    client.OnReady.Add onReadyEventHandler
    client.SkipIdenticalPresence <- true
    
    match client.Initialize () with
    | true -> printfn "Connection initialised successfully"
    | false ->
        printfn "Connection failed!"
        exit 0
    
    assets.LargeImageKey <- "icon"
    button.Label <- "Play on YouTube"
    presence.Assets <- assets
    
    let clearPresence () =
        client.ClearPresence ()

    let setPresence () =
        match getTrackInfo () with
        | Yes res ->
            // match oldState with
            // | Yes old ->
            //     match old.EndTime = res.EndTime with
            //     | true ->
            //         if res.CurrentTime = TimeSpan.Zero then
            //             presence.Timestamps <- Timestamps.FromTimeSpan res.CurrentTime
            //     | false ->
            //         oldState <- getTrackInfo () 
            //         presence.Timestamps <- Timestamps.FromTimeSpan res.CurrentTime
            // | No ->
            //     oldState <- getTrackInfo () 
            //     presence.Timestamps <- Timestamps.FromTimeSpan res.CurrentTime
            
            // if (res.CurrentTime +  TimeSpan.FromSeconds 10) > res.EndTime then
            presence.Timestamps <- Timestamps.FromTimeSpan res.RemainingTime
                
            presence.Details <- $"{res.Artist} - {res.Title}"
            presence.Buttons <- [| button |]
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
        | No ->
            presence.Details <- "Idle"
            assets.SmallImageKey <- "idle"
            assets.SmallImageText <- "Idle"
            
            presence.Buttons <- [||]
            
            client.SetPresence presence
