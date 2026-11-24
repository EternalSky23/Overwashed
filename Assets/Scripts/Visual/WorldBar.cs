using UnityEngine;

/// <summary>Small progress bar in the world (machines, work stations).</summary>
public class WorldBar : MonoBehaviour
{
    private Transform fill;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer zoneRenderer;
    private float width;
    private const float Height = 0.14f;

    public static WorldBar Create(Transform parent, Vector2 localPos, float width)
    {
        var go = new GameObject("Bar");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var bar = go.AddComponent<WorldBar>();
        bar.width = width;

        var bg = MakeQuad(go.transform, "Bg", new Color(0.08f, 0.08f, 0.1f, 0.9f), 25);
        bg.transform.localScale = new Vector3(width + 0.06f, Height + 0.06f, 1f);

        bar.zoneRenderer = MakeQuad(go.transform, "Zone", new Color(0.2f, 0.8f, 0.3f, 0.55f), 26);
        bar.zoneRenderer.enabled = false;

        bar.fillRenderer = MakeQuad(go.transform, "Fill", Color.white, 27);
        bar.fill = bar.fillRenderer.transform;
        bar.SetValue(0f, Color.white);
        return bar;
    }

    private static SpriteRenderer MakeQuad(Transform parent, string name, Color c, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Get("white");
        sr.color = c;
        sr.sortingOrder = order;
        return sr;
    }

    public void SetValue(float v, Color c)
    {
        v = Mathf.Clamp01(v);
        fill.localScale = new Vector3(width * v, Height, 1f);
        fill.localPosition = new Vector3(-width * 0.5f + width * v * 0.5f, 0f, 0f);
        fillRenderer.color = c;
    }

    public void SetZone(float min, float max)
    {
        zoneRenderer.enabled = true;
        zoneRenderer.transform.localScale = new Vector3(width * (max - min), Height + 0.06f, 1f);
        zoneRenderer.transform.localPosition = new Vector3(-width * 0.5f + width * (min + max) * 0.5f, 0f, 0f);
    }

    public void Show(bool on)
    {
        if (gameObject.activeSelf != on) gameObject.SetActive(on);
    }
}
