using UnityEngine;
using UnityEngine.UI; 
public abstract class ButtonListener : MonoBehaviour
{
    protected Button _btn;
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);
    }
    public abstract void OnClick();
} 
