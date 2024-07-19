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
        return [.. Plugin.Settings.RotationHelper?.RotationSettings.Select(i => i.Name)];
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

    [UI("Only Show when Weapon On", Parent = nameof(DrawRotation))]
    public bool OnlyShowRotationWhenWeaponOn { get; set; } = true;

    [UI("Locked", Parent = nameof(DrawRotation))]
    public bool RotationLocked { get; set; } = false;

    [UI("Locked Background Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationLockedBackgroundColor { get; set; } = new(0f, 0f, 0f, 0.5f);

    [UI("Unlocked Background Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationUnlockedBackgroundColor { get; set; } = new(0f, 0f, 0f, 0.75f);

    [UI("Rotation Highlight Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationHighlightColor { get; set; } = new Vector4(1f, 1f, 0.8f, 1);

    [UI("Rotation Failed Color", Parent = nameof(DrawRotation))]
    public Vector4 RotationFailedColor { get; set; } = new Vector4(0.8f, 0.5f, 0.5f, 1);

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

    [UI("Draw the rotation Vertically", Parent = nameof(DrawRotation))]
    public bool VerticalDraw { get; set; } = false;

    [UI("Reverse Draw", Parent = nameof(VerticalDraw))]
    public bool Reverse { get; set; } = false;

    [UI("Ignore Items", Parent = nameof(DrawRotation))]
    public bool IgnoreItems { get; set; } = false;

    [UI("Ignore System Actions", Parent = nameof(DrawRotation))]
    public bool IgnoreSystemActions { get; set; } = false;

    [UI("Ignore Role Actions", Parent = nameof(DrawRotation))]
    public bool IgnoreRoleActions { get; set; } = false;

    [UI("Ignore Limit Breaks", Parent = nameof(DrawRotation))]
    public bool IgnoreLimitBreaks { get; set; } = false;

    [UI("Ignore Duty Actions", Parent = nameof(DrawRotation))]
    public bool IgnoreDutyActions { get; set; } = false;
    [JsonIgnore]
    [TimelineChoices]
    [UI("Rotation Choice", 3)]
    public int RotationChoice
    {
        get => RotationHelper.ChoiceIndex;
        set => RotationHelper.ChoiceIndex = value;
    }

    [JsonIgnore]
    [UI("Record Rotation", 2)]
    public bool RecordRotation { get; set; } = false;

    [JsonIgnore]
    [UI("Gcd Time", 2)]
    public float GcdTime 
    { 
        get => RotationHelper.GcdTime;
        set => RotationHelper.GcdTime = value;
    } 


    [JsonProperty()]
    private Dictionary<Job, RotationsSetting> _rotationHelpers = [];

    [JsonIgnore]
    internal RotationsSetting RotationHelper
    {
        get
        {
            if (!Player.Available) return EmptyHolder;

            var job = Player.Job;
            if (!_rotationHelpers.TryGetValue(job, out var result)) _rotationHelpers[job] = result = new();

            return result;
        }
    }

    private static readonly RotationsSetting EmptyHolder = new();

    public int Version { get; set; } = 6;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
