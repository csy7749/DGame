using UnityEngine;

namespace GameLogic
{
    public sealed class SurvivorRuntimeDriver : MonoBehaviour
    {
        private SurvivorSession m_session;

        public void Initialize(SurvivorSession session)
        {
            m_session = session;
        }

        private void Update()
        {
            if (m_session != null)
            {
                m_session.Tick(Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// 当地面块离开玩家周围的 Area 时，将其移动到前进方向以形成循环地图。
    /// </summary>
    public sealed class SurvivorGroundLoopController : MonoBehaviour
    {
        private Transform m_player;
        private Vector3 m_initialLocalPosition;
        private float m_shiftDistance;
        private bool m_isInitialized;

        /// <summary>
        /// 绑定玩家并恢复地面块的初始位置，保证重开回合时地图状态一致。
        /// </summary>
        public void Initialize(Transform player, float shiftDistance)
        {
            m_player = player != null
                ? player
                : throw new System.ArgumentNullException(nameof(player));
            if (shiftDistance <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(shiftDistance), shiftDistance, null);
            }

            if (!m_isInitialized)
            {
                m_initialLocalPosition = transform.localPosition;
                m_isInitialized = true;
            }

            m_shiftDistance = shiftDistance;
            transform.localPosition = m_initialLocalPosition;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag(SurvivorConstants.AreaTag))
            {
                return;
            }

            Vector3 difference = m_player.position - transform.position;
            Vector3 direction = Mathf.Abs(difference.x) > Mathf.Abs(difference.y)
                ? Vector3.right * Mathf.Sign(difference.x)
                : Vector3.up * Mathf.Sign(difference.y);
            transform.position += direction * m_shiftDistance;
        }
    }
}
