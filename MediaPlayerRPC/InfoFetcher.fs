namespace MediaPlayerRPC

open System
open System.Diagnostics
open Windows.Media.Control

type Info =
    { Title: string
      Artist: string
      Album: string
      StartTime: TimeSpan
      EndTime: TimeSpan
      CurrentTime: TimeSpan
      PlaybackStatus: GlobalSystemMediaTransportControlsSessionPlaybackStatus }
    
type PlayerIsOpen =
    | Yes of Info
    | No

module InfoFetcher =
    let getTrackInfo () =
        if not ((Process.GetProcessesByName "Microsoft.Media.Player") |> Array.isEmpty) then
            let session = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult().GetCurrentSession()
            let songInfo = session.TryGetMediaPropertiesAsync().GetAwaiter().GetResult()
            let timelineInfo = session.GetTimelineProperties()
            let statusInfo = session.GetPlaybackInfo()

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
                  PlaybackStatus = statusInfo.PlaybackStatus }
        
            Yes info
        else
            No
