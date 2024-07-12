using ActionTimeline.Timeline;
using ActionTimeline.Windows;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Helpers;
using ActionTimelineEx.Windows;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using XIVConfigUI;
using XIVDrawer;

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
    public static Settings Settings { get; private set; } = null!;

    private static WindowSystem _windowSystem = null!;
    private static SettingsWindow _settingsWindow = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);
        XIVConfigUIMain.Init(pluginInterface, "/atle", "Opens the ActionTimelineEx configuration window.", PluginCommand, typeof(Settings), typeof(DrawingSettings), typeof(GroupItem), typeof(UiString));
        XIVDrawerMain.Init(pluginInterface, "ActionTimelineExOverlay");

        Svc.PluginInterface.UiBuilder.Draw += Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Svc.PluginInterface.UiBuilder.OpenMainUi += OpenConfigUi;

        TimelineManager.Initialize();

        try
        {
            Settings = pluginInterface.GetPluginConfig() as Settings ?? new Settings();
        }
        catch
        {
            Settings = new Settings();
        }

        RotationHelper.Init();
        CreateWindows();
    }

    public void Dispose()
    {
        Settings.Save();

        TimelineManager.Instance?.Dispose();
        RotationHelper.Dispose();

        _windowSystem.RemoveAllWindows();

        XIVConfigUIMain.Dispose();
        XIVDrawerMain.Dispose();
        ECommonsMain.Dispose();

        Svc.PluginInterface.UiBuilder.Draw -= Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Svc.PluginInterface.UiBuilder.OpenMainUi -= OpenConfigUi;
        GC.SuppressFinalize(this);
    }

    public static void PluginCommand(string _)
    {
        _settingsWindow.Toggle();
    }

    private static void CreateWindows()
    {
        _settingsWindow = new SettingsWindow();

        _windowSystem = new WindowSystem("ActionTimeline_Windows");
        _windowSystem.AddWindow(_settingsWindow);
    }

    private void Draw()
    {
        if (Settings == null || !Player.Available) return;
        if (Svc.GameGui.GameUiHidden) return;

        _windowSystem?.Draw();

        if (!ShowTimeline()) return;

        int index = 0;
        foreach (var setting in Settings.TimelineSettings)
        {
            TimelineWindow.Draw(setting, index++);
        }
        RotationHelperWindow.Draw();
    }

    private static bool ShowTimeline()
    {
        if (Settings.ShowTimelineOnlyInCombat && !Svc.Condition[ConditionFlag.InCombat])
        {
            return false;
        }

        if (Settings.ShowTimelineOnlyInDuty && !Svc.Condition[ConditionFlag.BoundByDuty])
        {
            return false;
        }

        if (Settings.HideTimelineInCutscene 
            && (Svc.Condition[ConditionFlag.WatchingCutscene] 
            || Svc.Condition[ConditionFlag.WatchingCutscene78]
            || Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]))
        {
            return false;
        }

        if (Settings.HideTimelineInQuestEvent 
            && (Svc.Condition[ConditionFlag.OccupiedInQuestEvent]))
        {
            return false;
        }

        return true;
    }

    public static void OpenConfigUi()
    {
        _settingsWindow.IsOpen = true;
    }
}
