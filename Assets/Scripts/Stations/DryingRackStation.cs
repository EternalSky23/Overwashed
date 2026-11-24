using UnityEngine;

/// <summary>
/// Drying rack (air dry). Two slots, each dries on its own. No overdue: clothes can wait here,
/// but kids can reach them.
/// </summary>
public class DryingRackStation : Station
{
    [SerializeField] private float duration = 10f;

    public override bool KidCanReach => true;

    public void Init(float dryDuration)
    {
        duration = dryDuration;
        AddSlot(new Vector2(-0.6f, 0.05f), new Vector2(-0.6f, 0.8f), 0.9f);
        AddSlot(new Vector2(0.6f, 0.05f), new Vector2(0.6f, 0.8f), 0.9f);
    }

    protected override Slot PreferredTakeSlot()
    {
        // prefer taking a dried cloth first
        foreach (var s in slots) if (!s.IsEmpty && s.Done) return s;
        return FirstFilledSlot();
    }

    protected override void Update()
    {
        base.Update();
        foreach (var s in slots)
        {
            if (s.IsEmpty) continue;
            var cloth = s.Cloth;
            if (cloth == null || cloth.IsBroken)
            {
                s.Bar.Show(false);
                continue;
            }
            if (s.Done)
            {
                s.Bar.Show(true);
                s.Bar.SetValue(1f, HUD.Good);
                continue;
            }
            s.Progress += Time.deltaTime / duration;
            s.Bar.Show(true);
            s.Bar.SetValue(s.Progress, new Color(0.55f, 0.85f, 1f));
            if (s.Progress >= 1f)
            {
                s.Done = true;
                cloth.Dry(DryMethod.AirDry, null);
                if (!cloth.IsBroken) HUD.Toast($"{cloth.DisplayName} is dry on the rack.", HUD.Info);
            }
        }
    }

    protected override void OnPlaced(Slot slot)
    {
        // a stained / dirty cloth placed again restarts drying
        slot.ResetWork();
    }
}
