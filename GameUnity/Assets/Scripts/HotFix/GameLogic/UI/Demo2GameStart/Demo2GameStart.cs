using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameLogic
{
    public class Demo2GameStart : Demo2GameStartAuto
    {
        protected override void OnCreate()
        {
            m_itemCharacter0.SetCharacter(SurvivorCharacterId.BeanFarmer, StartSurvivor);
            m_itemCharacter1.SetCharacter(SurvivorCharacterId.PotatoFarmer, StartSurvivor);
            m_itemCharacter2.SetCharacter(SurvivorCharacterId.RiceFarmer, StartSurvivor);
            m_itemCharacter3.SetCharacter(SurvivorCharacterId.BarleyFarmer, StartSurvivor);
        }

        protected override void OnClickQuitBtn()
        {
            Application.Quit();
        }

        private void StartSurvivor(SurvivorCharacterId characterId)
        {
            SurvivorStartOptions options = SurvivorStartOptions.ForCharacter(characterId);
            SurvivorFlowController.EnterAsync(this, options).Forget();
        }
    }
}
