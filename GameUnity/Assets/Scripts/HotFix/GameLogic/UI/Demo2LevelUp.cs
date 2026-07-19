using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 幸存者升级窗口，按照参考 Demo 每次随机展示三个可选 Buff。
    /// </summary>
    public class Demo2LevelUp : Demo2LevelUpAuto
    {
        private readonly List<LevelUpItem> m_items = new List<LevelUpItem>();
        private readonly Vector2[] m_slotPositions = new Vector2[SurvivorConstants.VisibleUpgradeCount];
        private SurvivorLevelUpWindowData m_data;

        protected override UILayer windowLayer => UILayer.Top;

        protected override ModelType GetModelType() => ModelType.NormalType75;

        protected override void OnCreate()
        {
            m_items.Add(m_itemLevelUpItem);
            m_items.Add(m_itemLevelUpItem1);
            m_items.Add(m_itemLevelUpItem2);
            m_items.Add(m_itemLevelUpItem3);
            m_items.Add(m_itemLevelUpItem4);
            for (int i = 0; i < m_slotPositions.Length; i++)
            {
                m_slotPositions[i] = GetItemRect(m_items[i]).anchoredPosition;
            }
        }

        protected override void OnRefresh()
        {
            m_data = UserData as SurvivorLevelUpWindowData;
            if (m_data == null)
            {
                throw new InvalidOperationException("Demo2LevelUp requires level-up window data.");
            }

            if (m_data.Options == null || m_data.Options.Count != m_items.Count)
            {
                throw new InvalidOperationException("Demo2LevelUp requires five upgrade options.");
            }

            for (int i = 0; i < m_items.Count; i++)
            {
                SurvivorLevelUpOption option = m_data.Options[i];
                if (option.IsVisible)
                {
                    SetDisplayPosition(m_items[i], option.DisplayOrder);
                }

                m_items[i].SetVisible(option.IsVisible);
                m_items[i].SetData(option, m_data.SelectCallback);
            }
        }

        private void SetDisplayPosition(LevelUpItem item, int displayOrder)
        {
            if (displayOrder < 0 || displayOrder >= m_slotPositions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, null);
            }

            GetItemRect(item).anchoredPosition = m_slotPositions[displayOrder];
        }

        private static RectTransform GetItemRect(LevelUpItem item)
        {
            return item.transform as RectTransform
                ?? throw new InvalidOperationException("LevelUpItem root must be a RectTransform.");
        }
    }
}
