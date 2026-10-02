using UnityEngine;

namespace LighthouseEscape
{
    // Compuerta AND: acumula pulsos de varias fuentes (placa, receptor, candado...).
    // Solo desbloquea sus targets cuando hay requiredSources fuentes activas a la vez;
    // si se apaga alguna, vuelve a trabar todo lo que dependa de ella.
    public class ConditionGate : MonoBehaviour, IUnlockable
    {
        [SerializeField] private int requiredSources = 2;
        [SerializeField] private MonoBehaviour[] unlockTargets;

        private int activeSources;

        public bool Open => activeSources >= requiredSources;

        public void Unlock()
        {
            activeSources = Mathf.Min(activeSources + 1, requiredSources);
            Debug.Log($"[Puzzle] Compuerta AND: {activeSources}/{requiredSources} fuentes");
            Apply();
        }

        public void Lock()
        {
            if (activeSources > 0)
                activeSources--;

            Debug.Log($"[Puzzle] Compuerta AND: {activeSources}/{requiredSources} fuentes");
            Apply();
        }

        private void Apply()
        {
            bool open = Open;
            if (unlockTargets == null)
                return;

            foreach (MonoBehaviour target in unlockTargets)
            {
                if (target is IUnlockable unlockable)
                {
                    if (open) unlockable.Unlock();
                    else unlockable.Lock();
                }
            }
        }
    }
}
