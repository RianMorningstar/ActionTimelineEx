﻿using ActionTimelineEx.Configurations;
using ActionTimelineEx.Timeline;
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
    public static ActionSetting? ActiveAction => Actions.FirstOrDefault();

    public static  IEnumerable<ActionSetting> Actions => RotationSetting.Actions.Skip((int)Count);

    public static RotationSetting RotationSetting => Plugin.Settings.GetSetting(Svc.ClientState.TerritoryType).RotationSetting;

    private static uint _count;
    public static uint Count 
    {
        get => _count;
        private set
        {
            if (_count == value) return;
            _count = value;

            UpdateHighlight();
        }
    }
    public static uint SuccessCount { get; private set; } = 0;

    private static void UpdateHighlight()
    {
        if (!Plugin.Settings.DrawRotation) return;

        if (_highLight == null) return;
        _highLight.Color = Plugin.Settings.RotationHighlightColor;
        _highLight.HotbarIDs.Clear();

        var action = ActiveAction;
        if (action == null) return;

        HotbarID? hotbar = null;

        switch (action.Type)
        {
            case ActionSettingType.Action:
                var isGAction = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(action.ActionId)?.ActionCategory.Row is 10 or 11;
                if (isGAction)
                {
                    var gAct = Svc.Data.GetExcelSheet<GeneralAction>()?.FirstOrDefault(g => g.Action.Row == action.ActionId);
                    if (gAct != null)
                    {
                        hotbar = new HotbarID(FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType.GeneralAction, gAct.RowId);
                        break;
                    }
                }

                hotbar = new HotbarID(FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType.Action, action.ActionId);
                break;

            case ActionSettingType.Item:
                hotbar = new HotbarID(FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType.Item, action.ActionId);
                break;
        }

        if (hotbar == null) return;
        _highLight.HotbarIDs.Add(hotbar.Value);
    }

    public static void Init()
    {
        ActionEffect.ActionEffectEvent += ActionFromSelf;
        Svc.DutyState.DutyWiped += DutyState_DutyWiped;
        Svc.DutyState.DutyCompleted += DutyState_DutyWiped;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
        ClientState_TerritoryChanged(Svc.ClientState.TerritoryType);
        _highLight = new();
        UpdateHighlight();
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
        var territory = Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(obj);
        if (IsTerritoryTypeValid(territory))
        {
            RotationHelperItem._territoryId = obj;
        }
        Clear();
    }

    private static void DutyState_DutyWiped(object? sender, ushort e)
    {
        Clear();
    }

    private static void ActionFromSelf(ActionEffectSet set)
    {
        if (!Player.Available) return;
        if (set.Source.EntityId != Player.Object.EntityId || !Plugin.Settings.DrawRotation) return;
        if ((ActionCate)(set.Action?.ActionCategory.Value?.RowId ?? 0) is ActionCate.AutoAttack) return; //Auto Attack.

        if (Plugin.Settings.RecordRotation)
        {
            RecordRotation(set);
            return;
        }

        var action = ActiveAction;
        if (action == null) return;

        bool succeed = set.Header.ActionID == action.ActionId;
        switch (set.Header.ActionType)
        {
            case FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action when action.Type != ActionSettingType.Action:
            case FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item when action.Type != ActionSettingType.Item:
                succeed = false;
                break;

        }
        if (succeed)
        {
            SuccessCount++;
        }
        else if(Plugin.Settings.ShowWrongClick)
        {
            Svc.Chat.PrintError($"Clicked the wrong action {set.Name}! You should Click {action.DisplayName}!");
        }
        Count++;
    }

    private static void RecordRotation(in ActionEffectSet set)
    {
        ActionSettingType? type = null;

        switch (set.Header.ActionType)
        {
            case FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action:
                type = ActionSettingType.Action;
                break;

            case FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item:
                type = ActionSettingType.Item;
                break;
        }

        if (type != null)
        {
            RotationSetting.Actions.Add(new ActionSetting()
            {
                ActionId = set.Header.ActionID,
                Type = type.Value,
            });
        }
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
