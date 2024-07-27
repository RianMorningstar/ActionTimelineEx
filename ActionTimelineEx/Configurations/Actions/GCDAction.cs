using ActionTimelineEx.Helpers;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System.ComponentModel;
using System.Numerics;
using XIVConfigUI.Attributes;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace ActionTimelineEx.Configurations.Actions;

[Description("GCD")]
public class GCDAction : ActionSetting
{
    internal override ActionSettingType Type => ActionSettingType.Action;

    internal float Gcd
    {
        get
        {
            var recastTime = Svc.Data.GetExcelSheet<Action>()?.GetRow(ActionId)?.Recast100ms ?? 0;

            return GcdOverride == 0
            ? Plugin.Settings.RotationHelper.GcdTime / 2.5f * recastTime / 10f
            : GcdOverride;
        }
    }

    [JsonIgnore]
    [Range(0, 20, ConfigUnitType.Seconds)]
    [UI("Recast time override")]
    public float GcdOverride
    {
        get => Plugin.Settings.ActionRecast.TryGetValue(ActionId, out var v) ? v : 0f;
        set => Plugin.Settings.ActionRecast[ActionId] = value;
    }

    [UI]
    public List<oGCDAction> oGCDs { get; set; } = [];

    public float Draw(ImDrawListPtr drawList, Vector2 point, bool pass, ActionSetting? activeAction, ref TimeSpan time)
    {
        if (Plugin.Settings.DrawTime)
        {
            var width = ImGui.CalcTextSize(time.GetString()).X + 5;
            drawList.AddText(point, uint.MaxValue, time.GetString() + "   ");
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
