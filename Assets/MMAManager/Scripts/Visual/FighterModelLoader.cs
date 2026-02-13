using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Visual
{
    /// <summary>
    /// 외부 모델링을 로드하여 파이터를 생성하는 시스템
    /// FBX, glTF 등 외부 3D 모델과 애니메이션을 지원
    /// </summary>
    public class FighterModelLoader : MonoBehaviour
    {
        [Header("Model References")]
        [Tooltip("파이터 3D 모델 프리팹 (FBX 등)")]
        [SerializeField] private GameObject fighterModelPrefab;

        [Header("Animation Clips")]
        [SerializeField] private AnimationClip idleAnimation;
        [SerializeField] private AnimationClip punchAnimation;
        [SerializeField] private AnimationClip kickAnimation;
        [SerializeField] private AnimationClip takedownAnimation;
        [SerializeField] private AnimationClip getHitAnimation;
        [SerializeField] private AnimationClip knockdownAnimation;
        [SerializeField] private AnimationClip blockingAnimation;
        [SerializeField] private AnimationClip walkAnimation;
        [SerializeField] private AnimationClip groundStrikingAnimation;
        [SerializeField] private AnimationClip submissionAttemptAnimation;
        [SerializeField] private AnimationClip groundGetUpAnimation;

        [Header("Materials")]
        [SerializeField] private Material fighter1Material;
        [SerializeField] private Material fighter2Material;
        [SerializeField] private Material skinMaterial;

        [Header("Settings")]
        [SerializeField] private bool useExternalModels = true;
        [SerializeField] private float modelScale = 1f;

        /// <summary>
        /// 모델을 로드하여 파이터를 생성합니다
        /// </summary>
        public GameObject CreateFighterFromModel(Fighter fighter, Vector3 position, bool isFighter1)
        {
            GameObject fighterObj;

            if (useExternalModels && fighterModelPrefab != null)
            {
                // 외부 모델 사용
                fighterObj = Instantiate(fighterModelPrefab, position, Quaternion.identity);
                fighterObj.name = $"Fighter_{SanitizeName(fighter.DisplayName)}";

                // 재질 적용
                ApplyFighterMaterials(fighterObj, isFighter1);

                // 애니메이션 설정
                SetupAnimations(fighterObj);
            }
            else
            {
                // 프리미티브 모델 (폴백)
                Debug.LogWarning($"[FighterModelLoader] No external model found, using primitive fallback for {fighter.DisplayName}");
                Fighter3DCreator primitiveCreator = gameObject.AddComponent<Fighter3DCreator>();
                fighterObj = primitiveCreator.CreateFighter3D(fighter, position, isFighter1);
            }

            // FighterVisualInfo 추가
            FighterVisualInfo visualInfo = fighterObj.GetComponent<FighterVisualInfo>();
            if (visualInfo == null)
            {
                visualInfo = fighterObj.AddComponent<FighterVisualInfo>();
            }
            visualInfo.Initialize(fighter);

            return fighterObj;
        }

        /// <summary>
        /// 특정 경로의 모델을 로드합니다
        /// </summary>
        public GameObject LoadModelFromPath(string modelPath, Vector3 position)
        {
            GameObject model = Resources.Load<GameObject>(modelPath);

            if (model != null)
            {
                GameObject instance = Instantiate(model, position, Quaternion.identity);
                return instance;
            }
            else
            {
                Debug.LogError($"[FighterModelLoader] Failed to load model from path: {modelPath}");
                return null;
            }
        }

        /// <summary>
        /// 에셋 번들에서 모델을 로드합니다
        /// </summary>
        public void LoadFromAssetBundle(string bundlePath, string assetName, Vector3 position, System.Action<GameObject> onComplete)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle != null)
            {
                AssetBundleRequest request = bundle.LoadAssetAsync<GameObject>(assetName);

                request.completed += (operation) =>
                {
                    GameObject model = Instantiate(request.asset as GameObject, position, Quaternion.identity);
                    onComplete?.Invoke(model);
                    bundle.Unload(false);
                };
            }
            else
            {
                Debug.LogError($"[FighterModelLoader] Failed to load AssetBundle: {bundlePath}");
                onComplete?.Invoke(null);
            }
        }

        private void ApplyFighterMaterials(GameObject fighterObj, bool isFighter1)
        {
            SkinnedMeshRenderer[] renderers = fighterObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            MeshRenderer[] staticRenderers = fighterObj.GetComponentsInChildren<MeshRenderer>();

            Material materialToApply = isFighter1 ? fighter1Material : fighter2Material;

            // SkinnedMeshRenderer에 재질 적용
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer != null && materialToApply != null)
                {
                    // 쉐이더를 유지하면서 텍스처만 교체하거나, 전체 재질 교체
                    Material[] newMaterials = new Material[renderer.materials.Length];

                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        // 스킨 메테리얼은 별도 처리
                        if (renderer.materials[i].name.ToLower().Contains("skin") && skinMaterial != null)
                        {
                            newMaterials[i] = skinMaterial;
                        }
                        else
                        {
                            newMaterials[i] = materialToApply;
                        }
                    }

                    renderer.materials = newMaterials;
                }
            }

            // 일반 MeshRenderer에 재질 적용
            foreach (MeshRenderer renderer in staticRenderers)
            {
                if (renderer != null && materialToApply != null)
                {
                    Material[] newMaterials = new Material[renderer.materials.Length];

                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        if (renderer.materials[i].name.ToLower().Contains("skin") && skinMaterial != null)
                        {
                            newMaterials[i] = skinMaterial;
                        }
                        else
                        {
                            newMaterials[i] = materialToApply;
                        }
                    }

                    renderer.materials = newMaterials;
                }
            }
        }

        private void SetupAnimations(GameObject fighterObj)
        {
            Animator animator = fighterObj.GetComponent<Animator>();

            if (animator == null)
            {
                animator = fighterObj.AddComponent<Animator>();
            }

            // Note: RuntimeAnimatorController는 인스펙터에서 수동 설정 필요
            // 또는 Resources 폴더에 프리팹으로 저장된 컨트롤러 사용

            // ModelAnimator 컴포넌트 추가/설정
            ModelAnimator modelAnimator = fighterObj.GetComponent<ModelAnimator>();
            if (modelAnimator == null)
            {
                modelAnimator = fighterObj.AddComponent<ModelAnimator>();
            }
            modelAnimator.Initialize(animator);
        }

        private string SanitizeName(string name)
        {
            return name.Replace(" ", "_").Replace("\"", "").Replace("/", "_");
        }

        /// <summary>
        /// 모델 프리팹을 설정합니다
        /// </summary>
        public void SetFighterModelPrefab(GameObject prefab)
        {
            fighterModelPrefab = prefab;
            useExternalModels = true;
        }

        /// <summary>
        /// 애니메이션 클립을 설정합니다
        /// </summary>
        public void SetAnimation(FighterAnimationType type, AnimationClip clip)
        {
            switch (type)
            {
                case FighterAnimationType.Idle:
                    idleAnimation = clip;
                    break;
                case FighterAnimationType.Punch:
                    punchAnimation = clip;
                    break;
                case FighterAnimationType.Kick:
                    kickAnimation = clip;
                    break;
                case FighterAnimationType.Takedown:
                    takedownAnimation = clip;
                    break;
                case FighterAnimationType.GetHit:
                    getHitAnimation = clip;
                    break;
                case FighterAnimationType.Knockdown:
                    knockdownAnimation = clip;
                    break;
                case FighterAnimationType.Blocking:
                    blockingAnimation = clip;
                    break;
                case FighterAnimationType.Walk:
                    walkAnimation = clip;
                    break;
                case FighterAnimationType.GroundStriking:
                    groundStrikingAnimation = clip;
                    break;
                case FighterAnimationType.SubmissionAttempt:
                    submissionAttemptAnimation = clip;
                    break;
                case FighterAnimationType.GroundGetUp:
                    groundGetUpAnimation = clip;
                    break;
            }
        }
    }

    /// <summary>
    /// 외부 모델용 애니메이터 - 애니메이션 이름으로 직접 재생
    /// </summary>
    public class ModelAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [Header("Animation Names")]
        [SerializeField] private string idleAnimName = "Idle";
        [SerializeField] private string punchAnimName = "Jab Cross";
        [SerializeField] private string kickAnimName = "Mma Kick";
        [SerializeField] private string takedownAnimName = "Double Leg Takedown - Attacker";
        [SerializeField] private string getHitAnimName = "Head Hit";
        [SerializeField] private string knockdownAnimName = "Defeated";
        [SerializeField] private string blockingAnimName = "Center Block";

        public void Initialize(Animator anim)
        {
            animator = anim;
        }

        public void PlayIdle()
        {
            PlayAnimationByName(idleAnimName);
        }

        public void PlayPunch()
        {
            PlayAnimationByName(punchAnimName);
        }

        public void PlayKick()
        {
            PlayAnimationByName(kickAnimName);
        }

        public void PlayTakedown()
        {
            PlayAnimationByName(takedownAnimName);
        }

        public void PlayGetHit()
        {
            PlayAnimationByName(getHitAnimName);
        }

        public void PlayKnockdown()
        {
            PlayAnimationByName(knockdownAnimName);
        }

        public void PlayBlocking()
        {
            PlayAnimationByName(blockingAnimName);
        }

        public void PlayAnimationByName(string animName)
        {
            if (animator != null && !string.IsNullOrEmpty(animName))
            {
                animator.Play(animName, 0, 0f);
            }
        }

        public void PlayAnimationByIndex(int index)
        {
            string[] animNames = new string[]
            {
                idleAnimName,
                punchAnimName,
                kickAnimName,
                takedownAnimName,
                getHitAnimName,
                knockdownAnimName,
                blockingAnimName
            };

            if (index >= 0 && index < animNames.Length)
            {
                PlayAnimationByName(animNames[index]);
            }
        }
    }

    /// <summary>
    /// 지원하는 애니메이션 타입
    /// </summary>
    public enum FighterAnimationType
    {
        Idle,
        Punch,
        Kick,
        Takedown,
        GetHit,
        Knockdown,
        Blocking,
        Walk,
        GroundStriking,
        SubmissionAttempt,
        GroundGetUp
    }
}
