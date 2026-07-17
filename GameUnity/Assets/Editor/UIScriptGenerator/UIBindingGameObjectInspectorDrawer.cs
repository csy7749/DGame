using System.Collections.Generic;
using GameLogic;
using UnityEditor;
using UnityEngine;

namespace DGame
{
    [InitializeOnLoad]
    public static class UIBindingGameObjectInspectorDrawer
    {
        static UIBindingGameObjectInspectorDrawer()
        {
            Editor.finishedDefaultHeaderGUI -= DrawBindingInspector;
            Editor.finishedDefaultHeaderGUI += DrawBindingInspector;
        }

        private static void DrawBindingInspector(Editor editor)
        {
            if (editor.targets.Length != 1 || editor.target is not GameObject gameObject)
            {
                return;
            }

            List<UIBindingNodeOption> options = UIScriptGenerator.GetNodeBindingOptions(gameObject);
            if (options.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("UI 绑定", EditorStyles.boldLabel);
            DrawOptions(options);
            EditorGUILayout.Space(4f);
        }

        private static void DrawOptions(IReadOnlyList<UIBindingNodeOption> options)
        {
            UIBindComponent currentOwner = null;
            foreach (UIBindingNodeOption option in options)
            {
                if (option.Owner != currentOwner)
                {
                    currentOwner = option.Owner;
                    DrawOwner(currentOwner);
                }

                DrawOption(option);
            }
        }

        private static void DrawOwner(UIBindComponent owner)
        {
            string ownerName = string.IsNullOrWhiteSpace(owner.ClassName) ? owner.name : owner.ClassName;
            EditorGUILayout.LabelField($"归属：{ownerName}", EditorStyles.miniBoldLabel);
        }

        private static void DrawOption(UIBindingNodeOption option)
        {
            UIBindingEntry entry = UIScriptGenerator.FindNodeBinding(option);
            EditorGUILayout.BeginHorizontal();
            DrawFieldName(option, entry);
            DrawAddButton(option);
            DrawRemoveButton(option, entry);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawFieldName(UIBindingNodeOption option, UIBindingEntry entry)
        {
            if (entry == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(option.SuggestedFieldName, GUILayout.MinWidth(160f));
                EditorGUI.EndDisabledGroup();
                return;
            }

            EditorGUI.BeginChangeCheck();
            string fieldName = EditorGUILayout.TextField(entry.FieldName, GUILayout.MinWidth(160f));
            if (EditorGUI.EndChangeCheck())
            {
                UIScriptGenerator.RenameNodeBinding(option, fieldName);
            }
        }

        private static void DrawAddButton(UIBindingNodeOption option)
        {
            bool configured = UIScriptGenerator.IsNodeBindingConfigured(option);
            EditorGUI.BeginDisabledGroup(configured);
            if (GUILayout.Button($"+ {option.DisplayTypeName}", GUILayout.MinWidth(100f)))
            {
                UIScriptGenerator.AddNodeBinding(option);
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawRemoveButton(UIBindingNodeOption option, UIBindingEntry entry)
        {
            EditorGUI.BeginDisabledGroup(entry == null);
            if (GUILayout.Button($"- {option.DisplayTypeName}", GUILayout.MinWidth(100f)))
            {
                UIScriptGenerator.RemoveNodeBinding(option);
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
