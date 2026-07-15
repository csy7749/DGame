using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public enum SurvivorCharacterId
    {
        BeanFarmer,
        PotatoFarmer,
        RiceFarmer,
        BarleyFarmer,
    }

    public sealed class SurvivorStartOptions
    {
        private SurvivorStartOptions()
        {
        }

        public SurvivorCharacterId CharacterId { get; private set; }

        public string CharacterName { get; private set; }

        public float MoveSpeedMultiplier { get; private set; } = 1f;

        public float DamageMultiplier { get; private set; } = 1f;

        public float FireIntervalMultiplier { get; private set; } = 1f;

        public float ProjectileSpeedMultiplier { get; private set; } = 1f;

        public int ExtraProjectilePierce { get; private set; }

        public SurvivorLevelUpChoice InitialLevelUpChoice { get; private set; }

        public static SurvivorStartOptions ForCharacter(SurvivorCharacterId characterId)
        {
            switch (characterId)
            {
                case SurvivorCharacterId.BeanFarmer:
                    return BeanFarmer();
                case SurvivorCharacterId.PotatoFarmer:
                    return PotatoFarmer();
                case SurvivorCharacterId.RiceFarmer:
                    return RiceFarmer();
                case SurvivorCharacterId.BarleyFarmer:
                    return BarleyFarmer();
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterId), characterId, null);
            }
        }

        private static SurvivorStartOptions BeanFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.BeanFarmer,
                CharacterName = "豆农",
                MoveSpeedMultiplier = SurvivorConstants.BeanFarmerMoveSpeedMultiplier,
                InitialLevelUpChoice = SurvivorLevelUpChoice.Damage,
            };
        }

        private static SurvivorStartOptions PotatoFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.PotatoFarmer,
                CharacterName = "土豆农夫",
                FireIntervalMultiplier = SurvivorConstants.PotatoFarmerFireIntervalMultiplier,
                ProjectileSpeedMultiplier = SurvivorConstants.PotatoFarmerProjectileSpeedMultiplier,
                InitialLevelUpChoice = SurvivorLevelUpChoice.FireRate,
            };
        }

        private static SurvivorStartOptions RiceFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.RiceFarmer,
                CharacterName = "稻农",
                DamageMultiplier = SurvivorConstants.RiceFarmerDamageMultiplier,
                InitialLevelUpChoice = SurvivorLevelUpChoice.Damage,
            };
        }

        private static SurvivorStartOptions BarleyFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.BarleyFarmer,
                CharacterName = "大麦农夫",
                ExtraProjectilePierce = SurvivorConstants.BarleyFarmerExtraProjectilePierce,
                InitialLevelUpChoice = SurvivorLevelUpChoice.FireRate,
            };
        }
    }

    public sealed class SurvivorBattleContext
    {
        private readonly List<Transform> m_spawnPoints;
        private readonly List<SurvivorEnemyController> m_enemies = new List<SurvivorEnemyController>();
        private readonly List<SurvivorProjectileController> m_projectiles = new List<SurvivorProjectileController>();

        public SurvivorBattleContext(SurvivorBattleContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            BattleRoot = options.BattleRoot;
            Player = options.Player;
            RuntimeRoot = options.RuntimeRoot;
            EnemyParent = options.EnemyParent;
            ProjectileParent = options.ProjectileParent;
            StartOptions = options.StartOptions
                ?? throw new ArgumentNullException(nameof(options.StartOptions));
            m_spawnPoints = new List<Transform>(options.SpawnPoints);
        }

        public Transform BattleRoot { get; }

        public SurvivorPlayerController Player { get; }

        public Transform RuntimeRoot { get; }

        public Transform EnemyParent { get; }

        public Transform ProjectileParent { get; }

        public SurvivorStartOptions StartOptions { get; }

        public IReadOnlyList<Transform> SpawnPoints => m_spawnPoints;

        public IReadOnlyList<SurvivorEnemyController> Enemies => m_enemies;

        public IReadOnlyList<SurvivorProjectileController> Projectiles => m_projectiles;

        public int KillCount { get; private set; }

        public int Level { get; private set; }

        public int Experience { get; private set; }

        public int ExperienceToNextLevel { get; private set; }

        public float PlayerHealth { get; private set; }

        public float DamageMultiplier { get; private set; }

        public float FireIntervalMultiplier { get; private set; }

        public float ElapsedTime { get; private set; }

        public SurvivorBattleState State { get; private set; }

        public bool IsRunning => State == SurvivorBattleState.Playing;

        public bool IsPlaying => State == SurvivorBattleState.Playing;

        public bool IsResult => State == SurvivorBattleState.Result;

        public bool IsVictory { get; private set; }

        public bool HasPendingLevelUp { get; private set; }

        public float ProjectileDamage => SurvivorConstants.ProjectileBaseDamage * DamageMultiplier;

        public int ProjectilePierce =>
            SurvivorConstants.ProjectilePierce + StartOptions.ExtraProjectilePierce;

        public float ProjectileSpeed =>
            SurvivorConstants.ProjectileSpeed * StartOptions.ProjectileSpeedMultiplier;

        public float WeaponFireInterval =>
            SurvivorConstants.WeaponBaseFireIntervalSeconds * FireIntervalMultiplier;

        public void Stop() => State = SurvivorBattleState.Stopped;

        public void Reset()
        {
            KillCount = 0;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = SurvivorConstants.BaseExperienceToNextLevel;
            PlayerHealth = SurvivorConstants.PlayerMaxHealth;
            DamageMultiplier = StartOptions.DamageMultiplier;
            FireIntervalMultiplier = StartOptions.FireIntervalMultiplier;
            ElapsedTime = 0f;
            IsVictory = false;
            HasPendingLevelUp = false;
            m_enemies.Clear();
            m_projectiles.Clear();
            State = SurvivorBattleState.Playing;
            ApplyLevelUpChoice(StartOptions.InitialLevelUpChoice);
        }

        public void AdvanceTime(float deltaTime)
        {
            if (!IsPlaying)
            {
                return;
            }

            ElapsedTime += deltaTime;
        }

        public void RegisterEnemy(SurvivorEnemyController enemy)
        {
            if (enemy != null && !m_enemies.Contains(enemy))
            {
                m_enemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(SurvivorEnemyController enemy)
        {
            m_enemies.Remove(enemy);
        }

        public void RegisterProjectile(SurvivorProjectileController projectile)
        {
            if (projectile != null && !m_projectiles.Contains(projectile))
            {
                m_projectiles.Add(projectile);
            }
        }

        public void UnregisterProjectile(SurvivorProjectileController projectile)
        {
            m_projectiles.Remove(projectile);
        }

        public void RecordEnemyDefeated()
        {
            KillCount++;
            AddExperience(SurvivorConstants.EnemyExperience);
        }

        public void ApplyPlayerDamage(float damage)
        {
            if (!IsPlaying)
            {
                return;
            }

            PlayerHealth = Mathf.Max(0f, PlayerHealth - damage);
            if (PlayerHealth <= 0f)
            {
                Finish(false);
            }
        }

        public void Finish(bool victory)
        {
            if (IsResult)
            {
                return;
            }

            IsVictory = victory;
            State = SurvivorBattleState.Result;
        }

        public bool TryPauseForLevelUp()
        {
            if (!IsPlaying || !HasPendingLevelUp)
            {
                return false;
            }

            State = SurvivorBattleState.LevelUpPaused;
            return true;
        }

        public void ApplyLevelUp(SurvivorLevelUpChoice choice)
        {
            if (State != SurvivorBattleState.LevelUpPaused)
            {
                throw new InvalidOperationException("Survivor level up applied outside level-up state.");
            }

            ApplyLevelUpChoice(choice);
            HasPendingLevelUp = false;
            State = SurvivorBattleState.Playing;
        }

        public SurvivorEnemyController FindNearestEnemy(Vector3 origin)
        {
            SurvivorEnemyController nearest = null;
            float nearestSqrDistance = float.MaxValue;
            for (int i = 0; i < m_enemies.Count; i++)
            {
                SurvivorEnemyController enemy = m_enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float sqrDistance = (enemy.transform.position - origin).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearest = enemy;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }

        private void AddExperience(int amount)
        {
            Experience += amount;
            if (Experience < ExperienceToNextLevel)
            {
                return;
            }

            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel += SurvivorConstants.ExperienceGrowthPerLevel;
            HasPendingLevelUp = true;
        }

        private void ApplyLevelUpChoice(SurvivorLevelUpChoice choice)
        {
            if (choice == SurvivorLevelUpChoice.Damage)
            {
                DamageMultiplier += SurvivorConstants.LevelUpDamageMultiplierBonus;
                return;
            }

            FireIntervalMultiplier = Mathf.Max(
                SurvivorConstants.MinimumFireIntervalMultiplier,
                FireIntervalMultiplier * SurvivorConstants.LevelUpFireIntervalMultiplier);
        }
    }

    public enum SurvivorBattleState
    {
        Stopped,
        Playing,
        LevelUpPaused,
        Result,
    }

    public enum SurvivorLevelUpChoice
    {
        Damage,
        FireRate,
    }

    public sealed class SurvivorLevelUpWindowData
    {
        public int Level { get; set; }

        public Action<SurvivorLevelUpChoice> SelectCallback { get; set; }
    }

    public sealed class SurvivorBattleContextOptions
    {
        public Transform BattleRoot { get; set; }

        public SurvivorPlayerController Player { get; set; }

        public Transform RuntimeRoot { get; set; }

        public Transform EnemyParent { get; set; }

        public Transform ProjectileParent { get; set; }

        public IReadOnlyList<Transform> SpawnPoints { get; set; } = Array.Empty<Transform>();

        public SurvivorStartOptions StartOptions { get; set; }
    }
}
