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
        Application.runInBackground = true;
        StartCoroutine(MenuShots());
    }

    private IEnumerator MenuShots()
    {
        yield return new WaitForSeconds(1.5f);
        Shot("01_menu.png");
        yield return new WaitForSeconds(0.3f);
        var tut = TutorialPanel.Open(false);
        yield return new WaitForSeconds(0.3f);
        Shot("01b_tutorial_p1.png");
        yield return new WaitForSeconds(0.2f);
        tut.NextPage();
        tut.NextPage();
        tut.NextPage();
        yield return new WaitForSeconds(0.3f);
        Shot("01c_tutorial_p4.png");
        yield return new WaitForSeconds(0.2f);
        tut.Close();
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("MainGame");
    }

    public void StartGameShots(GameBootstrap game)
    {
        Application.runInBackground = true;
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

    // ---------------- Scripted play-through (-playtest) ----------------

    public void StartPlaytest(GameBootstrap game)
    {
        Application.runInBackground = true;
        StartCoroutine(Playtest(game));
    }

    private IEnumerator Playtest(GameBootstrap g)
    {
        int pass = 0, fail = 0;
        void Check(string name, bool ok)
        {
            if (ok) pass++; else fail++;
            Debug.Log($"[PLAYTEST] {(ok ? "PASS" : "FAIL")} {name}");
        }

        yield return null;
        Time.timeScale = 5f;
        var gm = GameManager.Instance;
        var om = OrderManager.Instance;
        var p = PlayerController.Instance;
        om.AutoSpawn = false;
        RandomEventManager.Instance.AutoEvents = false;
        TrashStation trash = null;
        foreach (var s in g.Stations) if (s is TrashStation t) trash = t;

        IEnumerator WaitUntilDone(Func<bool> cond, float timeout = 30f)
        {
            float t = 0f;
            while (!cond() && t < timeout) { t += Time.deltaTime; yield return null; }
        }

        // ---- Order 1: T-shirt (clean) : washer -> dryer -> fold -> deliver ----
        var o1 = om.SpawnOrder(ClothType.TShirt, false, null);
        var c1 = o1.Cloth;
        g.Counter.Interact(p);
        Check("Pick up T-shirt from counter", p.Held == (IHoldable)c1);
        g.WasherA.Interact(p);
        yield return WaitUntilDone(() => c1.State == ClothState.Washed);
        Check("Washer washes T-shirt", c1.State == ClothState.Washed);
        g.WasherA.Interact(p);
        g.Dryer.Interact(p);
        yield return WaitUntilDone(() => c1.State == ClothState.Dried);
        g.Dryer.Interact(p);
        Check("Dryer dries T-shirt", c1.State == ClothState.Dried && p.Held == (IHoldable)c1);
        g.FoldTable.Interact(p);
        g.FoldTable.WorkPress(p);
        g.FoldTable.WorkHold(p, 1f);
        Check("Fold finishes T-shirt", c1.IsFinished);
        g.FoldTable.Interact(p);
        int cashBefore = gm.Cash;
        g.Counter.Interact(p);
        Check("Deliver T-shirt pays $60", gm.ClothesDelivered == 1 && gm.Cash == cashBefore + 60 && !p.IsHolding);

        // ---- Order 2: stained shirt : stain -> washer -> rack -> iron -> fold ----
        var o2 = om.SpawnOrder(ClothType.Shirt, true, null);
        var c2 = o2.Cloth;
        g.Counter.Interact(p);
        g.StainStation.Interact(p);
        g.StainStation.WorkPress(p);
        g.StainStation.WorkHold(p, 3f);
        Check("Stain removed", !c2.IsStained && !c2.IsBroken);
        g.StainStation.Interact(p);
        g.WasherB.Interact(p);
        yield return WaitUntilDone(() => c2.State == ClothState.Washed);
        g.WasherB.Interact(p);
        g.Rack.Interact(p);
        yield return WaitUntilDone(() => c2.State == ClothState.Dried);
        g.Rack.Interact(p);
        Check("Rack air-dries shirt", c2.State == ClothState.Dried && p.Held == (IHoldable)c2);
        g.IronBoard.Interact(p);
        g.IronBoard.WorkPress(p);
        g.IronBoard.WorkHold(p, 0.5f);
        g.IronBoard.WorkRelease(p);
        Check("Iron released too early does nothing", c2.StepIndex == 2 && !c2.IsBroken);
        g.IronBoard.WorkPress(p);
        g.IronBoard.WorkHold(p, 1.6f);
        g.IronBoard.WorkRelease(p);
        Check("Iron released in green zone works", c2.StepIndex == 3 && !c2.IsBroken);
        g.IronBoard.Interact(p);
        g.FoldTable.Interact(p);
        g.FoldTable.WorkHold(p, 1f);
        g.FoldTable.Interact(p);
        g.Counter.Interact(p);
        Check("Deliver shirt", gm.ClothesDelivered == 2);

        // ---- Order 3: sweater into the dryer -> ruined -> trash ----
        var o3 = om.SpawnOrder(ClothType.Sweater, false, null);
        var c3 = o3.Cloth;
        g.Counter.Interact(p);
        g.Dryer.Interact(p);
        yield return WaitUntilDone(() => c3.IsBroken);
        Check("Sweater ruined in dryer", c3.IsBroken && gm.ClothesRuined == 1 && !om.ActiveOrders.Contains(o3));
        g.Dryer.Interact(p);
        trash.Interact(p);
        yield return null;
        Check("Trash removes ruined sweater", c3 == null && !p.IsHolding);

        // ---- Overdue: leave a T-shirt in the washer ----
        var o4 = om.SpawnOrder(ClothType.TShirt, false, g.WasherA);
        var c4 = o4.Cloth;
        yield return WaitUntilDone(() => c4.IsBroken, 60f);
        Check("Cloth left in washer gets ruined", c4.IsBroken);
        g.WasherA.Interact(p);
        trash.Interact(p);

        // ---- Kid stains a cloth on a table ----
        var o5 = om.SpawnOrder(ClothType.Suit, false, null);
        var kid = RandomEventManager.Instance.SpawnKid();
        yield return WaitUntilDone(() => o5.Cloth.IsStained, 40f);
        Check("Kid stains the suit on the counter", o5.Cloth.IsStained && o5.Cloth.StepIndex == 0);

        // ---- Upgrade kit ----
        var kit = RandomEventManager.Instance.SpawnKit();
        RandomEventManager.Instance.PickUpKit(kit);
        p.Hold(kit);
        g.WasherB.Interact(p);
        Check("Upgrade kit makes washer 2x", g.WasherB.IsUpgraded && !p.IsHolding);

        // ---- Order expiry ----
        var o6 = om.SpawnOrder(ClothType.TShirt, false, g.Rack);
        o6.TimeLeft = 0.1f;
        yield return null;
        yield return null;
        Check("Expired order is counted", gm.OrdersExpired == 1 && o6.Cloth.IsBroken);

        Time.timeScale = 1f;
        Debug.Log($"[PLAYTEST] DONE pass={pass} fail={fail} cash={gm.Cash} score={gm.Score}");
        yield return new WaitForSecondsRealtime(0.5f);
        Application.Quit();
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
