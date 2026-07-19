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
                InitialLevelUpChoice = SurvivorLevelUpChoice.Shovel,
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
                InitialLevelUpChoice = SurvivorLevelUpChoice.Rifle,
            };
        }

        private static SurvivorStartOptions RiceFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.RiceFarmer,
                CharacterName = "稻农",
                DamageMultiplier = SurvivorConstants.RiceFarmerDamageMultiplier,
                InitialLevelUpChoice = SurvivorLevelUpChoice.Shovel,
            };
        }

        private static SurvivorStartOptions BarleyFarmer()
        {
            return new SurvivorStartOptions
            {
                CharacterId = SurvivorCharacterId.BarleyFarmer,
                CharacterName = "大麦农夫",
                ExtraProjectilePierce = SurvivorConstants.BarleyFarmerExtraProjectilePierce,
                InitialLevelUpChoice = SurvivorLevelUpChoice.Rifle,
            };
        }
    }

    public sealed class SurvivorBattleContext
    {
        private readonly List<Transform> m_spawnPoints;
        private readonly List<SurvivorEnemyController> m_enemies = new List<SurvivorEnemyController>();
        private readonly List<SurvivorProjectileController> m_projectiles = new List<SurvivorProjectileController>();
        private readonly int[] m_upgradeLevels = new int[4];

        private static readonly float[] s_shovelDamageRates = { 0.5f, 1f, 1.5f, 2f, 3f };
        private static readonly int[] s_shovelCountBonuses = { 1, 1, 1, 1, 2 };
        private static readonly float[] s_rifleDamageRates = { 0.35f, 0.7f, 1f, 1.4f, 2f };
        private static readonly int[] s_riflePierceBonuses = { 1, 1, 2, 3, 4 };
        private static readonly float[] s_fireRateBonuses = { 0.1f, 0.2f, 0.35f, 0.5f, 0.75f };
        private static readonly float[] s_moveSpeedBonuses = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

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

        public float MoveSpeedMultiplier { get; private set; }

        public int ExtraProjectilePierce { get; private set; }

        public float ElapsedTime { get; private set; }

        public SurvivorBattleState State { get; private set; }

        public bool IsRunning => State == SurvivorBattleState.Playing;

        public bool IsPlaying => State == SurvivorBattleState.Playing;

        public bool IsResult => State == SurvivorBattleState.Result;

        public bool IsVictory { get; private set; }

        public bool HasPendingLevelUp { get; private set; }

        public int ShovelLevel => m_upgradeLevels[(int)SurvivorLevelUpChoice.Shovel];

        public int RifleLevel => m_upgradeLevels[(int)SurvivorLevelUpChoice.Rifle];

        public float ShovelDamage => SurvivorConstants.ShovelBaseDamage
            * DamageMultiplier
            * (1f + GetAppliedWeaponRate(SurvivorLevelUpChoice.Shovel, s_shovelDamageRates));

        public int ShovelCount => CalculateShovelCount();

        public float ShovelAngularSpeed => SurvivorConstants.ShovelBaseAngularSpeed
            * (1f + GetAppliedRate(SurvivorLevelUpChoice.FireRate, s_fireRateBonuses));

        public float ProjectileDamage => SurvivorConstants.ProjectileBaseDamage
            * DamageMultiplier
            * (1f + GetAppliedWeaponRate(SurvivorLevelUpChoice.Rifle, s_rifleDamageRates));

        public int ProjectilePierce =>
            SurvivorConstants.ProjectilePierce + ExtraProjectilePierce;

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
            MoveSpeedMultiplier = StartOptions.MoveSpeedMultiplier;
            ExtraProjectilePierce = StartOptions.ExtraProjectilePierce;
            ElapsedTime = 0f;
            IsVictory = false;
            HasPendingLevelUp = false;
            m_enemies.Clear();
            m_projectiles.Clear();
            Array.Clear(m_upgradeLevels, 0, m_upgradeLevels.Length);
            State = SurvivorBattleState.Stopped;
            ApplyLevelUpChoice(StartOptions.InitialLevelUpChoice);
        }

        /// <summary>
        /// 在场景对象、对象池和武器系统全部初始化完成后启动本回合。
        /// </summary>
        public void StartRound()
        {
            if (State != SurvivorBattleState.Stopped)
            {
                throw new InvalidOperationException($"Survivor round cannot start from state {State}.");
            }

            State = SurvivorBattleState.Playing;
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

        /// <summary>
        /// 按参考 Demo 随机选择三个升级候选，并保留随机结果中的展示顺序。
        /// </summary>
        public IReadOnlyList<SurvivorLevelUpOption> CreateLevelUpOptions()
        {
            List<SurvivorLevelUpChoice> candidates = CreateUpgradeCandidates();
            Shuffle(candidates);
            Dictionary<SurvivorLevelUpChoice, int> displayOrders =
                new Dictionary<SurvivorLevelUpChoice, int>();
            int visibleCount = Mathf.Min(SurvivorConstants.VisibleUpgradeCount, candidates.Count);
            for (int i = 0; i < visibleCount; i++)
            {
                displayOrders.Add(candidates[i], i);
            }

            return CreateOptionList(displayOrders);
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
            if (choice == SurvivorLevelUpChoice.Heal)
            {
                PlayerHealth = SurvivorConstants.PlayerMaxHealth;
                return;
            }

            int index = (int)choice;
            if (m_upgradeLevels[index] >= SurvivorConstants.MaxUpgradeLevel)
            {
                throw new InvalidOperationException($"Upgrade {choice} has reached max level.");
            }

            m_upgradeLevels[index]++;
            RecalculateBuffs();
        }

        private List<SurvivorLevelUpChoice> CreateUpgradeCandidates()
        {
            List<SurvivorLevelUpChoice> candidates = new List<SurvivorLevelUpChoice>();
            bool hasMaxLevelUpgrade = false;
            for (int i = 0; i < m_upgradeLevels.Length; i++)
            {
                if (m_upgradeLevels[i] >= SurvivorConstants.MaxUpgradeLevel)
                {
                    hasMaxLevelUpgrade = true;
                    continue;
                }

                candidates.Add((SurvivorLevelUpChoice)i);
            }

            if (hasMaxLevelUpgrade)
            {
                candidates.Add(SurvivorLevelUpChoice.Heal);
            }

            return candidates;
        }

        private IReadOnlyList<SurvivorLevelUpOption> CreateOptionList(
            Dictionary<SurvivorLevelUpChoice, int> displayOrders)
        {
            return new[]
            {
                CreateOption(SurvivorLevelUpChoice.Shovel, "铲子", displayOrders),
                CreateOption(SurvivorLevelUpChoice.Rifle, "猎枪", displayOrders),
                CreateOption(SurvivorLevelUpChoice.FireRate, "技术手套", displayOrders),
                CreateOption(SurvivorLevelUpChoice.MoveSpeed, "战斗靴", displayOrders),
                CreateOption(SurvivorLevelUpChoice.Heal, "饮料", displayOrders),
            };
        }

        private SurvivorLevelUpOption CreateOption(
            SurvivorLevelUpChoice choice,
            string name,
            Dictionary<SurvivorLevelUpChoice, int> displayOrders)
        {
            bool isHeal = choice == SurvivorLevelUpChoice.Heal;
            int level = isHeal ? 0 : m_upgradeLevels[(int)choice];
            bool isMaxLevel = !isHeal && level >= SurvivorConstants.MaxUpgradeLevel;
            bool isVisible = displayOrders.TryGetValue(choice, out int displayOrder);
            return new SurvivorLevelUpOption
            {
                Choice = choice,
                Name = name,
                Description = isMaxLevel ? "已满级" : BuildUpgradeDescription(choice, level),
                LevelText = isHeal ? "完全恢复" : isMaxLevel ? "MAX" : $"Lv.{level + 1}",
                IsVisible = !isMaxLevel && isVisible,
                DisplayOrder = isVisible ? displayOrder : -1,
            };
        }

        private string BuildUpgradeDescription(SurvivorLevelUpChoice choice, int level)
        {
            if (choice != SurvivorLevelUpChoice.Heal
                && (level < 0 || level >= SurvivorConstants.MaxUpgradeLevel))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    level,
                    $"Upgrade description level must be between 0 and {SurvivorConstants.MaxUpgradeLevel - 1}.");
            }

            switch (choice)
            {
                case SurvivorLevelUpChoice.Shovel:
                    return $"伤害增加 {s_shovelDamageRates[level] * 100:0}%\n额外增加 {s_shovelCountBonuses[level]} 个旋转";
                case SurvivorLevelUpChoice.Rifle:
                    return $"伤害增加 {s_rifleDamageRates[level] * 100:0}%\n穿透力增加 {s_riflePierceBonuses[level]}";
                case SurvivorLevelUpChoice.FireRate:
                    return $"施法速度提高 {s_fireRateBonuses[level] * 100:0}%";
                case SurvivorLevelUpChoice.MoveSpeed:
                    return $"移动速度提升 {s_moveSpeedBonuses[level] * 100:0}%";
                case SurvivorLevelUpChoice.Heal:
                    return "全恢复生命值";
                default:
                    throw new ArgumentOutOfRangeException(nameof(choice), choice, null);
            }
        }

        private void RecalculateBuffs()
        {
            float fireRate = GetAppliedRate(SurvivorLevelUpChoice.FireRate, s_fireRateBonuses);
            float moveSpeed = GetAppliedRate(SurvivorLevelUpChoice.MoveSpeed, s_moveSpeedBonuses);
            FireIntervalMultiplier = Mathf.Max(
                SurvivorConstants.MinimumFireIntervalMultiplier,
                StartOptions.FireIntervalMultiplier * (1f - fireRate));
            MoveSpeedMultiplier = StartOptions.MoveSpeedMultiplier * (1f + moveSpeed);
            ExtraProjectilePierce = StartOptions.ExtraProjectilePierce
                + GetAppliedWeaponValue(SurvivorLevelUpChoice.Rifle, s_riflePierceBonuses);
        }

        private int CalculateShovelCount()
        {
            if (ShovelLevel == 0)
            {
                return 0;
            }

            int count = SurvivorConstants.ShovelBaseCount;
            for (int level = 1; level < ShovelLevel; level++)
            {
                count += s_shovelCountBonuses[level];
            }

            return count;
        }

        private float GetAppliedRate(SurvivorLevelUpChoice choice, float[] values)
        {
            int level = m_upgradeLevels[(int)choice];
            return level == 0 ? 0f : values[level - 1];
        }

        private float GetAppliedWeaponRate(SurvivorLevelUpChoice choice, float[] values)
        {
            int level = m_upgradeLevels[(int)choice];
            return level <= 1 ? 0f : values[level - 1];
        }

        private int GetAppliedWeaponValue(SurvivorLevelUpChoice choice, int[] values)
        {
            int level = m_upgradeLevels[(int)choice];
            return level <= 1 ? 0 : values[level - 1];
        }

        private void Shuffle(List<SurvivorLevelUpChoice> choices)
        {
            for (int i = choices.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (choices[i], choices[swapIndex]) = (choices[swapIndex], choices[i]);
            }
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
        Shovel,
        Rifle,
        FireRate,
        MoveSpeed,
        Heal,
    }

    public sealed class SurvivorLevelUpWindowData
    {
        public int Level { get; set; }

        public IReadOnlyList<SurvivorLevelUpOption> Options { get; set; }

        public Action<SurvivorLevelUpChoice> SelectCallback { get; set; }
    }

    /// <summary>
    /// 升级窗口单个条目的不可变展示数据。
    /// </summary>
    public sealed class SurvivorLevelUpOption
    {
        public SurvivorLevelUpChoice Choice { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LevelText { get; set; }

        public bool IsVisible { get; set; }

        public int DisplayOrder { get; set; }
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
