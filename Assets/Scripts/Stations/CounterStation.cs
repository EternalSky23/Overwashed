using UnityEngine;

/// <summary>
/// Counter: new orders arrive here, finished clothes are delivered here.
/// </summary>
public class CounterStation : Station
{
    public override bool KidCanReach => true;

    public void Init(int slotCount, float spacing)
    {
        float top = (slotCount - 1) * spacing * 0.5f;
        for (int i = 0; i < slotCount; i++)
        {
            float y = top - i * spacing;
            AddSlot(new Vector2(0.05f, y), new Vector2(0.05f, y + 0.45f), 0.8f);
        }
    }

    public override void Interact(PlayerController player)
    {
        if (player.Held is ClothBase cloth)
        {
            if (cloth.IsBroken)
            {
                HUD.Toast("This one is ruined - throw it in the trash.", HUD.Warn);
                return;
            }
            if (cloth.IsFinished)
            {
                player.Release();
                OrderManager.Instance.Deliver(cloth, transform.position);
                return;
            }
        }
        DefaultPutOrTake(player);
    }

    protected override Slot PreferredTakeSlot()
    {
        // take the most urgent order first
        Slot best = null;
        float bestTime = float.MaxValue;
        foreach (var s in slots)
        {
            if (s.IsEmpty) continue;
            float t = s.Cloth != null && s.Cloth.Order != null ? s.Cloth.Order.TimeLeft : float.MaxValue - 1f;
            if (t < bestTime)
            {
                bestTime = t;
                best = s;
            }
        }
        return best;
    }
}
