using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Builds the whole MainGame level in code: floor, walls, stations, player, managers and HUD.
/// Positions / timings are serialized so they can be tweaked in the Inspector.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Timings (seconds)")]
    [SerializeField] private float washerTime = 8f;
    [SerializeField] private float dryerTime = 8f;
    [SerializeField] private float dryCleanTime = 10f;
    [SerializeField] private float airDryTime = 10f;
    [SerializeField] private float stainRemoveTime = 2f;
    [SerializeField] private float foldTime = 0.7f;

    [Header("Third-party art (optional, see README)")]
    [Tooltip("Use downloaded pixel art from Assets/ThirdParty/Resources/Art when it exists.")]
    [SerializeField] private bool useThirdPartyArt = true;
    [Tooltip("Townsfolk character used for the player (Baker, Waiter, Woman, Teen, ...).")]
    [SerializeField] private string playerCharacter = "Baker";

    [Header("Tutorial")]
    [Tooltip("Show the tutorial at every round start (otherwise only the first time).")]
    [SerializeField] private bool alwaysShowTutorial = false;

    [Header("Layout")]
    [SerializeField] private Vector2 playerStart = new Vector2(0f, -3.5f);
    [SerializeField] private Vector2 doorPosition = new Vector2(-7.2f, -5.3f);

    // References (handy for debugging / AutoShot)
    public CounterStation Counter { get; private set; }
    public List<Station> Stations { get; } = new List<Station>();
    public MachineStation WasherA { get; private set; }
    public MachineStation WasherB { get; private set; }
    public MachineStation Dryer { get; private set; }
    public MachineStation DryCleaner { get; private set; }
    public HandWashStation HandWash { get; private set; }
    public DryingRackStation Rack { get; private set; }
    public HoldWorkStation StainStation { get; private set; }
    public IronStation IronBoard { get; private set; }
    public HoldWorkStation FoldTable { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        ThirdPartyArt.Enabled = useThirdPartyArt;
        PlayerController.CharacterName = playerCharacter;
        SetupCamera();
        SetupLight();
        BuildRoom();
        BuildStations();

        var player = new GameObject("Player");
        player.transform.position = playerStart;
        player.AddComponent<PlayerController>();

        var hud = new GameObject("HUD").AddComponent<HUD>();
        hud.Build();

        var orders = new GameObject("OrderManager").AddComponent<OrderManager>();
        orders.Init(Counter);

        var events = new GameObject("RandomEventManager").AddComponent<RandomEventManager>();
        events.Init(doorPosition, new[]
        {
            new Vector2(-5f, -3.8f), new Vector2(-2f, -3.6f), new Vector2(2.5f, -3.8f),
            new Vector2(5f, -1.6f), new Vector2(3.5f, 0.4f), new Vector2(-6f, 0.2f),
        }, Stations);

        if (DebugTools.HasArg("-selftest")) DebugTools.RunSelfTest();

        GameManager.Instance.ResetGame();
        AudioManager.PlayBgm(AudioManager.Bgm.Game);

        var debug = gameObject.AddComponent<DebugTools>(); // cheat keys F1-F4 in development builds
        if (DebugTools.HasArg("-autoshot")) debug.StartGameShots(this);
        if (DebugTools.HasArg("-playtest")) debug.StartPlaytest(this);

        // 新手教學: show automatically the first time a round is started (time paused until closed)
        bool automated = DebugTools.HasArg("-autoshot") || DebugTools.HasArg("-playtest");
        if (!automated && (alwaysShowTutorial || !TutorialPanel.HasBeenSeen))
        {
            TutorialPanel.Open(true);
        }
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.orthographic = true;
        // size / position are computed so the shop fits between the HUD bands at any aspect ratio
        if (cam.GetComponent<CameraFit>() == null) cam.gameObject.AddComponent<CameraFit>();
        cam.backgroundColor = new Color(0.12f, 0.1f, 0.16f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetupLight()
    {
        // URP 2D renderer: sprites need a global light to be visible
        if (FindObjectOfType<Light2D>() != null) return;
        var go = new GameObject("Global Light 2D");
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color = Color.white;
    }

    private void BuildRoom()
    {
        var floor = new GameObject("Floor").AddComponent<SpriteRenderer>();
        floor.sprite = PixelArt.Get("floor");
        floor.sortingOrder = -100;
        floor.transform.position = new Vector3(0f, -1f, 0f);

        var wall = new GameObject("Wall").AddComponent<SpriteRenderer>();
        wall.sprite = PixelArt.Get("wall");
        wall.sortingOrder = -99;
        wall.transform.position = new Vector3(0f, 4f, 0f);

        // door mat
        var door = new GameObject("Door").AddComponent<SpriteRenderer>();
        door.sprite = PixelArt.Get("white");
        door.color = new Color(0.45f, 0.3f, 0.25f);
        door.sortingOrder = -98;
        door.transform.position = doorPosition;
        door.transform.localScale = new Vector3(1.8f, 0.5f, 1f);
        var doorLabel = WorldText.Create(null, Vector2.zero, "ENTRANCE", 0.28f, new Color(1f, 0.95f, 0.8f), 30);
        doorLabel.transform.position = doorPosition + new Vector2(0f, 0.45f);

        // invisible walls
        MakeWall(new Vector2(0f, 4.0f), new Vector2(22f, 1f));
        MakeWall(new Vector2(0f, -6.0f), new Vector2(22f, 1f));
        MakeWall(new Vector2(-10.5f, -1f), new Vector2(1f, 12f));
        MakeWall(new Vector2(10.5f, -1f), new Vector2(1f, 12f));
    }

    private static void MakeWall(Vector2 pos, Vector2 size)
    {
        var go = new GameObject("WallCollider");
        go.transform.position = pos;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    private T NewStation<T>(string name, Vector2 pos) where T : Station
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var st = go.AddComponent<T>();
        Stations.Add(st);
        return st;
    }

    private MachineStation NewMachine(string title, string sprite, MachineKind kind, float time, Vector2 pos)
    {
        var m = NewStation<MachineStation>(title, pos);
        m.Setup(title, sprite, new Vector2(1.4f, 1.4f));
        m.Init(kind, time);
        return m;
    }

    private void BuildStations()
    {
        // ---- top row (against the wall) ----
        WasherA = NewMachine("Washer", "washer", MachineKind.Washer, washerTime, new Vector2(-7.4f, 2.5f));
        WasherB = NewMachine("Washer", "washer", MachineKind.Washer, washerTime, new Vector2(-5.4f, 2.5f));
        Dryer = NewMachine("Dryer", "dryer", MachineKind.Dryer, dryerTime, new Vector2(-3.4f, 2.5f));
        DryCleaner = NewMachine("Dry Cleaner", "drycleaner", MachineKind.DryCleaner, dryCleanTime, new Vector2(-1.4f, 2.5f));

        HandWash = NewStation<HandWashStation>("HandWash", new Vector2(0.9f, 2.5f));
        HandWash.Setup("Hand Wash", "handwash", new Vector2(1.5f, 1.4f));
        HandWash.Init();

        Rack = NewStation<DryingRackStation>("DryingRack", new Vector2(3.8f, 2.6f));
        Rack.Setup("Drying Rack", "rack", new Vector2(2.6f, 1.2f));
        Rack.Init(airDryTime);

        // ---- right column ----
        StainStation = NewStation<HoldWorkStation>("StainRemover", new Vector2(8.9f, 2.1f));
        StainStation.Setup("Stain Remover", "stainstation", new Vector2(1.4f, 1.4f));
        StainStation.Init(ProcessStep.StainRemoval, stainRemoveTime);

        IronBoard = NewStation<IronStation>("IronBoard", new Vector2(8.7f, -0.55f));
        IronBoard.Setup("Ironing Board", "iron", new Vector2(1.8f, 1.2f));
        IronBoard.Init();

        FoldTable = NewStation<HoldWorkStation>("FoldTable", new Vector2(8.9f, -3.4f));
        FoldTable.Setup("Fold & Pack", "fold", new Vector2(1.4f, 1.4f));
        FoldTable.Init(ProcessStep.Fold, foldTime);

        // ---- left: counter ----
        Counter = NewStation<CounterStation>("Counter", new Vector2(-9.2f, -0.9f));
        Counter.Setup("Counter", "counter", new Vector2(1.3f, 4.3f));
        Counter.Init(4, 1.0f);

        // ---- middle: tables ----
        var t1 = NewStation<TableStation>("Table", new Vector2(-3.0f, -1.2f));
        t1.Setup("Table", "table", new Vector2(1.2f, 1.2f));
        t1.Init();
        var t2 = NewStation<TableStation>("Table", new Vector2(1.0f, -1.2f));
        t2.Setup("Table", "table", new Vector2(1.2f, 1.2f));
        t2.Init();

        // ---- trash ----
        var trash = NewStation<TrashStation>("Trash", new Vector2(5.8f, -3.9f));
        trash.Setup("Trash", "trash", new Vector2(1f, 1.1f));
    }
}
