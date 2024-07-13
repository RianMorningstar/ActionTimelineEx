using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;
public class RotationSetting
{
    [UI("Rotation Name")]
    public string Name { get; set; } = "Default";

    [UI]
    public List<ActionSetting> Actions { get; set; } = [];

    [UI("Ignore Actions")]
    public List<ActionSetting> IgnoreActions { get; set; } = [];
}
