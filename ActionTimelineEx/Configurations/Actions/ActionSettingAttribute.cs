using ActionTimelineEx.Helpers;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.Attributes;
using XIVConfigUI.ConditionConfigs;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace ActionTimelineEx.Configurations.Actions;
internal class ActionSettingAttribute() : ListUIAttribute(0)
{
    private TimeSpan _time = TimeSpan.Zero;
    public override string DrawIndex(object obj, int index)
    {
        if (obj is not GCDAction setting) return string.Empty;

        if (index == 0) _time = TimeSpan.Zero;

        var result = $"{(int)_time.TotalMinutes}:{_time.Seconds:D2}.{_time.Milliseconds.ToString()[0]}";

        var recastTime = Svc.Data.GetExcelSheet<Action>()?.GetRow(setting.ActionId)?.Recast100ms ?? 0;

        var time = setting.GcdOverride == 0
            ? Plugin.Settings.RotationHelper.GcdTime / 2.5f * recastTime / 10f
            : setting.GcdOverride;

        _time = _time.Add(TimeSpan.FromSeconds(time));
        return result;
    }

    public override uint GetIcon(object obj)
    {
        if (obj is not ActionSetting setting)
            return base.GetIcon(obj);
        return setting.IconId;
    }

    public override void OnClick(object obj)
    {
        base.OnClick(obj);
        if (obj is not ActionSetting setting) return;

        ImGui.OpenPopup(setting.GetHashCode().ToString());
    }

    public override void OnTick(object obj)
    {
        base.OnTick(obj);

        if (obj is not ActionSetting setting) return;

        switch (setting.Type)
        {
            case ActionSettingType.Action:
                ActionSelectorPopup(setting, setting.GetHashCode().ToString(), setting is GCDAction);
                break;

            case ActionSettingType.Item:
                ItemSelectorPopup(setting, setting.GetHashCode().ToString());
                break;
        }
    }

    public override string GetDescription(object obj)
    {
        if (obj is not ActionSetting setting) return base.GetDescription(obj);
        return setting.DisplayName;
    }

    private static readonly CollapsingHeaderGroup _group = new()
    {
        HeaderSize = FontSize.Fifth,
    };

    private const int count = 8;
    private static void ItemSelectorPopup(ActionSetting setting, string popUpId)
    {
        using var popUp = ImRaii.Popup(popUpId);
        if (!popUp.Success) return;

        var actions = Svc.Data.GetExcelSheet<Item>()?.Where(i =>
        {
            if(i.ItemSearchCategory.Row != 43) return false;
            unsafe
            {
                if (InventoryManager.Instance()->GetInventoryItemCount(i.RowId, true) > 0) return true;
                if (InventoryManager.Instance()->GetInventoryItemCount(i.RowId, false) > 0) return true;
            }
            return false;
        });

        if (actions == null || !actions.Any()) return;

        var index = 0;
        foreach (var item in actions.OrderBy(t => t.RowId))
        {
            if (!ImageLoader.GetTexture(item.Icon, out var icon)) continue;

            if (index++ % count != 0)
            {
                ImGui.SameLine();
            }

            using (var group = ImRaii.Group())
            {
                var cursor = ImGui.GetCursorPos();
                if (ImGuiHelper.NoPaddingNoColorImageButton(icon.ImGuiHandle, Vector2.One * ConditionDrawer.IconSize, item.GetHashCode().ToString()))
                {
                    setting.ActionId = item.RowId;
                    ImGui.CloseCurrentPopup();
                }
                ImGuiHelper.DrawActionOverlay(cursor, ConditionDrawer.IconSize, 1);
            }

            var name = item.Name;
            if (!string.IsNullOrEmpty(name))
            {
                ImGuiHelper.HoveredTooltip(name);
            }
        }
    }
    private static void ActionSelectorPopup(ActionSetting setting, string popUpId, bool isGcd)
    {
        using var popUp = ImRaii.Popup(popUpId);
        if (!popUp.Success) return;

        var actions = Svc.Data.GetExcelSheet<Action>()?.Where(a => a.IsInJob() && !a.IsPvP && a.IsGcd() == isGcd);

        if (actions == null || !actions.Any()) return;

        _group.ClearCollapsingHeader();

        foreach (var action in actions.GroupBy(i => i.GetActionType()).OrderBy(i => i.Key))
        {
            _group.AddCollapsingHeader(() => action.Key.Local(), () =>
            {
                var index = 0;
                foreach (var item in action.OrderBy(t => t.RowId))
                {
                    if (!ImageLoader.GetTexture(item.Icon, out var icon)) continue;

                    if (index++ % count != 0)
                    {
                        ImGui.SameLine();
                    }

                    using (var group = ImRaii.Group())
                    {
                        var cursor = ImGui.GetCursorPos();
                        if (ImGuiHelper.NoPaddingNoColorImageButton(icon.ImGuiHandle, Vector2.One * ConditionDrawer.IconSize, item.GetHashCode().ToString()))
                        {
                            setting.ActionId = item.RowId;
                            ImGui.CloseCurrentPopup();
                        }
                        ImGuiHelper.DrawActionOverlay(cursor, ConditionDrawer.IconSize, 1);
                    }

                    var name = item.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        ImGuiHelper.HoveredTooltip(name);
                    }
                }
            });
        }

        _group.Draw();
    }
}

