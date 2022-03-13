namespace MediaPlayerRPC

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
open MediaPlayerRPC

module MainWindow =
    let settings = JsonValue.Load $"{__SOURCE_DIRECTORY__}./settings.json"
    
    type State =
        { isRunning: bool
          runOnStartup: bool
          hideOnStart: bool }
        
    let init =
        { isRunning = false
          runOnStartup = settings.GetProperty "runOnStartup" |> JsonExtensions.AsBoolean 
          hideOnStart = settings.GetProperty "hideOnStart" |> JsonExtensions.AsBoolean }
        
    type Msg =
        | SwitchRunning
        | SetRunOnStartup
        | HideOnStart
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
        | SetRunOnStartup ->
            { state with runOnStartup = not state.runOnStartup }
        | HideOnStart ->
            { state with hideOnStart = not state.hideOnStart }
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
                    StackPanel.spacing 20
                    StackPanel.children [
                        CheckBox.create [
                            CheckBox.dock Dock.Top
                            CheckBox.content "Run on OS startup"
                            CheckBox.horizontalAlignment HorizontalAlignment.Center
                            CheckBox.verticalAlignment VerticalAlignment.Center
                            CheckBox.isChecked state.runOnStartup
                            CheckBox.margin (2, 0)
                            CheckBox.onClick (fun _ -> dispatch SetRunOnStartup)
                        ]
                        CheckBox.create [
                            CheckBox.dock Dock.Top
                            CheckBox.content "Hide on start"
                            CheckBox.horizontalAlignment HorizontalAlignment.Center
                            CheckBox.verticalAlignment VerticalAlignment.Center
                            CheckBox.isChecked state.hideOnStart
                            CheckBox.margin (2, 0)
                            CheckBox.onClick (fun _ -> dispatch HideOnStart)
                        ]
                    ]
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text $"""{if state.isRunning then "Running" else "Stopped"}"""
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
