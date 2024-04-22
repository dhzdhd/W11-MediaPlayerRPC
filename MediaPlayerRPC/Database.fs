namespace MediaPlayerRPC

open LiteDB.FSharp
open LiteDB

type Settings =
    { Id: int
      RunOnStartup: bool
      HideOnStart: bool }

module Database =
    let mapper = FSharpBsonMapper ()
    let db = new LiteDatabase ("settings.db", mapper)
    let settings = db.GetCollection<Settings> "settings"
    
    let getData () =
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
        
    let insertData (state: Settings) =
        let res = settings.Upsert state
        printfn $"{res}"