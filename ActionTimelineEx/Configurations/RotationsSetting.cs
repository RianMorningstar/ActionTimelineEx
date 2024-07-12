namespace ActionTimelineEx.Configurations;

public class RotationsSetting
{
    public string Choice { get; set; } = "Default";
    public List<RotationSetting> RotationSettings { get; set; } = [];

    [JsonIgnore]
    public RotationSetting RotationSetting
    {
        get
        {
            var result = RotationSettings.FirstOrDefault(r => r.Name == Choice);
            if (result != null) return result;

            result = RotationSettings.FirstOrDefault();
            if (result == null)
            {
                result = new();
                RotationSettings.Add(result);
            }

            Choice = result.Name;
            return result;
        }
    }
}
