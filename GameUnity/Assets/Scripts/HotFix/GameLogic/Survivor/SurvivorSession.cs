using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorSession
    {
        private readonly SurvivorStartOptions m_startOptions;
        private CancellationTokenSource m_cancellationTokenSource;
        private SurvivorBattleContext m_context;
        private SurvivorPlayerController m_player;
        private SurvivorRuntimeDriver m_runtimeDriver;
        private SurvivorSpawnSystem m_spawnSystem;
        private SurvivorWeaponSystem m_weaponSystem;
        private float m_hudTimer;
        private bool m_isOpeningLevelUpWindow;
        private bool m_isShowingResult;

        public SurvivorSession(SurvivorStartOptions startOptions)
        {
            m_startOptions = startOptions ?? throw new ArgumentNullException(nameof(startOptions));
        }

        public async UniTask StartAsync()
        {
            Transform battleRoot = FindBattleRoot();
            m_player = FindPlayer(battleRoot);
            m_context = CreateContext(battleRoot, m_player);
            m_cancellationTokenSource = new CancellationTokenSource();
            await InitializeRoundAsync();
            await GameModule.UIModule.ShowWindowAsyncAwait<SurvivorHUDWindow>();
            SendHudChanged();
        }

        public async UniTask RestartAsync()
        {
            GameModule.UIModule.CloseWindow<SurvivorResultWindow>();
            GameModule.UIModule.CloseWindow<SurvivorLevelUpWindow>();
            CancelRoundOperations();
            DestroyRoundSystems();
            m_cancellationTokenSource = new CancellationTokenSource();
            await InitializeRoundAsync();
            SendHudChanged();
        }

        public UniTask DestroyAsync()
        {
            CancelRoundOperations();
            if (m_context != null)
            {
                m_context.Stop();
            }

            DestroyRoundSystems();
            m_cancellationTokenSource = null;
            GameModule.UIModule.CloseWindow<SurvivorHUDWindow>();
            GameModule.UIModule.CloseWindow<SurvivorResultWindow>();
            GameModule.UIModule.CloseWindow<SurvivorLevelUpWindow>();
            DestroyRuntimeRoot();
            m_context = null;
            m_player = null;
            return UniTask.CompletedTask;
        }

        private void CancelRoundOperations()
        {
            if (m_cancellationTokenSource != null)
            {
                m_cancellationTokenSource.Cancel();
                m_cancellationTokenSource.Dispose();
            }
        }

        public void Tick(float deltaTime)
        {
            if (m_context == null)
            {
                return;
            }

            if (m_context.IsResult)
            {
                ShowResultAsync().Forget();
                return;
            }

            if (!m_context.IsPlaying)
            {
                return;
            }

            m_context.AdvanceTime(deltaTime);
            TryFinishByTime();
            if (m_context.IsResult)
            {
                ShowResultAsync().Forget();
                return;
            }

            if (m_spawnSystem != null)
            {
                m_spawnSystem.Tick(deltaTime);
            }

            if (m_weaponSystem != null)
            {
                m_weaponSystem.Tick(deltaTime);
            }

            TryPauseForLevelUp();
            if (m_context.IsResult)
            {
                ShowResultAsync().Forget();
            }

            RefreshHudByInterval(deltaTime);
        }

        private async UniTask InitializeRoundAsync()
        {
            if (m_context == null)
            {
                throw new InvalidOperationException("Survivor context must be created before round initialization.");
            }

            m_player.ApplyStartOptions(m_startOptions);
            m_player.ResetState();
            m_context.Reset();
            m_isShowingResult = false;
            m_isOpeningLevelUpWindow = false;
            CreateRuntimeDriver();
            m_spawnSystem = new SurvivorSpawnSystem(m_context, m_cancellationTokenSource.Token);
            m_weaponSystem = new SurvivorWeaponSystem(m_context, m_cancellationTokenSource.Token);
            await m_spawnSystem.InitializeAsync();
            await m_weaponSystem.InitializeAsync();
        }

        private SurvivorBattleContext CreateContext(Transform battleRoot, SurvivorPlayerController player)
        {
            Transform runtimeRoot = CreateChild(battleRoot, SurvivorConstants.RuntimeRootName);
            return new SurvivorBattleContext(new SurvivorBattleContextOptions
            {
                BattleRoot = battleRoot,
                Player = player,
                RuntimeRoot = runtimeRoot,
                EnemyParent = CreateChild(runtimeRoot, SurvivorConstants.EnemyParentName),
                ProjectileParent = CreateChild(runtimeRoot, SurvivorConstants.ProjectileParentName),
                SpawnPoints = FindSpawnPoints(battleRoot),
                StartOptions = m_startOptions,
            });
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

        private List<Transform> FindSpawnPoints(Transform battleRoot)
        {
            Transform spawner = battleRoot.Find(SurvivorConstants.SpawnerPath);
            if (spawner == null)
            {
                throw new InvalidOperationException("Survivor scene is missing Player/Spawner.");
            }

            List<Transform> points = new List<Transform>();
            for (int i = 0; i < spawner.childCount; i++)
            {
                points.Add(spawner.GetChild(i));
            }

            if (points.Count == 0)
            {
                throw new InvalidOperationException("Survivor spawner has no child points.");
            }

            return points;
        }

        private Transform CreateChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private void CreateRuntimeDriver()
        {
            if (m_runtimeDriver != null)
            {
                m_runtimeDriver.Initialize(this);
                return;
            }

            m_runtimeDriver = m_context.RuntimeRoot.gameObject.AddComponent<SurvivorRuntimeDriver>();
            m_runtimeDriver.Initialize(this);
        }

        public void ApplyLevelUp(SurvivorLevelUpChoice choice)
        {
            if (m_context == null)
            {
                throw new InvalidOperationException("Survivor level up requested without context.");
            }

            m_context.ApplyLevelUp(choice);
            m_player.SetControlEnabled(true);
            GameModule.UIModule.CloseWindow<SurvivorLevelUpWindow>();
            SendHudChanged();
        }

        private void DestroyRoundSystems(bool stopContext = true)
        {
            if (m_context != null && stopContext)
            {
                m_context.Stop();
            }

            if (m_weaponSystem != null)
            {
                m_weaponSystem.Destroy();
            }

            if (m_spawnSystem != null)
            {
                m_spawnSystem.Destroy();
            }

            m_weaponSystem = null;
            m_spawnSystem = null;
        }

        private void DestroyRuntimeRoot()
        {
            if (m_context != null && m_context.RuntimeRoot != null)
            {
                UnityEngine.Object.Destroy(m_context.RuntimeRoot.gameObject);
            }

            m_runtimeDriver = null;
        }

        private void RefreshHudByInterval(float deltaTime)
        {
            m_hudTimer += deltaTime;
            if (m_hudTimer < SurvivorConstants.HudRefreshIntervalSeconds)
            {
                return;
            }

            m_hudTimer = 0f;
            SendHudChanged();
        }

        private void SendHudChanged()
        {
            string title = "Undead Survivor";
            string status = m_context == null
                ? "Demo2 scene ready"
                : BuildHudStatus();
            GameEvent.Get<ISurvivorUI>().OnHudChanged(title, status);
        }

        private void TryFinishByTime()
        {
            if (m_context.ElapsedTime >= SurvivorConstants.BattleDurationSeconds)
            {
                m_context.Finish(true);
            }
        }

        private void TryPauseForLevelUp()
        {
            if (m_isOpeningLevelUpWindow || !m_context.TryPauseForLevelUp())
            {
                return;
            }

            m_player.SetControlEnabled(false);
            ShowLevelUpAsync().Forget();
        }

        private async UniTaskVoid ShowLevelUpAsync()
        {
            m_isOpeningLevelUpWindow = true;
            SurvivorLevelUpWindowData data = new SurvivorLevelUpWindowData
            {
                Level = m_context.Level,
                SelectCallback = ApplyLevelUp,
            };
            await GameModule.UIModule.ShowWindowAsyncAwait<SurvivorLevelUpWindow>(data);
            m_isOpeningLevelUpWindow = false;
            SendHudChanged();
        }

        private async UniTaskVoid ShowResultAsync()
        {
            if (m_isShowingResult)
            {
                return;
            }

            m_isShowingResult = true;
            m_player.SetControlEnabled(false);
            DestroyRoundSystems(false);
            GameModule.UIModule.CloseWindow<SurvivorLevelUpWindow>();
            string result = BuildResultText();
            await GameModule.UIModule.ShowWindowAsyncAwait<SurvivorResultWindow>(result);
            GameEvent.Get<ISurvivorUI>().OnResultChanged(result);
        }

        private string BuildHudStatus()
        {
            return $"{m_startOptions.CharacterName} | Time {m_context.ElapsedTime:0.0}s | HP {m_context.PlayerHealth:0}"
                + $" | Lv {m_context.Level} {m_context.Experience}/{m_context.ExperienceToNextLevel}"
                + $" | Kills {m_context.KillCount} | Enemies {m_context.Enemies.Count}";
        }

        private string BuildResultText()
        {
            string outcome = m_context.IsVictory ? "Victory" : "Defeat";
            return $"{outcome}\nTime {m_context.ElapsedTime:0.0}s\nKills {m_context.KillCount}\nLevel {m_context.Level}";
        }
    }
}
