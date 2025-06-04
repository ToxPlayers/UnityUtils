using UnityEngine;
using System;
using System.Collections.Generic;
using TriInspector;
using UnityEditor;
using UnityEngine.Events;

[Serializable]
public class FloatStatCollection<T> : ISerializationCallbackReceiver where T : struct,Enum
{
    [System.Serializable]
    public struct StatValue
    {  
        [ShowInInspector, HideLabel, PropertyOrder(1)] public T Stat { get; private set; }
        [PropertyOrder(3), SerializeField] public FloatProcessor Processor;
        public StatValue(T stat, FloatProcessor processor)
        { 
            Stat = stat;
            Processor = processor;
        }
    }

    [SerializeField, TableList(AlwaysExpanded = true, HideAddButton = true, HideRemoveButton = true)] 
    StatValue[] _stats;
    public FloatStatCollection() => _stats = new StatValue[CExtensions.EnumCount<T>()];
    public FloatProcessor GetStatProcessor(T stat) => _stats[Convert.ToInt32(stat)].Processor;
    public float GetValue(T stat) => GetStatProcessor(stat).Value;
    public int GetIntFloorValue(T stat) => GetStatProcessor(stat).IntFloorValue;
    public float this[T stat] => GetValue(stat);
    public UnityEvent<IModifier<float>> OnRegMod(T stat) => GetStatProcessor(stat).OnRegisterMod;
    public UnityEvent<IModifier<float>> OnUnregMod(T stat) => GetStatProcessor(stat).OnUnregisterMod;
    [Button] public void RegisterMod(T stat, IModifier<float> mod)
    {
        GetStatProcessor(stat).RegisterModifier(mod);
    }
    public void UnregisterMod(T stat, IModifier<float> mod)
    {
        GetStatProcessor(stat).UnregisterModifier(mod);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        var values = Enum.GetValues(typeof(T)); 
        var count = values.Length;
        _stats ??= new StatValue[count];

        if(_stats.Length != count)
        {
            var copy = new StatValue[count];
            var copyMaxIndex = Mathf.Min(_stats.Length, count);
            Array.Copy(_stats, copy, copyMaxIndex);
            _stats = copy;
        }

        int i = 0;
        for (; i < count; i++)
        {
            var enumVal = values.GetValue(i);
            _stats[i].Processor ??= new(1f);
            if ( ! _stats[i].Stat.Equals(enumVal) )
                _stats[i] = new StatValue((T)enumVal, _stats[i].Processor);
        }  
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize() {  }
}
 