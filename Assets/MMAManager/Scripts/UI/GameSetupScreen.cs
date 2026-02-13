using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MMAManager.Models;
using MMAManager.Systems;

namespace MMAManager.UI
{
    /// <summary>
    /// 게임 시작 화면 - 초반 설정, 신규 영입, 체육관 선택
    /// </summary>
    public class GameSetupScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject inputBlocker;

        [Header("Scene Setup")]
        [SerializeField] private GameObject gymScenePrefab;
        [SerializeField] private GameObject characterCreatePrefab;

        private int selectedWeightClass = 4; // Default: Lightweight

        void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Debug.Log("=== Game Setup Screen Initialize ===");

            // 체육관 생성
            if (inputBlocker == null)
            {
                CreateInputBlocker();
            }

            // 씬 로드 확인 (새 씬인지 Resources 폴더에 있는지 확인)
            LoadOrCreateCareer();

            Debug.Log("=== Setup Complete ===");
        }

        private void CreateInputBlocker()
        {
            GameObject blockerObj = new GameObject("InputBlocker");
            DontDestroyOnLoad(blockerObj);

            // EventSystem 추가
            UnityEngine.EventSystems.EventSystem eventSystem = blockerObj.AddComponent<UnityEngine.EventSystems.EventSystem>();

            // UI 레이어 생성
            GameObject uiPanel = CreateMainUIPanel();
            uiPanel.transform.SetParent(mainCanvas.transform, false);
        }

        private GameObject CreateMainUIPanel()
        {
            GameObject panel = new GameObject("GameSetupPanel");
            panel.transform.SetParent(mainCanvas.transform, false);
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 배경
            GameObject background = CreateBackgroundImage(panel.transform);
            background.AddComponent<CanvasGroup>();

            // 제목
            CreateTitle(panel.transform);

            // 체급 선택기
            CreateWeightClassSelector(panel.transform);

            // 체육관 생성 버튼
            CreateGymButton(panel.transform);

            // 신규 영입 버튼
            CreateCharacterCreateButton(panel.transform);

            // 시작 버튼
            CreateStartButton(panel.transform);

            return panel;
        }

        private GameObject CreateBackgroundImage(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent);
            Image image = bg.AddComponent<Image>();
            image.raycastTarget = false;

            // 간단한 색상 - 직접 설정
            image.color = new Color(0.1f, 0.1f, 0.15f);

            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.sizeDelta = new Vector2(1, 1);

            return bg;
        }

        private GameObject CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "MMA MANAGER";
            titleText.fontSize = 48;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf.ArialBold.ttf");
            titleText.alignment = TextAnchor.UpperCenter;

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, 10);
            titleRect.sizeDelta = new Vector2(0.5f, -0.1f);

            return titleObj;
        }

        private void CreateWeightClassSelector(Transform parent)
        {
            GameObject selectorObj = new GameObject("WeightClassSelector");
            selectorObj.transform.SetParent(parent);

            // 현재 선택 표시
            GameObject currentLabel = CreateLabel(selectorObj.transform, "현재: Lightweight", new Vector2(-180, 80));

            // 선택 버튼들
            string[] classes = System.Enum.GetNames(typeof(WeightClass));
            float buttonWidth = 140f;
            float startX = -210f;

            for (int i = 0; i < classes.Length; i++)
            {
                string className = classes[i];
                bool isSelected = (i == selectedWeightClass);

                GameObject buttonObj = CreateClassButton(
                    selectorObj.transform,
                    className,
                    isSelected,
                    new Vector2(startX + (i * buttonWidth), 50)
                );

                // 버튼에 이벤트 추가
                ButtonEvents events = buttonObj.GetComponent<ButtonEvents>();
                events.onClick.AddListener(() =>
                {
                    selectedWeightClass = i;
                    UpdateWeightClassDisplay();
                    PlayClickSound();
                });
            }
        }

        private GameObject CreateClassButton(Transform parent, string className, bool isSelected, Vector2 position)
        {
            GameObject buttonObj = new GameObject($"WeightClass_{className}");
            buttonObj.transform.SetParent(parent);
            buttonObj.transform.localPosition = new Vector3(position.x, position.y, 0);

            // 이미지
            Image buttonImage = buttonObj.AddComponent<Image>();
            // 간단한 색상 (진: 초록, 선택: 노랑)
            Color buttonColor = isSelected ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            buttonImage.color = buttonColor;

            // 테두리
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(130, 40);

            // Button 이벤트 컴포넌트 추가
            ButtonEvents events = buttonObj.AddComponent<ButtonEvents>();
            if (events.onClick == null)
                events.onClick = new UnityEngine.Events.UnityEvent();

            return buttonObj;
        }

        private GameObject CreateLabel(Transform parent, string text, Vector2 position)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(parent);
            labelObj.transform.localPosition = new Vector3(position.x, position.y, 0);

            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = text;
            labelText.fontSize = 18;
            labelText.alignment = TextAnchor.MiddleLeft;

            return labelObj;
        }

        private GameObject CreateGymButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("GymButton");
            buttonObj.transform.SetParent(parent);
            buttonObj.transform.localPosition = new Vector3(-250, -80, 0);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.9f);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(120, 45);

            // 텍스트
            Text buttonText = buttonObj.AddComponent<Text>();
            buttonText.text = "체육관";
            buttonText.fontSize = 20;
            buttonText.alignment = TextAnchor.MiddleCenter;

            // Button 이벤트 컴포넌트 추가
            ButtonEvents events = buttonObj.AddComponent<ButtonEvents>();
            if (events.onClick == null)
                events.onClick = new UnityEngine.Events.UnityEvent();

            return buttonObj;
        }

        private GameObject CreateCharacterCreateButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("CharacterCreateButton");
            buttonObj.transform.SetParent(parent);
            buttonObj.transform.localPosition = new Vector3(250, -80, 0);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.9f, 0.5f, 0.2f);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(120, 45);

            // 텍스트
            Text buttonText = buttonObj.AddComponent<Text>();
            buttonText.text = "신규 영입";
            buttonText.fontSize = 20;
            buttonText.alignment = TextAnchor.MiddleCenter;

            // Button 이벤트 컴포넌트 추가
            ButtonEvents events = buttonObj.AddComponent<ButtonEvents>();
            if (events.onClick == null)
                events.onClick = new UnityEngine.Events.UnityEvent();

            return buttonObj;
        }

        private GameObject CreateStartButton(Transform parent)
        {
            GameObject buttonObj = new GameObject("StartButton");
            buttonObj.transform.SetParent(parent);
            buttonObj.transform.localPosition = new Vector3(0, -150, 0);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.1f, 0.8f, 0.3f);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 60);

            // 텍스트
            Text buttonText = buttonObj.AddComponent<Text>();
            buttonText.text = "게임 시작";
            buttonText.fontSize = 24;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.color = Color.white;

            // Button 이벤트 컴포넌트 추가
            ButtonEvents events = buttonObj.AddComponent<ButtonEvents>();
            if (events.onClick == null)
                events.onClick = new UnityEngine.Events.UnityEvent();

            return buttonObj;
        }

        private void UpdateWeightClassDisplay()
        {
            // 현재 선택 표시 업데이트
            Transform currentLabel = inputBlocker.transform.Find("WeightClassSelector/Label");
            if (currentLabel != null)
            {
                Text labelText = currentLabel.GetComponent<Text>();
                WeightClass selectedClass = (WeightClass)selectedWeightClass;
                labelText.text = $"현재: {GetKoreanWeightClass(selectedClass)}";
            }
        }

        private string GetKoreanWeightClass(WeightClass weightClass)
        {
            return weightClass switch
            {
                WeightClass.Atomweight => "아톰급 (105lbs)",
                WeightClass.Strawweight => "스트로급 (115lbs)",
                WeightClass.Flyweight => "플라이급 (125lbs)",
                WeightClass.Bantamweight => "밴텀급 (135lbs)",
                WeightClass.Featherweight => "페더급 (145lbs)",
                WeightClass.Lightweight => "라급 (155lbs)",
                WeightClass.Welterweight => "웰터급 (170lbs)",
                WeightClass.Middleweight => "미들급 (185lbs)",
                WeightClass.LightHeavyweight => "라이트헤비급 (205lbs)",
                WeightClass.Cruiserweight => "크루저급 (225lbs)",
                WeightClass.Heavyweight => "헤비급 (265lbs)",
                _ => "알 수 없음"
            };
        }

        private void LoadOrCreateCareer()
        {
            // Resources 폴더에서 Career 데이터 로드 시도
            PlayerCareer career = Resources.Load<PlayerCareer>("DefaultCareer");

            if (career == null)
            {
                Debug.LogWarning("[GameSetupScreen] 기존 경영 데이터를 찾을 수 없습니다. 새로운 경영을 생성합니다.");
                career = ScriptableObject.CreateInstance<PlayerCareer>();
                career.careerId = "DefaultCareer";
            }

            Debug.Log($"[GameSetupScreen] 경영 데이터 로드 완료: {career.careerId}");
        }

        private void PlayClickSound()
        {
            // 간단한 클릭 사운드 효과
            // 실제에서는 AudioClip을 사용하겠지만, 여기선 간단히 구현
            Debug.Log("Click!");
        }
    }

    /// <summary>
    /// UI 이벤트 시스템
    /// </summary>
    public class ButtonEvents : MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent onClick;
    }
}
