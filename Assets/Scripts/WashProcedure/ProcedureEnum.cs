using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WashMethod
{
    MachineWash,
    HandWash,
    DryClean, // 乾洗
}

public enum DryMethod
{
    AirDry,
    MachineDry,
}

/// <summary>
/// Every single step a piece of cloth can go through.
/// A cloth type defines its correct procedure as an ordered list of these steps.
/// </summary>
public enum ProcessStep
{
    StainRemoval, // 去漬
    MachineWash,  // 洗衣機
    HandWash,     // 手洗
    DryClean,     // 乾洗
    MachineDry,   // 烘乾機
    AirDry,       // 曬衣架
    Iron,         // 燙衣
    Fold,         // 摺衣 / 包裝
}

public static class ProcessStepUtil
{
    public static ProcessStep ToStep(WashMethod method)
    {
        switch (method)
        {
            case WashMethod.HandWash: return ProcessStep.HandWash;
            case WashMethod.DryClean: return ProcessStep.DryClean;
            default: return ProcessStep.MachineWash;
        }
    }

    public static ProcessStep ToStep(DryMethod method)
    {
        return method == DryMethod.AirDry ? ProcessStep.AirDry : ProcessStep.MachineDry;
    }

    public static bool IsWash(ProcessStep step)
    {
        return step == ProcessStep.MachineWash || step == ProcessStep.HandWash || step == ProcessStep.DryClean;
    }

    public static bool IsDry(ProcessStep step)
    {
        return step == ProcessStep.MachineDry || step == ProcessStep.AirDry;
    }

    /// <summary>Short label used on order cards.</summary>
    public static string ShortName(ProcessStep step)
    {
        switch (step)
        {
            case ProcessStep.StainRemoval: return "Stain";
            case ProcessStep.MachineWash: return "Washer";
            case ProcessStep.HandWash: return "HandWash";
            case ProcessStep.DryClean: return "DryClean";
            case ProcessStep.MachineDry: return "Dryer";
            case ProcessStep.AirDry: return "Rack";
            case ProcessStep.Iron: return "Iron";
            case ProcessStep.Fold: return "Fold";
        }
        return step.ToString();
    }
}
