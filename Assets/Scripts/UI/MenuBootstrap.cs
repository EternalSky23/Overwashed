using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the main menu UI in code.</summary>
public class MenuBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Time.timeScale = 1f;
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.12f, 0.2f);
        }

        var canvas = UIFactory.CreateCanvas("Menu Canvas");
        var root = canvas.transform;
        UIFactory.Stretch(root, "Bg", new Color(0.14f, 0.12f, 0.2f), false);

        // decorative clothes
        string[] keys = { "cloth_tshirt", "cloth_shirt", "cloth_sweater", "cloth_suit", "washer", "dryer" };
        for (int i = 0; i < keys.Length; i++)
        {
            var img = UIFactory.Panel(root, "Deco" + i, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-500 + i * 200, 250), new Vector2(110, 110));
            img.sprite = PixelArt.Get(keys[i]);
            img.preserveAspect = true;
        }

        UIFactory.Label(root, "Title", "OVERWASHED", 150, new Color(0.6f, 0.85f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 410), new Vector2(1400, 180));
        UIFactory.Label(root, "Subtitle", "A chaotic laundromat — wash it right, wash it fast!", 40, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(1400, 60));

        UIFactory.Label(root, "HowTo",
            "<b>How to play</b>\n" +
            "Orders arrive at the <b>Counter</b>. Process each cloth in the right order, then deliver it back before time runs out.\n" +
            "One wrong step ruins the cloth! Machines must be emptied soon after they finish.\n" +
            "Watch out for <b>kids</b> making a mess, and grab <b>upgrade kits</b> to speed up machines.\n\n" +
            HUD.RecipeText() + "\n\n" +
            "<b>Controls</b>   Move: WASD / Arrows   |   Space: Pick up / Put down / Use   |   E or Left Click: Work (hold / mash)   |   Esc: Pause",
            28, new Color(0.88f, 0.9f, 1f), TextAnchor.UpperCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(1500, 440));

        UIFactory.Button(root, "START", new Vector2(-200, -420), new Vector2(340, 100), new Color(0.25f, 0.6f, 0.35f), StartGame);
        UIFactory.Button(root, "QUIT", new Vector2(200, -420), new Vector2(340, 100), new Color(0.55f, 0.3f, 0.3f), Quit);

        if (DebugTools.HasArg("-autoshot")) gameObject.AddComponent<DebugTools>().StartMenuShots();
        if (DebugTools.HasArg("-playtest"))
        {
            Application.runInBackground = true;
            SceneManager.LoadScene("MainGame");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) StartGame();
    }

    private void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    private void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
