#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace GameLogic
{
    [CustomPropertyDrawer(typeof(UIBindingEntry))]
    public class UIBindingEntryDrawer : PropertyDrawer
    {
        private const int BASE_LINE_COUNT = 6;
        private const int EVENT_LINE_COUNT = 2;
        private const int WIDGET_LINE_COUNT = 1;
        private const float ELEMENT_PADDING = 6f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty generateEvent = property.FindPropertyRelative("m_generateUnityEvent");
            SerializedProperty kind = property.FindPropertyRelative("m_kind");
            int lineCount = BASE_LINE_COUNT + (generateEvent.boolValue ? EVENT_LINE_COUNT : 0)
                + (kind.enumValueIndex == (int)UIBindingKind.Widget ? WIDGET_LINE_COUNT : 0);
            float contentHeight = lineCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            return contentHeight + ELEMENT_PADDING * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            Rect line = new Rect(
                position.x + ELEMENT_PADDING,
                position.y + ELEMENT_PADDING,
                position.width - ELEMENT_PADDING * 2f,
                EditorGUIUtility.singleLineHeight);
            DrawReadOnlyProperty(ref line, property.FindPropertyRelative("m_bindingId"), "Binding ID");
            DrawProperty(ref line, property.FindPropertyRelative("m_fieldName"), "字段名");
            DrawReadOnlyProperty(ref line, property.FindPropertyRelative("m_target"), "目标组件");
            DrawReadOnlyProperty(ref line, property.FindPropertyRelative("m_expectedTypeName"), "预期类型");
            DrawReadOnlyProperty(ref line, property.FindPropertyRelative("m_kind"), "绑定类型");
            DrawWidgetProperty(ref line, property);
            DrawProperty(ref line, property.FindPropertyRelative("m_generateUnityEvent"), "生成UnityEvent");
            DrawEventProperties(ref line, property);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.FindProperty("m_manifestSignature").stringValue = string.Empty;
            }
            EditorGUI.EndProperty();
        }

        private static void DrawEventProperties(ref Rect line, SerializedProperty property)
        {
            if (!property.FindPropertyRelative("m_generateUnityEvent").boolValue)
            {
                return;
            }

            DrawProperty(ref line, property.FindPropertyRelative("m_eventKind"), "事件类型");
            DrawProperty(ref line, property.FindPropertyRelative("m_eventHandlerName"), "处理器");
        }

        private static void DrawWidgetProperty(ref Rect line, SerializedProperty property)
        {
            SerializedProperty kind = property.FindPropertyRelative("m_kind");
            if (kind.enumValueIndex == (int)UIBindingKind.Widget)
            {
                DrawReadOnlyProperty(ref line, property.FindPropertyRelative("m_widgetTypeName"), "Widget类型");
            }
        }

        private static void DrawReadOnlyProperty(ref Rect line, SerializedProperty property, string label)
        {
            EditorGUI.BeginDisabledGroup(true);
            DrawProperty(ref line, property, label);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawProperty(ref Rect line, SerializedProperty property, string label)
        {
            EditorGUI.PropertyField(line, property, new GUIContent(label));
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}

#endif
