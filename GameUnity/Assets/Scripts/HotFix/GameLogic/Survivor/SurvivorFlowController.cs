using System;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLogic
{
    public static class SurvivorFlowController
    {
        private static SurvivorSession s_session;
        private static bool s_busy;

        public static async UniTask EnterAsync(UIWindow launcherWindow, SurvivorStartOptions startOptions)
        {
            if (launcherWindow == null)
            {
                throw new ArgumentNullException(nameof(launcherWindow));
            }

            if (startOptions == null)
            {
                throw new ArgumentNullException(nameof(startOptions));
            }

            if (s_busy)
            {
                DLogger.Error("Survivor enter ignored because another flow operation is running.");
                return;
            }

            s_busy = true;
            try
            {
                await EnterInternalAsync(launcherWindow, startOptions);
            }
            catch
            {
                await DestroySessionAsync();
                await GameModule.SceneModule.UnloadAsync(SurvivorConstants.SceneLocation);
                RestoreUiCameraAsBase();
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
                RestoreUiCameraAsBase();
                GameModule.ResourceModule.UnloadUnusedAssets();
                GameModule.UIModule.ShowWindow<MainWindow>();
            }
            finally
            {
                s_busy = false;
            }
        }

        private static async UniTask EnterInternalAsync(
            UIWindow launcherWindow,
            SurvivorStartOptions startOptions)
        {
            EnsureSceneLocationValid();
            Scene scene = await GameModule.SceneModule.LoadSceneAsync(
                SurvivorConstants.SceneLocation,
                LoadSceneMode.Additive);
            if (!GameModule.SceneModule.ActivateScene(SurvivorConstants.SceneLocation))
            {
                throw new InvalidOperationException("Survivor scene activation failed.");
            }

            StackUiCameraOnSceneCamera(scene);
            s_session = new SurvivorSession(startOptions);
            await s_session.StartAsync();
            launcherWindow.Close();
        }

        private static void StackUiCameraOnSceneCamera(Scene scene)
        {
            Camera baseCamera = FindSceneBaseCamera(scene);
            Camera uiCamera = GameModule.UIModule.UICamera;
            if (uiCamera == null)
            {
                throw new InvalidOperationException("UIRoot is missing UICamera.");
            }

            DGame.Utility.CameraUtil.StackOverlayCamera(baseCamera, uiCamera);
        }

        private static Camera FindSceneBaseCamera(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
                for (int j = 0; j < cameras.Length; j++)
                {
                    if (cameras[j].CompareTag(SurvivorConstants.MainCameraTag))
                    {
                        return cameras[j];
                    }
                }
            }

            throw new InvalidOperationException("Survivor scene is missing MainCamera.");
        }

        private static void RestoreUiCameraAsBase()
        {
            Camera uiCamera = GameModule.UIModule.UICamera;
            if (uiCamera == null)
            {
                throw new InvalidOperationException("UIRoot is missing UICamera.");
            }

            DGame.Utility.CameraUtil.SetCameraRenderTypeBase(uiCamera);
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
