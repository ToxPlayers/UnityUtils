using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public interface IReadonlyStatValue
{
    public float BaseValue { get; }
    public float Value { get; }
    public int ValueRounded { get; }
}

[Serializable]
public class StatValue : IReadonlyStatValue {
    [field: SerializeField]
    public float BaseValue { get; set; } = 1f;
    [ShowInInspector]
    public float Value {
        get {
            var baseVal = BaseValue;
            var mult = 1f;
            foreach (var mod in _modifiers)
                mod.Modify(ref baseVal, ref mult);
            return baseVal * mult;
        }
    }
    [ShowInInspector, ReadOnly, PropertyOrder(100)]
    List<IStatModifierBase> _modifiers = new();
    static public implicit operator float(StatValue stat) => stat.Value;
    public StatValue() { }
    public StatValue(float baseValue) { BaseValue = baseValue; }
    public int ValueRounded => Value.RoundInt();
    public IReadOnlyList<IStatModifierBase> Modifiers => _modifiers;
    [HideInInspector]
    public UnityEvent<IStatModifierBase> OnAddedModifier = new(), OnRemovedModifier = new();

    public void AddModifier(IStatModifierBase modifier)
    {
        _modifiers.Add(modifier);
        OnAddedModifier.Invoke(modifier); 
    }
    public void RemoveModifier(IStatModifierBase modifier)
    {
        if(_modifiers.Remove(modifier))
            OnAddedModifier.Invoke(modifier);
    }
}
