using System;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorSession
    {
        private SurvivorPlayerController m_player;

        public async UniTask StartAsync()
        {
            Transform battleRoot = FindBattleRoot();
            m_player = FindPlayer(battleRoot);
            m_player.ResetState();
            await GameModule.UIModule.ShowWindowAsyncAwait<SurvivorHUDWindow>();
            SendHudChanged();
        }

        public UniTask RestartAsync()
        {
            GameModule.UIModule.CloseWindow<SurvivorResultWindow>();
            if (m_player != null)
            {
                m_player.ResetState();
            }

            SendHudChanged();
            return UniTask.CompletedTask;
        }

        public UniTask DestroyAsync()
        {
            GameModule.UIModule.CloseWindow<SurvivorHUDWindow>();
            GameModule.UIModule.CloseWindow<SurvivorResultWindow>();
            m_player = null;
            return UniTask.CompletedTask;
        }

        private Transform FindBattleRoot()
        {
            GameObject battleRoot = GameObject.Find(SurvivorConstants.BattleRootName);
            if (battleRoot == null)
            {
                throw new InvalidOperationException("Survivor scene is missing SurvivorBattleRoot.");
            }

            return battleRoot.transform;
        }

        private SurvivorPlayerController FindPlayer(Transform battleRoot)
        {
            Transform playerTransform = battleRoot.Find(SurvivorConstants.PlayerName);
            if (playerTransform == null)
            {
                throw new InvalidOperationException("Survivor scene is missing Player.");
            }

            SurvivorPlayerController player = playerTransform.GetComponent<SurvivorPlayerController>();
            if (player == null)
            {
                throw new InvalidOperationException("Survivor Player is missing SurvivorPlayerController.");
            }

            return player;
        }

        private void SendHudChanged()
        {
            string title = "Undead Survivor";
            string status = m_player == null
                ? "Demo2 scene ready"
                : $"Demo2 scene ready | speed {m_player.MoveSpeed:0.0}";
            GameEvent.Get<ISurvivorUI>().OnHudChanged(title, status);
        }
    }
}
