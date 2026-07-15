using System;
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
        private float m_fireTimer;
        private bool m_isFiring;

        public SurvivorWeaponSystem(SurvivorBattleContext context, CancellationToken cancellationToken)
        {
            m_context = context;
            m_cancellationToken = cancellationToken;
        }

        public async UniTask InitializeAsync()
        {
            await CreateProjectilePoolAsync(SurvivorConstants.ProjectilePrefabLocation);
        }

        public void Tick(float deltaTime)
        {
            if (!m_context.IsRunning || m_isFiring)
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
}
