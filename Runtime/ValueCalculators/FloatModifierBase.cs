
using System;
using UnityEngine;
 
public abstract class FloatModifierBase : IModifier<float>
{
    [SerializeField] bool _isBaseValue;
    public float Amount;
    public bool IsBaseValue => _isBaseValue;

    public abstract void Modify(ref float value);
}
[Serializable]
public class MultModifier : FloatModifierBase
{
    public override void Modify(ref float value) { value *= Amount; }
}
[Serializable]
public class AddModifier : FloatModifierBase
{
    public override void Modify(ref float value) { value += Amount; }
}
