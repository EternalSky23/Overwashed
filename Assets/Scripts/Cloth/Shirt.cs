using System.Collections.Generic;
using UnityEngine;

/// <summary>Shirt: (stain) -> washer -> drying rack -> iron -> fold</summary>
public class Shirt : ClothBase
{
    public override ClothType Type => ClothType.Shirt;
    public override string DisplayName => "Shirt";
    protected override ProcessStep[] Procedure => procedure;
    private static readonly ProcessStep[] procedure = { ProcessStep.MachineWash, ProcessStep.AirDry, ProcessStep.Iron, ProcessStep.Fold };
    protected override int DefaultProfit => 90;
    protected override int DefaultCompensate => 60;
    protected override int DefaultScore => 150;

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
            case ProcessStep.MachineDry: return "Shirt wrinkled beyond saving in the dryer!";
            case ProcessStep.Fold: return "Shirt folded too early - ruined!";
        }
        return base.WrongStepReason(step);
    }
}
