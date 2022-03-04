namespace DiscordRPC

open System
open System.Diagnostics
open Constants
open Windows.Foundation

type Info =
    { Title: string
      Artist: string
      Album: string }

module InfoFetcher =
    let getTrackInfo = 
        let info = { Title = "Unknown title"; Artist = "Unknown artist"; Album = "Unknown album" }
        
        let a = (Process.GetProcessesByName "Microsoft.Media.Player") |> Array.isEmpty     
        printfn "%b" a
        
        async {
            
        }
        
        ()
    
    

