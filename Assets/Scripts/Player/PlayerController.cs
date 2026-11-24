using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Top-down player.
/// Move: WASD / Arrow keys
/// Space: interact (pick up / put down / use kit / deliver / shoo kid) with the station in front
/// E or Left Mouse: work (scrub, remove stain, iron, fold). Mouse also clicks stations within reach.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private float moveSpeed = 5.2f;
    [SerializeField] private float reach = 0.75f;
    [SerializeField] private float mouseReach = 1.3f;

    public IHoldable Held { get; private set; }
    public bool IsHolding => Held != null && (Held as Object) != null;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform hand;
    private Vector2 moveInput;
    private Vector2 facing = Vector2.down;

    private Station target;
    private Station workTarget;
    private bool workByMouse;
    private float walkAnim;
    private CharacterAnimator anim;

    /// <summary>Townsfolk sheet used for the player when the third-party art is present.</summary>
    public static string CharacterName = "Mailman";

    private void Awake()
    {
        Instance = this;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Get("player");
        sr.sortingOrder = 10;
        anim = CharacterAnimator.TryAttach(sr, CharacterName, 24f);

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.32f;
        col.offset = new Vector2(0f, -0.15f);

        hand = new GameObject("Hand").transform;
        hand.SetParent(transform, false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private bool CanAct => GameManager.Instance != null && GameManager.Instance.IsRunning && Time.timeScale > 0f;

    private void Update()
    {
        if (!CanAct)
        {
            moveInput = Vector2.zero;
            EndWork();
            return;
        }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        if (moveInput.sqrMagnitude > 0.01f)
        {
            facing = moveInput.normalized;
            if (anim == null && Mathf.Abs(moveInput.x) > 0.01f) sr.flipX = moveInput.x < 0f;
            walkAnim += Time.deltaTime * 14f;
        }
        else
        {
            walkAnim = 0f;
        }
        // tiny bounce while walking
        if (anim != null) anim.Set(facing, moveInput.sqrMagnitude > 0.01f);
        else sr.transform.localScale = new Vector3(1f, 1f + Mathf.Abs(Mathf.Sin(walkAnim)) * 0.06f, 1f);

        UpdateHand();
        UpdateTarget();

        // ---- Space: interact ----
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DoInteract(target, true);
        }

        // ---- E: work ----
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (target != null && target.WantsWork(this)) BeginWork(target, false);
            else if (target != null) target.WorkPress(this); // gives feedback (e.g. "no stain")
            else HUD.Toast("Nothing to work on here.", HUD.Info);
        }

        // ---- Mouse ----
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            HandleMouseClick();
        }

        // ---- Holding work ----
        if (workTarget != null)
        {
            bool held = workByMouse ? Input.GetMouseButton(0) : Input.GetKey(KeyCode.E);
            bool stillInRange = workByMouse ? InReach(workTarget, mouseReach) : workTarget == target;
            if (!held || !stillInRange)
            {
                EndWork();
            }
            else
            {
                workTarget.WorkHold(this, Time.deltaTime);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }

    // ---------------- Holding ----------------

    public void Hold(IHoldable item)
    {
        if (item == null) return;
        Held = item;
        item.CurrentStation = null;
        item.AttachTo(hand, 12);
        AudioManager.Play(item is UpgradeKit ? AudioManager.Sfx.Kit : AudioManager.Sfx.Pickup, 0.8f, 1.2f, 0.08f);
        item.transform.localScale = Vector3.one * 0.75f;
    }

    public IHoldable Release()
    {
        var item = Held;
        if (item != null) AudioManager.Play(AudioManager.Sfx.PutDown, 0.8f, 1.2f, 0.08f);
        Held = null;
        if (item != null) item.transform.localScale = Vector3.one * 0.85f;
        return item;
    }

    private void UpdateHand()
    {
        Vector2 offset = facing * 0.42f + new Vector2(0f, -0.05f);
        hand.localPosition = offset;
        if (IsHolding)
        {
            // draw held item behind the player when facing up
            int order = facing.y > 0.5f ? 9 : 12;
            if (Held is ClothBase c) c.SetSorting(order);
            else
            {
                var r = Held.gameObject.GetComponent<SpriteRenderer>();
                if (r != null) r.sortingOrder = order;
            }
        }
    }

    // ---------------- Targeting ----------------

    private void UpdateTarget()
    {
        Vector2 probe = (Vector2)transform.position + facing * 0.55f;
        var hits = Physics2D.OverlapCircleAll(probe, reach);
        Station best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            var st = h.GetComponent<Station>();
            if (st == null) continue;
            float d = Vector2.Distance(probe, h.ClosestPoint(probe));
            if (d < bestDist)
            {
                bestDist = d;
                best = st;
            }
        }

        if (best != target)
        {
            if (target != null) target.SetHighlight(false);
            target = best;
            if (target != null) target.SetHighlight(true);
        }
    }

    private bool InReach(Station st, float distance)
    {
        if (st == null || st.Collider == null) return false;
        Vector2 p = transform.position;
        return Vector2.Distance(p, st.Collider.ClosestPoint(p)) <= distance;
    }

    // ---------------- Actions ----------------

    private void DoInteract(Station st, bool fromKeyboard)
    {
        // 1. shoo a kid nearby
        var kid = Kid.FindNear(transform.position, 1.4f);
        if (kid != null && fromKeyboard)
        {
            kid.Shoo();
            return;
        }

        // 2. pick up an upgrade kit lying on the floor
        if (!IsHolding && fromKeyboard)
        {
            var kit = RandomEventManager.Instance != null ? RandomEventManager.Instance.FindFloorKit(transform.position, 1.0f) : null;
            if (kit != null)
            {
                RandomEventManager.Instance.PickUpKit(kit);
                Hold(kit);
                HUD.Toast("Picked up an upgrade kit! Use it on a machine (Space).", HUD.Good);
                return;
            }
        }

        if (st != null) st.Interact(this);
    }

    private void HandleMouseClick()
    {
        Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.OverlapPointAll(world);
        foreach (var h in hits)
        {
            var kid = h.GetComponent<Kid>();
            if (kid != null)
            {
                if (Vector2.Distance(transform.position, kid.transform.position) <= 3f) kid.Shoo();
                else HUD.Toast("Get closer to shoo the kid!", HUD.Warn);
                return;
            }

            var kit = h.GetComponent<UpgradeKit>();
            if (kit != null && kit.OnFloor)
            {
                if (IsHolding) HUD.Toast("Your hands are full.", HUD.Warn);
                else if (Vector2.Distance(transform.position, kit.transform.position) <= mouseReach + 0.4f)
                {
                    RandomEventManager.Instance.PickUpKit(kit);
                    Hold(kit);
                }
                else HUD.Toast("Too far away.", HUD.Warn);
                return;
            }
        }

        foreach (var h in hits)
        {
            var st = h.GetComponent<Station>();
            if (st == null) continue;
            if (!InReach(st, mouseReach))
            {
                HUD.Toast("Too far away - walk closer.", HUD.Warn);
                return;
            }
            if (st.WantsWork(this)) BeginWork(st, true);
            else st.Interact(this);
            return;
        }
    }

    private void BeginWork(Station st, bool byMouse)
    {
        EndWork();
        workTarget = st;
        workByMouse = byMouse;
        st.WorkPress(this);
    }

    private void EndWork()
    {
        if (workTarget != null)
        {
            var w = workTarget;
            workTarget = null;
            w.WorkRelease(this);
        }
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
