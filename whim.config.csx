#nullable enable
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "WHIM_PATH\plugins\Whim.SliceLayout\Whim.SliceLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.Updater\Whim.Updater.dll"

using System;
using System.Collections.Generic;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;
using Whim.TreeLayout.CommandPalette;
using Whim.Updater;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

// FocusFollowsMouse hack for SwapWindows
using System.Threading.Tasks;

void DoConfig(IContext context)
{
    context.Logger.Config = new LoggerConfig() { BaseMinLogLevel = LogLevel.Debug };

    /**************
     * Status bar *
     **************/

    // overload bar defaults with local "bar.resources.xaml"
    string file = context.FileManager.GetWhimFileDir("bar.resources.xaml");
    context.ResourceManager.AddUserDictionary(file);

    List<BarComponent> leftComponents = new() 
    {
        ActiveLayoutWidget.CreateComponent(),
        WorkspaceWidget.CreateComponent() 
    };
    List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
    List<BarComponent> rightComponents = new()
    {
        // BatteryWidget.CreateComponent(),
        DateTimeWidget.CreateComponent(60*1000, "M/d/yy  ·  h:mm tt")
    };

    BarConfig barConfig = new(leftComponents, centerComponents, rightComponents) { };
    BarPlugin barPlugin = new(context, barConfig);
    context.PluginManager.AddPlugin(barPlugin);

    /****************
     * Updater      *
     ****************/

    // UpdaterConfig updaterConfig = new() { ReleaseChannel = ReleaseChannel.Alpha };
    // UpdaterPlugin updaterPlugin = new(context, updaterConfig);
    // context.PluginManager.AddPlugin(updaterPlugin);

    /****************
     * Misc plugins *
     ****************/

    // Floating window plugin.
    FloatingLayoutPlugin floatingLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(floatingLayoutPlugin);

    // Gap plugin. For gaps to match border with 150% scaling, set OuterGap =
    // InnerGap = {x} and BorderSize = {3x - 2}
    int x = 3;
    GapsConfig gapsConfig = new() { OuterGap = x, InnerGap = x };
    GapsPlugin gapsPlugin = new(context, gapsConfig);
    context.PluginManager.AddPlugin(gapsPlugin);

    // Focus indicator.
    Brush borderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 129, 161, 193));
    FocusIndicatorConfig focusIndicatorConfig = new() { Color = borderBrush, FadeEnabled = false, BorderSize = 3 * x - 2 };
    FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
    context.PluginManager.AddPlugin(focusIndicatorPlugin);

    // Command palette.
    CommandPaletteConfig commandPaletteConfig = new(context);
    CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
    context.PluginManager.AddPlugin(commandPalettePlugin);

    /**************
     * Workspaces *
     **************/

    // Slice layout
    SliceLayoutPlugin sliceLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(sliceLayoutPlugin);

    // Tree layout
    TreeLayoutPlugin treeLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(treeLayoutPlugin);

    // Tree layout bar
    TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
    context.PluginManager.AddPlugin(treeLayoutBarPlugin);
    rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

    // Tree layout command palette
    TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin = new(context, treeLayoutPlugin, commandPalettePlugin);
    context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);

    // Layout preview
    LayoutPreviewPlugin layoutPreviewPlugin = new(context);
    context.PluginManager.AddPlugin(layoutPreviewPlugin);

    // Set up workspaces
    context.WorkspaceManager.Add("\udb81\udea1");   // main icon:  nf-md-home_outline
    context.WorkspaceManager.Add("\udb80\udd74");   // dev icon:   nf-md-code_tags
    context.WorkspaceManager.Add("\udb80\udde7");   // web icon:   nf-md-earth
    context.WorkspaceManager.Add("\udb85\uddd6");   // other icon: nf-md-book_open_page_variant_outline

    /*****e*****
     * Layouts *
     ***********/

    context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
    {

    (id) => new SliceLayoutEngine(
        context,
        sliceLayoutPlugin,
        id,
        new ParentArea(
            isRow: true,
            (0.25, new SliceArea(order: 1, maxChildren: 2)),
            (0.5,
                new ParentArea(
                    isRow: false,
                    (0.5, new SliceArea(order: 0, maxChildren: 1)),
                    (0.5, new SliceArea(order: 3, maxChildren: 1))
                )
             ),
            (0.25,
                new ParentArea(
                    isRow: false,
                    (0.667, new SliceArea(order: 2, maxChildren: 2)),
                    (0.333, new OverflowArea())
                )
             )
        )
    ) { Name = "Grid"},

    (id) => new SliceLayoutEngine(
        context,
        sliceLayoutPlugin,
        id,
        new ParentArea(
            isRow: true,
            (0.4, new OverflowArea()),
            (0.6, new SliceArea(order: 0, maxChildren: 1))
        )
    ) { Name = "Primary Stack"},

    (id) => new SliceLayoutEngine(
        context,
        sliceLayoutPlugin,
        id,
        new ParentArea(
            isRow: true,
            (0.25, new SliceArea(order: 1, maxChildren: 2)),
            (0.5, new SliceArea(order: 0, maxChildren: 1)),
            (0.25, new OverflowArea())
        )
    ) { Name = "Secondary Stack"},

    (id) => new FocusLayoutEngine(id)

    // (id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
    // (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
    // (id) => new ColumnLayoutEngine(id),

    };

    /*******************
     * Custom commands *
     *******************/

    // Next/previous layout engine
    context.CommandManager.Add(
            identifier:"next_layout_engine", 
            title: "Next Layout Engine",
            callback: () => context.WorkspaceManager.ActiveWorkspace.NextLayoutEngine()
    );
    context.CommandManager.Add(
            identifier:"previous_layout_engine", 
            title: "Previous Layout Engine",
            callback: () => context.WorkspaceManager.ActiveWorkspace.PreviousLayoutEngine()
    );

    // Swap monitors
    context.CommandManager.Add(
            identifier:"swap_workspace_with_next_monitor", 
            title: "Swap monitors",
            callback: () => context.WorkspaceManager.SwapActiveWorkspaceWithAdjacentMonitor()
    );

    // Activate adjacent workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "activate_previous_workspace",
        title: "Activate the previous inactive workspace",
        callback: () => context.WorkspaceManager.ActivateAdjacent(reverse: true, skipActive: true)
    );
    context.CommandManager.Add(
        identifier: "activate_next_workspace",
        title: "Activate the next inactive workspace",
        callback: () => context.WorkspaceManager.ActivateAdjacent(skipActive: true)
    );

    // Move current window to adjacent workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "move_window_to_previous_workspace",
        title: "Move focused window to the previous inactive workspace",
        callback: () => context.WorkspaceManager.MoveWindowToAdjacentWorkspace(reverse: true, skipActive: true)
    );
    context.CommandManager.Add(
        identifier: "move_window_to_next_workspace",
        title: "Move focused window to the next inactive workspace",
        callback: () => context.WorkspaceManager.MoveWindowToAdjacentWorkspace(skipActive: true)
    );

    // Close active window
    context.CommandManager.Add(
        identifier: "close_window",
        title: "Close focused window",
        callback: () => context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Close()
    );

    // Toggle focus layout
    context.CommandManager.Add(
            identifier: "toggle_focus_layout",
            title: "Toggle focus layout",
            callback: () =>
            {
                if (context.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine.Name == "Focus")
                {
                    context.WorkspaceManager.ActiveWorkspace.LastActiveLayoutEngine();
                }
                else
                {
                    context.WorkspaceManager.ActiveWorkspace.SetLayoutEngineFromName("Focus");
                }
            }
    );

    // Toggle maximized if focus layout is active, otherwise activate focus layout and set maximized to true
    context.CommandManager.Add(
            identifier: "toggle_focus_maximize",
            title: "Toggle focus layout maximize state",
            callback: () =>
            {
                IWorkspace workspace = context.WorkspaceManager.ActiveWorkspace;
                if (workspace.ActiveLayoutEngine.Name == "Focus")
                {
                    context.CommandManager.TryGetCommand("whim.core.focus_layout.toggle_maximized").TryExecute();
                }
                else
                {
                    workspace.SetLayoutEngineFromName("Focus");
                    workspace.PerformCustomLayoutEngineAction(
                        new LayoutEngineCustomAction()
                        {
                            Name = "Focus.set_maximized",
                            Window = null
                        }
                    );
                }
            }
    );

    // Move window to next monitor variant that refocuses moved window after 200ms
    context.CommandManager.Add(
        identifier: "move_window_to_next_monitor",
        title: "Move focused window to the next monitor",
        callback: () =>
        {
            // Get the next monitor.
            IMonitor monitor = context.MonitorManager.ActiveMonitor;
            IMonitor nextMonitor = context.MonitorManager.GetNextMonitor(monitor);

            // Get the current window
            IWindow? window = context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
            if (window == null)
            {
                Logger.Error("No window was found");
                return;
            }

            IMonitor? oldMonitor = context.WorkspaceManager.GetMonitorForWindow(window);
            if (oldMonitor == null)
            {
                Logger.Error($"Window {window} was not found in any monitor");
                return;
            }

            if (oldMonitor.Equals(nextMonitor))
            {
                Logger.Error($"Window {window} is already on monitor {nextMonitor}");
                return;
            }

            IWorkspace? workspace = context.WorkspaceManager.GetWorkspaceForMonitor(nextMonitor);
            if (workspace == null)
            {
                Logger.Error($"Monitor {nextMonitor} was not found in any workspace");
                return;
            }

            context.WorkspaceManager.MoveWindowToWorkspace(workspace, window);
            Task.Run(async delegate
            {
                await Task.Delay(200);
                window.Focus();
            });
        }
    );

    /****************
     * Key bindings *
     ****************/

    // Bindings make most (only) sense for Colemak-DH keyboard layout

    KeyModifiers Mod1 = KeyModifiers.LAlt;
    KeyModifiers Mod2 = KeyModifiers.LAlt | KeyModifiers.LShift;

    void Bind(KeyModifiers mod, VIRTUAL_KEY key, string cmd)
    {
        context.KeybindManager.SetKeybind(cmd, new Keybind(mod, key));
    }

    // Clear defaults
    context.KeybindManager.Clear();

    // Control palette
    Bind(Mod1, VIRTUAL_KEY.VK_P, "whim.command_palette.toggle");

    // Focus windows
    Bind(Mod1, VIRTUAL_KEY.VK_N, "whim.core.focus_window_in_direction.left");
    Bind(Mod1, VIRTUAL_KEY.VK_I, "whim.core.focus_window_in_direction.right");
    Bind(Mod1, VIRTUAL_KEY.VK_U, "whim.core.focus_window_in_direction.up");
    Bind(Mod1, VIRTUAL_KEY.VK_E, "whim.core.focus_window_in_direction.down");
    Bind(Mod1, VIRTUAL_KEY.VK_M, "whim.slice_layout.focus.promote");
    Bind(Mod1, VIRTUAL_KEY.VK_O, "whim.core.focus_next_monitor");

    // Move windows
    Bind(Mod2, VIRTUAL_KEY.VK_N, "whim.core.swap_window_in_direction.left");
    Bind(Mod2, VIRTUAL_KEY.VK_I, "whim.core.swap_window_in_direction.right");
    Bind(Mod2, VIRTUAL_KEY.VK_U, "whim.core.swap_window_in_direction.up");
    Bind(Mod2, VIRTUAL_KEY.VK_E, "whim.core.swap_window_in_direction.down");
    Bind(Mod2, VIRTUAL_KEY.VK_M, "whim.slice_layout.window.promote");
    Bind(Mod2, VIRTUAL_KEY.VK_O, "whim.custom.move_window_to_next_monitor");

    // Workspaces
    Bind(Mod1, VIRTUAL_KEY.VK_K, "whim.custom.activate_previous_workspace");
    Bind(Mod1, VIRTUAL_KEY.VK_H, "whim.custom.activate_next_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_K, "whim.custom.move_window_to_previous_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_H, "whim.custom.move_window_to_next_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_OEM_2, "whim.custom.swap_workspace_with_next_monitor");  // MOD1 + ? (or MOD2 + /)

    // Layout
    Bind(Mod1, VIRTUAL_KEY.VK_J, "whim.core.move_window_right_edge_left");
    Bind(Mod1, VIRTUAL_KEY.VK_L, "whim.core.move_window_right_edge_right");
    Bind(Mod1, VIRTUAL_KEY.VK_Y, "whim.custom.next_layout_engine");
    Bind(Mod1, VIRTUAL_KEY.VK_Y, "whim.custom.next_layout_engine");
    Bind(Mod2, VIRTUAL_KEY.VK_Y, "whim.custom.previous_layout_engine");

    // Manipulate windows
    Bind(Mod1, VIRTUAL_KEY.VK_T, "whim.floating_layout.toggle_window_floating");
    Bind(Mod1, VIRTUAL_KEY.VK_F, "whim.custom.toggle_focus_layout");
    Bind(Mod2, VIRTUAL_KEY.VK_F, "whim.custom.toggle_focus_maximize");
    Bind(Mod1, VIRTUAL_KEY.VK_X, "whim.core.minimize_window");
    Bind(Mod2, VIRTUAL_KEY.VK_C, "whim.custom.close_window");

    /*********************
     * Filters & routers *
     ********************/

    // context.FilterManager.Clear();
    context.FilterManager.AddTitleMatchFilter(".*[s|S]etup.*");
    context.FilterManager.AddTitleMatchFilter(".*[i|I]nstaller.*");
    context.FilterManager.AddProcessFileNameFilter("X-Mouse Controls.exe");

    context.RouterManager.AddProcessFileNameRoute("firefox.exe", "\udb80\udde7");
    context.RouterManager.RouterOptions = RouterOptions.RouteToLastTrackedActiveWorkspace;

}

return DoConfig;
