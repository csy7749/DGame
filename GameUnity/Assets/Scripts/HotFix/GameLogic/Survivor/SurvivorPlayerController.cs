using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameLogic
{
    public sealed class SurvivorPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;

        private Rigidbody2D m_rigidbody;
        private Vector2 m_moveInput;

        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }

        public void ResetState()
        {
            m_moveInput = Vector2.zero;
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = Vector2.zero;
                m_rigidbody.position = Vector2.zero;
            }

            transform.position = Vector3.zero;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 input = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input.x -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input.x += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input.y -= 1f;
                }

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input.y += 1f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.leftStick.ReadValue().sqrMagnitude > input.sqrMagnitude)
            {
                input = gamepad.leftStick.ReadValue();
            }

            m_moveInput = input.sqrMagnitude > 1f ? input.normalized : input;
#else
            m_moveInput = Vector2.zero;
#endif
        }

        private void FixedUpdate()
        {
            if (m_rigidbody == null)
            {
                return;
            }

            m_rigidbody.velocity = m_moveInput * moveSpeed;
        }
    }
}
