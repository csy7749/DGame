using System;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorHudData
    {
        public SurvivorHudData(SurvivorHudDataOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ExperienceToNextLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.ExperienceToNextLevel),
                    "Experience max must be greater than zero.");
            }

            if (options.MaxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.MaxHealth),
                    "Health max must be greater than zero.");
            }

            Level = options.Level;
            KillCount = options.KillCount;
            ElapsedTime = options.ElapsedTime;
            BattleDuration = options.BattleDuration;
            Experience = options.Experience;
            ExperienceToNextLevel = options.ExperienceToNextLevel;
            Health = options.Health;
            MaxHealth = options.MaxHealth;
        }

        public int Level { get; }

        public int KillCount { get; }

        public float ElapsedTime { get; }

        public float BattleDuration { get; }

        public int Experience { get; }

        public int ExperienceToNextLevel { get; }

        public float Health { get; }

        public float MaxHealth { get; }

        public float RemainingTime => Mathf.Max(0f, BattleDuration - ElapsedTime);

        public float ExperienceProgress => Mathf.Clamp01(Experience / (float)ExperienceToNextLevel);

        public float HealthProgress => Mathf.Clamp01(Health / MaxHealth);
    }

    public sealed class SurvivorHudDataOptions
    {
        public int Level { get; set; }

        public int KillCount { get; set; }

        public float ElapsedTime { get; set; }

        public float BattleDuration { get; set; }

        public int Experience { get; set; }

        public int ExperienceToNextLevel { get; set; }

        public float Health { get; set; }

        public float MaxHealth { get; set; }
    }
}
