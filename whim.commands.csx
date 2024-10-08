using Whim;
using System.Threading.Tasks;  // used for FocusFollowsMouse hack

void AddUserCommands(IContext context)
{

    /************************************
     * Cycle over _inactive_ workspaces *
     ************************************/

    // Activate next workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "activate_previous_workspace",
        title: "Activate the previous inactive workspace",
        callback: () => context.Butler.ActivateAdjacent(reverse: true, skipActive: true)
    );

    // Activate previous workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "activate_next_workspace",
        title: "Activate the next inactive workspace",
        callback: () => context.Butler.ActivateAdjacent(skipActive: true)
    );

    // Move current window to next workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "move_window_to_previous_workspace",
        title: "Move focused window to the previous inactive workspace",
        callback: () => context.Butler.MoveWindowToAdjacentWorkspace(reverse: true, skipActive: true)
    );

    // Move current window to previous workspace, skipping over those that are active on other monitors
    context.CommandManager.Add(
        identifier: "move_window_to_next_workspace",
        title: "Move focused window to the next inactive workspace",
        callback: () => context.Butler.MoveWindowToAdjacentWorkspace(skipActive: true)
    );

    /*****************
     * Swap monitors *
     *****************/

    // Swap workspace with next monitor
    context.CommandManager.Add(
            identifier:"swap_workspace_with_next_monitor",
            title: "Swap monitors",
            callback: () => context.Butler.SwapWorkspaceWithAdjacentMonitor()
    );

    /***********************
     * Toggle focus layout *
     ***********************/

    // Toggle focus layout
    context.CommandManager.Add(
            identifier: "toggle_focus_layout",
            title: "Toggle focus layout",
            callback: () =>
            {
                IWorkspace workspace = context.WorkspaceManager.ActiveWorkspace;
                if (workspace.ActiveLayoutEngine.Name == "Focus")
                {
                    workspace.ActivatePreviouslyActiveLayoutEngine();
                }
                else
                {
                    workspace.PerformCustomLayoutEngineAction(
                        new LayoutEngineCustomAction()
                        {
                            Name = "Focus.unset_maximized",
                            Window = null
                        }
                    );
                    workspace.TrySetLayoutEngineFromName("Focus");
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
                    workspace.PerformCustomLayoutEngineAction(
                        new LayoutEngineCustomAction()
                        {
                            Name = "Focus.set_maximized",
                            Window = null
                        }
                    );
                    workspace.TrySetLayoutEngineFromName("Focus");
                }
            }
    );

    /****************
     * Close window *
     ****************/

    // Close active window
    context.CommandManager.Add(
        identifier: "close_window",
        title: "Close focused window",
        callback: () => context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Close()
    );

    /**************************
     * FocusFollowsMouse hack *
     **************************/

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

            IMonitor? oldMonitor = context.Butler.Pantry.GetMonitorForWindow(window);
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

            IWorkspace? workspace = context.Butler.Pantry.GetWorkspaceForMonitor(nextMonitor);
            if (workspace == null)
            {
                Logger.Error($"Monitor {nextMonitor} was not found in any workspace");
                return;
            }

            context.Butler.MoveWindowToWorkspace(workspace, window);
            Task.Run(async delegate
            {
                await Task.Delay(200);
                window.Focus();
            });
        }
    );
}
