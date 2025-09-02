#if ODIN_INSPECTOR && UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorIconer
{
    [CreateAssetMenu(fileName = IconerEditorWindow.CamSettingsCollectionName, menuName = "EditorCamSettings")]
    [Serializable]
    public class CamSettingsCollection : ScriptableObject
    {
        public List<CamSettings> AllCamSettings = new();
        [SerializeField] int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => _currentIndex = value.RollIndex(AllCamSettings.Count); 
        }  

        private void OnValidate()
        {
            if (AllCamSettings.Count == 0)
                AllCamSettings.Add(new() { CamSettingName = "Default" });
        }
        public bool IsDefault => _currentIndex == 0;
        public CamSettings Current
        {
            get
            {
                OnValidate(); 
                CurrentIndex = CurrentIndex.RollIndex(AllCamSettings.Count);
                return AllCamSettings[CurrentIndex];
            }
        }
    }
}
#endif
