using ActionTimeline;
using ActionTimeline.Helpers;
using ActionTimelineEx.Configurations;
using ImGuiNET;
using ImGuiScene;
using System.Numerics;

namespace ActionTimelineEx.Timeline;

public class StatusLineItem : ITimelineItem
{
    public uint Icon { get; set; }
    public float TimeDuration { get; set; }
    public DateTime StartTime { get; init; }

    public DateTime EndTime => StartTime + TimeSpan.FromSeconds(TimeDuration);

    public byte Stack { get; set; }

    public void Draw(DateTime time, Vector2 windowPos, Vector2 windowSize, DrawingSettings setting)
    {
        var sizePerSecond = setting.SizePerSecond;
        var rightCenter = windowPos + new Vector2(windowSize.X, windowSize.Y / 2 + setting.CenterOffset);
        rightCenter -= Vector2.UnitX * setting.TimeOffset * sizePerSecond;
        DrawItemWithCenter(rightCenter - Vector2.UnitX * (float)(time - StartTime).TotalSeconds * sizePerSecond, 
            windowPos, setting);
    }

    public void DrawItemWithCenter(Vector2 centerPos, Vector2 windowPos, DrawingSettings setting)
    {
        var GcdSize = setting.GCDIconSize;
        var drawList = ImGui.GetWindowDrawList();
        var xUnitPerSecond = Vector2.UnitX * setting.SizePerSecond;

        var statusHeight = setting.StatusLineSize;
        var flag = ImDrawFlags.RoundCornersAll;
        var rounding = 2;

        TextureWrap? texture = DrawHelper.GetTextureFromIconId(Icon);
        if (texture == null) return;

        var col = DrawHelper.GetTextureAverageColor(Icon);

        var leftTop = centerPos + new Vector2(0, statusHeight * Stack + GcdSize / 2);
        if(setting.ShowAutoAttack)
        {
            var autoAttackOffset = setting.AutoAttackOffset;
            var autoAttackSize = setting.AutoAttackIconSize;
            leftTop += Vector2.UnitY *( autoAttackOffset * GcdSize + autoAttackSize);
        }
        var shrink = new Vector2(0, statusHeight * 0.3f);
        var rightBottom = leftTop + xUnitPerSecond * TimeDuration + Vector2.UnitY * statusHeight - shrink;
        drawList.AddRectFilled(leftTop + shrink, rightBottom,col, rounding, flag);

        if (rightBottom.X <= windowPos.X) return;

        leftTop.X = Math.Max(leftTop.X, windowPos.X);

        drawList.AddImage(texture.ImGuiHandle, leftTop,
            leftTop + new Vector2(statusHeight / TimelineItem.HeightRatio, statusHeight), Vector2.Zero, Vector2.One);
    }
}
