using System;
using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorEnemyController : MonoBehaviour
    {
        private static readonly int DeadAnimatorParameter = Animator.StringToHash("Dead");
        private static readonly int HitAnimatorParameter = Animator.StringToHash("Hit");

        private Rigidbody2D m_rigidbody;
        private Collider2D m_collider;
        private SpriteRenderer m_spriteRenderer;
        private Animator m_animator;
        private Transform m_target;
        private Func<bool> m_canAct;
        private Action<SurvivorEnemyController> m_defeatedCallback;
        private Action<float> m_playerDamageCallback;
        private float m_health;
        private float m_moveSpeed;
        private float m_contactDamagePerSecond;
        private float m_contactDamageDistance;
        private float m_hitTimer;
        private int m_recycleToken;
        private SurvivorEnemyState m_state;

        public bool IsAlive => m_state != SurvivorEnemyState.Dead;

        public int RecycleToken => m_recycleToken;

        public SurvivorEnemyState State => m_state;

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_collider = GetComponent<Collider2D>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_animator = GetComponent<Animator>();
        }

        public void ResetState(SurvivorEnemyOptions options)
        {
            EnsureRequiredComponents();
            m_target = options.Target;
            m_canAct = options.CanAct;
            m_health = options.MaxHealth;
            m_moveSpeed = options.MoveSpeed;
            m_contactDamagePerSecond = options.ContactDamagePerSecond;
            m_contactDamageDistance = options.ContactDamageDistance;
            m_defeatedCallback = options.DefeatedCallback;
            m_playerDamageCallback = options.PlayerDamageCallback;
            m_hitTimer = 0f;
            m_recycleToken++;
            SetState(SurvivorEnemyState.Run);
            m_collider.enabled = true;
            m_rigidbody.simulated = true;
            m_rigidbody.velocity = Vector2.zero;
        }

        public void ApplyDamage(float damage)
        {
            if (m_state == SurvivorEnemyState.Dead)
            {
                return;
            }

            m_health -= damage;
            if (m_health <= 0f)
            {
                Die();
                return;
            }

            SetState(SurvivorEnemyState.Hit);
        }

        private void FixedUpdate()
        {
            if (m_state == SurvivorEnemyState.Dead || m_target == null)
            {
                return;
            }

            if (m_canAct != null && !m_canAct.Invoke())
            {
                m_rigidbody.velocity = Vector2.zero;
                return;
            }

            if (TickHitState())
            {
                return;
            }

            MoveToTarget();
            ApplyContactDamage();
        }

        private void MoveToTarget()
        {
            Vector2 direction = ((Vector2)m_target.position - m_rigidbody.position).normalized;
            Vector2 nextPosition = m_rigidbody.position + direction * (m_moveSpeed * Time.fixedDeltaTime);
            m_rigidbody.MovePosition(nextPosition);
            m_rigidbody.velocity = Vector2.zero;
        }

        private void ApplyContactDamage()
        {
            float sqrDistance = (m_target.position - transform.position).sqrMagnitude;
            float contactSqrDistance = m_contactDamageDistance * m_contactDamageDistance;
            if (sqrDistance > contactSqrDistance || m_playerDamageCallback == null)
            {
                return;
            }

            m_playerDamageCallback.Invoke(m_contactDamagePerSecond * Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (m_state == SurvivorEnemyState.Dead || m_target == null || m_spriteRenderer == null)
            {
                return;
            }

            m_spriteRenderer.flipX = m_target.position.x < transform.position.x;
        }

        private void Die()
        {
            m_recycleToken++;
            SetState(SurvivorEnemyState.Dead);
            m_collider.enabled = false;
            m_rigidbody.simulated = false;
            m_rigidbody.velocity = Vector2.zero;
            if (m_defeatedCallback != null)
            {
                m_defeatedCallback.Invoke(this);
            }
        }

        public bool CanRecycle(int recycleToken)
        {
            return m_state == SurvivorEnemyState.Dead && m_recycleToken == recycleToken;
        }

        public void MarkRecycled()
        {
            m_recycleToken++;
            m_target = null;
            m_canAct = null;
            m_defeatedCallback = null;
            m_playerDamageCallback = null;
            m_hitTimer = 0f;
        }

        private bool TickHitState()
        {
            if (m_state != SurvivorEnemyState.Hit)
            {
                return false;
            }

            m_hitTimer -= Time.fixedDeltaTime;
            if (m_hitTimer > 0f)
            {
                m_rigidbody.velocity = Vector2.zero;
                return true;
            }

            SetState(SurvivorEnemyState.Run);
            return false;
        }

        private void SetState(SurvivorEnemyState state)
        {
            m_state = state;
            if (state == SurvivorEnemyState.Hit)
            {
                m_hitTimer = SurvivorConstants.EnemyHitDurationSeconds;
            }

            ApplyAnimatorState(state);
        }

        private void ApplyAnimatorState(SurvivorEnemyState state)
        {
            if (m_animator == null)
            {
                return;
            }

            if (state == SurvivorEnemyState.Run)
            {
                m_animator.ResetTrigger(HitAnimatorParameter);
                m_animator.SetBool(DeadAnimatorParameter, false);
                return;
            }

            if (state == SurvivorEnemyState.Hit)
            {
                m_animator.SetTrigger(HitAnimatorParameter);
                return;
            }

            m_animator.SetBool(DeadAnimatorParameter, true);
        }

        private void EnsureRequiredComponents()
        {
            if (m_rigidbody == null)
            {
                throw new InvalidOperationException("Survivor enemy prefab is missing Rigidbody2D.");
            }

            if (m_collider == null)
            {
                throw new InvalidOperationException("Survivor enemy prefab is missing Collider2D.");
            }
        }
    }

    public enum SurvivorEnemyState
    {
        Dead,
        Hit,
        Run,
    }

    public sealed class SurvivorEnemyOptions
    {
        public Transform Target { get; set; }

        public Func<bool> CanAct { get; set; }

        public float MaxHealth { get; set; }

        public float MoveSpeed { get; set; }

        public float ContactDamagePerSecond { get; set; }

        public float ContactDamageDistance { get; set; }

        public Action<SurvivorEnemyController> DefeatedCallback { get; set; }

        public Action<float> PlayerDamageCallback { get; set; }
    }
}
