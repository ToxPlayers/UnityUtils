using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
using UnityEngine.Events;
using System;
using UnityInternalExpose; 

public abstract class ReadOnlyNotifier<T>
{
    public abstract T GetValue();
    public T Value => GetValue();
    public abstract T PreviousValue { get; }
    public abstract void Sub(UnityAction<T, T> action, bool callNow = true);
    public abstract void Sub(UnityAction<T> action, bool callNow = true);
    public abstract void SubToggle(UnityAction<T, T> action, bool sub);
    public abstract void SubToggle(UnityAction<T> action, bool sub);
    public abstract void Unsub(UnityAction<T, T> action);
    public abstract void Unsub(UnityAction<T> action);
    static public implicit operator T(ReadOnlyNotifier<T> w) => w.GetValue();
}

[Serializable, HideMonoScript, InlineProperty]
public class Notifier<T> : ReadOnlyNotifier<T> {
    [NonSerialized] T _prevValue;
    [SerializeField, HideInInspector] T _value;
    [NonSerialized] UnityEvent<T, T> _onChange = new();
    [NonSerialized] UnityEvent<T> _onChangeSingle = new(); 
    public ReadOnlyNotifier<T> ReadOnly => this;
    public override T PreviousValue => _prevValue;
    public int ListenerCount => _onChange.GetListenerCount() + _onChangeSingle.GetListenerCount();
    [ShowInInspector, HideLabel, PropertyOrder(-10)]
#if ODIN_INSPECTOR
    [SuffixLabel("@" + nameof(ListenerCount), SdfIconType.EarFill)]
#endif
    public new T Value
    {
        get => _value;
        set {  
            if (_value == null && value == null)
				return;

			if (_value != null && _value.Equals(value))
				return;
			ForceValueChange(value);
		} 
    }
    public override T GetValue() => Value;
     

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
    public override void Sub(UnityAction<T> action, bool callNow = true)
    {
        _onChangeSingle.AddListener(action);
        if (callNow)
            action.Invoke(_value);
    }

    public override void Sub(UnityAction<T, T> action, bool callNow = true)
    {
        _onChange.AddListener(action);
        if (callNow)
            action.Invoke(_prevValue, _value);
    }
    public override void SubToggle(UnityAction<T> action, bool sub)
    {
        if (sub)
            Sub(action);
        else Unsub(action);
    }
    public override void SubToggle(UnityAction<T, T> action, bool sub)
    {
        if (sub)
            Sub(action);
        else Unsub(action);
    }
    public override void Unsub(UnityAction<T, T> action)
    {
        _onChange.RemoveListener(action);
    }
    public override void Unsub(UnityAction<T> action)
    {
        _onChangeSingle.RemoveListener(action); 
    } 

}