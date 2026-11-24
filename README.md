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
- 美術：目前是 `PixelArt.cs` 程式產生的像素圖；把圖拖進 MainGame 場景 `ArtLibrary` 物件的欄位即可直接替換
- 選單 `Overwashed > Setup Prototype Scenes / Build Mac / Build Windows`
- Development build 作弊鍵：F1 生訂單、F2 小孩、F3 升級套件、F4 直接結束回合
- 測試：執行檔加 `-selftest`（衣物流程）或 `-playtest`（模擬完整遊玩流程），結果寫在 Player.log
