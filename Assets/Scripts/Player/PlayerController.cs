using UnityEngine;
using UnityEngine.InputSystem;

namespace LighthouseEscape.Player
{
    // Movimiento WASD y giro de cámara en primera persona (Input System).
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Input")]
        public InputActionAsset actionsAsset;

        private CharacterController characterController;
        private Transform viewPoint;   // cámara hija (solo hace pitch)
        private InputAction moveAction;
        private InputAction lookAction;
        private float verticalVelocity;
        private float pitch;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            Camera view = GetComponentInChildren<Camera>();
            if (view != null)
                viewPoint = view.transform;

            if (actionsAsset != null)
            {
                moveAction = actionsAsset.FindAction("Move");
                lookAction = actionsAsset.FindAction("Look");
                actionsAsset.Enable();
            }

            // Sin menús: cursor bloqueado para mirar con el ratón.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            if (actionsAsset != null)
                actionsAsset.Disable();
        }

        private void Update()
        {
            Look(lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero);
            Move();
        }

        private void Move()
        {
            Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

            // Dirección relativa al cuerpo en el plano XZ.
            Vector3 motion = (transform.right * input.x + transform.forward * input.y) * walkSpeed;
            characterController.Move(motion * Time.deltaTime);

            ApplyGravity();
        }

        private void ApplyGravity()
        {
            // Si está en el suelo, mantenemos una gravedad mínima para detectar el suelo.
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            verticalVelocity += gravity * Time.deltaTime;
            characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void Look(Vector2 delta)
        {
            if (delta.sqrMagnitude <= 0f || viewPoint == null)
                return;

            // Yaw en el cuerpo entero; pitch solo en la cámara, con tope.
            transform.Rotate(Vector3.up, delta.x * lookSensitivity);
            pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity, -maxPitch, maxPitch);
            viewPoint.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
