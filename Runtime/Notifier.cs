using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
using UnityEngine.Events;
using System;


[Serializable, HideMonoScript, InlineProperty]
public class Notifier<T>
{
    public class ReadOnly
    {
        Notifier<T> _notifier;
        public ReadOnly(Notifier<T> notifier)
        { _notifier = notifier; }
        public T Value => _notifier.Value;
        public T PreviousValue => _notifier.PreviousValue; 
        public void Sub(UnityAction<T, T> action, bool callNow = true) => _notifier.Sub(action, callNow);
        public void SubToggle(UnityAction<T, T> action, bool sub) => _notifier.SubToggle(action, sub);
        public void Unsub(UnityAction<T, T> action) => _notifier.Sub(action);
        static public implicit operator T(ReadOnly w) => w.Value;
    }

    [NonSerialized] T _prevValue;
    [SerializeField, HideInInspector] T _value;
    [SerializeField, HideInEditorMode] UnityEvent<T, T> _onChange = new();
    [SerializeField, HideInEditorMode] UnityEvent<T> _onChangeSingle = new();
    public ReadOnly Readonly => new(this);
    public T PreviousValue => _prevValue;
    public int EstimatedListenerCount { get; private set; }
    [ShowInInspector, HideLabel, PropertyOrder(-10)]
#if ODIN_INSPECTOR
    [SuffixLabel("@" + nameof(EstimatedListenerCount), SdfIconType.EarFill)]
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
        EstimatedListenerCount++;
        if (callNow)
            action.Invoke(_value);
    }

    public void Sub(UnityAction<T, T> action, bool callNow = true)
    {
        EstimatedListenerCount++;
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
        EstimatedListenerCount--;
        _onChange.RemoveListener(action);
    }
    public void Unsub(UnityAction<T> action)
    {
        EstimatedListenerCount--;
        _onChangeSingle.RemoveListener(action);
    }
    static public implicit operator T(Notifier<T> w) => w.Value;
}