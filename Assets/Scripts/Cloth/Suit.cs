using System.Collections.Generic;
using UnityEngine;

/// <summary>Suit: (stain) -> dry clean -> iron -> fold. Water ruins it.</summary>
public class Suit : ClothBase
{
    public override ClothType Type => ClothType.Suit;
    public override string DisplayName => "Suit";
    protected override ProcessStep[] Procedure => procedure;
    private static readonly ProcessStep[] procedure = { ProcessStep.DryClean, ProcessStep.Iron, ProcessStep.Fold };
    protected override int DefaultProfit => 140;
    protected override int DefaultCompensate => 120;
    protected override int DefaultScore => 220;

    public override void Wash(WashMethod method, Dictionary<string, object> args)
    {
        TryProcess(ProcessStepUtil.ToStep(method));
    }

    public override void Dry(DryMethod method, Dictionary<string, object> args)
    {
        TryProcess(ProcessStepUtil.ToStep(method));
    }

    protected override string WrongStepReason(ProcessStep step)
    {
        switch (step)
        {
            case ProcessStep.MachineWash:
            case ProcessStep.HandWash: return "Suit soaked in water - it lost its shape!";
            case ProcessStep.MachineDry: return "Suit shrank in the dryer!";
        }
        return base.WrongStepReason(step);
    }
}
