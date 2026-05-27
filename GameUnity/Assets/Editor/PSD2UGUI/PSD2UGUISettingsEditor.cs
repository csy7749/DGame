#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DGame.PSD2UGUI
{
    /// <summary>
    /// PSD2UGUISettings 的自定义 Inspector：
    /// 文件夹字段改为 DefaultAsset ObjectField（只接受 Project 中的文件夹）；
    /// 字体路径改为 Font ObjectField。
    /// </summary>
    [CustomEditor(typeof(PSD2UGUISettings))]
    public class PSD2UGUISettingsEditor : Editor
    {
        private static readonly HashSet<string> FolderArrayProps = new HashSet<string>
        {
            "spriteSearchFolders",
            "textureSearchFolders",
        };

        /// <summary>
        /// 字段名 → 中文显示名
        /// </summary>
        private static readonly Dictionary<string, string> LabelMap = new Dictionary<string, string>
        {
            { "textComponentTypeName", "文本组件名" },
            { "imageComponentTypeName", "图片组件名" },
            { "buttonComponentTypeName", "按钮组件名" },
            { "rawImageComponentTypeName", "RawImage 组件名" },
            { "shadowComponentTypeName", "阴影组件名" },
            { "outlineComponentTypeName", "描边组件名" },
            { "gradientComponentTypeName", "渐变组件名" },
            { "spriteSearchFolders", "Sprite 查找路径" },
            { "textureSearchFolders", "Texture 查找路径" },
            { "defaultFontPath", "默认字体" },
            { "fontPaths", "字体映射" },
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var iter = serializedObject.GetIterator();
            iter.NextVisible(true); // m_Script
            while (iter.NextVisible(false))
            {
                if (FolderArrayProps.Contains(iter.name))
                {
                    DrawFolderArray(iter);
                }
                else if (iter.name == "defaultFontPath")
                {
                    DrawAssetPathField<Font>(iter, GetLabel(iter));
                }
                else if (iter.name == "fontPaths")
                {
                    DrawFontPathList(iter);
                }
                else if (iter.name == "resolution")
                {
                    EditorGUILayout.PropertyField(iter, GUIContent.none, true);
                }
                else
                {
                    EditorGUILayout.PropertyField(iter, GetLabel(iter), true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private static GUIContent GetLabel(SerializedProperty prop)
        {
            string label = LabelMap.TryGetValue(prop.name, out var cn) ? cn : prop.displayName;
            return new GUIContent(label, prop.tooltip);
        }

        private void DrawFolderArray(SerializedProperty prop)
        {
            EditorGUILayout.LabelField(GetLabel(prop), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            int removeIdx = -1;
            for (int i = 0; i < prop.arraySize; i++)
            {
                var el = prop.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                DefaultAsset folder = string.IsNullOrEmpty(el.stringValue)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<DefaultAsset>(el.stringValue);

                EditorGUI.BeginChangeCheck();
                var newFolder = (DefaultAsset)EditorGUILayout.ObjectField($"路径 {i}", folder, typeof(DefaultAsset), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newFolder == null)
                    {
                        el.stringValue = "";
                    }
                    else
                    {
                        string path = AssetDatabase.GetAssetPath(newFolder);
                        if (AssetDatabase.IsValidFolder(path))
                        {
                            el.stringValue = path;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("无效选择", "请选择一个文件夹", "确定");
                        }
                    }
                }
                if (GUILayout.Button("-", GUILayout.Width(22)))
                {
                    removeIdx = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx >= 0)
            {
                prop.DeleteArrayElementAtIndex(removeIdx);
            }

            if (GUILayout.Button("+ 添加文件夹"))
            {
                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).stringValue = "";
            }
            EditorGUI.indentLevel--;
        }

        private void DrawAssetPathField<T>(SerializedProperty prop, GUIContent label) where T : Object
        {
            var current = string.IsNullOrEmpty(prop.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<T>(prop.stringValue);
            EditorGUI.BeginChangeCheck();
            var newObj = (T)EditorGUILayout.ObjectField(label, current, typeof(T), false);
            if (EditorGUI.EndChangeCheck())
            {
                prop.stringValue = newObj == null ? "" : AssetDatabase.GetAssetPath(newObj);
            }
        }

        private void DrawFontPathList(SerializedProperty prop)
        {
            EditorGUILayout.LabelField(GetLabel(prop) + " (PSD 字体名 → Unity Font)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            int removeIdx = -1;
            for (int i = 0; i < prop.arraySize; i++)
            {
                var el = prop.GetArrayElementAtIndex(i);
                var nameProp = el.FindPropertyRelative("fontName");
                var pathProp = el.FindPropertyRelative("assetPath");

                EditorGUILayout.BeginHorizontal();
                nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue, GUILayout.MinWidth(80));

                var current = string.IsNullOrEmpty(pathProp.stringValue)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<Font>(pathProp.stringValue);
                EditorGUI.BeginChangeCheck();
                var newFont = (Font)EditorGUILayout.ObjectField(current, typeof(Font), false);
                if (EditorGUI.EndChangeCheck())
                {
                    pathProp.stringValue = newFont == null ? "" : AssetDatabase.GetAssetPath(newFont);
                }
                if (GUILayout.Button("-", GUILayout.Width(22))) removeIdx = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx >= 0) prop.DeleteArrayElementAtIndex(removeIdx);

            if (GUILayout.Button("+ 添加字体映射"))
            {
                prop.arraySize++;
                var el = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                el.FindPropertyRelative("fontName").stringValue = "";
                el.FindPropertyRelative("assetPath").stringValue = "";
            }
            EditorGUI.indentLevel--;
        }
    }
}
#endif
