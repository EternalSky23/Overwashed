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

### 音樂／音效（不在 git 裡，要自己下載）

音效系統在 `Scripts/Managers/AudioManager.cs`。音檔來自 [魔王魂](https://maou.audio/)（BGM）與 [効果音ラボ](https://soundeffect-lab.info/)（音效），兩站都可免費用在遊戲、可剪輯、不需標示出處，**但禁止再散佈原檔**；我們的 repo 是公開的，所以音檔放在 git-ignore 的 `Assets/ThirdParty/Resources/Audio/`，請照下表自己下載（或從 DC 拿打包好的資料夾）。**沒有音檔遊戲也能正常玩，只是沒聲音。**

下載方式：
1. 用瀏覽器打開下表的頁面，點對應的檔案下載（魔王魂選「ループ版」的 **OGG**）。
2. Mac：全部下載到「下載項目」後執行 `bash Tools/import_audio.sh`，會自動複製並改名。Windows：手動複製到 `Assets/ThirdParty/Resources/Audio/` 並改成「遊戲內檔名」。
3. 回到 Unity 讓它匯入即可。遊戲中按 **M** 可靜音。

| 遊戲內檔名 | 用途 | 網站上的名稱 | 頁面 | 原始檔名 |
|---|---|---|---|---|
| `bgm_menu.ogg` | 主選單 BGM | アコースティック50「至福の隙間」（ループ版 OGG） | [魔王魂](https://maou.audio/bgm_acoustic50/) | maou_loop_bgm_acoustic50.ogg |
| `bgm_game.ogg` | 遊戲中 BGM | 8bit29「子猫のもんたのクッキー屋さん」（ループ版 OGG） | [魔王魂](https://maou.audio/bgm_8bit29/) | maou_loop_bgm_8bit29.ogg |
| `bgm_hurry.ogg` | 最後 30 秒 BGM | ファンタジー15「駆け抜ける戦場の嵐」（ループ版 OGG） | [魔王魂](https://maou.audio/bgm_fantasy15/) | maou_loop_bgm_fantasy15.ogg |
| `sfx_pickup.mp3` | 拿起衣服 | 洗濯機から服を取り出す | [生活・家電](https://soundeffect-lab.info/sound/various/) | washing-machine-laundry-takeout1.mp3 |
| `sfx_putdown.mp3` | 放下衣服 | 洗濯機に服を入れる「ボフッ」 | [生活・家電](https://soundeffect-lab.info/sound/various/) | washing-machine-laundry-put1.mp3 |
| `sfx_machine_start.mp3` | 機器開始 | 洗濯機スイッチオン | [生活・家電](https://soundeffect-lab.info/sound/various/) | washing-machine-switch1.mp3 |
| `sfx_machine_done.mp3` | 機器完成 | 洗濯機動作終了「ガコンピーッピーッ」 | [生活・家電](https://soundeffect-lab.info/sound/various/) | washing-machine-operation-completed1.mp3 |
| `sfx_scrub.mp3` | 手洗刷洗 | お風呂でごしごし1 | [生活・家電](https://soundeffect-lab.info/sound/various/) | bath-wash1.mp3 |
| `sfx_iron.mp3` | 燙衣（暫代） | 電車の圧搾空気排出「プシューッ」 | [機械](https://soundeffect-lab.info/sound/machine/) | train-steam1.mp3 |
| `sfx_stain.mp3` | 去漬（暫代） | 水をバシャッとかける1 | [生活・家電](https://soundeffect-lab.info/sound/various/) | sprinkle-water1.mp3 |
| `sfx_fold.mp3` | 摺衣 | 濡れた洗濯物を広げる | [生活・家電](https://soundeffect-lab.info/sound/various/) | laundry-spread1.mp3 |
| `sfx_order.mp3` | 新訂單 | ドアチャイム1「玄関のピンポン」 | [生活・家電](https://soundeffect-lab.info/sound/various/) | doorchime1.mp3 |
| `sfx_deliver.mp3` | 交件收錢 | レジスターで精算 | [生活・家電](https://soundeffect-lab.info/sound/various/) | clearing1.mp3 |
| `sfx_ruined.mp3` | 衣服壞掉 | クイズ不正解1「ブブー」 | [アニメ・ゲーム](https://soundeffect-lab.info/sound/anime/) | incorrect1.mp3 |
| `sfx_kid.mp3` | 小孩出現 | 盗賊の少年「べ～だ」 | [声素材・ゲーム](https://soundeffect-lab.info/sound/voice/game.html) | thief-boy-guard1.mp3 |
| `sfx_kit.mp3` | 撿到升級套件 | アイテムを入手1 | [アニメ・ゲーム](https://soundeffect-lab.info/sound/anime/) | item-get1.mp3 |
| `sfx_upgrade.mp3` | 機器升級 | レベルアップ「テッテレー」 | [アニメ・ゲーム](https://soundeffect-lab.info/sound/anime/) | levelup1.mp3 |
| `sfx_click.mp3` | 按鈕 | 決定ボタンを押す31「カチッ」 | [ボタン・システム音](https://soundeffect-lab.info/sound/button/) | decision31.mp3 |
| `sfx_timeup.mp3` | 時間到 | 試合終了のゴング | [アニメ・ゲーム](https://soundeffect-lab.info/sound/anime/) | gong-played2.mp3 |

想換音效：把新檔案用同樣的「遊戲內檔名」放進資料夾即可（mp3 / ogg / wav 都行），不用改程式。

### 第三方美術素材（不在 git 裡，從 DC 拿）

以下兩包像素素材的授權都**禁止再散佈原檔**，所以放在 git-ignore 的資料夾；**沒有這些檔案遊戲照樣能玩**，會自動改用程式產生的替代圖。

| 素材 | 放在哪裡 | 用在哪 |
|---|---|---|
| Pixel Townsfolk（by Anemon Creative） | `Assets/ThirdParty/Resources/Art/Townsfolk/`（保留原本的 `sheets/` 子資料夾） | 玩家（預設 Baker）、搗亂小孩（Kid），四方向走路／待機動畫 |
| Pixel Bathroom & Laundry Icons（by reodesuxz） | `Assets/ThirdParty/Resources/Art/Icons/`（保留原本的 `PNG/32x32/...` 子資料夾） | 洗衣機、烘乾機、燙衣板、垃圾桶 |

- 拿到 DC 上的兩個資料夾後，改名成 `Townsfolk`、`Icons` 放到上表位置即可，Unity 會自動用像素風設定匯入（`Editor/ThirdPartyArtImporter.cs`）。
- 想換玩家角色：MainGame 場景的 `GameBootstrap` → `Player Character` 填 Townsfolk 的角色名（Waiter、Woman、Teen…）。
- 想關掉第三方美術：取消勾選 `GameBootstrap` → `Use Third Party Art`。
- 要換哪些物件用哪張圖示：`Scripts/Visual/ThirdPartyArt.cs` 的 `IconMap`。
- 另外三包 3D 素材（CubedBear、PSX Laundromat ×2）不適合目前的 2D 俯視風格，沒有使用；已加進 `.gitignore` 避免誤傳。

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
