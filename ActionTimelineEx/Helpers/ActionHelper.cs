using ECommons.GameHelpers;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace ActionTimelineEx.Helpers;

internal enum ActionType : byte
{
    GCD,
    oGCD,
    RoleAction,
    SystemAction,
    LimitBreak,
    DutyAction,
}

internal static class ActionHelper
{
    internal static ActionType GetActionType(this Action action)
    {
        var category = action.ClassJobCategory.Value;
        if (action.ActionCategory.Row is 10 or 11)
        {
            return ActionType.SystemAction;
        }
        else if (action.IsRoleAction)
        {
            return ActionType.RoleAction;
        }
        else if(action.ActionCategory.Row is 9 or 15)
        {
             return ActionType.LimitBreak;
        }
        else if(category != null && category.IsSingleJobForCombat())
        {
            return action.IsGcd() ? ActionType.GCD : ActionType.oGCD;
        }
        else
        {
            return ActionType.DutyAction;
        }
    }

    internal static bool IsGcd(this Action action)
    {
        return action.CooldownGroup == 58 || action.AdditionalCooldownGroup == 58;
    }

    public static bool IsSingleJobForCombat(this ClassJobCategory jobCategory)
    {
        if (jobCategory.RowId == 68) return true; // ACN SMN SCH 
        var str = jobCategory.Name.RawString.Replace(" ", "");
        if (!str.All(char.IsUpper)) return false;
        if (str.Length is not 3 and not 6) return false;
        return true;
    }

    internal static bool IsInJob(this Action i)
    {
        if (!Player.Available) return false;

        var cate = i.ClassJobCategory.Value;
        if (cate != null)
        {
            var inJob = (bool?)cate.GetType().GetProperty(Player.Job.ToString())?.GetValue(cate);
            if (inJob.HasValue && !inJob.Value) return false;
        }
        return true;
    }

    internal static string GetString(this TimeSpan timespan)
    {
        return $"{(int)timespan.TotalMinutes}:{timespan.Seconds:D2}.{timespan.Milliseconds.ToString("000")[0..1]}";
    }
}
