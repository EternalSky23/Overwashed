using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGarmentData", menuName = "Laundry/Garment Data")]
public class GarmentData : ScriptableObject
{
    public string garmentTypeID;                 // 衣物種類識別碼 (如: "Shirt_Cotton", "Suit_Wool")
    public string garmentName;                   // 衣物顯示名稱 (如: "普通棉衫", "高級西裝")
    public Sprite garmentIcon;                  // 衣物圖示
    public List<ProcessStep> requiredProcessFlow; // 標準處理步驟流程 (需按順序執行)
}
