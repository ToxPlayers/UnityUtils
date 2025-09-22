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
	public float MaxHP { get; }
	public float HPValue { get; } 
	public float HPNormalized { get; }
	public void Damage(float dmg);
    public void Heal(float heal);
}

public class Health : MonoBehaviour, IHealth
{    
	[SerializeField, Min(1f)] float _maxHP = 100f;
    [ShowInInspector, Range(0f, 1f), PropertyOrder(-200)]
    public float HPNormalized => HPValue / MaxHP;
    [ShowInInspector, ReadOnly, HideInEdit] float _health;
    public float MaxHP => _maxHP;
    public float HPValue => _health;
	public bool IsAlive => _health > 0;
	public bool IsDead => !IsAlive;

	[FoldoutGroup("Events"), PropertyOrder(20)]
    public UnityEvent<float> OnHPChange, OnMaxHPChange, OnDamage, OnHeal;
    [FoldoutGroup("Events")]
    public UnityEvent OnRevive, OnDeath;

	protected virtual void OnEnable()
	{
		if (_maxHP <= 0f)
			Debug.LogError("Max hp is zero", this);

		_health = 0;
		Heal(_maxHP);
	}

	
    public virtual void SetMaxHP(float maxHp)
	{
		_maxHP = Mathf.Max(0, maxHp);
		OnHPChange.Invoke(_health);
		OnMaxHPChange.Invoke(_maxHP);
	}
	
    public virtual void Damage(float dmg)
	{
		if (IsDead)
			return;
		if(dmg < 0f)
		{
			Debug.LogError("Damage is negative");
			return;
		}
		if (dmg == 0f)
			return;
		
		var newHP = _health - dmg;
		newHP = Mathf.Max(newHP, 0f);
		_health = newHP;
		OnHPChange?.Invoke(-dmg); 
		OnDamage?.Invoke(dmg);
		
		if (!IsDead) 
			return;
		OnDeath?.Invoke();
	}

    public virtual void Heal(float heal)
	{
		if(heal < 0)
		{
			Debug.LogError("Heal is negative");
			return;
		}

		var wasDead = IsDead;

		_health += heal;
		_health = Mathf.Min(_health, _maxHP);

		if (wasDead && ! IsDead)
			OnRevive?.Invoke();
		OnHPChange?.Invoke(heal);
		OnHeal?.Invoke(heal); 
	}
} 
