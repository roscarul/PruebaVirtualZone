using UnityEngine;

namespace LighthouseEscape
{
    // Tapa de la caja fuerte: al acertar el código gira sobre su bisagra y descubre la llave.
    public class SafeDoor : MonoBehaviour, IUnlockable
    {
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float openSpeed = 140f;

        private float appliedAngle;
        private Vector3 hingePoint;

        public bool Unlocked { get; private set; }
        public bool IsOpening => appliedAngle > 0f;

        private void Start()
        {
            // Bisagra en el borde en z del panel; al abrir gira hacia fuera de la caja.
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            float halfDepth = 0.105f;
            if (meshFilter != null && meshFilter.sharedMesh != null)
                halfDepth = 0.5f * meshFilter.sharedMesh.bounds.size.z * transform.lossyScale.z;

            hingePoint = transform.position + transform.forward * halfDepth;
        }

        public void Unlock()
        {
            if (Unlocked)
                return;

            Unlocked = true;
            TryOpen();
        }

        public void Lock()
        {
            if (!Unlocked || IsOpening)
                return;

            Unlocked = false;
        }

        public void TryOpen()
        {
            if (!Unlocked || IsOpening)
                return;

            appliedAngle = 0.001f;
        }

        private void Update()
        {
            if (appliedAngle <= 0f || appliedAngle >= openAngle)
                return;

            float step = Mathf.Min(openSpeed * Time.deltaTime, openAngle - appliedAngle);
            transform.RotateAround(hingePoint, Vector3.up, -step);
            appliedAngle += step;
        }
    }
}
