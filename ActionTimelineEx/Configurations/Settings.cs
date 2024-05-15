using ActionTimelineEx.Configurations;
using Dalamud.Configuration;
using ECommons.DalamudServices;
using System.Numerics;
using XIVConfigUI.Attributes;

namespace ActionTimeline;

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

    public List<DrawingSettings> TimelineSettings = [];
    public HashSet<ushort> HideStatusIds { get; set; } = [];

    public int Version { get; set; } = 6;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
