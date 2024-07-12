using ActionTimeline.Helpers;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Common.Lua;
using ImGuiNET;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;
using XIVConfigUI.Attributes;

namespace ActionTimelineEx.Configurations;

internal class ActionSettingAttribute() : ListUIAttribute(0)
{
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

        //TODO: Change the acion ID...
    }
}

public enum ActionSettingType : byte
{
    Action,
    Item,
}

[ActionSetting]
public class ActionSetting
{
    internal uint IconId { get; private set; } = 0;
    internal bool IsGCD { get; private set; } = false;

    [UI("Name")]
    internal string DisplayName { get; private set; } = "";

    private uint _actionId;

    [UI("Id")]
    public uint ActionId 
    {
        get => _actionId;
        set
        {
            if (value == _actionId) return;
            _actionId = value;

            Update();
        }
    }

    private ActionSettingType _type;

    [UI("Type")]
    public ActionSettingType Type
    {
        get => _type;
        set
        {
            if (value == _type) return;
            _type = value;

            Update();
        }
    }

    private void Update()
    {
        ClearData();

        switch (Type)
        {
            case ActionSettingType.Action:
                UpdateAction();
                return;

            case ActionSettingType.Item:
                UpdateItem();
                return;
        }

        void UpdateItem()
        {
            var item = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(ActionId);
            if (item == null) return;

            IconId = item.Icon;
            DisplayName = item.Name;
        }

        void UpdateAction()
        {
            var action = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(ActionId);
            if (action == null) return;

            IsGCD = action.CooldownGroup == 58 || action.AdditionalCooldownGroup == 58;

            IconId = action.Icon;
            DisplayName = $"{action.Name} ({(IsGCD ? "GCD" : "Ability")})";
        }

        void ClearData()
        {
            IconId = 0;
            DisplayName = string.Empty;
            IsGCD = false;
        }
    }

    public void Draw(ImDrawListPtr drawList, Vector2 point, float size)
    {
        drawList.DrawActionIcon(IconId, Type is ActionSettingType.Item, point, size);
        if (!string.IsNullOrEmpty(DisplayName) && DrawHelper.IsInRect(point, new Vector2(size))) ImGui.SetTooltip(DisplayName);
    }
}
