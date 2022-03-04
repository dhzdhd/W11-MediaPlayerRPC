namespace DiscordRPC

open System
open System.Diagnostics
open Windows.Media.Control

type Info =
    { Title: string
      Artist: string
      Album: string }
    
type PlayerIsOpen =
    | Yes of Info
    | No

module InfoFetcher =
    let getTrackInfo () =
        if not ((Process.GetProcessesByName "Microsoft.Media.Player") |> Array.isEmpty) then
            let result = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult().GetCurrentSession().TryGetMediaPropertiesAsync().GetAwaiter().GetResult()
            let info =
                { Title =
                      match result.Title with
                      | "" -> "Unknown Song"
                      | x -> x
                  Artist =
                      match result.AlbumArtist with
                      | "" -> "Unknown Artist"
                      | x -> x
                  Album =
                      match result.AlbumTitle with
                      | "" -> "Unknown Album"
                      | x -> x }
            printfn $"{info.Album} {info.Artist} {info.Title}"
        
            Yes info
        else
            No
