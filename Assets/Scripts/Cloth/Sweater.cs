using System.Collections.Generic;
using UnityEngine;

/// <summary>Sweater: hand wash -> drying rack -> fold. Never machine wash / machine dry.</summary>
public class Sweater : ClothBase
{
    public override ClothType Type => ClothType.Sweater;
    public override string DisplayName => "Sweater";
    protected override ProcessStep[] Procedure => procedure;
    private static readonly ProcessStep[] procedure = { ProcessStep.HandWash, ProcessStep.AirDry, ProcessStep.Fold };
    protected override int DefaultProfit => 100;
    protected override int DefaultCompensate => 80;
    protected override int DefaultScore => 160;

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
            case ProcessStep.MachineDry: return "Sweater shrank in the dryer!";
            case ProcessStep.MachineWash: return "Sweater felted in the washer!";
            case ProcessStep.Iron: return "Sweater scorched by the iron!";
        }
        return base.WrongStepReason(step);
    }
}
