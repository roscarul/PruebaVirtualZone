using UnityEngine;

namespace LighthouseEscape
{
    // Panel que tapa el hueco de la pared: sube cuando la placa soporta peso y baja si lo quitan.
    public class SlidingPanel : MonoBehaviour, IUnlockable
    {
        [SerializeField] private float slideDistance = 1.85f;
        [SerializeField] private float slideSpeed = 1.4f;

        private Vector3 closedPos;
        private float amount;
        private bool opening;

        public bool IsOpen => opening && amount >= slideDistance;

        private void Start()
        {
            closedPos = transform.position;
        }

        public void Unlock()
        {
            opening = true;
        }

        public void Lock()
        {
            opening = false;
        }

        private void Update()
        {
            float target = opening ? slideDistance : 0f;
            if (Mathf.Approximately(amount, target))
                return;

            amount = Mathf.MoveTowards(amount, target, slideSpeed * Time.deltaTime);
            transform.position = closedPos + Vector3.up * amount;
        }
    }
}
