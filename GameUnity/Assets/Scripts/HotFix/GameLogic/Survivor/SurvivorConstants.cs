namespace GameLogic
{
    internal static class SurvivorConstants
    {
        public const string SceneLocation = "Demo2";
        public const string BattleRootName = "SurvivorBattleRoot";
        public const string PlayerName = "Player";
        public const string MainCameraTag = "MainCamera";
        public const string AreaTag = "Area";
        public const string GroundRootName = "Ground";
        public const string SpawnerPath = "Player/Spawner";
        public const string RuntimeRootName = "SurvivorRuntime";
        public const string EnemyParentName = "Enemies";
        public const string ProjectileParentName = "Projectiles";
        public const string ShovelOrbitRootName = "ShovelOrbit";
        public const string EnemyPrefabLocation = "SurvivorEnemy";
        public const string ShovelPrefabLocation = "SurvivorBullet0";
        public const string ProjectilePrefabLocation = "SurvivorBullet1";
        public const string MusicLocation = "SurvivorBGM";
        public const int EnemyPoolInitCapacity = 12;
        public const int EnemyPoolMaxCapacity = 80;
        public const int MaxActiveEnemies = 80;
        public const int ProjectilePoolInitCapacity = 8;
        public const int ProjectilePoolMaxCapacity = 40;
        public const int ShovelPoolCapacity = 8;
        public const int GroundTileCount = 4;
        public const float PoolAutoDestroyTime = 30f;
        public const float GroundShiftDistance = 40f;
        public const float BattleDurationSeconds = 300f;
        public const float HudRefreshIntervalSeconds = 0.5f;
        public const float PlayerMaxHealth = 100f;
        public const float EnemyContactDamagePerSecond = 8f;
        public const float EnemyContactDamageDistance = 0.7f;
        public const float EnemyHitDurationSeconds = 0.15f;
        public const float EnemyDeadRecycleDelaySeconds = 1.2f;
        public const int EnemyExperience = 1;
        public const int BaseExperienceToNextLevel = 5;
        public const int ExperienceGrowthPerLevel = 3;
        public const int MaxUpgradeLevel = 5;
        public const int VisibleUpgradeCount = 3;
        public const float MinimumFireIntervalMultiplier = 0.25f;
        public const float FirstWaveDurationSeconds = 150f;
        public const float FirstWaveSpawnIntervalSeconds = 0.3f;
        public const float SecondWaveSpawnIntervalSeconds = 0.1f;
        public const float FirstWaveEnemyHealth = 15f;
        public const float SecondWaveEnemyHealth = 35f;
        public const float FirstWaveEnemySpeed = 1.5f;
        public const float SecondWaveEnemySpeed = 2f;
        public const float ShovelBaseDamage = 4.5f;
        public const int ShovelBaseCount = 3;
        public const float ShovelOrbitRadius = 1.5f;
        public const float ShovelBaseAngularSpeed = 150f;
        public const float ProjectileBaseDamage = 3f;
        public const int ProjectilePierce = 0;
        public const float ProjectileSpeed = 15f;
        public const float ProjectileLifetimeSeconds = 2f;
        public const float WeaponBaseFireIntervalSeconds = 0.5f;
        public const float BeanFarmerMoveSpeedMultiplier = 1.1f;
        public const float PotatoFarmerProjectileSpeedMultiplier = 1.1f;
        public const float PotatoFarmerFireIntervalMultiplier = 0.9f;
        public const float RiceFarmerDamageMultiplier = 1.2f;
        public const int BarleyFarmerExtraProjectilePierce = 1;
    }
}
