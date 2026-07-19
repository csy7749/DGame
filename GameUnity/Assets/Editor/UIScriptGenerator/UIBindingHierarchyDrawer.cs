#if UNITY_EDITOR

using System.Collections.Generic;
using GameLogic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DGame
{
    [InitializeOnLoad]
    public static class UIBindingHierarchyDrawer
    {
        private const float ICON_SIZE = 16f;
        private const float ICON_SPACING = 3.5f;
        private const float TRIANGLE_WIDTH = 6f;
        private const int MAX_ICON_COUNT = 5;

        static UIBindingHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
        }

        private static void DrawHierarchyItem(int instanceId, Rect selectionRect)
        {
            GameObject node = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (node == null)
            {
                return;
            }

            List<UIBindingEntry> entries = CollectEntries(node);
            DrawEntryIcons(selectionRect, entries);
        }

        private static List<UIBindingEntry> CollectEntries(GameObject node)
        {
            var entries = new List<UIBindingEntry>();
            var bindingIds = new HashSet<string>();
            Transform current = node.transform;
            while (current != null && entries.Count < MAX_ICON_COUNT)
            {
                UIBindComponent owner = current.GetComponent<UIBindComponent>();
                if (owner != null && owner.IsManifestBinding)
                {
                    AddTargetEntries(owner, node, entries, bindingIds);
                }

                current = current.parent;
            }

            return entries;
        }

        private static void AddTargetEntries(UIBindComponent owner, GameObject node,
            ICollection<UIBindingEntry> entries, ISet<string> bindingIds)
        {
            foreach (UIBindingEntry entry in owner.BindingEntries)
            {
                if (entry?.Target == null || entry.Target.gameObject != node || !bindingIds.Add(entry.BindingId))
                {
                    continue;
                }

                entries.Add(entry);
                if (entries.Count >= MAX_ICON_COUNT)
                {
                    return;
                }
            }
        }

        private static void DrawEntryIcons(Rect selectionRect, IReadOnlyList<UIBindingEntry> entries)
        {
            float x = selectionRect.x - ICON_SIZE - ICON_SPACING - TRIANGLE_WIDTH;
            foreach (UIBindingEntry entry in entries)
            {
                Texture icon = GetEntryIcon(entry);
                if (icon == null)
                {
                    continue;
                }

                Rect iconRect = new Rect(x, selectionRect.y, ICON_SIZE, ICON_SIZE);
                GUI.Label(iconRect, new GUIContent(icon, CreateTooltip(entry)));
                x -= ICON_SIZE;
            }
        }

        private static Texture GetEntryIcon(UIBindingEntry entry)
        {
            if (entry.Kind == UIBindingKind.Widget)
            {
                return EditorGUIUtility.IconContent("cs Script Icon").image;
            }

            if (entry.Kind == UIBindingKind.GameObject)
            {
                return EditorGUIUtility.IconContent("GameObject Icon").image;
            }

            return EditorGUIUtility.ObjectContent(null, GetDisplayIconType(entry.Target)).image;
        }

        private static System.Type GetDisplayIconType(Component target)
        {
            if (target is Button) return typeof(Button);
            if (target is Image) return typeof(Image);
            if (target is RawImage) return typeof(RawImage);
            if (target is Text) return typeof(Text);
            if (target is Toggle) return typeof(Toggle);
            if (target is Slider) return typeof(Slider);
            if (target is InputField) return typeof(InputField);
            if (target is Dropdown) return typeof(Dropdown);
            if (target is ScrollRect) return typeof(ScrollRect);
            if (target is Scrollbar) return typeof(Scrollbar);
            return target.GetType();
        }

        private static string CreateTooltip(UIBindingEntry entry)
        {
            string typeName = entry.Kind == UIBindingKind.Widget ? entry.WidgetTypeName : entry.Target.GetType().Name;
            return $"{entry.FieldName} ({typeName})";
        }
    }
}

#endif
