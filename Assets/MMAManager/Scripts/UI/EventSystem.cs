using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MMAManager.UI
{
    /// <summary>
    /// 게임 전체 이벤트를 관리하는 시스템
    /// 모든 UI 상호작용은 여기서 중앙
    /// </summary>
    public class EventSystem : MonoBehaviour
    {
        public static EventSystem Instance { get; private set; }

        [Header("Events")]
        [SerializeField] private float clickSoundVolume = 0.5f;
        [SerializeField] private float uiAnimationSpeed = 2f;

        private AudioSource audioSource;

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

        public void Initialize()
        {
            // Audio 소스 초기화
            audioSource = gameObject.AddComponent<AudioSource>();

            Debug.Log("[EventSystem] 이벤트 시스템 초기화 완료");
        }

        /// <summary>
        /// UI 이벤트 실행 (PlaySound, ShowNotification 등)
        /// </summary>
        public void PlayClickSound()
        {
            // Play sound는 사전에 준비된 AudioClip을 사용
            // 여기선 간단히 로그만 출력
            Debug.Log("[EventSystem] Click sound played");
        }

        public void ShowNotification(string title, string message, float duration = 3f)
        {
            Debug.Log($"[EventSystem] 알림: {title} - {message}");

            // 실제에서는 알림창을 생성
            // 향후 UI 프레임워크에서 구현 가능

            // 임시로 UI 생성
            GameObject notification = new GameObject("NotificationPanel");
            DontDestroyOnLoad(notification);

            // Canvas
            Canvas canvas = notification.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            // 배경
            GameObject background = new GameObject("Background");
            background.transform.SetParent(notification.transform);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(400, 250);

            // 제목 텍스트
            GameObject titleTextObj = new GameObject("Title");
            titleTextObj.transform.SetParent(background.transform);

            Text titleText = titleTextObj.AddComponent<Text>();
            titleText.text = title;
            titleText.fontSize = 24;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf.ArialBold.ttf");
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.UpperCenter;

            RectTransform titleRect = titleTextObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.7f);
            titleRect.anchorMax = new Vector2(0.7f, 0.9f);
            titleRect.anchoredPosition = new Vector2(0, 10);

            // 메시지 텍스트
            GameObject messageTextObj = new GameObject("Message");
            messageTextObj.transform.SetParent(background.transform);

            Text messageText = messageTextObj.AddComponent<Text>();
            messageText.text = message;
            messageText.fontSize = 18;
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf.Arial.ttf");
            messageText.color = new Color(0.9f, 0.9f, 0.9f);
            messageText.alignment = TextAnchor.UpperCenter;

            RectTransform messageRect = messageTextObj.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.3f, 0.5f);
            messageRect.anchorMax = new Vector2(0.7f, 0.6f);
            messageRect.anchoredPosition = new Vector2(0, 0);

            // 자동 파괴
            Object.Destroy(notification, duration);

            Debug.Log($"[EventSystem] 알림 표시: {title}");
        }

        public void ShowLoading(string message = "로드 중...")
        {
            ShowNotification("로드 중...", message, 0f);
        }

        public void HideLoading()
        {
            // TODO: 로딩 창 닫기
        }
    }
}
