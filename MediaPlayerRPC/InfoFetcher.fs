namespace MediaPlayerRPC

open System
open System.Diagnostics
open type Windows.Media.MediaPlaybackAutoRepeatMode
open Windows.Media.Control

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
    let session = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult().GetCurrentSession()

    let isPlayerOpen () : bool =
        not ((Process.GetProcessesByName "Microsoft.Media.Player")
        |> Array.isEmpty)
    
    let getTrackInfo () =
        if isPlayerOpen () then
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
