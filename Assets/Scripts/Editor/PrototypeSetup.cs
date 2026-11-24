using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu: Overwashed > Setup Prototype Scenes / Build Mac / Build Windows.
/// Puts the bootstrap objects into Menu.unity and MainGame.unity and sets the build scene list.
/// Can also run in batch mode: -executeMethod PrototypeSetup.SetupScenes
/// </summary>
public static class PrototypeSetup
{
    private const string MenuScene = "Assets/Scenes/Menu.unity";
    private const string GameScene = "Assets/Scenes/MainGame.unity";

    [MenuItem("Overwashed/Setup Prototype Scenes")]
    public static void SetupScenes()
    {
        var menu = EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        Ensure<MenuBootstrap>("MenuBootstrap");
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);

        var game = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        Ensure<ArtLibrary>("ArtLibrary");
        Ensure<GameBootstrap>("GameBootstrap");
        EditorSceneManager.MarkSceneDirty(game);
        EditorSceneManager.SaveScene(game);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScene, true),
            new EditorBuildSettingsScene(GameScene, true),
        };

        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.resizableWindow = true;

        AssetDatabase.SaveAssets();
        Debug.Log("[PrototypeSetup] Scenes ready: " + MenuScene + ", " + GameScene);
    }

    private static T Ensure<T>(string name) where T : Component
    {
        var existing = Object.FindObjectOfType<T>();
        if (existing != null) return existing;
        var go = new GameObject(name);
        return go.AddComponent<T>();
    }

    [MenuItem("Overwashed/Build Mac")]
    public static void BuildMac()
    {
        Build(BuildTarget.StandaloneOSX, "Builds/Mac/Overwashed.app");
    }

    [MenuItem("Overwashed/Build Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Overwashed.exe");
    }

    private static void Build(BuildTarget target, string path)
    {
        SetupScenes();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var options = new BuildPlayerOptions
        {
            scenes = new[] { MenuScene, GameScene },
            locationPathName = path,
            target = target,
            options = BuildOptions.Development,
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[PrototypeSetup] Build {report.summary.result}: {path} ({report.summary.totalErrors} errors)");
        if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
