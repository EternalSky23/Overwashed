using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    public float GameTime = 600f;
    public int Cash = 1000;
    public int Score = 0;

    [Header("Round settings")]
    [SerializeField]
    private float roundLength = 600f;
    [SerializeField]
    private int startingCash = 1000;

    // Round statistics (shown on the result panel)
    public int ClothesDelivered = 0;
    public int ClothesRuined = 0;
    public int OrdersExpired = 0;

    /// <summary>True while a round is being played (timer counting down).</summary>
    public bool IsRunning { get; private set; }

    // Attribute getting
    public bool IsTimeOver => GameTime <= 0f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Reset every value for a new round. Needed because this object survives scene loads.
    /// </summary>
    public void ResetGame()
    {
        GameTime = roundLength;
        Cash = startingCash;
        Score = 0;
        ClothesDelivered = 0;
        ClothesRuined = 0;
        OrdersExpired = 0;
        IsRunning = true;
    }

    public void StopGame()
    {
        IsRunning = false;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (!IsRunning) return;

        if (GameTime > 0f)
        {
            GameTime = Mathf.Max(0f, GameTime - Time.fixedDeltaTime);
        }
        if (IsTimeOver)
        {
            IsRunning = false;
        }
    }

    public void ModifyScore(int value)
    {
        // Allow negative value argument
        Score += value;
    }

    public void ModifyCash(int value)
    {
        // Allow negative cash
        Cash += value;
    }

    public bool UseCash(int value)
    {
        // Return true if enough cash, and successfully use cash.
        // false otherwise.
        if (value < 0 || value > Cash)
        {
            return false;
        }

        Cash -= value;
        return true;
    }

    public string GetRemainingTime()
    {
        // Return a string format: "minute:second"
        int remainingSeconds = Mathf.Max(0, Mathf.FloorToInt(GameTime));
        int minutes = remainingSeconds / 60;
        int seconds = remainingSeconds % 60;

        return $"{minutes}:{seconds:00}";
    }
}
