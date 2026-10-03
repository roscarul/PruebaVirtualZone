using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseEscape.Player;

namespace LighthouseEscape
{
    // Trigger de salida del nivel: al atravesarlo se carga la pantalla completado.
    public class LevelFinish : MonoBehaviour
    {
        [SerializeField] private AudioClip soundComplete;

        private bool done;

        private void OnTriggerEnter(Collider other)
        {
            if (done || other.GetComponentInParent<PlayerController>() == null)
                return;

            done = true;
            GameFeedback.Play(soundComplete);
            SceneManager.LoadScene("Completado");
        }
    }
}
