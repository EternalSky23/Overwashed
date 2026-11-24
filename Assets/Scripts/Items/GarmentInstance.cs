using System;
using UnityEngine;

public class GarmentInstance : MonoBehaviour
{
    [Header("衣物資料配置")]
    [SerializeField] private GarmentData garmentData;

    public GarmentData Data => garmentData;
    public ProcessStep CurrentState { get; private set; } = ProcessStep.Dirty;
    public int CurrentStepIndex { get; private set; } = 0;

    // 狀態變更事件 (供 UI 或特效監聽)
    public event Action<ProcessStep> OnStateChanged;

    public void Initialize(GarmentData data)
    {
        garmentData = data;
        CurrentState = ProcessStep.Dirty;
        CurrentStepIndex = 0;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// 嘗試對衣物執行下一步處理 (如放入洗衣機、烘衣機或燙衣板)
    /// </summary>
    /// <param name="attemptedStep">玩家對衣服執行的步驟</param>
    /// <returns>處理是否成功</returns>
    public bool ProcessNextStep(ProcessStep attemptedStep)
    {
        // 若衣服已經壞掉或已完工，無法繼續處理
        if (CurrentState == ProcessStep.Ruined || CurrentState == ProcessStep.Completed)
        {
            Debug.LogWarning($"[Garment] {garmentData.garmentName} 處於 {CurrentState} 狀態，無法再進行處理！");
            return false;
        }

        // 檢查邊界條件
        if (garmentData == null || garmentData.requiredProcessFlow == null || CurrentStepIndex >= garmentData.requiredProcessFlow.Count)
        {
            SetRuined();
            return false;
        }

        // 取得該衣物預期的下一個正確步驟
        ProcessStep expectedStep = garmentData.requiredProcessFlow[CurrentStepIndex];

        if (attemptedStep == expectedStep)
        {
            CurrentStepIndex++;
            
            // 檢查是否已完成所有所需步驟
            if (CurrentStepIndex >= garmentData.requiredProcessFlow.Count)
            {
                CurrentState = ProcessStep.Completed;
            }
            else
            {
                CurrentState = attemptedStep;
            }

            OnStateChanged?.Invoke(CurrentState);
            Debug.Log($"[Garment] {garmentData.garmentName} 成功處理至步驟：{CurrentState}");
            return true;
        }
        else
        {
            // 做錯一步：直接壞掉！
            SetRuined();
            return false;
        }
    }

    /// <summary>
    /// 將衣物強制設為毀損狀態
    /// </summary>
    public void SetRuined()
    {
        CurrentState = ProcessStep.Ruined;
        OnStateChanged?.Invoke(CurrentState);
        Debug.LogWarning($"[Garment] 步驟錯誤！{garmentData?.garmentName ?? "衣物"} 已經毀損壞掉 (Ruined)！");
    }
}
