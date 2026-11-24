using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM + sound effects.
///
/// Audio files are NOT in git (the sites' terms forbid redistributing the raw files and the repo is public).
/// Put them in  Assets/ThirdParty/Resources/Audio/  (ThirdParty is git-ignored) using the names in
/// <see cref="Sfx"/> / <see cref="Bgm"/>, e.g. "sfx_deliver.mp3", "bgm_game.ogg".
/// Missing files are simply silent, so the game works without them.
///
/// Press M in game to mute / unmute.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static class Bgm
    {
        public const string Menu = "bgm_menu";
        public const string Game = "bgm_game";
        public const string Hurry = "bgm_hurry";
    }

    public static class Sfx
    {
        public const string Pickup = "sfx_pickup";
        public const string PutDown = "sfx_putdown";
        public const string MachineStart = "sfx_machine_start";
        public const string MachineDone = "sfx_machine_done";
        public const string Scrub = "sfx_scrub";
        public const string Iron = "sfx_iron";
        public const string Stain = "sfx_stain";
        public const string Fold = "sfx_fold";
        public const string Order = "sfx_order";
        public const string Deliver = "sfx_deliver";
        public const string Ruined = "sfx_ruined";
        public const string Kid = "sfx_kid";
        public const string Kit = "sfx_kit";
        public const string Upgrade = "sfx_upgrade";
        public const string Click = "sfx_click";
        public const string TimeUp = "sfx_timeup";
    }

    private const string Folder = "Audio/";
    private const int PoolSize = 10;

    [Range(0f, 1f)] public float bgmVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private static AudioManager instance;
    private AudioSource bgmSource;
    private readonly List<AudioSource> pool = new List<AudioSource>();
    private readonly float[] stopAt = new float[PoolSize];
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
    private string currentBgm;
    private bool muted;

    private static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AudioManager");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AudioManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        for (int i = 0; i < PoolSize; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            pool.Add(s);
        }
    }

    private AudioClip Load(string name)
    {
        if (cache.TryGetValue(name, out var clip)) return clip;
        clip = Resources.Load<AudioClip>(Folder + name); // null when the file is not there -> silent
        cache[name] = clip;
        return clip;
    }

    // ---------------- Public API ----------------

    /// <summary>Start a looping BGM (does nothing if it is already playing).</summary>
    public static void PlayBgm(string name)
    {
        var m = Instance;
        if (m.currentBgm == name && m.bgmSource.isPlaying) return;
        m.currentBgm = name;
        var clip = m.Load(name);
        if (clip == null) return; // file not downloaded: keep whatever is playing
        m.bgmSource.Stop();
        m.bgmSource.clip = clip;
        m.bgmSource.volume = m.bgmVolume;
        m.bgmSource.Play();
    }

    public static void StopBgm()
    {
        if (instance == null) return;
        instance.bgmSource.Stop();
        instance.currentBgm = null;
    }

    /// <summary>
    /// Play a sound effect.
    /// maxLength &gt; 0 cuts long clips (e.g. a long washing-machine recording) after that many seconds.
    /// </summary>
    public static void Play(string name, float volume = 1f, float maxLength = 0f, float pitchJitter = 0f)
    {
        var m = Instance;
        var clip = m.Load(name);
        if (clip == null) return;

        // avoid the same sound stacking up in a single moment
        if (m.lastPlayed.TryGetValue(name, out var t) && Time.unscaledTime - t < 0.06f) return;
        m.lastPlayed[name] = Time.unscaledTime;

        int index = m.FreeSource();
        var src = m.pool[index];
        src.clip = clip;
        src.volume = m.sfxVolume * volume;
        src.pitch = 1f + (pitchJitter > 0f ? Random.Range(-pitchJitter, pitchJitter) : 0f);
        src.Play();
        m.stopAt[index] = maxLength > 0f ? Time.unscaledTime + maxLength : float.MaxValue;
    }

    private int FreeSource()
    {
        int oldest = 0;
        float oldestTime = float.MaxValue;
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].isPlaying) return i;
            if (pool[i].time > 0f && stopAt[i] < oldestTime)
            {
                oldestTime = stopAt[i];
                oldest = i;
            }
        }
        return oldest;
    }

    private void Update()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].isPlaying && Time.unscaledTime >= stopAt[i]) pool[i].Stop();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            muted = !muted;
            AudioListener.volume = muted ? 0f : 1f;
            HUD.Toast(muted ? "Sound muted (M)" : "Sound on (M)", HUD.Info);
        }
    }
}
