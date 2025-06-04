public interface IModifier<T>
{
    public bool IsBaseValue { get; }
    public void Modify(ref T value); 
}
