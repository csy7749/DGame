using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GameLogic;
using UnityEditor;
using UnityEngine;

namespace DGame
{
    public sealed class UIBindingValidationError
    {
        public UIBindingValidationError(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public static class UIBindingManifestValidator
    {
        private const string FIELD_NAME_PATTERN = "^[A-Za-z_][A-Za-z0-9_]*$";
        private const string GENERATOR_VERSION = "manifest-v2";

        public static List<UIBindingValidationError> Validate(UIBindComponent bindComponent)
        {
            var errors = new List<UIBindingValidationError>();
            if (bindComponent == null || !bindComponent.IsManifestBinding)
            {
                errors.Add(new UIBindingValidationError("当前UI未启用Binding Manifest。"));
                return errors;
            }

            ValidateEntries(bindComponent, errors);
            ValidateEventHandlers(bindComponent, errors);
            return errors;
        }

        public static string CalculateSignature(UIBindComponent bindComponent)
        {
            var lines = new List<string>();
            foreach (UIBindingEntry entry in bindComponent.BindingEntries)
            {
                lines.Add(CreateSignatureLine(entry));
            }

            lines.Sort(StringComparer.Ordinal);
            var builder = new StringBuilder(GENERATOR_VERSION);
            builder.Append('|').Append(bindComponent.ClassName);
            builder.Append('|').Append(bindComponent.UITypeName);
            builder.Append('|').Append(bindComponent.IsWidgetRoot);
            foreach (string line in lines)
            {
                builder.Append('\n').Append(line);
            }

            return Hash128.Compute(builder.ToString()).ToString();
        }

        private static void ValidateEntries(UIBindComponent bindComponent, List<UIBindingValidationError> errors)
        {
            var ids = new HashSet<string>();
            var fields = new HashSet<string>();
            foreach (UIBindingEntry entry in bindComponent.BindingEntries)
            {
                ValidateEntry(bindComponent, entry, ids, fields, errors);
            }
        }

        private static void ValidateEntry(UIBindComponent root, UIBindingEntry entry, ISet<string> ids,
            ISet<string> fields, List<UIBindingValidationError> errors)
        {
            if (entry == null)
            {
                errors.Add(new UIBindingValidationError($"UI={root.name}: 存在空BindingEntry。"));
                return;
            }

            ValidateIdentity(root, entry, ids, fields, errors);
            ValidateTarget(root, entry, errors);
            ValidateWidget(root, entry, errors);
        }

        private static void ValidateIdentity(UIBindComponent root, UIBindingEntry entry, ISet<string> ids,
            ISet<string> fields, List<UIBindingValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(entry.BindingId) || !ids.Add(entry.BindingId))
            {
                errors.Add(new UIBindingValidationError($"UI={root.name}, Field={entry.FieldName}: BindingId为空或重复。"));
            }

            if (!Regex.IsMatch(entry.FieldName ?? string.Empty, FIELD_NAME_PATTERN) || !fields.Add(entry.FieldName))
            {
                errors.Add(new UIBindingValidationError($"UI={root.name}, BindingId={entry.BindingId}: 字段名为空、非法或重复。"));
            }
        }

        private static void ValidateTarget(UIBindComponent root, UIBindingEntry entry,
            List<UIBindingValidationError> errors)
        {
            if (entry.Target == null)
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry, "目标组件为空。")));
                return;
            }

            if (entry.Target.transform != root.transform && !entry.Target.transform.IsChildOf(root.transform))
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry, "目标组件不属于当前Prefab根节点。")));
            }

            ValidateTargetOwner(root, entry, errors);

            if (entry.Kind == UIBindingKind.Widget)
            {
                return;
            }

            Type expectedType = Type.GetType(entry.ExpectedTypeName);
            if (expectedType == null || !expectedType.IsInstanceOfType(entry.Target))
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry, "预期类型与目标组件不匹配。")));
            }
        }

        private static void ValidateTargetOwner(UIBindComponent root, UIBindingEntry entry,
            ICollection<UIBindingValidationError> errors)
        {
            UIBindComponent nearestOwner = FindNearestBinding(entry.Target.transform);
            if (entry.Kind == UIBindingKind.Widget)
            {
                if (nearestOwner == null || nearestOwner == root)
                {
                    errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry,
                        "Widget目标必须拥有独立的UIBindComponent。")));
                }
                return;
            }

            if (nearestOwner != root)
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry,
                    "目标属于另一个UI或Widget绑定容器。")));
            }
        }

        private static void ValidateWidget(UIBindComponent root, UIBindingEntry entry,
            ICollection<UIBindingValidationError> errors)
        {
            if (entry.Kind != UIBindingKind.Widget)
            {
                return;
            }

            Type expectedType = Type.GetType(entry.ExpectedTypeName);
            if (entry.Target is not RectTransform || expectedType == null
                || !typeof(UIWidget).IsAssignableFrom(expectedType))
            {
                errors.Add(new UIBindingValidationError($"BindingId={entry.BindingId}: Widget必须绑定RectTransform，预期类型必须是UIWidget子类。"));
                return;
            }

            UIBindComponent widgetRoot = entry.Target.GetComponent<UIBindComponent>();
            if (widgetRoot == null || !widgetRoot.IsManifestBinding || !widgetRoot.IsWidgetRoot)
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry,
                    "Widget目标未启用Manifest或未标记为Widget根节点。")));
                return;
            }

            if (FindNearestBinding(widgetRoot.transform.parent) != root)
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry,
                    "Widget父级归属与当前Manifest不一致。")));
            }

            if (widgetRoot.ClassName != entry.WidgetTypeName || expectedType.Name != widgetRoot.ClassName)
            {
                errors.Add(new UIBindingValidationError(CreateEntryMessage(root, entry,
                    $"Widget类型应与子UI类名 {widgetRoot.ClassName} 一致。")));
            }
        }

        private static UIBindComponent FindNearestBinding(Transform current)
        {
            while (current != null)
            {
                UIBindComponent binding = current.GetComponent<UIBindComponent>();
                if (binding != null)
                {
                    return binding;
                }

                current = current.parent;
            }

            return null;
        }

        private static void ValidateEventHandlers(UIBindComponent bindComponent, List<UIBindingValidationError> errors)
        {
            foreach (UIBindingEntry entry in bindComponent.BindingEntries)
            {
                ValidateEventHandler(bindComponent, entry, errors);
            }
        }

        private static void ValidateEventHandler(UIBindComponent bindComponent, UIBindingEntry entry,
            List<UIBindingValidationError> errors)
        {
            if (entry == null || !entry.GenerateUnityEvent)
            {
                return;
            }

            if (entry.EventKind == UIBindingEventKind.None || string.IsNullOrWhiteSpace(entry.EventHandlerName))
            {
                errors.Add(new UIBindingValidationError($"BindingId={entry.BindingId}: UnityEvent未配置事件类型或处理器。"));
                return;
            }

            if (!HasEventHandler(bindComponent, entry.EventHandlerName))
            {
                errors.Add(new UIBindingValidationError($"BindingId={entry.BindingId}: 未找到业务处理器 {entry.EventHandlerName}。"));
            }
        }

        private static bool HasEventHandler(UIBindComponent bindComponent, string handlerName)
        {
            string path = Path.Combine(bindComponent.ImplementationCodePath, bindComponent.ClassName + ".cs");
            if (!File.Exists(path))
            {
                return false;
            }

            string source = File.ReadAllText(path);
            return Regex.IsMatch(source, $@"\b{Regex.Escape(handlerName)}\s*\(");
        }

        private static string CreateEntryMessage(UIBindComponent root, UIBindingEntry entry, string reason)
        {
            string actualType = entry.Target == null ? "null" : entry.Target.GetType().FullName;
            return $"UI={root.name}, BindingId={entry.BindingId}, Field={entry.FieldName}, Expected={entry.ExpectedTypeName}, Actual={actualType}: {reason}";
        }

        private static string CreateSignatureLine(UIBindingEntry entry)
        {
            string targetId = entry.Target == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(entry.Target).ToString();
            return string.Join("|", entry.BindingId, entry.FieldName, entry.ExpectedTypeName, entry.Kind,
                entry.GenerateUnityEvent, entry.EventKind, entry.EventHandlerName, entry.WidgetTypeName, targetId);
        }
    }
}
