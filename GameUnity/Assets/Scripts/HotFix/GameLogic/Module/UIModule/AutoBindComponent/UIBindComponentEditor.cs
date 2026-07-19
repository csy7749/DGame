#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public partial class UIBindComponent
    {
        [SerializeField, HideInInspector] private string className;
        [SerializeField, HideInInspector] private string widgetTypeName;
        [SerializeField, HideInInspector] private string dataTypeName;
        [SerializeField, HideInInspector] private string uiType;

        public IReadOnlyList<UIBindingEntry> BindingEntries => m_bindingEntries;
        public string ManifestSignature => m_manifestSignature;
        public string GeneratedSignature => m_generatedSignature;
        public bool IsGeneratedSignatureCurrent => m_useBindingManifest && m_manifestSignature == m_generatedSignature;
        public string ClassName => className;
        public string UITypeName => uiType;
        public string WidgetTypeName => widgetTypeName;
        public string DataTypeName => dataTypeName;

        public void AddComponent(Component component)
        {
            if (component != null && !m_components.Contains(component))
            {
                m_components.Add(component);
            }
        }

        public void Clear()
        {
            m_components.Clear();
        }

        public void EnableBindingManifest()
        {
            m_useBindingManifest = true;
        }

        public void ClearBindingEntries()
        {
            m_bindingEntries.Clear();
            m_useBindingManifest = true;
            InvalidateManifestSignature();
            m_bindingEntryMap = null;
        }

        public UIBindingEntry AddBindingEntry()
        {
            var entry = new UIBindingEntry();
            entry.EnsureBindingId();
            m_bindingEntries.Add(entry);
            m_useBindingManifest = true;
            InvalidateManifestSignature();
            m_bindingEntryMap = null;
            return entry;
        }

        public bool RemoveBindingEntry(UIBindingEntry entry)
        {
            if (!m_bindingEntries.Remove(entry))
            {
                return false;
            }

            InvalidateManifestSignature();
            m_bindingEntryMap = null;
            return true;
        }

        public void InvalidateManifestSignature()
        {
            m_manifestSignature = string.Empty;
        }

        public void SetManifestSignature(string signature)
        {
            m_manifestSignature = signature;
        }

        public void SetGeneratedSignature(string signature)
        {
            m_generatedSignature = signature;
        }

        public void SetWidgetRoot(bool isWidgetRoot)
        {
            m_isWidgetRoot = isWidgetRoot;
            InvalidateManifestSignature();
        }

        public void SetClassNameIfEmpty(string suggestedClassName)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                className = suggestedClassName;
            }
        }

        public void SetUITypeIfEmpty(string suggestedUIType)
        {
            if (string.IsNullOrWhiteSpace(uiType))
            {
                uiType = suggestedUIType;
            }
        }

    }
}

#endif
