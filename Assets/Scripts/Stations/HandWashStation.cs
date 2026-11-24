using UnityEngine;

/// <summary>
/// Hand wash basin: put a cloth in, then mash E / click the mouse to scrub it clean.
/// </summary>
public class HandWashStation : Station
{
    [SerializeField] private float progressPerScrub = 0.1f;
    private Slot slot;
    private WorldText hint;

    public void Init()
    {
        slot = AddSlot(new Vector2(0f, 0.05f), new Vector2(0f, 0.95f));
        hint = WorldText.Create(transform, new Vector2(0f, 1.3f), "", 0.24f, Color.white, 31);
    }

    public override bool WantsWork(PlayerController player)
    {
        return !slot.IsEmpty && !slot.Done && slot.Cloth != null && !slot.Cloth.IsBroken;
    }

    public override void Interact(PlayerController player)
    {
        if (player.IsHolding && !slot.IsEmpty)
        {
            HUD.Toast("The basin is full.", HUD.Warn);
            return;
        }
        DefaultPutOrTake(player);
    }

    public override void WorkPress(PlayerController player)
    {
        if (!WantsWork(player)) return;
        slot.Progress += progressPerScrub;
        FloatingText.Spawn(slot.Anchor.position + (Vector3)Random.insideUnitCircle * 0.3f, "scrub!", new Color(0.6f, 0.85f, 1f), 0.5f);
        slot.Anchor.localPosition = new Vector3(Random.Range(-0.08f, 0.08f), 0.05f + Random.Range(-0.05f, 0.05f), 0f);
        if (slot.Progress >= 1f)
        {
            slot.Done = true;
            var cloth = slot.Cloth;
            cloth.Wash(WashMethod.HandWash, null);
            if (!cloth.IsBroken) HUD.Toast($"{cloth.DisplayName} hand washed!", HUD.Info);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (slot == null) return;
        bool active = WantsWork(null);
        slot.Bar.Show(!slot.IsEmpty && slot.Cloth != null && !slot.Cloth.IsBroken);
        slot.Bar.SetValue(slot.Progress, slot.Done ? HUD.Good : new Color(0.3f, 0.7f, 1f));
        hint.SetText(active ? "Mash E / Click!" : "");
    }
}
