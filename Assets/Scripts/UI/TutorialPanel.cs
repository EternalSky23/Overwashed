using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新手教學: a full-screen, page-by-page tutorial (Prev / Next / Skip).
/// Keyboard: Right/D/Enter = next, Left/A = previous, Esc = close.
/// Used from the main menu (TUTORIAL button), at the first game start, and from the pause menu.
/// While open inside the game, time is paused.
/// </summary>
public class TutorialPanel : MonoBehaviour
{
    private const string SeenKey = "Overwashed.TutorialSeen";

    private class Page
    {
        public string Title;
        public string Body;
        public string[] Icons;
        public string[] IconCaptions;
        public bool FlowChart; // draw the cloth flow chart instead of icons + text
    }

    private static readonly List<Page> Pages = new List<Page>
    {
        new Page
        {
            Title = "Welcome to OVERWASHED!",
            Body = "You run a busy laundromat.\n\nCustomers drop off clothes at the <b>Counter</b>. " +
                   "Wash each piece the <b>right way</b> and return it before the customer runs out of patience.\n\n" +
                   "Earn as much <color=#FFE066>cash</color> and <b>score</b> as you can in <b>10 minutes</b>!",
            Icons = new[] { "player", "counter", "washer" },
            IconCaptions = new[] { "You", "Counter", "Machines" },
        },
        new Page
        {
            Title = "Controls",
            Body = "<b>WASD / Arrow keys</b> : Move\n" +
                   "<b>Space</b> : Pick up / Put down / Use the station in front of you\n" +
                   "      (you can carry <b>one</b> item at a time)\n" +
                   "<b>E</b> or <b>Left Click</b> : Work on a station (hold or mash)\n" +
                   "      Clicking a station next to you also works like Space.\n" +
                   "<b>Esc</b> : Pause",
            Icons = new[] { "player" },
            IconCaptions = new[] { "Walk up to a station" },
        },
        new Page
        {
            Title = "Orders",
            Body = "New orders appear at the <b>Counter</b> (one piece of cloth each).\n\n" +
                   "The <b>order cards</b> at the top show every step:\n" +
                   "   <color=#3A9A4A>✓ done</color>   →   <color=#FF7050><b>next step</b></color>   →   <color=#9A9AA5>later</color>\n\n" +
                   "The bar under each card is the customer's patience.\n" +
                   "When a cloth is <b>folded & packed</b>, bring it back to the Counter to get paid.",
            Icons = new[] { "counter", "cloth_tshirt", "cloth_tshirt_folded" },
            IconCaptions = new[] { "Counter", "New order", "Ready!" },
        },
        new Page
        {
            Title = "Recipes",
            Body = "Every type of cloth has its own procedure:\n\n" +
                   "<b>T-shirt</b> :  Washer → Dryer → Fold\n" +
                   "<b>Shirt</b> :  Washer → Drying Rack → Iron → Fold\n" +
                   "<b>Sweater</b> :  Hand Wash → Drying Rack → Fold\n" +
                   "<b>Suit</b> :  Dry Cleaner → Iron → Fold\n\n" +
                   "<color=#FF7050><b>One wrong step ruins the cloth!</b></color>  (e.g. a sweater in the dryer shrinks)",
            Icons = new[] { "cloth_tshirt", "cloth_shirt", "cloth_sweater", "cloth_suit" },
            IconCaptions = new[] { "T-shirt", "Shirt", "Sweater", "Suit" },
        },
        new Page
        {
            Title = "Stains & Ruined Clothes",
            Body = "Some clothes arrive with a <color=#C08040><b>STAIN</b></color>.\n" +
                   "Put them on the <b>Stain Remover</b> and <b>hold E / mouse</b> first — washing a stained cloth sets the stain forever.\n\n" +
                   "A ruined cloth costs you <color=#FF7050>compensation</color> and the order is cancelled.\n" +
                   "Throw ruined clothes in the <b>Trash</b> to free your hands.",
            Icons = new[] { "stainstation", "stain", "trash" },
            IconCaptions = new[] { "Stain Remover", "Stain", "Trash" },
        },
        new Page
        {
            Title = "Machines",
            Body = "<b>Washer / Dryer / Dry Cleaner</b> start by themselves when you put a cloth in.\n\n" +
                   "When one shows <color=#7CF080><b>DONE!</b></color>, take the cloth out quickly —\n" +
                   "after about <b>20 seconds</b> it gets moldy or overheated and is ruined!\n\n" +
                   "The <b>Drying Rack</b> holds 2 pieces and never over-dries, but it is slow.",
            Icons = new[] { "washer", "dryer", "drycleaner", "rack" },
            IconCaptions = new[] { "Washer", "Dryer", "Dry Cleaner", "Drying Rack" },
        },
        new Page
        {
            Title = "Hand Work",
            Body = "<b>Hand Wash</b> : <b>mash</b> E or click the mouse to scrub.\n\n" +
                   "<b>Ironing Board</b> : <b>hold</b> E / mouse and <b>release inside the green zone</b>.\n" +
                   "      Too early = try again.   Too late = burnt!\n\n" +
                   "<b>Fold & Pack</b> : hold E / mouse for a moment — the last step of every cloth.",
            Icons = new[] { "handwash", "iron", "fold" },
            IconCaptions = new[] { "Hand Wash", "Ironing", "Fold & Pack" },
        },
        new Page
        {
            Title = "Random Events",
            Body = "<b>Naughty kid</b> : smears a cloth lying around (not inside machines).\n" +
                   "      That cloth needs stain removal and starts its procedure over!\n" +
                   "      Walk next to the kid and press <b>Space</b> (or click them) to shoo them away.\n\n" +
                   "<b>Upgrade kit</b> : appears on the floor. Pick it up and use it on a machine\n" +
                   "      to make that machine <color=#FFE066><b>2x faster</b></color> for the rest of the day.",
            Icons = new[] { "kid", "kit", "bolt" },
            IconCaptions = new[] { "Kid", "Upgrade kit", "2x speed" },
        },
        new Page
        {
            Title = "Ready?",
            Body = "Plan ahead: keep machines busy, empty them on time, and deliver before the bars run out.\n\n" +
                   "You can reopen this tutorial from the <b>Pause</b> menu (Esc) or the main menu.\n\n" +
                   "<b>Good luck — don't get OVERWASHED!</b>",
            Icons = new[] { "cloth_tshirt_folded", "cloth_shirt_folded", "cloth_sweater_folded", "cloth_suit_folded" },
            IconCaptions = new[] { "", "", "", "" },
        },
        new Page
        {
            Title = "Cloth Flow Chart",
            FlowChart = true,
        },
    };

    public static TutorialPanel Current { get; private set; }
    public static bool IsOpen => Current != null && Current.gameObject.activeSelf;

    public static bool HasBeenSeen
    {
        get
        {
            try { return PlayerPrefs.GetInt(SeenKey, 0) == 1; }
            catch { return false; }
        }
    }

    private Text titleText;
    private Text bodyText;
    private Text pageText;
    private Text nextLabel;
    private Button prevButton;
    private readonly List<Image> iconImages = new List<Image>();
    private readonly List<Text> iconCaptions = new List<Text>();
    private RectTransform iconRow;
    private RectTransform flowChart;
    private int page;
    private bool pauseGame;
    private float previousTimeScale = 1f;
    private Action onClosed;

    /// <summary>Create and open the tutorial on top of everything.</summary>
    public static TutorialPanel Open(bool pauseGame, Action onClosed = null)
    {
        if (Current != null) Destroy(Current.gameObject);

        var canvas = UIFactory.CreateCanvas("Tutorial Canvas", 100);
        var panel = canvas.gameObject.AddComponent<TutorialPanel>();
        panel.pauseGame = pauseGame;
        panel.onClosed = onClosed;
        panel.Build(canvas.transform);
        Current = panel;

        if (pauseGame)
        {
            panel.previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }
        panel.ShowPage(0);
        return panel;
    }

    private void Build(Transform root)
    {
        UIFactory.Stretch(root, "Dim", new Color(0.03f, 0.02f, 0.06f, 0.88f), true);

        var card = UIFactory.Panel(root, "Card", new Color(0.16f, 0.14f, 0.24f, 0.98f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 860), true);
        var t = card.transform;

        // header strip
        UIFactory.Panel(t, "Header", new Color(0.35f, 0.6f, 0.85f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 12));

        UIFactory.Label(t, "Tag", "TUTORIAL", 26, new Color(0.6f, 0.85f, 1f), TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -34), new Vector2(400, 40));
        pageText = UIFactory.Label(t, "Page", "", 26, new Color(0.75f, 0.75f, 0.85f), TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -34), new Vector2(300, 40));
        titleText = UIFactory.Label(t, "Title", "", 64, new Color(1f, 0.9f, 0.45f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1300, 90));

        iconRow = UIFactory.Rect(t, "Icons", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -175), new Vector2(1300, 170));
        for (int i = 0; i < 4; i++)
        {
            var img = UIFactory.Panel(iconRow, "Icon" + i, Color.white, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                Vector2.zero, new Vector2(120, 120));
            img.preserveAspect = true;
            iconImages.Add(img);
            var cap = UIFactory.Label(iconRow, "Caption" + i, "", 24, new Color(0.85f, 0.88f, 1f), TextAnchor.UpperCenter,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(240, 36));
            iconCaptions.Add(cap);
        }

        bodyText = UIFactory.Label(t, "Body", "", 34, Color.white, TextAnchor.UpperLeft,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -365), new Vector2(1240, 335));
        bodyText.lineSpacing = 1.05f;
        // shrink long pages so text never runs under the buttons
        bodyText.verticalOverflow = VerticalWrapMode.Truncate;
        bodyText.resizeTextForBestFit = true;
        bodyText.resizeTextMinSize = 20;
        bodyText.resizeTextMaxSize = 34;

        flowChart = BuildFlowChart(t);
        flowChart.gameObject.SetActive(false);

        // navigation
        prevButton = UIFactory.Button(t, "< Prev", new Vector2(-450, -340), new Vector2(240, 80), new Color(0.35f, 0.35f, 0.45f), Prev);
        UIFactory.Button(t, "Skip", new Vector2(0, -340), new Vector2(200, 70), new Color(0.3f, 0.28f, 0.36f), Close);
        var next = UIFactory.Button(t, "Next >", new Vector2(450, -340), new Vector2(240, 80), new Color(0.25f, 0.6f, 0.35f), Next);
        nextLabel = next.GetComponentInChildren<Text>();

        UIFactory.Label(t, "KeysHint", "← / → or A / D to flip pages · Enter = next · Esc = close", 22, new Color(0.6f, 0.6f, 0.7f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(1000, 30));
    }

    // ---------------- Flow chart (last page) ----------------

    private static readonly Color ChipMachine = new Color(0.22f, 0.45f, 0.72f);
    private static readonly Color ChipHand = new Color(0.45f, 0.33f, 0.68f);
    private static readonly Color ChipRack = new Color(0.18f, 0.55f, 0.55f);
    private static readonly Color ChipStain = new Color(0.72f, 0.47f, 0.18f);
    private static readonly Color ChipGood = new Color(0.22f, 0.58f, 0.32f);
    private static readonly Color ChipBad = new Color(0.75f, 0.25f, 0.25f);
    private static readonly Color ChipGray = new Color(0.35f, 0.35f, 0.42f);
    private static readonly Color ChipWarn = new Color(0.72f, 0.6f, 0.15f);

    private static Color StepColor(ProcessStep step)
    {
        switch (step)
        {
            case ProcessStep.MachineWash:
            case ProcessStep.MachineDry:
            case ProcessStep.DryClean: return ChipMachine;
            case ProcessStep.AirDry: return ChipRack;
            case ProcessStep.StainRemoval: return ChipStain;
            default: return ChipHand; // hand wash, iron, fold
        }
    }

    private static string StepLabel(ProcessStep step)
    {
        switch (step)
        {
            case ProcessStep.MachineWash: return "Washer";
            case ProcessStep.MachineDry: return "Dryer";
            case ProcessStep.DryClean: return "Dry Cleaner";
            case ProcessStep.AirDry: return "Drying Rack";
            case ProcessStep.HandWash: return "Hand Wash";
            case ProcessStep.Iron: return "Iron";
            case ProcessStep.Fold: return "Fold & Pack";
            case ProcessStep.StainRemoval: return "Stain Remover";
        }
        return step.ToString();
    }

    private static void Chip(RectTransform parent, float x, float y, float w, float h, Color color, string text, int fontSize = 22)
    {
        var img = UIFactory.Panel(parent, "Chip", color, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, y), new Vector2(w, h));
        var label = UIFactory.Label(img.transform, "Text", text, fontSize, Color.white, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-8, 0));
        label.lineSpacing = 0.9f;
    }

    private static void Arrow(RectTransform parent, float x, float y, float h, Color color)
    {
        UIFactory.Label(parent, "Arrow", "→", 34, color, TextAnchor.MiddleCenter,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, y), new Vector2(34, h));
    }

    private static void Caption(RectTransform parent, float x, float y, float w, string text, int size, Color color)
    {
        UIFactory.Label(parent, "Caption", text, size, color, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, y), new Vector2(w, 34));
    }

    /// <summary>
    /// Every cloth: (Stain Remover if stained) -> its procedure -> Deliver,
    /// then what happens when something goes wrong. Built from the real procedures in the cloth classes.
    /// </summary>
    private static RectTransform BuildFlowChart(Transform card)
    {
        var root = UIFactory.Rect(card, "FlowChart", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -155), new Vector2(1320, 570));

        const float chipW = 138f, chipH = 50f, arrowW = 34f, rowH = 64f;

        Caption(root, 0, 0, 1320, "<b>Normal flow</b>  (left → right, one step at a time)", 24, new Color(0.6f, 0.85f, 1f));

        // procedures come straight from the cloth classes, so the chart never goes out of date
        // (each cloth needs its own GameObject: a cloth adds its own SpriteRenderer)
        var temp = new GameObject("FlowChartTemp");
        T Make<T>() where T : ClothBase
        {
            var go = new GameObject(typeof(T).Name);
            go.transform.SetParent(temp.transform, false);
            return go.AddComponent<T>();
        }
        var clothes = new ClothBase[] { Make<TShirt>(), Make<Shirt>(), Make<Sweater>(), Make<Suit>() };

        for (int r = 0; r < clothes.Length; r++)
        {
            var cloth = clothes[r];
            float y = -40f - r * rowH;
            var icon = UIFactory.Panel(root, "Icon", Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, y - 2), new Vector2(54, 54));
            icon.sprite = PixelArt.Get(PixelArt.ClothKey(cloth.Type));
            icon.preserveAspect = true;
            Caption(root, 60, y - 8, 120, $"<b>{cloth.DisplayName}</b>", 24, Color.white);

            float x = 180f;
            Chip(root, x, y, chipW, chipH, ChipStain, "Stain Remover\n<size=16>(only if stained)</size>", 19);
            x += chipW;
            foreach (var step in cloth.GetProcedure())
            {
                Arrow(root, x, y, chipH, Color.white);
                x += arrowW;
                Chip(root, x, y, chipW, chipH, StepColor(step), StepLabel(step));
                x += chipW;
            }
            Arrow(root, x, y, chipH, Color.white);
            x += arrowW;
            Chip(root, x, y, chipW, chipH, ChipGood, $"Deliver\n<size=17>+${cloth.Profit}</size>", 21);
        }
        Destroy(temp);

        // ---- exceptions ----
        float ey = -40f - clothes.Length * rowH - 14f;
        Caption(root, 0, ey, 1320, "<b>When something goes wrong</b>", 24, new Color(1f, 0.75f, 0.6f));

        float y1 = ey - 40f;
        Chip(root, 0, y1, 520, 64, ChipBad,
            "Wrong station  ·  washed with a stain  ·  machine not emptied\nwithin 20 s  ·  iron held too long  ·  customer waited too long", 18);
        Arrow(root, 520, y1, 64, new Color(1f, 0.5f, 0.45f));
        Chip(root, 554, y1, 170, 64, ChipBad, "<b>RUINED</b>\n<size=17>-compensation</size>", 22);
        Arrow(root, 724, y1, 64, new Color(1f, 0.5f, 0.45f));
        Chip(root, 758, y1, 130, 64, ChipGray, "Trash");
        Caption(root, 900, y1 - 15, 420, "order cancelled, hands free again", 21, new Color(0.85f, 0.85f, 0.9f));

        float y2 = y1 - 76f;
        Chip(root, 0, y2, 230, chipH, ChipWarn, "Kid smears it");
        Arrow(root, 230, y2, chipH, new Color(1f, 0.85f, 0.4f));
        Chip(root, 264, y2, 150, chipH, ChipStain, "<b>STAINED</b>");
        Arrow(root, 414, y2, chipH, new Color(1f, 0.85f, 0.4f));
        Chip(root, 448, y2, 170, chipH, ChipStain, "Stain Remover");
        Arrow(root, 618, y2, chipH, new Color(1f, 0.85f, 0.4f));
        Chip(root, 652, y2, 236, chipH, ChipGray, "Restart from step 1");
        Caption(root, 900, y2 - 8, 420, "progress is lost, but not ruined", 21, new Color(0.85f, 0.85f, 0.9f));

        float y3 = y2 - 62f;
        Chip(root, 0, y3, 230, chipH, ChipWarn, "Iron released early");
        Arrow(root, 230, y3, chipH, new Color(1f, 0.85f, 0.4f));
        Chip(root, 264, y3, 354, chipH, ChipHand, "Stay on Iron — try again");
        Caption(root, 640, y3 - 8, 680, "release inside the green zone to pass", 21, new Color(0.85f, 0.85f, 0.9f));

        return root;
    }

    private void ShowPage(int index)
    {
        page = Mathf.Clamp(index, 0, Pages.Count - 1);
        var p = Pages[page];
        titleText.text = p.Title;
        bodyText.text = p.Body ?? "";
        flowChart.gameObject.SetActive(p.FlowChart);
        iconRow.gameObject.SetActive(!p.FlowChart);
        bodyText.gameObject.SetActive(!p.FlowChart);
        pageText.text = $"{page + 1} / {Pages.Count}";
        prevButton.gameObject.SetActive(page > 0);
        nextLabel.text = page == Pages.Count - 1 ? "Let's go!" : "Next >";

        int count = p.Icons != null ? p.Icons.Length : 0;
        float spacing = 280f;
        float start = -(count - 1) * spacing * 0.5f;
        for (int i = 0; i < iconImages.Count; i++)
        {
            bool on = i < count;
            iconImages[i].gameObject.SetActive(on);
            iconCaptions[i].gameObject.SetActive(on);
            if (!on) continue;
            iconImages[i].sprite = PixelArt.Get(p.Icons[i]);
            iconImages[i].rectTransform.anchoredPosition = new Vector2(start + i * spacing, 0);
            iconCaptions[i].text = p.IconCaptions != null && i < p.IconCaptions.Length ? p.IconCaptions[i] : "";
            iconCaptions[i].rectTransform.anchoredPosition = new Vector2(start + i * spacing, -128);
        }
    }

    public void NextPage() => Next();
    public int PageCount => Pages.Count;

    private void Next()
    {
        if (page >= Pages.Count - 1) Close();
        else ShowPage(page + 1);
    }

    private void Prev()
    {
        ShowPage(page - 1);
    }

    public void Close()
    {
        // automated screenshot runs must not mark the tutorial as seen on this machine
        if (!DebugTools.HasArg("-autoshot"))
        {
            try
            {
                PlayerPrefs.SetInt(SeenKey, 1);
                PlayerPrefs.Save();
            }
            catch { /* not critical */ }
        }

        if (pauseGame) Time.timeScale = previousTimeScale;
        if (Current == this) Current = null;
        var callback = onClosed;
        Destroy(gameObject);
        callback?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Next();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (page > 0) Prev();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }
}
