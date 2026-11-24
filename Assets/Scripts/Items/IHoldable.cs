using UnityEngine;

/// <summary>
/// Anything the player can carry (clothes, upgrade kits). The player carries one at a time.
/// </summary>
public interface IHoldable
{
    Transform transform { get; }
    GameObject gameObject { get; }

    /// <summary>The station this item sits on, null when held or lying on the floor.</summary>
    Station CurrentStation { get; set; }

    void AttachTo(Transform anchor, int sortingOrder);
    void Detach();
}
