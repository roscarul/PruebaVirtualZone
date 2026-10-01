using UnityEngine;
using UnityEngine.InputSystem;

namespace LighthouseEscape.Player
{
    // Interacción con E: coger/soltar, abrir la puerta, girar ruedas de candados e inspeccionar objetos.
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float reach = 2.6f;
        [SerializeField] private float holdDistance = 1.4f;
        public InputActionAsset actionsAsset;

        // true mientras dura una inspección; PlayerController lo usa para congelar al jugador.
        public static bool IsInspecting => inspecting;

        private static bool inspecting;

        private InputAction interactAction;
        private InputAction cancelAction;
        private Camera viewCamera;
        private Grabbable held;
        private Grabbable aimedGrabbable;
        private ExitDoor aimedDoor;
        private KeypadButton aimedButton;
        private CombinationLock aimedLock;
        private Inspectable aimedInspectable;
        private string prompt = "";
        private GUIStyle promptStyle;
        private GUIStyle captionStyle;

        private Inspectable inspected;
        private Transform inspectParent;
        private Vector3 inspectPosition;
        private Quaternion inspectRotation;
        private Rigidbody inspectBody;
        private bool inspectBodyWasKinematic;
        private bool showCaption;

        private void Awake()
        {
            viewCamera = GetComponentInChildren<Camera>();

            if (actionsAsset != null)
            {
                interactAction = actionsAsset.FindAction("Interact");
                cancelAction = actionsAsset.FindAction("Cancel");
                actionsAsset.Enable();
            }
        }

        private void OnDestroy()
        {
            if (actionsAsset != null)
                actionsAsset.Disable();
        }

        private void Update()
        {
            if (inspected != null)
            {
                UpdateInspect();
                return;
            }

            if (held != null)
            {
                held.transform.localPosition = new Vector3(0f, -0.25f, holdDistance);
                held.transform.rotation = Quaternion.identity;
                prompt = "E — soltar";
            }
            else
            {
                UpdateAim();
            }

            if (interactAction != null && interactAction.WasPressedThisFrame())
                Press();
        }

        private void UpdateAim()
        {
            aimedDoor = null;
            aimedButton = null;
            aimedLock = null;
            aimedGrabbable = null;
            aimedInspectable = null;
            prompt = "";

            if (viewCamera == null)
                return;

            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, reach))
                return;

            aimedDoor = hit.collider.GetComponentInParent<ExitDoor>();
            if (aimedDoor != null)
            {
                if (!aimedDoor.Unlocked)
                    prompt = "Puerta trabada";
                else if (!aimedDoor.IsOpening)
                    prompt = "E — abrir";
                return;
            }

            aimedButton = hit.collider.GetComponentInParent<KeypadButton>();
            if (aimedButton != null)
            {
                prompt = aimedButton.Prompt();
                return;
            }

            aimedLock = hit.collider.GetComponentInParent<CombinationLock>();
            if (aimedLock != null)
            {
                prompt = aimedLock.CodeText;
                return;
            }

            aimedGrabbable = hit.collider.GetComponentInParent<Grabbable>();
            if (aimedGrabbable != null)
            {
                prompt = "E — coger";
                return;
            }

            aimedInspectable = hit.collider.GetComponentInParent<Inspectable>();
            if (aimedInspectable != null)
                prompt = "E — inspeccionar";
        }

        private void Press()
        {
            if (held != null)
            {
                Drop();
                return;
            }

            if (aimedDoor != null)
            {
                aimedDoor.TryOpen();
                return;
            }

            if (aimedButton != null)
            {
                aimedButton.Press();
                prompt = aimedButton.Prompt();
                return;
            }

            if (aimedGrabbable != null)
            {
                Grab(aimedGrabbable);
                return;
            }

            if (aimedInspectable != null)
                EnterInspect(aimedInspectable);
        }

        private void Grab(Grabbable target)
        {
            held = target;

            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body != null)
                body.isKinematic = true;

            target.transform.SetParent(viewCamera.transform, false);
            target.transform.localPosition = new Vector3(0f, -0.25f, holdDistance);
            target.transform.rotation = Quaternion.identity;
        }

        private void Drop()
        {
            Rigidbody body = held.GetComponent<Rigidbody>();
            held.transform.SetParent(null, true);
            held.transform.rotation = Quaternion.identity;

            if (body != null)
                body.isKinematic = false;

            held = null;
        }

        // ---- Modo inspección: el objeto se centra frente a la cámara y se rota con el ratón. ----

        private void EnterInspect(Inspectable target)
        {
            inspected = target;
            inspecting = true;
            showCaption = false;

            Transform t = target.transform;
            inspectParent = t.parent;
            inspectPosition = t.position;
            inspectRotation = t.rotation;

            inspectBody = target.GetComponent<Rigidbody>();
            if (inspectBody != null)
            {
                inspectBodyWasKinematic = inspectBody.isKinematic;
                inspectBody.isKinematic = true;
            }

            t.SetParent(viewCamera.transform, true);
            t.localPosition = Vector3.forward * target.FocusDistance;

            // Su cara +Z hacia la cámara: al entrar se ve el frente, no la parte de detrás.
            t.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // Centra el objeto por su renderer, no por su pivote.
            Renderer rend = target.CachedRenderer;
            if (rend != null)
                t.position += FocusPoint() - rend.bounds.center;
        }

        private void UpdateInspect()
        {
            Transform t = inspected.transform;
            Vector3 center = FocusPoint();

            Vector2 look = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            t.RotateAround(center, viewCamera.transform.up, -look.x * inspected.RotationSpeed);
            t.RotateAround(center, viewCamera.transform.right, -look.y * inspected.RotationSpeed);

            // Si la cámara se movió (gravedad), vuelve a centrarlo en pantalla.
            Renderer rend = inspected.CachedRenderer;
            if (rend != null)
                t.position += center - rend.bounds.center;

            // La pista de texto solo aparece con la cara trasera del objeto hacia la cámara.
            showCaption = Vector3.Dot(viewCamera.transform.forward, t.forward) > 0f;

            prompt = "E / Esc — salir";

            bool exit = (interactAction != null && interactAction.WasPressedThisFrame())
                        || (cancelAction != null && cancelAction.WasPressedThisFrame());
            if (exit)
                ExitInspect();
        }

        private Vector3 FocusPoint()
        {
            return viewCamera.transform.TransformPoint(Vector3.forward * inspected.FocusDistance);
        }

        private void ExitInspect()
        {
            Transform t = inspected.transform;

            t.SetParent(inspectParent, true);
            t.SetPositionAndRotation(inspectPosition, inspectRotation);

            if (inspectBody != null)
            {
                inspectBody.isKinematic = inspectBodyWasKinematic;
                if (!inspectBodyWasKinematic)
                {
                    inspectBody.linearVelocity = Vector3.zero;
                    inspectBody.angularVelocity = Vector3.zero;
                }
            }

            inspectBody = null;
            inspected = null;
            inspecting = false;
            showCaption = false;
            prompt = "";
        }

        private void OnGUI()
        {
            GUI.color = Color.white;

            if (inspected == null)
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f, Screen.height * 0.5f - 2f, 4f, 4f), Texture2D.whiteTexture);

            if (showCaption && inspected != null && !string.IsNullOrEmpty(inspected.Caption))
            {
                if (captionStyle == null)
                    captionStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };

                GUI.Label(new Rect(0f, Screen.height * 0.55f, Screen.width, 28f), inspected.Caption, captionStyle);
            }

            if (string.IsNullOrEmpty(prompt))
                return;

            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };

            GUI.Label(new Rect(0f, Screen.height * 0.6f, Screen.width, 32f), prompt, promptStyle);
        }
    }
}
