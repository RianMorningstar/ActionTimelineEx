using ActionTimelineEx.Configurations;
using Dalamud.Configuration;
using ECommons.DalamudServices;

namespace ActionTimeline;

[Serializable]
public class Settings : IPluginConfiguration
{
    public bool ShowTimelineOnlyInDuty = false;
    public bool ShowTimelineOnlyInCombat = false;
    public float StatusCheckDelay = 0.1f;
    public DrawingSettings TimelineSetting = new DrawingSettings();

    public int Version { get; set; } = 6;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
