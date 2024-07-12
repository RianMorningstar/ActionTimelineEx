using ActionTimeline;
using ActionTimelineEx.Configurations;
using ActionTimelineEx.Helpers;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.ConditionConfigs;
using XIVDrawer;

namespace ActionTimelineEx.Windows;

[Description("Rotation Helper")]
internal class RotationHelperItem() : ConfigWindowItem
{
    internal static uint _territoryId { get; set; } = 0;

    private static CollapsingHeaderGroup? _group;

    private static TerritoryType[]? _territories;

    public override bool GetIcon(out IDalamudTextureWrap texture)
    {
        return ImageLoader.GetTexture(25, out texture);
    }

    public override void Draw(ConfigWindow window)
    {
        var setting = Plugin.Settings.GetSetting(_territoryId);
        Plugin.Settings.EditSetting = setting;

        DrawTerritoryHeader();

        _group ??= new CollapsingHeaderGroup(new()
        {
            { () => UiString.RotationSetting.Local(), () =>  DrawSetting(window) },
            { () => UiString.Rotation.Local(), () => DrawRotation(window, setting.RotationSetting)},
        });

        _group.Draw();
        base.Draw(window);
    }

    private static void DrawSetting(ConfigWindow window)
    {
        window.Collection.DrawItems(1);
    }

    private static void DrawRotation(ConfigWindow window, RotationSetting setting)
    {
        if (ImGui.Button(UiString.RotationReset.Local()))
        {
            RotationHelper.Clear();
        }

        ImGui.SameLine();

        if (ImGui.Button(LocalString.CopyToClipboard.Local()))
        {
            var str = JsonHelper.SerializeObject(setting.Actions);
            ImGui.SetClipboardText(str);
        }

        ImGui.SameLine();

        if (ImGui.Button(LocalString.FromClipboard.Local()))
        {
            var str = ImGui.GetClipboardText();

            try
            {
                setting.Actions = JsonHelper.DeserializeObject<List<ActionSetting>>(str)!;
            }
            catch (Exception ex)
            {
                Svc.Log.Warning(ex, "Failed to load the timeline.");
            }
        }

        window.Collection.DrawItems(2);
        ConditionDrawer.Draw(setting.Actions);
    }

    private static void DrawTerritoryHeader()
    {
        _territories ??= Svc.Data.GetExcelSheet<TerritoryType>()?
            .Where(RotationHelper.IsTerritoryTypeValid)
            .Reverse().ToArray();

        var rightTerritory = Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(_territoryId);
        var name = GetName(rightTerritory);

        var imFont = DrawingExtensions.GetFont(21);
        float width = 0, height = 0;
        using (var font = ImRaii.PushFont(imFont))
        {
            width = ImGui.CalcTextSize(name).X + (ImGui.GetStyle().ItemSpacing.X * 2);
            height = ImGui.CalcTextSize(name).Y + (ImGui.GetStyle().ItemSpacing.Y * 2);
        }

        var HasJob = ImageLoader.GetTexture(62100 + Player.Object.ClassJob.Id, out var jobTexture);

        if (HasJob)
        {
            width += height + ImGui.GetStyle().ItemSpacing.X;
        }

        ImGuiHelper.DrawItemMiddle(() =>
        {
            if (HasJob)
            {
                ImGui.Image(jobTexture.ImGuiHandle, Vector2.One * height);
                ImGui.SameLine();
            }

            var territories = _territories ?? [];
            var index = Array.IndexOf(territories, rightTerritory);
            if (ImGuiHelper.SelectableCombo("##Choice the specific dungeon", [.. territories.Select(GetName)], ref index, imFont, ImGuiColors.DalamudYellow))
            {
                _territoryId = territories[index]?.RowId ?? 0;
            }
        }, ImGui.GetWindowWidth(), width);

        DrawContentFinder(rightTerritory?.ContentFinderCondition?.Value);

        static string GetName(TerritoryType? territory)
        {
            var str = territory?.ContentFinderCondition?.Value?.Name?.RawString;
            if (string.IsNullOrEmpty(str)) str = territory?.PlaceName?.Value?.Name?.RawString;
            if (string.IsNullOrEmpty(str)) return "Unnamed Territory";
            return str;
        }
    }

    private static void DrawContentFinder(ContentFinderCondition? content)
    {
        var badge = content?.Image;
        if (badge != null && badge.Value != 0
            && ImageLoader.GetTexture(badge.Value, out var badgeTexture))
        {
            var wholeWidth = ImGui.GetWindowWidth();
            var size = new Vector2(badgeTexture.Width, badgeTexture.Height) * MathF.Min(1, MathF.Min(480, wholeWidth) / badgeTexture.Width);

            ImGuiHelper.DrawItemMiddle(() =>
            {
                ImGui.Image(badgeTexture.ImGuiHandle, size);
            }, wholeWidth, size.X);
        }
    }
}
