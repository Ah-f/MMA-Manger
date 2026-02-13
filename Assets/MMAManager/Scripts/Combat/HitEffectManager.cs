using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMAManager.Combat
{
    public class HitEffectManager : MonoBehaviour
    {
        private static HitEffectManager instance;
        private Camera mainCam;
        private Vector3 originalCamPos;
        private bool isShaking;

        // Particle pool
        private ParticleSystem hitParticleLight;
        private ParticleSystem hitParticleHeavy;

        void Awake()
        {
            instance = this;
            mainCam = Camera.main;
            if (mainCam != null)
                originalCamPos = mainCam.transform.localPosition;

            CreateHitParticles();
        }

        public static HitEffectManager Instance => instance;

        #region Particle Setup

        private void CreateHitParticles()
        {
            hitParticleLight = CreateParticleSystem("HitFX_Light",
                new Color(1f, 0.9f, 0.6f, 1f), 20, 0.4f, 3f);
            hitParticleHeavy = CreateParticleSystem("HitFX_Heavy",
                new Color(1f, 0.3f, 0.2f, 1f), 35, 0.6f, 4f);
        }

        private ParticleSystem CreateParticleSystem(string name, Color color, int burstCount, float size, float speed)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);

            var ps = go.AddComponent<ParticleSystem>();

            // Stop immediately to avoid "duration while playing" warning
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.5f;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 50;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 2f;

            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, burstCount)
            });

            // Shape - sphere spread
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // Size over lifetime - shrink
            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            // Color over lifetime - fade out
            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(color, 0.3f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLife.color = gradient;

            // Renderer
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                ?? Shader.Find("Mobile/Particles/Additive");
            if (shader != null)
            {
                renderer.material = new Material(shader);
                renderer.material.color = color;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        #endregion

        #region Public API

        public void PlayHitEffect(Vector3 position, bool isHeavy)
        {
            // Particle burst
            var ps = isHeavy ? hitParticleHeavy : hitParticleLight;
            if (ps != null)
            {
                ps.transform.position = position;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }

            // Camera shake
            if (isHeavy)
                StartCoroutine(CameraShake(0.15f, 0.12f));
            else
                StartCoroutine(CameraShake(0.08f, 0.04f));
        }

        public void PlayFlash(GameObject target, bool isHeavy)
        {
            StartCoroutine(MaterialFlash(target, isHeavy));
        }

        #endregion

        #region Camera Shake

        private IEnumerator CameraShake(float duration, float magnitude)
        {
            if (mainCam == null || isShaking) yield break;

            isShaking = true;
            var combatCam = CombatCamera.Instance;
            if (combatCam != null) combatCam.IsShaking = true;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                if (combatCam != null)
                {
                    // Use CombatCamera's shake offset system
                    combatCam.SetShakeOffset(new Vector3(x, y, 0f));
                }
                else
                {
                    mainCam.transform.localPosition = originalCamPos + new Vector3(x, y, 0f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (combatCam != null)
            {
                combatCam.SetShakeOffset(Vector3.zero);
                combatCam.IsShaking = false;
            }
            else
            {
                mainCam.transform.localPosition = originalCamPos;
            }

            isShaking = false;
        }

        #endregion

        #region Material Flash

        private IEnumerator MaterialFlash(GameObject target, bool isHeavy)
        {
            if (target == null) yield break;

            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) yield break;

            // Store original colors
            var originalColors = new Dictionary<Renderer, Color[]>();
            foreach (var r in renderers)
            {
                var colors = new Color[r.materials.Length];
                for (int i = 0; i < r.materials.Length; i++)
                {
                    if (r.materials[i].HasProperty("_Color"))
                        colors[i] = r.materials[i].color;
                    else
                        colors[i] = Color.white;
                }
                originalColors[r] = colors;
            }

            // Flash color
            Color flashColor = isHeavy
                ? new Color(1f, 0.3f, 0.3f, 1f)
                : new Color(1f, 0.85f, 0.85f, 1f);

            // Apply flash
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                        mat.color = flashColor;
                }
            }

            // Hold flash
            yield return new WaitForSeconds(isHeavy ? 0.1f : 0.06f);

            // Fade back to original
            float fadeDuration = isHeavy ? 0.15f : 0.1f;
            float fadeElapsed = 0f;

            while (fadeElapsed < fadeDuration)
            {
                float t = fadeElapsed / fadeDuration;
                foreach (var r in renderers)
                {
                    if (!originalColors.ContainsKey(r)) continue;
                    var origColors = originalColors[r];
                    for (int i = 0; i < r.materials.Length; i++)
                    {
                        if (r.materials[i].HasProperty("_Color"))
                            r.materials[i].color = Color.Lerp(flashColor, origColors[i], t);
                    }
                }
                fadeElapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure original colors restored
            foreach (var r in renderers)
            {
                if (!originalColors.ContainsKey(r)) continue;
                var origColors = originalColors[r];
                for (int i = 0; i < r.materials.Length; i++)
                {
                    if (r.materials[i].HasProperty("_Color"))
                        r.materials[i].color = origColors[i];
                }
            }
        }

        #endregion
    }
}
