using System;
using UnityEngine;

namespace GameLogic.Survivor
{
    public sealed class SurvivorCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform m_target = null;
        [SerializeField] private Vector3 m_offset = new(0, 0, -10);
        [SerializeField] private float m_damping = 12f;

        private void LateUpdate()
        {
            if (m_target == null)
            {
                throw new InvalidOperationException("SurvivorCameraFollow target is not assigned.");
            }

            Vector3 targetPosition = m_target.position + m_offset;
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                m_damping * Time.deltaTime);
        }
    }
}
