using UnityEngine;

/// <summary>Text placed in the world (labels above stations etc.).</summary>
public class WorldText : MonoBehaviour
{
    private TextMesh main;
    private TextMesh shadow;

    private static Font font;
    public static Font DefaultFont
    {
        get
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }

    public static WorldText Create(Transform parent, Vector2 localPos, string text, float height, Color color, int sortingOrder)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var wt = go.AddComponent<WorldText>();
        wt.shadow = MakeMesh(go.transform, new Vector2(0.03f, -0.03f), height, new Color(0f, 0f, 0f, 0.75f), sortingOrder);
        wt.main = MakeMesh(go.transform, Vector2.zero, height, color, sortingOrder + 1);
        wt.SetText(text);
        return wt;
    }

    private static TextMesh MakeMesh(Transform parent, Vector2 offset, float height, Color color, int order)
    {
        var go = new GameObject("Mesh");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = offset;
        var tm = go.AddComponent<TextMesh>();
        tm.font = DefaultFont;
        tm.fontSize = 48;
        // world height of a line ~= characterSize * fontSize / 10
        tm.characterSize = height * 10f / tm.fontSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = DefaultFont.material;
        mr.sortingOrder = order;
        return tm;
    }

    public void SetText(string text)
    {
        if (main.text == text) return;
        main.text = text;
        shadow.text = text;
    }

    public void SetColor(Color c)
    {
        main.color = c;
    }
}
