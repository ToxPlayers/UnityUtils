using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
#if ODIN_INSPECTOR
using HideInEdit = Sirenix.OdinInspector.HideInEditorModeAttribute;
#else 
using HideInEdit = TriInspector.HideInEditModeAttribute;
#endif
using UnityEngine.Events;

public interface IHealth 
{ 
	public float MaxValue { get; }
	public float Value { get; } 
    public void Damage(float dmg );
    public void Heal(float heal, bool overheal = false);
}
