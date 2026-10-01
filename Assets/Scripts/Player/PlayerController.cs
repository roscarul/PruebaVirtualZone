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

        [Header("Agacharse")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchEyeHeight = 0.95f;
        [SerializeField] private float crouchSpeedFactor = 0.55f;
        [SerializeField] private float crouchLerpSpeed = 10f;
        [SerializeField] private float standCheckRadius = 0.28f;

        [Header("Input")]
        public InputActionAsset actionsAsset;

        private CharacterController characterController;
        private Transform viewPoint;   // cámara hija (solo hace pitch)
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction crouchAction;
        private float verticalVelocity;
        private float pitch;

        private float standHeight;      // medidas de pie, leídas del CharacterController de la escena
        private float standCenterY;
        private float standEyeHeight;
        private bool crouched;
        private float crouchT;          // 0 = de pie, 1 = agachado

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            Camera view = GetComponentInChildren<Camera>();
            if (view != null)
                viewPoint = view.transform;

            standHeight = characterController.height;
            standCenterY = characterController.center.y;
            if (viewPoint != null)
                standEyeHeight = viewPoint.localPosition.y;

            if (actionsAsset != null)
            {
                moveAction = actionsAsset.FindAction("Move");
                lookAction = actionsAsset.FindAction("Look");
                crouchAction = actionsAsset.FindAction("Crouch");
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
            // Durante una inspección el jugador queda quieto (solo le aplica la gravedad).
            if (PlayerInteractor.IsInspecting)
            {
                ApplyGravity();
                return;
            }

            if (crouchAction != null && crouchAction.WasPressedThisFrame())
                ToggleCrouch();

            ApplyCrouch();

            Look(lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero);
            Move();
        }

        private void ToggleCrouch()
        {
            if (!crouched)
            {
                crouched = true;
                return;
            }

            if (HasHeadroom())
                crouched = false;
        }

        private bool HasHeadroom()
        {
            // Esfera donde iría la cabeza de pie; el colisor agachado no llega, no se choca a sí mismo.
            Vector3 head = transform.position + Vector3.up * (standHeight - standCheckRadius);
            return !Physics.CheckSphere(head, standCheckRadius, ~0, QueryTriggerInteraction.Ignore);
        }

        private void ApplyCrouch()
        {
            crouchT = Mathf.MoveTowards(crouchT, crouched ? 1f : 0f, crouchLerpSpeed * Time.deltaTime);

            float height = Mathf.Lerp(standHeight, crouchHeight, crouchT);
            float bottom = standCenterY - standHeight * 0.5f;

            characterController.height = height;
            characterController.center = new Vector3(0f, bottom + height * 0.5f, 0f);

            if (viewPoint != null)
            {
                Vector3 eye = viewPoint.localPosition;
                eye.y = Mathf.Lerp(standEyeHeight, crouchEyeHeight, crouchT);
                viewPoint.localPosition = eye;
            }
        }

        private void Move()
        {
            Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

            // Dirección relativa al cuerpo en el plano XZ (más lento agachado).
            float speed = walkSpeed * Mathf.Lerp(1f, crouchSpeedFactor, crouchT);
            Vector3 motion = (transform.right * input.x + transform.forward * input.y) * speed;
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
