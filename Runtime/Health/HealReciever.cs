using UnityEngine;

public class HealReciever : MonoBehaviour
{
	[SerializeField, GetParent] Health _hp;
	public Health HP => _hp;
	public virtual void RecieveHealth(float hpGain)
	{
		_hp.Heal(hpGain);
	}
}
