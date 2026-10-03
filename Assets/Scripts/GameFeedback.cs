using UnityEngine;

namespace LighthouseEscape
{
    // Reproductor central de sonidos de jugada: reproduce los AudioClips que
    // se asignan en el inspector de cada componente (sin generar nada en runtime).
    public static class GameFeedback
    {
        private static AudioSource source;

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
                return;

            if (source == null)
            {
                GameObject go = new GameObject("GameFeedbackAudio");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
            }

            source.PlayOneShot(clip, volume);
        }
    }
}
