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
}
