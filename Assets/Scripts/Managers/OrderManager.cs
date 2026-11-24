using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("訂單配置")]
    [SerializeField] private List<OrderData> availableOrders; // 可隨機產生的訂單資料庫
    [SerializeField] private int maxActiveOrders = 4; // 上限同時進行的訂單數
    [SerializeField] private float spawnInterval = 8f; // 生成新訂單間隔秒數

    private float spawnTimer;
    public List<OrderInstance> ActiveOrders { get; private set; } = new List<OrderInstance>();

    // 事件通知（供 UI 顯示訂單列表）
    public static event Action OnOrdersUpdated;

    [System.Serializable]
    public class OrderInstance
    {
        public OrderData data;
        public float remainingTime;

        public OrderInstance(OrderData data)
        {
            this.data = data;
            this.remainingTime = data.orderTimeLimit;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) 
            return;

        // 1. 定時生成訂單
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnOrder();
        }

        // 2. 更新活躍訂單倒數時間
        for (int i = ActiveOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrders[i].remainingTime -= Time.deltaTime;
            if (ActiveOrders[i].remainingTime <= 0)
            {
                // 訂單超時失敗懲罰
                Debug.Log($"訂單 {ActiveOrders[i].data.orderID} 超時失敗！");
                ActiveOrders.RemoveAt(i);
                OnOrdersUpdated?.Invoke();
            }
        }
    }

    public void TrySpawnOrder()
    {
        if (ActiveOrders.Count >= maxActiveOrders || availableOrders.Count == 0) return;

        OrderData randomData = availableOrders[UnityEngine.Random.Range(0, availableOrders.Count)];
        ActiveOrders.Add(new OrderInstance(randomData));
        OnOrdersUpdated?.Invoke();
        Debug.Log($"新訂單上門：{randomData.orderID}");
    }

    /// <summary>
    /// 當玩家嘗試交貨時呼叫
    /// </summary>
    /// <param name="deliveredGarmentTypeID">玩家交付的衣物類別</param>
    /// <param name="isCorrectlyProcessed">衣物是否正確處理完畢（無壞掉）</param>
    public bool DeliverGarment(string deliveredGarmentTypeID, bool isCorrectlyProcessed)
    {
        if (!isCorrectlyProcessed)
        {
            Debug.Log("交付了損壞或步驟錯誤的衣服，無法完成訂單！");
            return false;
        }

        // 尋找符合種類的訂單
        OrderInstance matchingOrder = ActiveOrders.Find(o => o.data.garmentTypeID == deliveredGarmentTypeID);
        if (matchingOrder != null)
        {
            GameManager.Instance.AddScore(matchingOrder.data.rewardScore);
            ActiveOrders.Remove(matchingOrder);
            OnOrdersUpdated?.Invoke();
            Debug.Log($"成功完成訂單：{matchingOrder.data.orderID}！獲得 {matchingOrder.data.rewardScore} 分");
            return true;
        }

        Debug.Log("未找到匹配此衣物的訂單！");
        return false;
    }
}