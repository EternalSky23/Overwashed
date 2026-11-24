using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 隨機事件:
///  1. Kid (every 45-70 s): runs to a reachable cloth and stains it (cloth restarts its procedure).
///  2. Upgrade kit (every 60-90 s): dropped on the floor; use it on a machine for 2x speed.
/// </summary>
public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager Instance { get; private set; }

    [SerializeField] private Vector2 kidInterval = new Vector2(45f, 70f);
    [SerializeField] private Vector2 kitInterval = new Vector2(60f, 90f);

    private Vector2 doorPosition;
    private Vector2[] kitSpawnPoints;
    private List<Station> stations;
    private float nextKid;
    private float nextKit;
    private readonly List<UpgradeKit> floorKits = new List<UpgradeKit>();

    /// <summary>Turn off automatic events (used by the scripted playtest).</summary>
    public bool AutoEvents { get; set; } = true;

    public void Init(Vector2 door, Vector2[] kitPoints, List<Station> allStations)
    {
        Instance = this;
        doorPosition = door;
        kitSpawnPoints = kitPoints;
        stations = allStations;
        nextKid = Random.Range(kidInterval.x, kidInterval.y);
        nextKit = Random.Range(kitInterval.x, kitInterval.y);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning || !AutoEvents) return;

        nextKid -= Time.deltaTime;
        if (nextKid <= 0f)
        {
            SpawnKid();
            nextKid = Random.Range(kidInterval.x, kidInterval.y);
        }

        nextKit -= Time.deltaTime;
        if (nextKit <= 0f)
        {
            SpawnKit();
            nextKit = Random.Range(kitInterval.x, kitInterval.y);
        }
    }

    public Kid SpawnKid()
    {
        var go = new GameObject("Kid");
        go.transform.position = doorPosition + Vector2.down * 0.6f;
        var kid = go.AddComponent<Kid>();
        kid.Init(doorPosition, stations);
        HUD.Toast("A kid ran into the shop! Shoo them away (Space / click)!", HUD.Warn);
        return kid;
    }

    public UpgradeKit SpawnKit()
    {
        floorKits.RemoveAll(k => k == null);
        if (floorKits.Count > 0) return null; // one kit on the floor at a time

        bool anyUpgradable = false;
        foreach (var s in stations)
        {
            if (s is MachineStation m && !m.IsUpgraded) anyUpgradable = true;
        }
        if (!anyUpgradable) return null;

        var go = new GameObject("UpgradeKit");
        go.transform.position = kitSpawnPoints[Random.Range(0, kitSpawnPoints.Length)];
        var kit = go.AddComponent<UpgradeKit>();
        floorKits.Add(kit);
        HUD.Toast("An upgrade kit appeared! Bring it to a machine for 2x speed.", HUD.Good);
        FloatingText.Spawn(go.transform.position + Vector3.up * 0.6f, "UPGRADE KIT!", HUD.Good);
        return kit;
    }

    public UpgradeKit FindFloorKit(Vector2 pos, float radius)
    {
        floorKits.RemoveAll(k => k == null);
        foreach (var k in floorKits)
        {
            if (k.OnFloor && Vector2.Distance(pos, k.transform.position) <= radius) return k;
        }
        return null;
    }

    public void PickUpKit(UpgradeKit kit)
    {
        floorKits.Remove(kit);
    }
}
