using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
using UnityEngine.Events;
using System;
using UnityInternalExpose;

public interface IReadOnlyNotifier<T>
{  
    public T Value { get; }
    public T PreviousValue { get; }
    public void Sub(UnityAction<T, T> action, bool callNow = true);
    public void Sub(UnityAction<T> action, bool callNow = true);
    public void SubToggle(UnityAction<T, T> action, bool sub);
    public void SubToggle(UnityAction<T> action, bool sub);
    public void Unsub(UnityAction<T, T> action);
    public void Unsub(UnityAction<T> action);
}

[Serializable, HideMonoScript, InlineProperty]
public class Notifier<T> : IReadOnlyNotifier<T>
{ 
    [NonSerialized] T _prevValue;
    [SerializeField, HideInInspector] T _value;
    [NonSerialized] UnityEvent<T, T> _onChange = new();
    [NonSerialized] UnityEvent<T> _onChangeSingle = new();
    public IReadOnlyNotifier<T> Readonly => this;
    public T PreviousValue => _prevValue;
    public int ListenerCount => _onChange.GetListenerCount() + _onChangeSingle.GetListenerCount();
    [ShowInInspector, HideLabel, PropertyOrder(-10)]
#if ODIN_INSPECTOR
    [SuffixLabel("@" + nameof(ListenerCount), SdfIconType.EarFill)]
#endif
    public T Value
    {
        get => _value; 
		set
		{  
            if (_value == null && value == null)
				return;

			if (_value != null && _value.Equals(value))
				return;
			ForceValueChange(value);
		} 
    } 
	public Notifier() {}
    public Notifier(T value)
    { 
        ForceValueChange(value);
    }
    public void ForceValueChange(T value)
    {
        _prevValue = _value;
        _value = value; 
        InvokeChanged();
    } 
    public void InvokeChanged()
    {
        _onChange.Invoke(_prevValue, _value);
        _onChangeSingle.Invoke(_value);
    }
    public void Sub(UnityAction<T> action, bool callNow = true)
    {
        _onChangeSingle.AddListener(action);
        if (callNow)
            action.Invoke(_value);
    }

    public void Sub(UnityAction<T, T> action, bool callNow = true)
    {
        _onChange.AddListener(action);
        if (callNow)
            action.Invoke(_prevValue, _value);
    }
    public void SubToggle(UnityAction<T> action, bool sub)
    {
        if (sub)
            Sub(action);
        else Unsub(action);
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
    public void Unsub(UnityAction<T> action)
    {
        _onChangeSingle.RemoveListener(action); 
    }
    static public implicit operator T(Notifier<T> w) => w.Value;
}