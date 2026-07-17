using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class UILoopScrollWidget<TItem, TData> : UILoopScrollWidgetBase<TItem, TData>,
        LoopScrollPrefabSource, LoopScrollDataSource
        where TItem : UIWidget, new()
    {
        public LoopScrollRect LoopRectView { get; private set; }

        public LoopScrollRectBase.ScrollRectEvent OnValueChanged => LoopRectView.onValueChanged;

        public RectTransform ContentRect => LoopRectView.content;

        public Vector2 NormalizedPosition
        {
            get => LoopRectView.normalizedPosition;
            set => LoopRectView.normalizedPosition = value;
        }

        protected override void BindMemberProperty()
        {
            base.BindMemberProperty();
            LoopRectView = rectTransform.GetComponent<LoopScrollRect>();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            if (LoopRectView == null)
            {
                DGame.DLogger.Error($"{GetType().Name} requires a LoopScrollRect component.");
                return;
            }
            LoopRectView.prefabSource = this;
            LoopRectView.dataSource = this;
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
            return RentItem(0, BaseItemPrefab, LoopRectView.content);
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

        public void RefillCellsFromEnd(int endItem = 0, float contentOffset = 0)
        {
            LoopRectView.RefillCellsFromEnd(endItem, contentOffset);
        }

        public void RefreshLayoutAfterConstraintChanged()
        {
            LoopRectView.RefreshContentConstraintCount();
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
            base.OnDestroy();
        }

        private bool CanRefresh()
        {
            if (LoopRectView == null)
            {
                DGame.DLogger.Error($"{GetType().Name} has no LoopScrollRect component.");
                return false;
            }
            if (BaseItemPrefab != null)
            {
                return true;
            }
            DGame.DLogger.Error($"{GetType().Name}.BaseItemPrefab is null.");
            return false;
        }
    }
}
