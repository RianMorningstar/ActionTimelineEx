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
        RotationHelper.RotationSetting.Draw();
    }
}
