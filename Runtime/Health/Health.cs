using UnityEngine;
using TriInspector;
using UnityEngine.Events;
 
public class Health : MonoBehaviour
{    
	[SerializeField, Min(1f)] public float MaxHP = 100f;
	[ShowInInspector, ReadOnly, HideInEditMode] float _health;
	public float HPValueNormalized => _health / MaxHP;
	public float HPValue => _health;
	public bool IsAlive => _health > 0;
	public bool IsDead => !IsAlive;
	public UnityEvent<float> OnHPChange, OnDamage, OnHeal;
	public UnityEvent OnRevive, OnDeath;

	protected virtual void OnEnable()
	{
		if (MaxHP <= 0f)
			Debug.LogError("Max hp is zero", this);

		_health = 0;
		Heal(MaxHP);
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
		_health = Mathf.Min(_health, MaxHP);

		if (wasDead && ! IsDead)
			OnRevive?.Invoke();
		OnHPChange?.Invoke(heal);
		OnHeal?.Invoke(heal); 
	}
} 
