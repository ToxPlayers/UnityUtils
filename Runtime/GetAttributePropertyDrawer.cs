using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor; 
#endif
namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class GetAttribute : PropertyAttribute 
    { 
        public bool Required { get; set; }
        public GetAttribute() { }
        public GetAttribute(bool required) => Required = required; 
    }
    public sealed class GetChildAttribute : GetAttribute { }
    public sealed class GetParentAttribute : GetAttribute { }
#if UNITY_EDITOR
    public abstract class GetAttributePropertyDrawerBase : PropertyDrawer
    {
        static Texture ErrorIcon = EditorGUIUtility.IconContent("console.erroricon").image;
        static Texture WarnIcon = EditorGUIUtility.IconContent("console.warnicon").image;
        static GUIStyle ErrorLabelStyle = new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(26, 4, 4, 4),
            fontSize = 10,
            alignment = TextAnchor.MiddleLeft,
            richText = true,
            wordWrap = true
        };
        static readonly Color ClassColor = new Color(0.3f, 0.78f, 0.7f);

        const float IconMargin = 3;
        const float BoxMargin = 5;

        public abstract Component FindComponentAction(GameObject go, Type type);
        
        static public string SetColor(string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }

        void DrawDefault(Rect rect, SerializedProperty property, GUIContent label)
        { 
            EditorGUI.PropertyField(rect, property, label, true);
            if (property.serializedObject.hasModifiedProperties)
                property.serializedObject.ApplyModifiedProperties();
        }
        protected bool ShouldntFindComp(SerializedProperty property, out string invalidErrors, out GameObject ownerGo)
        {
            ownerGo = null;
            invalidErrors = null;
            var wrongPropType = property.propertyType != SerializedPropertyType.ObjectReference;

            if (Application.isPlaying || (!wrongPropType && property.objectReferenceValue != null))
                return true;

            var targetObj = property.serializedObject.targetObject;
            var comp = targetObj as Component;
            if (!comp)
                invalidErrors = $"Only component class {targetObj.GetType().Name} with {nameof(GetAttribute)}"
                                    + " must be a gameobject";
            ownerGo = comp.gameObject;
            var fieldType = fieldInfo.FieldType;
            var isFieldTypeWrong = !typeof(Component).IsAssignableFrom(fieldType);
            if (isFieldTypeWrong)
                invalidErrors = SetColor(fieldType.Name, ClassColor) + $" is not a component type";

            return invalidErrors != null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (_msgShown ? _msgRectHeight + 4 : 0);
        }

        float _msgRectHeight;
        void DrawMsgBox(Rect rect, SerializedProperty property, string msg, bool isError)
        {
            _msgShown = true;

            var guiLabel = GUI.skin.textArea;
            guiLabel.richText = true;

            _msgRectHeight = ErrorLabelStyle.CalcHeight(new GUIContent(msg), rect.width);

            var boxRect = new Rect();
            boxRect.height = _msgRectHeight;
            boxRect.width = rect.width;
            boxRect.position = new Vector2(rect.x, rect.y + _msgRectHeight);

            var iconRect = new Rect();
            iconRect.size = (_msgRectHeight - IconMargin) * Vector2.one;
            iconRect.position = boxRect.position + Vector2.one * IconMargin;

            var prevGUIColor = GUI.color;
            GUI.color = isError ? Color.red : Color.yellow;
            GUI.Label(boxRect, string.Empty, EditorStyles.helpBox);
            GUI.color = prevGUIColor;

            var fullMsg = GetAttributeDisplayName();
            fullMsg += isError ? " Error: " : " Warning: ";
            fullMsg += msg;
            GUI.Label(boxRect, fullMsg, ErrorLabelStyle);
            GUI.DrawTexture(iconRect, isError ? ErrorIcon : WarnIcon, ScaleMode.ScaleToFit);
        }

        bool _msgShown; 

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            _msgShown = false;
            _msgRectHeight = 0; 
            if (ShouldntFindComp(property, out string error, out GameObject ownerGo))
            {
                if (error != null)
                    DrawMsgBox(rect, property, error, true);
                DrawDefault(rect, property, label);
                return;
            }  

            bool isPropertyValueNull = property.objectReferenceValue == null;
            if (isPropertyValueNull)
            {
                var foundComponent = FindComponentAction(ownerGo, fieldInfo.FieldType);
                if (foundComponent)
                    property.objectReferenceValue = foundComponent;
                else if (attribute is GetAttribute attr && attr.Required)
                {
                    var warnMsg = $"Failed to find ";
                    warnMsg += SetColor(fieldInfo.FieldType.Name, ClassColor);
                    DrawMsgBox(rect, property, warnMsg, false);
                }
            } 
            
            if(property.serializedObject.hasModifiedProperties)
                property.serializedObject.ApplyModifiedProperties();
             

            EditorGUI.PropertyField(rect, property, label, true);
        }

        string GetAttributeDisplayName()
        {
            var displayFunc = attribute.GetType().Name.Replace("Attribute", "Component");
            return SetColor($"[{displayFunc}]", ClassColor);
        }
    }

    [CustomPropertyDrawer(typeof(GetAttribute))]
    public class GetAttributeDrawer : GetAttributePropertyDrawerBase
    {
        public override Component FindComponentAction(GameObject go, Type type)
        { 
            return go.GetComponent(type);
        } 
    }

    [CustomPropertyDrawer(typeof(GetChildAttribute))]
    public class GetChildAttributeDrawer : GetAttributePropertyDrawerBase
    {
        public override Component FindComponentAction(GameObject go, Type type)
        {
            return go.GetComponentInChildren(type);
        }
    }

    [CustomPropertyDrawer(typeof(GetParentAttribute))]
    public class GetParentAttributeDrawer : GetAttributePropertyDrawerBase
    {
        public override Component FindComponentAction(GameObject go, Type type)
        {
            return go.GetComponentInParent(type);
        }
    }

#endif


}
