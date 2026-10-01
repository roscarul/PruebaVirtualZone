using UnityEngine;

namespace LighthouseEscape
{
    // Placa de presión: se activa cuando el peso encima llega al umbral; se desactiva si lo quitas.
    // El jugador también cuenta: pisarla enseña que la puerta abre con peso (al irse, se apaga).
    // Se mide por solape directo (OverlapBox), no por eventos, para que valga con el CharacterController.
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] private float requiredWeight = 20f;
        [SerializeField] private float playerWeight = 80f;

        private readonly Collider[] overlaps = new Collider[32];
        private BoxCollider triggerBox;
        private Renderer plateRenderer;
        private Color originalColor;
        private bool activated;

        private void Start()
        {
            plateRenderer = GetComponent<Renderer>();
            if (plateRenderer != null)
                originalColor = plateRenderer.material.color;

            foreach (BoxCollider box in GetComponents<BoxCollider>())
            {
                if (box.isTrigger)
                {
                    triggerBox = box;
                    break;
                }
            }
        }

        private void Update()
        {
            if (triggerBox == null)
                return;

            Vector3 half = Vector3.Scale(triggerBox.size * 0.5f, transform.lossyScale);
            int count = Physics.OverlapBoxNonAlloc(transform.position, half, overlaps, transform.rotation);

            float total = 0f;
            for (int i = 0; i < count; i++)
            {
                Collider hit = overlaps[i];
                if (hit.transform.IsChildOf(transform))
                    continue;

                if (hit is CharacterController)
                {
                    total += playerWeight;
                    continue;
                }

                Rigidbody body = hit.attachedRigidbody;
                if (body == null)
                    continue;

                // Un mismo cuerpo con varios colisores solo cuenta una vez.
                bool alreadyCounted = false;
                for (int j = 0; j < i; j++)
                {
                    if (overlaps[j] != null && overlaps[j].attachedRigidbody == body)
                    {
                        alreadyCounted = true;
                        break;
                    }
                }

                if (alreadyCounted)
                    continue;

                total += body.mass;
            }

            if (!activated && total >= requiredWeight)
                Activate();
            else if (activated && total < requiredWeight)
                Deactivate();
        }

        private void Activate()
        {
            activated = true;

            if (plateRenderer != null)
                plateRenderer.material.color = new Color(0.45f, 1f, 0.5f);

            ExitDoor door = FindAnyObjectByType<ExitDoor>();
            if (door != null)
                door.Unlock();
        }

        private void Deactivate()
        {
            activated = false;

            if (plateRenderer != null)
                plateRenderer.material.color = originalColor;

            ExitDoor door = FindAnyObjectByType<ExitDoor>();
            if (door != null)
                door.Lock();
        }
    }
}
