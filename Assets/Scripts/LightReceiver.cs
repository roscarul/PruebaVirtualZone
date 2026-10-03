using UnityEngine;

namespace LighthouseEscape
{
    // Receptor del haz: en reposo brilla en rojo; cuando llega el haz pasa a
    // verde, reproduce su sonido y desbloquea sus objetivos. Si el haz se
    // corta, vuelve a rojo y los vuelve a bloquear.
    public class LightReceiver : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] unlockTargets;
        [SerializeField] private AudioClip soundPowered;
        [SerializeField] private Color unpoweredColor = new Color(0.85f, 0.1f, 0.08f, 0.35f);
        [SerializeField] private Color poweredColor = new Color(0.45f, 1f, 0.5f, 1f);

        private Material receiverMaterial;
        private bool powered;

        private void Awake()
        {
            Renderer receiverRenderer = GetComponent<Renderer>();

            if (receiverRenderer != null)
                receiverMaterial = receiverRenderer.material;

            ApplyVisual();
        }

        public void SetPowered(bool on)
        {
            if (on == powered)
                return;

            powered = on;
            ApplyVisual();

            if (on)
            {
                GameFeedback.Play(soundPowered);
                Debug.Log("[Puzzle] Haz conectado");
            }
            else
            {
                Debug.Log("[Puzzle] Haz cortado");
            }

            if (unlockTargets == null)
                return;

            foreach (MonoBehaviour target in unlockTargets)
            {
                if (!(target is IUnlockable unlockable))
                    continue;

                if (on)
                    unlockable.Unlock();
                else
                    unlockable.Lock();
            }
        }

        private void ApplyVisual()
        {
            if (receiverMaterial == null)
                return;

            Color color = powered ? poweredColor : unpoweredColor;
            receiverMaterial.color = color;
            receiverMaterial.SetColor("_EmissionColor", color * 2f);
            receiverMaterial.EnableKeyword("_EMISSION");
        }
    }
}
