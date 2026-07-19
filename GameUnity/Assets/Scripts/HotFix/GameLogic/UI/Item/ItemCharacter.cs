namespace GameLogic
{
	/// <summary>
	/// ItemCharacter 的 UI 业务逻辑。
	/// </summary>
	public class ItemCharacter : ItemCharacterAuto
	{
		private System.Action<SurvivorCharacterId> m_selectCallback;
		private SurvivorCharacterId m_characterId;

		/// <summary>
		/// 绑定该角色条目代表的角色和父窗口选择回调。
		/// </summary>
		public void SetCharacter(
			SurvivorCharacterId characterId,
			System.Action<SurvivorCharacterId> selectCallback)
		{
			m_characterId = characterId;
			m_selectCallback = selectCallback
				?? throw new System.ArgumentNullException(nameof(selectCallback));
		}

		protected override void OnClickCharacter0Btn()
		{
			if (m_selectCallback == null)
			{
				throw new System.InvalidOperationException(
					"ItemCharacter has not been bound to a selection callback.");
			}

			m_selectCallback.Invoke(m_characterId);
		}
	}
}
