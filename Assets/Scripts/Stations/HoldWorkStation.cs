using UnityEngine;

/// <summary>
/// Stain remover / folding table: place a cloth, then HOLD E / left mouse until the bar fills.
/// </summary>
public class HoldWorkStation : Station
{
    [SerializeField] private ProcessStep step = ProcessStep.StainRemoval;
    [SerializeField] private float duration = 2f;
    private Slot slot;
    private WorldText hint;

    public override bool KidCanReach => true;

    public void Init(ProcessStep workStep, float workDuration)
    {
        step = workStep;
        duration = workDuration;
        slot = AddSlot(new Vector2(0f, 0.05f), new Vector2(0f, 0.85f));
        hint = WorldText.Create(transform, new Vector2(0f, 1.12f), "", 0.24f, Color.white, 31);
    }

    private bool HasValidCloth => !slot.IsEmpty && slot.Cloth != null && !slot.Cloth.IsBroken;

    public override bool WantsWork(PlayerController player)
    {
        if (!HasValidCloth) return false;
        if (step == ProcessStep.StainRemoval) return slot.Cloth.IsStained;
        return !slot.Cloth.IsFinished;
    }

    public override void Interact(PlayerController player)
    {
        if (player.IsHolding && !slot.IsEmpty)
        {
            HUD.Toast($"The {title} is occupied.", HUD.Warn);
            return;
        }
        DefaultPutOrTake(player);
    }

    public override void WorkPress(PlayerController player)
    {
        if (HasValidCloth && step == ProcessStep.StainRemoval && !slot.Cloth.IsStained)
        {
            HUD.Toast("There is no stain on this one.", HUD.Info);
        }
    }

    public override void WorkHold(PlayerController player, float dt)
    {
        if (!WantsWork(player)) return;
        slot.Progress += dt / duration;
        if (slot.Progress >= 1f)
        {
            slot.Progress = 0f;
            var cloth = slot.Cloth;
            bool ok = cloth.TryProcess(step);
            if (ok)
            {
                if (step == ProcessStep.StainRemoval) HUD.Toast("Stain removed!", HUD.Good);
                else if (cloth.IsFinished) HUD.Toast($"{cloth.DisplayName} is packed! Deliver it at the counter.", HUD.Good);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (slot == null) return;
        bool work = WantsWork(null);
        slot.Bar.Show(work && slot.Progress > 0f);
        slot.Bar.SetValue(slot.Progress, new Color(1f, 0.8f, 0.3f));
        hint.SetText(work ? "Hold E / Mouse" : "");
    }
}
