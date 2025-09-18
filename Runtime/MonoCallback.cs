using UnityEngine;
using UnityEngine.Events;
 
public class MonoCallback : MonoBehaviour
{
    public UnityEvent OnDisabled = new();
    private void OnDisable()
    {
        OnDisabled.Invoke();
    }
} 