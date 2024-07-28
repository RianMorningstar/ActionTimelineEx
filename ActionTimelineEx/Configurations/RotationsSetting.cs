namespace ActionTimelineEx.Configurations;

public class RotationsSetting
{
    public int ChoiceIndex { get; set; } = 0;

    public List<RotationSetting> RotationSettings { get; set; } = [];

    [JsonIgnore]
    public RotationSetting RotationSetting
    {
        get
        {
            if (RotationSettings.Count == 0)
            {
                RotationSettings.Add(new());
                return RotationSettings[0];
            }
            else
            {
                return RotationSettings[ChoiceIndex % RotationSettings.Count];
            }
        }
    }
}
