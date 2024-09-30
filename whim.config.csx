#nullable enable
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingWindow\Whim.FloatingWindow.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "WHIM_PATH\plugins\Whim.SliceLayout\Whim.SliceLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.Updater\Whim.Updater.dll"

// Load local resources
#load "C:\Users\rober\.whim\whim.commands.csx"
#load "C:\Users\rober\.whim\whim.layouts.csx"

// Set compiler options
#define DEV    // raise debug level and de-activate auto-updater
#undef MOBILE  // add battery widget to bar
#undef TREE    // configure tree layout and its plugins

using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingWindow;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
#if TREE
    using Whim.TreeLayout;
    using Whim.TreeLayout.Bar;
    using Whim.TreeLayout.CommandPalette;
#endif
#if !DEV
    using Whim.Updater;
#endif
using Windows.Win32.UI.Input.KeyboardAndMouse;

void DoConfig(IContext context)
{
    #if DEV
        context.Logger.Config = new LoggerConfig() { BaseMinLogLevel = LogLevel.Debug };
    #else
        context.Logger.Config = new LoggerConfig() { BaseMinLogLevel = LogLevel.Error };
    #endif

    /***********
     * Plugins *
     ***********/

    // Auto-updater
    #if !DEV
        UpdaterConfig updaterConfig = new() { ReleaseChannel = ReleaseChannel.Alpha };
        UpdaterPlugin updaterPlugin = new(context, updaterConfig);
        context.PluginManager.AddPlugin(updaterPlugin);
    #endif

    // Command palette
    CommandPaletteConfig commandPaletteConfig = new(context);
    CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
    context.PluginManager.AddPlugin(commandPalettePlugin);

    // Layout preview
    LayoutPreviewPlugin layoutPreviewPlugin = new(context);
    context.PluginManager.AddPlugin(layoutPreviewPlugin);

    // Floating window plugin
    FloatingWindowPlugin floatingWindowPlugin = new(context);
    context.PluginManager.AddPlugin(floatingWindowPlugin);

    // Slice layout
    SliceLayoutPlugin sliceLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(sliceLayoutPlugin);

    // Tree layout
    #if TREE
        TreeLayoutPlugin treeLayoutPlugin = new(context);
        context.PluginManager.AddPlugin(treeLayoutPlugin);

        TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
        context.PluginManager.AddPlugin(treeLayoutBarPlugin);

        TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin = new(context, treeLayoutPlugin, commandPalettePlugin);
        context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);
    #endif

    // Source custom commands
    AddUserCommands(context);

    /**************
     * Status bar *
     **************/

    // Overload bar defaults with local "bar.resources.xaml"
    string file = context.FileManager.GetWhimFileDir("bar.resources.xaml");
    context.ResourceManager.AddUserDictionary(file);

    // Left components
    List<BarComponent> leftComponents = new()
    {
        ActiveLayoutWidget.CreateComponent(),
        #if TREE
            treeLayoutBarPlugin.CreateComponent(),
        #endif
        WorkspaceWidget.CreateComponent()
    };

    // Center components
    List<BarComponent> centerComponents = new()
    {
        DateTimeWidget.CreateComponent(60*1000, "MMM d   h:mm tt")
    };

    // Right components, note that these are defined right to left
    List<BarComponent> rightComponents = new()
    {
        #if MOBILE
            BatteryWidget.CreateComponent(),
        #endif
        FocusedWindowWidget.CreateComponent()
    };

    BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
    BarPlugin barPlugin = new(context, barConfig);
    context.PluginManager.AddPlugin(barPlugin);

    /**************************
     * Gaps & Focus indicator *
     **************************/

    // Total gap = 2 * floor(scale * x), looks good with 150% display scale
    int gap = 5;

    // Border width of focused window
    int borderSize = 4;

    // Gaps plugin
    GapsConfig gapsConfig = new() { OuterGap = gap, InnerGap = gap };
    GapsPlugin gapsPlugin = new(context, gapsConfig);
    context.PluginManager.AddPlugin(gapsPlugin);

    // Focus indicator
    Brush borderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 120, 212));
    FocusIndicatorConfig focusIndicatorConfig = new() { Color = borderBrush, FadeEnabled = false, BorderSize = borderSize };
    FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
    context.PluginManager.AddPlugin(focusIndicatorPlugin);

    /************************
     * Workspaces & layouts *
     ************************/

    Dictionary<string, string> workspaces = new Dictionary<string, string>();
    void AddWorkspace(string name, string icon) {
        workspaces.Add(name, icon);
        context.WorkspaceManager.Add(icon);
    }

    // Workspaces, uses Nerd Fonts icons as names
    AddWorkspace("main", "\udb81\udea1");   // icon: nf-md-home_outline
    AddWorkspace("code", "\udb80\udd74");   // icon: nf-md-code_tags
    AddWorkspace("web", "\udb80\udde7");    // icon: nf-md-earth
    AddWorkspace("other", "\udb85\uddd6");  // icon: nf-md-book_open_page_variant_outline

    // Layout engines
    context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
    {
        (id) => CustomLayouts.CreateGridLayout(context, sliceLayoutPlugin, id, "Grid"),
        (id) => CustomLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id, 0.6, true, "Primary stack"),
        (id) => CustomLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id, 1, 2, "Secondary stack"),
        #if TREE
            (id) => new TreeLayoutEngine(context, treeLayoutPlugin, id) {Name = "Tree"},
        #endif
        (id) => new FocusLayoutEngine(id) {Name = "Focus"}
    };

    /****************
     * Key bindings *
     ****************/

    // Note: bindings are intended for Colemak-DH keyboard layout and probably don't make much sense for QWERTY

    // Modifiers
    KeyModifiers mod1 = KeyModifiers.LAlt;
    KeyModifiers mod2 = KeyModifiers.LAlt | KeyModifiers.LShift;

    void Bind(KeyModifiers mod, string key, string cmd)
    {
        VIRTUAL_KEY vk = (VIRTUAL_KEY)Enum.Parse(typeof(VIRTUAL_KEY), "VK_" + key);
        context.KeybindManager.SetKeybind(cmd, new Keybind(mod, vk));
    }

    // Clear defaults
    context.KeybindManager.Clear();

    // Command palette
    Bind(mod1, "P", "whim.command_palette.toggle");
    Bind(mod2, "P", "whim.command_palette.find_focus_window");

    // Focus windows
    Bind(mod1, "N", "whim.core.focus_window_in_direction.left");
    Bind(mod1, "I", "whim.core.focus_window_in_direction.right");
    Bind(mod1, "U", "whim.core.focus_window_in_direction.up");
    Bind(mod1, "E", "whim.core.focus_window_in_direction.down");
    Bind(mod1, "M", "whim.slice_layout.focus.promote");
    Bind(mod1, "O", "whim.core.focus_next_monitor");

    // Move windows
    Bind(mod2, "N", "whim.core.swap_window_in_direction.left");
    Bind(mod2, "I", "whim.core.swap_window_in_direction.right");
    Bind(mod2, "U", "whim.core.swap_window_in_direction.up");
    Bind(mod2, "E", "whim.core.swap_window_in_direction.down");
    Bind(mod2, "M", "whim.slice_layout.window.promote");
    Bind(mod2, "O", "whim.custom.move_window_to_next_monitor");

    // Move around workspaces
    Bind(mod1, "K", "whim.custom.activate_previous_workspace");
    Bind(mod1, "H", "whim.custom.activate_next_workspace");
    Bind(mod2, "K", "whim.custom.move_window_to_previous_workspace");
    Bind(mod2, "H", "whim.custom.move_window_to_next_workspace");
    Bind(mod2, "OEM_2", "whim.custom.swap_workspace_with_next_monitor");  // mod2 + "/" or mod1 + "?"

    // Change the layout
    Bind(mod1, "J", "whim.core.move_window_right_edge_left");
    Bind(mod1, "L", "whim.core.move_window_right_edge_right");
    Bind(mod1, "Y", "whim.core.cycle_layout_engine.next");
    Bind(mod2, "Y", "whim.core.cycle_layout_engine.previous");

    // Manipulate windows
    Bind(mod1, "T", "whim.floating_window.toggle_window_floating");
    Bind(mod1, "F", "whim.custom.toggle_focus_layout");
    Bind(mod2, "F", "whim.custom.toggle_focus_maximize");
    Bind(mod1, "X", "whim.core.minimize_window");
    Bind(mod2, "C", "whim.custom.close_window");

    /*********************
     * Filters & routers *
     ********************/

    // Route to currently active workspace by default
    context.RouterManager.RouterOptions = RouterOptions.RouteToLastTrackedActiveWorkspace;

    // Custom workspace routings
    context.RouterManager.AddWindowClassRoute("SunAwtFrame", workspaces["code"]);  // Pycharm & Rider
    context.RouterManager.AddProcessFileNameRoute("firefox.exe", workspaces["web"]);
    context.RouterManager.AddProcessFileNameRoute("TIDAL.exe", workspaces["other"]);

    // Custom filters (aka ignored windows)
    context.FilterManager.AddTitleMatchFilter(".*[s|S]etup.*");
    context.FilterManager.AddTitleMatchFilter(".*[i|I]nstaller.*");
    context.FilterManager.AddProcessFileNameFilter("SshTaskTray.exe");  // ScanSnap Task Tray
    context.FilterManager.AddProcessFileNameFilter("PfuSshMain.exe");  // ScanSnap Home
    context.FilterManager.AddProcessFileNameFilter("X-Mouse Controls.exe");
    context.FilterManager.AddProcessFileNameFilter("LogiPresentationUI.exe");
    context.FilterManager.Add((window) => window.WindowClass.StartsWith("WindowsForms10.Window.20008.app"));  // preview window of explorer on Windows10
}

return DoConfig;
