using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Helpers to build uGUI in code (no prefabs needed for the prototype).</summary>
public static class UIFactory
{
    public static Canvas CreateCanvas(string name, int sortingOrder = 0)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        return canvas;
    }

    public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static Image Panel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, bool blockClicks = false)
    {
        var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = blockClicks;
        return img;
    }

    public static Image Stretch(Transform parent, string name, Color color, bool blockClicks)
    {
        var img = Panel(parent, name, color, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, blockClicks);
        return img;
    }

    public static Text Label(Transform parent, string name, string text, int fontSize, Color color, TextAnchor align,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = WorldText.DefaultFont;
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.supportRichText = true;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var shadow = rt.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    public static Button Button(Transform parent, string label, Vector2 pos, Vector2 size, Color color, UnityAction onClick)
    {
        var img = Panel(parent, "Button_" + label, color, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, true);
        var btn = img.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);
        Label(img.transform, "Text", label, 36, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        return btn;
    }
}
