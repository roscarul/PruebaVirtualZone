using UnityEngine;

namespace LighthouseEscape
{
    // Puerta de salida: trabada hasta que la placa se activa; con E gira sobre su bisagra.
    public class ExitDoor : MonoBehaviour, IUnlockable
    {
        [SerializeField] private bool hingeOnLeft = true;
        [SerializeField] private float openAngle = 105f;
        [SerializeField] private float openSpeed = 100f;
        // Luz propia de esta puerta; si no se asigna, se usa la LuzPuerta global (escenas antiguas).
        [SerializeField] private Light customLight;
        // true = para abrirla hace falta la llave; la primera puerta va sin llave (solo la placa).
        [SerializeField] private bool requiresKey = true;
        // true = se abre/cierra sola con la placa (como la bandeja de la caja); false = con E.
        [SerializeField] private bool autoOpen;
        // Sonido al desbloquear con E (las puertas auto ya se anuncian con su luz).
        [SerializeField] private AudioClip soundUnlock;

        private Light doorLight;
        private float appliedAngle;
        private float targetAngle;
        private float direction = -1f;
        private Vector3 hingePoint;
        private Vector3 closedPos;
        private Quaternion closedRot;

        public bool Unlocked { get; private set; }
        public bool IsOpening => appliedAngle > 0f;
        public bool RequiresKey => requiresKey;

        private void Start()
        {
            if (customLight != null)
            {
                doorLight = customLight;
            }
            else
            {
                GameObject lightGo = GameObject.Find("LuzPuerta");
                if (lightGo != null)
                    doorLight = lightGo.GetComponent<Light>();
            }

            // Naranja mientras la puerta está trabada.
            if (doorLight != null)
                doorLight.color = new Color(1f, 0.55f, 0.35f);

            // Bisagra: borde de la hoja a media anchura.
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            float halfWidth = 0.7f;
            if (meshFilter != null && meshFilter.sharedMesh != null)
                halfWidth = 0.5f * meshFilter.sharedMesh.bounds.size.x * transform.lossyScale.x;

            Vector3 side = transform.right * (hingeOnLeft ? -1f : 1f);
            hingePoint = transform.position + side * halfWidth;
            closedPos = transform.position;
            closedRot = transform.rotation;
        }

        public void Unlock()
        {
            if (Unlocked)
                return;

            Unlocked = true;

            // Verde: desbloqueada.
            if (doorLight != null)
                doorLight.color = new Color(0.45f, 1f, 0.5f);

            if (autoOpen)
            {
                StartMoving(openAngle);
            }
            else
            {
                GameFeedback.Play(soundUnlock);
            }
        }

        // Vuelve a trabar si quitan los barriles de la placa. Las puertas auto (placa) se cierran solas.
        public void Lock()
        {
            if (!Unlocked)
                return;

            if (!autoOpen && IsOpening)
                return;

            Unlocked = false;

            if (doorLight != null)
                doorLight.color = new Color(1f, 0.55f, 0.35f);

            if (autoOpen)
                targetAngle = 0f;
        }

        public void TryOpen()
        {
            if (autoOpen || !Unlocked || IsOpening)
                return;

            StartMoving(openAngle);
        }

        private void StartMoving(float angle)
        {
            direction = hingeOnLeft ? -1f : 1f;
            targetAngle = angle;
            if (appliedAngle <= 0f)
                appliedAngle = 0.001f;
        }

        private void Update()
        {
            if (Mathf.Approximately(appliedAngle, targetAngle))
            {
                // Cierre exacto en la posicion inicial (evita deriva del RotateAround).
                if (targetAngle <= 0f && appliedAngle != 0f)
                {
                    transform.SetPositionAndRotation(closedPos, closedRot);
                    appliedAngle = 0f;
                }
                return;
            }

            float step = openSpeed * Time.deltaTime;
            float next = appliedAngle < targetAngle
                ? Mathf.Min(appliedAngle + step, targetAngle)
                : Mathf.Max(appliedAngle - step, targetAngle);
            float delta = next - appliedAngle;
            if (delta != 0f)
                transform.RotateAround(hingePoint, Vector3.up, direction * delta);
            appliedAngle = next;

            if (appliedAngle <= 0f && targetAngle <= 0f)
            {
                transform.SetPositionAndRotation(closedPos, closedRot);
                appliedAngle = 0f;
            }
        }
    }
}
