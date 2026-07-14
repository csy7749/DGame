using System;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine.SceneManagement;

namespace GameLogic
{
    public static class SurvivorFlowController
    {
        private static SurvivorSession s_session;
        private static bool s_busy;

        public static async UniTask EnterAsync(MainWindow mainWindow)
        {
            if (s_busy)
            {
                DLogger.Error("Survivor enter ignored because another flow operation is running.");
                return;
            }

            s_busy = true;
            try
            {
                await EnterInternalAsync(mainWindow);
            }
            catch
            {
                await DestroySessionAsync();
                await GameModule.SceneModule.UnloadAsync(SurvivorConstants.SceneLocation);
                throw;
            }
            finally
            {
                s_busy = false;
            }
        }

        public static async UniTask RestartAsync()
        {
            if (s_session == null)
            {
                throw new InvalidOperationException("Survivor restart requested without an active session.");
            }

            await s_session.RestartAsync();
        }

        public static async UniTask ExitToMainAsync()
        {
            if (s_busy)
            {
                DLogger.Error("Survivor exit ignored because another flow operation is running.");
                return;
            }

            s_busy = true;
            try
            {
                await DestroySessionAsync();
                await GameModule.SceneModule.UnloadAsync(SurvivorConstants.SceneLocation);
                GameModule.ResourceModule.UnloadUnusedAssets();
                GameModule.UIModule.ShowWindow<MainWindow>();
            }
            finally
            {
                s_busy = false;
            }
        }

        private static async UniTask EnterInternalAsync(MainWindow mainWindow)
        {
            EnsureSceneLocationValid();
            await GameModule.SceneModule.LoadSceneAsync(SurvivorConstants.SceneLocation, LoadSceneMode.Additive);
            if (!GameModule.SceneModule.ActivateScene(SurvivorConstants.SceneLocation))
            {
                throw new InvalidOperationException("Survivor scene activation failed.");
            }

            s_session = new SurvivorSession();
            await s_session.StartAsync();
            mainWindow.Close();
        }

        private static async UniTask DestroySessionAsync()
        {
            if (s_session == null)
            {
                return;
            }

            await s_session.DestroyAsync();
            s_session = null;
        }

        private static void EnsureSceneLocationValid()
        {
            CheckAssetStatus status = GameModule.ResourceModule.ContainsAsset(SurvivorConstants.SceneLocation);
            if (status == CheckAssetStatus.NotExist || status == CheckAssetStatus.Invalid)
            {
                throw new InvalidOperationException($"Survivor scene location is invalid: {status}.");
            }
        }
    }
}
