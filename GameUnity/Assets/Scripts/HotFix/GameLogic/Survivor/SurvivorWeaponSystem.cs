using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DGame;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorWeaponSystem
    {
        private readonly SurvivorBattleContext m_context;
        private readonly CancellationToken m_cancellationToken;
        private readonly SurvivorShovelSystem m_shovelSystem;
        private float m_fireTimer;
        private bool m_isFiring;

        public SurvivorWeaponSystem(SurvivorBattleContext context, CancellationToken cancellationToken)
        {
            m_context = context;
            m_cancellationToken = cancellationToken;
            m_shovelSystem = new SurvivorShovelSystem(context, cancellationToken);
        }

        public async UniTask InitializeAsync()
        {
            await CreateProjectilePoolAsync(SurvivorConstants.ProjectilePrefabLocation);
            await m_shovelSystem.InitializeAsync();
        }

        public void Tick(float deltaTime)
        {
            m_shovelSystem.Tick(deltaTime);
            if (!m_context.IsRunning || m_context.RifleLevel == 0 || m_isFiring)
            {
                return;
            }

            m_fireTimer += deltaTime;
            if (m_fireTimer < m_context.WeaponFireInterval)
            {
                return;
            }

            m_fireTimer = 0f;
            FireAsync().Forget();
        }

        public void Destroy()
        {
            m_shovelSystem.Destroy();
            for (int i = m_context.Projectiles.Count - 1; i >= 0; i--)
            {
                RecycleProjectile(m_context.Projectiles[i]);
            }

            GameModule.GameObjectPool.DestroyPool(SurvivorConstants.ProjectilePrefabLocation);
        }

        private async UniTask CreateProjectilePoolAsync(string location)
        {
            await GameModule.GameObjectPool.CreateGameObjectPoolAsync(
                location,
                SurvivorConstants.ProjectilePoolInitCapacity,
                SurvivorConstants.ProjectilePoolMaxCapacity,
                SurvivorConstants.PoolAutoDestroyTime,
                ct: m_cancellationToken);
        }

        private async UniTaskVoid FireAsync()
        {
            SurvivorEnemyController target = m_context.FindNearestEnemy(m_context.Player.transform.position);
            if (target == null)
            {
                return;
            }

            m_isFiring = true;
            try
            {
                await SpawnProjectileAsync(target);
            }
            finally
            {
                m_isFiring = false;
            }
        }

        private async UniTask SpawnProjectileAsync(SurvivorEnemyController target)
        {
            Transform playerTransform = m_context.Player.transform;
            Vector3 localPosition = m_context.ProjectileParent.InverseTransformPoint(playerTransform.position);
            Vector2 direction = target.transform.position - playerTransform.position;
            GameObject projectileGo = await GameModule.GameObjectPool.SpawnAsync(
                SurvivorConstants.ProjectilePrefabLocation,
                m_context.ProjectileParent,
                localPosition,
                Quaternion.FromToRotation(Vector3.up, direction),
                m_cancellationToken);
            if (m_cancellationToken.IsCancellationRequested)
            {
                RecycleSpawnedObject(projectileGo);
                return;
            }

            InitializeProjectile(projectileGo, direction);
        }

        private void InitializeProjectile(GameObject projectileGo, Vector2 direction)
        {
            if (projectileGo == null)
            {
                throw new InvalidOperationException("Survivor projectile spawn returned null.");
            }

            SurvivorProjectileController projectile = projectileGo.GetComponent<SurvivorProjectileController>()
                ?? projectileGo.AddComponent<SurvivorProjectileController>();
            projectile.ResetState(new SurvivorProjectileOptions
            {
                Damage = m_context.ProjectileDamage,
                Pierce = m_context.ProjectilePierce,
                Direction = direction,
                Speed = m_context.ProjectileSpeed,
                Lifetime = SurvivorConstants.ProjectileLifetimeSeconds,
                CanAct = () => m_context.IsPlaying,
                RecycleCallback = RecycleProjectile,
            });
            m_context.RegisterProjectile(projectile);
        }

        private void RecycleProjectile(SurvivorProjectileController projectile)
        {
            if (projectile == null)
            {
                return;
            }

            m_context.UnregisterProjectile(projectile);
            GameModule.GameObjectPool.Recycle(projectile.gameObject);
        }

        private static void RecycleSpawnedObject(GameObject projectileGo)
        {
            if (projectileGo != null)
            {
                GameModule.GameObjectPool.Recycle(projectileGo);
            }
        }
    }

    /// <summary>
    /// 管理围绕玩家旋转的铲子武器，并在升级后同步数量、伤害与角速度。
    /// </summary>
    internal sealed class SurvivorShovelSystem
    {
        private readonly List<SurvivorOrbitProjectileController> m_shovels = new();
        private readonly SurvivorBattleContext m_context;
        private readonly CancellationToken m_cancellationToken;
        private Transform m_orbitRoot;
        private int m_appliedLevel = -1;
        private bool m_isRefreshing;

        public SurvivorShovelSystem(
            SurvivorBattleContext context,
            CancellationToken cancellationToken)
        {
            m_context = context;
            m_cancellationToken = cancellationToken;
        }

        public async UniTask InitializeAsync()
        {
            CreateOrbitRoot();
            await GameModule.GameObjectPool.CreateGameObjectPoolAsync(
                SurvivorConstants.ShovelPrefabLocation,
                SurvivorConstants.ShovelPoolCapacity,
                SurvivorConstants.ShovelPoolCapacity,
                SurvivorConstants.PoolAutoDestroyTime,
                ct: m_cancellationToken);
            await RefreshAsync();
        }

        public void Tick(float deltaTime)
        {
            if (!m_context.IsRunning)
            {
                return;
            }

            m_orbitRoot.Rotate(Vector3.back, m_context.ShovelAngularSpeed * deltaTime);
            if (m_appliedLevel != m_context.ShovelLevel && !m_isRefreshing)
            {
                RefreshAsync().Forget();
            }
        }

        public void Destroy()
        {
            while (m_shovels.Count > 0)
            {
                RecycleLastShovel();
            }

            if (m_orbitRoot != null)
            {
                UnityEngine.Object.Destroy(m_orbitRoot.gameObject);
                m_orbitRoot = null;
            }

            GameModule.GameObjectPool.DestroyPool(SurvivorConstants.ShovelPrefabLocation);
        }

        private void CreateOrbitRoot()
        {
            GameObject root = new GameObject(SurvivorConstants.ShovelOrbitRootName);
            m_orbitRoot = root.transform;
            m_orbitRoot.SetParent(m_context.Player.transform, false);
        }

        private async UniTask RefreshAsync()
        {
            m_isRefreshing = true;
            try
            {
                await AdjustShovelCountAsync(m_context.ShovelCount);
                UpdateShovelDamage();
                ArrangeShovels();
                m_appliedLevel = m_context.ShovelLevel;
            }
            finally
            {
                m_isRefreshing = false;
            }
        }

        private async UniTask AdjustShovelCountAsync(int targetCount)
        {
            while (m_shovels.Count < targetCount)
            {
                m_cancellationToken.ThrowIfCancellationRequested();
                await SpawnShovelAsync();
            }

            while (m_shovels.Count > targetCount)
            {
                RecycleLastShovel();
            }
        }

        private async UniTask SpawnShovelAsync()
        {
            GameObject shovelGo = await GameModule.GameObjectPool.SpawnAsync(
                SurvivorConstants.ShovelPrefabLocation,
                m_orbitRoot,
                m_cancellationToken);
            if (m_cancellationToken.IsCancellationRequested)
            {
                RecycleSpawnedObject(shovelGo);
                m_cancellationToken.ThrowIfCancellationRequested();
            }

            if (shovelGo == null)
            {
                throw new InvalidOperationException("Survivor shovel spawn returned null.");
            }

            SurvivorOrbitProjectileController shovel =
                shovelGo.GetComponent<SurvivorOrbitProjectileController>()
                ?? shovelGo.AddComponent<SurvivorOrbitProjectileController>();
            shovel.ResetState(new SurvivorOrbitProjectileOptions
            {
                Damage = m_context.ShovelDamage,
                CanAct = () => m_context.IsPlaying,
            });
            m_shovels.Add(shovel);
        }

        private void UpdateShovelDamage()
        {
            for (int i = 0; i < m_shovels.Count; i++)
            {
                m_shovels[i].SetDamage(m_context.ShovelDamage);
            }
        }

        private void ArrangeShovels()
        {
            for (int i = 0; i < m_shovels.Count; i++)
            {
                float angle = 360f * i / m_shovels.Count;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
                Transform shovel = m_shovels[i].transform;
                shovel.SetLocalPositionAndRotation(
                    rotation * (Vector3.up * SurvivorConstants.ShovelOrbitRadius),
                    rotation);
            }
        }

        private void RecycleLastShovel()
        {
            int index = m_shovels.Count - 1;
            SurvivorOrbitProjectileController shovel = m_shovels[index];
            m_shovels.RemoveAt(index);
            shovel.MarkRecycled();
            GameModule.GameObjectPool.Recycle(shovel.gameObject);
        }

        private static void RecycleSpawnedObject(GameObject shovelGo)
        {
            if (shovelGo != null)
            {
                GameModule.GameObjectPool.Recycle(shovelGo);
            }
        }
    }
}
