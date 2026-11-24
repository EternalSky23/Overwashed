using System.Collections.Generic;
using UnityEngine;

/// <summary>T-shirt: (stain) -> washer -> dryer -> fold</summary>
public class TShirt : ClothBase
{
    public override ClothType Type => ClothType.TShirt;
    public override string DisplayName => "T-shirt";
    protected override ProcessStep[] Procedure => procedure;
    private static readonly ProcessStep[] procedure = { ProcessStep.MachineWash, ProcessStep.MachineDry, ProcessStep.Fold };
    protected override int DefaultProfit => 60;
    protected override int DefaultCompensate => 40;
    protected override int DefaultScore => 100;

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
            case ProcessStep.Fold: return "T-shirt folded while still wet - it went moldy!";
            case ProcessStep.Iron: return "T-shirt print melted under the iron!";
        }
        return base.WrongStepReason(step);
    }
}
