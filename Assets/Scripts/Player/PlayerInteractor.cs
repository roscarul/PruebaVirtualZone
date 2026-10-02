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
        private Mirror aimedMirror;
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
            aimedMirror = null;
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

            aimedMirror = hit.collider.GetComponentInParent<Mirror>();
            if (aimedMirror != null)
            {
                prompt = "E — girar espejo";
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

            if (aimedMirror != null)
            {
                aimedMirror.Rotate();
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

            // Si hay pared (o suelo) por delante, se retira hasta quedar justo fuera de la
            // superficie: antes el objeto aparecía dentro de la pared y physics lo expulsaba.
            Vector3 camPos = viewCamera.transform.position;
            Vector3 dropPos = held.transform.position;
            Vector3 dir = (dropPos - camPos).normalized;
            float dist = (dropPos - camPos).magnitude;

            Collider[] heldColliders = held.GetComponentsInChildren<Collider>();
            foreach (Collider col in heldColliders)
                col.enabled = false;

            RaycastHit[] hits = Physics.RaycastAll(new Ray(camPos, dir), dist + 2f, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            Renderer heldRenderer = held.GetComponentInChildren<Renderer>();
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.root == transform.root || hit.transform.IsChildOf(held.transform))
                    continue;

                dropPos = PullOutOfSurface(hit.point, dir, dropPos, heldRenderer);
                break;
            }

            foreach (Collider col in heldColliders)
                col.enabled = true;

            // Que no acabe dentro de la propia cámara.
            if (Vector3.Dot(dropPos - camPos, dir) < 0.25f)
                dropPos = camPos + dir * 0.25f;

            held.transform.position = dropPos;

            if (body != null)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            held = null;
        }

        // Mueve el objeto por -dir hasta que sus bounds queden tocando la superficie golpeada,
        // sin penetrarla.
        private static Vector3 PullOutOfSurface(Vector3 hitPoint, Vector3 dir, Vector3 pos, Renderer rend)
        {
            if (rend == null)
                return hitPoint - dir * 0.3f;

            Bounds b = rend.bounds;
            Vector3 e = b.extents;
            float support = Mathf.Abs(dir.x) * e.x + Mathf.Abs(dir.y) * e.y + Mathf.Abs(dir.z) * e.z;
            float pull = Vector3.Dot(b.center - hitPoint, dir) + support;
            return pull > 0f ? pos - dir * pull : pos;
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
