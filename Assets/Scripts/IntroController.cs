using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using LighthouseEscape.Player;

namespace LighthouseEscape
{
    // Intro del nivel: durante la Timeline el jugador queda inmovil y puede
    // saltarse con cualquier tecla o clic; al terminar restaura cámara, cursor
    // y control. Se coloca en el GameObject del jugador (junto a PlayerController).
    public class IntroController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private GameObject introUI;

        private PlayerController playerController;
        private PlayerInteractor playerInteractor;
        private Transform viewPoint;
        private Vector3 rootPosition;
        private Quaternion rootRotation;
        private Vector3 eyePosition;
        private Quaternion eyeRotation;
        private float skipReadyAt;
        private bool finished;

        private void Awake()
        {
            playerController = GetComponentInChildren<PlayerController>();
            playerInteractor = GetComponentInChildren<PlayerInteractor>();

            Camera view = GetComponentInChildren<Camera>();
            if (view != null)
                viewPoint = view.transform;

            SavePose();
        }

        private void Start()
        {
            SetPlayerActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
            skipReadyAt = Time.unscaledTime + 0.4f;

            if (director != null)
            {
                director.stopped += OnStopped;
                director.Play();
            }
            else
            {
                Finish();
            }
        }

        private void Update()
        {
            if (finished || Time.unscaledTime < skipReadyAt)
                return;

            if (SkipPressed())
                Finish();
        }

        private void OnDestroy()
        {
            if (director != null)
                director.stopped -= OnStopped;
        }

        private void OnStopped(PlayableDirector _)
        {
            Finish();
        }

        private void Finish()
        {
            if (finished)
                return;
            finished = true;

            if (director != null)
            {
                director.stopped -= OnStopped;
                director.Stop();
            }

            if (introUI != null)
                introUI.SetActive(false);

            transform.SetPositionAndRotation(rootPosition, rootRotation);
            if (viewPoint != null)
                viewPoint.SetLocalPositionAndRotation(eyePosition, eyeRotation);

            RestoreMainCamera();

            SetPlayerActive(true);
            Cursor.visible = false;
        }

        private void RestoreMainCamera()
        {
            if (viewPoint == null)
                return;

            viewPoint.gameObject.SetActive(true);
            foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if (cam.transform != viewPoint)
                    cam.gameObject.SetActive(false);
            }
        }

        private void SavePose()
        {
            rootPosition = transform.position;
            rootRotation = transform.rotation;

            if (viewPoint != null)
            {
                eyePosition = viewPoint.localPosition;
                eyeRotation = viewPoint.localRotation;
            }
        }

        private void SetPlayerActive(bool active)
        {
            if (playerController != null)
                playerController.enabled = active;
            if (playerInteractor != null)
                playerInteractor.enabled = active;
        }

        private static bool SkipPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
                return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return true;

            return false;
        }
    }
}
