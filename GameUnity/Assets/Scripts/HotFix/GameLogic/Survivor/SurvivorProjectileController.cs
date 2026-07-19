using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorProjectileController : MonoBehaviour
    {
        private readonly HashSet<SurvivorEnemyController> m_hitEnemies = new HashSet<SurvivorEnemyController>();
        private Rigidbody2D m_rigidbody;
        private Func<bool> m_canAct;
        private Action<SurvivorProjectileController> m_recycleCallback;
        private Vector2 m_direction;
        private float m_damage;
        private float m_speed;
        private float m_lifetime;
        private float m_elapsed;
        private int m_pierce;
        private bool m_isLive;

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }

        public void ResetState(SurvivorProjectileOptions options)
        {
            m_damage = options.Damage;
            m_pierce = options.Pierce;
            m_direction = options.Direction.normalized;
            m_speed = options.Speed;
            m_lifetime = options.Lifetime;
            m_canAct = options.CanAct;
            m_recycleCallback = options.RecycleCallback;
            m_elapsed = 0f;
            m_isLive = true;
            m_hitEnemies.Clear();
            ApplyVelocity();
        }

        private void Update()
        {
            if (!m_isLive || IsPaused())
            {
                return;
            }

            m_elapsed += Time.deltaTime;
            if (m_elapsed >= m_lifetime)
            {
                RecycleSelf();
                return;
            }

            MoveWithoutRigidbody();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!m_isLive || IsPaused())
            {
                return;
            }

            SurvivorEnemyController enemy = collision.GetComponentInParent<SurvivorEnemyController>();
            if (enemy == null || m_hitEnemies.Contains(enemy))
            {
                return;
            }

            m_hitEnemies.Add(enemy);
            enemy.ApplyDamage(m_damage);
            m_pierce--;
            if (m_pierce < 0)
            {
                RecycleSelf();
            }
        }

        private void ApplyVelocity()
        {
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = m_direction * m_speed;
            }
        }

        private void MoveWithoutRigidbody()
        {
            if (m_rigidbody == null)
            {
                transform.position += (Vector3)(m_direction * (m_speed * Time.deltaTime));
            }
        }

        private void RecycleSelf()
        {
            m_isLive = false;
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = Vector2.zero;
            }

            if (m_recycleCallback != null)
            {
                m_recycleCallback.Invoke(this);
            }
        }

        private bool IsPaused() => m_canAct != null && !m_canAct.Invoke();
    }

    public sealed class SurvivorProjectileOptions
    {
        public float Damage { get; set; }

        public int Pierce { get; set; }

        public Vector2 Direction { get; set; }

        public float Speed { get; set; }

        public float Lifetime { get; set; }

        public Func<bool> CanAct { get; set; }

        public Action<SurvivorProjectileController> RecycleCallback { get; set; }
    }

    /// <summary>
    /// 处理环绕铲子与敌人的接触伤害；敌人离开后允许下一圈再次命中。
    /// </summary>
    public sealed class SurvivorOrbitProjectileController : MonoBehaviour
    {
        private readonly HashSet<SurvivorEnemyController> m_hitEnemies = new();
        private Func<bool> m_canAct;
        private float m_damage;
        private bool m_isLive;

        public void ResetState(SurvivorOrbitProjectileOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            m_damage = options.Damage;
            m_canAct = options.CanAct;
            m_isLive = true;
            m_hitEnemies.Clear();
        }

        public void SetDamage(float damage)
        {
            m_damage = damage;
        }

        public void MarkRecycled()
        {
            m_isLive = false;
            m_canAct = null;
            m_hitEnemies.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!m_isLive || IsPaused())
            {
                return;
            }

            SurvivorEnemyController enemy = collision.GetComponentInParent<SurvivorEnemyController>();
            if (enemy != null && m_hitEnemies.Add(enemy))
            {
                enemy.ApplyDamage(m_damage);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SurvivorEnemyController enemy = collision.GetComponentInParent<SurvivorEnemyController>();
            if (enemy != null)
            {
                m_hitEnemies.Remove(enemy);
            }
        }

        private bool IsPaused() => m_canAct != null && !m_canAct.Invoke();
    }

    /// <summary>
    /// 创建或复用环绕铲子时所需的运行时参数。
    /// </summary>
    public sealed class SurvivorOrbitProjectileOptions
    {
        public float Damage { get; set; }

        public Func<bool> CanAct { get; set; }
    }
}
