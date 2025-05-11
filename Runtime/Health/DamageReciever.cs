using UnityEngine;
using TriInspector;
using UnityEngine.Events;

public class DamageReciever : MonoBehaviour
{     
	[SerializeField, GetParent] Health _hp;
	public Health HP => _hp;

	public virtual void RecieveDamage(float dmg)
	{
		_hp.Damage(dmg);
	}
} 
