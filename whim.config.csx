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

// Load local resources
#load "C:\Users\rober\.whim\whim.commands.csx"
#load "C:\Users\rober\.whim\whim.layouts.csx"

// Set compiler options
#define DEV    // raise debug level and de-activate auto-updater
#undef MOBILE  // add battery widget to bar
#undef TREE    // configure tree layout and its plugins

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
    FloatingLayoutPlugin floatingLayoutPlugin = new(context);
    context.PluginManager.AddPlugin(floatingLayoutPlugin);

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
    List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
    
    // Right components, note that these are defined right to left
    List<BarComponent> rightComponents = new()
    {
        #if MOBILE
            BatteryWidget.CreateComponent(),
        #endif
        DateTimeWidget.CreateComponent(60*1000, "M/d/yy  ·  h:mm tt")
    };

    BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
    BarPlugin barPlugin = new(context, barConfig);
    context.PluginManager.AddPlugin(barPlugin);

    /**************************
     * Gaps & Focus indicator *
     **************************/
    
    // Total gap = 2x * 150% scaling
    int gap = 3;
    
    // Let focus indicator completely fill gaps
    int borderSize = 3 * gap - 2;

    // Gaps plugin
    GapsConfig gapsConfig = new() { OuterGap = gap, InnerGap = gap };
    GapsPlugin gapsPlugin = new(context, gapsConfig);
    context.PluginManager.AddPlugin(gapsPlugin);

    // Focus indicator
    Brush borderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 129, 161, 193));
    FocusIndicatorConfig focusIndicatorConfig = new() { Color = borderBrush, FadeEnabled = false, BorderSize = borderSize };
    FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
    context.PluginManager.AddPlugin(focusIndicatorPlugin);
    
    /************************
     * Workspaces & layouts *
     ************************/

    // Workspaces, uses Nerd Fonts icons as names
    context.WorkspaceManager.Add("\udb81\udea1");  // main  <icon: nf-md-home_outline>
    context.WorkspaceManager.Add("\udb80\udd74");  // dev   <icon: nf-md-code_tags>
    context.WorkspaceManager.Add("\udb80\udde7");  // web   <icon: nf-md-earth>
    context.WorkspaceManager.Add("\udb85\uddd6");  // other <icon: nf-md-book_open_page_variant_outline>

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
    KeyModifiers Mod1 = KeyModifiers.LAlt;
    KeyModifiers Mod2 = KeyModifiers.LAlt | KeyModifiers.LShift;

    void Bind(KeyModifiers mod, VIRTUAL_KEY key, string cmd)
    {
        context.KeybindManager.SetKeybind(cmd, new Keybind(mod, key));
    }

    // Clear defaults
    context.KeybindManager.Clear();
    
    // Command palette
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

    // Move around workspaces
    Bind(Mod1, VIRTUAL_KEY.VK_K, "whim.custom.activate_previous_workspace");
    Bind(Mod1, VIRTUAL_KEY.VK_H, "whim.custom.activate_next_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_K, "whim.custom.move_window_to_previous_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_H, "whim.custom.move_window_to_next_workspace");
    Bind(Mod2, VIRTUAL_KEY.VK_OEM_2, "whim.custom.swap_workspace_with_next_monitor");  // MOD1 + ? or MOD2 + /

    // Change the layout
    Bind(Mod1, VIRTUAL_KEY.VK_J, "whim.core.move_window_right_edge_left");
    Bind(Mod1, VIRTUAL_KEY.VK_L, "whim.core.move_window_right_edge_right");
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