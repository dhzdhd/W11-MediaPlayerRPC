namespace MediaPlayerRPC

open System
open System.Diagnostics
open type Windows.Media.MediaPlaybackAutoRepeatMode
open Windows.Media.Control
open FsHttp
open Windows.Media.Playback

type Info =
    { Title: string
      Artist: string
      Album: string
      StartTime: TimeSpan
      EndTime: TimeSpan
      CurrentTime: TimeSpan
      RemainingTime: TimeSpan
      RepeatMode: Windows.Media.MediaPlaybackAutoRepeatMode option
      PlaybackStatus: GlobalSystemMediaTransportControlsSessionPlaybackStatus }
    
type PlayerIsOpen =
    | Yes of Info
    | No

module InfoFetcher =
    let isPlayerOpen () : bool =
        not ((Process.GetProcessesByName "Microsoft.Media.Player")
        |> Array.isEmpty)

    let getAlbumArt search =
        http {
            GET "https://itunes.apple.com/search"
            query
                [ "term", search
                  "limit", "1"
                  "country", "us"
                  "entity", "song"
                  "media", "music" ]
        }
        |> Request.sendAsync
        |> Async.RunSynchronously
        |> Response.toJson
        |> fun json ->
                match json?resultCount.GetInt32() with
                | 0 -> Option.None
                | _ -> Some (json?results.[0]?artworkUrl100.GetString())
        
    let getTrackInfo () =
        let session = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult().GetCurrentSession()
        
        if isPlayerOpen () && session.SourceAppUserModelId.Contains "Microsoft.ZuneMusic" then
            let songInfo = session.TryGetMediaPropertiesAsync().GetAwaiter().GetResult()
            let timelineInfo = session.GetTimelineProperties()
            let statusInfo = session.GetPlaybackInfo()
            
            printfn $"{timelineInfo.LastUpdatedTime}"
            printfn $"{songInfo.Title}, {songInfo.Artist}, {songInfo.AlbumArtist}, {songInfo.AlbumTitle} {songInfo.PlaybackType.Value}"

            let info =
                { Title =
                      match songInfo.Title with
                      | "" -> "Unknown Song"
                      | x -> x
                  Artist =
                      match songInfo.Artist with
                      | "" ->
                          match songInfo.AlbumArtist with
                          | "" -> "Unknown Artist"
                          | x -> x
                      | x -> x
                  Album =
                      match songInfo.AlbumTitle with
                      | "" -> "Unknown Album"
                      | x -> x
                  StartTime = timelineInfo.StartTime
                  EndTime = timelineInfo.EndTime
                  CurrentTime = timelineInfo.Position
                  RemainingTime = timelineInfo.EndTime - timelineInfo.Position
                  RepeatMode = Option.ofNullable statusInfo.AutoRepeatMode
                  PlaybackStatus = statusInfo.PlaybackStatus }
        
            Yes info
        else
            No
