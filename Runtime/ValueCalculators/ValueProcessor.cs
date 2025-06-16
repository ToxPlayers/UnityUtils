using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

public class ValueProcessor<T> : ISerializationCallbackReceiver
{
    [SerializeReference, ReadOnly, PropertyOrder(20)] protected List<IModifier<T>> _modifiers = new();
    [SerializeField, HideInInspector] public UnityEvent<IModifier<T>> OnRegisterMod = new(), OnUnregisterMod = new(); 
    public void RegisterModifier(IModifier<T> mod)
    {
        _modifiers.Add(mod);
        OnRegisterMod.Invoke(mod);
    }
    public void UnregisterModifier(IModifier<T> mod)
    {
        _modifiers.Remove(mod);
        OnUnregisterMod.Invoke(mod);
    }
     
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        OnRegisterMod ??= new();
        OnUnregisterMod ??= new();
        _modifiers ??= new();
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {   }

    [PropertyOrder(0)] public T BaseValue;
    [ShowInInspector, PropertyOrder(10)] public T Value
    {
        get
        {
            var baseVal = BaseValue;

            foreach (var mod in _modifiers)
                if (mod != null && mod.IsBaseValue)
                    mod.Modify(ref baseVal);

            foreach (var mod in _modifiers)
                if(mod != null && !mod.IsBaseValue)
                    mod.Modify(ref baseVal);

            return baseVal;
        }
    } 
    public ValueProcessor(T baseValue)
    {
        BaseValue = baseValue;
    }
}
