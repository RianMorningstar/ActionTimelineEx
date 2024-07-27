using ActionTimelineEx.Configurations.Actions;
using ActionTimelineEx.Helpers;
using ExCSS;
using ImGuiNET;
using System;
using System.Numerics;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;
public class RotationSetting
{
    [UI("Rotation Name")]
    public string Name { get; set; } = "Default";

    [UI]
    public List<GCDAction> GCDs { get; set; } = [];

    [UI("Ignore Actions")]
    public List<ActionSetting> IgnoreActions { get; set; } = [];

    public ActionSetting? GetNextAction(int index, byte subIndex)
    {
        if (GCDs.Count == 0) return null;

        var thisIndex = index - 1;
        if (thisIndex >= GCDs.Count) return null;

        var thisGcd = GCDs[Math.Max(0, thisIndex)];

        if (thisGcd == null) return null;
        if (index == 0) return thisGcd;

        var result = thisGcd.oGCDs.Where(i => !i.IsEmpty).Skip(subIndex).FirstOrDefault();

        if (result != null) return result;

        if (index >= GCDs.Count) return null;
        return GCDs[Math.Max(0, index)];
    }

    public void Draw(float heightReduce)
    {
        var gcdHeight = Plugin.Settings.GCDIconSize;
        var spacing = Plugin.Settings.IconSpacing;
        var drawList = ImGui.GetWindowDrawList();

        var wholeHeight = ImGui.GetWindowSize().Y - heightReduce;
        var windowPos = ImGui.GetWindowPos() + Vector2.UnitY * heightReduce;

        var pos = windowPos + new Vector2(gcdHeight * 0.2f,
            Plugin.Settings.VerticalDraw
            ? (Plugin.Settings.Reverse
                ? wholeHeight - gcdHeight * 1.4f
                : gcdHeight * 0.2f)
            : wholeHeight / 2 - gcdHeight / 2);

        var maxX = windowPos.X + ImGui.GetWindowSize().X;
        var minY = windowPos.Y;
        var maxY = minY + wholeHeight;
        var minPosX = pos.X;

        var nextAction = RotationHelper.ActiveAction;

        TimeSpan span = TimeSpan.Zero;
        bool isNotFirst = false;

        for (var i = 0; i < GCDs.Count; i++)
        {
            var item = GCDs[i];

            if (i < RotationHelper.GcdUsedCount - 1)
            {
                span += TimeSpan.FromSeconds(item.Gcd);
                continue;
            }

            if (Plugin.Settings.VerticalDraw && isNotFirst)
            {
                pos.X = minPosX;

                if (Plugin.Settings.Reverse)
                {
                    pos.Y -= gcdHeight + spacing;
                }
                else
                {
                    pos.Y += gcdHeight + spacing;
                }
            }

            isNotFirst = true;

            var width = item.Draw(drawList, pos, i < RotationHelper.GcdUsedCount, nextAction, ref span);

            pos += new Vector2(width + spacing, 0);

            if (pos.X >= maxX || pos.Y >= maxY || pos.Y <= minY) break;
        }
    }
}
