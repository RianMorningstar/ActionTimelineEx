using ActionTimeline;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Windows;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using Lumina.Excel.GeneratedSheets;
using XIVDrawer.ElementSpecial;

namespace ActionTimelineEx.Helpers;

internal static class RotationHelper
{
    private static DrawingHighlightHotbar? _highLight;
    public static ActionSetting? ActiveActon => Actions.FirstOrDefault();

    public static  IEnumerable< ActionSetting> Actions => RotationSetting.Actions.Skip((int)Count);

    public static RotationSetting RotationSetting => Plugin.Settings.GetSetting(Svc.ClientState.TerritoryType).RotationSetting;

    private static uint _count;
    public static uint Count 
    {
        get => _count;
        private set
        {
            if (_count == value) return;
            _count = value;

            if (_highLight == null) return;
            _highLight.Color = Plugin.Settings.RotationHighlightColor;
            _highLight.HotbarIDs.Clear();

            var action = ActiveActon;
            if (action == null) return;

            HotbarID? hotbar = null;

            switch (action.Type)
            {
                case ActionSettingType.Action:
                    hotbar = new HotbarID(FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType.Action, action.ActionId);
                    break;

                case ActionSettingType.Item:
                    hotbar = new HotbarID(FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType.Item, action.ActionId);
                    break;
            }

            if (hotbar == null) return;
            _highLight.HotbarIDs.Add(hotbar.Value);
        }
    }
    public static uint SuccessCount { get; private set; } = 0;

    public static void Init()
    {
        ActionEffect.ActionEffectEvent += ActionFromSelf;
        Svc.DutyState.DutyWiped += DutyState_DutyWiped;
        Svc.DutyState.DutyCompleted += DutyState_DutyWiped;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

        _highLight = new();
    }

    public static void Dispose()
    {
        ActionEffect.ActionEffectEvent -= ActionFromSelf;
        Svc.DutyState.DutyWiped -= DutyState_DutyWiped;
        Svc.DutyState.DutyCompleted -= DutyState_DutyWiped;
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
    }

    private static void ClientState_TerritoryChanged(ushort obj)
    {
        Clear();

        var territory = Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(obj);
        if (IsTerritoryTypeValid(territory))
        {
            RotationHelperItem._territoryId = obj;
        }
    }

    private static void DutyState_DutyWiped(object? sender, ushort e)
    {
        Clear();
    }

    private static void ActionFromSelf(ActionEffectSet set)
    {
        if (!Player.Available) return;
        if (set.Source.EntityId != Player.Object.EntityId || !Plugin.Settings.DrawRotation) return;
        if (set.Action == null) return;

        var action = ActiveActon;
        if (action == null) return;

        var succeed = set.Action.RowId == action.ActionId;
        if (succeed)
        {
            SuccessCount++;
        }
        else
        {
            Svc.Chat.Print("Failed to click the action!");
        }
        Count++;
    }

    public static void Clear()
    {
        Count = 0;
    }

    public static bool IsTerritoryTypeValid(TerritoryType? territory)
    {
        if (territory == null) return false;
        if (territory.BattalionMode == 0) return false;
        if (territory.IsPvpZone) return false;
        if (territory.PlaceName.Row == 0) return false;

        return true;
    }
}
