using UnityEngine;

namespace LighthouseEscape
{
    // Objeto de inspección: al pulsar E se pone en foco frente a la cámara y se rota con el
    // ratón para examinarlo.
    [RequireComponent(typeof(Collider))]
    public class Inspectable : MonoBehaviour
    {
        [SerializeField] private float focusDistance = 0.6f;
        [SerializeField] private float rotationSpeed = 0.3f;   // grados por píxel de ratón
        [SerializeField] private string caption = "";           // pista de texto (se ve con la cara trasera a la vista)

        public float FocusDistance => focusDistance;
        public float RotationSpeed => rotationSpeed;
        public string Caption => caption;

        public Renderer CachedRenderer { get; private set; }

        private void Awake()
        {
            CachedRenderer = GetComponent<Renderer>();
        }
    }
}
