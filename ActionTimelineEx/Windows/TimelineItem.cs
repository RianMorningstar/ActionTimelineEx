using ActionTimelineEx.Configurations;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using XIVConfigUI;

namespace ActionTimelineEx.Windows;

internal class TimelineItem(DrawingSettings setting, Action clearItems) : ConfigWindowItem
{
    internal readonly SearchableCollection Collection = new(setting);
    private CollapsingHeaderGroup? _extraHeader;

    public override string Name => setting.Name;

    public override bool GetIcon(out IDalamudTextureWrap texture)
    {
        return ImageLoader.GetTexture(42, out texture);
    }

    public override void Draw(ConfigWindow window)
    {
        if (RemoveValue(setting.Name))
        {
            Plugin.Settings.TimelineSettings.Remove(setting);
            clearItems();
        }

        _extraHeader ??= new(new()
            {
                { () => GroupItem.General.Local(), () => Collection.DrawItems((int)GroupItem.General) },
                { () => GroupItem.Icons.Local(), () => Collection.DrawItems((int)GroupItem.Icons) },
                { () => GroupItem.Bar.Local(), () => Collection.DrawItems((int)GroupItem.Bar) },
                { () => GroupItem.Grid.Local(), () => Collection.DrawItems((int)GroupItem.Grid) },
                { () => GroupItem.GcdClipping.Local(), () => Collection.DrawItems((int)GroupItem.GcdClipping) },
            });
        _extraHeader?.Draw();
    }

    private static string _undoName = string.Empty;
    private static DateTime _lastTime = DateTime.MinValue;
    internal static bool RemoveValue(string name)
    {
        bool isLast = false, isTime = false;

        if (_lastTime != DateTime.MinValue)
        {
            isLast = name == _undoName && DateTime.Now - _lastTime < TimeSpan.FromSeconds(2);
            isTime = DateTime.Now - _lastTime > TimeSpan.FromSeconds(0.5);
        }

        bool result = false;

        if (isLast) ImGui.PushStyleColor(ImGuiCol.Text, isTime ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed);

        ImGui.Text(UiString.RemoveDesc.Local());
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{(isLast ? FontAwesomeIcon.Check : FontAwesomeIcon.Ban).ToIconString()}##Remove{name}"))
        {
            if (isLast && isTime)
            {
                result = true;
                _lastTime = DateTime.MinValue;
            }
            else
            {
                _lastTime = DateTime.Now;
                _undoName = name;
            }
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(!isTime ? UiString.Wait.Local()
                : isLast ? UiString.Confirm.Local()
                : UiString.Remove.Local());
        }

        if (isLast) ImGui.PopStyleColor();
        return result;
    }
}
