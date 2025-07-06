using UnityEngine;
using TriInspector;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using TMPro;

[Serializable, HideMonoScript, InlineProperty]
public class Notifier<T>
{
    [NonSerialized] T _prevValue;
    [SerializeField, HideInInspector] T _value;
    [NonSerialized] UnityEvent<T, T> _onChange = new();
    [SerializeField] List<TMP_Text> _txtBinds = new();
    [ShowInInspector, HideLabel, PropertyOrder(-10)]
    public T Value
    {
        get => _value;
        set
        {
            if (_value.Equals(value))
                return;
            ForceValueChange(value);
        }
    }
    public void ForceValueChange(T value)
    {
        _prevValue = _value;
        _value = value;
        foreach (var txt in _txtBinds)
            txt.text = _value.ToString();
        InvokeChanged();
    }
    public void InvokeChanged()
    {
        _onChange.Invoke(_prevValue, _value);
    }
    public T PreviousValue => _prevValue;
    public void Bind(TMP_Text txt) => _txtBinds.Add(txt);
    public void Unbind(TMP_Text txt) => _txtBinds.Remove(txt);
    public void Sub(UnityAction<T, T> action, bool callNow = true)
    {
        _onChange.AddListener(action);
        if (callNow)
            action.Invoke(_prevValue, _value);
    }
    public void SubToggle(UnityAction<T, T> action, bool sub)
    {
        if (sub)
            Sub(action);
        else Unsub(action);
    }
    public void Unsub(UnityAction<T, T> action)
    {
        _onChange.RemoveListener(action);
    }
    static public implicit operator T(Notifier<T> w) => w.Value;
}