using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 小孩搗亂: runs to a cloth that is NOT inside a machine (counter, tables, rack, work tables)
/// and stains it. The cloth goes back to the first step of its procedure.
/// Press Space near the kid (or click it) to shoo them away.
/// </summary>
public class Kid : MonoBehaviour
{
    private static readonly List<Kid> All = new List<Kid>();

    [SerializeField] private float speed = 2.4f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float giveUpTime = 18f;

    private enum Mode { Seeking, Fleeing }

    private Mode mode = Mode.Seeking;
    private Vector2 door;
    private List<Station> stations;
    private ClothBase targetCloth;
    private Vector2 wanderPoint;
    private float lifeTime;
    private float retargetTimer;
    private SpriteRenderer sr;
    private float anim;
    private CharacterAnimator animator;
    private Vector2 lastMove = Vector2.down;

    public static Kid FindNear(Vector2 pos, float radius)
    {
        foreach (var k in All)
        {
            if (k != null && k.mode == Mode.Seeking && Vector2.Distance(pos, k.transform.position) <= radius) return k;
        }
        return null;
    }

    public void Init(Vector2 doorPos, List<Station> allStations)
    {
        door = doorPos;
        stations = allStations;
        wanderPoint = doorPos + new Vector2(3f, 3f);
    }

    private void Awake()
    {
        All.Add(this);
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Get("kid");
        sr.sortingOrder = 11;
        animator = CharacterAnimator.TryAttach(sr, "Kid", 24f);
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;
        WorldText.Create(transform, new Vector2(0f, 0.6f), "!", 0.4f, HUD.Bad, 32);
    }

    private void OnDestroy()
    {
        All.Remove(this);
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning) return;

        lifeTime += Time.deltaTime;
        anim += Time.deltaTime * 16f;
        if (animator != null) animator.Set(lastMove, true);
        else sr.transform.localScale = new Vector3(1f, 1f + Mathf.Abs(Mathf.Sin(anim)) * 0.1f, 1f);

        if (mode == Mode.Fleeing)
        {
            MoveTowards(door + Vector2.down * 1.5f, fleeSpeed);
            if (Vector2.Distance(transform.position, door + Vector2.down * 1.5f) < 0.2f) Destroy(gameObject);
            return;
        }

        if (lifeTime > giveUpTime)
        {
            mode = Mode.Fleeing;
            return;
        }

        retargetTimer -= Time.deltaTime;
        if (!IsValidTarget(targetCloth) || retargetTimer <= 0f)
        {
            targetCloth = PickTarget();
            retargetTimer = 1f;
        }

        if (targetCloth == null)
        {
            // nothing to mess with: wander around
            MoveTowards(wanderPoint, speed);
            if (Vector2.Distance(transform.position, wanderPoint) < 0.3f)
            {
                wanderPoint = new Vector2(Random.Range(-7f, 6f), Random.Range(-4f, 0.5f));
            }
            return;
        }

        Vector2 tp = targetCloth.transform.position;
        MoveTowards(tp, speed);
        if (Vector2.Distance(transform.position, tp) < 0.7f)
        {
            targetCloth.MakeDirty();
            HUD.Toast($"The kid smeared the {targetCloth.DisplayName}! Remove the stain and start over.", HUD.Bad);
            FloatingText.Spawn(tp + Vector2.up * 0.5f, "SPLAT!", new Color(0.6f, 0.4f, 0.2f));
            mode = Mode.Fleeing;
        }
    }

    private void MoveTowards(Vector2 p, float s)
    {
        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, p, s * Time.deltaTime);
        if (next != pos) lastMove = next - pos;
        if (animator == null && Mathf.Abs(next.x - pos.x) > 0.001f) sr.flipX = next.x < pos.x;
        transform.position = next;
    }

    private bool IsValidTarget(ClothBase c)
    {
        return c != null && !c.IsBroken && !c.IsStained && c.CurrentStation != null && c.CurrentStation.KidCanReach;
    }

    private ClothBase PickTarget()
    {
        ClothBase best = null;
        float bestDist = float.MaxValue;
        foreach (var st in stations)
        {
            if (st == null || !st.KidCanReach) continue;
            foreach (var c in st.ClothesHere())
            {
                if (!IsValidTarget(c)) continue;
                float d = Vector2.Distance(transform.position, c.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }
        }
        return best;
    }

    public void Shoo()
    {
        if (mode == Mode.Fleeing) return;
        mode = Mode.Fleeing;
        HUD.Toast("Shooed the kid away!", HUD.Good);
        FloatingText.Spawn(transform.position + Vector3.up * 0.6f, "Go away!", HUD.Good);
    }
}
