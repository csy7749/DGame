using System.Collections.Generic;
using System.Text;
using GameLogic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DGame
{
    public sealed class UIBindingNodeOption
    {
        public UIBindComponent Owner { get; internal set; }
        public Component Target { get; internal set; }
        public UIBindingKind Kind { get; internal set; }
        public string DisplayTypeName { get; internal set; }
        public string SuggestedFieldName { get; internal set; }
        public string WidgetTypeName { get; internal set; }
    }

    public partial class UIScriptGenerator
    {
        public static List<UIBindingNodeOption> GetNodeBindingOptions(GameObject node)
        {
            var options = new List<UIBindingNodeOption>();
            if (!IsNodeInPrefabStage(node) || UIScriptGeneratorSettings.Instance == null)
            {
                return options;
            }

            AddWidgetOption(node, options);
            UIBindComponent componentOwner = GetComponentOwner(node);
            if (componentOwner == null)
            {
                return options;
            }

            AddGameObjectOption(node, componentOwner, options);
            AddComponentOptions(node, componentOwner, options);
            return options;
        }

        public static UIBindingEntry FindNodeBinding(UIBindingNodeOption option)
        {
            if (option?.Owner == null)
            {
                return null;
            }

            foreach (UIBindingEntry entry in option.Owner.BindingEntries)
            {
                if (entry != null && entry.Target == option.Target && entry.Kind == option.Kind)
                {
                    return entry;
                }
            }

            return null;
        }

        public static UIBindingEntry AddNodeBinding(UIBindingNodeOption option)
        {
            UIBindingEntry existing = FindNodeBinding(option);
            if (existing != null)
            {
                RepairWidgetBinding(option, existing);
                return existing;
            }

            if (option == null || option.Owner == null || option.Target == null)
            {
                return null;
            }

            PrepareComponentOwner(option);
            string widgetTypeName = PrepareWidgetRoot(option);
            Undo.RecordObject(option.Owner, "添加 UI Binding Entry");
            UIBindingEntry entry = option.Owner.AddBindingEntry();
            entry.SetSuggestion(option.SuggestedFieldName, option.Target, option.Kind);
            if (option.Kind == UIBindingKind.Widget)
            {
                entry.SetWidgetTypeName(widgetTypeName);
            }

            MarkBindingDirty(option.Owner);
            return entry;
        }

        public static bool IsNodeBindingConfigured(UIBindingNodeOption option)
        {
            UIBindingEntry entry = FindNodeBinding(option);
            if (entry == null)
            {
                return false;
            }

            if (option.Kind != UIBindingKind.Widget)
            {
                return true;
            }

            UIBindComponent widgetRoot = option.Target.GetComponent<UIBindComponent>();
            return widgetRoot != null && widgetRoot.IsManifestBinding && widgetRoot.IsWidgetRoot
                && widgetRoot.ClassName == entry.WidgetTypeName;
        }

        /// <summary>
        /// 根据当前 Prefab 中显式的 Widget 根重建父级 Widget 条目，并清理所有空目标条目。
        /// </summary>
        public static bool RecollectManifestBindings(UIBindComponent owner)
        {
            if (owner == null || !owner.IsManifestBinding || PrefabStageUtility.GetPrefabStage(owner.gameObject) == null)
            {
                Debug.LogError("[UI Binding Manifest] 重新收集绑定只能在 Prefab Stage 的 Manifest 根节点上执行。");
                return false;
            }

            Undo.RecordObject(owner, "重新收集 UI Binding Manifest");
            RemoveStaleEntries(owner);
            AddOwnedWidgetEntries(owner);
            owner.InvalidateManifestSignature();
            MarkBindingDirty(owner);
            return true;
        }

        private static void RemoveStaleEntries(UIBindComponent owner)
        {
            var entries = new List<UIBindingEntry>(owner.BindingEntries);
            foreach (UIBindingEntry entry in entries)
            {
                if (entry == null || entry.Target == null || entry.Kind == UIBindingKind.Widget)
                {
                    owner.RemoveBindingEntry(entry);
                }
            }
        }

        private static void AddOwnedWidgetEntries(UIBindComponent owner)
        {
            foreach (UIBindComponent widgetRoot in owner.GetComponentsInChildren<UIBindComponent>(true))
            {
                if (widgetRoot == owner || !widgetRoot.IsManifestBinding || !widgetRoot.IsWidgetRoot)
                {
                    continue;
                }

                UIBindingNodeOption option = GetNodeBindingOptions(widgetRoot.gameObject)
                    .Find(candidate => candidate.Kind == UIBindingKind.Widget && candidate.Owner == owner);
                if (option != null)
                {
                    AddNodeBinding(option);
                }
            }
        }

        public static void RemoveNodeBinding(UIBindingNodeOption option)
        {
            UIBindingEntry entry = FindNodeBinding(option);
            if (entry == null)
            {
                return;
            }

            Undo.RecordObject(option.Owner, "移除 UI Binding Entry");
            option.Owner.RemoveBindingEntry(entry);
            MarkBindingDirty(option.Owner);
        }

        public static void RenameNodeBinding(UIBindingNodeOption option, string fieldName)
        {
            UIBindingEntry entry = FindNodeBinding(option);
            if (entry == null || entry.FieldName == fieldName)
            {
                return;
            }

            Undo.RecordObject(option.Owner, "重命名 UI Binding Entry");
            entry.SetFieldName(fieldName);
            option.Owner.InvalidateManifestSignature();
            MarkBindingDirty(option.Owner);
        }

        private static void AddWidgetOption(GameObject node, ICollection<UIBindingNodeOption> options)
        {
            if (!IsWidgetCandidate(node))
            {
                return;
            }

            UIBindComponent owner = FindNearestManifestOwner(node.transform.parent);
            RectTransform target = node.GetComponent<RectTransform>();
            if (owner == null || target == null)
            {
                return;
            }

            options.Add(new UIBindingNodeOption
            {
                Owner = owner,
                Target = target,
                Kind = UIBindingKind.Widget,
                DisplayTypeName = "Widget",
                SuggestedFieldName = CreateWidgetFieldName(owner, node.name),
                WidgetTypeName = CreateWidgetTypeName(node),
            });
        }

        private static void AddGameObjectOption(GameObject node, UIBindComponent owner,
            ICollection<UIBindingNodeOption> options)
        {
            RectTransform target = node.GetComponent<RectTransform>();
            if (target == null)
            {
                return;
            }

            options.Add(new UIBindingNodeOption
            {
                Owner = owner,
                Target = target,
                Kind = UIBindingKind.GameObject,
                DisplayTypeName = nameof(GameObject),
                SuggestedFieldName = CreateGameObjectFieldName(owner, node.name),
            });
        }

        private static void AddComponentOptions(GameObject node, UIBindComponent owner,
            ICollection<UIBindingNodeOption> options)
        {
            foreach (Component component in node.GetComponents<Component>())
            {
                if (IsIgnoredComponent(component))
                {
                    continue;
                }

                UIScriptGenerateRuler rule = FindComponentRule(component.GetType());
                if (rule == null)
                {
                    continue;
                }

                options.Add(new UIBindingNodeOption
                {
                    Owner = owner,
                    Target = component,
                    Kind = UIBindingKind.Component,
                    DisplayTypeName = component.GetType().Name,
                    SuggestedFieldName = CreateComponentFieldName(owner, node.name, rule),
                });
            }
        }

        private static string PrepareWidgetRoot(UIBindingNodeOption option)
        {
            if (option.Kind != UIBindingKind.Widget)
            {
                return string.Empty;
            }

            GameObject node = option.Target.gameObject;
            UIBindComponent widgetRoot = node.GetComponent<UIBindComponent>();
            if (widgetRoot == null)
            {
                widgetRoot = Undo.AddComponent<UIBindComponent>(node);
            }
            else
            {
                Undo.RecordObject(widgetRoot, "配置 UI Widget Root");
            }

            ConfigureWidgetRoot(widgetRoot, option.WidgetTypeName);
            return widgetRoot.ClassName;
        }

        private static void RepairWidgetBinding(UIBindingNodeOption option, UIBindingEntry entry)
        {
            if (option.Kind != UIBindingKind.Widget || IsNodeBindingConfigured(option))
            {
                return;
            }

            string widgetTypeName = PrepareWidgetRoot(option);
            Undo.RecordObject(option.Owner, "修复 UI Widget Binding");
            entry.SetWidgetTypeName(widgetTypeName);
            option.Owner.InvalidateManifestSignature();
            MarkBindingDirty(option.Owner);
        }

        private static void PrepareComponentOwner(UIBindingNodeOption option)
        {
            if (option.Kind == UIBindingKind.Widget || option.Owner.gameObject != option.Target.gameObject
                || !IsWidgetCandidate(option.Owner.gameObject))
            {
                return;
            }

            Undo.RecordObject(option.Owner, "配置 UI Widget Root");
            ConfigureWidgetRoot(option.Owner, CreateWidgetTypeName(option.Owner.gameObject));
        }

        private static void ConfigureWidgetRoot(UIBindComponent widgetRoot, string widgetTypeName)
        {
            widgetRoot.EnableBindingManifest();
            widgetRoot.SetWidgetRoot(true);
            widgetRoot.SetClassNameIfEmpty(widgetTypeName);
            widgetRoot.SetUITypeIfEmpty(nameof(UIWidget));
            MarkBindingDirty(widgetRoot);
        }

        private static UIBindComponent GetComponentOwner(GameObject node)
        {
            UIBindComponent current = node.GetComponent<UIBindComponent>();
            if (current != null)
            {
                if (current.IsManifestBinding || HasParentWidgetBinding(node))
                {
                    return current;
                }

                return null;
            }

            if (IsWidgetCandidate(node))
            {
                return null;
            }

            return FindNearestManifestOwner(node.transform.parent);
        }

        private static bool HasParentWidgetBinding(GameObject node)
        {
            UIBindComponent owner = FindNearestManifestOwner(node.transform.parent);
            RectTransform target = node.GetComponent<RectTransform>();
            if (owner == null || target == null)
            {
                return false;
            }

            foreach (UIBindingEntry entry in owner.BindingEntries)
            {
                if (entry != null && entry.Kind == UIBindingKind.Widget && entry.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static UIBindComponent FindNearestManifestOwner(Transform current)
        {
            while (current != null)
            {
                UIBindComponent owner = current.GetComponent<UIBindComponent>();
                if (owner != null)
                {
                    return owner.IsManifestBinding ? owner : null;
                }

                current = current.parent;
            }

            return null;
        }

        private static bool IsWidgetCandidate(GameObject node)
        {
            UIBindComponent binding = node.GetComponent<UIBindComponent>();
            if (binding != null && (binding.IsWidgetRoot || binding.UITypeName == nameof(UIWidget)))
            {
                return true;
            }

            UIScriptGenerateRuler rule = FindRule(node.name);
            return rule != null && rule.isUIWidget;
        }

        private static bool IsNodeInPrefabStage(GameObject node)
        {
            if (node == null)
            {
                return false;
            }

            PrefabStage stage = PrefabStageUtility.GetPrefabStage(node);
            return stage != null && stage.prefabContentsRoot != null;
        }

        private static bool IsIgnoredComponent(Component component)
        {
            return component == null || component is Transform || component is CanvasRenderer
                || component is UIBindComponent;
        }

        private static UIScriptGenerateRuler FindComponentRule(System.Type componentType)
        {
            foreach (UIScriptGenerateRuler rule in UIScriptGeneratorSettings.GetScriptGenerateRulers())
            {
                if (rule.componentName == UIComponentName.GameObject || rule.isUIWidget)
                {
                    continue;
                }

                System.Type whitelistType = GetComponentTypeFromEnumName(rule.componentName);
                if (whitelistType == componentType)
                {
                    return rule;
                }
            }

            foreach (UIScriptGenerateRuler rule in UIScriptGeneratorSettings.GetScriptGenerateRulers())
            {
                if (rule.componentName == UIComponentName.GameObject || rule.isUIWidget)
                {
                    continue;
                }

                System.Type whitelistType = GetComponentTypeFromEnumName(rule.componentName);
                if (whitelistType != null && whitelistType.IsAssignableFrom(componentType))
                {
                    return rule;
                }
            }

            return null;
        }

        private static UIScriptGenerateRuler FindRule(string nodeName)
        {
            return UIScriptGeneratorSettings.GetScriptGenerateRulers()
                .Find(rule => nodeName.StartsWith(rule.uiElementRegex));
        }

        private static string CreateComponentFieldName(UIBindComponent owner, string nodeName,
            UIScriptGenerateRuler componentRule)
        {
            UIScriptGenerateRuler nodeRule = FindRule(nodeName);
            if (nodeRule != null && !nodeRule.isUIWidget && nodeRule.componentName == componentRule.componentName)
            {
                return CreateUniqueFieldName(owner, NormalizeNodeFieldName(nodeName));
            }

            string componentName = UIScriptGeneratorSettings.GetUIComponentWithoutPrefixName(
                componentRule.componentName);
            string baseName = GetPrefixName() + componentName + GetNodeSemanticName(nodeName);
            return CreateUniqueFieldName(owner, baseName);
        }

        private static string CreateGameObjectFieldName(UIBindComponent owner, string nodeName)
        {
            UIScriptGenerateRuler nodeRule = FindRule(nodeName);
            if (nodeRule != null && !nodeRule.isUIWidget && nodeRule.componentName == UIComponentName.GameObject)
            {
                return CreateUniqueFieldName(owner, NormalizeNodeFieldName(nodeName));
            }

            string baseName = GetPrefixName() + "go" + GetNodeSemanticName(nodeName);
            return CreateUniqueFieldName(owner, baseName);
        }

        private static string CreateWidgetFieldName(UIBindComponent owner, string nodeName)
        {
            UIScriptGenerateRuler nodeRule = FindRule(nodeName);
            if (nodeRule != null && nodeRule.isUIWidget)
            {
                return CreateUniqueFieldName(owner, NormalizeNodeFieldName(nodeName));
            }

            string baseName = GetPrefixName() + UIScriptGeneratorSettings.Instance.WidgetName
                + GetNodeSemanticName(nodeName);
            return CreateUniqueFieldName(owner, baseName);
        }

        private static string CreateWidgetTypeName(GameObject node)
        {
            UIBindComponent binding = node.GetComponent<UIBindComponent>();
            if (binding != null && !string.IsNullOrWhiteSpace(binding.ClassName))
            {
                return binding.ClassName;
            }

            return GetNodeSemanticName(node.name);
        }

        private static string NormalizeNodeFieldName(string nodeName)
        {
            string semanticName = nodeName.StartsWith("m_") ? nodeName.Substring(2) : nodeName.TrimStart('_');
            if (string.IsNullOrWhiteSpace(semanticName))
            {
                semanticName = "node";
            }

            return GetPrefixName() + char.ToLowerInvariant(semanticName[0]) + semanticName.Substring(1);
        }

        private static string GetNodeSemanticName(string nodeName)
        {
            string semanticName = nodeName.StartsWith("m_") ? nodeName.Substring(2) : nodeName.TrimStart('_');
            return ToPascalIdentifier(semanticName);
        }

        private static string ToPascalIdentifier(string value)
        {
            var builder = new StringBuilder();
            bool capitalize = true;
            foreach (char character in value ?? string.Empty)
            {
                if (!char.IsLetterOrDigit(character))
                {
                    capitalize = true;
                    continue;
                }

                builder.Append(capitalize ? char.ToUpperInvariant(character) : character);
                capitalize = false;
            }

            if (builder.Length == 0)
            {
                return "Node";
            }

            return char.IsDigit(builder[0]) ? "Node" + builder : builder.ToString();
        }

        private static string CreateUniqueFieldName(UIBindComponent owner, string baseName)
        {
            string fieldName = baseName;
            int suffix = 2;
            while (HasFieldName(owner, fieldName))
            {
                fieldName = baseName + suffix;
                suffix++;
            }

            return fieldName;
        }

        private static bool HasFieldName(UIBindComponent owner, string fieldName)
        {
            foreach (UIBindingEntry entry in owner.BindingEntries)
            {
                if (entry != null && entry.FieldName == fieldName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MarkBindingDirty(UIBindComponent binding)
        {
            EditorUtility.SetDirty(binding);
            EditorApplication.RepaintHierarchyWindow();
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(binding.gameObject);
            if (stage != null)
            {
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
        }
    }
}
