using ActionTimelineEx.Helpers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
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
        using var border = ImRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 0);
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        if (ImGui.Begin("Rotation Helper Window", flag))
        {
            var rotations = Plugin.Settings.RotationHelper;

            var index = rotations.ChoiceIndex;
            if (ImGuiHelper.SelectableCombo("Change Rotation", [.. rotations.RotationSettings.Select(i => i.Name)], ref index))
            {
                rotations.ChoiceIndex = index;
            }

            //Double click to clear.
            if (DrawHelper.IsInRect(ImGui.GetWindowPos(), ImGui.GetWindowSize()) && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                RotationHelper.Clear();
            }

            var heightReduce = ImGui.GetCursorPosY();
            try
            {
                RotationHelper.RotationSetting.Draw(heightReduce);
            }
            finally
            {
                ImGui.End();
            }
        }
    }
}
