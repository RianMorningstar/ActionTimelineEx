using System.ComponentModel;

namespace ActionTimelineEx.Configurations;
internal enum UiString
{
    Setting,

    [Description("Showed Statuses")]
    ShowedStatuses,

    [Description("Don't record these statuses")]
    NotStatues,
    Help,

    [Description("Please wait for a second.")]
    Wait,

    [Description("Are you sure to remove this timeline?")]
    Confirm,

    [Description("Click to remove this timeline.")]
    Remove,
}
