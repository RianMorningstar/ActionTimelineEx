using ActionTimelineEx.Configurations;
using ActionTimelineEx.Helpers;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.ComponentModel;
using System.Numerics;
using XIVConfigUI;
using XIVConfigUI.ConditionConfigs;

namespace ActionTimelineEx.Windows;

[Description("Rotation Helper")]
internal class RotationHelperItem() : ConfigWindowItem
{
    private static CollapsingHeaderGroup? _group;

    public override bool GetIcon(out IDalamudTextureWrap texture)
    {
        return ImageLoader.GetTexture(25, out texture);
    }

    public override void Draw(ConfigWindow window)
    {
        if (!Player.Available) return;

        var setting = Plugin.Settings.RotationHelper;

        if (ImageLoader.GetTexture(62100 + Player.Object.ClassJob.Id, out var jobTexture))
        {
            ImGuiHelper.DrawItemMiddle(() =>
            {
                ImGui.Image(jobTexture.ImGuiHandle, Vector2.One * 50);

            }, ImGui.GetWindowWidth(), 50);
        }

        if (ImGui.Button(UiString.AddOneRotation.Local()))
        {
            setting.RotationSettings.Add(new()
            {
                Name = setting.RotationSettings.Count.ToString(),
            });
        }

        ImGui.SameLine();

        if (TimelineItem.RemoveValue(setting.RotationSetting.Name))
        {
            setting.RotationSettings.Remove(setting.RotationSetting);
        }

        ImGui.SameLine();

        window.Collection.DrawItems(3);

        _group ??= new CollapsingHeaderGroup(new()
        {
            { () => UiString.RotationSetting.Local(), () =>  DrawSetting(window) },
            { () => UiString.Rotation.Local(), () => DrawRotation(window)},
        });

        _group.Draw();
        base.Draw(window);
    }

    private static void DrawSetting(ConfigWindow window)
    {
        window.Collection.DrawItems(1);
    }

    private static void DrawRotation(ConfigWindow window)
    {
        var setting = Plugin.Settings.RotationHelper;

        if (ImGui.Button(UiString.RotationReset.Local()))
        {
            RotationHelper.Clear();
        }

        ImGui.SameLine();

        if (ImGui.Button(LocalString.CopyToClipboard.Local()))
        {
            var str = JsonHelper.SerializeObject(setting.RotationSetting.Actions);
            ImGui.SetClipboardText(str);
        }

        ImGui.SameLine();

        if (ImGui.Button(LocalString.FromClipboard.Local()))
        {
            var str = ImGui.GetClipboardText();

            try
            {
                setting.RotationSetting.Actions = JsonHelper.DeserializeObject<List<ActionSetting>>(str)!;
            }
            catch (Exception ex)
            {
                Svc.Log.Warning(ex, "Failed to load the timeline.");
            }
        }


        window.Collection.DrawItems(2);

        ImGui.Separator();

        ConditionDrawer.Draw(setting.RotationSetting);
    }
}
