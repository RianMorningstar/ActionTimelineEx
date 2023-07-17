using ActionTimeline.Helpers;
using ActionTimeline.Timeline;
using ActionTimeline.Windows;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Commands;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace ActionTimeline;

public class Plugin : IDalamudPlugin
{
    public static readonly SortedList<uint, byte> IconStack = new()
    {
        { 16203, 0 }, // Medicine.

        { 10155, 1 }, //PLD Fight or Flight.

        { 12556, 1 }, //WAR Inner Strength.

        { 17926, 1 }, //DRK Blood Weapon.

        { 13601, 1 }, //GNB No mercy.

        { 12627, 1 }, //WHM Presence of Mind.

        { 12809, 1 }, //SCH Chain Stratagem.

        { 13245, 1 }, //AST Divination.
        { 13259, 2 }, //AST Harmony of Spirit.

        { 12532, 1 }, //MNK Brotherhood.
        { 12528, 2 }, //MNK Brotherhood.

        { 12578, 1 }, //DRG Battle Litany.
        { 12581, 2 }, //DRG Right Eye.
        { 10304, 3 }, //DRG Lance Charge.

        { 12918, 1 }, //NIN Trick Attack.
        { 15020, 2 }, //NIN Vulnerability Up.

        { 12601, 1 }, //BRD Battle Voice.
        { 12622, 2 }, //BRD Radiant Finale.
        { 10354, 3 }, //BRD Radiant Finale.

        { 13011, 1 }, //MCH Wildfire.

        { 13714, 1 }, //DNC Devilment.
        { 13709, 2 }, //DNC Technical Finish.

        { 12653, 1 }, //BLM Ley Lines.

        { 13409, 1 }, //RDM Embolden.

        { 12699, 1 }, //SMN 2703.
    };
    public string Name => "ActionTimelineEx";

    public static string Version { get; private set; } = "";

    public static Settings Settings { get; private set; } = null!;

    private static WindowSystem _windowSystem = null!;
    private static SettingsWindow _settingsWindow = null!;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);

        Svc.PluginInterface.UiBuilder.Draw += Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

        TimelineManager.Initialize();
        DrawHelper.Init();

        try
        {
            Settings = pluginInterface.GetPluginConfig() as Settings ?? new Settings();
        }
        catch
        {
            Settings = new Settings();
        }

        CreateWindows();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Cmd("/atl", "Opens the ActionTimeline configuration window.")]
    [SubCmd("lock", "Lock all windows")]
    [SubCmd("unlock", "Unlock all windows")]
    private void PluginCommand(string command, string arguments)
    {
        var sub = arguments.Split(' ').FirstOrDefault();
        if(string.Equals("unlock", sub, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var window in _windowSystem.Windows)
            {
                if (window is not TimelineWindow tWindow) continue;

                tWindow.Setting.Locked = false;
            }
        }
        else if (string.Equals("lock", sub, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var window in _windowSystem.Windows)
            {
                if (window is not TimelineWindow tWindow) continue;

                tWindow.Setting.Locked = true;
            }
        }
        else
        {
            _settingsWindow.IsOpen = !_settingsWindow.IsOpen;
        }
    }

    private void CreateWindows()
    {
        _settingsWindow = new SettingsWindow();

        _windowSystem = new WindowSystem("ActionTimeline_Windows");
        _windowSystem.AddWindow(_settingsWindow);
        _windowSystem.AddWindow(new TimelineWindow("Timeline")
        {
            Setting = Settings.TimelineSetting,
        });
    }

    private void Draw()
    {
        if (Settings == null || !Player.Available) return;

        UpdateTimeline();

        _windowSystem?.Draw();
    }

    private void UpdateTimeline()
    {
        foreach (var window in _windowSystem.Windows)
        {
            if (window is not TimelineWindow tWindow) continue;

            bool show = tWindow.Setting.Enable;
            if (show)
            {
                if (Settings.ShowTimelineOnlyInCombat && !Svc.Condition[ConditionFlag.InCombat])
                {
                    show = false;
                }

                if (Settings.ShowTimelineOnlyInDuty && !Svc.Condition[ConditionFlag.BoundByDuty])
                {
                    show = false;
                }
            }

            tWindow.IsOpen = show;
        }
    }

    public static void OpenConfigUi()
    {
        _settingsWindow.IsOpen = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Settings.Save();

        TimelineManager.Instance?.Dispose();

        _windowSystem.RemoveAllWindows();

        ECommonsMain.Dispose();

        Svc.PluginInterface.UiBuilder.Draw -= Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Svc.PluginInterface.UiBuilder.RebuildFonts();
    }
}
