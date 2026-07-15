using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorSpawnSystem
    {
        private readonly SurvivorBattleContext m_context;
        private readonly CancellationToken m_cancellationToken;
        private float m_spawnTimer;
        private bool m_isSpawning;

        public SurvivorSpawnSystem(SurvivorBattleContext context, CancellationToken cancellationToken)
        {
            m_context = context;
            m_cancellationToken = cancellationToken;
        }

        public async UniTask InitializeAsync()
        {
            await GameModule.GameObjectPool.CreateGameObjectPoolAsync(
                SurvivorConstants.EnemyPrefabLocation,
                SurvivorConstants.EnemyPoolInitCapacity,
                SurvivorConstants.EnemyPoolMaxCapacity,
                SurvivorConstants.PoolAutoDestroyTime,
                ct: m_cancellationToken);
        }

        public void Tick(float deltaTime)
        {
            if (!m_context.IsRunning || m_isSpawning)
            {
                return;
            }

            if (m_context.Enemies.Count >= SurvivorConstants.MaxActiveEnemies)
            {
                return;
            }

            m_spawnTimer += deltaTime;
            SurvivorSpawnWave wave = GetWave();
            if (m_spawnTimer < wave.SpawnInterval)
            {
                return;
            }

            m_spawnTimer = 0f;
            SpawnEnemyAsync(wave).Forget();
        }

        public void Destroy()
        {
            for (int i = m_context.Enemies.Count - 1; i >= 0; i--)
            {
                RecycleEnemy(m_context.Enemies[i]);
            }

            GameModule.GameObjectPool.DestroyPool(SurvivorConstants.EnemyPrefabLocation);
        }

        private async UniTaskVoid SpawnEnemyAsync(SurvivorSpawnWave wave)
        {
            m_isSpawning = true;
            try
            {
                Transform spawnPoint = PickSpawnPoint();
                Vector3 localPosition = m_context.EnemyParent.InverseTransformPoint(spawnPoint.position);
                GameObject enemyGo = await GameModule.GameObjectPool.SpawnAsync(
                    SurvivorConstants.EnemyPrefabLocation,
                    m_context.EnemyParent,
                    localPosition,
                    Quaternion.identity,
                    m_cancellationToken);
                if (m_cancellationToken.IsCancellationRequested)
                {
                    RecycleSpawnedObject(enemyGo);
                    return;
                }

                InitializeEnemy(enemyGo, wave);
            }
            finally
            {
                m_isSpawning = false;
            }
        }

        private void InitializeEnemy(GameObject enemyGo, SurvivorSpawnWave wave)
        {
            if (enemyGo == null)
            {
                if (m_context.Enemies.Count >= SurvivorConstants.MaxActiveEnemies)
                {
                    return;
                }

                throw new InvalidOperationException("Survivor enemy spawn returned null.");
            }

            SurvivorEnemyController enemy = enemyGo.GetComponent<SurvivorEnemyController>()
                ?? enemyGo.AddComponent<SurvivorEnemyController>();
            enemy.ResetState(new SurvivorEnemyOptions
            {
                Target = m_context.Player.transform,
                CanAct = () => m_context.IsPlaying,
                MaxHealth = wave.Health,
                MoveSpeed = wave.MoveSpeed,
                ContactDamagePerSecond = SurvivorConstants.EnemyContactDamagePerSecond,
                ContactDamageDistance = SurvivorConstants.EnemyContactDamageDistance,
                DefeatedCallback = OnEnemyDefeated,
                PlayerDamageCallback = m_context.ApplyPlayerDamage,
            });
            m_context.RegisterEnemy(enemy);
        }

        private void OnEnemyDefeated(SurvivorEnemyController enemy)
        {
            m_context.RecordEnemyDefeated();
            RecycleDefeatedEnemyAsync(enemy, enemy.RecycleToken).Forget();
        }

        private async UniTaskVoid RecycleDefeatedEnemyAsync(
            SurvivorEnemyController enemy,
            int recycleToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(SurvivorConstants.EnemyDeadRecycleDelaySeconds));
            if (m_cancellationToken.IsCancellationRequested || enemy == null || !enemy.CanRecycle(recycleToken))
            {
                return;
            }

            RecycleEnemy(enemy);
        }

        private void RecycleEnemy(SurvivorEnemyController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            m_context.UnregisterEnemy(enemy);
            enemy.MarkRecycled();
            GameModule.GameObjectPool.Recycle(enemy.gameObject);
        }

        private static void RecycleSpawnedObject(GameObject enemyGo)
        {
            if (enemyGo != null)
            {
                GameModule.GameObjectPool.Recycle(enemyGo);
            }
        }

        private Transform PickSpawnPoint()
        {
            if (m_context.SpawnPoints.Count == 0)
            {
                throw new InvalidOperationException("Survivor spawner has no spawn points.");
            }

            int index = UnityEngine.Random.Range(0, m_context.SpawnPoints.Count);
            return m_context.SpawnPoints[index];
        }

        private SurvivorSpawnWave GetWave()
        {
            if (m_context.ElapsedTime < SurvivorConstants.FirstWaveDurationSeconds)
            {
                return SurvivorSpawnWave.First;
            }

            return SurvivorSpawnWave.Second;
        }
    }

    public readonly struct SurvivorSpawnWave
    {
        private SurvivorSpawnWave(float spawnInterval, float health, float moveSpeed)
        {
            SpawnInterval = spawnInterval;
            Health = health;
            MoveSpeed = moveSpeed;
        }

        public float SpawnInterval { get; }

        public float Health { get; }

        public float MoveSpeed { get; }

        public static SurvivorSpawnWave First => new SurvivorSpawnWave(
            SurvivorConstants.FirstWaveSpawnIntervalSeconds,
            SurvivorConstants.FirstWaveEnemyHealth,
            SurvivorConstants.FirstWaveEnemySpeed);

        public static SurvivorSpawnWave Second => new SurvivorSpawnWave(
            SurvivorConstants.SecondWaveSpawnIntervalSeconds,
            SurvivorConstants.SecondWaveEnemyHealth,
            SurvivorConstants.SecondWaveEnemySpeed);
    }
}
