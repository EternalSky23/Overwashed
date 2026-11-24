using UnityEngine;

/// <summary>Throw away ruined clothes (or unwanted kits). Throwing away a good cloth ruins the order.</summary>
public class TrashStation : Station
{
    public override void Interact(PlayerController player)
    {
        if (!player.IsHolding)
        {
            HUD.Toast("Bring ruined clothes here to throw them away.", HUD.Info);
            return;
        }

        var item = player.Release();
        if (item is ClothBase cloth)
        {
            if (!cloth.IsBroken) cloth.Break($"{cloth.DisplayName} thrown away!");
            else HUD.Toast($"Threw away the ruined {cloth.DisplayName}.", HUD.Info);
        }
        Destroy(item.gameObject);
    }
}
