using System.Collections.Generic;
using UnityEngine;

/// <summary>One customer order = one piece of cloth to process and return.</summary>
public class Order
{
    public int Id;
    public ClothBase Cloth;
    public float TimeLeft;
    public float TotalTime;
    public bool Expired;
}

/// <summary>
/// 接單系統: spawns orders at the counter every 15-25 s (max 4 at once).
/// Deliver the finished cloth to the counter before the order timer runs out.
/// Late / ruined = compensation is paid (see ClothBase.ChangeState).
/// </summary>
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [SerializeField] private int maxOrders = 4;
    [SerializeField] private Vector2 spawnInterval = new Vector2(15f, 25f);
    [SerializeField] private float stainChance = 0.35f;
    [SerializeField] private float baseOrderTime = 60f;
    [SerializeField] private float timePerStep = 12f;

    public readonly List<Order> ActiveOrders = new List<Order>();

    /// <summary>Turn off automatic orders (used by the scripted playtest).</summary>
    public bool AutoSpawn { get; set; } = true;

    private CounterStation counter;
    private float nextSpawn = 1.5f;
    private int spawnedCount;
    private int nextId = 1;

    public void Init(CounterStation counterStation)
    {
        Instance = this;
        counter = counterStation;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning) return;

        nextSpawn -= Time.deltaTime;
        if (AutoSpawn && nextSpawn <= 0f && ActiveOrders.Count < maxOrders && counter.HasFreeSlot())
        {
            SpawnOrder();
            // the second order comes a bit faster so the start is not boring
            nextSpawn = spawnedCount == 1 ? 8f : Random.Range(spawnInterval.x, spawnInterval.y);
        }

        for (int i = ActiveOrders.Count - 1; i >= 0; i--)
        {
            var o = ActiveOrders[i];
            o.TimeLeft -= Time.deltaTime;
            if (o.TimeLeft <= 0f)
            {
                o.Expired = true;
                if (o.Cloth != null) o.Cloth.Break($"Customer gave up waiting for the {o.Cloth.DisplayName}!");
                else ActiveOrders.Remove(o);
            }
        }
    }

    public Order SpawnOrder(ClothType? forcedType = null, bool? forcedStain = null, Station placeAt = null)
    {
        var type = forcedType ?? (ClothType)Random.Range(0, 4);
        var go = new GameObject("Cloth_" + type);
        ClothBase cloth;
        switch (type)
        {
            case ClothType.Shirt: cloth = go.AddComponent<Shirt>(); break;
            case ClothType.Sweater: cloth = go.AddComponent<Sweater>(); break;
            case ClothType.Suit: cloth = go.AddComponent<Suit>(); break;
            default: cloth = go.AddComponent<TShirt>(); break;
        }

        bool stained = forcedStain ?? (type != ClothType.Sweater && Random.value < stainChance);
        cloth.SetStained(stained);

        int steps = cloth.GetProcedure().Length + (stained ? 1 : 0);
        var order = new Order
        {
            Id = nextId++,
            Cloth = cloth,
            TotalTime = baseOrderTime + timePerStep * steps,
        };
        order.TimeLeft = order.TotalTime;
        cloth.Order = order;
        cloth.OnBroken += HandleBroken;

        ActiveOrders.Add(order);
        spawnedCount++;
        var station = placeAt != null ? placeAt : counter;
        if (station != null) station.ForcePlace(cloth);
        HUD.Toast($"New order: {cloth.DisplayName}{(stained ? " (stained)" : "")}", HUD.Info);
        return order;
    }

    public void Deliver(ClothBase cloth, Vector3 at)
    {
        var order = cloth.Order;
        if (order != null && ActiveOrders.Contains(order))
        {
            cloth.Deliver();
            ActiveOrders.Remove(order);
            HUD.Toast($"{cloth.DisplayName} delivered! +${cloth.Profit}", HUD.Good);
            FloatingText.Spawn(at + Vector3.up * 0.5f, $"+${cloth.Profit}", HUD.Good);
        }
        else
        {
            HUD.Toast("No customer is waiting for this anymore.", HUD.Warn);
        }
        Destroy(cloth.gameObject);
    }

    private void HandleBroken(ClothBase cloth)
    {
        var order = cloth.Order;
        if (order != null) ActiveOrders.Remove(order);
        var gm = GameManager.Instance;
        if (gm != null)
        {
            if (order != null && order.Expired) gm.OrdersExpired++;
            else gm.ClothesRuined++;
        }
        HUD.Toast(cloth.BrokenReason + $"  -${cloth.Compensate}", HUD.Bad);
        FloatingText.Spawn(cloth.transform.position + Vector3.up * 0.5f, $"-${cloth.Compensate}", HUD.Bad);
    }
}
