using System;
using System.Collections.Generic;
using DGame;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public enum UIBindingKind
    {
        Component,
        GameObject,
        Widget,
    }

    public enum UIBindingEventKind
    {
        None,
        Click,
        ToggleValueChanged,
        SliderValueChanged,
    }

    [Serializable]
    public sealed class UIBindingEntry
    {
        [SerializeField] private string m_bindingId;
        [SerializeField] private string m_fieldName;
        [SerializeField] private string m_expectedTypeName;
        [SerializeField] private Component m_target;
        [SerializeField] private UIBindingKind m_kind;
        [SerializeField] private bool m_generateUnityEvent;
        [SerializeField] private UIBindingEventKind m_eventKind;
        [SerializeField] private string m_eventHandlerName;
        [SerializeField] private string m_widgetTypeName;

        public string BindingId => m_bindingId;
        public string FieldName => m_fieldName;
        public string ExpectedTypeName => m_expectedTypeName;
        public Component Target => m_target;
        public UIBindingKind Kind => m_kind;
        public bool GenerateUnityEvent => m_generateUnityEvent;
        public UIBindingEventKind EventKind => m_eventKind;
        public string EventHandlerName => m_eventHandlerName;
        public string WidgetTypeName => m_widgetTypeName;

#if UNITY_EDITOR
        public void EnsureBindingId()
        {
            if (!string.IsNullOrWhiteSpace(m_bindingId))
            {
                return;
            }

            m_bindingId = Guid.NewGuid().ToString("N");
        }

        public void SetTarget(Component target)
        {
            m_target = target;
            m_expectedTypeName = target == null ? string.Empty : target.GetType().AssemblyQualifiedName;
            ConfigureDefaultUnityEvent();
        }

        public void SetSuggestion(string fieldName, Component target, UIBindingKind kind)
        {
            m_fieldName = fieldName;
            m_kind = kind;
            SetTarget(target);
            EnsureBindingId();
        }

        public void SetFieldName(string fieldName)
        {
            m_fieldName = fieldName;
        }

        public void SetWidgetTypeName(string widgetTypeName)
        {
            m_widgetTypeName = widgetTypeName;
            SynchronizeWidgetExpectedType();
        }

        public void SynchronizeExpectedType()
        {
            if (m_kind == UIBindingKind.Widget)
            {
                SynchronizeWidgetExpectedType();
                return;
            }

            SetTarget(m_target);
        }

        private void SynchronizeWidgetExpectedType()
        {
            if (string.IsNullOrWhiteSpace(m_widgetTypeName))
            {
                SynchronizeWidgetTypeName();
                return;
            }

            Type widgetType = ResolveWidgetType();
            m_expectedTypeName = widgetType == null ? string.Empty : widgetType.AssemblyQualifiedName;
        }

        private void SynchronizeWidgetTypeName()
        {
            Type expectedType = Type.GetType(m_expectedTypeName);
            if (expectedType != null && typeof(UIWidget).IsAssignableFrom(expectedType))
            {
                m_widgetTypeName = expectedType.Name;
            }
        }

        private Type ResolveWidgetType()
        {
            if (string.IsNullOrWhiteSpace(m_widgetTypeName))
            {
                return null;
            }

            string fullTypeName = m_widgetTypeName.Contains(".")
                ? m_widgetTypeName
                : $"{typeof(UIWidget).Namespace}.{m_widgetTypeName}";
            Type widgetType = typeof(UIWidget).Assembly.GetType(fullTypeName);
            return widgetType != null && typeof(UIWidget).IsAssignableFrom(widgetType) ? widgetType : null;
        }

        private void ConfigureDefaultUnityEvent()
        {
            m_generateUnityEvent = false;
            m_eventKind = UIBindingEventKind.None;
            m_eventHandlerName = string.Empty;

            if (m_target is Button)
            {
                SetUnityEvent(UIBindingEventKind.Click, "OnClick", "m_btn", "Btn");
                return;
            }

            if (m_target is Toggle)
            {
                SetUnityEvent(UIBindingEventKind.ToggleValueChanged, "OnToggle", "m_toggle", "Change");
                return;
            }

            if (m_target is Slider)
            {
                SetUnityEvent(UIBindingEventKind.SliderValueChanged, "OnSlider", "m_slider", "Change");
            }
        }

        private void SetUnityEvent(UIBindingEventKind eventKind, string methodPrefix, string fieldPrefix,
            string methodSuffix)
        {
            m_generateUnityEvent = true;
            m_eventKind = eventKind;
            m_eventHandlerName = methodPrefix + GetEventFieldName(fieldPrefix) + methodSuffix;
        }

        private string GetEventFieldName(string fieldPrefix)
        {
            string fieldName = m_fieldName ?? string.Empty;
            if (fieldName.StartsWith(fieldPrefix))
            {
                return fieldName.Substring(fieldPrefix.Length);
            }

            return fieldName.StartsWith("m_") ? fieldName.Substring(2) : fieldName;
        }
#endif
    }

    [DisallowMultipleComponent]
    public partial class UIBindComponent : MonoBehaviour
    {
        [SerializeField] private List<Component> m_components = new List<Component>();
        [SerializeField] private bool m_useBindingManifest;
        [SerializeField] private bool m_isWidgetRoot;
        [SerializeField] private List<UIBindingEntry> m_bindingEntries = new List<UIBindingEntry>();
        [SerializeField] private string m_manifestSignature;
        [SerializeField] private string m_generatedSignature;

        private Dictionary<string, UIBindingEntry> m_bindingEntryMap;

        public bool IsManifestBinding => m_useBindingManifest;
        public bool IsWidgetRoot => m_isWidgetRoot;

        public T GetComponent<T>(int index) where T : Component
        {
            if (index < 0 || index >= m_components.Count)
            {
                DLogger.Error("索引超出范围");
                return null;
            }

            T component = m_components[index] as T;
            if (component == null)
            {
                DLogger.Error($"没有找到对应类型: {typeof(T).FullName}");
            }

            return component;
        }

        public T GetRequired<T>(string bindingId) where T : Component
        {
            UIBindingEntry entry = GetRequiredEntry(bindingId);
            ValidateEntryTarget(entry);

            T component = entry.Target as T;
            if (component != null)
            {
                return component;
            }

            throw CreateBindingException(entry, typeof(T).FullName, entry.Target.GetType().FullName);
        }

        private UIBindingEntry GetRequiredEntry(string bindingId)
        {
            EnsureBindingEntryMap();
            if (string.IsNullOrWhiteSpace(bindingId) || !m_bindingEntryMap.TryGetValue(bindingId, out UIBindingEntry entry))
            {
                throw new InvalidOperationException($"UI绑定缺失: UI={GetHierarchyPath()}, BindingId={bindingId}");
            }

            if (entry.Target == null)
            {
                throw CreateBindingException(entry, entry.ExpectedTypeName, "null");
            }

            return entry;
        }

        private void EnsureBindingEntryMap()
        {
            if (m_bindingEntryMap != null)
            {
                return;
            }

            m_bindingEntryMap = new Dictionary<string, UIBindingEntry>();
            for (int index = 0; index < m_bindingEntries.Count; index++)
            {
                UIBindingEntry entry = m_bindingEntries[index];
                AddBindingEntryToMap(entry, index);
            }
        }

        private void AddBindingEntryToMap(UIBindingEntry entry, int index)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.BindingId))
            {
                throw new InvalidOperationException($"UI绑定ID无效: UI={GetHierarchyPath()}, Index={index}");
            }

            if (m_bindingEntryMap.ContainsKey(entry.BindingId))
            {
                throw new InvalidOperationException($"UI绑定ID重复: UI={GetHierarchyPath()}, BindingId={entry.BindingId}");
            }

            m_bindingEntryMap.Add(entry.BindingId, entry);
        }

        private void ValidateExpectedType(UIBindingEntry entry)
        {
            Type expectedType = Type.GetType(entry.ExpectedTypeName);
            if (expectedType == null || !typeof(Component).IsAssignableFrom(expectedType))
            {
                throw CreateBindingException(entry, entry.ExpectedTypeName, entry.Target.GetType().FullName);
            }

            if (!expectedType.IsInstanceOfType(entry.Target))
            {
                throw CreateBindingException(entry, expectedType.FullName, entry.Target.GetType().FullName);
            }
        }

        private void ValidateEntryTarget(UIBindingEntry entry)
        {
            if (entry.Kind == UIBindingKind.Widget)
            {
                ValidateWidgetTarget(entry);
                return;
            }

            ValidateExpectedType(entry);
        }

        private void ValidateWidgetTarget(UIBindingEntry entry)
        {
            Type expectedType = Type.GetType(entry.ExpectedTypeName);
            if (expectedType == null || !typeof(UIWidget).IsAssignableFrom(expectedType))
            {
                throw CreateBindingException(entry, typeof(UIWidget).FullName, entry.ExpectedTypeName);
            }

            if (entry.Target is not RectTransform)
            {
                throw CreateBindingException(entry, typeof(RectTransform).FullName, entry.Target.GetType().FullName);
            }
        }

        private InvalidOperationException CreateBindingException(UIBindingEntry entry, string expectedType, string actualType)
        {
            return new InvalidOperationException(
                $"UI绑定类型无效: UI={GetHierarchyPath()}, BindingId={entry.BindingId}, Field={entry.FieldName}, Expected={expectedType}, Actual={actualType}");
        }

        private string GetHierarchyPath()
        {
            var names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private void OnValidate()
        {
            m_bindingEntryMap = null;
#if UNITY_EDITOR
            for (int index = 0; index < m_bindingEntries.Count; index++)
            {
                m_bindingEntries[index]?.EnsureBindingId();
            }
#endif
        }
    }
}
