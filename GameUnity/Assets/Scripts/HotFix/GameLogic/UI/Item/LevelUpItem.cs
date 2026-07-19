using System;

namespace GameLogic
{
    /// <summary>
    /// 单个升级选项，负责展示 Buff 等级与转发选择结果。
    /// </summary>
    public class LevelUpItem : LevelUpItemAuto
    {
        private SurvivorLevelUpOption m_option;
        private Action<SurvivorLevelUpChoice> m_selectCallback;

        public void SetData(
            SurvivorLevelUpOption option,
            Action<SurvivorLevelUpChoice> selectCallback)
        {
            m_option = option ?? throw new ArgumentNullException(nameof(option));
            m_selectCallback = selectCallback ?? throw new ArgumentNullException(nameof(selectCallback));
            m_textLevel.text = option.LevelText;
            m_textName.text = option.Name;
            m_textDesc.text = option.Description;
            m_btnLevelUpItem.interactable = true;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        protected override void OnClickLevelUpItemBtn()
        {
            if (m_option == null || m_selectCallback == null)
            {
                throw new InvalidOperationException("Level-up item has not been initialized.");
            }

            m_btnLevelUpItem.interactable = false;
            m_selectCallback.Invoke(m_option.Choice);
        }
    }
}
