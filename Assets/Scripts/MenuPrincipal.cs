using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseEscape
{
    // Menú inicial: Jugar carga el nivel y Salir cierra la aplicación.
    public class MenuPrincipal : MonoBehaviour
    {
        [SerializeField] private AudioClip soundButton;

        public void StartGame()
        {
            GameFeedback.Play(soundButton);
            SceneManager.LoadScene("EscapeRoomNivel");
        }

        public void QuitGame()
        {
            GameFeedback.Play(soundButton);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
