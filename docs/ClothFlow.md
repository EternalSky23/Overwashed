# 衣物處理流程圖（Cloth Flow）

這份文件說明每件衣服從「送到店裡」到「交回客人」的完整流程，以及過程中出事時會跑到哪一步。
遊戲內的新手教學最後一頁（Cloth Flow Chart）也是同一張圖，那一頁是直接讀取程式裡的流程畫出來的，改程式就會跟著變。

![遊戲內流程圖](cloth_flow_tutorial.png)

> 程式對應：流程定義在 `Assets/Scripts/Cloth/*.cs` 的 `procedure` 陣列；判斷邏輯在 `ClothBase.TryProcess()`。

---

## 1. 整體流程

```mermaid
flowchart LR
    A([客人送衣服到櫃台<br/>Order arrives]) --> S{有污漬？<br/>Stained?}
    S -- 有 --> SR[去漬台 Stain Remover<br/>長按 E / 滑鼠]
    S -- 沒有 --> P
    SR --> P[[依衣物種類的流程<br/>見下方第 2 節]]
    P --> F[摺衣台 Fold & Pack<br/>長按 E / 滑鼠]
    F --> D([送回櫃台交件 Deliver<br/>+ 金錢 + 分數])

    P -. 做錯任何一步 .-> X[壞掉 RUINED<br/>扣賠償金、訂單取消]
    F -. 做錯 .-> X
    X --> T[垃圾桶 Trash]

    P -. 小孩搗亂 .-> K[沾上污漬 STAINED]
    K --> SR2[去漬台] --> R[從第 1 步重新開始]
    R --> P
```

## 2. 各衣物的正確流程

| 衣物 | 正確流程（有污漬時先去漬） | 交件金額 | 壞掉賠償 |
|---|---|---|---|
| 👕 T-shirt | 洗衣機 → 烘乾機 → 摺衣 | $60 | $40 |
| 👔 Shirt 襯衫 | 洗衣機 → 曬衣架 → 燙衣 → 摺衣 | $90 | $60 |
| 🧶 Sweater 毛衣 | 手洗盆（狂點） → 曬衣架 → 摺衣 | $100 | $80 |
| 🤵 Suit 西裝 | 乾洗機 → 燙衣 → 摺衣 | $140 | $120 |

```mermaid
flowchart LR
    subgraph TS[T-shirt]
        direction LR
        t0[去漬?] --> t1[洗衣機] --> t2[烘乾機] --> t3[摺衣] --> t4([交件 $60])
    end
    subgraph SH[Shirt 襯衫]
        direction LR
        s0[去漬?] --> s1[洗衣機] --> s2[曬衣架] --> s3[燙衣] --> s4[摺衣] --> s5([交件 $90])
    end
    subgraph SW[Sweater 毛衣]
        direction LR
        w0[去漬?] --> w1[手洗盆] --> w2[曬衣架] --> w3[摺衣] --> w4([交件 $100])
    end
    subgraph SU[Suit 西裝]
        direction LR
        u0[去漬?] --> u1[乾洗機] --> u2[燙衣] --> u3[摺衣] --> u4([交件 $140])
    end
```

## 3. 出事時會到哪一步

| 狀況 | 結果 | 接下來 |
|---|---|---|
| 放到錯的工作站（例：毛衣進烘乾機、西裝進洗衣機） | **壞掉**，扣賠償金，訂單取消 | 拿去垃圾桶丟掉 |
| 有污漬就直接洗／烘 | **壞掉**（污漬洗不掉了） | 垃圾桶 |
| 洗衣機／烘乾機／乾洗機做完後約 20 秒沒取出 | **壞掉**（發霉／過熱） | 垃圾桶 |
| 燙衣時按太久（進度條超過綠色區間） | **壞掉**（燙焦） | 垃圾桶 |
| 客人等太久（訂單倒數歸零） | **壞掉**，算「逾時」 | 垃圾桶 |
| 小孩把衣服弄髒（只會碰不在機器裡的衣服） | **沾上污漬**，進度歸零（沒壞） | 去漬台 → 從第 1 步重新開始 |
| 燙衣時太早放開 | 沒事，停在「燙衣」這一步 | 再燙一次，在綠色區間放開 |
| 衣服沒污漬卻放到去漬台 | 沒事，提示「沒有污漬」 | 拿起來繼續下一步 |
| 洗到一半把衣服從機器拿出來 | 沒事，該步驟沒有完成 | 放回去重新開始這一步 |

## 4. 程式裡的狀態（給寫程式的組員）

`ClothBase` 用組長原本的 `ClothState` 表示大階段，再用 `stepIndex` 記錄目前做到流程的第幾步：

```mermaid
stateDiagram-v2
    [*] --> NotHandled
    NotHandled --> Washed: 洗衣機 / 手洗 / 乾洗
    Washed --> Dried: 烘乾機 / 曬衣架
    Washed --> Washed: 燙衣（西裝）
    Dried --> Dried: 燙衣（襯衫）
    Dried --> Finished: 摺衣
    Washed --> Finished: 摺衣（西裝）
    Finished --> [*]: 交件 Deliver()

    NotHandled --> Broken: 做錯一步
    Washed --> Broken: 做錯一步 / 放太久
    Dried --> Broken: 做錯一步 / 燙焦
    Finished --> Broken: 逾時 / 丟掉

    Washed --> NotHandled: 小孩弄髒（加污漬）
    Dried --> NotHandled: 小孩弄髒（加污漬）
    Finished --> NotHandled: 小孩弄髒（加污漬）
    Broken --> [*]: 垃圾桶
```

- `Broken` 之後不能再回復（`ChangeState` 會直接 return），之後若要加「補救機制」可以從這裡改。
- 污漬 `IsStained` 是獨立的旗標：有污漬時下一步一定是去漬，否則做任何步驟都會壞掉。
