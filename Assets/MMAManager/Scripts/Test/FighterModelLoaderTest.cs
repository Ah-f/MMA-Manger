using UnityEngine;
using MMAManager.Models;
using MMAManager.Visual;

namespace MMAManager.Test
{
    /// <summary>
    /// 외부 모델링 로더 테스트 스크립트
    /// 모델과 애니메이션 파일을 임포트 후 이 스크립트에서 설정하여 사용
    /// </summary>
    public class FighterModelLoaderTest : MonoBehaviour
    {
        [Header("Model Loader")]
        [SerializeField] private FighterModelLoader modelLoader;

        [Header("Fighter Models")]
        [Tooltip("여기에 FBX 모델을 드래그앤드롭하세요")]
        [SerializeField] private GameObject fighterModelPrefab;

        [Header("Animation Clips")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip punchClip;
        [SerializeField] private AnimationClip kickClip;
        [SerializeField] private AnimationClip takedownClip;
        [SerializeField] private AnimationClip getHitClip;
        [SerializeField] private AnimationClip knockdownClip;

        [Header("Materials")]
        [SerializeField] private Material blueFighterMaterial;
        [SerializeField] private Material redFighterMaterial;

        [Header("Test Data")]
        private Fighter fighter1;
        private Fighter fighter2;
        private GameObject fighter1Obj;
        private GameObject fighter2Obj;
        private GameObject octagon;

        void Start()
        {
            Debug.Log("=== FIGHTER MODEL LOADER TEST ===");

            // 모델 로더 초기화
            if (modelLoader == null)
            {
                GameObject loaderObj = new GameObject("ModelLoader");
                modelLoader = loaderObj.AddComponent<FighterModelLoader>();
            }

            // 모델과 애니메이션 설정
            SetupModelAndAnimations();

            // 테스트 파이터 생성
            CreateTestFighters();

            // 옥타곤 생성
            CreateOctagon();

            // 파이터 모델 생성
            CreateFighterModels();

            // 카메라 설정
            SetupCamera();

            Debug.Log("=== SETUP COMPLETE ===");
            Debug.Log("1~6: 애니메이션 테스트 | R: 재시작");
        }

        private void SetupModelAndAnimations()
        {
            if (fighterModelPrefab != null)
            {
                modelLoader.SetFighterModelPrefab(fighterModelPrefab);
                Debug.Log($"모델 설정 완료: {fighterModelPrefab.name}");
            }
            else
            {
                Debug.LogWarning("fighterModelPrefab이 없습니다. 프리미티브 모델을 사용합니다.");
            }

            // 애니메이션 클립 설정
            if (idleClip != null) modelLoader.SetAnimation(FighterAnimationType.Idle, idleClip);
            if (punchClip != null) modelLoader.SetAnimation(FighterAnimationType.Punch, punchClip);
            if (kickClip != null) modelLoader.SetAnimation(FighterAnimationType.Kick, kickClip);
            if (takedownClip != null) modelLoader.SetAnimation(FighterAnimationType.Takedown, takedownClip);
            if (getHitClip != null) modelLoader.SetAnimation(FighterAnimationType.GetHit, getHitClip);
            if (knockdownClip != null) modelLoader.SetAnimation(FighterAnimationType.Knockdown, knockdownClip);

            Debug.Log("애니메이션 설정 완료");
        }

        private void CreateTestFighters()
        {
            fighter1 = new Fighter("Korean", "Zombie", 28, WeightClass.Lightweight);
            fighter2 = new Fighter("Islam", "Makhachev", 27, WeightClass.Lightweight);

            SetFighterStats(fighter1, 85, 88, 82, 75, 70, 40, 45, 90);
            SetFighterStats(fighter2, 75, 85, 78, 80, 75, 88, 85, 88);

            Debug.Log($"파이터 생성: {fighter1.DisplayName} vs {fighter2.DisplayName}");
        }

        private void SetFighterStats(Fighter fighter, int str, int tec, int spd, int sta, int def, int wrest, int bjj, int pot)
        {
            fighter.SetStat(StatType.Strength, str);
            fighter.SetStat(StatType.Technique, tec);
            fighter.SetStat(StatType.Speed, spd);
            fighter.SetStat(StatType.Stamina, sta);
            fighter.SetStat(StatType.Defense, def);
            fighter.SetStat(StatType.Wrestling, wrest);
            fighter.SetStat(StatType.BJJ, bjj);
            fighter.SetStat(StatType.Potential, pot);
        }

        private void CreateOctagon()
        {
            Fighter3DCreator creator = gameObject.AddComponent<Fighter3DCreator>();
            octagon = creator.CreateOctagon(Vector3.zero);
        }

        private void CreateFighterModels()
        {
            fighter1Obj = modelLoader.CreateFighterFromModel(fighter1, new Vector3(-2, 0, 0), true);
            fighter1Obj.transform.rotation = Quaternion.Euler(0, 90, 0);

            fighter2Obj = modelLoader.CreateFighterFromModel(fighter2, new Vector3(2, 0, 0), false);
            fighter2Obj.transform.rotation = Quaternion.Euler(0, -90, 0);
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            mainCamera.transform.position = new Vector3(0, 2, -6);
            mainCamera.transform.rotation = Quaternion.Euler(10, 0, 0);
        }

        void Update()
        {
            // 애니메이션 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAnimation(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAnimation(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAnimation(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAnimation(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) PlayAnimation(5);
            if (Input.GetKeyDown(KeyCode.Alpha6)) PlayAnimation(6);

            if (Input.GetKeyDown(KeyCode.R)) RestartTest();
        }

        private void PlayAnimation(int animType)
        {
            if (fighter1Obj != null)
            {
                ModelAnimator animator = fighter1Obj.GetComponent<ModelAnimator>();
                if (animator != null)
                {
                    switch (animType)
                    {
                        case 1: animator.PlayPunch(); Debug.Log("Punch"); break;
                        case 2: animator.PlayKick(); Debug.Log("Kick"); break;
                        case 3: animator.PlayTakedown(); Debug.Log("Takedown"); break;
                        case 4: animator.PlayGetHit(); Debug.Log("GetHit"); break;
                        case 5: animator.PlayKnockdown(); Debug.Log("Knockdown"); break;
                        case 6: animator.PlayBlocking(); Debug.Log("Blocking"); break;
                    }
                }
            }
        }

        private void RestartTest()
        {
            if (fighter1Obj != null) Destroy(fighter1Obj);
            if (fighter2Obj != null) Destroy(fighter2Obj);
            if (octagon != null) Destroy(octagon);

            Start();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 320, 450));
            GUILayout.Box("Model Loader Test");
            GUILayout.Space(10);

            GUILayout.Label("모델 설정:");
            GUILayout.Label($"Prefab: {(fighterModelPrefab != null ? fighterModelPrefab.name : "None")}");

            GUILayout.Space(10);
            GUILayout.Label("애니메이션 클립:");
            GUILayout.Label($"Idle: {(idleClip != null ? idleClip.name : "None")}");
            GUILayout.Label($"Punch: {(punchClip != null ? punchClip.name : "None")}");
            GUILayout.Label($"Kick: {(kickClip != null ? kickClip.name : "None")}");

            GUILayout.Space(10);
            GUILayout.Label("컨트롤:");
            GUILayout.Label("1 - Punch");
            GUILayout.Label("2 - Kick");
            GUILayout.Label("3 - Takedown");
            GUILayout.Label("4 - GetHit");
            GUILayout.Label("5 - Knockdown");
            GUILayout.Label("6 - Blocking");
            GUILayout.Label("R - Restart");

            GUILayout.Space(10);
            GUILayout.Label("사용 방법:");
            GUILayout.Label("1. Assets에 FBX 모델 임포트");
            GUILayout.Label("2. Fighter Model Prefab 슬롯에 드래그");
            GUILayout.Label("3. Animation Clips 슬롯에 드래그");
            GUILayout.Label("4. Play 모드로 테스트");

            GUILayout.EndArea();
        }
    }
}
