
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrderData", menuName = "Laundry/Order Data")]
public class OrderData : ScriptableObject
{
    public string orderID;
    public string garmentTypeID; // 對應衣物種類識別碼（如: "Shirt_Cotton"）
    public float orderTimeLimit = 30f; // 單筆訂單倒數時間
    public int rewardScore = 100; // 完成獎勵分數
    public Sprite garmentIcon; // UI 顯示圖示
}
