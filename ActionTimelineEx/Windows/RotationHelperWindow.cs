using ActionTimelineEx.Helpers;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Numerics;
using XIVConfigUI;

namespace ActionTimelineEx.Windows;
internal static class RotationHelperWindow
{
    public static void Draw()
    {
        var setting = Plugin.Settings;
        if (!setting.DrawRotation) return;

        var flag = TimelineWindow._baseFlags;
        if (setting.RotationLocked)
        {
            flag |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs;
        }

        Vector4 bgColor = setting.RotationLocked ? setting.RotationLockedBackgroundColor : setting.RotationUnlockedBackgroundColor;
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);

        ImGui.SetNextWindowSize(new Vector2(560, 100) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(200, 200) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

        if (ImGui.Begin("Rotation Helper Window", flag))
        {
            DrawContent();
            ImGui.End();
        }

        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    private static void DrawContent()
    {
        var gcdHeight = Plugin.Settings.GCDIconSize;
        var ogcdHeight = Plugin.Settings.OGCDIconSize;
        var spacing = Plugin.Settings.IconSpacing;
        var drawList = ImGui.GetWindowDrawList();

        var pos = ImGui.GetWindowPos() + new Vector2(gcdHeight * 0.2f,
            Plugin.Settings.VerticalDraw ? gcdHeight * 0.2f : ImGui.GetWindowSize().Y / 2 - gcdHeight / 2);
        var maxX = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X;
        var maxY = ImGui.GetWindowPos().Y + ImGui.GetWindowSize().Y;
        var minPosX = pos.X;

        bool isFirst = true;
        foreach (var item in RotationHelper.Actions)
        {
            var size = item.IsGCD ? gcdHeight : ogcdHeight;

            if (item.IsGCD && Plugin.Settings.VerticalDraw && !isFirst)
            {
                pos.X = minPosX;
                pos.Y += gcdHeight + spacing;
            }

            item.Draw(drawList, pos, size);
            if (isFirst)
            {
                drawList.DrawSlotHighlight(pos, size, ImGui.ColorConvertFloat4ToU32(Plugin.Settings.RotationHighlightColor));
                isFirst = false;
            }

            pos += new Vector2(size + spacing, 0);

            if (pos.X >= maxX || pos.Y >= maxY) break;
        }
    }
}
