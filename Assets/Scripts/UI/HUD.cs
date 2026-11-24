using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// In-game GUI: timer / cash / score, order cards, toasts, controls hint, pause and result panels.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    public static readonly Color Good = new Color(0.45f, 0.95f, 0.5f);
    public static readonly Color Bad = new Color(1f, 0.42f, 0.38f);
    public static readonly Color Warn = new Color(1f, 0.8f, 0.35f);
    public static readonly Color Info = new Color(0.85f, 0.92f, 1f);

    private const int MaxCards = 4;

    private Text timeText;
    private Text cashText;
    private Text scoreText;
    private Text statsText;

    private class Card
    {
        public Image Bg;
        public Image Icon;
        public Text Title;
        public Text Steps;
        public RectTransform TimeFill;
        public Image TimeFillImg;
    }
    private readonly List<Card> cards = new List<Card>();

    private class Toasty
    {
        public Text Text;
        public float Life;
    }
    private RectTransform toastRoot;
    private readonly List<Toasty> toasts = new List<Toasty>();

    private GameObject pausePanel;
    private GameObject resultPanel;
    private Text resultText;
    private bool resultShown;

    public static void Toast(string message, Color color)
    {
        if (Instance != null) Instance.AddToast(message, color);
        else Debug.Log("[Toast] " + message);
    }

    public void Build()
    {
        Instance = this;
        var canvas = UIFactory.CreateCanvas("HUD Canvas", 10);
        var root = canvas.transform;

        // ---- Top-left stats ----
        var statPanel = UIFactory.Panel(root, "Stats", new Color(0.1f, 0.08f, 0.14f, 0.85f),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -14), new Vector2(300, 190));
        timeText = UIFactory.Label(statPanel.transform, "Time", "10:00", 56, Color.white, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(20, -8), new Vector2(-20, 70));
        cashText = UIFactory.Label(statPanel.transform, "Cash", "$1000", 38, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(20, -80), new Vector2(-20, 48));
        scoreText = UIFactory.Label(statPanel.transform, "Score", "Score 0", 32, Info, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(20, -130), new Vector2(-20, 44));

        // ---- Top-right stats ----
        var rightPanel = UIFactory.Panel(root, "RightStats", new Color(0.1f, 0.08f, 0.14f, 0.85f),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -14), new Vector2(250, 190));
        statsText = UIFactory.Label(rightPanel.transform, "Stats", "", 28, Color.white, TextAnchor.UpperLeft,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(8, -6), new Vector2(-30, -24));

        // ---- Order cards ----
        float cardW = 300, cardH = 190, gap = 14;
        float totalW = MaxCards * cardW + (MaxCards - 1) * gap;
        for (int i = 0; i < MaxCards; i++)
        {
            float x = -totalW / 2 + cardW / 2 + i * (cardW + gap);
            var bg = UIFactory.Panel(root, "Order" + i, new Color(0.97f, 0.95f, 0.88f, 0.95f),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(x, -14), new Vector2(cardW, cardH));
            var card = new Card { Bg = bg };
            card.Icon = UIFactory.Panel(bg.transform, "Icon", Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(10, -10), new Vector2(64, 64));
            card.Icon.preserveAspect = true;
            card.Title = UIFactory.Label(bg.transform, "Title", "", 32, new Color(0.15f, 0.12f, 0.2f), TextAnchor.MiddleLeft,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(84, -10), new Vector2(-94, 64));
            card.Title.GetComponent<Shadow>().enabled = false;
            card.Steps = UIFactory.Label(bg.transform, "Steps", "", 24, new Color(0.3f, 0.3f, 0.35f), TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(12, -80), new Vector2(-24, 80));
            card.Steps.GetComponent<Shadow>().enabled = false;
            var timeBg = UIFactory.Panel(bg.transform, "TimeBg", new Color(0.2f, 0.2f, 0.25f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0),
                new Vector2(10, 10), new Vector2(-20, 16));
            card.TimeFillImg = UIFactory.Panel(timeBg.transform, "Fill", Good, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f),
                Vector2.zero, Vector2.zero);
            card.TimeFill = card.TimeFillImg.rectTransform;
            bg.gameObject.SetActive(false);
            cards.Add(card);
        }

        // ---- Toasts ----
        toastRoot = UIFactory.Rect(root, "Toasts", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 62), new Vector2(1300, 200));

        // ---- Controls hint ----
        var hintBg = UIFactory.Panel(root, "Controls", new Color(0f, 0f, 0f, 0.55f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 8), new Vector2(1500, 44));
        UIFactory.Label(hintBg.transform, "Text",
            "Move: WASD / Arrows     Space: Pick up / Put down / Use     E or Left Click: Work (hold / mash)     Esc: Pause",
            24, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        BuildPausePanel(root);
        BuildResultPanel(root);
    }

    private void BuildPausePanel(Transform root)
    {
        var dim = UIFactory.Stretch(root, "PausePanel", new Color(0f, 0f, 0f, 0.7f), true);
        pausePanel = dim.gameObject;
        UIFactory.Label(dim.transform, "Title", "PAUSED", 90, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(800, 120));
        UIFactory.Label(dim.transform, "Recipes", RecipeText(), 28, Info, TextAnchor.UpperLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(900, 220));
        UIFactory.Button(dim.transform, "Resume", new Vector2(-170, -160), new Vector2(300, 80), new Color(0.25f, 0.6f, 0.35f), TogglePause);
        UIFactory.Button(dim.transform, "Main Menu", new Vector2(170, -160), new Vector2(300, 80), new Color(0.55f, 0.3f, 0.3f), GoToMenu);
        pausePanel.SetActive(false);
    }

    private void BuildResultPanel(Transform root)
    {
        var dim = UIFactory.Stretch(root, "ResultPanel", new Color(0.05f, 0.04f, 0.08f, 0.85f), true);
        resultPanel = dim.gameObject;
        UIFactory.Label(dim.transform, "Title", "TIME'S UP!", 100, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 280), new Vector2(1000, 140));
        resultText = UIFactory.Label(dim.transform, "Result", "", 44, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1000, 340));
        UIFactory.Button(dim.transform, "Play Again", new Vector2(-190, -250), new Vector2(340, 90), new Color(0.25f, 0.6f, 0.35f), Restart);
        UIFactory.Button(dim.transform, "Main Menu", new Vector2(190, -250), new Vector2(340, 90), new Color(0.55f, 0.3f, 0.3f), GoToMenu);
        resultPanel.SetActive(false);
    }

    public static string RecipeText()
    {
        return "<b>Recipes</b>  (stained clothes: Stain Remover first!)\n" +
               "T-shirt :  Washer → Dryer → Fold\n" +
               "Shirt   :  Washer → Drying Rack → Iron → Fold\n" +
               "Sweater :  Hand Wash → Drying Rack → Fold\n" +
               "Suit    :  Dry Cleaner → Iron → Fold";
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        timeText.text = gm.GetRemainingTime();
        timeText.color = gm.GameTime < 30f && Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f ? Bad : Color.white;
        cashText.text = $"$ {gm.Cash}";
        scoreText.text = $"Score {gm.Score}";
        statsText.text = $"Delivered: <color=#7CF080>{gm.ClothesDelivered}</color>\nRuined: <color=#FF7A70>{gm.ClothesRuined}</color>\nExpired: <color=#FFCC60>{gm.OrdersExpired}</color>";

        UpdateCards();
        UpdateToasts();

        if (Input.GetKeyDown(KeyCode.Escape) && !resultShown) TogglePause();

        if (gm.IsTimeOver && !resultShown) ShowResult();
    }

    private void UpdateCards()
    {
        var orders = OrderManager.Instance != null ? OrderManager.Instance.ActiveOrders : null;
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (orders == null || i >= orders.Count || orders[i].Cloth == null)
            {
                if (card.Bg.gameObject.activeSelf) card.Bg.gameObject.SetActive(false);
                continue;
            }
            var o = orders[i];
            var cloth = o.Cloth;
            if (!card.Bg.gameObject.activeSelf) card.Bg.gameObject.SetActive(true);

            card.Icon.sprite = PixelArt.Get(PixelArt.ClothKey(cloth.Type));
            card.Title.text = $"<b>{cloth.DisplayName}</b>  <size=24>${cloth.Profit}</size>";
            card.Steps.text = StepsText(cloth);

            float t = Mathf.Clamp01(o.TimeLeft / o.TotalTime);
            card.TimeFill.anchorMax = new Vector2(t, 1f);
            card.TimeFillImg.color = t > 0.5f ? Good : t > 0.25f ? Warn : Bad;
            bool urgent = t < 0.25f && Mathf.Repeat(Time.time, 0.6f) < 0.3f;
            card.Bg.color = urgent ? new Color(1f, 0.8f, 0.75f, 0.95f) : new Color(0.97f, 0.95f, 0.88f, 0.95f);
        }
    }

    private static string StepsText(ClothBase cloth)
    {
        var sb = new StringBuilder();
        if (cloth.IsStained) sb.Append("<color=#B0702A><b>STAIN!</b></color> → ");
        var proc = cloth.GetProcedure();
        for (int i = 0; i < proc.Length; i++)
        {
            if (i > 0) sb.Append(" → ");
            string name = ProcessStepUtil.ShortName(proc[i]);
            if (i < cloth.StepIndex) sb.Append($"<color=#3A9A4A>✓{name}</color>");
            else if (i == cloth.StepIndex && !cloth.IsStained) sb.Append($"<color=#C04020><b>{name}</b></color>");
            else sb.Append($"<color=#8A8A95>{name}</color>");
        }
        if (cloth.IsFinished) sb.Append("\n<color=#3A9A4A><b>READY - deliver to counter!</b></color>");
        return sb.ToString();
    }

    private void AddToast(string message, Color color)
    {
        var t = UIFactory.Label(toastRoot, "Toast", message, 30, color, TextAnchor.MiddleCenter,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 40));
        var outline = t.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);
        toasts.Insert(0, new Toasty { Text = t, Life = 3.2f });
        UpdateToasts();
        while (toasts.Count > 4)
        {
            Destroy(toasts[toasts.Count - 1].Text.gameObject);
            toasts.RemoveAt(toasts.Count - 1);
        }
    }

    private void UpdateToasts()
    {
        for (int i = toasts.Count - 1; i >= 0; i--)
        {
            var t = toasts[i];
            t.Life -= Time.unscaledDeltaTime;
            if (t.Life <= 0f)
            {
                Destroy(t.Text.gameObject);
                toasts.RemoveAt(i);
                continue;
            }
            var c = t.Text.color;
            c.a = Mathf.Clamp01(t.Life / 0.6f);
            t.Text.color = c;
        }
        for (int i = 0; i < toasts.Count; i++)
        {
            toasts[i].Text.rectTransform.anchoredPosition = new Vector2(0, i * 42);
        }
    }

    private void TogglePause()
    {
        bool paused = !pausePanel.activeSelf;
        pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    private void ShowResult()
    {
        resultShown = true;
        var gm = GameManager.Instance;
        gm.StopGame();
        pausePanel.SetActive(false);
        resultPanel.SetActive(true);
        resultText.text =
            $"Clothes delivered:  <color=#7CF080>{gm.ClothesDelivered}</color>\n" +
            $"Clothes ruined:  <color=#FF7A70>{gm.ClothesRuined}</color>\n" +
            $"Orders expired:  <color=#FFCC60>{gm.OrdersExpired}</color>\n\n" +
            $"Cash:  <color=#FFE066>${gm.Cash}</color>\n" +
            $"<size=60><b>Score: {gm.Score}</b></size>";
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.StopGame();
        SceneManager.LoadScene("Menu");
    }
}
