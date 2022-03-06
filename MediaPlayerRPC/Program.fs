namespace MediaPlayerRPC

open Avalonia
open Avalonia.FuncUI.Components.Hosts
open Avalonia.Themes.Fluent
open Elmish
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.Controls.ApplicationLifetimes
open Microsoft.FSharp.Control

module Counter =
    open Avalonia.Controls
    open Avalonia.Layout
    
    type State =
        { isRunning: bool }
        
    let init = { isRunning = false }
        
    type Msg =
        | Switch

    let processTask =
        async {
            Presence.presence()
        }
    
    let update (msg: Msg) (state: State) : State =
        match msg with
        | Switch ->
            { state with isRunning = not state.isRunning }
    
    let view (state: State) dispatch =
        DockPanel.create [
            DockPanel.children [
                Button.create [
                    Button.dock Dock.Bottom
                    Button.content (if state.isRunning then "Stop" else "Start")
                    Button.verticalAlignment VerticalAlignment.Center
                    Button.horizontalAlignment HorizontalAlignment.Center
                    Button.margin (0, 20)
                    Button.onClick (fun _ ->
                        processTask |> Async.Start
                        dispatch Switch )
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text $"{state.isRunning}"
                ]
//                TextBox.create [
//                    TextBox.dock Dock.Bottom
//                    TextBox.text (string state.isRunning)
//                    TextBox.horizontalAlignment HorizontalAlignment.Stretch
//                ]
            ]
        ]

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Media Player RPC"
        base.Height <- 400.0
        base.Width <- 400.0    
      
        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true
        Elmish.Program.mkSimple (fun () -> Counter.init) Counter.update Counter.view
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
