using Dalamud.Configuration;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using System.Numerics;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;

internal class TimelineChoicesAttribute : ChoicesAttribute
{
    protected override Pair[] GetChoices()
    {
        return [.. Plugin.Settings.EditSetting?.RotationSettings.Select(i => i.Name)];
    }
}

public class Settings : IPluginConfiguration
{
    [UI("Record Data")]
    public bool Record { get; set; } = true;

    [UI("Show Only In Duty")]
    public bool ShowTimelineOnlyInDuty { get; set; } = false;

    [UI("Show Only In Combat")]
    public bool ShowTimelineOnlyInCombat { get; set; } = false;

    [UI("Hide In Cutscene")]
    public bool HideTimelineInCutscene { get; set; } = true;

    [UI("Hide In Quest Event")]
    public bool HideTimelineInQuestEvent { get; set; } = true;

    [UI("Say Clipping Time")]
    public bool SayClipping { get; set; } = false;

    [UI("Clipping Range", Parent = nameof(SayClipping))]
    public Vector2 ClippintTime { get; set; } = new(0.15f, 2);

    [UI("Record Target Status")]
    public bool RecordTargetStatus { get; set; } = true;

    [UI("Show the donate link.")]
    public bool ShowDonate { get; set; } = true;

    public List<DrawingSettings> TimelineSettings { get; set; } = [];
    public HashSet<ushort> HideStatusIds { get; set; } = [];

    [UI("Draw Rotation", 1)]
    public bool DrawRotation { get; set; } = false;

    [UI("Locked", Parent = nameof(DrawRotation))]
    public bool RotationLocked { get; set; } = false;

    [UI("Locked Background Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationLockedBackgroundColor { get; set; } = new(0f, 0f, 0f, 0.5f);

    [UI("Unlocked Background Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationUnlockedBackgroundColor { get; set; } = new(0f, 0f, 0f, 0.75f);

    [UI("Rotation Highlight Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationHighlightColor { get; set; } = new Vector4(1, 1, 1, 1);

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("GCD Icon Size", Parent = nameof(DrawRotation))]
    public int GCDIconSize { get; set; } = 40;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("Off GCD Icon Size", Parent = nameof(DrawRotation))]
    public int OGCDIconSize { get; set; } = 30;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("Icon Spacing", Parent = nameof(DrawRotation))]
    public int IconSpacing { get; set; } = 5;

    [UI("Show the wrong clicking", Parent = nameof(DrawRotation))]
    public bool ShowWrongClick { get; set; } = true;

    [JsonIgnore]
    [TimelineChoices]
    [UI("Rotation Choice", Parent = nameof(DrawRotation))]
    public string RotationChoice
    {
        get => EditSetting?.Choice ?? "Default";
        set
        {
            if (EditSetting == null) return;
            EditSetting.Choice = value;
        }
    }

    [JsonIgnore]
    [UI("Record Rotation", 2)]
    public bool RecordRotation { get; set; } = false;

    [JsonProperty]
    private Dictionary<uint, Dictionary<Job, RotationsSetting>> _rotationHelpers = [];

    [JsonIgnore]
    internal RotationsSetting? EditSetting { get; set; } = null;

    private static readonly RotationsSetting EmptyHolder = new();
    public RotationsSetting GetSetting(uint territoryId)
    {
        if (!_rotationHelpers.TryGetValue(territoryId, out var dict)) _rotationHelpers[territoryId] = dict = [];

        if (!Player.Available) return EmptyHolder;

        var job = Player.Job;
        if (!dict.TryGetValue(job, out var result)) dict[job] = result = new();

        return result;
    }

    public int Version { get; set; } = 6;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
