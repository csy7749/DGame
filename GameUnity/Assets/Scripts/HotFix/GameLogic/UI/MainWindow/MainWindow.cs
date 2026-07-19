using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DGame;
using GameProto;

namespace GameLogic
{
	public class MainWindow : MainWindowAuto
	{
		protected override void BindMemberProperty()
		{
			GameModule.GuideModule.TryStartGuide(9000);
			// DLogger.Warning(Utility.TimeUtil.CalcDiffDay(1769745600, 1769839200));

			var optionsList = new List<Dropdown.OptionData>();

			for (int i = 0; i < (int)LocalAreaType.MAX; i++)
			{
				Dropdown.OptionData optionData = new Dropdown.OptionData();
				optionData.text = GetLanguageStr((LocalAreaType)i);
				optionsList.Add(optionData);
			}

			OnDropdownSelect((int)GameModule.LocalizationModule.CurrentLanguage);
			m_dropDownLanguage.AddOptions(optionsList);
		}

		public string GetLanguageStr(LocalAreaType lang)
		{
			var langStr = "英文";
			switch (lang)
			{
				case LocalAreaType.CN:
					langStr = "中文";
					break;

				case LocalAreaType.EN:
					langStr = "英文";
					break;

				case LocalAreaType.GAT:
					langStr = "繁体";
					break;

				case LocalAreaType.KR:
					langStr = "韩文";
					break;

				case LocalAreaType.JP:
					langStr = "日文";
					break;

				case LocalAreaType.VN:
					langStr = "越南语";
					break;

				case LocalAreaType.INDO:
					langStr = "印尼";
					break;
			}

			return langStr;
		}

		protected override void RegisterEvent()
		{
			AddUIEvent<int>(ILocalization_Event.OnLanguageChanged, _ =>
			{
				RefreshUI();
			});
		}

		protected override void OnDropdownSelect(int val)
		{
			GameModule.LocalizationModule.SetLanguage((LocalAreaType)val);
			// string[] inputDrop = m_dropDownLanguage.captionText.text.Split(' ');
		}

		public void RefreshUI()
		{
			m_textTitle.text = G.R(TextDefine.ID_LABEL_START_GAME);
		}

		#region 事件

		protected override void OnClickStartGameBtn()
		{
			SurvivorStartOptions options = SurvivorStartOptions.ForCharacter(
				SurvivorCharacterId.BeanFarmer);
			SurvivorFlowController.EnterAsync(this, options).Forget();
		}


		protected override void OnClickQuitGameBtn()
		{
		}

		#endregion
	}
}
