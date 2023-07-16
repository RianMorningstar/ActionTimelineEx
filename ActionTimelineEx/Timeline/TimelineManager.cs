using ActionTimelineEx.Timeline;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using RotationSolver.Basic.Data;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Status = Lumina.Excel.GeneratedSheets.Status;

namespace ActionTimeline.Timeline;

public class TimelineManager
{
    internal const byte GCDCooldownGroup = 58;

    #region singleton
    public static void Initialize() { Instance = new TimelineManager(); }

    public static TimelineManager Instance { get; private set; } = null!;

    public TimelineManager()
    {
        try
        {
            SignatureHelper.Initialise(this);
            _onActorControlHook?.Enable();
            _onCastHook?.Enable();

            ActionEffect.ActionEffectEvent += ActionFromSelfAsync;
        }
        catch (Exception e)
        {
            PluginLog.Error("Error initiating hooks: " + e.Message);
        }
    }

    ~TimelineManager()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _items.Clear();

        ActionEffect.ActionEffectEvent -= ActionFromSelfAsync;

        _onActorControlHook?.Disable();
        _onActorControlHook?.Dispose();

        _onCastHook?.Disable();
        _onCastHook?.Dispose();
    }
    #endregion

    private delegate void OnActorControlDelegate(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, ulong targetId, byte unk6);
    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(OnActorControl))]
    private readonly Hook<OnActorControlDelegate>? _onActorControlHook = null;

    private delegate void OnCastDelegate(uint sourceId, IntPtr sourceCharacter);
    [Signature("40 55 56 48 81 EC ?? ?? ?? ?? 48 8B EA", DetourName = nameof(OnCast))]
    private readonly Hook<OnCastDelegate>? _onCastHook = null;


    public DateTime EndTime { get; private set; } = DateTime.Now;
    private static int kMaxItemCount = 128;
    private readonly Queue<TimelineItem> _items = new Queue<TimelineItem>(kMaxItemCount);
    private TimelineItem? _lastItem = null;
    private void AddItem(TimelineItem item)
    {
        if (item == null) return;
        if (_items.Count >= kMaxItemCount)
        {
            _items.Dequeue();
        }
        _items.Enqueue(item);
        _lastItem = item;
        UpdateEndTime(item.EndTime);
    }

    private void UpdateEndTime(DateTime endTime)
    {
        if(endTime > EndTime) EndTime = endTime;
    }

    public List<TimelineItem> GetItems(DateTime time, out DateTime lastEndTime)
    {
        return GetItems(_items, time, out lastEndTime);
    }

    private static int kMaxStatusCount = 16;
    private readonly Queue<StatusLineItem> _statusItems = new Queue<StatusLineItem>(kMaxStatusCount);
    private void AddItem(StatusLineItem item)
    {
        if (item == null) return;
        if (_statusItems.Count >= kMaxStatusCount)
        {
            _statusItems.Dequeue();
        }
        _statusItems.Enqueue(item);
    }

    public List<StatusLineItem> GetStatus(DateTime time, out DateTime lastEndTime)
    {
        return GetItems(_statusItems, time, out lastEndTime);
    }

    private List<T> GetItems<T>(IEnumerable<T> items, DateTime time, out DateTime lastEndTime) where T : ITimelineItem 
    {
        var result = new List<T>();
        lastEndTime = DateTime.Now;
        foreach (var item in items)
        {
            if (item == null) continue;
            if (item.EndTime > time)
            {
                result.Add(item);
            }
            else if(item is TimelineItem tItem && tItem.Type == TimelineItemType.GCD)
            {
                lastEndTime = item.EndTime;
            }
        }
        return result;
    }

    public unsafe float GCD
    {
        get
        {
            var cdGrp = ActionManager.Instance()->GetRecastGroupDetail(GCDCooldownGroup - 1);
            return cdGrp->Total - cdGrp->Elapsed;
        }
    }

    private static TimelineItemType GetActionType(uint actionId, ActionType type)
    {

        switch (type)
        {
            case ActionType.Spell:
                var action = Svc.Data.GetExcelSheet<Action>()?.GetRow(actionId);
                if (action == null) break;

                if (actionId == 3) return TimelineItemType.OGCD; //Sprint

                var actionCate = (ActionCate)(action.ActionCategory.Value?.RowId ?? 0);

                var isRealGcd = action.CooldownGroup == GCDCooldownGroup || action.AdditionalCooldownGroup == GCDCooldownGroup;
                return actionCate == ActionCate.AutoAttack
                    ? TimelineItemType.AutoAttack
                    : !isRealGcd && actionCate == ActionCate.Ability ? TimelineItemType.OGCD
                    : TimelineItemType.GCD;

            case ActionType.Item:
                return TimelineItemType.OGCD;
        }

        return TimelineItemType.GCD;
    }

    private void CancelCasting()
    {
        if (_lastItem == null || _lastItem.CastingTime == 0) return;

        _lastItem.State = TimelineItemState.Canceled;
        var maxTime = (float)(DateTime.Now - _lastItem.StartTime).TotalSeconds;
        _lastItem.GCDTime = 0;
        _lastItem.CastingTime = MathF.Min(maxTime, _lastItem.CastingTime);
    }

    private async void ActionFromSelfAsync(ActionEffectSet set)
    {
        if (!Player.Available) return;

#if DEBUG
        //Svc.Chat.Print($"Id: {set.Header.ActionID}; {set.Header.ActionType}");
#endif 
        if (set.Source.ObjectId != Player.Object.ObjectId) return;

        DamageType damage = DamageType.None;
        if(set.TargetEffects[0][0].type is ActionEffectType.Damage or ActionEffectType.Heal)
        {
            var flag = set.TargetEffects[0][0].param0;
            var hasDirect = (flag & 64) == 64;
            var hasCritical = (flag & 32) == 32;
            damage = hasCritical ? (hasDirect ? DamageType.CriticalDirect : DamageType.Critical)
                : hasDirect ? DamageType.Direct : DamageType.None;
        }

        List<uint> statusGain = new List<uint>(), statusLose = new List<uint>();
        var isTargetMe = set.TargetEffects[0].TargetID == Player.Object.ObjectId;
        set.TargetEffects[0].ForEach(x =>
        {
            var status = Svc.Data.GetExcelSheet<Status>()?.GetRow(x.value);
            if (status == null) return;

            var icon = status.Icon;

            switch (x.type)
            {
                case ActionEffectType.ApplyStatusEffectTarget:
                case ActionEffectType.ApplyStatusEffectSource when isTargetMe:
                case ActionEffectType.GpGain:
                    statusGain.Add(icon + (uint)Math.Max(0, status.MaxStacks - 1));
                    break;

                case ActionEffectType.LoseStatusEffectTarget:
                case ActionEffectType.LoseStatusEffectSource when isTargetMe:
                    var stack = Player.Object.StatusList.FirstOrDefault(s => s.StatusId == x.value)?.StackCount ?? 0;
                    stack++;
                    statusLose.Add(icon + (uint)Math.Max(0, stack - 1));
                    break;
            }
        });

        var type = GetActionType(set.Header.ActionID, set.Header.ActionType);

        if (_lastItem != null && _lastItem.CastingTime > 0 && type == TimelineItemType.GCD
            && _lastItem.State == TimelineItemState.Casting) // Finish the casting.
        {
            _lastItem.State = TimelineItemState.Finished;
            _lastItem.AnimationLockTime = set.Header.AnimationLockTime;
            _lastItem.Name = set.Name;
            _lastItem.Icon = set.IconId;
            _lastItem.Damage = damage;
            statusGain.ForEach(i => _lastItem.StatusGainIcon.Add(i));
            statusLose.ForEach(i => _lastItem.StatusLoseIcon.Add(i));
        }
        else
        {
            var item = new TimelineItem()
            {
                Name = set.Name,
                Icon = set.IconId,
                StartTime = DateTime.Now,
                AnimationLockTime = type == TimelineItemType.AutoAttack ? 0 : set.Header.AnimationLockTime,
                GCDTime = type == TimelineItemType.GCD ? GCD : 0,
                Type = type,
                State = TimelineItemState.Finished,
                Damage = damage,
            };

            statusGain.ForEach(i => item.StatusGainIcon.Add(i));
            statusLose.ForEach(i => item.StatusLoseIcon.Add(i));

            AddItem(item);
        }

        var effectItem = _lastItem;

        if( effectItem != null)
        {
            UpdateEndTime(effectItem.EndTime);
        }

        if (effectItem?.Type is TimelineItemType.AutoAttack) return;

        int statusDelay = 0;
        if(Plugin.Settings.StatusCheckDelay is > 0 and < 0.5f)
        {
            statusDelay = (int)(Plugin.Settings.StatusCheckDelay * 1000);
            var previousStatus = Player.Object.StatusList
                .Where(s => s.SourceId == Player.Object.ObjectId && (s.RemainingTime > Plugin.Settings.StatusCheckDelay || s.RemainingTime <= 0))
                .Select(s => (s.StatusId, s.StackCount))
                .ToArray();

            await Task.Delay(statusDelay);

            var nowStatus = Player.Object.StatusList
            .Where(s => s.SourceId == Player.Object.ObjectId)
            .Select(s => (s.StatusId, s.StackCount))
            .ToArray();

            foreach (var pre in previousStatus)
            {
                var status = Svc.Data.GetExcelSheet<Status>()?.GetRow(pre.StatusId);
                if (status == null) continue;
                var now = nowStatus.FirstOrDefault(i => i.StatusId == pre.StatusId);
                if (now.StatusId == 0 || now.StackCount < pre.StackCount)
                {
                    effectItem?.StatusLoseIcon.Add(status.Icon + (uint)Math.Max(0, pre.StackCount - 1));
                }
            }

            foreach (var now in nowStatus)
            {
                var status = Svc.Data.GetExcelSheet<Status>()?.GetRow(now.StatusId);
                if (status == null) continue;
                var pre = previousStatus.FirstOrDefault(i => i.StatusId == now.StatusId);
                if (pre.StatusId == 0)
                {
                    effectItem?.StatusGainIcon.Add(status.Icon + (uint)Math.Max(0, now.StackCount - 1));
                }
            }
        }

        if (effectItem != null)
        {
            List<StatusLineItem> list = new List<StatusLineItem>(4);
            foreach (var icon in effectItem.StatusGainIcon)
            {
                if (Plugin.IconStack.TryGetValue(icon, out var stack))
                {
                    var item = new StatusLineItem()
                    {
                        Icon = icon,
                        TimeDuration = 6,
                        Stack = stack,
                        StartTime = effectItem.StartTime,
                    };
                    list.Add(item);
                    AddItem(item);
                }
            }

            var statusList = Player.Object.StatusList.Where(s => s.SourceId == Player.Object.ObjectId);
            if ( Svc.Objects.SearchById(set.TargetEffects[0].TargetID) is BattleChara b)
            {
                statusList = statusList.Union(b.StatusList.Where(s => s.SourceId == Player.Object.ObjectId));
            }

            await Task.Delay(1000 - statusDelay);

            foreach (var status in statusList)
            {
                var icon = Svc.Data.GetExcelSheet<Status>()?.GetRow(status.StatusId)?.Icon;
                if (icon == null) continue;

                foreach (var item in list)
                {
                    if (item.Icon == icon)
                    {
                        item.TimeDuration = (float)(DateTime.Now - effectItem.StartTime).TotalSeconds + status.RemainingTime;
                    }
                }
            }
        }
    }

    private void OnActorControl(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg4, uint arg5, ulong targetId, byte a10)
    {
        _onActorControlHook?.Original(entityId, type, buffID, direct, actionId, sourceId, arg4, arg5, targetId, a10);

        if (type != 15) { return; }

        PlayerCharacter? player = Player.Object;
        if (player == null || entityId != player.ObjectId) { return; }

        CancelCasting();
    }

    private unsafe void OnCast(uint sourceId, IntPtr ptr)
    {
        _onCastHook?.Original(sourceId, ptr);

        PlayerCharacter? player = Player.Object;
        if (player == null || sourceId != player.ObjectId) { return; }

        var actionId = *(ushort*)ptr;

        var action = Svc.Data.GetExcelSheet<Action>()?.GetRow(actionId);

        var icon = actionId == 4 ? (ushort)118 //Mount
            : action?.Icon ?? 0;

        AddItem(new TimelineItem()
        {
            Name = action?.Name ?? string.Empty,
            Icon = icon,
            StartTime = DateTime.Now,
            GCDTime = GCD,
            CastingTime = Player.Object.TotalCastTime - Player.Object.CurrentCastTime,
            Type = TimelineItemType.GCD,
            State = TimelineItemState.Casting,
        });
    }
}
