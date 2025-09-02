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
        [SerializeField] int _current;
        public CamSettings DefaultSetting = new();
        public int CurrentIndex;
        public CamSettings Current
        {
            get
            { 
                _current = _current.RollIndex(AllCamSettings.Count);
                if (AllCamSettings.ValidIndex(_current))
                    return AllCamSettings[_current];

                return DefaultSetting;
            }
        }
    }
}
#endif
