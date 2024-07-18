using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Data.Files;
using System.Numerics;
using XIVConfigUI;

namespace ActionTimelineEx.Helpers;

internal enum IconType : byte
{
    Normal,
    Blacked,
    Highlight,
}

internal static class DrawHelper
{
    public static void DrawActionIcon(this ImDrawListPtr drawList, uint iconId, bool isHq, Vector2 position, float size, IconType type = IconType.Normal)
    {
        IDalamudTextureWrap? texture = GetTextureFromIconId(iconId, isHq);
        if (texture == null) return;

        var pixPerUnit = size / 82;

        drawList.AddImage(texture.ImGuiHandle, position, position + new Vector2(size));

        if (ImageLoader.GetTexture("ui/uld/icona_frame_hr1.tex", out var frameText))
        {
            var coverPos = position - new Vector2(pixPerUnit * 3, pixPerUnit * 4);

            var start = new Vector2(4f / frameText.Width, 0f / frameText.Height);

            switch (type)
            {
                case IconType.Highlight:
                    start += new Vector2(96f / frameText.Width * 2, 96f / frameText.Height * 0);
                    break;

                case IconType.Blacked:
                    start += new Vector2(96f / frameText.Width * 1, 96f / frameText.Height * 0);
                    break;
            }

            drawList.AddImage(frameText.ImGuiHandle, coverPos, coverPos + new Vector2(pixPerUnit * 88, pixPerUnit * 96),
               start, start + new Vector2(88f / frameText.Width, 94f / frameText.Height));
        }
    }

    public static IDalamudTextureWrap? GetTextureFromIconId(uint iconId, bool highQuality = true)
        => ImageLoader.GetTexture(new GameIconLookup(iconId, false, highQuality), out var texture) ? texture
        : ImageLoader.GetTexture(new GameIconLookup(0, false, highQuality), out texture) ? texture : null;

    private static readonly Dictionary<uint, uint> textureColorCache = [];
    private static readonly Queue<uint> calculating = new();
    public static uint GetTextureAverageColor(uint iconId)
    {
        if (textureColorCache.TryGetValue(iconId, out var color)) return color;

        if (!calculating.Contains(iconId)) calculating.Enqueue(iconId);

        CalculateColor();
        return uint.MaxValue;
    }

    private static bool _run;
    private static void CalculateColor()
    {
        if (_run) return;
        _run = true;

        Task.Run(() =>
        {
            while (calculating.TryDequeue(out var icon))
            {
                var tex = Svc.Data.GetFile<TexFile>($"ui/icon/{icon / 1000:D3}000/{icon:D6}.tex");
                if (tex == null)
                {
                    textureColorCache[icon] = uint.MaxValue;
                    continue;
                }

                byte[] imageData = tex.ImageData;
                float whole = 0, r = 0, g = 0, b = 0;
                for (int i = 0; i < imageData.Length; i += 4)
                {
                    var alpha = imageData[i + 3] / (float)byte.MaxValue;
                    b += imageData[i] / (float)byte.MaxValue * alpha;
                    g += imageData[i + 1] / (float)byte.MaxValue * alpha;
                    r += imageData[i + 2] / (float)byte.MaxValue * alpha;

                    whole += alpha;
                }

                textureColorCache[icon] = ImGui.ColorConvertFloat4ToU32(new Vector4(r / whole, g / whole, b / whole, 1));
            }
            _run = false;
        });
    }

    public static bool IsInRect(Vector2 leftTop, Vector2 size)
    {
        var pos = ImGui.GetMousePos() - leftTop;
        if (pos.X < 0 || pos.Y < 0 || pos.X > size.X || pos.Y > size.Y) return false;
        return true;
    }
}
