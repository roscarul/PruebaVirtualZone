using UnityEngine;

namespace LighthouseEscape
{
    // Espejo del puzzle de haz, gira 45°.
    public class Mirror : MonoBehaviour
    {
        private const float StepAngle = 45f;

        [SerializeField] private AudioClip soundRotate;

        public void Rotate()
        {
            transform.Rotate(0f, StepAngle, 0f, Space.Self);
            GameFeedback.Play(soundRotate);
        }
    }
}
