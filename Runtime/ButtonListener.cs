using UnityEngine;
using UnityEngine.UI; 
public abstract class ButtonListener : MonoBehaviour
{
    [SerializeField, Get] public Button Btn;
    protected virtual void Awake()
    {
        Btn.onClick.AddListener(OnClick);
    }
    public abstract void OnClick();
} 
