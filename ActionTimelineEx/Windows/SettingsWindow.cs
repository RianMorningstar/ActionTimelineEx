using ActionTimeline.Helpers;
using ActionTimeline.Timeline;
using ActionTimelineEx.Configurations;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using ECommons.Commands;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.SearchableConfigs;

namespace ActionTimeline.Windows;

public class SettingsWindow : ConfigWindow
{
    public override IEnumerable<Searchable> Searchables
    {
        get
        {
            if(ActiveItem is TimelineItem item)
            {
                return [..base.Searchables, ..item.Collection];
            }
            else
            {
                return base.Searchables;
            }
        }
    }

    internal class TimelineItem(DrawingSettings setting, System.Action clearItems) : ConfigWindowItem
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
                Settings.TimelineSettings.Remove(setting);
                clearItems();
            }

            _extraHeader ??= new(new()
            {
                { () =>GroupItem.General.Local(), () => Collection.DrawItems((int)GroupItem.General) },
                { () =>GroupItem.Icons.Local(), () => Collection.DrawItems((int)GroupItem.Icons) },
                { () =>GroupItem.Bar.Local(), () => Collection.DrawItems((int)GroupItem.Bar) },
                { () =>GroupItem.Grid.Local(), () => Collection.DrawItems((int)GroupItem.Grid) },
                { () =>GroupItem.GcdClipping.Local(), () => Collection.DrawItems((int)GroupItem.GcdClipping) },
            });
            _extraHeader?.Draw();
        }

        private string _undoName = string.Empty;
        private DateTime _lastTime = DateTime.MinValue;
        private bool RemoveValue(string name)
        {
            bool isLast = name == _undoName && DateTime.Now - _lastTime < TimeSpan.FromSeconds(2);
            bool isTime = DateTime.Now - _lastTime > TimeSpan.FromSeconds(0.5);

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
    private static float _scale => ImGuiHelpers.GlobalScale;
    public override SearchableCollection Collection { get; } = new(Settings);
    protected override string Kofi => "B0B0IN5DX";

    protected override string DiscordServerID => "1228953752585637908";
    protected override string DiscordServerInviteLink => "9D4E8eZW5g";

    protected override string Crowdin => "actiontimelineex";
    private static Settings Settings => Plugin.Settings;
    public SettingsWindow() : base(typeof(SettingsWindow).Assembly.GetName())
    {
        Size = new Vector2(300, 490f);
        RespectCloseHotkey = true;

        ImGuiHelper.GetFont(FontSize.Third, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Forth, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Fifth, GameFontFamily.Axis);
        ImGuiHelper.GetFont(FontSize.Forth, GameFontFamily.MiedingerMid);
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
            { () => UiString.Help.Local(), () => CmdManager.DrawHelp() },
        });


        base.DrawAbout();

        _aboutHeaders.Draw();
    }

    private void DrawSetting()
    {
        if (ImGui.Button(UiString.AddOne.Local()))
        {
            Settings.TimelineSettings.Add(new DrawingSettings()
            {
                Name = (Settings.TimelineSettings.Count + 1).ToString(),
            });
            ClearItems();
        }

        Collection.DrawItems(0);
    }

    protected override ConfigWindowItem[] GetItems()
    {
        return [..Settings.TimelineSettings.Select(i => new TimelineItem(i, ClearItems))];
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
