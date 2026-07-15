using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace GameLogic
{
    public sealed class SurvivorResultWindow : UIWindow
    {
        private Text m_textResult;
        private Button m_btnRestart;
        private Button m_btnReturn;

        protected override UILayer windowLayer => UILayer.Top;

        public override string AssetLocation => "Demo2ResultWindow";

        protected override ModelType GetModelType() => ModelType.NormalType75;

        protected override void ScriptGenerator()
        {
            m_textResult = FindChildComponent<Text>("m_textResult");
            m_btnRestart = FindChildComponent<Button>("m_btnRestart");
            m_btnReturn = FindChildComponent<Button>("m_btnReturn");
        }

        protected override void RegisterEvent()
        {
            AddUIEvent<string>(ISurvivorUI_Event.OnResultChanged, OnResultChanged);
            m_btnRestart.onClick.AddListener(OnClickRestart);
            m_btnReturn.onClick.AddListener(OnClickReturn);
        }

        protected override void OnRefresh()
        {
            if (UserData is string result)
            {
                OnResultChanged(result);
            }
        }

        private void OnResultChanged(string result)
        {
            m_textResult.text = result;
        }

        private void OnClickRestart()
        {
            SurvivorFlowController.RestartAsync().Forget();
        }

        private void OnClickReturn()
        {
            SurvivorFlowController.ExitToMainAsync().Forget();
        }
    }

    public sealed class SurvivorLevelUpWindow : UIWindow
    {
        private Text m_textResult;
        private Button m_btnDamage;
        private Button m_btnFireRate;
        private Text m_textDamage;
        private Text m_textFireRate;
        private SurvivorLevelUpWindowData m_data;

        protected override UILayer windowLayer => UILayer.Top;

        public override string AssetLocation => "Demo2ResultWindow";

        protected override ModelType GetModelType() => ModelType.NormalType75;

        protected override void ScriptGenerator()
        {
            m_textResult = FindChildComponent<Text>("m_textResult");
            m_btnDamage = FindChildComponent<Button>("m_btnRestart");
            m_btnFireRate = FindChildComponent<Button>("m_btnReturn");
            m_textDamage = FindChildComponent<Text>(m_btnDamage.transform, "Text");
            m_textFireRate = FindChildComponent<Text>(m_btnFireRate.transform, "Text");
        }

        protected override void RegisterEvent()
        {
            m_btnDamage.onClick.AddListener(OnClickDamage);
            m_btnFireRate.onClick.AddListener(OnClickFireRate);
        }

        protected override void OnRefresh()
        {
            m_data = UserData as SurvivorLevelUpWindowData;
            if (m_data == null)
            {
                throw new InvalidOperationException("Survivor level up window requires data.");
            }

            m_textResult.text = $"Level {m_data.Level}\nChoose an upgrade";
            m_textDamage.text = "Damage +25%";
            m_textFireRate.text = "Fire Rate +15%";
        }

        private void OnClickDamage()
        {
            Select(SurvivorLevelUpChoice.Damage);
        }

        private void OnClickFireRate()
        {
            Select(SurvivorLevelUpChoice.FireRate);
        }

        private void Select(SurvivorLevelUpChoice choice)
        {
            if (m_data == null || m_data.SelectCallback == null)
            {
                throw new InvalidOperationException("Survivor level up callback is missing.");
            }

            m_data.SelectCallback.Invoke(choice);
        }
    }
}
