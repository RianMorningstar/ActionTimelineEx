using ActionTimelineEx.Helpers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
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
        using var bgColorPush = ImRaii.PushColor(ImGuiCol.WindowBg, bgColor);

        ImGui.SetNextWindowSize(new Vector2(560, 100) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(200, 200) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);


        if (ImGui.Begin("Rotation Helper Window", flag))
        {
            var padding = ImGui.GetStyle().WindowPadding;
            var border = ImGui.GetStyle().WindowBorderSize;
            ImGui.GetStyle().WindowPadding = default;
            ImGui.GetStyle().WindowBorderSize = 0;
            try
            {
                DrawContent();
            }
            finally
            {
                ImGui.End();
                ImGui.GetStyle().WindowPadding = padding;
                ImGui.GetStyle().WindowBorderSize = border;
            }
        }
    }

    private static void DrawContent()
    {
        var gcdHeight = Plugin.Settings.GCDIconSize;
        var ogcdHeight = Plugin.Settings.OGCDIconSize;
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

        bool isFirst = true;
        foreach (var item in RotationHelper.Actions)
        {
            var size = item.IsGCD ? gcdHeight : ogcdHeight;

            if (item.IsGCD && Plugin.Settings.VerticalDraw && !isFirst)
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

            item.Draw(drawList, pos, size);
            if (isFirst)
            {
                drawList.DrawSlotHighlight(pos, size, ImGui.ColorConvertFloat4ToU32(Plugin.Settings.RotationHighlightColor));
                isFirst = false;
            }

            pos += new Vector2(size + spacing, 0);

            if (pos.X >= maxX || pos.Y >= maxY || pos.Y <= minY) break;
        }
    }
}
