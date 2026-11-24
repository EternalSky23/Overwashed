using UnityEngine;

/// <summary>
/// Ironing board: HOLD E / left mouse, release when the bar is inside the green zone.
/// Release too early = try again; hold too long = burnt.
/// </summary>
public class IronStation : Station
{
    [SerializeField] private float fillTime = 2.2f;
    [SerializeField] private float zoneMin = 0.62f;
    [SerializeField] private float zoneMax = 0.85f;
    private Slot slot;
    private WorldText hint;
    private bool holding;

    public override bool KidCanReach => true;

    public void Init()
    {
        slot = AddSlot(new Vector2(-0.15f, 0.05f), new Vector2(0f, 0.8f), 1.4f);
        slot.Bar.SetZone(zoneMin, zoneMax);
        hint = WorldText.Create(transform, new Vector2(0f, 1.05f), "", 0.24f, Color.white, 31);
    }

    private bool HasValidCloth => !slot.IsEmpty && slot.Cloth != null && !slot.Cloth.IsBroken;

    public override bool WantsWork(PlayerController player) => HasValidCloth && !slot.Cloth.IsFinished;

    public override void Interact(PlayerController player)
    {
        if (player.IsHolding && !slot.IsEmpty)
        {
            HUD.Toast("The ironing board is occupied.", HUD.Warn);
            return;
        }
        DefaultPutOrTake(player);
    }

    public override void WorkPress(PlayerController player)
    {
        if (!WantsWork(player)) return;
        holding = true;
        slot.Progress = 0f;
    }

    public override void WorkHold(PlayerController player, float dt)
    {
        if (!holding || !WantsWork(player)) return;
        slot.Progress += dt / fillTime;
        if (slot.Progress >= 1f)
        {
            holding = false;
            slot.Progress = 0f;
            slot.Cloth.Break($"{slot.Cloth.DisplayName} burnt by the iron!");
        }
    }

    public override void WorkRelease(PlayerController player)
    {
        if (!holding) return;
        holding = false;
        float p = slot.Progress;
        slot.Progress = 0f;
        if (!HasValidCloth) return;

        if (p >= zoneMin && p <= zoneMax)
        {
            var cloth = slot.Cloth;
            if (cloth.TryProcess(ProcessStep.Iron)) HUD.Toast($"{cloth.DisplayName} ironed perfectly!", HUD.Good);
        }
        else if (p > 0.08f)
        {
            HUD.Toast("Not pressed enough - try again!", HUD.Warn);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (slot == null) return;
        bool work = WantsWork(null);
        slot.Bar.Show(work);
        slot.Bar.SetValue(slot.Progress, slot.Progress > zoneMax ? HUD.Bad : new Color(1f, 0.85f, 0.35f));
        hint.SetText(work ? "Hold, release in green" : "");
        if (!work) holding = false;
    }
}
