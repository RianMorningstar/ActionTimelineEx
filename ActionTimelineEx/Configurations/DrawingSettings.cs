using Dalamud.Interface.Colors;
using Newtonsoft.Json;
using System.Numerics;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;

public class DrawingSettings
{
    [UI("The timeline Name", (int)GroupItem.General)]
    public string Name { get; set; } = "Major";

    [UI("Enable", (int)GroupItem.General)]
    public bool Enable { get; set; } = true;

    [UI("Is Rotation", (int)GroupItem.General)]
    public bool IsRotation { get; set; } = false;

    [UI("Is Horizonal", (int)GroupItem.General)]
    public bool IsHorizonal { get; set; } = true;

    [UI("Is Reverse", (int)GroupItem.General)]
    public bool IsReverse { get; set; } = false;

    [JsonIgnore]
    public Vector2 TimeDirectionPerSecond => TimeDirection * SizePerSecond;

    [JsonIgnore]
    public Vector2 TimeDirection => IsHorizonal ? Vector2.UnitX : Vector2.UnitY;

    [JsonIgnore]
    public Vector2 DownDirection => IsReverse ? -RealDownDirection : RealDownDirection;

    [JsonIgnore]
    public Vector2 RealDownDirection => IsHorizonal ? Vector2.UnitY : Vector2.UnitX;

    [UI("Locked", (int)GroupItem.General)]
    public bool Locked { get; set; } = false;

    [UI("Locked Background Color", (int)GroupItem.General)]
    public Vector4 LockedBackgroundColor { get; set; } = new (0f, 0f, 0f, 0.5f);

    [UI("Unlocked Background Color", (int)GroupItem.General)]
    public Vector4 UnlockedBackgroundColor { get; set; } = new (0f, 0f, 0f, 0.75f);

    [Range(20, 150, ConfigUnitType.Pixels, 0.3f)]
    [UI("Size per second", (int)GroupItem.General, Description = "This is the width of every second drawn on the window.")]
    public float SizePerSecond { get; set; } = 60;

    [Range(0, 5, ConfigUnitType.Seconds, 0.1f)]
    [UI("Offset Time", (int)GroupItem.General, Description = "If your Is Rotation is on, this means the Offset time of rotation, or this is the advanced time about action using.")]
    public int TimeOffsetSetting { get; set; } = 2;


    [Range(-500, 500, ConfigUnitType.Pixels, 0.3f)]
    [UI("Center Offset", (int)GroupItem.General)]
    public float CenterOffset { get; set; } = 0;

    [JsonIgnore]
    public int TimeOffset => IsRotation ? -TimeOffsetSetting : TimeOffsetSetting;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("GCD Icon Size", (int)GroupItem.Icons)]
    public int GCDIconSize { get; set; } = 40;

    [UI("Show Off GCD", (int)GroupItem.Icons)]
    public bool ShowOGCD { get; set; } = true;

    [Range(1, 100, ConfigUnitType.Pixels)]
    [UI("Off GCD Icon Size", (int)GroupItem.Icons, Parent = nameof(ShowOGCD))]
    public int OGCDIconSize { get; set; } = 30;

    [Range(0, 1, ConfigUnitType.Percent, 0.01f)]
    [UI("Off GCD Vertical Offset", (int)GroupItem.Icons, Parent = nameof(ShowOGCD))]
    public float OGCDOffset { get; set; } = 0.1f;

    [UI("Show Auto Attack", (int)GroupItem.Icons)]
    public bool ShowAutoAttack { get; set; } = true;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("Auto Attack Icon Size", (int)GroupItem.Icons, Parent = nameof(ShowAutoAttack))]
    public int AutoAttackIconSize { get; set; } = 15;

    [Range(0, 1, ConfigUnitType.Percent, 0.01f)]
    [UI("Auto Attack Offset", (int)GroupItem.Icons, Parent = nameof(ShowAutoAttack))]
    public float AutoAttackOffset { get; set; } = 0.1f;

    [UI("Show Status Gain Lose", (int)GroupItem.Icons)]
    public bool ShowStatus { get; set; } = true;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("Status Icon Size", (int)GroupItem.Icons, Parent = nameof(ShowStatus))]
    public int StatusIconSize { get; set; } = 15;

    [Range(0 , 1, ConfigUnitType.Percent, 0.01f)]
    [UI("Status Icon Alpha", (int)GroupItem.Icons, Parent = nameof(ShowStatus))]
    public float StatusIconAlpha { get; set; } = 0.5f;

    [UI("Status Gain Color", (int)GroupItem.Icons, Parent = nameof(ShowStatus))]
    public Vector4 StatusGainColor { get; set; } = ImGuiColors.HealerGreen;

    [UI("Status Lose Color", (int)GroupItem.Icons, Parent = nameof(ShowStatus))]
    public Vector4 StatusLoseColor { get; set; } = ImGuiColors.DalamudRed;

    [UI("Status Offset", (int)GroupItem.Icons, Parent = nameof(ShowStatus))]
    [Range(0, 1, ConfigUnitType.Percent, 0.01f)]
    public float StatusOffset { get; set; } = 0.1f;

    [UI("Show Damage Type", (int)GroupItem.Icons)]
    public bool ShowDamageType { get; set; } = true;

    [UI("Direct Damage Color", (int)GroupItem.Icons, Parent = nameof(ShowDamageType))]
    public Vector4 DirectColor { get; set; } = ImGuiColors.DalamudYellow;

    [UI("Critical Damage Color", (int)GroupItem.Icons, Parent = nameof(ShowDamageType))]
    public Vector4 CriticalColor { get; set; } = ImGuiColors.DalamudOrange;

    [UI("Critical Direct Damage Color", (int)GroupItem.Icons, Parent = nameof(ShowDamageType))]
    public Vector4 CriticalDirectColor { get; set; } = ImGuiColors.DPSRed;

    [UI("Bar Background Color", (int)GroupItem.Bar)]
    public Vector4 BackgroundColor { get; set; } = new (0.5f, 0.5f, 0.5f, 0.5f);

    [UI("GCD Border Color", (int)GroupItem.Bar)]
    public Vector4 GCDBorderColor { get; set; } = new (0.9f, 0.9f, 0.9f, 1f);

    [Range(0, 10, ConfigUnitType.Pixels, 0.01f)]
    [UI("GCD Border Thickness", (int)GroupItem.Bar)]
    public float GCDThickness { get; set; } = 1.5f;

    [Range(0, 10, ConfigUnitType.Pixels, 0.01f)]
    [UI("GCD Border Round", (int)GroupItem.Bar)]
    public float GCDRound { get; set; } = 2;

    [Range(0, 1, ConfigUnitType.Percent, 0.01f)]
    [UI("GCD Bar Height", (int)GroupItem.Bar)]
    public Vector2 GCDHeight { get; set; } = new Vector2(0.5f, 0.8f);

    [UI("Cast In Progress Color", (int)GroupItem.Bar)]
    public Vector4 CastInProgressColor { get; set; } = new (0.2f, 0.8f, 0.2f, 1f);

    [UI("Cast Finished Color", (int)GroupItem.Bar)]
    public Vector4 CastFinishedColor { get; set; } = new (0.5f, 0.5f, 0.5f, 1f);

    [UI("Cast Canceled Color", (int)GroupItem.Bar)]
    public Vector4 CastCanceledColor { get; set; } = new (0.8f, 0.2f, 0.2f, 1f);

    [UI("Show Animation Lock Time", (int)GroupItem.Bar)]
    public bool ShowAnimationLock { get; set; } = true;

    [UI("Animation Lock Color", (int)GroupItem.Bar, Parent = nameof(ShowAnimationLock))]
    public Vector4 AnimationLockColor { get; set; } = new (0.8f, 0.7f, 0.6f, 1f);

    [UI("Show Status Line", (int)GroupItem.Bar)]
    public bool ShowStatusLine { get; set; } = true;

    [Range(1, 100, ConfigUnitType.Pixels, 0.2f)]
    [UI("Status Line Height", (int)GroupItem.Bar, Parent =nameof(ShowStatusLine))]
    public float StatusLineSize { get; set; } = 18;

    [UI("Show Grid", (int)GroupItem.Grid)]
    public bool ShowGrid { get; set; } = true;

    [Range(0.1f, 10, ConfigUnitType.Pixels, 0.1f)]
    [UI("Start Line Width", (int)GroupItem.Grid)]
    public float GridStartLineWidth { get; set; } = 3;

    [UI("Start Line Color", (int)GroupItem.Grid)]
    public Vector4 GridStartLineColor { get; set; } = new(0.3f, 0.5f, 0.2f, 1f);

    [UI("Show Center Line", (int)GroupItem.Grid, Parent = nameof(ShowGrid))]
    public bool ShowGridCenterLine { get; set; } = false;

    [Range(0.1f, 10,  ConfigUnitType.Pixels, 0.1f)]
    [UI("Center Line Width", (int)GroupItem.Grid, Parent = nameof(ShowGridCenterLine))]
    public float GridCenterLineWidth { get; set; } = 1f;

    [UI("Center Line Color", (int)GroupItem.Grid, Parent = nameof(ShowGridCenterLine))]
    public Vector4 GridCenterLineColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.3f);

    [Range(0.1f, 10, ConfigUnitType.Pixels, 0.1f)]
    [UI("Line Width", (int)GroupItem.Grid, Parent = nameof(ShowGrid))]
    public float GridLineWidth { get; set; } = 1;

    [UI("Line Color", (int)GroupItem.Grid, Parent = nameof(ShowGrid))]
    public Vector4 GridLineColor { get; set; } = new(0.3f, 0.3f, 0.3f, 1f);

    [UI("Divide By Seconds", (int)GroupItem.Grid, Parent = nameof(ShowGrid))]
    public bool GridDivideBySeconds { get; set; } = true;

    [UI("Show Text", (int)GroupItem.Grid, Parent = nameof(GridDivideBySeconds))]
    public bool GridShowSecondsText { get; set; } = true;

    [UI("Sub-Divide By Seconds", (int)GroupItem.Grid, Parent = nameof(GridDivideBySeconds))]
    public bool GridSubdivideSeconds { get; set; } = true;

    [Range(2, 8, ConfigUnitType.None, 0.2f)]
    [UI("Sub-Division Count", (int)GroupItem.Grid, Parent = nameof(GridSubdivideSeconds))]
    public int GridSubdivisionCount { get; set; } = 2;

    [Range(1, 5, ConfigUnitType.None, 0.5f)]
    [UI("Sub-Division Line Width", (int)GroupItem.Grid, Parent = nameof(GridSubdivideSeconds))]
    public float GridSubdivisionLineWidth { get; set; } = 1;

    [UI("Sub-Division Line Color", (int)GroupItem.Grid, Parent = nameof(GridSubdivideSeconds))]
    public Vector4 GridSubdivisionLineColor { get; set; } = new (0.3f, 0.3f, 0.3f, 0.2f);

    [UI("Enable GCD Clipping", (int)GroupItem.GcdClipping,
        Description = "This only shown when timeline is not rotation.")]
    public bool ShowGCDClippingSetting = true;

    [JsonIgnore]
    public bool ShowGCDClipping => !IsRotation && ShowGCDClippingSetting;

    [Range(0, 1, ConfigUnitType.Seconds)]
    [UI("Threshold", (int)GroupItem.GcdClipping,
        Parent = nameof(ShowGCDClippingSetting),
        Description = "This can be used filter out \"false positives\" due to latency or other factors. Any GCD clipping detected that is shorter than this value will be ignored.\nIt is strongly recommended that you test out different values and find out what works best for your setup.")]
    public float GCDClippingThreshold { get; set; } = 0.15f;

    [Range(3, 60, ConfigUnitType.Seconds)]
    [UI("Max Time", (int)GroupItem.GcdClipping,
        Parent = nameof(ShowGCDClippingSetting),
        Description = "Any GCD clip longer than this will be capped")]
    public int GCDClippingMaxTime { get; set; } = 2;

    [UI("Color", (int)GroupItem.GcdClipping,
        Parent = nameof(ShowGCDClippingSetting))]
    public Vector4 GCDClippingColor { get; set; } = new (1f, 0.2f, 0.2f, 0.3f);

    [UI("Text Color", (int)GroupItem.GcdClipping,
        Parent = nameof(ShowGCDClippingSetting))]
    public Vector4 GCDClippingTextColor { get; set; } = new (0.9f, 0.9f, 0.9f, 1f);
}
