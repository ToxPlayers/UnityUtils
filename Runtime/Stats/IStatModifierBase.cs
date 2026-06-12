using Sirenix.OdinInspector;
using System;
using UnityEngine;

public interface IStatModifierBase
{
    public string ModifierName { get; }
    public void Modify(ref float baseValue, ref float multValue);
}
[Serializable]
public class StatMultiplierModifier : IStatModifierBase { 
    [field: HorizontalGroup, SerializeField] public string ModifierName { get; private set; } = "Flat modifier";
    [HorizontalGroup] public float Multiplier;
    public void Modify(ref float baseValue, ref float multiplierValue) => multiplierValue *= Multiplier;
}
[Serializable]
public class StatFlatModifier : IStatModifierBase {
    [field: HorizontalGroup, SerializeField] public string ModifierName { get; private set; } = "Multiplier modifier";
    [HorizontalGroup] public float BaseAddAmount;
    public void Modify(ref float baseValue, ref float multiplierValue) => baseValue += BaseAddAmount;
}