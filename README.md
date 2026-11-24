# Please Read!!

為避免之後可能的問題，以及增加效率，請各位組員注意以下命名相關規則。

## File Structure

由於可能因為空資料夾不會push到GitHub，所以如果發現沒有資料夾，請按照以下命名規則。
Audio/ -> 音樂/音效
Materials/ -> 材質相關
Scripts/ -> 所有程式碼
Prefabs/ -> 放Prefabs
Scene/ -> 所有場景
Resources/ -> 其他可能無法分類到上述資料夾的檔案

若有其他較大的檔案，或因copyright無法上傳，請丟到 ThirdParty/ 並在DC群內放上網址。

## 一些規則

- 上述資料夾內可以自己按照功能分進subfolder裡面，命名還請不要太過奔放
- 如果需要測試自己寫的功能，或是要push測試/示範用場景，請**務必**自己建立一個場景，多個人處理同個場景容易造成衝突

## Prototype（`prototype` branch）

開 `Assets/Scenes/Menu.unity` 按 Play 就能玩（Build Settings 已設好 Menu → MainGame）。
整個關卡由 `GameBootstrap` 在執行時用程式組出來，數值（機器時間等）可以在 MainGame 場景的 `GameBootstrap` Inspector 調整。

- 操作：WASD/方向鍵移動、Space 拿起/放下/使用、E 或滑鼠左鍵工作（長按或連點）、Esc 暫停
- 衣物流程定義在 `Scripts/Cloth/*.cs`（`procedure` 陣列），做錯一步就壞
- **衣物流程圖**：[`docs/ClothFlow.md`](docs/ClothFlow.md)（每件衣服的步驟、出事時會到哪一步、程式狀態圖）；遊戲內新手教學最後一頁也有同一張圖
- 美術：目前是 `PixelArt.cs` 程式產生的像素圖；把圖拖進 MainGame 場景 `ArtLibrary` 物件的欄位即可直接替換
- 選單 `Overwashed > Setup Prototype Scenes / Build Mac / Build Windows`
- Development build 作弊鍵：F1 生訂單、F2 小孩、F3 升級套件、F4 直接結束回合
- 測試：執行檔加 `-selftest`（衣物流程）或 `-playtest`（模擬完整遊玩流程），結果寫在 Player.log
- 新手教學：`Scripts/UI/TutorialPanel.cs`，一頁一頁按 Next 翻（← → / A D / Enter 也可以），主選單 TUTORIAL 按鈕、暫停選單、第一次開局會自動跳出（期間遊戲暫停）。頁面內容在檔案上方的 `Pages` 清單，要改文字或加頁直接改那裡；想每局都顯示可以勾 `GameBootstrap` 的 `Always Show Tutorial`

### 給組長／組員：相對於 `base-impl` 的變更

以下是 prototype 對組長原本程式或專案設定做的改動，合併前請確認：

1. **移除 `com.unity.ads` 套件**（`Packages/manifest.json`）
   在 Unity 2021.3.5f1 上這個套件本身會編譯失敗（`ScreenOrientation.Landscape` obsolete），導致無法 Build。專案內沒有任何地方使用廣告功能。之後若真的需要再用 Package Manager 裝新版即可。
2. **`ClothBase` 的加錢時機改為「交件時」**
   原本 `ChangeState(Finished)` 時就加錢加分，現在改成把衣服送回櫃台時才呼叫 `Deliver()` 加錢加分；否則衣服摺好但沒交、或訂單逾時，都會已經拿到錢。壞掉時扣賠償金的邏輯維持原樣。
   其他命名與結構都保留：`ClothState`、`ChangeState`、`Wash(WashMethod, args)` / `Dry(DryMethod, args)` 抽象方法都還在，四種衣服（`TShirt`/`Shirt`/`Sweater`/`Suit`）就是實作這兩個方法。
3. **`ClothBase` 的 `profit` / `compensate` / `scoreGain` 加上 `[SerializeField]`**
   原本是 private 又沒序列化，數值永遠是 0。
4. **`GameManager` 新增 `ResetGame()`、`IsRunning`、統計欄位**
   因為 `DontDestroyOnLoad`，重開一局時要重置時間／金錢／分數；倒數仍在原本的 `FixedUpdate`，回合長度維持 600 秒。
5. **新增 `ProcessStep` enum**（在 `ProcedureEnum.cs`）
   原本的 `WashMethod` / `DryMethod` 保留，另外加上去漬、燙衣、摺衣等步驟，用來定義每種衣服的流程。
6. **場景與設定**
   `Menu.unity`、`MainGame.unity` 各多了一個 bootstrap 物件（MainGame 另有 `ArtLibrary`）；Build Settings 改為 Menu → MainGame；Player Settings 改為視窗模式 1600×900、可調整視窗大小。`SampleScene`、`UI Test` 沒有動。
7. **規格外的小調整**
   店中間多放 2 張桌子，讓衣服可以暫放（小孩也碰得到）。

**還沒做／之後可以做：** 音效、正式美術、補救機制（目前做錯一步就直接壞）、設備升級商店、顧客角色與互動、數值平衡。
