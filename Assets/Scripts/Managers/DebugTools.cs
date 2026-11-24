using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Developer tools, only active with command line arguments:
///   -selftest            : run cloth state-machine checks and log [SELFTEST] lines
///   -autoshot &lt;dir&gt;     : take screenshots of the menu / game / result screen and quit
/// Also: F1 in game spawns an order, F2 a kid, F3 an upgrade kit, F4 ends the round (editor / development builds).
/// </summary>
public class DebugTools : MonoBehaviour
{
    public static bool HasArg(string name)
    {
        foreach (var a in Environment.GetCommandLineArgs()) if (a == name) return true;
        return false;
    }

    private static string ArgValue(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }

    private static string ShotDir => ArgValue("-autoshot") ?? Application.persistentDataPath;

    // ---------------- Self test ----------------

    public static void RunSelfTest()
    {
        int pass = 0, fail = 0;
        void Check(string name, bool ok)
        {
            if (ok) pass++; else fail++;
            Debug.Log($"[SELFTEST] {(ok ? "PASS" : "FAIL")} {name}");
        }

        ClothBase Make<T>() where T : ClothBase => new GameObject("Test").AddComponent<T>();

        var t = Make<TShirt>();
        t.Wash(WashMethod.MachineWash, null);
        t.Dry(DryMethod.MachineDry, null);
        t.TryProcess(ProcessStep.Fold);
        Check("T-shirt correct flow finishes", t.IsFinished);

        var t2 = Make<TShirt>();
        t2.SetStained(true);
        t2.Wash(WashMethod.MachineWash, null);
        Check("Stained T-shirt washed without stain removal breaks", t2.IsBroken);

        var t3 = Make<TShirt>();
        t3.SetStained(true);
        Check("Stain removal works", t3.TryProcess(ProcessStep.StainRemoval) && !t3.IsStained);
        t3.Wash(WashMethod.MachineWash, null);
        Check("After stain removal, wash OK", !t3.IsBroken && t3.State == ClothState.Washed);

        var s = Make<Sweater>();
        s.Wash(WashMethod.HandWash, null);
        s.Dry(DryMethod.MachineDry, null);
        Check("Sweater in dryer breaks", s.IsBroken);

        var sh = Make<Shirt>();
        sh.Wash(WashMethod.MachineWash, null);
        sh.Dry(DryMethod.AirDry, null);
        sh.TryProcess(ProcessStep.Iron);
        sh.TryProcess(ProcessStep.Fold);
        Check("Shirt correct flow finishes", sh.IsFinished);

        var su = Make<Suit>();
        su.Wash(WashMethod.MachineWash, null);
        Check("Suit in washer breaks", su.IsBroken);

        var su2 = Make<Suit>();
        su2.Wash(WashMethod.DryClean, null);
        su2.TryProcess(ProcessStep.Iron);
        su2.MakeDirty();
        Check("Dirty suit restarts procedure", su2.IsStained && su2.StepIndex == 0 && !su2.IsBroken);

        var clean = Make<Sweater>();
        Check("Stain removal on clean cloth does nothing", !clean.TryProcess(ProcessStep.StainRemoval) && !clean.IsBroken);

        foreach (var c in FindObjectsOfType<ClothBase>()) Destroy(c.gameObject);
        Debug.Log($"[SELFTEST] DONE pass={pass} fail={fail}");
    }

    // ---------------- Screenshots ----------------

    public void StartMenuShots()
    {
        StartCoroutine(MenuShots());
    }

    private IEnumerator MenuShots()
    {
        yield return new WaitForSeconds(1.5f);
        Shot("01_menu.png");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("MainGame");
    }

    public void StartGameShots(GameBootstrap game)
    {
        StartCoroutine(GameShots(game));
    }

    private IEnumerator GameShots(GameBootstrap g)
    {
        yield return new WaitForSeconds(0.5f);
        var om = OrderManager.Instance;
        om.SpawnOrder(ClothType.TShirt, true);
        om.SpawnOrder(ClothType.Shirt, false, g.WasherA);
        om.SpawnOrder(ClothType.Sweater, false, g.HandWash);
        om.SpawnOrder(ClothType.Suit, false, g.IronBoard);
        RandomEventManager.Instance.SpawnKit();
        yield return new WaitForSeconds(2.5f);
        Shot("02_game.png");

        RandomEventManager.Instance.SpawnKid();
        yield return new WaitForSeconds(6f);
        Shot("03_game_later.png");

        GameManager.Instance.GameTime = 0.1f;
        yield return new WaitForSeconds(1.5f);
        Shot("04_result.png");
        yield return new WaitForSecondsRealtime(1f);
        Application.Quit();
    }

    private static void Shot(string file)
    {
        Directory.CreateDirectory(ShotDir);
        var path = Path.Combine(ShotDir, file);
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("[AUTOSHOT] " + path);
    }

    // ---------------- Cheat keys ----------------

    private void Update()
    {
        if (!Debug.isDebugBuild) return;
        if (OrderManager.Instance == null) return;
        if (Input.GetKeyDown(KeyCode.F1)) OrderManager.Instance.SpawnOrder();
        if (Input.GetKeyDown(KeyCode.F2)) RandomEventManager.Instance.SpawnKid();
        if (Input.GetKeyDown(KeyCode.F3)) RandomEventManager.Instance.SpawnKit();
        if (Input.GetKeyDown(KeyCode.F4)) GameManager.Instance.GameTime = 0.1f;
    }
}
