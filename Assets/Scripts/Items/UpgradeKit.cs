using UnityEngine;

/// <summary>
/// Random event item: dropped on the floor. Carry it to a machine (washer / dryer / dry cleaner)
/// and press Space to make that machine permanently 2x faster.
/// </summary>
public class UpgradeKit : MonoBehaviour, IHoldable
{
    public Station CurrentStation { get; set; }
    public bool OnFloor { get; private set; } = true;

    private SpriteRenderer sr;
    private float bobTime;

    private void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Get("kit");
        sr.sortingOrder = 4;
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;
    }

    private void Update()
    {
        if (!OnFloor) return;
        // little bobbing so the player notices it
        bobTime += Time.deltaTime;
        sr.transform.localScale = Vector3.one * (1f + Mathf.Sin(bobTime * 6f) * 0.08f);
    }

    public void AttachTo(Transform anchor, int sortingOrder)
    {
        OnFloor = false;
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        sr.sortingOrder = sortingOrder;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void Detach()
    {
        CurrentStation = null;
    }
}
