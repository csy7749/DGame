using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace GameLogic
{
    public sealed class SurvivorHUDWindow : UIWindow
    {
        private Text m_textMoves;
        private Text m_textTarget;
        private Button m_btnReturn;

        protected override UILayer windowLayer => UILayer.UI;

        public override string AssetLocation => "Demo2HUDWindow";

        protected override ModelType GetModelType() => ModelType.NoneType;

        protected override void ScriptGenerator()
        {
            m_textMoves = FindChildComponent<Text>("m_textMoves");
            m_textTarget = FindChildComponent<Text>("m_textTarget");
            m_btnReturn = FindChildComponent<Button>("m_btnReturn");
        }

        protected override void RegisterEvent()
        {
            AddUIEvent<string, string>(ISurvivorUI_Event.OnHudChanged, OnHudChanged);
            m_btnReturn.onClick.AddListener(OnClickReturn);
        }

        private void OnHudChanged(string title, string status)
        {
            m_textMoves.text = title;
            m_textTarget.text = status;
        }

        private void OnClickReturn()
        {
            SurvivorFlowController.ExitToMainAsync().Forget();
        }
    }
}
