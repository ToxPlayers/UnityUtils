using UnityEngine;
using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
using UnityEditor;
using UnityEngine.Events;
using System.Collections;
using System.Collections.ObjectModel;
using Sirenix.Serialization;

[Serializable]
public class StatCollection<TEnum> : IEnumerable<(TEnum statName, StatValue stat)> where TEnum : struct,Enum 
{
    static public int EnumCount => EnumValues.Count;
    static public readonly ReadOnlyCollection<TEnum> EnumValues; 
    static StatCollection()
    {
        var arr = Enum.GetValues(typeof(TEnum));
        var values = new TEnum[arr.Length];
        Array.Copy(arr, values, arr.Length);
        EnumValues = new ReadOnlyCollection<TEnum>(values);
    }

    [SerializeField, ReadOnly]
    Dictionary<TEnum, StatValue> _stats;
    [ShowInInspector]
    public IReadOnlyDictionary<TEnum, StatValue> Stats => _stats;
    public StatCollection()
    {
        _stats = new();
        foreach (var entum in EnumValues)
            _stats.Add(entum, new());
    }
    public StatValue GetStat(TEnum stat) => _stats[stat];
    public float GetValue(TEnum stat) => GetStat(stat).Value;
    public int GetRoundedIntValue(TEnum stat) => GetStat(stat).ValueRounded;
    public float this[TEnum stat] => GetValue(stat);
    public UnityEvent<IStatModifierBase> OnRegMod(TEnum stat) => GetStat(stat).OnAddedModifier;
    public UnityEvent<IStatModifierBase> OnUnregMod(TEnum stat) => GetStat(stat).OnRemovedModifier;
    [Button] public void RegisterMod(TEnum stat, IStatModifierBase mod)
    {
        GetStat(stat).AddModifier(mod);
    }
    public void UnregisterMod(TEnum stat, IStatModifierBase mod)
    {
        GetStat(stat).RemoveModifier(mod);
    }
      
    public IEnumerator<(TEnum statName, StatValue stat)> GetEnumerator()
    {
        foreach(var tenum in EnumValues)
            yield return (tenum, GetStat(tenum));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
 