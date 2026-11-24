using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates placeholder pixel-art sprites in code so the prototype looks like a game without any art assets.
/// Every sprite can be replaced by assigning a real sprite on the ArtLibrary component.
/// </summary>
public static class PixelArt
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static string ClothKey(ClothType type)
    {
        switch (type)
        {
            case ClothType.Shirt: return "cloth_shirt";
            case ClothType.Sweater: return "cloth_sweater";
            case ClothType.Suit: return "cloth_suit";
            default: return "cloth_tshirt";
        }
    }

    public static Sprite Get(string key)
    {
        if (ArtLibrary.Instance != null)
        {
            var custom = ArtLibrary.Instance.Find(key);
            if (custom != null) return custom;
        }
        if (cache.TryGetValue(key, out var s) && s != null) return s;
        s = Build(key);
        cache[key] = s;
        return s;
    }

    // ---------------- Painter ----------------

    public class Painter
    {
        public readonly int W, H;
        public readonly Color32[] Px;

        public Painter(int w, int h)
        {
            W = w;
            H = h;
            Px = new Color32[w * h];
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            Px[y * W + x] = c;
        }

        public void Rect(int x, int y, int w, int h, Color32 c)
        {
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                    Set(i, j, c);
        }

        public void Box(int x, int y, int w, int h, Color32 fill, Color32 outline)
        {
            Rect(x, y, w, h, outline);
            Rect(x + 1, y + 1, w - 2, h - 2, fill);
        }

        public void Circle(float cx, float cy, float r, Color32 c)
        {
            for (int j = 0; j < H; j++)
                for (int i = 0; i < W; i++)
                {
                    float dx = i + 0.5f - cx, dy = j + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) Set(i, j, c);
                }
        }

        public void HLine(int x0, int x1, int y, Color32 c)
        {
            for (int i = x0; i <= x1; i++) Set(i, y, c);
        }

        public void VLine(int x, int y0, int y1, Color32 c)
        {
            for (int j = y0; j <= y1; j++) Set(x, j, c);
        }
    }

    private static Color32 C(int r, int g, int b, int a = 255) => new Color32((byte)r, (byte)g, (byte)b, (byte)a);

    private static readonly Color32 Outline = C(40, 34, 48);
    private static readonly Color32 Clear = C(0, 0, 0, 0);

    public static Sprite Make(int w, int h, float ppu, Action<Painter> draw)
    {
        var p = new Painter(w, h);
        draw(p);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(p.Px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
    }

    // ---------------- Sprites ----------------

    private static Sprite Build(string key)
    {
        switch (key)
        {
            case "white": return Make(4, 4, 4, p => p.Rect(0, 0, 4, 4, C(255, 255, 255)));
            case "player": return Player(C(60, 140, 200), C(90, 60, 40));
            case "kid": return Kid();
            case "washer": return Machine(C(232, 234, 240), C(150, 160, 175), C(90, 150, 215), C(170, 210, 245));
            case "dryer": return Machine(C(245, 225, 200), C(220, 140, 70), C(70, 70, 85), C(140, 140, 160));
            case "drycleaner": return DryCleaner();
            case "handwash": return HandWash();
            case "rack": return Rack();
            case "stainstation": return StainStation();
            case "iron": return IronBoard();
            case "fold": return FoldTable();
            case "counter": return Counter();
            case "table": return Table();
            case "trash": return Trash();
            case "kit": return Kit();
            case "bolt": return Bolt();
            case "cloth_tshirt": return TShirt(C(220, 70, 70), false);
            case "cloth_tshirt_folded": return Folded(C(220, 70, 70), C(180, 50, 50));
            case "cloth_shirt": return Shirt(false);
            case "cloth_shirt_folded": return Folded(C(170, 205, 240), C(130, 170, 210));
            case "cloth_sweater": return Sweater();
            case "cloth_sweater_folded": return Folded(C(80, 160, 90), C(55, 120, 65));
            case "cloth_suit": return Suit();
            case "cloth_suit_folded": return Folded(C(50, 60, 95), C(35, 40, 70));
            case "stain": return Stain();
            case "broken": return Broken();
            case "wall": return Wall(20, 1);
            case "floor": return Floor(20, 9);
        }
        Debug.LogWarning("PixelArt: unknown key " + key);
        return Make(8, 8, 8, p => p.Rect(0, 0, 8, 8, C(255, 0, 255)));
    }

    private static Sprite Player(Color32 shirt, Color32 hair)
    {
        return Make(16, 20, 16, p =>
        {
            var skin = C(250, 210, 170);
            // legs
            p.Rect(5, 0, 2, 4, C(50, 50, 70));
            p.Rect(9, 0, 2, 4, C(50, 50, 70));
            // body
            p.Box(3, 3, 10, 8, shirt, Outline);
            // apron
            p.Rect(5, 4, 6, 6, C(245, 245, 245));
            p.Rect(6, 7, 4, 1, C(200, 200, 210));
            // arms
            p.Rect(2, 5, 1, 4, skin);
            p.Rect(13, 5, 1, 4, skin);
            // head
            p.Circle(8, 14.5f, 4.6f, Outline);
            p.Circle(8, 14.5f, 3.8f, skin);
            // hair
            p.Rect(4, 16, 8, 3, hair);
            p.Rect(4, 15, 2, 1, hair);
            // eyes
            p.Set(6, 14, Outline);
            p.Set(9, 14, Outline);
            p.Set(7, 12, C(220, 120, 110));
            p.Set(8, 12, C(220, 120, 110));
        });
    }

    private static Sprite Kid()
    {
        return Make(14, 16, 16, p =>
        {
            var skin = C(250, 215, 180);
            p.Rect(4, 0, 2, 3, C(60, 60, 90));
            p.Rect(8, 0, 2, 3, C(60, 60, 90));
            p.Box(3, 2, 8, 6, C(250, 200, 60), Outline);
            p.Rect(4, 4, 6, 1, C(230, 120, 40));
            p.Circle(7, 10.5f, 4f, Outline);
            p.Circle(7, 10.5f, 3.2f, skin);
            // cap
            p.Rect(3, 12, 8, 3, C(220, 50, 50));
            p.Rect(9, 12, 4, 1, C(180, 30, 30));
            p.Set(5, 10, Outline);
            p.Set(8, 10, Outline);
            p.Rect(6, 8, 2, 1, C(200, 80, 80));
        });
    }

    private static Sprite Machine(Color32 body, Color32 panel, Color32 glass, Color32 glassHi)
    {
        return Make(24, 24, 16, p =>
        {
            p.Box(0, 0, 24, 24, body, Outline);
            // shadow at the bottom
            p.Rect(1, 1, 22, 2, C(190, 190, 200));
            // control panel
            p.Rect(1, 18, 22, 5, panel);
            p.HLine(1, 22, 18, Outline);
            p.Circle(4.5f, 20.5f, 1.6f, C(60, 60, 70));
            p.Circle(8.5f, 20.5f, 1.6f, C(60, 60, 70));
            p.Rect(14, 19, 7, 3, C(30, 40, 30));
            p.Rect(15, 20, 3, 1, C(90, 230, 120));
            // door
            p.Circle(12, 9.5f, 7.6f, Outline);
            p.Circle(12, 9.5f, 6.8f, C(170, 175, 185));
            p.Circle(12, 9.5f, 5.4f, glass);
            p.Circle(10, 11.5f, 1.8f, glassHi);
            p.Rect(19, 8, 2, 3, C(120, 120, 130));
        });
    }

    private static Sprite DryCleaner()
    {
        return Make(24, 24, 16, p =>
        {
            p.Box(0, 0, 24, 24, C(110, 180, 175), Outline);
            p.Rect(1, 1, 22, 2, C(80, 140, 135));
            p.Rect(1, 18, 22, 5, C(40, 90, 95));
            p.HLine(1, 22, 18, Outline);
            p.Rect(3, 20, 4, 1, C(240, 220, 90));
            p.Rect(9, 20, 4, 1, C(240, 220, 90));
            p.Rect(15, 19, 6, 3, C(20, 30, 30));
            p.Rect(16, 20, 2, 1, C(250, 120, 90));
            // square window with a hanger inside
            p.Box(4, 3, 16, 13, C(200, 230, 235), Outline);
            p.Rect(5, 4, 14, 11, C(150, 200, 210));
            p.Set(12, 13, Outline);
            p.Set(12, 12, Outline);
            p.HLine(8, 16, 10, Outline);
            p.Set(9, 11, Outline);
            p.Set(15, 11, Outline);
            p.Rect(6, 13, 3, 1, C(230, 245, 250));
        });
    }

    private static Sprite HandWash()
    {
        return Make(26, 24, 16, p =>
        {
            // wooden tub
            p.Circle(13, 11, 11.5f, Outline);
            p.Circle(13, 11, 10.6f, C(170, 115, 70));
            p.Circle(13, 11, 8.6f, Outline);
            p.Circle(13, 11, 7.9f, C(110, 175, 225));
            // bands
            p.HLine(3, 23, 4, C(120, 80, 50));
            p.HLine(3, 23, 18, C(120, 80, 50));
            // bubbles
            p.Circle(10, 13, 1.6f, C(240, 248, 255));
            p.Circle(15, 9, 1.3f, C(240, 248, 255));
            p.Circle(16, 14, 1f, C(240, 248, 255));
            p.Circle(9, 8, 0.9f, C(240, 248, 255));
            // washboard
            p.Rect(20, 13, 4, 10, C(200, 160, 110));
            for (int y = 15; y < 22; y += 2) p.HLine(20, 23, y, C(150, 110, 70));
        });
    }

    private static Sprite Rack()
    {
        return Make(44, 22, 16, p =>
        {
            // shadow
            p.Rect(2, 0, 40, 2, C(0, 0, 0, 40));
            // posts
            p.Box(1, 1, 4, 20, C(160, 110, 70), Outline);
            p.Box(39, 1, 4, 20, C(160, 110, 70), Outline);
            // lines
            p.HLine(4, 40, 17, C(220, 220, 230));
            p.HLine(4, 40, 16, C(150, 150, 160));
            p.HLine(4, 40, 9, C(220, 220, 230));
            p.HLine(4, 40, 8, C(150, 150, 160));
            // pegs
            for (int x = 9; x < 40; x += 6)
            {
                p.Rect(x, 16, 1, 3, C(240, 180, 60));
            }
        });
    }

    private static Sprite StainStation()
    {
        return Make(24, 24, 16, p =>
        {
            p.Box(0, 0, 24, 24, C(225, 225, 215), Outline);
            p.Rect(1, 1, 22, 3, C(185, 185, 175));
            // spray bottle
            p.Box(17, 10, 5, 10, C(80, 170, 230), Outline);
            p.Rect(18, 19, 3, 3, C(240, 240, 240));
            p.Rect(15, 21, 3, 1, C(240, 240, 240));
            // brush
            p.Box(2, 17, 9, 4, C(190, 130, 70), Outline);
            for (int x = 3; x < 10; x += 2) p.Rect(x, 15, 1, 2, C(240, 220, 160));
            // bubble marks
            p.Circle(7, 8, 3f, C(205, 225, 240));
            p.Circle(10, 6, 2f, C(205, 225, 240));
        });
    }

    private static Sprite IronBoard()
    {
        return Make(30, 20, 16, p =>
        {
            // legs (shadow)
            p.Rect(6, 0, 2, 4, C(90, 90, 100));
            p.Rect(20, 0, 2, 4, C(90, 90, 100));
            // board (pointy on the right)
            p.Rect(1, 4, 22, 12, Outline);
            for (int i = 0; i < 6; i++) p.Rect(23 + i, 5 + i, 1, 10 - i * 2, Outline);
            p.Rect(2, 5, 21, 10, C(170, 190, 225));
            for (int i = 0; i < 5; i++) p.Rect(23 + i, 6 + i, 1, 8 - i * 2, C(170, 190, 225));
            for (int x = 3; x < 22; x += 4) p.Rect(x, 7, 2, 1, C(140, 160, 205));
            for (int x = 5; x < 22; x += 4) p.Rect(x, 12, 2, 1, C(140, 160, 205));
            // iron
            p.Box(18, 14, 9, 5, C(230, 80, 70), Outline);
            p.Rect(20, 18, 5, 2, Outline);
        });
    }

    private static Sprite FoldTable()
    {
        return Make(24, 24, 16, p =>
        {
            p.Box(0, 0, 24, 24, C(200, 160, 110), Outline);
            p.Rect(1, 1, 22, 3, C(160, 120, 80));
            for (int y = 6; y < 23; y += 5) p.HLine(1, 22, y, C(185, 145, 100));
            // folded stack
            p.Box(3, 13, 10, 3, C(220, 70, 70), Outline);
            p.Box(3, 15, 10, 3, C(170, 205, 240), Outline);
            p.Box(3, 17, 10, 3, C(80, 160, 90), Outline);
            // box
            p.Box(14, 4, 8, 7, C(210, 170, 110), Outline);
            p.HLine(14, 21, 8, Outline);
        });
    }

    private static Sprite Counter()
    {
        return Make(22, 70, 16, p =>
        {
            p.Box(0, 0, 22, 70, C(150, 95, 60), Outline);
            p.Rect(1, 1, 3, 68, C(115, 70, 45));
            p.Rect(18, 1, 3, 68, C(180, 120, 80));
            for (int y = 17; y < 69; y += 17) p.HLine(1, 20, y, C(120, 75, 50));
            // bell on top
            p.Circle(11, 66, 2.5f, C(240, 200, 60));
            p.Rect(8, 63, 7, 1, Outline);
        });
    }

    private static Sprite Table()
    {
        return Make(20, 20, 16, p =>
        {
            p.Box(0, 0, 20, 20, C(185, 140, 95), Outline);
            p.Rect(1, 1, 18, 3, C(145, 105, 70));
            for (int y = 7; y < 19; y += 5) p.HLine(1, 18, y, C(165, 122, 82));
        });
    }

    private static Sprite Trash()
    {
        return Make(16, 18, 16, p =>
        {
            p.Box(2, 0, 12, 14, C(120, 130, 140), Outline);
            for (int x = 4; x < 13; x += 3) p.VLine(x, 2, 11, C(95, 105, 115));
            p.Box(1, 13, 14, 3, C(150, 160, 170), Outline);
            p.Rect(6, 16, 4, 2, Outline);
        });
    }

    private static Sprite Kit()
    {
        return Make(16, 14, 16, p =>
        {
            p.Box(1, 0, 14, 10, C(245, 200, 40), Outline);
            p.Rect(5, 10, 6, 3, Outline);
            p.Rect(6, 11, 4, 1, C(245, 200, 40));
            // lightning
            p.Rect(8, 6, 2, 3, Outline);
            p.Rect(6, 4, 4, 2, Outline);
            p.Rect(6, 1, 2, 3, Outline);
        });
    }

    private static Sprite Bolt()
    {
        return Make(8, 10, 16, p =>
        {
            p.Rect(4, 6, 3, 4, C(255, 220, 40));
            p.Rect(2, 4, 5, 2, C(255, 220, 40));
            p.Rect(1, 0, 3, 4, C(255, 220, 40));
        });
    }

    private static Sprite TShirt(Color32 col, bool unused)
    {
        return Make(16, 16, 16, p =>
        {
            var dark = C(col.r * 3 / 4, col.g * 3 / 4, col.b * 3 / 4);
            // sleeves
            p.Box(0, 9, 5, 5, col, Outline);
            p.Box(11, 9, 5, 5, col, Outline);
            // body
            p.Box(3, 1, 10, 14, col, Outline);
            p.Rect(4, 2, 8, 1, dark);
            // neck
            p.Rect(6, 13, 4, 2, Outline);
            p.Rect(7, 14, 2, 1, Clear);
            // print
            p.Circle(8, 8, 2.2f, C(250, 240, 200));
        });
    }

    private static Sprite Shirt(bool unused)
    {
        var col = C(170, 205, 240);
        return Make(16, 16, 16, p =>
        {
            p.Box(0, 4, 4, 10, col, Outline);
            p.Box(12, 4, 4, 10, col, Outline);
            p.Box(3, 1, 10, 14, col, Outline);
            // collar
            p.Rect(5, 13, 2, 2, C(250, 250, 250));
            p.Rect(9, 13, 2, 2, C(250, 250, 250));
            p.VLine(8, 2, 12, C(120, 150, 190));
            for (int y = 3; y < 12; y += 3) p.Set(7, y, C(255, 255, 255));
            p.Rect(10, 9, 2, 2, C(140, 175, 215));
        });
    }

    private static Sprite Sweater()
    {
        var col = C(80, 160, 90);
        var dark = C(55, 120, 65);
        return Make(16, 16, 16, p =>
        {
            p.Box(0, 2, 4, 12, col, Outline);
            p.Box(12, 2, 4, 12, col, Outline);
            p.Box(3, 1, 10, 14, col, Outline);
            // knit pattern
            for (int y = 4; y < 13; y += 3)
                for (int x = 4; x < 12; x += 2)
                    p.Set(x + (y / 3) % 2, y, dark);
            // ribbed hem & cuffs
            p.Rect(4, 2, 8, 1, dark);
            p.Rect(1, 3, 2, 1, dark);
            p.Rect(13, 3, 2, 1, dark);
            p.Rect(6, 13, 4, 2, dark);
        });
    }

    private static Sprite Suit()
    {
        var col = C(50, 60, 95);
        return Make(16, 16, 16, p =>
        {
            p.Box(0, 3, 4, 11, col, Outline);
            p.Box(12, 3, 4, 11, col, Outline);
            p.Box(3, 1, 10, 14, col, Outline);
            // white shirt V
            for (int i = 0; i < 5; i++) p.Rect(6 + i / 2, 14 - i * 2, 4 - (i / 2) * 2, 2, C(245, 245, 245));
            // tie
            p.Rect(7, 8, 2, 6, C(200, 50, 60));
            // buttons
            p.Set(7, 5, C(200, 190, 120));
            p.Set(7, 3, C(200, 190, 120));
            p.Rect(4, 10, 2, 1, C(80, 90, 130));
        });
    }

    private static Sprite Folded(Color32 col, Color32 dark)
    {
        return Make(16, 16, 16, p =>
        {
            p.Box(1, 3, 14, 5, col, Outline);
            p.Box(1, 7, 14, 5, col, Outline);
            p.HLine(2, 13, 8, dark);
            // ribbon
            p.VLine(8, 3, 11, C(250, 230, 120));
            // check tag
            p.Box(10, 10, 6, 6, C(80, 200, 100), Outline);
            p.Set(11, 13, C(255, 255, 255));
            p.Set(12, 12, C(255, 255, 255));
            p.Set(13, 13, C(255, 255, 255));
            p.Set(14, 14, C(255, 255, 255));
        });
    }

    private static Sprite Stain()
    {
        return Make(16, 16, 16, p =>
        {
            var s = C(120, 80, 40, 230);
            p.Circle(6, 7, 2.6f, s);
            p.Circle(9, 6, 1.8f, s);
            p.Circle(10, 10, 1.5f, s);
            p.Set(4, 10, s);
            p.Set(12, 5, s);
            p.Set(7, 3, s);
        });
    }

    private static Sprite Broken()
    {
        return Make(16, 16, 16, p =>
        {
            var red = C(230, 40, 40);
            for (int i = 2; i < 14; i++)
            {
                p.Set(i, i, red);
                p.Set(i + 1, i, red);
                p.Set(15 - i, i, red);
                p.Set(14 - i, i, red);
            }
        });
    }

    private static Sprite Wall(int unitsW, int unitsH)
    {
        int w = unitsW * 16, h = unitsH * 16;
        return Make(w, h, 16, p =>
        {
            p.Rect(0, 0, w, h, C(150, 185, 190));
            for (int x = 0; x < w; x += 8) p.VLine(x, 3, h - 1, C(135, 172, 178));
            p.Rect(0, 0, w, 3, C(110, 80, 60));
            p.HLine(0, w - 1, 3, C(80, 60, 45));
            p.HLine(0, w - 1, h - 1, C(90, 120, 125));
        });
    }

    private static Sprite Floor(int unitsW, int unitsH)
    {
        int w = unitsW * 16, h = unitsH * 16;
        return Make(w, h, 16, p =>
        {
            for (int ty = 0; ty < unitsH; ty++)
                for (int tx = 0; tx < unitsW; tx++)
                {
                    bool alt = (tx + ty) % 2 == 0;
                    var c = alt ? C(236, 228, 212) : C(222, 212, 194);
                    p.Rect(tx * 16, ty * 16, 16, 16, c);
                    p.HLine(tx * 16, tx * 16 + 15, ty * 16, C(205, 195, 178));
                    p.VLine(tx * 16, ty * 16, ty * 16 + 15, C(205, 195, 178));
                }
        });
    }
}
