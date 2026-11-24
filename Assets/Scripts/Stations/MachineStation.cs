using UnityEngine;

public enum MachineKind
{
    Washer,
    Dryer,
    DryCleaner,
}

/// <summary>
/// Washer / dryer / dry cleaner. Put a cloth in and it starts automatically.
/// When finished, take the cloth out within <see cref="overdueLimit"/> seconds or it gets ruined.
/// Can be upgraded once with an UpgradeKit (2x speed).
/// </summary>
public class MachineStation : Station
{
    [SerializeField] private MachineKind kind;
    [SerializeField] private float duration = 8f;
    [SerializeField] private float overdueLimit = 20f;
    [SerializeField] private float speedMultiplier = 1f;

    public MachineKind Kind => kind;
    public bool IsUpgraded => speedMultiplier > 1f;

    private Slot slot;
    private WorldText upgradeMark;
    private WorldText statusText;
    private Vector3 basePos;

    public void Init(MachineKind machineKind, float machineDuration)
    {
        kind = machineKind;
        duration = machineDuration;
        slot = AddSlot(new Vector2(0f, -0.1f), new Vector2(0f, 0.95f));
        upgradeMark = WorldText.Create(transform, new Vector2(0.62f, 0.55f), "", 0.3f, new Color(1f, 0.85f, 0.1f), 31);
        statusText = WorldText.Create(transform, new Vector2(0f, 1.3f), "", 0.26f, Color.white, 31);
        basePos = transform.localPosition;
    }

    public override void Interact(PlayerController player)
    {
        if (player.Held is UpgradeKit kit)
        {
            if (IsUpgraded)
            {
                HUD.Toast($"The {title} is already upgraded.", HUD.Warn);
                return;
            }
            player.Release();
            Destroy(kit.gameObject);
            speedMultiplier = 2f;
            upgradeMark.SetText("x2");
            AudioManager.Play(AudioManager.Sfx.Upgrade);
            var bolt = new GameObject("UpgradeBolt").AddComponent<SpriteRenderer>();
            bolt.sprite = PixelArt.Get("bolt");
            bolt.sortingOrder = 31;
            bolt.transform.SetParent(transform, false);
            bolt.transform.localPosition = new Vector3(0.3f, 0.55f, 0f);
            HUD.Toast($"{title} upgraded! 2x speed", HUD.Good);
            FloatingText.Spawn(transform.position + Vector3.up, "UPGRADED!", HUD.Good);
            return;
        }

        if (player.IsHolding)
        {
            if (!slot.IsEmpty)
            {
                HUD.Toast($"The {title} is busy.", HUD.Warn);
                return;
            }
            Place(player.Release(), slot);
            return;
        }

        if (!slot.IsEmpty)
        {
            if (!slot.Done) HUD.Toast($"Stopped the {title} early.", HUD.Warn);
            player.Hold(Take(slot));
        }
    }

    protected override void OnPlaced(Slot s)
    {
        AudioManager.Play(AudioManager.Sfx.MachineStart, 0.6f, 1.5f);
        // the cloth is "inside" the machine: draw it small behind the door glass
        s.Item.transform.localScale = Vector3.one * 0.55f;
        if (s.Cloth != null) s.Cloth.SetSorting(2);
    }

    protected override void OnTaken(Slot s)
    {
        statusText.SetText("");
        transform.localPosition = basePos;
    }

    protected override void Update()
    {
        base.Update();
        if (slot == null) return;

        if (slot.IsEmpty)
        {
            transform.localPosition = basePos;
            statusText.SetText("");
            return;
        }

        var cloth = slot.Cloth;
        if (!slot.Done)
        {
            slot.Progress += Time.deltaTime * speedMultiplier / duration;
            slot.Bar.Show(true);
            slot.Bar.SetValue(slot.Progress, new Color(0.3f, 0.7f, 1f));
            // shake while running
            transform.localPosition = basePos + (Vector3)(Random.insideUnitCircle * 0.03f);
            statusText.SetText("");

            if (slot.Progress >= 1f)
            {
                slot.Done = true;
                transform.localPosition = basePos;
                ApplyStep(cloth);
                if (cloth != null && !cloth.IsBroken) AudioManager.Play(AudioManager.Sfx.MachineDone, 0.7f, 2.5f);
            }
            return;
        }

        // Done: waiting to be taken out
        if (cloth == null || cloth.IsBroken)
        {
            slot.Bar.Show(false);
            statusText.SetText(cloth != null ? "RUINED" : "");
            statusText.SetColor(HUD.Bad);
            return;
        }

        slot.Overdue += Time.deltaTime;
        float left = overdueLimit - slot.Overdue;
        slot.Bar.Show(true);
        slot.Bar.SetValue(left / overdueLimit, left < 7f ? HUD.Bad : HUD.Good);
        bool blink = left < 7f && Mathf.Repeat(Time.time, 0.5f) < 0.25f;
        statusText.SetText(blink ? "" : "DONE!");
        statusText.SetColor(left < 7f ? HUD.Bad : HUD.Good);

        if (left <= 0f)
        {
            cloth.Break(OverdueReason(cloth));
        }
    }

    private void ApplyStep(ClothBase cloth)
    {
        if (cloth == null || cloth.IsBroken) return;
        switch (kind)
        {
            case MachineKind.Washer: cloth.Wash(WashMethod.MachineWash, null); break;
            case MachineKind.DryCleaner: cloth.Wash(WashMethod.DryClean, null); break;
            case MachineKind.Dryer: cloth.Dry(DryMethod.MachineDry, null); break;
        }
        if (!cloth.IsBroken)
        {
            HUD.Toast($"{cloth.DisplayName} is done in the {title}! Take it out soon.", HUD.Info);
        }
    }

    private string OverdueReason(ClothBase cloth)
    {
        switch (kind)
        {
            case MachineKind.Dryer: return $"{cloth.DisplayName} overheated in the dryer!";
            case MachineKind.DryCleaner: return $"{cloth.DisplayName} left too long in the dry cleaner!";
            default: return $"{cloth.DisplayName} left wet in the washer - it went moldy!";
        }
    }
}
