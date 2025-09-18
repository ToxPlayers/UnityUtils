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

[Serializable]
public class StatCollection<TEnum> : ISerializationCallbackReceiver where TEnum : struct,Enum
{ 
    [SerializeField, TableList(AlwaysExpanded = true,
#if ODIN_INSPECTOR
        IsReadOnly = true
#else 
HideAddButton = true, HideRemoveButton = true
#endif
        )] 
    StatValue[] _stats;
    public StatCollection() => _stats = new StatValue[CExtensions.EnumCount<TEnum>()];
    public StatValue GetStat(TEnum stat) => _stats[Convert.ToInt32(stat)];
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

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        var values = Enum.GetValues(typeof(TEnum)); 
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
            _stats[i] ??= new(1f);
            if ( ! _stats[i].Equals(enumVal) )
                _stats[i] = new StatValue(_stats[i]);
        }  
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize() {  }
}
 