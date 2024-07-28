using System.ComponentModel;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations.Actions;

[Description("oGCD")]
public class oGCDAction : ActionSetting
{
    private ActionSettingType _type = ActionSettingType.Action;

    [UI("Type")]
    public ActionSettingType ActionType
    {
        get => _type;
        set
        {
            if (value == _type) return;
            _type = value;

            Update();
}
    }

    public override ActionSettingType Type => ActionType;
}
