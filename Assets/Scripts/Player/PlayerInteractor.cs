using UnityEngine;
using UnityEngine.InputSystem;

namespace LighthouseEscape.Player
{
    // Interacción con E: coger/soltar objetos y abrir la puerta (raycast desde la cámara).
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float reach = 2.6f;
        [SerializeField] private float holdDistance = 1.4f;
        public InputActionAsset actionsAsset;

        private InputAction interactAction;
        private Camera viewCamera;
        private Grabbable held;
        private Grabbable aimedGrabbable;
        private ExitDoor aimedDoor;
        private string prompt = "";
        private GUIStyle promptStyle;

        private void Awake()
        {
            viewCamera = GetComponentInChildren<Camera>();

            if (actionsAsset != null)
            {
                interactAction = actionsAsset.FindAction("Interact");
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
            aimedGrabbable = null;
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

            aimedGrabbable = hit.collider.GetComponentInParent<Grabbable>();
            if (aimedGrabbable != null)
                prompt = "E — coger";
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

            if (aimedGrabbable != null)
                Grab(aimedGrabbable);
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

        private void OnGUI()
        {
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f, Screen.height * 0.5f - 2f, 4f, 4f), Texture2D.whiteTexture);

            if (string.IsNullOrEmpty(prompt))
                return;

            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };

            GUI.Label(new Rect(0f, Screen.height * 0.6f, Screen.width, 32f), prompt, promptStyle);
        }
    }
}
