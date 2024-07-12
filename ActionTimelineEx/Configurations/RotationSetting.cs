using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;
public class RotationSetting
{
    [UI("Rotation Name")]
    public string Name { get; set; } = "Default";

    public List<ActionSetting> Actions { get; set; } = [];
}
