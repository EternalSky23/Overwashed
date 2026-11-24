using System.Collections.Generic;
using UnityEngine;

// 衣物處理的各個步驟狀態
public enum ProcessStep
{
    Dirty,       // 髒衣服 (初始狀態)
    Washing,     // 水洗 / 洗衣機
    Drying,      // 烘乾 / 烘衣機
    Ironing,     // 熨燙 / 燙衣板
    Completed,   // 完工成衣 (成功可交付)
    Ruined       // 毀損 / 壞掉 (步驟錯誤)
}