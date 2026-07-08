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
    [field: HorizontalGroup, SerializeField] public string ModifierName { get; set; } = "Flat modifier";
    [HorizontalGroup] public float Multiplier;  
    public void Modify(ref float baseValue, ref float multiplierValue) => multiplierValue *= Multiplier;
    public StatMultiplierModifier(string name) {
        ModifierName = name;
        Multiplier = 1f;
    }
    public StatMultiplierModifier(string name, float multiplier) {
        ModifierName = name;
        Multiplier = multiplier;
    }
}
[Serializable]
public class StatFlatModifier : IStatModifierBase {
    [field: HorizontalGroup, SerializeField] public string ModifierName { get; set; } = "Multiplier modifier";
    [HorizontalGroup] public float BaseAddAmount;
    public void Modify(ref float baseValue, ref float multiplierValue) => baseValue += BaseAddAmount;
    public StatFlatModifier(string name, float baseAddAmount) {
        ModifierName = name;
        BaseAddAmount = baseAddAmount;
    }
}