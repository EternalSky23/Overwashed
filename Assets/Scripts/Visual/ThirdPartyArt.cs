using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optional third-party pixel art (NOT in git: the licenses forbid redistributing the files).
/// Expected under Assets/ThirdParty/Resources/Art/ (git-ignored):
///   Art/Townsfolk/sheets/*.png   - "Pixel Townsfolk" by Anemon Creative (32x32 frames, 6 cols x 4 rows)
///   Art/Icons/PNG/32x32/...      - "Pixel Bathroom & Laundry Icons" by reodesuxz
/// When a file is missing the generated placeholder art (PixelArt) is used instead.
/// Turn it all off with GameBootstrap > Use Third Party Art.
/// </summary>
public static class ThirdPartyArt
{
    public static bool Enabled = true;

    private const string Root = "Art/";

    // PixelArt key -> (icon path, size in world units)
    private static readonly Dictionary<string, (string path, float size)> IconMap = new Dictionary<string, (string, float)>
    {
        { "washer", ("Icons/PNG/32x32/laundry/washing_machine", 1.7f) },
        { "dryer", ("Icons/PNG/32x32/laundry/dryer", 1.7f) },
        { "iron", ("Icons/PNG/32x32/laundry/ironing_board", 1.75f) },
        { "trash", ("Icons/PNG/32x32/bath/trash_bin", 1.15f) },
    };

    private static readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite[,]> characterCache = new Dictionary<string, Sprite[,]>();

    private static Texture2D LoadTexture(string path)
    {
        var tex = Resources.Load<Texture2D>(Root + path);
        if (tex != null)
        {
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
        }
        return tex;
    }

    /// <summary>Icon replacing a PixelArt key, or null.</summary>
    public static Sprite TryGetIcon(string key)
    {
        if (!Enabled || !IconMap.TryGetValue(key, out var entry)) return null;
        if (iconCache.TryGetValue(key, out var cached)) return cached;

        Sprite sprite = null;
        var tex = LoadTexture(entry.path);
        if (tex != null)
        {
            float ppu = tex.width / entry.size;
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        }
        iconCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Walk/idle frames of a Townsfolk character: [direction, frame].
    /// Direction 0 = down, 1 = left, 2 = right, 3 = up. Frames 0-1 idle, 2-5 walk. Null when missing.
    /// </summary>
    public static Sprite[,] TryGetCharacter(string name, float pixelsPerUnit)
    {
        if (!Enabled || string.IsNullOrEmpty(name)) return null;
        string key = name + "@" + pixelsPerUnit;
        if (characterCache.TryGetValue(key, out var cached)) return cached;

        Sprite[,] frames = null;
        var tex = LoadTexture("Townsfolk/sheets/" + name);
        if (tex != null && tex.width >= 192 && tex.height >= 128)
        {
            frames = new Sprite[4, 6];
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 6; col++)
                {
                    // rows are listed top to bottom, texture coordinates start at the bottom
                    var rect = new Rect(col * 32, (3 - row) * 32, 32, 32);
                    frames[row, col] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
                }
        }
        characterCache[key] = frames;
        return frames;
    }
}
