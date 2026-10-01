using System.Collections.Generic;
using UnityEngine;

namespace LighthouseEscape
{
    // Placa de presión: se activa cuando el peso encima llega al umbral; se desactiva si lo quitas.
    // El jugador también cuenta: pisarla enseña que la puerta abre con peso (al irse, se apaga).
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] private float requiredWeight = 20f;
        [SerializeField] private float playerWeight = 80f;

        private readonly HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();
        private Renderer plateRenderer;
        private Color originalColor;
        private bool playerInside;
        private bool activated;

        private void Start()
        {
            plateRenderer = GetComponent<Renderer>();
            if (plateRenderer != null)
                originalColor = plateRenderer.material.color;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other is CharacterController)
                playerInside = true;
            else if (other.attachedRigidbody != null)
                bodies.Add(other.attachedRigidbody);
            else
                return;

            Refresh();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other is CharacterController)
                playerInside = false;
            else if (other.attachedRigidbody != null)
                bodies.Remove(other.attachedRigidbody);
            else
                return;

            Refresh();
        }

        private void Refresh()
        {
            float total = playerInside ? playerWeight : 0f;
            foreach (Rigidbody body in bodies)
                total += body.mass;

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
