using ActionTimeline.Timeline;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Timeline;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace ActionTimeline.Windows;

internal class TimelineWindow : Window
{
    public DrawingSettings Setting { get; set; } = new DrawingSettings();

    private ImGuiWindowFlags _baseFlags = ImGuiWindowFlags.NoScrollbar
                                        | ImGuiWindowFlags.NoCollapse
                                        | ImGuiWindowFlags.NoTitleBar
                                        | ImGuiWindowFlags.NoNav
                                        | ImGuiWindowFlags.NoScrollWithMouse;

    public TimelineWindow(string name) : base(name)
    {
        Flags = _baseFlags;

        Size = new Vector2(560, 100);
        SizeCondition = ImGuiCond.FirstUseEver;

        Position = new Vector2(200, 200);
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw()
    {
        Vector4 bgColor = Setting.Locked ? Setting.LockedBackgroundColor : Setting.UnlockedBackgroundColor;
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);

        Flags = _baseFlags;

        if (Setting.Locked)
        {
            Flags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
    }

    public override void Draw()
    {
        if (ImGui.IsWindowHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                Plugin.OpenConfigUi();
            }
        }
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();

        var now = Setting.IsRotation ? TimelineManager.Instance?.EndTime ?? DateTime.Now : DateTime.Now;

        var endTime = now - TimeSpan.FromSeconds(size.X / Setting.SizePerSecond - Setting.TimeOffset);

        var last = now;
        var list = TimelineManager.Instance?.GetItems(endTime, out last);

        DrawGrid(pos, size);

        if (Setting.ShowGCDClipping && list != null) //Clipping
        {
            var gcdClippingColor = ImGui.ColorConvertFloat4ToU32(Setting.GCDClippingColor);
            var threshold = TimeSpan.FromSeconds(Setting.GCDClippingThreshold);
            var max = TimeSpan.FromSeconds(Setting.GCDClippingMaxTime);
            var sizePerSecond = Setting.SizePerSecond;

            foreach (var item in list)
            {
                if (item.Type != TimelineItemType.GCD) continue;

                var start = item.StartTime;
                var span = start - last;

                if (last != DateTime.MinValue && span >= threshold && span < max)
                {
                    ImGui.GetWindowDrawList().AddRectFilled(
                        pos + new Vector2(size.X - (Setting.TimeOffset + (float)(now - last).TotalSeconds) * sizePerSecond, 0),
                        pos + new Vector2(size.X - (Setting.TimeOffset + (float)(now - start).TotalSeconds) * sizePerSecond, size.Y), gcdClippingColor);
                }

                last = item.EndTime;
            }
        }

        if (list != null)
        {
            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.General, Setting);
            }
            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.Status, Setting);
            }

            var status = TimelineManager.Instance?.GetStatus(endTime, out _);
            if (status != null && Setting.ShowStatusLine)
            {
                foreach (var item in status)
                {
                    item.Draw(now, pos, size, Setting);
                }
            }

            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.Icon, Setting);
            }
        }

        uint lineColor = ImGui.ColorConvertFloat4ToU32(Setting.GridStartLineColor);

        var x = pos.X + size.X - Setting.TimeOffset * Setting.SizePerSecond;
        ImGui.GetWindowDrawList().AddLine(new Vector2(x, pos.Y), new Vector2(x, pos.Y + size.Y), lineColor, Setting.GridStartLineWidth);
    }

    private void DrawGrid(Vector2 pos, Vector2 size)
    {
        if (!Setting.ShowGrid) return;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        float width = size.X;
        float height = size.Y;

        uint lineColor = ImGui.ColorConvertFloat4ToU32(Setting.GridLineColor);
        uint subdivisionLineColor = ImGui.ColorConvertFloat4ToU32(Setting.GridSubdivisionLineColor);

        if (Setting.ShowGridCenterLine)
        {
            drawList.AddLine(new Vector2(pos.X, pos.Y + height / 2f), new Vector2(pos.X + width, pos.Y + height / 2f), lineColor, Setting.GridLineWidth);
        }

        if (!Setting.GridDivideBySeconds) return;

        float step = Setting.SizePerSecond;

        for (int i = 0; i < width / step; i++)
        {
            float x = step * i;
            var start = pos.X + width - x;

            if (Setting.GridSubdivideSeconds && Setting.GridSubdivisionCount > 1)
            {
                float subStep = step * 1f / Setting.GridSubdivisionCount;
                for (int j = 1; j < Setting.GridSubdivisionCount; j++)
                {
                    drawList.AddLine(new Vector2(start + subStep * j, pos.Y), new Vector2(start + subStep * j, pos.Y + height), subdivisionLineColor, Setting.GridSubdivisionLineWidth);
                }
            }
            var time = -i + Setting.TimeOffset;

            if (time != 0)
            {
                drawList.AddLine(new Vector2(start, pos.Y), new Vector2(start, pos.Y + height), lineColor, Setting.GridLineWidth);
            }

            if (Setting.GridShowSecondsText)
            {
                drawList.AddText(new Vector2(start + 2, pos.Y), lineColor, $" {time}s");
            }
        }
        return;
    }
}
