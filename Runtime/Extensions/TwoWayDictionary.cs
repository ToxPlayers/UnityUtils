using System.Collections;
using System.Collections.Generic;

public class TwoWayDictionary<T1, T2> : IDictionary<T1, T2> 
{
    IDictionary<T1, T2> _a = new Dictionary<T1,T2>();
    IDictionary<T2, T1> _b = new Dictionary<T2, T1>();

    public Dictionary<T1,T2> CopyDictionary() => new Dictionary<T1, T2>(_a); 
    public Dictionary<T2, T1> CopyReverseDictionary() => new Dictionary<T2, T1>(_b); 

    public T2 this[T1 key] 
    { 
        get => _a[key];
        set
        {
            _a[key] = value;
            _b[value] = key;
        }
    }

    public T1 this[T2 key]
    {
        get => _b[key];
        set
        {
            _b[key] = value;
            _a[value] = key;
        }
    }
     
    public ICollection<T1> Keys => _a.Keys;

    public ICollection<T2> Values => _a.Values;

    public int Count => _a.Count;

    public bool IsReadOnly => false;
    ICollection<T1> IDictionary<T1, T2>.Keys => _a.Keys; 
    ICollection<T2> IDictionary<T1, T2>.Values => _a.Values; 
    public void Add(T1 key, T2 value)
    {
        _a.Add(key, value);
        _b.Add(value, key);
    }
    public void Add(KeyValuePair<T1, T2> item) => _a.Add(item);
    public void Add(T2 key, T1 value) => Add(value, key);
    public void Add(KeyValuePair<T2, T1> item) => Add(item.Value, item.Key);
    public void Clear()
    {
        _a.Clear();
        _b.Clear();
    }
    public bool Contains(KeyValuePair<T1, T2> item) => _a.Contains(item);
    public bool Contains(KeyValuePair<T2, T1> item) => _b.Contains(item);
    public bool ContainsKey(T1 key) => _a.ContainsKey(key);
    public bool ContainsKey(T2 key) => _b.ContainsKey(key);
    public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex) => _a.CopyTo(array, arrayIndex);
    public void CopyTo(KeyValuePair<T2, T1>[] array, int arrayIndex) => _b.CopyTo(array, arrayIndex);


    public bool Remove(T1 key)
    {
        var value = _a[key];
        var res = _a.Remove(key);
        _b.Remove(value);
        return res;
    }
    public bool Remove(T2 key) 
    { 
        var value = _b[key];
        return Remove(value);
    }
    public bool Remove(KeyValuePair<T1, T2> item) => Remove(item.Key);
    public bool Remove(KeyValuePair<T2, T1> item) => Remove(item.Value);
    public bool TryGetValue(T1 key, out T2 value) => _a.TryGetValue(key, out value);
    public bool TryGetValue(T2 key, out T1 value) => _b.TryGetValue(key, out value);
    public IEnumerator GetEnumerator() => _a.GetEnumerator();
    IEnumerator<KeyValuePair<T1, T2>> IEnumerable<KeyValuePair<T1, T2>>.GetEnumerator() => _a.GetEnumerator();
}
