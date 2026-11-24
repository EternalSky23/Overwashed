using System.Collections.Generic;
using UnityEngine;

/// <summary>A place on a station where one item can sit.</summary>
public class Slot
{
    public Transform Anchor;
    public IHoldable Item;
    public float Progress;   // 0..1 work / machine progress
    public bool Done;        // the station's step was applied
    public float Overdue;    // seconds since done (machines)
    public WorldBar Bar;

    public ClothBase Cloth => Item as ClothBase;
    public bool IsEmpty => Item == null || (Item as Object) == null;

    public void ResetWork()
    {
        Progress = 0f;
        Done = false;
        Overdue = 0f;
    }
}

/// <summary>
/// Base class of every interactable station in the shop.
/// Controls: Space = Interact (pick up / put down / use), E or left mouse = Work (scrub, iron, fold...).
/// </summary>
public abstract class Station : MonoBehaviour
{
    [SerializeField]
    protected string title = "Station";

    protected SpriteRenderer body;
    protected readonly List<Slot> slots = new List<Slot>();
    private bool highlighted;

    public string Title => title;
    public BoxCollider2D Collider { get; private set; }

    /// <summary>Can the kid (random event) reach clothes placed here?</summary>
    public virtual bool KidCanReach => false;

    /// <summary>Visual + collider setup. Called by GameBootstrap after AddComponent.</summary>
    public virtual void Setup(string displayTitle, string spriteKey, Vector2 size)
    {
        title = displayTitle;
        body = gameObject.AddComponent<SpriteRenderer>();
        body.sprite = PixelArt.Get(spriteKey);
        body.sortingOrder = 1;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        Collider.size = size;

        WorldText.Create(transform, new Vector2(0f, -size.y * 0.5f - 0.22f), displayTitle, 0.28f, new Color(1f, 1f, 1f, 0.9f), 30);
    }

    protected Slot AddSlot(Vector2 offset, Vector2 barOffset, float barWidth = 1.1f)
    {
        var anchor = new GameObject("Slot" + slots.Count).transform;
        anchor.SetParent(transform, false);
        anchor.localPosition = offset;
        var slot = new Slot { Anchor = anchor };
        slot.Bar = WorldBar.Create(transform, barOffset, barWidth);
        slot.Bar.Show(false);
        slots.Add(slot);
        return slot;
    }

    public void SetHighlight(bool on)
    {
        if (highlighted == on || body == null) return;
        highlighted = on;
        body.color = on ? new Color(1f, 0.95f, 0.6f) : Color.white;
    }

    public IEnumerable<ClothBase> ClothesHere()
    {
        foreach (var s in slots)
        {
            if (!s.IsEmpty && s.Cloth != null) yield return s.Cloth;
        }
    }

    public bool HasFreeSlot()
    {
        foreach (var s in slots) if (s.IsEmpty) return true;
        return false;
    }

    protected Slot FreeSlot()
    {
        foreach (var s in slots) if (s.IsEmpty) return s;
        return null;
    }

    protected Slot FirstFilledSlot()
    {
        foreach (var s in slots) if (!s.IsEmpty) return s;
        return null;
    }

    // ---------------- Input entry points ----------------

    /// <summary>Space / left click when there is no work to do here.</summary>
    public virtual void Interact(PlayerController player)
    {
        DefaultPutOrTake(player);
    }

    /// <summary>True if E / left mouse should do work here (instead of a normal interact).</summary>
    public virtual bool WantsWork(PlayerController player) => false;
    public virtual void WorkPress(PlayerController player) { }
    public virtual void WorkHold(PlayerController player, float dt) { }
    public virtual void WorkRelease(PlayerController player) { }

    // ---------------- Helpers ----------------

    protected void DefaultPutOrTake(PlayerController player)
    {
        if (player.IsHolding)
        {
            if (!CanAccept(player.Held))
            {
                HUD.Toast($"Can't put that on the {title}.", HUD.Warn);
                return;
            }
            var free = FreeSlot();
            if (free == null)
            {
                HUD.Toast($"The {title} is full.", HUD.Warn);
                return;
            }
            Place(player.Release(), free);
        }
        else
        {
            var filled = PreferredTakeSlot();
            if (filled != null) player.Hold(Take(filled));
        }
    }

    protected virtual Slot PreferredTakeSlot() => FirstFilledSlot();

    protected virtual bool CanAccept(IHoldable item) => item is ClothBase;

    public void Place(IHoldable item, Slot slot)
    {
        slot.Item = item;
        slot.ResetWork();
        item.CurrentStation = this;
        item.AttachTo(slot.Anchor, 5);
        OnPlaced(slot);
    }

    /// <summary>Put an item on the first free slot (used by orders and debug tools).</summary>
    public bool ForcePlace(IHoldable item)
    {
        var free = FreeSlot();
        if (free == null) return false;
        Place(item, free);
        return true;
    }

    protected IHoldable Take(Slot slot)
    {
        var item = slot.Item;
        slot.Item = null;
        slot.ResetWork();
        slot.Bar.Show(false);
        if (item != null) item.Detach();
        OnTaken(slot);
        return item;
    }

    protected virtual void OnPlaced(Slot slot) { }
    protected virtual void OnTaken(Slot slot) { }

    /// <summary>Called by the cloth / managers when an item is destroyed while sitting here.</summary>
    public void RemoveDestroyed(IHoldable item)
    {
        foreach (var s in slots)
        {
            if (s.Item == item)
            {
                s.Item = null;
                s.ResetWork();
                s.Bar.Show(false);
            }
        }
    }

    protected virtual void Update()
    {
        // Clean up slots whose item was destroyed elsewhere
        foreach (var s in slots)
        {
            if (s.Item != null && (s.Item as Object) == null)
            {
                s.Item = null;
                s.ResetWork();
                s.Bar.Show(false);
            }
        }
    }
}
