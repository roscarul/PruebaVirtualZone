using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseEscape
{
    // Pantalla de nivel superado: volver al menú o repetir el nivel.
    public class PantallaCompletado : MonoBehaviour
    {
        [SerializeField] private AudioClip soundButton;

        private void Awake()
        {
            // El nivel deja el cursor bloqueado: se libera para poder pulsar los botones.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void GoToMenu()
        {
            GameFeedback.Play(soundButton);
            SceneManager.LoadScene("Menu");
        }

        public void Retry()
        {
            GameFeedback.Play(soundButton);
            SceneManager.LoadScene("EscapeRoomNivel");
        }
    }
}
