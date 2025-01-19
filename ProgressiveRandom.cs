using System;
using TriInspector;
using UnityEngine;

[Serializable]
public class ProgressiveRandom
{
    [Range(0, 1)] public float StartingChance = 0.2f;
    [Range(0, 1)] public float PowOnFailed = 0.1f;
    [Range(0, 1)] public float MaxChance = 1f;
    [NonSerialized, ReadOnly, ShowInInspector] float _curChance;

    [Button] public bool GetBool()
    {
        if (_curChance == 0)
            _curChance = StartingChance;

        if (UnityEngine.Random.value <= _curChance)
        {
            _curChance = StartingChance;
            return true;
        }

        _curChance = Mathf.Pow(_curChance, PowOnFailed);
        _curChance = Mathf.Min(_curChance, MaxChance);
        return false;
    }
}
