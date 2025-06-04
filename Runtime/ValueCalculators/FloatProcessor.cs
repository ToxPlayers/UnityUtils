using System;
using UnityEngine;

[Serializable]
public class FloatProcessor : ValueProcessor<float>
{
    public int IntFloorValue => Mathf.FloorToInt(Value); 
    public int IntRoundValue => Mathf.RoundToInt(Value);
    public int IntCeilValue => Mathf.CeilToInt(Value);
    public FloatProcessor(float baseValue) : base(baseValue) { }

    static public implicit operator float(FloatProcessor fc) => fc.Value;
}