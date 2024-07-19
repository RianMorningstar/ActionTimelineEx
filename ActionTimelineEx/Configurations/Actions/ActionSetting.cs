using ActionTimelineEx.Helpers;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.Attributes;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace ActionTimelineEx.Configurations.Actions;


/// <summary>
/// From https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/ActionManager.cs#L373-L395
/// </summary>
public enum ActionSettingType : byte
{
    Action = 0x01, // Spell, Weaponskill, Ability. Confusing name, I know.
    Item = 0x02,
}

[ActionSetting]
public abstract class ActionSetting
{
    internal uint IconId { get; private set; } = 0;
    internal string DisplayName { get; private set; } = "";

    private uint _actionId;

    public bool IsEmpty => ActionId == 0;

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

    [UI("Action ID")]
    public int ActionID { get => (int) ActionId; set => ActionId = (uint)value; }

    internal abstract  ActionSettingType Type { get; }

    [UI("Is this Action Highlight")]
    public bool Highlight { get; set; }

    protected void Update()
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
            var item = Svc.Data.GetExcelSheet<Item>()?.GetRow(ActionId);
            if (item == null) return;

            IconId = IsEmpty ? 0u : item.Icon;
            DisplayName = item.Name;
        }

        void UpdateAction()
        {
            var action = Svc.Data.GetExcelSheet<Action>()?.GetRow(ActionId);
            if (action == null) return;

            IconId = IsEmpty ? 0u : GetActionIcon(action);
            DisplayName = $"{action.Name} ({(action.IsGcd() ? "GCD" : "oGCD")})";
        }

        void ClearData()
        {
            IconId = 0;
            DisplayName = string.Empty;
        }
    }

    private static uint GetActionIcon(Action action)
    {
        var isGAction = action.ActionCategory.Row is 10 or 11;
        if (!isGAction) return action.Icon;

        var gAct = Svc.Data.GetExcelSheet<GeneralAction>()?.FirstOrDefault(g => g.Action.Row == action.RowId);

        if (gAct == null) return action.Icon;
        return (uint)gAct.Icon;
    }

    public bool IsMatched(uint id, ActionSettingType type)
    {
        return id == ActionId && type == Type;
    }

    public void DrawIcon(ImDrawListPtr drawList, Vector2 point, float size, bool passed, ActionSetting? activeAction)
    {
        if (passed)
        {
            if (!RotationHelper.SuccessActions.Contains(this))
            {
                ImGuiHelper.DrawSlotHighlight(drawList, point, size, ImGui.ColorConvertFloat4ToU32(Plugin.Settings.RotationFailedColor));
            }
        }

        IconType iconType = passed ? IconType.Blacked
            : Highlight ? IconType.Highlight : IconType.Normal;
        drawList.DrawActionIcon(IconId, Type is ActionSettingType.Item, point, size, iconType);
        if (!string.IsNullOrEmpty(DisplayName) && DrawHelper.IsInRect(point, new Vector2(size)))
            ImGui.SetTooltip(DisplayName);

        if (!passed && activeAction == this)
        {
            ImGuiHelper.DrawSlotHighlight(drawList, point, size, ImGui.ColorConvertFloat4ToU32(Plugin.Settings.RotationHighlightColor));
        }
    }
}
