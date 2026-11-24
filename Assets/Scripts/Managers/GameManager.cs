using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { WaitingToStart, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;

    [Header("時間與分數設定")]
    [SerializeField] private float totalGameTime = 120f; // 一回合預設120秒
    public float RemainingTime { get; private set; }
    public int Score { get; private set; }

    // C# 廣播事件（供 UI 與其他系統訂閱）
    public static event Action<GameState> OnStateChanged;
    public static event Action<float> OnTimeUpdated;
    public static event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            RemainingTime -= Time.deltaTime;
            OnTimeUpdated?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                EndGame();
            }
        }
    }

    public void StartGame()
    {
        Score = 0;
        RemainingTime = totalGameTime;
        ChangeState(GameState.Playing);
        OnScoreChanged?.Invoke(Score);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    private void EndGame()
    {
        ChangeState(GameState.GameOver);
        Debug.Log($"遊戲結束！最終得分：{Score}");
    }
}
