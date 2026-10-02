using UnityEngine;

namespace LighthouseEscape
{
    // Receptor del haz: encendido se desbloquea , si el haz se corta se apaga.
    public class LightReceiver : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] unlockTargets;

        private Material receiverMaterial;
        private Color idleColor;
        private bool powered;

        private void Awake()
        {
            Renderer receiverRenderer = GetComponent<Renderer>();

            if (receiverRenderer != null)
            {
                receiverMaterial = receiverRenderer.material;
                idleColor = receiverMaterial.color;
            }
        }

        public void SetPowered(bool on)
        {
            if (on == powered)
                return;

            powered = on;

            if (receiverMaterial != null)
                receiverMaterial.color = on ? new Color(0.45f, 1f, 0.5f) : idleColor;

            if (on)
                Debug.Log("[Puzzle] Haz conectado");
            else
                Debug.Log("[Puzzle] Haz cortado");

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
    }
}
