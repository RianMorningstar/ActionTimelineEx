using ActionTimelineEx.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System.ComponentModel;
using System.Numerics;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations.Actions;

[Description("GCD")]
public class GCDAction : ActionSetting
{
    internal override ActionSettingType Type => ActionSettingType.Action;

    internal float Gcd => ActionManager.GetAdjustedRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, ActionId) / 1000f;

    [UI]
    public List<oGCDAction> oGCDs { get; set; } = [];

    public float Draw(ImDrawListPtr drawList, Vector2 point, bool pass, ActionSetting? activeAction, ref TimeSpan time)
    {
        if (Plugin.Settings.DrawTime)
        {
            var width = ImGui.CalcTextSize(time.GetString()).X + 5;
            drawList.AddText(point, uint.MaxValue, time.GetString() + "s  ");
            point += Vector2.UnitX * width;
            time += TimeSpan.FromSeconds(Gcd);
        }

        var gcd = Plugin.Settings.GCDIconSize;
        var ogcd = Plugin.Settings.OGCDIconSize;
        var spacing = Plugin.Settings.IconSpacing;

        float result = gcd;
        DrawIcon(drawList, point, gcd, pass, activeAction);
        point += new Vector2(gcd + spacing, 0);
        int index = 0;
        foreach (var oGcd in oGCDs)
        {
            oGcd.DrawIcon(drawList, point, ogcd, pass && index < RotationHelper.oGcdUsedCount, activeAction);
            if (!oGcd.IsEmpty) index++;

            point += new Vector2(ogcd + spacing, 0);
            result += ogcd + spacing;
        }

        return result;
    }
}
