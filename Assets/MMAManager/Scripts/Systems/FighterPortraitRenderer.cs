using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// UMA 파이터를 2D 초상화 텍스처로 렌더링하는 시스템
    /// 별도 카메라 + RenderTexture로 상반신 촬영 후 Texture2D로 변환
    /// </summary>
    public class FighterPortraitRenderer : MonoBehaviour
    {
        private static FighterPortraitRenderer _instance;
        public static FighterPortraitRenderer Instance => _instance;

        [Header("Portrait Settings")]
        [SerializeField] private int portraitWidth = 256;
        [SerializeField] private int portraitHeight = 256;
        [SerializeField] private int portraitLayer = 31; // unused layer for portrait rendering

        // Portrait cache (fighterId → texture)
        private Dictionary<string, Texture2D> portraitCache = new Dictionary<string, Texture2D>();

        // Rendering setup
        private Camera portraitCamera;
        private RenderTexture renderTexture;
        private Vector3 portraitStudioPosition = new Vector3(1000f, 0f, 1000f); // far away from gameplay

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SetupPortraitCamera();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupPortraitCamera()
        {
            // Create render texture
            renderTexture = new RenderTexture(portraitWidth, portraitHeight, 24);
            renderTexture.antiAliasing = 4;

            // Create portrait camera
            var camObj = new GameObject("PortraitCamera");
            camObj.transform.SetParent(transform);
            // 상반신 초상화
            // 자연스러운 초상화 각도: 눈높이보다 약간 위에서
            camObj.transform.position = portraitStudioPosition + new Vector3(0f, 1.7f, -1.6f);
            camObj.transform.LookAt(portraitStudioPosition + new Vector3(0f, 1.65f, 0f));

            portraitCamera = camObj.AddComponent<Camera>();
            portraitCamera.cullingMask = 1 << portraitLayer;
            portraitCamera.targetTexture = renderTexture;
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color(0.15f, 0.15f, 0.22f, 1f); // match CardColor
            portraitCamera.fieldOfView = 20f; // 좁은 FOV로 얼굴 클로즈업
            portraitCamera.nearClipPlane = 0.1f;
            portraitCamera.farClipPlane = 10f;
            portraitCamera.enabled = false; // only render on demand

            // Create directional light for portrait studio
            var lightObj = new GameObject("PortraitLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.position = portraitStudioPosition + new Vector3(0.5f, 2.5f, -1f);
            lightObj.transform.LookAt(portraitStudioPosition + new Vector3(0f, 1.6f, 0f));

            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.cullingMask = 1 << portraitLayer;
        }

        /// <summary>
        /// 캐시된 초상화가 있으면 즉시 반환, 없으면 null 반환 후 비동기 생성
        /// </summary>
        public Texture2D GetPortrait(Fighter fighter)
        {
            if (portraitCache.TryGetValue(fighter.FighterId, out Texture2D cached))
                return cached;

            return null;
        }

        /// <summary>
        /// 초상화를 비동기로 렌더링 (UMA 캐릭터 빌드 완료 후 촬영)
        /// </summary>
        public void RenderPortraitAsync(Fighter fighter, Action<Texture2D> onComplete)
        {
            // Check cache
            if (portraitCache.TryGetValue(fighter.FighterId, out Texture2D cached))
            {
                onComplete?.Invoke(cached);
                return;
            }

            StartCoroutine(RenderPortraitCoroutine(fighter, onComplete));
        }

        private IEnumerator RenderPortraitCoroutine(Fighter fighter, Action<Texture2D> onComplete)
        {
            var generator = UMAFighterGenerator.Instance;
            if (generator == null)
            {
                Debug.LogWarning("[PortraitRenderer] UMAFighterGenerator not found");
                onComplete?.Invoke(null);
                yield break;
            }

            bool characterReady = false;
            GameObject tempCharacter = null;

            // Generate temp UMA character at portrait studio position
            tempCharacter = generator.GenerateFighterObject(
                fighter,
                portraitStudioPosition,
                Quaternion.Euler(0f, 180f, 0f), // face camera (-Z)
                (go) =>
                {
                    characterReady = true;
                }
            );

            if (tempCharacter == null)
            {
                Debug.LogWarning("[PortraitRenderer] Failed to create temp character");
                onComplete?.Invoke(null);
                yield break;
            }

            // 레이어는 빌드 완료 후에 설정 (빌드 중 레이어 변경하면 UMA가 오작동)

            // Wait for UMA character build (max 15 seconds - 첫 로드 시 오래 걸림)
            float timeout = 15f;
            while (!characterReady && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (!characterReady)
            {
                Debug.LogWarning($"[PortraitRenderer] Character build timeout for {fighter.FullName}");
                Destroy(tempCharacter);
                onComplete?.Invoke(null);
                yield break;
            }

            // 빌드 완료 후 레이어 설정
            SetLayerRecursive(tempCharacter, portraitLayer);

            // Animator를 비활성화해서 T포즈 대신 기본 바인드포즈 유지
            var animator = tempCharacter.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            // 몇 프레임 대기 (렌더링 안정화)
            yield return null;
            yield return null;

            // UMA가 레이어 리셋할 수 있으니 다시 설정
            SetLayerRecursive(tempCharacter, portraitLayer);

            // 머리 뼈 위치 기반으로 카메라 동적 조정
            Vector3 headPos = portraitStudioPosition + new Vector3(0f, 1.65f, 0f); // 기본값
            if (animator != null)
            {
                Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                if (headBone != null)
                    headPos = headBone.position;
            }

            // 카메라를 머리 기준으로 배치
            portraitCamera.transform.position = headPos + new Vector3(0f, 0.05f, -1.6f);
            portraitCamera.transform.LookAt(headPos);

            // Ensure layer is set after UMA build (UMA might reset it)
            SetLayerRecursive(tempCharacter, portraitLayer);

            // Render portrait
            portraitCamera.enabled = true;
            portraitCamera.Render();
            portraitCamera.enabled = false;

            // Read pixels to Texture2D
            Texture2D portrait = new Texture2D(portraitWidth, portraitHeight, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            portrait.ReadPixels(new Rect(0, 0, portraitWidth, portraitHeight), 0, 0);
            portrait.Apply();
            RenderTexture.active = null;

            // Cache
            portraitCache[fighter.FighterId] = portrait;

            // Cleanup temp character
            Destroy(tempCharacter);

            Debug.Log($"[PortraitRenderer] Portrait rendered for {fighter.FullName}");
            onComplete?.Invoke(portrait);
        }

        /// <summary>
        /// 초상화를 Sprite로 변환
        /// </summary>
        public Sprite GetPortraitSprite(Fighter fighter)
        {
            var tex = GetPortrait(fighter);
            if (tex == null) return null;

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 특정 파이터 초상화 캐시 삭제
        /// </summary>
        public void InvalidatePortrait(string fighterId)
        {
            if (portraitCache.TryGetValue(fighterId, out Texture2D tex))
            {
                Destroy(tex);
                portraitCache.Remove(fighterId);
            }
        }

        /// <summary>
        /// 전체 캐시 삭제
        /// </summary>
        public void ClearCache()
        {
            foreach (var tex in portraitCache.Values)
            {
                if (tex != null) Destroy(tex);
            }
            portraitCache.Clear();
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private void OnDestroy()
        {
            ClearCache();
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
