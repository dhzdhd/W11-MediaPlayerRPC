namespace MediaPlayerRPC

open LiteDB.FSharp
open LiteDB.FSharp.Extensions
open LiteDB

type Settings =
    { Id: int
      RunOnStartup: bool
      HideOnStart: bool }
    
type AlbumArts =
    { Id: int
      Title: string
      Url: string }

module Database =
    let mapper = FSharpBsonMapper ()
    let settingsDb = new LiteDatabase ("settings.db", mapper)
    let albumArtsDb = new LiteDatabase ("albumarts.db", mapper)
    let settings = settingsDb.GetCollection<Settings> "settings"
    let albumArts = albumArtsDb.GetCollection<AlbumArts> "albumarts"
    
    let getSettings () =
        match (settings.Count() <> 0) with
        | true ->
            let data =
                settings.FindAll ()
                |> Seq.toList
                |> List.head
            // printfn $"getData {data}"
            data
        | false ->
            // printfn "empty"
            { Id = 1; RunOnStartup = false; HideOnStart = false }
        
    let upsertSettings (state: Settings) =
        settings.Upsert state
        
    let getAlbumArts title =
        match (albumArts.Count () <> 0) with
        | true ->
            try
                Some (albumArts.findOne <@ fun album -> album.Title = title @>) 
            with
            | exc -> None
        | false -> None
        
    let upsertAlbumArts (state: AlbumArts) =
        albumArts.Upsert state
