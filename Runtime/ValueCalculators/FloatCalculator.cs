
[System.Serializable]
public class FloatCalculator : ValueCalculator<float>
{
    public FloatCalculator(float baseValue) : base(baseValue) { }

    static public implicit operator float(FloatCalculator fc) => fc.Value;
}