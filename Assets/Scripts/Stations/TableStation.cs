using UnityEngine;

/// <summary>A plain table to temporarily put things on. Kids can reach it.</summary>
public class TableStation : Station
{
    public override bool KidCanReach => true;

    public void Init()
    {
        AddSlot(new Vector2(0f, 0.05f), new Vector2(0f, 0.8f));
    }

    protected override bool CanAccept(IHoldable item) => true;
}
