using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    public float GameTime = 600f;
    public int Cash = 1000;
    public int Score = 0;
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

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (GameTime > 0f)
        {
            GameTime = Mathf.Max(0f, GameTime - Time.fixedDeltaTime);
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
