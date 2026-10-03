using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseEscape
{
    // Pantalla de nivel superado: volver al menú o repetir el nivel.
    public class PantallaCompletado : MonoBehaviour
    {
        [SerializeField] private AudioClip soundButton;

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
