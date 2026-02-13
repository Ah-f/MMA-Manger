using UnityEngine;

namespace MMAManager.Combat
{
    public class CombatSoundManager : MonoBehaviour
    {
        private static CombatSoundManager instance;
        public static CombatSoundManager Instance => instance;

        private AudioSource audioSource;
        private AudioClip lightHitClip;
        private AudioClip heavyHitClip;
        private AudioClip koClip;
        private AudioClip blockClip;

        void Awake()
        {
            instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;

            GenerateClips();
        }

        private void GenerateClips()
        {
            lightHitClip = CreateImpactClip("LightHit", 0.08f, 300f, 0.5f);
            heavyHitClip = CreateImpactClip("HeavyHit", 0.15f, 150f, 0.8f);
            koClip = CreateKOClip();
            blockClip = CreateImpactClip("Block", 0.06f, 500f, 0.3f);
        }

        /// <summary>
        /// Creates a short percussive impact sound.
        /// Uses noise burst with pitch envelope for punchy hit feel.
        /// </summary>
        private AudioClip CreateImpactClip(string name, float duration, float basePitch, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / samples;

                // Sharp attack, fast decay envelope
                float envelope = Mathf.Exp(-progress * 8f) * (1f - Mathf.Exp(-progress * 80f));

                // Mix of noise + low sine for body
                float noise = (Random.value * 2f - 1f);
                float sine = Mathf.Sin(2f * Mathf.PI * basePitch * (1f - progress * 0.5f) * t);

                data[i] = (noise * 0.6f + sine * 0.4f) * envelope * volume;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Creates a dramatic KO impact: deeper, longer, with rumble.
        /// </summary>
        private AudioClip CreateKOClip()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / samples;

                // Longer envelope with sustain
                float envelope = Mathf.Exp(-progress * 4f) * (1f - Mathf.Exp(-progress * 60f));

                // Deep rumble
                float bass = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.5f;
                // Mid crack
                float mid = Mathf.Sin(2f * Mathf.PI * 200f * (1f - progress * 0.3f) * t) * 0.3f;
                // Noise for impact texture
                float noise = (Random.value * 2f - 1f) * 0.4f;
                // Sub hit
                float sub = Mathf.Sin(2f * Mathf.PI * 35f * t) * Mathf.Exp(-progress * 3f) * 0.3f;

                data[i] = (bass + mid + noise + sub) * envelope * 0.9f;
            }

            var clip = AudioClip.Create("KO", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void PlayHitSound(bool isHeavy)
        {
            if (audioSource == null) return;

            var clip = isHeavy ? heavyHitClip : lightHitClip;
            float pitch = Random.Range(0.85f, 1.15f);
            float vol = isHeavy ? 0.7f : 0.5f;

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, vol);
        }

        public void PlayBlockSound()
        {
            if (audioSource == null) return;

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(blockClip, 0.4f);
        }

        public void PlayKOSound()
        {
            if (audioSource == null) return;

            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(koClip, 1f);
        }
    }
}
