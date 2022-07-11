namespace MediaPlayerRPC

open System.IO
open System.Threading.Tasks
open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI.Components.Hosts
open Avalonia.Layout
open Avalonia.Themes.Fluent
open Elmish
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.Controls.ApplicationLifetimes
open FSharp.Data
open FSharp.Data.JsonProvider
open FSharp.Json
open MediaPlayerRPC

module MainWindow =
    let path = $"{__SOURCE_DIRECTORY__}./settings.json"
    let settings = JsonValue.Load $"{__SOURCE_DIRECTORY__}./settings.json"
    
    type State =
        { isRunning: bool
          runOnStartup: bool
          hideOnStart: bool }
        
    let writeToJson (state: State) =
        use writer = new StreamWriter(path)
        task {
            let json = Json.serialize state
            do! writer.WriteLineAsync json
        } |> Task.WaitAll 
        ()
        
    let init =
        { isRunning = false
          runOnStartup = settings.GetProperty "runOnStartup" |> JsonExtensions.AsBoolean 
          hideOnStart = settings.GetProperty "hideOnStart" |> JsonExtensions.AsBoolean }
        
    type Msg =
        | SwitchRunning
        | SetRunOnStartup of bool
        | SetHideOnStart of bool
        | Hide
    
    let update (msg: Msg) (state: State) (windowService: HostWindow) : State =
        match msg with
        | SwitchRunning ->
            let isRunning = not state.isRunning
            printfn $"{isRunning}"
            async {
                match isRunning with
                | true ->
                    printfn "running"
                    while isRunning do Presence.setPresence ()
                | false ->
                    printfn "stopped"
                    Presence.clearPresence ()
            } |> Async.Start
            { state with isRunning = not state.isRunning }
        | SetRunOnStartup x ->
            let newState = { state with runOnStartup = x }
            writeToJson newState
            newState
        | SetHideOnStart x ->
            { state with hideOnStart = x }
        | Hide ->
            windowService.Hide ()
            state
        
    let view (state: State) dispatch =
        DockPanel.create [
            DockPanel.children [
                StackPanel.create [
                    StackPanel.dock Dock.Bottom
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.horizontalAlignment HorizontalAlignment.Center
                    StackPanel.verticalAlignment VerticalAlignment.Center
                    StackPanel.spacing 20
                    StackPanel.margin (0, 20)
                    StackPanel.children [
                        Button.create [
                            Button.dock Dock.Bottom
                            Button.content (if state.isRunning then "Stop" else "Start")
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.horizontalAlignment HorizontalAlignment.Center
                            Button.onClick (fun _ -> dispatch SwitchRunning )
                        ]
                        Button.create [
                            Button.dock Dock.Bottom
                            Button.isVisible state.isRunning
                            Button.content "Hide"
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.horizontalAlignment HorizontalAlignment.Center
                            Button.onClick (fun _ -> dispatch Hide )
                        ]
                    ]
                ]
                StackPanel.create [
                    StackPanel.dock Dock.Bottom
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.horizontalAlignment HorizontalAlignment.Center
                    StackPanel.verticalAlignment VerticalAlignment.Center
                    StackPanel.spacing 60
                    StackPanel.children [
                        ToggleSwitch.create [
                            ToggleSwitch.dock Dock.Top
                            ToggleSwitch.content "Run on OS startup"
                            ToggleSwitch.horizontalAlignment HorizontalAlignment.Center
                            ToggleSwitch.verticalAlignment VerticalAlignment.Center
                            ToggleSwitch.isChecked state.runOnStartup
                            ToggleSwitch.margin (2, 0)
                            ToggleSwitch.onChecked (fun _ -> dispatch (SetRunOnStartup true))
                            ToggleSwitch.onUnchecked (fun _ -> dispatch (SetRunOnStartup false))
                        ]
                        ToggleSwitch.create [
                            ToggleSwitch.dock Dock.Top
                            ToggleSwitch.content "Hide on start"
                            ToggleSwitch.horizontalAlignment HorizontalAlignment.Center
                            ToggleSwitch.verticalAlignment VerticalAlignment.Center
                            ToggleSwitch.isChecked state.hideOnStart
                            ToggleSwitch.margin (2, 0)
                            ToggleSwitch.onChecked (fun _ -> dispatch (SetHideOnStart true))
                            ToggleSwitch.onUnchecked (fun _ -> dispatch (SetHideOnStart false))
                        ]
                    ]
                ]
                
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text (if state.isRunning then "Running" else "Stopped")
                ]
            ]
        ]

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Media Player RPC"
        base.Height <- 400.0
        base.Width <- 400.0

#if DEBUG     
        this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true
#endif

        let update state msg =
            MainWindow.update state msg this

        Elmish.Program.mkSimple (fun () -> MainWindow.init) update MainWindow.view
        |> Program.withHost this
        |> Program.withConsoleTrace
        |> Program.run

        
type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))
       
    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

module Program =
    [<EntryPoint>]
    let main (args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
