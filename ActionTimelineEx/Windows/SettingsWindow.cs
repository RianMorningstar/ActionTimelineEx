using ActionTimelineEx;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Helpers;
using ActionTimelineEx.Timeline;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Utility;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.SearchableConfigs;

namespace ActionTimelineEx.Windows;

public class SettingsWindow : ConfigWindow
{
    public override IEnumerable<Searchable> Searchables
    {
        get
        {
            if (ActiveItem is TimelineItem item)
            {
                return [.. base.Searchables, .. item.Collection];
            }
            else
            {
                return base.Searchables;
            }
        }
    }

    private static float _scale => ImGuiHelpers.GlobalScale;
    public override SearchableCollection Collection { get; } = new(Settings);
    protected override bool ShowDonate => Settings.ShowDonate;
    protected override string Kofi => "B0B0IN5DX";
    protected override string Crowdin => "actiontimelineex";
    private static Settings Settings => Plugin.Settings;
    public SettingsWindow() : base(typeof(SettingsWindow).Assembly.GetName())
    {
        Size = new Vector2(740, 490f);
        RespectCloseHotkey = true;

        ImGuiHelper.GetFont(FontSize.Third, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Fourth, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Fifth, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Fourth, GameFontFamily.MiedingerMid);
        ImGuiHelper.GetFont(FontSize.Fifth, GameFontFamily.MiedingerMid);
    }

    public override void OnClose()
    {
        Settings.Save();
        base.OnClose();
    }

    private CollapsingHeaderGroup? _aboutHeaders = null;
    protected override void DrawAbout()
    {
        _aboutHeaders ??= new(new()
        {
            { () => UiString.Setting.Local(),DrawSetting},
            { () => UiString.ShowedStatuses.Local(), DrawShowedStatues},
            { () => UiString.NotStatues.Local(), DrawGeneralSetting},
        });

        base.DrawAbout();

        if (ImGui.Button(UiString.AddOne.Local()))
        {
            Settings.TimelineSettings.Add(new DrawingSettings()
            {
                Name = (Settings.TimelineSettings.Count + 1).ToString(),
            });
            ClearItems();
        }

        _aboutHeaders.Draw();
    }

    private void DrawSetting()
    {
        Collection.DrawItems(0);
    }

    protected override ConfigWindowItem[] GetItems()
    {
        return
        [
            new RotationHelperItem(),
            ..Settings.TimelineSettings.Select(i => new TimelineItem(i, ClearItems)),
            new ChangeLogItem(),
        ];
    }

    private void DrawShowedStatues()
    {
        var index = 0;

        foreach (var statusId in TimelineManager.ShowedStatusId)
        {
            var status = Svc.Data.GetExcelSheet<Status>()?.GetRow(statusId);
            var texture = DrawHelper.GetTextureFromIconId(status?.Icon ?? 0);
            if (texture != null)
            {
                ImGui.Image(texture.ImGuiHandle, new Vector2(18, 24));
                var tips = $"{status?.Name ?? string.Empty} [{status?.RowId ?? 0}]";
                ImGuiHelper.HoveredTooltip(tips);
                if (++index % 10 != 0) ImGui.SameLine();
            }
        }
    }

    private ushort _aboutAdd = 0;
    private void DrawGeneralSetting()
    {
        ushort removeId = 0, addId = 0;
        var index = 0;
        foreach (var statusId in Plugin.Settings.HideStatusIds)
        {
            var status = Svc.Data.GetExcelSheet<Status>()?.GetRow(statusId);
            var texture = DrawHelper.GetTextureFromIconId(status?.Icon ?? 0);
            if (texture != null)
            {
                ImGui.Image(texture.ImGuiHandle, new Vector2(24, 30));
                ImGuiHelper.HoveredTooltip(status?.Name ?? string.Empty);
                ImGui.SameLine();
            }

            var id = statusId.ToString();
            ImGui.SetNextItemWidth(100 * _scale);
            if (ImGui.InputText($"##Status{index++}", ref id, 8) && ushort.TryParse(id, out var newId))
            {
                removeId = statusId;
                addId = newId;
            }

            ImGui.SameLine();

            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Ban.ToIconString()}##Remove{statusId}"))
            {
                removeId = statusId;
            }
            ImGui.PopFont();
        }
        var oneId = string.Empty;
        ImGui.SetNextItemWidth(100 * _scale);
        if (ImGui.InputText($"##AddOne", ref oneId, 8) && ushort.TryParse(oneId, out var newOneId))
        {
            _aboutAdd = newOneId;
        }
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddNew"))
        {
            addId = _aboutAdd;
        }
        ImGui.PopFont();

        if (removeId != 0)
        {
            Plugin.Settings.HideStatusIds.Remove(removeId);
        }
        if (addId != 0)
        {
            Plugin.Settings.HideStatusIds.Add(addId);
        }
    }
}
