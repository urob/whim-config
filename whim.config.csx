#nullable enable
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
#r "C:\Users\rober\dev\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Updater\Whim.Updater.dll"

// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.Gaps\Whim.Gaps.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
// #r "C:\Users\rober\AppData\Local\Programs\Whim\plugins\Whim.Updater\Whim.Updater.dll"

using System;
using System.Collections.Generic;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
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
    context.Logger.Config = new LoggerConfig();

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
        DateTimeWidget.CreateComponent(60*1000, "MM/dd  ·  hh:mm t\\M")
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

    // Tree layout.
    TreeLayoutPlugin treeLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(treeLayoutPlugin);

    // Tree layout bar.
    TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
    context.PluginManager.AddPlugin(treeLayoutBarPlugin);
    rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

    // Tree layout command palette.
    TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin = new(context, treeLayoutPlugin, commandPalettePlugin);
    context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);

    // Layout preview.
    LayoutPreviewPlugin layoutPreviewPlugin = new(context);
    context.PluginManager.AddPlugin(layoutPreviewPlugin);

    // Set up workspaces.
    context.WorkspaceManager.Add("\udb81\udea1");   // main icon:  nf-md-home_outline
    context.WorkspaceManager.Add("\udb80\udd74");   // dev icon:   nf-md-code_tags
    context.WorkspaceManager.Add("\udb80\udde7");   // web icon:   nf-md-earth
    context.WorkspaceManager.Add("\udb85\uddd6");   // other icon: nf-md-book_open_page_variant_outline

    // Set up layout engines.
    context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
    {
        (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
        (id) => new ColumnLayoutEngine(id)
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
            callback: () => context.WorkspaceManager.SwapActiveWorkspaceWithNextMonitor()
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
            identifier:"close_window", 
            title: "close focused window",
            callback: () => context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Close()
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
    Bind(Mod1, VIRTUAL_KEY.VK_O, "whim.core.focus_next_monitor");

    // Move windows
    Bind(Mod2, VIRTUAL_KEY.VK_N, "whim.core.swap_window_in_direction.left");
    Bind(Mod2, VIRTUAL_KEY.VK_I, "whim.core.swap_window_in_direction.right");
    Bind(Mod2, VIRTUAL_KEY.VK_U, "whim.core.swap_window_in_direction.up");
    Bind(Mod2, VIRTUAL_KEY.VK_E, "whim.core.swap_window_in_direction.down");
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
    Bind(Mod1, VIRTUAL_KEY.VK_F, "whim.core.maximize_window");
    Bind(Mod1, VIRTUAL_KEY.VK_X, "whim.core.minimize_window");
    Bind(Mod2, VIRTUAL_KEY.VK_C, "whim.custom.close_window");

    /*********************
     * Filters & routers *
     ********************/

    // context.FilterManager.Clear();
    context.FilterManager.AddTitleMatchFilter(".*[s|S]etup.*");
    context.FilterManager.AddTitleMatchFilter(".*[i|I]nstaller.*");

    context.RouterManager.AddProcessFileNameRoute("firefox.exe", "\udb80\udde7");
    context.RouterManager.RouteToActiveWorkspace = true;

}

return DoConfig;
