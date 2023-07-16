using ActionTimeline;
using ActionTimeline.Helpers;
using ActionTimelineEx.Configurations;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using ImGuiScene;
using System.Numerics;

namespace ActionTimelineEx.Timeline;

public class TimelineItem : ITimelineItem
{
    public string? Name { get; set; }
    public ushort Icon { get; set; }

    public DateTime StartTime { get; init; }

    public float AnimationLockTime { get; set; }

    public float CastingTime { get; set; }

    public float GCDTime { get; set; }

    public TimelineItemType Type { get; set; }

    public TimelineItemState State { get; set; }

    public DamageType Damage { get; set; } = DamageType.None;

    public float TimeDuration => MathF.Max(GCDTime, CastingTime + AnimationLockTime);

    public DateTime EndTime => StartTime + TimeSpan.FromSeconds(TimeDuration);

    public HashSet<uint> StatusGainIcon { get; } = new(4);
    public HashSet<uint> StatusLoseIcon { get;  } = new(4);

    public void Draw(DateTime time, Vector2 windowPos, Vector2 windowSize, TimelineLayer icon, DrawingSettings setting)
    {
        var sizePerSecond = setting.SizePerSecond;
        var rightCenter = windowPos + new Vector2(windowSize.X, windowSize.Y/ 2 + setting.CenterOffset);
        rightCenter -= Vector2.UnitX * setting.TimeOffset * sizePerSecond; 
        DrawItemWithCenter(rightCenter - Vector2.UnitX * (float)(time - StartTime).TotalSeconds * sizePerSecond, icon, setting);
    }

    public void DrawItemWithCenter(Vector2 centerPos, TimelineLayer icon, DrawingSettings setting)
    {
        var GcdSize = setting.GCDIconSize;
        var drawList = ImGui.GetWindowDrawList();
        var xUnitPerSecond = Vector2.UnitX * setting.SizePerSecond;

        switch (Type)
        {
            case TimelineItemType.GCD:
                DrawItemWithCenter(drawList, centerPos, xUnitPerSecond, GcdSize, icon, setting);
                break;

            case TimelineItemType.OGCD when setting.ShowOGCD:
                var oGcdOffset = setting.OGCDOffset;
                var oGcdSize = setting.OGCDIconSize;
                var oGcdCenter = new Vector2(centerPos.X, centerPos.Y - oGcdOffset * GcdSize - oGcdSize / 2);
                DrawItemWithCenter(drawList, oGcdCenter, xUnitPerSecond, oGcdSize, icon, setting);
                break;

            case TimelineItemType.AutoAttack when setting.ShowAutoAttack:
                var autoAttackOffset = setting.AutoAttackOffset;
                var autoAttackSize = setting.AutoAttackIconSize;
                var autoAttackCenter = new Vector2(centerPos.X, centerPos.Y + autoAttackOffset * GcdSize
                    + (autoAttackSize + GcdSize) / 2);
                DrawItemWithCenter(drawList, autoAttackCenter, xUnitPerSecond, autoAttackSize, icon, setting);
                break;
        }
    }

    private static TextureWrap[] GetTextures(HashSet<uint> iconIds)
    {
        var result = new List<TextureWrap>(iconIds.Count);
        foreach (var item in iconIds)
        {
            TextureWrap? texture = DrawHelper.GetTextureFromIconId(item);
            if (texture == null) continue;
            result.Add(texture);
        }
        return result.ToArray();
    }

    public const float HeightRatio = 4 / 3f;
    private void DrawItemWithCenter(ImDrawListPtr drawList, Vector2 centerPos, Vector2 xUnitPerSecond, float iconSize, TimelineLayer icon, DrawingSettings setting)
    {
        switch (icon)
        {
            case TimelineLayer.Icon:
                drawList.DrawActionIcon(Icon, new Vector2(centerPos.X, centerPos.Y - iconSize / 2), iconSize);
                return;

            case TimelineLayer.Status when setting.ShowStatus:
                var statusSize = setting.StatusIconSize;

                var center = new Vector2(centerPos.X + iconSize / 2, centerPos.Y - iconSize / 2 - statusSize * HeightRatio);
                var gains = GetTextures(StatusGainIcon);
                var lose = GetTextures(StatusLoseIcon);
                var color = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, setting.StatusIconAlpha));
                var gainColor = ImGui.ColorConvertFloat4ToU32(setting.StatusGainColor);
                var loseColor = ImGui.ColorConvertFloat4ToU32(setting.StatusLoseColor);

                center -= Vector2.UnitX * statusSize * (gains.Length + lose.Length) / 2;
                for (int i = 0; i < gains.Length; i++)
                {
                    drawList.AddImage(gains[i].ImGuiHandle, center,
                        center + new Vector2(statusSize, statusSize * HeightRatio), Vector2.Zero, Vector2.One, color);

                    drawList.AddText(UiBuilder.IconFont, statusSize / 2f, center, gainColor, FontAwesomeIcon.Plus.ToIconString());

                    center += Vector2.UnitX * statusSize;
                }
                for (int i = 0; i < lose.Length; i++)
                {
                    drawList.AddImage(lose[i].ImGuiHandle, center,
                        center + new Vector2(statusSize, statusSize * HeightRatio), Vector2.Zero, Vector2.One, color);

                    drawList.AddText(UiBuilder.IconFont, statusSize / 2f, center, loseColor, FontAwesomeIcon.Ban.ToIconString());

                    center += Vector2.UnitX * statusSize;
                }

                return;

            case TimelineLayer.General:
                //Get Info.
                float highPos = 0.5f;
                float lowPos = 0.8f;
                float rounding = 2f;

                var leftTop = new Vector2(centerPos.X, centerPos.Y - iconSize / 2 + highPos * iconSize);
                var leftBottom = new Vector2(centerPos.X, centerPos.Y - iconSize / 2 + lowPos * iconSize);
                var flag = ImDrawFlags.RoundCornersAll;

                var minX = centerPos.X + iconSize / 2;

                //Background
                var GcdBackColor = ImGui.ColorConvertFloat4ToU32(setting.BackgroundColor);
                drawList.AddRectFilled(MinX(leftTop, minX), MinX(leftBottom + xUnitPerSecond * MathF.Max(GCDTime, setting.ShowAnimationLock ? CastingTime + AnimationLockTime : CastingTime), minX), GcdBackColor, rounding, flag);

                var castOffset = xUnitPerSecond * CastingTime;

                //AnimationLock
                if (setting.ShowAnimationLock)
                {
                    var animationLockColor = ImGui.ColorConvertFloat4ToU32(setting.AnimationLockColor);
                    drawList.AddRectFilled(MinX(leftTop, minX),
                        MinX(leftBottom + castOffset + xUnitPerSecond * AnimationLockTime, minX),
                        animationLockColor, rounding, flag);
                }

                //Casting
                var castColor = State switch
                {
                    TimelineItemState.Canceled => ImGui.ColorConvertFloat4ToU32(setting.CastCanceledColor),
                    TimelineItemState.Casting => ImGui.ColorConvertFloat4ToU32(setting.CastInProgressColor),
                    _ => ImGui.ColorConvertFloat4ToU32(setting.CastFinishedColor)
                };
                drawList.AddRectFilled(MinX(leftTop, minX), MinX(leftBottom + castOffset, minX), castColor, rounding, flag);

                //GCD Fore
                var GcdForeColor = ImGui.ColorConvertFloat4ToU32(setting.GCDBorderColor);
                drawList.AddRect(MinX(leftTop, minX),
                     MinX(leftBottom + xUnitPerSecond * GCDTime, minX),
                    GcdForeColor, rounding, flag, setting.GCDThickness);

                //Damage
                if (setting.ShowDamageType)
                {
                    var lightCol = Damage switch
                    {
                        DamageType.Critical => ImGui.ColorConvertFloat4ToU32(setting.CriticalColor),
                        DamageType.Direct => ImGui.ColorConvertFloat4ToU32(setting.DirectColor),
                        DamageType.CriticalDirect => ImGui.ColorConvertFloat4ToU32(setting.CriticalDirectColor),
                        _ => 0u,
                    };
                    drawList.DrawDamage(new Vector2(centerPos.X, centerPos.Y - iconSize / 2), iconSize, lightCol);
                }

                return;
        }
        //Name
    }

    private Vector2 MinX(Vector2 pos, float minPos)
    {
        return new Vector2(MathF.Max(pos.X, minPos), pos.Y);
    }
}
