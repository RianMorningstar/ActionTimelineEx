using System.ComponentModel;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations.Actions;

[Description("oGCD")]
public class oGCDAction : ActionSetting
{
    private ActionSettingType _type = ActionSettingType.Action;

    [UI("Type")]

    internal ActionSettingType ActionType
    {
        get => _type;
        set
        {
            if (value == _type) return;
            _type = value;

            Update();
}
    }

    internal override ActionSettingType Type => ActionType;
}
