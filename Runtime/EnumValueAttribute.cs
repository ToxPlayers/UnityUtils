using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EnumValueAttribute : PropertyAttribute { public EnumValueAttribute() { } }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumValueAttribute))]
public class EnumValueAttributeDrawer : PropertyDrawer {
	public override VisualElement CreatePropertyGUI(SerializedProperty property) {
		return base.CreatePropertyGUI(property);
	}
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {

		var intValue = _property.intValue;
		var enu = EditorGUILayout.EnumPopup(_label, (Enum)Enum.ToObject(fieldInfo.FieldType, intValue));
		var newInt = enu.AsInt();
		if(newInt != intValue)
			_property.intValue = newInt;
	}
}
#endif
