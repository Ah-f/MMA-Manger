using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMAManager.Models;
using MMAManager.Systems;
using MMAManager.UI;
using MMAManager.Visual;

namespace MMAManager.Scenes
{
    /// <summary>
    /// 메인 게임 씬 - 모든 매치/경영 진행을 관리
    /// </summary>
    public class MainScene : MonoBehaviour
    {
        public static MainScene Instance { get; private set; }

        [Header("Scene Setup")]
        [SerializeField] private Material octagonFloorMaterial;
        [SerializeField] private Material octagonFenceMaterial;
        [SerializeField] private Material octagonPostMaterial;

        [Header("Camera")]
        [SerializeField] private Transform mainCamera;
        [SerializeField] private GameObject cameraRig;

        [Header("Lighting")]
        [SerializeField] private Light mainLight;
        [SerializeField] private Light fillLight;
        [SerializeField] private Light rimLight;

        [Header("Fighters")]
        [SerializeField] private Transform fighter1SpawnPoint;
        [SerializeField] private Transform fighter2SpawnPoint;

        [Header("UI")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject eventSystemPrefab;
        [SerializeField] private GameObject hudPrefab;

        private GameObject currentFighter1;
        private GameObject currentFighter2;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("=== MainScene Initialize ===");

            // 시스템 연결 확인
            ConnectSystems();

            // 옥타곤 생성
            CreateOctagon();

            // 3D 모델 로드
            LoadFighterModels();

            // UI 생성
            CreateUI();

            // 카메라 설정
            SetupCamera();

            // 라이팅 설정
            SetupLighting();

            Debug.Log("=== MainScene Setup Complete ===");
        }

        private void ConnectSystems()
        {
            // 시스템 연결은 MainGameManager에서 처리
            Debug.Log("[MainScene] 시스템 연결 완료");
        }

        private void CreateOctagon()
        {
            GameObject octagon = new GameObject("Octagon");
            octagon.transform.position = Vector3.zero;
            octagon.tag = "Octagon";

            // 바닥 생성
            CreateOctagonFloor(octagon.transform);

            // 8개의 포스트
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 postPos = new Vector3(Mathf.Cos(angle) * 5f, 0.5f, Mathf.Sin(angle) * 5f);

                // 포스트 생성
                GameObject post = CreateOctagonPost(octagon.transform, postPos, i);

                // 이웃 포스트는 더 높이
                if (i == 0 || i == 4)
                {
                    SetPostGlow(post, true);
                }
            }

            // 펜스 연결
            CreateOctagonFence(octagon.transform);
        }

        private GameObject CreateOctagonFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "OctagonFloor";
            floor.transform.SetParent(parent);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(10f, 0.1f, 10f);
            floor.transform.rotation = Quaternion.Euler(90, 0, 0); // 수평으로 회전

            // 재질 설정
            MeshRenderer renderer = floor.GetComponent<MeshRenderer>();
            if (octagonFloorMaterial != null && renderer != null)
            {
                renderer.material = octagonFloorMaterial;
            }

            return floor;
        }

        private GameObject CreateOctagonPost(Transform parent, Vector3 position, int index)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = $"OctagonPost_{index}";
            post.transform.SetParent(parent);
            post.transform.position = position;
            post.transform.localScale = new Vector3(0.15f, 3f, 0.15f); // 반지름 8cm, 높이 3m

            // 재질 설정
            MeshRenderer renderer = post.GetComponent<MeshRenderer>();
            if (octagonPostMaterial != null && renderer != null)
            {
                renderer.material = octagonPostMaterial;
            }

            // 라이팅 효과
            if (octagonFloorMaterial != null)
            {
                // 포스트에 그림자 추가
                Light postLight = post.AddComponent<Light>();
                postLight.type = LightType.Point;
                postLight.intensity = 0.8f;
                postLight.range = 3f;
                postLight.color = new Color(1f, 0.8f, 0.3f); // 따뜻한 빨간색
            }

            return post;
        }

        private void SetPostGlow(GameObject post, bool isChallengerPost)
        {
            Light[] lights = post.GetComponentsInChildren<Light>();

            foreach (Light light in lights)
            {
                // 챔피언십 포스트는 더 밝게
                float intensity = isChallengerPost ? 1.2f : 0.8f;
                light.intensity = intensity;
            }
        }

        private void CreateOctagonFence(Transform parent)
        {
            GameObject fence = new GameObject("OctagonFence");
            fence.transform.SetParent(parent);

            // 펜스 생성 (8개)
            for (int i = 0; i < 8; i++)
            {
                float startAngle = i * 45f * Mathf.Deg2Rad;
                float endAngle = (i + 1) * 45f * Mathf.Deg2Rad;

                CreateFenceSegment(parent, startAngle, endAngle, i);
            }

            // 라이팅 설정
            MeshFilter fenceFilter = fence.GetComponent<MeshFilter>();
            if (octagonFenceMaterial != null && fenceFilter != null)
            {
                fenceFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder");
            }

            MeshRenderer fenceRenderer = fence.GetComponent<MeshRenderer>();
            if (octagonFenceMaterial != null && fenceRenderer != null)
            {
                fenceRenderer.material = octagonFenceMaterial;
            }
        }

        private void CreateFenceSegment(Transform parent, float startAngle, float endAngle, int index)
        {
            float midAngle = (startAngle + endAngle) / 2f;

            GameObject segment = new GameObject($"Fence_{index}");
            segment.transform.SetParent(parent);

            // 위치 계산 (반지름 5m)
            float x = Mathf.Cos(midAngle) * 5f;
            float z = Mathf.Sin(midAngle) * 5f;

            segment.transform.position = new Vector3(x, 0, z);

            // 회전 (각도를 유지하며 세로)
            Vector3 lookAt = Vector3.zero - segment.transform.position;
            segment.transform.rotation = Quaternion.LookRotation(lookAt);

            // 크기 조정
            segment.transform.localScale = new Vector3(0.05f, 1f, 0.05f);
        }

        private void LoadFighterModels()
        {
            // Fighter 1
            if (fighter1SpawnPoint != null)
            {
                currentFighter1 = CreateFighterAtPoint(fighter1SpawnPoint, 1);
            }

            // Fighter 2
            if (fighter2SpawnPoint != null)
            {
                currentFighter2 = CreateFighterAtPoint(fighter2SpawnPoint, 2);
            }
        }

        private GameObject CreateFighterAtPoint(Transform spawnPoint, int fighterIndex)
        {
            Fighter3DCreator creator = FindObjectOfType<Fighter3DCreator>();
            if (creator == null)
            {
                GameObject creatorObj = new GameObject("Fighter3DCreator");
                creator = creatorObj.AddComponent<Fighter3DCreator>();
            }

            Fighter fighter = new Fighter("Fighter", $"F{fighterIndex}", 30, WeightClass.Lightweight);
            GameObject fighterObj = creator.CreateFighter3D(fighter, spawnPoint.position, fighterIndex == 1);
            fighterObj.transform.rotation = Quaternion.Euler(0, fighterIndex == 1 ? 180 : 0, 0);

            return fighterObj;
        }

        private void CreateUI()
        {
            if (mainCanvas == null)
            {
                CreateMainCanvas();
            }

            // EventSystem 생성
            if (eventSystemPrefab != null)
            {
                GameObject eventSystemObj = Instantiate(eventSystemPrefab, mainCanvas.transform);
                eventSystemObj.name = "EventSystem";
            }
            else
            {
                // 이미 존재하면 재사용
                GameObject.Find("EventSystem");
            }
        }

        private void CreateMainCanvas()
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            // Canvas Scaler (해상도에 따른 UI 스케일)
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // EventSystem 추가
            EventSystem eventSystem = canvasObj.AddComponent<EventSystem>();

            // UI 레이어 생성
            CreateHUD();
        }

        private void CreateHUD()
        {
            if (hudPrefab != null)
            {
                GameObject hudObj = Instantiate(hudPrefab, mainCanvas.transform);
                hudObj.name = "FightHUD";
            }
        }

        private void SetupCamera()
        {
            if (mainCamera == null)
            {
                // 카메라 리그 생성
                GameObject cameraObj = new GameObject("MainCamera");
                mainCamera = cameraObj.transform;

                Camera camera = cameraObj.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.05f, 0.1f, 0.15f);
                camera.cullingMask = LayerMask.GetMask("Default", "UI", "Fighters");
            }
        }

        private void SetupLighting()
        {
            // 메인 라이트 (스�포트라이트)
            if (mainLight == null)
            {
                GameObject lightObj = new GameObject("MainLight");
                mainLight = lightObj.AddComponent<Light>();
                mainLight.type = LightType.Spot;
                mainLight.shadows = LightShadows.Soft;
                mainLight.intensity = 1.2f;
                mainLight.spotAngle = 45f;
                mainLight.range = 30f;
                mainLight.color = new Color(1f, 0.95f, 0.8f);
            }

            // 필 라이트 (전체적으로 조명)
            if (fillLight == null)
            {
                GameObject fillObj = new GameObject("FillLight");
                fillLight = fillObj.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.intensity = 0.5f;
                fillLight.range = 20f;
                fillLight.color = new Color(0.8f, 0.9f, 1f);
            }

            // 림 라이트 (육골을 강조)
            if (rimLight == null)
            {
                GameObject rimObj = new GameObject("RimLight");
                rimLight = rimObj.AddComponent<Light>();
                rimLight.type = LightType.Point;
                rimLight.intensity = 0.3f;
                rimLight.range = 15f;
                rimLight.color = new Color(1f, 0.7f, 0.5f);
            }
        }
    }

    /// <summary>
    /// HUD - 체력바, 경영 정보 표시
    /// </summary>
    public class FightHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text fighter1Name;
        [SerializeField] private Text fighter1Health;
        [SerializeField] private Text fighter1Stamina;
        [SerializeField] private Text fighter2Name;
        [SerializeField] private Text fighter2Health;
        [SerializeField] private Text fighter2Stamina;
        [SerializeField] private Text roundInfo;
        [SerializeField] private Text timeDisplay;

        [Header("Colors")]
        [SerializeField] private Color healthHighColor = Color.red;
        [SerializeField] private Color healthMediumColor = Color.yellow;
        [SerializeField] private Color healthLowColor = new Color(1f, 0.7f, 0.3f);
        [SerializeField] private Color staminaFullColor = new Color(0.3f, 0.8f, 0.3f);

        private Fighter displayedFighter1;
        private Fighter displayedFighter2;

        private float healthUpdateInterval = 0.3f;

        void Start()
        {
            Debug.Log("=== FightHUD Initialize ===");
            FindFightersInScene();
        }

        private void FindFightersInScene()
        {
            // 옥타곤 내의 파이터를 찾음
            Visual.Fighter3DCreator[] fighterCreators = GameObject.FindObjectsOfType<Visual.Fighter3DCreator>();

            if (fighterCreators != null && fighterCreators.Length > 0)
            {
                // 각 Visual.Fighter3DCreator의 CreateFighter3D로 생성된 파이터 사용
                foreach (Visual.Fighter3DCreator creator in fighterCreators)
                {
                    FighterVisualInfo[] fighters = creator.GetComponentsInChildren<FighterVisualInfo>();

                    foreach (FighterVisualInfo fighter in fighters)
                    {
                        if (fighter.Fighter != null)
                        {
                            // 첫 번째 발견된 파이터를 Fighter1으로 설정
                            if (displayedFighter1 == null)
                            {
                                displayedFighter1 = fighter.Fighter;
                                UpdateFighter1Display(fighter.Fighter);
                            }

                            // 두 번째 발견된 파이터를 Fighter2으로 설정
                            if (displayedFighter2 == null)
                            {
                                displayedFighter2 = fighter.Fighter;
                                UpdateFighter2Display(fighter.Fighter);
                            }
                        }
                    }
                }

                // Creator 제거 (선택적 - 완료 후 정리)
                if (displayedFighter1 != null && displayedFighter2 != null)
                {
                    // 디버깅용도로 남겨있는 8개 포스트만 남기고 파이터 제거
                    // 실게임에서는 FighterVisualInfo만 참조하면 됨
                }
            }
        }

        private void LogFoundFighters(int creatorCount, int fighterCount)
        {
            Debug.Log($"[FightHUD] {creatorCount}개의 Visual.Fighter3DCreator 발견, 총 {fighterCount}명의 파이터 발견");
        }

        private int CountFighters()
        {
            int count = 0;
            if (displayedFighter1 != null) count++;
            if (displayedFighter2 != null) count++;
            return count;
        }

        private void UpdateFighter1Display(Fighter fighter)
        {
            displayedFighter1 = fighter;
            fighter1Name.text = fighter.DisplayName;
            UpdateHealthBar(fighter1Health, fighter);
        }

        private void UpdateFighter2Display(Fighter fighter)
        {
            displayedFighter2 = fighter;
            fighter2Name.text = fighter.DisplayName;
            UpdateHealthBar(fighter2Health, fighter);
        }

        private void UpdateHealthBar(Text healthText, Fighter fighter)
        {
            if (fighter == null) return;

            float healthPercent = fighter.Condition / 100f;

            healthText.text = $"HP: {Mathf.RoundToInt(healthPercent)}%";

            // 색상 변경
            if (healthPercent >= 70)
            {
                healthText.color = healthHighColor;
            }
            else if (healthPercent >= 40)
            {
                healthText.color = healthMediumColor;
            }
            else
            {
                healthText.color = healthLowColor;
            }
        }

        void Update()
        {
            // 주기 HUD 업데이트 (0.3초마다)
            healthUpdateInterval += Time.deltaTime;

            if (healthUpdateInterval >= 0.3f)
            {
                healthUpdateInterval = 0f;

                // 체력바 업데이트
                if (displayedFighter1 != null)
                {
                    UpdateHealthBar(fighter1Health, displayedFighter1);
                }

                // 스태미나 바 업데이트
                if (displayedFighter2 != null)
                {
                    UpdateHealthBar(fighter2Health, displayedFighter2);
                }
            }
        }
    }
}
