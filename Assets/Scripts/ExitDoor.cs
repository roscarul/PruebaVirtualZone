using UnityEngine;

namespace LighthouseEscape
{
    // Puerta de salida: trabada hasta que la placa se activa; con E gira sobre su bisagra.
    public class ExitDoor : MonoBehaviour, IUnlockable
    {
        [SerializeField] private bool hingeOnLeft = true;
        [SerializeField] private float openAngle = 105f;
        [SerializeField] private float openSpeed = 100f;

        private Light doorLight;
        private float appliedAngle;
        private float direction = -1f;
        private Vector3 hingePoint;

        public bool Unlocked { get; private set; }
        public bool IsOpening => appliedAngle > 0f;

        private void Start()
        {
            GameObject lightGo = GameObject.Find("LuzPuerta");
            if (lightGo != null)
                doorLight = lightGo.GetComponent<Light>();

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
        }

        public void Unlock()
        {
            if (Unlocked)
                return;

            Unlocked = true;

            // Verde: desbloqueada.
            if (doorLight != null)
                doorLight.color = new Color(0.45f, 1f, 0.5f);
        }

        // Vuelve a trabar si quitan los barriles de la placa (si la puerta ya se está abriendo, se queda abierta).
        public void Lock()
        {
            if (!Unlocked || IsOpening)
                return;

            Unlocked = false;

            if (doorLight != null)
                doorLight.color = new Color(1f, 0.55f, 0.35f);
        }

        public void TryOpen()
        {
            if (!Unlocked || IsOpening)
                return;

            direction = hingeOnLeft ? -1f : 1f;
            appliedAngle = 0.001f;
        }

        private void Update()
        {
            if (appliedAngle <= 0f || appliedAngle >= openAngle)
                return;

            float step = Mathf.Min(openSpeed * Time.deltaTime, openAngle - appliedAngle);
            transform.RotateAround(hingePoint, Vector3.up, direction * step);
            appliedAngle += step;
        }
    }
}
