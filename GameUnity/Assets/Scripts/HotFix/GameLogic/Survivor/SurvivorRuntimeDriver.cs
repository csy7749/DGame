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
}
