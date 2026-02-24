using System;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// UMA 기반 파이터 3D 외형 생성 시스템
    /// </summary>
    public class UMAFighterGenerator : MonoBehaviour
    {
        private static UMAFighterGenerator _instance;
        public static UMAFighterGenerator Instance => _instance;

        [Header("UMA Settings")]
        [SerializeField] private GameObject umaAvatarPrefab;
        [SerializeField] private RuntimeAnimatorController fighterAnimatorController;

        // 피부톤 프리셋 (R, G, B 범위 0-1)
        private static readonly Vector3[] SkinTonePresets = {
            new Vector3(0.95f, 0.80f, 0.65f),  // 밝은 피부
            new Vector3(0.85f, 0.70f, 0.55f),  // 연한 피부
            new Vector3(0.75f, 0.58f, 0.42f),  // 중간 피부
            new Vector3(0.65f, 0.48f, 0.35f),  // 올리브
            new Vector3(0.55f, 0.38f, 0.25f),  // 갈색
            new Vector3(0.45f, 0.30f, 0.20f),  // 진한 갈색
            new Vector3(0.35f, 0.22f, 0.15f),  // 어두운 피부
            new Vector3(0.25f, 0.16f, 0.10f),  // 매우 어두운 피부
        };

        // 체급별 DNA 프리셋
        private static readonly Dictionary<WeightClass, float[]> WeightClassDNA = new Dictionary<WeightClass, float[]>
        {
            //                                    height, upperWeight, lowerWeight, bodyFitness
            { WeightClass.Flyweight,        new[] { 0.30f, 0.25f, 0.25f, 0.55f } },
            { WeightClass.Bantamweight,     new[] { 0.35f, 0.30f, 0.28f, 0.58f } },
            { WeightClass.Featherweight,    new[] { 0.38f, 0.33f, 0.30f, 0.60f } },
            { WeightClass.Lightweight,      new[] { 0.42f, 0.35f, 0.33f, 0.62f } },
            { WeightClass.Welterweight,     new[] { 0.48f, 0.40f, 0.38f, 0.65f } },
            { WeightClass.Middleweight,     new[] { 0.55f, 0.48f, 0.45f, 0.68f } },
            { WeightClass.LightHeavyweight, new[] { 0.65f, 0.55f, 0.52f, 0.70f } },
            { WeightClass.Heavyweight,      new[] { 0.78f, 0.65f, 0.60f, 0.60f } },
        };

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                FindUMAPrefab();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void FindUMAPrefab()
        {
            if (umaAvatarPrefab == null)
            {
                // 런타임에 UMA 프리팹 찾기
                umaAvatarPrefab = Resources.Load<GameObject>("UMADynamicCharacterAvatar");
                if (umaAvatarPrefab == null)
                {
                    Debug.LogWarning("[UMAFighterGenerator] UMA Avatar prefab not assigned. Assign in Inspector or place in Resources folder.");
                }
            }
        }

        /// <summary>
        /// Fighter 데이터 기반으로 UMA 캐릭터 생성
        /// </summary>
        public GameObject GenerateFighterObject(Fighter fighter, Vector3 position, Quaternion rotation, Action<GameObject> onComplete = null)
        {
            if (umaAvatarPrefab == null)
            {
                Debug.LogError("[UMAFighterGenerator] UMA Avatar prefab is null!");
                return null;
            }

            var go = Instantiate(umaAvatarPrefab, position, rotation);
            go.name = $"UMA_{fighter.FirstName}_{fighter.LastName}";

            var avatar = go.GetComponent<DynamicCharacterAvatar>();
            if (avatar == null)
            {
                Debug.LogError("[UMAFighterGenerator] DynamicCharacterAvatar component not found on prefab!");
                Destroy(go);
                return null;
            }

            // 레이스 설정
            avatar.ChangeRace("HumanMale");

            // 애니메이터 컨트롤러 설정
            if (fighterAnimatorController != null)
            {
                avatar.animationController = fighterAnimatorController;
            }

            // predefinedDNA로 빌드 전에 DNA 설정 (빌드 시 자동 적용)
            SetPredefinedDNA(avatar, fighter);

            // 피부색 설정
            ApplySkinColor(avatar, fighter);

            // 빌드 완료 콜백
            avatar.CharacterCreated.AddListener((umaData) =>
            {
                OnCharacterCreated(go, fighter);
                onComplete?.Invoke(go);
            });

            // 캐릭터 빌드
            avatar.BuildCharacter(true);

            return go;
        }

        /// <summary>
        /// Fighter 데이터를 predefinedDNA로 설정 (빌드 시 자동 적용)
        /// </summary>
        private void SetPredefinedDNA(DynamicCharacterAvatar avatar, Fighter fighter)
        {
            // 시드 기반 랜덤 (동일 시드 → 동일 외형)
            var rng = new System.Random(fighter.AppearanceSeed);

            // predefinedDNA에 추가
            System.Action<string, float> setDna = (name, value) =>
            {
                avatar.predefinedDNA.AddDNA(name, Mathf.Clamp01(value));
            };

            // 체급별 기본 체형
            if (WeightClassDNA.TryGetValue(fighter.WeightClass, out float[] wcDna))
            {
                setDna("height", wcDna[0] + RngRange(rng, -0.03f, 0.03f));
                setDna("upperWeight", wcDna[1] + RngRange(rng, -0.05f, 0.05f));
                setDna("lowerWeight", wcDna[2] + RngRange(rng, -0.05f, 0.05f));
            }

            // 스탯 기반 근육량
            float strFactor = fighter.STR / 100f;
            setDna("upperMuscle", Mathf.Lerp(0.2f, 0.85f, strFactor) + RngRange(rng, -0.05f, 0.05f));
            setDna("lowerMuscle", Mathf.Lerp(0.25f, 0.75f, strFactor) + RngRange(rng, -0.05f, 0.05f));

            // 체력 기반 피트니스 보정
            float staFactor = fighter.STA / 100f;
            setDna("belly", Mathf.Lerp(0.6f, 0.3f, staFactor));
            setDna("waist", Mathf.Lerp(0.6f, 0.35f, staFactor));

            // 얼굴 랜덤 (넓은 범위로 차이 확실하게)
            setDna("headSize", RngRange(rng, 0.25f, 0.75f));
            setDna("headWidth", RngRange(rng, 0.25f, 0.75f));
            setDna("neckThickness", Mathf.Lerp(0.3f, 0.7f, strFactor));

            setDna("noseSize", RngRange(rng, 0.15f, 0.85f));
            setDna("noseCurve", RngRange(rng, 0.10f, 0.90f));
            setDna("noseWidth", RngRange(rng, 0.15f, 0.85f));
            setDna("nosePronounced", RngRange(rng, 0.10f, 0.90f));
            setDna("noseFlatten", RngRange(rng, 0.10f, 0.90f));

            setDna("jawsSize", RngRange(rng, 0.20f, 0.80f));
            setDna("jawsPosition", RngRange(rng, 0.20f, 0.80f));
            setDna("chinSize", RngRange(rng, 0.15f, 0.85f));
            setDna("chinPronounced", RngRange(rng, 0.15f, 0.85f));
            setDna("chinPosition", RngRange(rng, 0.20f, 0.80f));

            setDna("cheekSize", RngRange(rng, 0.15f, 0.85f));
            setDna("cheekPosition", RngRange(rng, 0.20f, 0.80f));
            setDna("lowCheekPronounced", RngRange(rng, 0.15f, 0.85f));
            setDna("foreheadSize", RngRange(rng, 0.20f, 0.80f));
            setDna("foreheadPosition", RngRange(rng, 0.20f, 0.80f));

            setDna("lipsSize", RngRange(rng, 0.15f, 0.85f));
            setDna("mouthSize", RngRange(rng, 0.20f, 0.80f));

            setDna("eyeSize", RngRange(rng, 0.20f, 0.80f));
            setDna("eyeRotation", RngRange(rng, 0.25f, 0.75f));
            setDna("eyeSpacing", RngRange(rng, 0.25f, 0.75f));

            setDna("earsSize", RngRange(rng, 0.20f, 0.80f));
            setDna("earsPosition", RngRange(rng, 0.20f, 0.80f));
            setDna("earsRotation", RngRange(rng, 0.25f, 0.75f));

            // 팔/다리
            setDna("armLength", RngRange(rng, 0.40f, 0.60f));
            setDna("forearmLength", RngRange(rng, 0.40f, 0.60f));
            setDna("armWidth", Mathf.Lerp(0.3f, 0.7f, strFactor));
            setDna("forearmWidth", Mathf.Lerp(0.3f, 0.65f, strFactor));
            setDna("handsSize", RngRange(rng, 0.35f, 0.65f));
            setDna("feetSize", RngRange(rng, 0.35f, 0.65f));

            Debug.Log($"[UMAFighterGenerator] DNA 설정 완료: {fighter.FullName} (seed:{fighter.AppearanceSeed})");
        }

        /// <summary>
        /// 피부색을 UMA SharedColor로 설정
        /// </summary>
        private void ApplySkinColor(DynamicCharacterAvatar avatar, Fighter fighter)
        {
            int skinIdx = Mathf.Clamp(fighter.SkinToneIndex, 0, SkinTonePresets.Length - 1);
            Vector3 tone = SkinTonePresets[skinIdx];
            Color skinColor = new Color(tone.x, tone.y, tone.z, 1f);
            avatar.SetColor("Skin", skinColor);
        }

        private void OnCharacterCreated(GameObject go, Fighter fighter)
        {
            Debug.Log($"[UMAFighterGenerator] 캐릭터 생성 완료: {fighter.FullName}");

            // Animator 설정 보정
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        /// <summary>
        /// 시드 기반 범위 랜덤
        /// </summary>
        private float RngRange(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        /// <summary>
        /// 애니메이터 컨트롤러 설정
        /// </summary>
        public void SetAnimatorController(RuntimeAnimatorController controller)
        {
            fighterAnimatorController = controller;
        }
    }
}
