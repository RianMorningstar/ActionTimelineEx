using ActionTimelineEx.Helpers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ImGuiNET;
using System.Numerics;

namespace ActionTimelineEx.Windows;
internal static class RotationHelperWindow
{
    public static void Draw()
    {
        var setting = Plugin.Settings;
        if (!setting.DrawRotation) return;

        unsafe
        {
            if (setting.OnlyShowRotationWhenWeaponOn
                && !Player.BattleChara->IsWeaponDrawn) return;
        }

        var flag = TimelineWindow._baseFlags;
        if (setting.RotationLocked)
        {
            flag |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        }

        Vector4 bgColor = setting.RotationLocked ? setting.RotationLockedBackgroundColor : setting.RotationUnlockedBackgroundColor;
        using var bgColorPush = ImRaii.PushColor(ImGuiCol.WindowBg, bgColor);

        ImGui.SetNextWindowSize(new Vector2(560, 100) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(200, 200) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);


        if (ImGui.Begin("Rotation Helper Window", flag))
        {
            //Double click to clear.
            if (DrawHelper.IsInRect(ImGui.GetWindowPos(), ImGui.GetWindowSize()) && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                RotationHelper.Clear();
            }

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
