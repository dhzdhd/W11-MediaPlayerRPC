namespace MediaPlayerRPC

open System.Reflection
open System.Threading
open System.Threading.Tasks
open Avalonia
open Avalonia.FuncUI.Hosts
open Avalonia.Media
open Avalonia.Media.Imaging
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.Controls.ApplicationLifetimes
open MediaPlayerRPC
open Microsoft.Win32
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.Controls
open Avalonia.Layout

module Main =
    type State =
        { IsRunning: bool
          RunOnStartup: bool
          HideOnStart: bool }
        
    let setRunOnStartup (flag: bool) =
        let rk = Registry.CurrentUser.OpenSubKey ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
        printfn $"{__SOURCE_DIRECTORY__}"
        
        match flag with
        | true -> rk.SetValue ("MediaPlayerRPC", $"{__SOURCE_DIRECTORY__}", RegistryValueKind.String)
        | false -> rk.DeleteValue "MediaPlayerRPC"
        
    let init =
        let settings = Database.getData ()
        if settings.HideOnStart then
            Presence.setPresence ()
        { IsRunning = false
          RunOnStartup = settings.RunOnStartup 
          HideOnStart = settings.HideOnStart }
        
    type Msg =
        | SwitchRunning
        | SetRunOnStartup of bool
        | SetHideOnStart of bool
        | Hide

    let mutable cts = new CancellationTokenSource ()
        
    let update (msg: Msg) (state: State) (windowService: HostWindow) : State =
        match msg with
        | SwitchRunning ->
            let isRunning = not state.IsRunning
            
            let token = cts.Token
            
            if isRunning then
                let work = async {
                    while not state.IsRunning && not token.IsCancellationRequested do
                        printfn "running"
                        Presence.setPresence ()
                        Async.Sleep 6000 |> Async.RunSynchronously
                }
                Async.Start (work, cancellationToken = token)
            else
                cts.Cancel ()
                cts.Dispose ()
                cts <- new CancellationTokenSource ()
                printfn "stopped"
                
                Presence.clearPresence ()

            { state with IsRunning = not state.IsRunning }
        | SetRunOnStartup x ->
            let newState = { state with RunOnStartup = x }
            setRunOnStartup x
            Database.insertData { Id = 1
                                  RunOnStartup = newState.RunOnStartup
                                  HideOnStart = newState.HideOnStart }
            newState
        | SetHideOnStart x ->
            let newState = { state with HideOnStart = x }
            Database.insertData { Id = 1
                                  RunOnStartup = newState.RunOnStartup
                                  HideOnStart = newState.HideOnStart }
            newState
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
                            Button.content (if state.IsRunning then "Stop" else "Start")
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.horizontalAlignment HorizontalAlignment.Center
                            Button.onClick (fun _ -> dispatch SwitchRunning )
                        ]
                        Button.create [
                            Button.dock Dock.Bottom
                            Button.isVisible state.IsRunning
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
                            ToggleSwitch.content "Run on startup"
                            ToggleSwitch.horizontalAlignment HorizontalAlignment.Center
                            ToggleSwitch.verticalAlignment VerticalAlignment.Center
                            ToggleSwitch.isChecked state.RunOnStartup
                            ToggleSwitch.margin (2, 0)
                            ToggleSwitch.onChecked (fun _ -> dispatch (SetRunOnStartup true))
                            ToggleSwitch.onUnchecked (fun _ -> dispatch (SetRunOnStartup false))
                        ]
                        ToggleSwitch.create [
                            ToggleSwitch.dock Dock.Top
                            ToggleSwitch.content "Hide on start"
                            ToggleSwitch.horizontalAlignment HorizontalAlignment.Center
                            ToggleSwitch.verticalAlignment VerticalAlignment.Center
                            ToggleSwitch.isChecked state.HideOnStart
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
                    TextBlock.text (if state.IsRunning then "Running" else "Stopped")
                ]
            ]
        ]

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Media Player RPC"
        base.Height <- 400.0
        base.Width <- 400.0
        base.Icon <- WindowIcon "Assets/icon.png"
        base.TransparencyLevelHint <- [WindowTransparencyLevel.Mica; WindowTransparencyLevel.Blur]
    
        let update state msg =
            Main.update state msg this
        
        Elmish.Program.mkSimple (fun () -> Main.init) update Main.view
        |> Program.withHost this
        |> Program.withConsoleTrace
        |> Program.run
        
type App() =
    inherit Application()

    override this.Initialize() =
         this.Styles.Add (FluentTheme())
         this.RequestedThemeVariant <- Styling.ThemeVariant.Dark
       
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
