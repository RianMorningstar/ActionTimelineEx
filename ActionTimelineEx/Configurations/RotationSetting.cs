using ActionTimelineEx.Configurations.Actions;
using ActionTimelineEx.Helpers;
using ImGuiNET;
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

    public void Draw()
    {
        var gcdHeight = Plugin.Settings.GCDIconSize;
        var spacing = Plugin.Settings.IconSpacing;
        var drawList = ImGui.GetWindowDrawList();

        var pos = ImGui.GetWindowPos() + new Vector2(gcdHeight * 0.2f,
            Plugin.Settings.VerticalDraw
            ? (Plugin.Settings.Reverse
                ? ImGui.GetWindowSize().Y - gcdHeight * 1.4f
                : gcdHeight * 0.2f)
            : ImGui.GetWindowSize().Y / 2 - gcdHeight / 2);
        var maxX = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X;
        var minY = ImGui.GetWindowPos().Y;
        var maxY = minY + ImGui.GetWindowSize().Y;
        var minPosX = pos.X;

        var nextAction = RotationHelper.ActiveAction;

        for (var i = 0; i < GCDs.Count; i++)
        {
            if (i < RotationHelper.GcdUsedCount - 1) continue;

            var item = GCDs[i];

            if (Plugin.Settings.VerticalDraw && i != 0)
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

            var width = item.Draw(drawList, pos, i < RotationHelper.GcdUsedCount, nextAction);

            pos += new Vector2(width + spacing, 0);

            if (pos.X >= maxX || pos.Y >= maxY || pos.Y <= minY) break;
        }
    }
}
