using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class StatValue
{
    [ShowInInspector, ReadOnly, TableList]
    public HashSet<IStatModifierBase> Modifiers = new();
    public float BaseValue;
    [ShowInInspector] public float Value
    {
        get
        {
            var baseVal = BaseValue;
            var mult = 1f;
            foreach(var mod  in Modifiers) 
                mod.Modify(ref baseVal, ref mult);
            return baseVal * mult;
        }
    }
    static public implicit operator float(StatValue stat) => stat.Value;
    public StatValue() { }
    public StatValue(float baseValue) { BaseValue = baseValue; }

    public int ValueRounded => Value.RoundInt();
    public UnityEvent<IStatModifierBase> OnAddedModifier, OnRemovedModifier;

    public void AddModifier(IStatModifierBase modifier)
    {
        if (Modifiers.Add(modifier))
            OnAddedModifier.Invoke(modifier);
        else
            Debug.LogWarning("Tried adding modifier twice to the same StatValue");
    }
    public void RemoveModifier(IStatModifierBase modifier)
    {
        if(Modifiers.Remove(modifier))
            OnAddedModifier.Invoke(modifier);
    }
}
public interface IStatModifierBase
{
    public string ModifierName { get; }
    public void Modify(ref float baseValue, ref float multValue);
}
public class StatMultModifier : IStatModifierBase
{ 
    public float MultAmount;
    [field:SerializeField] public string ModifierName { get; private set; }
    public void Modify(ref float baseValue, ref float multValue) => multValue *= MultAmount;
}
public class StatBaseAddModifier : IStatModifierBase
{ 
    public float BaseAddAmount;
    [field: SerializeField] public string ModifierName { get; private set; }
    public void Modify(ref float baseValue, ref float multValue) => baseValue += BaseAddAmount;
}