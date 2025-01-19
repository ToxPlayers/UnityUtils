using System.Collections.Generic;
using TriInspector; 

[System.Serializable]
abstract public class ValueCalculator<T> where T : struct
{ 
    public interface IModifier  {  public T Modify(T value); } 
    [ShowInInspector, ReadOnly] protected readonly HashSet<IModifier> _modifiers = new();
    public void RegisterModifier(IModifier mod) => _modifiers.Add(mod);  
    public void UnregisterModifier(IModifier mod) => _modifiers.Remove(mod);
    public T BaseValue; 
    public T Value
    {
        get
        { 
            foreach (var mod in _modifiers)
                BaseValue = mod.Modify(BaseValue);
            return BaseValue;
        }
    }
} 