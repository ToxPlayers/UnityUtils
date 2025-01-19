
[System.Serializable]
public class FloatCalculator : ValueCalculator<float>
{
    static public implicit operator float(FloatCalculator fc) => fc.Value;
}