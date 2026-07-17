using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class UILoopScrollMultiWidget<TItem, TData> : UILoopScrollWidgetBase<TItem, TData>,
        LoopScrollPrefabSource, LoopScrollMultiDataSource
        where TItem : UIWidget, new()
    {
        private readonly List<GameObject> m_itemPrefabs = new();
        private Func<int, int> m_itemPrefabIndexProvider;

        public LoopScrollRectMulti LoopRectView { get; private set; }

        public LoopScrollRectBase.ScrollRectEvent OnValueChanged => LoopRectView.onValueChanged;

        public RectTransform ContentRect => LoopRectView.content;

        protected override void BindMemberProperty()
        {
            base.BindMemberProperty();
            LoopRectView = rectTransform.GetComponent<LoopScrollRectMulti>();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            if (LoopRectView == null)
            {
                DGame.DLogger.Error($"{GetType().Name} requires a LoopScrollRectMulti component.");
                return;
            }
            LoopRectView.prefabSource = this;
            LoopRectView.dataSource = this;
        }

        public void SetItemPrefabs(IReadOnlyList<GameObject> itemPrefabs)
        {
            m_itemPrefabs.Clear();
            if (itemPrefabs == null)
            {
                return;
            }
            for (int i = 0; i < itemPrefabs.Count; i++)
            {
                m_itemPrefabs.Add(itemPrefabs[i]);
            }
        }

        public void SetItemPrefabIndexProvider(Func<int, int> provider)
        {
            m_itemPrefabIndexProvider = provider;
        }

        protected override void AdjustItemNum(int count, List<TData> datas = null,
            Action<TItem, int> itemUpdater = null)
        {
            base.AdjustItemNum(count, datas, itemUpdater);
            if (!CanRefresh())
            {
                return;
            }

            ItemUpdater = itemUpdater;
            try
            {
                int previousCount = LoopRectView.totalCount;
                LoopRectView.totalCount = count;
                if (previousCount == count)
                {
                    LoopRectView.RefreshCells();
                }
                else
                {
                    LoopRectView.RefillCells();
                }
            }
            finally
            {
                ItemUpdater = null;
            }
        }

        public GameObject GetObject(int index)
        {
            int prefabIndex = GetPrefabIndex(index);
            if (prefabIndex < 0)
            {
                return null;
            }
            return RentItem(prefabIndex, m_itemPrefabs[prefabIndex], LoopRectView.content);
        }

        public void ProvideData(Transform itemTransform, int index)
        {
            BindItemData(itemTransform, index);
        }

        public void ReturnObject(Transform itemTransform)
        {
            ReturnItem(itemTransform);
        }

        public void RefreshCells()
        {
            LoopRectView.RefreshCells();
        }

        public void RefillCells(int startItem = 0, float contentOffset = 0)
        {
            LoopRectView.RefillCells(startItem, contentOffset);
        }

        public void ScrollToCell(int index, float speed, float offset = 0,
            LoopScrollRectBase.ScrollMode mode = LoopScrollRectBase.ScrollMode.ToStart)
        {
            LoopRectView.ScrollToCell(index, speed, offset, mode);
        }

        public void ScrollToCellWithinTime(int index, float time, float offset = 0,
            LoopScrollRectBase.ScrollMode mode = LoopScrollRectBase.ScrollMode.ToStart)
        {
            LoopRectView.ScrollToCellWithinTime(index, time, offset, mode);
        }

        public void Clear()
        {
            LoopRectView.ClearCells();
        }

        protected override void OnDestroy()
        {
            if (LoopRectView != null)
            {
                LoopRectView.ClearCells();
                LoopRectView.prefabSource = null;
                LoopRectView.dataSource = null;
            }
            m_itemPrefabIndexProvider = null;
            m_itemPrefabs.Clear();
            base.OnDestroy();
        }

        private int GetPrefabIndex(int itemIndex)
        {
            if (m_itemPrefabIndexProvider == null)
            {
                DGame.DLogger.Error($"{GetType().Name} item prefab index provider is null.");
                return -1;
            }
            int prefabIndex = m_itemPrefabIndexProvider(itemIndex);
            if (prefabIndex >= 0 && prefabIndex < m_itemPrefabs.Count)
            {
                return prefabIndex;
            }
            DGame.DLogger.Error($"Invalid loop item prefab index: {prefabIndex}, item index: {itemIndex}.");
            return -1;
        }

        private bool CanRefresh()
        {
            if (LoopRectView == null)
            {
                DGame.DLogger.Error($"{GetType().Name} has no LoopScrollRectMulti component.");
                return false;
            }
            if (m_itemPrefabs.Count > 0 && m_itemPrefabIndexProvider != null)
            {
                return true;
            }
            DGame.DLogger.Error($"{GetType().Name} item prefabs or prefab index provider is not configured.");
            return false;
        }
    }
}
