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

        [Header("Tattoo Settings")]
        [SerializeField] private bool tattooEnabled = true;
        [Range(0f, 1f)] [SerializeField] private float tattooChance = 0.4f;

        // 문신 위치 프리셋 (UV 좌표)
        private static readonly Vector2[] TattooPositionPresets = {
            new Vector2(0.606f, 0.18f),  // 왼쪽 종아리
            new Vector2(0.292f, 0.18f),  // 오른 종아리
            new Vector2(0.089f, 0.805f), // 오른 가슴/어깨
            new Vector2(0.243f, 0.805f), // 중앙 목/가슴
            new Vector2(0.373f, 0.805f), // 왼 어깨
            new Vector2(0.444f, 0.805f), // 왼 어깨/등
        };
        private const float TattooFixedSize = 0.25f;

        // MMA 파이터용 남성 헤어스타일 레시피 이름 (UMA Wardrobe Recipe)
        // null = 대머리 (MMA에서 흔함)
        private static readonly string[] MaleHairRecipes = {
            null,                        // 대머리 (30% 확률로 나오도록 3개)
            null,
            null,
            "MaleHair1",                 // 기본 헤어 1
            "MaleHair2",                 // 기본 헤어 2
            "MaleHair3",                 // 기본 헤어 3
            "MaleShortHair_Recipe",      // 숏컷
            "MaleHairSlick01_Recipe",    // 슬릭백
            "bb_Male_Military_Hair",     // 밀리터리 컷
        };

        // 머리카락 색상 프리셋
        private static readonly Color[] HairColorPresets = {
            // 흑발 계열 (아시아/아프리카 등 가장 흔함 - 3개로 비중 높임)
            new Color(0.02f, 0.02f, 0.02f, 1f),  // 순수 흑발
            new Color(0.05f, 0.03f, 0.02f, 1f),  // 흑발
            new Color(0.08f, 0.05f, 0.03f, 1f),  // 흑갈색
            // 갈색 계열
            new Color(0.12f, 0.07f, 0.04f, 1f),  // 매우 진한 갈색
            new Color(0.20f, 0.12f, 0.06f, 1f),  // 진한 갈색
            new Color(0.30f, 0.18f, 0.08f, 1f),  // 갈색
            new Color(0.40f, 0.25f, 0.12f, 1f),  // 밝은 갈색
            new Color(0.48f, 0.32f, 0.16f, 1f),  // 밤색
            // 금발 계열
            new Color(0.55f, 0.42f, 0.22f, 1f),  // 어두운 금발
            new Color(0.65f, 0.50f, 0.28f, 1f),  // 금갈색
            new Color(0.75f, 0.60f, 0.35f, 1f),  // 금발
            new Color(0.85f, 0.72f, 0.45f, 1f),  // 밝은 금발
            // 적색 계열
            new Color(0.30f, 0.08f, 0.03f, 1f),  // 적갈색
            new Color(0.45f, 0.12f, 0.05f, 1f),  // 붉은 머리
            new Color(0.55f, 0.18f, 0.08f, 1f),  // 밝은 적색
            // 회색/백발 (나이든 파이터)
            new Color(0.45f, 0.45f, 0.45f, 1f),  // 회색
            new Color(0.65f, 0.65f, 0.63f, 1f),  // 은발
        };

        // 눈 색상 프리셋
        private static readonly Color[] EyeColorPresets = {
            new Color(0.25f, 0.15f, 0.05f, 1f),  // 진한 갈색
            new Color(0.35f, 0.22f, 0.10f, 1f),  // 갈색
            new Color(0.45f, 0.32f, 0.15f, 1f),  // 밝은 갈색
            new Color(0.22f, 0.32f, 0.18f, 1f),  // 녹색
            new Color(0.30f, 0.40f, 0.25f, 1f),  // 연녹색
            new Color(0.25f, 0.35f, 0.50f, 1f),  // 청회색
            new Color(0.20f, 0.30f, 0.55f, 1f),  // 파란색
            new Color(0.15f, 0.15f, 0.15f, 1f),  // 거의 검정
        };

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
            { WeightClass.Flyweight,        new[] { 0.42f, 0.40f, 0.38f, 0.60f } },
            { WeightClass.Bantamweight,     new[] { 0.45f, 0.43f, 0.40f, 0.62f } },
            { WeightClass.Featherweight,    new[] { 0.48f, 0.45f, 0.42f, 0.63f } },
            { WeightClass.Lightweight,      new[] { 0.51f, 0.48f, 0.45f, 0.65f } },
            { WeightClass.Welterweight,     new[] { 0.54f, 0.50f, 0.48f, 0.66f } },
            { WeightClass.Middleweight,     new[] { 0.57f, 0.53f, 0.50f, 0.67f } },
            { WeightClass.LightHeavyweight, new[] { 0.61f, 0.56f, 0.53f, 0.65f } },
            { WeightClass.Heavyweight,      new[] { 0.65f, 0.60f, 0.57f, 0.60f } },
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

            // 헤어스타일 설정
            ApplyHairStyle(avatar, fighter);

            // MMA 의상 설정 (반바지 + 글러브) - 빌드 전에 설정
            ApplyFighterOutfit(avatar, fighter);

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

            // 스탯 기반 근육량 (0.6 중심)
            float strFactor = fighter.STR / 100f;
            setDna("upperMuscle", Mathf.Lerp(0.50f, 0.75f, strFactor) + RngRange(rng, -0.03f, 0.03f));
            setDna("lowerMuscle", Mathf.Lerp(0.50f, 0.70f, strFactor) + RngRange(rng, -0.03f, 0.03f));

            // 체력 기반 피트니스 보정
            float staFactor = fighter.STA / 100f;
            setDna("belly", Mathf.Lerp(0.65f, 0.50f, staFactor));
            setDna("waist", Mathf.Lerp(0.65f, 0.52f, staFactor));

            // 머리/목 (0.6 중심 ±0.08)
            setDna("headSize", RngRange(rng, 0.52f, 0.68f));
            setDna("headWidth", RngRange(rng, 0.52f, 0.68f));
            setDna("neckThickness", Mathf.Lerp(0.55f, 0.68f, strFactor));

            // 코 (0.6 중심 ±0.15)
            setDna("noseSize", RngRange(rng, 0.45f, 0.75f));
            setDna("noseCurve", RngRange(rng, 0.42f, 0.78f));
            setDna("noseWidth", RngRange(rng, 0.45f, 0.75f));
            setDna("nosePronounced", RngRange(rng, 0.42f, 0.78f));
            setDna("noseFlatten", RngRange(rng, 0.45f, 0.75f));

            // 턱 (0.6 중심 ±0.13)
            setDna("jawsSize", RngRange(rng, 0.47f, 0.73f));
            setDna("jawsPosition", RngRange(rng, 0.50f, 0.70f));
            setDna("chinSize", RngRange(rng, 0.45f, 0.75f));
            setDna("chinPronounced", RngRange(rng, 0.45f, 0.75f));
            setDna("chinPosition", RngRange(rng, 0.50f, 0.70f));

            // 볼/이마 (0.6 중심 ±0.12)
            setDna("cheekSize", RngRange(rng, 0.48f, 0.72f));
            setDna("cheekPosition", RngRange(rng, 0.50f, 0.70f));
            setDna("lowCheekPronounced", RngRange(rng, 0.48f, 0.72f));
            setDna("foreheadSize", RngRange(rng, 0.50f, 0.70f));
            setDna("foreheadPosition", RngRange(rng, 0.50f, 0.70f));

            // 입 (0.6 중심 ±0.12)
            setDna("lipsSize", RngRange(rng, 0.48f, 0.72f));
            setDna("mouthSize", RngRange(rng, 0.50f, 0.70f));

            // 눈 (0.6 중심 ±0.12)
            setDna("eyeSize", RngRange(rng, 0.48f, 0.72f));
            setDna("eyeRotation", RngRange(rng, 0.50f, 0.70f));
            setDna("eyeSpacing", RngRange(rng, 0.50f, 0.70f));

            // 귀 (0.6 중심 ±0.06)
            setDna("earsSize", RngRange(rng, 0.54f, 0.66f));
            setDna("earsPosition", RngRange(rng, 0.55f, 0.65f));
            setDna("earsRotation", RngRange(rng, 0.55f, 0.65f));

            // 팔/다리 (0.6 중심)
            setDna("armLength", RngRange(rng, 0.56f, 0.65f));
            setDna("forearmLength", RngRange(rng, 0.55f, 0.65f));
            setDna("armWidth", Mathf.Lerp(0.50f, 0.72f, strFactor));
            setDna("forearmWidth", Mathf.Lerp(0.50f, 0.68f, strFactor));
            setDna("handsSize", RngRange(rng, 0.54f, 0.66f));
            setDna("feetSize", RngRange(rng, 0.54f, 0.66f));

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

            // 눈 색상 (시드 기반)
            var rng = new System.Random(fighter.AppearanceSeed + 333);
            int eyeIdx = rng.Next(EyeColorPresets.Length);
            avatar.SetColor("Eyes", EyeColorPresets[eyeIdx]);
        }

        /// <summary>
        /// 헤어스타일 + 머리카락 색상 적용
        /// </summary>
        private void ApplyHairStyle(DynamicCharacterAvatar avatar, Fighter fighter)
        {
            var rng = new System.Random(fighter.AppearanceSeed + 777); // 별도 시드 오프셋

            // 헤어스타일 선택
            int hairIdx = rng.Next(MaleHairRecipes.Length);
            string hairRecipe = MaleHairRecipes[hairIdx];

            if (hairRecipe != null)
            {
                avatar.SetSlot("Hair", hairRecipe);

                // 머리카락 색상 설정
                int colorIdx = rng.Next(HairColorPresets.Length);
                Color hairColor = HairColorPresets[colorIdx];
                avatar.SetColor("Hair", hairColor);

                Debug.Log($"[UMAFighterGenerator] 헤어 설정: {fighter.FullName} → {hairRecipe}");
            }
            else
            {
                // 대머리 - Hair 슬롯 비우기
                avatar.ClearSlot("Hair");
                Debug.Log($"[UMAFighterGenerator] 헤어 설정: {fighter.FullName} → 대머리");
            }
        }

        // MMA 반바지 레시피 (Legs 슬롯)
        private static readonly string[] ShortsRecipes = {
            "MaleShorts1",
        };

        // 반바지 색상 프리셋
        private static readonly Color[] ShortsColorPresets = {
            new Color(0.10f, 0.10f, 0.10f, 1f),  // 검정
            new Color(0.15f, 0.15f, 0.30f, 1f),  // 네이비
            new Color(0.30f, 0.10f, 0.10f, 1f),  // 다크레드
            new Color(0.10f, 0.25f, 0.10f, 1f),  // 다크그린
            new Color(0.25f, 0.25f, 0.25f, 1f),  // 다크그레이
            new Color(0.80f, 0.80f, 0.80f, 1f),  // 화이트
            new Color(0.60f, 0.15f, 0.05f, 1f),  // 오렌지레드
            new Color(0.05f, 0.20f, 0.45f, 1f),  // 블루
        };

        /// <summary>
        /// MMA 파이터 의상 적용 (반바지 + 글러브)
        /// </summary>
        private void ApplyFighterOutfit(DynamicCharacterAvatar avatar, Fighter fighter)
        {
            var rng = new System.Random(fighter.AppearanceSeed + 555);

            // 반바지
            string shorts = ShortsRecipes[rng.Next(ShortsRecipes.Length)];
            avatar.SetSlot("Legs", shorts);

            // 글러브
            avatar.SetSlot("Hands", "MaleGloves");

            // 반바지 색상은 빌드 후 콜백에서 적용 (SharedColor 이름을 런타임에 확인)
            int colorIdx = rng.Next(ShortsColorPresets.Length);
            _pendingShortsColor[fighter.FighterId] = ShortsColorPresets[colorIdx];

            Debug.Log($"[UMAFighterGenerator] 의상 설정: {fighter.FullName} → {shorts}");
        }

        // 빌드 후 적용할 반바지 색상 임시 저장
        private Dictionary<string, Color> _pendingShortsColor = new Dictionary<string, Color>();

        private void OnCharacterCreated(GameObject go, Fighter fighter)
        {
            Debug.Log($"[UMAFighterGenerator] 캐릭터 생성 완료: {fighter.FullName}");

            // Animator 설정 보정
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            // 반바지 색상 적용
            if (_pendingShortsColor.TryGetValue(fighter.FighterId, out Color shortsColor))
            {
                var avatar = go.GetComponent<DynamicCharacterAvatar>();
                if (avatar != null)
                {
                    foreach (var colorName in avatar.CurrentSharedColors)
                    {
                        string name = colorName.name;
                        if (name != "Skin" && name != "Hair" && name != "Eyes")
                        {
                            avatar.SetColor(name, shortsColor);
                        }
                    }
                    avatar.UpdateColors(true);
                }
                _pendingShortsColor.Remove(fighter.FighterId);
            }

            // 문신 적용 (빌드 후, 바지 메쉬가 자연스럽게 덮음)
            if (tattooEnabled)
            {
                var tattooRng = new System.Random(fighter.AppearanceSeed + 999);
                if (tattooRng.NextDouble() < tattooChance)
                {
                    ApplyTattoo(go, fighter);
                }
            }
        }

        // TAT.png 스프라이트시트 설정 (5열 x 4행 = 20개 문신)
        private const int TattooColumns = 5;
        private const int TattooRows = 4;
        private const int TattooCount = 20;
        private Texture2D _tatSpriteSheet;

        /// <summary>
        /// TAT.png 스프라이트시트에서 개별 문신 추출 후 오버레이 적용
        /// </summary>
        private void ApplyTattoo(GameObject go, Fighter fighter)
        {
            var umaData = go.GetComponent<UMAData>();
            if (umaData == null || umaData.umaRecipe == null) return;

            try
            {
                // 스프라이트시트 로드 (캐시)
                if (_tatSpriteSheet == null)
                {
                    _tatSpriteSheet = Resources.Load<Texture2D>("TAT");
                    if (_tatSpriteSheet == null)
                    {
                        Debug.LogWarning("[UMAFighterGenerator] TAT.png not found in Resources");
                        return;
                    }
                }

                // WolfTattoo 오버레이를 템플릿으로 사용
                var context = UMAContextBase.Instance;
                if (context == null) return;
                var tattooOverlay = context.InstantiateOverlay("WolfTattoo");
                if (tattooOverlay == null)
                {
                    Debug.LogWarning("[UMAFighterGenerator] WolfTattoo overlay not found");
                    return;
                }

                // 랜덤 문신 선택
                var tattooRng = new System.Random(fighter.AppearanceSeed + 1234);
                int idx = tattooRng.Next(TattooCount);
                int col = idx % TattooColumns;
                int row = idx / TattooColumns;

                // GPU로 개별 문신 추출 (Read/Write 불필요)
                int cellW = _tatSpriteSheet.width / TattooColumns;
                int cellH = _tatSpriteSheet.height / TattooRows;

                RenderTexture rt = RenderTexture.GetTemporary(cellW, cellH, 0, RenderTextureFormat.ARGB32);
                rt.filterMode = FilterMode.Bilinear;
                Graphics.Blit(_tatSpriteSheet, rt,
                    new Vector2(1f / TattooColumns, 1f / TattooRows),
                    new Vector2((float)col / TattooColumns, (float)(TattooRows - 1 - row) / TattooRows));

                Texture2D tattooTex = new Texture2D(cellW, cellH, TextureFormat.RGBA32, false);
                RenderTexture.active = rt;
                tattooTex.ReadPixels(new Rect(0, 0, cellW, cellH), 0, 0);
                tattooTex.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                // 오버레이 텍스처 교체
                tattooOverlay.asset.textureList[0] = tattooTex;

                // 오퍼시티 0.65
                tattooOverlay.colorData.channelMask[0] = new Color(1f, 1f, 1f, 0.4f);

                // 프리셋 중 랜덤 위치 선택
                int posIdx = tattooRng.Next(TattooPositionPresets.Length);
                Vector2 pos = TattooPositionPresets[posIdx];
                tattooOverlay.rect = new Rect(pos.x, pos.y, TattooFixedSize, TattooFixedSize);

                // body 슬롯에 추가
                foreach (var slot in umaData.umaRecipe.slotDataList)
                {
                    if (slot == null) continue;
                    if (slot.slotName.Contains("Body") || slot.slotName.Contains("Torso") || slot.slotName.Contains("body"))
                    {
                        slot.AddOverlay(tattooOverlay);
                        Debug.Log($"[UMAFighterGenerator] 문신 적용: {fighter.FullName} → 문신#{idx} ({slot.slotName})");
                        break;
                    }
                }

                umaData.Dirty(true, true, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UMAFighterGenerator] 문신 적용 실패: {e.Message}");
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

        // 모든 DNA 파라미터 이름 (표준 UMA HumanMale 48개)
        private static readonly string[] AllDnaNames = {
            "height", "belly", "waist", "gluteusSize", "legSeparation", "legsSize", "breastSize",
            "upperMuscle", "lowerMuscle", "upperWeight", "lowerWeight", "bodyFitness",
            "headSize", "headWidth", "neckThickness",
            "noseSize", "noseCurve", "noseWidth", "noseInclination", "nosePosition", "nosePronounced", "noseFlatten",
            "jawsSize", "jawsPosition", "chinSize", "chinPronounced", "chinPosition", "mandibleSize",
            "cheekSize", "cheekPosition", "lowCheekPronounced", "lowCheekPosition",
            "foreheadSize", "foreheadPosition",
            "lipsSize", "mouthSize",
            "eyeSize", "eyeRotation", "eyeSpacing",
            "earsSize", "earsPosition", "earsRotation",
            "armLength", "forearmLength", "armWidth", "forearmWidth",
            "handsSize", "feetSize",
        };

        /// <summary>
        /// 모든 DNA를 동일한 값으로 설정한 테스트 캐릭터 생성
        /// </summary>
        public GameObject GenerateUniformDnaCharacter(float dnaValue, Vector3 position, Quaternion rotation)
        {
            if (umaAvatarPrefab == null)
            {
                Debug.LogError("[UMAFighterGenerator] UMA Avatar prefab is null!");
                return null;
            }

            var go = Instantiate(umaAvatarPrefab, position, rotation);
            go.name = $"UMA_Test_DNA_{dnaValue:F1}";

            var avatar = go.GetComponent<DynamicCharacterAvatar>();
            if (avatar == null)
            {
                Destroy(go);
                return null;
            }

            avatar.ChangeRace("HumanMale");

            if (fighterAnimatorController != null)
                avatar.animationController = fighterAnimatorController;

            // 모든 DNA를 동일한 값으로
            float clamped = Mathf.Clamp01(dnaValue);
            foreach (var dnaName in AllDnaNames)
            {
                avatar.predefinedDNA.AddDNA(dnaName, clamped);
            }

            avatar.CharacterCreated.AddListener((umaData) =>
            {
                var animator = go.GetComponent<Animator>();
                if (animator != null) animator.applyRootMotion = false;
                Debug.Log($"[UMAFighterGenerator] 테스트 캐릭터 생성: DNA={dnaValue:F1}");
            });

            avatar.BuildCharacter(true);
            return go;
        }
    }
}
