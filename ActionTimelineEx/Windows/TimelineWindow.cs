using ActionTimeline.Timeline;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Timeline;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;

namespace ActionTimeline.Windows;

internal static class TimelineWindow
{
    private const ImGuiWindowFlags _baseFlags = ImGuiWindowFlags.NoScrollbar
                                        | ImGuiWindowFlags.NoCollapse
                                        | ImGuiWindowFlags.NoTitleBar
                                        | ImGuiWindowFlags.NoNav
                                        | ImGuiWindowFlags.NoScrollWithMouse;

    public static void Draw(DrawingSettings setting)
    {
        if (!setting.Enable || string.IsNullOrEmpty(setting.Name)) return;

        var flag = _baseFlags;
        if (setting.Locked)
        {
            flag |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs;
        }

        Vector4 bgColor = setting.Locked ? setting.LockedBackgroundColor : setting.UnlockedBackgroundColor;
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);

        ImGui.SetNextWindowSize(new Vector2(560, 100) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(200, 200) * ImGuiHelpers.GlobalScale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($"Timeline: {setting.Name}", flag))
        {
            DrawContent(setting);
            ImGui.End();
        }

        ImGui.PopStyleColor();
    }

    private static void DrawContent(DrawingSettings setting)
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

        var now = setting.IsRotation ? (TimelineManager.Instance?.EndTime ?? DateTime.Now - TimeSpan.FromSeconds(setting.TimeOffsetSetting)) : DateTime.Now;

        var endTime = now - TimeSpan.FromSeconds(size.X / setting.SizePerSecond - setting.TimeOffset);

        var last = now;
        var list = TimelineManager.Instance?.GetItems(endTime, out last);

        DrawGrid(pos, size, setting);

        if (setting.ShowGCDClipping && list != null) //Clipping
        {
            var gcdClippingColor = ImGui.ColorConvertFloat4ToU32(setting.GCDClippingColor);
            var threshold = TimeSpan.FromSeconds(setting.GCDClippingThreshold);
            var max = TimeSpan.FromSeconds(setting.GCDClippingMaxTime);
            var sizePerSecond = setting.SizePerSecond;

            foreach (var item in list)
            {
                if (item.Type != TimelineItemType.GCD) continue;

                var start = item.StartTime;
                var span = start - last;

                if (last != DateTime.MinValue && span >= threshold && span < max)
                {
                    var drawingLeftTop = pos + new Vector2(size.X - (setting.TimeOffset + (float)(now - last).TotalSeconds) * sizePerSecond, 0);
                    ImGui.GetWindowDrawList().AddRectFilled( drawingLeftTop,
                        pos + new Vector2(size.X - (setting.TimeOffset + (float)(now - start).TotalSeconds) * sizePerSecond, size.Y), gcdClippingColor);
                    ImGui.GetWindowDrawList().AddText(drawingLeftTop, 
                        ImGui.ColorConvertFloat4ToU32(setting.GCDClippingTextColor),
                        $"{(int)span.TotalMilliseconds}ms");
                }

                last = item.EndTime;
            }
        }

        if (list != null)
        {
            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.General, setting);
            }
            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.Status, setting);
            }

            var status = TimelineManager.Instance?.GetStatus(endTime, out _);
            if (status != null && setting.ShowStatusLine)
            {
                foreach (var item in status)
                {
                    item.Draw(now, pos, size, setting);
                }
            }

            foreach (var item in list)
            {
                item.Draw(now, pos, size, TimelineLayer.Icon, setting);
            }
        }

        uint lineColor = ImGui.ColorConvertFloat4ToU32(setting.GridStartLineColor);

        var x = pos.X + size.X - setting.TimeOffset * setting.SizePerSecond;
        ImGui.GetWindowDrawList().AddLine(new Vector2(x, pos.Y), new Vector2(x, pos.Y + size.Y), lineColor, setting.GridStartLineWidth);

        if (!setting.Locked) ImGui.Text(setting.Name);
    }

    private static void DrawGrid(Vector2 pos, Vector2 size, DrawingSettings setting)
    {
        if (!setting.ShowGrid) return;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        float width = size.X;
        float height = size.Y;

        uint lineColor = ImGui.ColorConvertFloat4ToU32(setting.GridLineColor);
        uint subdivisionLineColor = ImGui.ColorConvertFloat4ToU32(setting.GridSubdivisionLineColor);

        if (setting.GridDivideBySeconds)
        {
            float step = setting.SizePerSecond;

            for (int i = 0; i < width / step; i++)
            {
                float x = step * i;
                var start = pos.X + width - x;

                if (setting.GridSubdivideSeconds && setting.GridSubdivisionCount > 1)
                {
                    float subStep = step * 1f / setting.GridSubdivisionCount;
                    for (int j = 1; j < setting.GridSubdivisionCount; j++)
                    {
                        drawList.AddLine(new Vector2(start + subStep * j, pos.Y), new Vector2(start + subStep * j, pos.Y + height), subdivisionLineColor, setting.GridSubdivisionLineWidth);
                    }
                }
                var time = -i + setting.TimeOffset;

                if (time != 0)
                {
                    drawList.AddLine(new Vector2(start, pos.Y), new Vector2(start, pos.Y + height), lineColor, setting.GridLineWidth);
                }

                if (setting.GridShowSecondsText)
                {
                    drawList.AddText(new Vector2(start + 2, pos.Y), lineColor, $" {time}s");
                }
            }
        }

        lineColor = ImGui.ColorConvertFloat4ToU32(setting.GridCenterLineColor);
        if (setting.ShowGridCenterLine)
        {
            drawList.AddLine(new Vector2(pos.X, pos.Y + height / 2f + setting.CenterOffset), new Vector2(pos.X + width, pos.Y + height / 2f + setting.CenterOffset), lineColor, setting.GridCenterLineWidth);
        }
    }
}
