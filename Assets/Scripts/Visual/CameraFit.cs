using UnityEngine;

/// <summary>
/// Keeps the whole shop visible between the top HUD band and the bottom controls bar,
/// for any window size / aspect ratio (16:9, 16:10, resized windows...).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Tooltip("World-space rectangle that must always be visible (floor + wall).")]
    public Rect worldBounds = new Rect(-10.1f, -5.6f, 20.2f, 10.15f);

    private Camera cam;
    private int lastW, lastH;
    private float lastScale;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        Fit();
    }

    private void LateUpdate()
    {
        float scale = CurrentCanvasScale();
        if (Screen.width != lastW || Screen.height != lastH || !Mathf.Approximately(scale, lastScale)) Fit();
    }

    private static float CurrentCanvasScale()
    {
        if (HUD.Instance != null && HUD.Instance.Canvas != null) return HUD.Instance.Canvas.scaleFactor;
        // same formula as CanvasScaler (1920x1080 reference, match 0.5) before the canvas exists
        float w = Mathf.Log(Screen.width / 1920f, 2f);
        float h = Mathf.Log(Screen.height / 1080f, 2f);
        return Mathf.Pow(2f, Mathf.Lerp(w, h, 0.5f));
    }

    public void Fit()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        lastScale = CurrentCanvasScale();
        if (Screen.height <= 0) return;

        float topFrac = HUD.TopBand * lastScale / Screen.height;
        float bottomFrac = HUD.BottomBand * lastScale / Screen.height;
        float availFrac = Mathf.Clamp(1f - topFrac - bottomFrac, 0.3f, 1f);
        float aspect = (float)Screen.width / Screen.height;

        // ortho size so the bounds fit vertically inside the free band, and horizontally in the window
        float sizeForHeight = worldBounds.height / (2f * availFrac);
        float sizeForWidth = worldBounds.width / (2f * aspect) * 1.02f;
        float size = Mathf.Max(sizeForHeight, sizeForWidth);
        cam.orthographicSize = size;

        // centre the bounds inside the free band
        float bandCentre = bottomFrac + availFrac * 0.5f; // in viewport 0..1
        float y = worldBounds.center.y + (0.5f - bandCentre) * 2f * size;
        transform.position = new Vector3(worldBounds.center.x, y, -10f);
    }
}
