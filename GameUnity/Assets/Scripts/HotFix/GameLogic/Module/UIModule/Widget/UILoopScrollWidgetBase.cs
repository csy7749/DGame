using System;
using System.Collections.Generic;
using DGame;
using UnityEngine;

namespace GameLogic
{
    public abstract class UILoopScrollWidgetBase<TItem, TData> : UIListBase<TItem, TData>
        where TItem : UIWidget, new()
    {
        private readonly Dictionary<Transform, TItem> m_itemByTransform = new();
        private readonly Dictionary<int, TItem> m_visibleItemByIndex = new();
        private readonly Dictionary<Transform, int> m_visibleIndexByTransform = new();
        private readonly Dictionary<Transform, int> m_poolKeyByTransform = new();
        private readonly Dictionary<int, Stack<GameObject>> m_itemPools = new();

        protected Action<TItem, int> ItemUpdater { get; set; }

        public override TItem GetItem(int index)
        {
            return m_visibleItemByIndex.TryGetValue(index, out var item) ? item : null;
        }

        public void GetVisibleItems(List<TItem> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            results.Clear();
            foreach (var item in m_visibleItemByIndex.Values)
            {
                results.Add(item);
            }
        }

        protected GameObject RentItem(int poolKey, GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                DLogger.Error($"{GetType().Name} item prefab is null, pool key: {poolKey}.");
                return null;
            }

            var pool = GetOrCreatePool(poolKey);
            if (pool.Count > 0)
            {
                return ActivateItem(pool.Pop(), parent);
            }

            var widget = CreateWidgetByPrefab<TItem>(prefab, parent, false);
            if (widget == null)
            {
                return null;
            }

            var itemObject = widget.gameObject;
            m_itemByTransform.Add(itemObject.transform, widget);
            m_poolKeyByTransform.Add(itemObject.transform, poolKey);
            return ActivateItem(itemObject, parent);
        }

        protected void BindItemData(Transform itemTransform, int index)
        {
            if (!m_itemByTransform.TryGetValue(itemTransform, out var item))
            {
                DLogger.Error($"Loop scroll item is not registered: {itemTransform.name}.");
                return;
            }

            RemoveVisibleIndex(itemTransform);
            RemoveConflictingItem(index, item);
            m_visibleIndexByTransform[itemTransform] = index;
            m_visibleItemByIndex[index] = item;
            if (item is IListSelectItem selectItem)
            {
                selectItem.SetItemIndex(index);
            }
            UpdateListItem(item, index, ItemUpdater);
        }

        protected void ReturnItem(Transform itemTransform)
        {
            if (!m_itemByTransform.TryGetValue(itemTransform, out var item))
            {
                DLogger.Error($"Loop scroll item is not registered: {itemTransform.name}.");
                return;
            }

            RemoveVisibleIndex(itemTransform);
            item.Show(false);
            itemTransform.SetParent(null, false);
            GetOrCreatePool(m_poolKeyByTransform[itemTransform]).Push(item.gameObject);
        }

        protected override void OnDestroy()
        {
            ItemUpdater = null;
            m_visibleItemByIndex.Clear();
            m_visibleIndexByTransform.Clear();
            m_poolKeyByTransform.Clear();
            m_itemPools.Clear();
            m_itemByTransform.Clear();
            base.OnDestroy();
        }

        private Stack<GameObject> GetOrCreatePool(int poolKey)
        {
            if (m_itemPools.TryGetValue(poolKey, out var pool))
            {
                return pool;
            }

            pool = new Stack<GameObject>();
            m_itemPools.Add(poolKey, pool);
            return pool;
        }

        private GameObject ActivateItem(GameObject itemObject, Transform parent)
        {
            itemObject.transform.SetParent(parent, false);
            m_itemByTransform[itemObject.transform].Show(true);
            return itemObject;
        }

        private void RemoveVisibleIndex(Transform itemTransform)
        {
            if (!m_visibleIndexByTransform.Remove(itemTransform, out var previousIndex))
            {
                return;
            }
            if (m_visibleItemByIndex.TryGetValue(previousIndex, out var visibleItem) &&
                ReferenceEquals(visibleItem, m_itemByTransform[itemTransform]))
            {
                m_visibleItemByIndex.Remove(previousIndex);
            }
        }

        private void RemoveConflictingItem(int index, TItem item)
        {
            if (!m_visibleItemByIndex.TryGetValue(index, out var previousItem) ||
                ReferenceEquals(previousItem, item))
            {
                return;
            }
            m_visibleIndexByTransform.Remove(previousItem.transform);
        }
    }
}
