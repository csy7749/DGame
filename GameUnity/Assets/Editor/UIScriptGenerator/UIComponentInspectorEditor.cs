#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using DGame;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameLogic
{
    [CustomEditor(typeof(UIBindComponent))]
    public class UIComponentInspectorEditor : Editor
    {
        private UIBindComponent m_uiBindComponent;
        private ReorderableList m_reorderableList;
        private ReorderableList m_manifestList;
        private SerializedProperty m_componentsProperty;
        private SerializedProperty m_bindingEntriesProperty;
        private SerializedProperty m_useBindingManifestProperty;
        private SerializedProperty m_isWidgetRootProperty;
        private SerializedProperty m_manifestSignatureProperty;
        private SerializedProperty m_generatedSignatureProperty;
        private SerializedProperty m_className;
        private SerializedProperty m_uiType;
        private SerializedProperty m_dataTypeName;
        private SerializedProperty m_widgetTypeName;

        private List<UIGenType> m_uiGenTypes = new List<UIGenType>();
        private string[] m_uiTypeOptions;
        private int m_selectedIndex = 0;

        private void OnEnable()
        {
            m_uiBindComponent = (UIBindComponent)target;
            m_componentsProperty = serializedObject.FindProperty("m_components");
            m_bindingEntriesProperty = serializedObject.FindProperty("m_bindingEntries");
            m_useBindingManifestProperty = serializedObject.FindProperty("m_useBindingManifest");
            m_isWidgetRootProperty = serializedObject.FindProperty("m_isWidgetRoot");
            m_manifestSignatureProperty = serializedObject.FindProperty("m_manifestSignature");
            m_generatedSignatureProperty = serializedObject.FindProperty("m_generatedSignature");
            m_className = serializedObject.FindProperty("className");
            m_uiType = serializedObject.FindProperty("uiType");
            m_dataTypeName = serializedObject.FindProperty("dataTypeName");
            m_widgetTypeName = serializedObject.FindProperty("widgetTypeName");

            serializedObject.Update();
            if (string.IsNullOrEmpty(m_className.stringValue))
            {
                m_className.stringValue = target.name;
            }
            InitializeUIType();
            serializedObject.ApplyModifiedProperties();
            CreateReorderableList();
            CreateManifestList();
        }

        private void InitializeUIType()
        {
            if (!string.IsNullOrWhiteSpace(m_uiType.stringValue))
            {
                return;
            }

            string defaultType = m_uiBindComponent.IsWidgetRoot ? nameof(UIWidget) : nameof(UIWindow);
            if (UIScriptGeneratorSettings.GetUIGenType(defaultType) != null)
            {
                m_uiType.stringValue = defaultType;
            }
        }

        private void CreateManifestList()
        {
            m_manifestList = new ReorderableList(serializedObject, m_bindingEntriesProperty, false, true, false, false);
            m_manifestList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "绑定清单");
            m_manifestList.elementHeightCallback = index =>
            {
                SerializedProperty element = m_bindingEntriesProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 4f;
            };
            m_manifestList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = m_bindingEntriesProperty.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUI.GetPropertyHeight(element, true);
                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };
            m_manifestList.drawNoneElementCallback = rect =>
                EditorGUI.LabelField(rect, "选中 Prefab 节点后，通过逐组件 +/- 建立绑定。");
        }

        private void CreateReorderableList()
        {
            m_reorderableList = new ReorderableList(serializedObject, m_componentsProperty, true, true, true, true);
            m_reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                float width = rect.width - 20;
                float indexWidth = 90f;
                float nameWidth = 150f;
                float componentWidth = width - indexWidth - nameWidth - 15f;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, indexWidth, rect.height), "序号");
                EditorGUI.LabelField(new Rect(rect.x + indexWidth, rect.y, nameWidth, rect.height), "对象名称");
                EditorGUI.LabelField(new Rect(rect.x + indexWidth + nameWidth, rect.y, componentWidth, rect.height),
                    "组件引用");
            };

            m_reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = m_componentsProperty.GetArrayElementAtIndex(index);
                Component component = element.objectReferenceValue as Component;

                float height = EditorGUIUtility.singleLineHeight;
                float padding = 2f;
                float indexWidth = 70f;
                float nameWidth = 150f;
                float componentWidth = rect.width - indexWidth - nameWidth - 10f;

                // 序号（不可编辑）
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(new Rect(rect.x, rect.y + padding, indexWidth, height), $"【{index}】");
                EditorGUI.EndDisabledGroup();

                // 对象名称（不可编辑）
                EditorGUI.BeginDisabledGroup(true);
                string objectName = component != null ? component.gameObject.name : "Null Reference";
                EditorGUI.TextField(new Rect(rect.x + indexWidth, rect.y + padding, nameWidth, height), objectName);
                EditorGUI.EndDisabledGroup();

                // 组件引用（可编辑）
                EditorGUI.PropertyField(
                    new Rect(rect.x + indexWidth + nameWidth + 8, rect.y + padding, componentWidth, height),
                    element, GUIContent.none);
            };

            m_reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
            m_reorderableList.onAddCallback = (ReorderableList list) =>
            {
                m_componentsProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };
            m_reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (list.index >= 0 && list.index < m_componentsProperty.arraySize)
                {
                    m_componentsProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
            m_reorderableList.drawNoneElementCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "列表为空 - 点击上方重新绑定进行组件重绑");
            };
        }

        private void RemoveComponentAtIndex(int index)
        {
            if (index >= 0 && index < m_componentsProperty.arraySize)
            {
                m_componentsProperty.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();

                // 重新选择相邻的元素，避免选择丢失
                if (m_reorderableList.index >= m_componentsProperty.arraySize)
                {
                    m_reorderableList.index = m_componentsProperty.arraySize - 1;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (m_uiBindComponent.IsManifestBinding)
            {
                DrawManifestInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawManifestMigrationButton();
            DrawTopButtons();
            EditorGUILayout.Space();
            m_reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawManifestMigrationButton()
        {
            EditorGUILayout.HelpBox("当前 Prefab 仍使用旧组件索引绑定。启用 Manifest 后，请在 Prefab Stage 逐节点添加绑定。", MessageType.Info);
            if (!GUILayout.Button("启用 Manifest", GUILayout.Height(24)))
            {
                return;
            }

            Undo.RecordObject(m_uiBindComponent, "启用 UI Binding Manifest");
            m_uiBindComponent.EnableBindingManifest();
            m_useBindingManifestProperty.boolValue = true;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_uiBindComponent);
        }

        private void DrawManifestInspector()
        {
            EditorGUILayout.HelpBox("Manifest 是该 UI 的唯一绑定来源。请在 Prefab Stage 选中节点，通过每个组件独立的 +/- 修改清单。", MessageType.Info);
            string autoTypeName = string.IsNullOrWhiteSpace(m_className.stringValue) ? target.name + "Auto" : m_className.stringValue + "Auto";
            EditorGUILayout.HelpBox($"继承式生成：自动类为 {autoTypeName}，业务类请声明为 {m_className.stringValue} : {autoTypeName}。", MessageType.None);
            DrawWidgetRootProperty();
            DrawManifestSignatureStatus();
            DrawManifestGenerateButton();
            DrawCodeGenerationSettings();
            EditorGUILayout.Space();
            m_manifestList.DoLayoutList();
        }

        private void DrawWidgetRootProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_isWidgetRootProperty, new GUIContent("Widget 根节点"));
            if (EditorGUI.EndChangeCheck())
            {
                m_manifestSignatureProperty.stringValue = string.Empty;
            }
        }

        private void DrawManifestSignatureStatus()
        {
            bool isCurrent = !string.IsNullOrEmpty(m_manifestSignatureProperty.stringValue)
                && m_manifestSignatureProperty.stringValue == m_generatedSignatureProperty.stringValue;
            string state = isCurrent ? "已同步" : "已陈旧";
            EditorGUILayout.LabelField("生成状态", state);
            EditorGUILayout.LabelField("Manifest签名", m_manifestSignatureProperty.stringValue);
            EditorGUILayout.LabelField("代码签名", m_generatedSignatureProperty.stringValue);
        }

        private void DrawManifestGenerateButton()
        {
            if (GUILayout.Button("重新收集绑定", GUILayout.Height(24)))
            {
                serializedObject.ApplyModifiedProperties();
                if (UIScriptGenerator.RecollectManifestBindings(m_uiBindComponent))
                {
                    serializedObject.Update();
                }
            }

            if (GUILayout.Button("校验并生成 Manifest 代码", GUILayout.Height(26)))
            {
                serializedObject.ApplyModifiedProperties();
                UIScriptGenerator.GenerateManifestCSharpScript(m_uiBindComponent, false);
            }
        }

        private void DrawCodeGenerationSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("代码生成设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_className, new GUIContent("类名"));
            bool isBothGeneric = DrawUITypePopup();
            if (isBothGeneric)
            {
                EditorGUILayout.PropertyField(m_widgetTypeName, new GUIContent("Widget类型"));
                EditorGUILayout.PropertyField(m_dataTypeName, new GUIContent("数据类型"));
            }
            DrawProjectCodePaths();
        }

        private bool DrawUITypePopup()
        {
            m_uiGenTypes = UIScriptGeneratorSettings.Instance.UIGenTypes;
            m_uiTypeOptions = m_uiGenTypes.Select(type => type.uiTypeName).ToArray();
            if (m_uiTypeOptions.Length == 0)
            {
                EditorGUILayout.HelpBox("请先在 UISettings 中配置 UI 类型。", MessageType.Error);
                return false;
            }

            int currentIndex = System.Array.IndexOf(m_uiTypeOptions, m_uiType.stringValue);
            if (currentIndex < 0 && !string.IsNullOrWhiteSpace(m_uiType.stringValue))
            {
                EditorGUILayout.HelpBox($"UI 类型 {m_uiType.stringValue} 未在 UISettings 中配置。", MessageType.Error);
            }

            int newIndex = EditorGUILayout.Popup("UI类型", currentIndex, m_uiTypeOptions);
            if (newIndex >= 0 && newIndex != currentIndex)
            {
                m_uiType.stringValue = m_uiTypeOptions[newIndex];
                m_manifestSignatureProperty.stringValue = string.Empty;
            }

            return newIndex >= 0 && UIScriptGeneratorSettings.GetUIGenType(m_uiTypeOptions[newIndex]).bothGeneric;
        }

        private void DrawTopButtons()
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("重新绑定组件", GUILayout.Height(25)))
                {
                    RebindComponents();
                }
                var bothGeneric = UIScriptGeneratorSettings.GetUIGenType(m_uiType.stringValue)?.bothGeneric ?? false;
                if (GUILayout.Button("自动生成脚本", GUILayout.Height(25)))
                {
                    if (bothGeneric)
                    {
                        if (string.IsNullOrWhiteSpace(m_widgetTypeName.stringValue) || string.IsNullOrWhiteSpace(m_dataTypeName.stringValue))
                        {
                            UnityEngine.Debug.LogError($"如果选择UI类型为: {m_uiType.stringValue} 则Widget的类型或者Data的数据类型不能为空。请检查{target.name}的UIBindComponent的设置！");
                            return;
                        }
                    }
                    // RemoveNullComponents();
                    UIScriptGenerator.GenerateCSharpScript(true, false, true,
                        UIScriptGeneratorSettings.GetGenCodePath(), m_className.stringValue,
                        m_uiTypeOptions[m_selectedIndex], m_widgetTypeName.stringValue,
                        m_dataTypeName.stringValue);
                }
                if (GUILayout.Button("自动生成UniTask脚本", GUILayout.Height(25)))
                {
                    if (bothGeneric)
                    {
                        if (string.IsNullOrWhiteSpace(m_widgetTypeName.stringValue) || string.IsNullOrWhiteSpace(m_dataTypeName.stringValue))
                        {
                            UnityEngine.Debug.LogError($"如果选择UI类型为: {m_uiType.stringValue} 则Widget的类型或者Data的数据类型不能为空。请检查{target.name}的UIBindComponent的设置！");
                            return;
                        }
                    }
                    // RemoveNullComponents();
                    UIScriptGenerator.GenerateCSharpScript(true, true, true,
                        UIScriptGeneratorSettings.GetGenCodePath(), m_className.stringValue,
                        m_uiTypeOptions[m_selectedIndex], m_widgetTypeName.stringValue,
                        m_dataTypeName.stringValue);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("生成标准版粘贴板代码", GUILayout.Height(25)))
                {
                    UIScriptGenerator.GenerateCSharpScript(false);
                }
                if (GUILayout.Button("生成UniTask粘贴板代码", GUILayout.Height(25)))
                {
                    UIScriptGenerator.GenerateCSharpScript(false, true);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("HelpBox");
            {
                // 绘制序列化属性字段
                EditorGUILayout.LabelField("代码生成设置", EditorStyles.boldLabel);

                bool isBothGeneric = false;
                m_uiGenTypes = UIScriptGeneratorSettings.Instance.UIGenTypes;
                // 获取所有的 uiTypeName
                m_uiTypeOptions = m_uiGenTypes.Select(t => t.uiTypeName).ToArray();
                // 确保有选项时才显示 Popup
                if (m_uiTypeOptions.Length > 0)
                {
                    // m_selectedIndex = EditorGUILayout.Popup("UI类型", m_selectedIndex, m_uiTypeOptions);
                    // 优先根据 uiType(字符串) 还原选择，避免规则顺序变化导致 index 错位
                    if (m_uiType != null && !string.IsNullOrEmpty(m_uiType.stringValue))
                    {
                        int indexByName = System.Array.IndexOf(m_uiTypeOptions, m_uiType.stringValue);
                        if (indexByName >= 0)
                        {
                            m_selectedIndex = indexByName;
                        }
                    }

                    // index 越界保护（规则数量变更/配置变更）
                    m_selectedIndex = Mathf.Clamp(m_selectedIndex, 0, m_uiTypeOptions.Length - 1);

                    int newIndex = EditorGUILayout.Popup("UI类型", m_selectedIndex, m_uiTypeOptions);
                    if (newIndex != m_selectedIndex)
                    {
                        m_selectedIndex = newIndex;
                    }

                    if (m_uiType != null)
                    {
                        string selectedTypeName = m_uiTypeOptions[m_selectedIndex];
                        if (m_uiType.stringValue != selectedTypeName)
                        {
                            m_uiType.stringValue = selectedTypeName;
                        }
                        isBothGeneric = UIScriptGeneratorSettings.GetUIGenType(selectedTypeName)?.bothGeneric ?? false;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("请先去UISetting中设置UI类型规则", MessageType.Info);
                }
                // EditorGUILayout.PropertyField(m_uiType, new GUIContent("UI类型"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_className, new GUIContent("类名"));
                if(GUILayout.Button("物体名", GUILayout.Width(60), GUILayout.Height(18)))
                {
                    m_className.stringValue = target.name;
                }
                EditorGUILayout.EndHorizontal();

                if (isBothGeneric)
                {
                    EditorGUILayout.PropertyField(m_widgetTypeName, new GUIContent("Widget类型"));
                    EditorGUILayout.PropertyField(m_dataTypeName, new GUIContent("数据类型"));
                }

                DrawProjectCodePaths();

            }
            EditorGUILayout.EndVertical();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void RebindComponents()
        {
            if (m_uiBindComponent == null) return;
            m_uiBindComponent.Clear();
            UIScriptGenerator.GenerateUIComponentScript();
            UIScriptGenerator.GenerateCSharpScript(false);
        }

        private void RemoveNullComponents()
        {
            if (m_uiBindComponent == null) return;

            for (int i = m_componentsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty element = m_componentsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null)
                {
                    m_componentsProperty.DeleteArrayElementAtIndex(i);
                }
            }
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"已清除空引用，剩余组件数量: {m_componentsProperty.arraySize}");
        }

        /// <summary>
        /// 路径由项目配置统一管理，Prefab 只保留自身的类型与生成行为。
        /// </summary>
        private static void DrawProjectCodePaths()
        {
            EditorGUILayout.LabelField("项目代码生成路径", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("组件代码", UIScriptGeneratorSettings.GetGenCodePath());
            string logicRoot = UIScriptGeneratorSettings.GetImpCodePath();
            EditorGUILayout.LabelField("Window逻辑", $"{logicRoot}/<WindowName>/<WindowName>.cs");
            EditorGUILayout.LabelField("Item逻辑", $"{logicRoot}/Item/<ItemName>.cs");
            if (GUILayout.Button("打开项目配置", GUILayout.Height(22)))
            {
                DGameUIGeneratorSettingsProvider.OpenUIGeneratorSettings();
            }
        }
    }
#endif
}
