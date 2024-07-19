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
    private static Vector2 _size = default;
    private static bool _open = true, _changed = false;
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
        if (!_open)
        {
            flag |= ImGuiWindowFlags.AlwaysAutoResize;
        }

        Vector4 bgColor = setting.RotationLocked ? setting.RotationLockedBackgroundColor : setting.RotationUnlockedBackgroundColor;
        using var bgColorPush = ImRaii.PushColor(ImGuiCol.WindowBg, bgColor);

        ImGui.SetNextWindowSize(new Vector2(560, 100) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);

        if (_changed)
        {
            ImGui.SetNextWindowSize(_size);
            _changed = false;
        }

        if (ImGui.Begin("Rotation Helper Window", flag))
        {
            var rotations = Plugin.Settings.RotationHelper;

            if (ImGui.Checkbox("##Open", ref _open))
            {
                if (_open)
                {
                    _changed = true;
                    ImGui.End();
                    return;
                }
                else
                {
                    _size = ImGui.GetWindowSize();
                }
            }

            if (_open)
            {
                ImGui.SameLine();
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
    }

    private static void DrawContent()
    {
        RotationHelper.RotationSetting.Draw();
    }
}
