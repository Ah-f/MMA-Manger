using UnityEngine;
using MMAManager.Models;
using MMAManager.Systems;
using MMAManager.UI;

namespace MMAManager
{
    /// <summary>
    /// MMA Manager 메인 게임 매니저 - 모든 시스템을 통합 관리
    /// </summary>
    public class MainGameManager : MonoBehaviour
    {
        public static MainGameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private CareerSystem careerSystem;
        [SerializeField] private EconomySystem economySystem;
        [SerializeField] private MatchSimulationSystem matchSimulation;
        [SerializeField] private TrainingSystem trainingSystem;

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.CareerMode;

        [Header("Player")]
        [SerializeField] private Fighter playerFighter;

        [Header("UI")]
        [SerializeField] private GameObject gameSetupScreenPrefab;

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
            Debug.Log("=== MainGameManager Initialize ===");

            // 시스템 초기화 확인
            InitializeSystems();
            LoadGameState();
            ShowGameSetupScreen();
        }

        private void InitializeSystems()
        {
            // 각 시스템 싱글톤 확인
            if (CareerSystem.Instance == null)
            {
                Debug.LogWarning("[MainGameManager] CareerSystem 인스턴스가 없습니다.");
            }

            if (EconomySystem.Instance == null)
            {
                Debug.LogWarning("[MainGameManager] EconomySystem 인스턴스가 없습니다.");
            }

            if (MatchSimulationSystem.Instance == null)
            {
                Debug.LogWarning("[MainGameManager] MatchSimulationSystem 인스턴스가 없습니다.");
            }

            if (TrainingSystem.Instance == null)
            {
                Debug.LogWarning("[MainGameManager] TrainingSystem 인스턴스가 없습니다.");
            }

            Debug.Log("[MainGameManager] 모든 시스템 초기화 완료");
        }

        private void LoadGameState()
        {
            // TODO: 파일 시스템으로 저장/로드
            // 현재는 New Game으로 시작
            Debug.Log("[MainGameManager] 새로운 경영 시작 준비 완료");
        }

        private void ShowGameSetupScreen()
        {
            if (gameSetupScreenPrefab != null)
            {
                GameObject screen = Instantiate(gameSetupScreenPrefab);
                DontDestroyOnLoad(screen);

                Debug.Log("[MainGameManager] Game Setup 화면 표시");
            }
        }

        /// <summary>
        /// 경영 모드 변경
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            currentGameState = newState;

            switch (newState)
            {
                case GameState.CareerMode:
                    Debug.Log("[MainGameManager] 경영 모드: Career Mode");
                    break;
                case GameState.QuickMatch:
                    Debug.Log("[MainGameManager] 경영 모드: Quick Match");
                    break;
                case GameState.PromotionMode:
                    Debug.Log("[MainGameManager] 경영 모드: Promotion Mode");
                    break;
                case GameState.TournamentMode:
                    Debug.Log("[MainGameManager] 경영 모드: Tournament Mode");
                    break;
            }
        }

        /// <summary>
        /// 새로운 경영 시작
        /// </summary>
        public void StartNewCareer(Fighter fighter)
        {
            if (careerSystem != null)
            {
                Debug.LogError("[MainGameManager] CareerSystem이 null입니다.");
                return;
            }

            // 현재 상태 확인
            if (currentGameState != GameState.CareerMode)
            {
                Debug.LogWarning($"[MainGameManager] 현재 상태({currentGameState})에서 경영 시작은 불가합니다.");
                return;
            }

            // 경영 시작
            ChangeGameState(GameState.CareerMode);
            careerSystem.StartNewCareer(
                fighter.FullName,
                fighter.Age,
                fighter.WeightClass
            );

            // UI 닫기
            HideGameSetupScreen();

            Debug.Log($"[MainGameManager] 새로운 경영 시작: {fighter.DisplayName}");
        }

        /// <summary>
        /// 빠른 매치 진행
        /// </summary>
        public void StartQuickMatch(Fighter fighter1, Fighter fighter2)
        {
            if (matchSimulation != null)
            {
                Debug.LogError("[MainGameManager] MatchSimulationSystem이 null입니다.");
                return;
            }

            // Quick Match 모드
            ChangeGameState(GameState.QuickMatch);

            // 매치 생성 및 시뮬레이션
            MatchResult result = matchSimulation.SimulateMatch(fighter1, fighter2);
            OnMatchComplete(result);
        }

        /// <summary>
        /// 매치 완료 처리
        /// </summary>
        private void OnMatchComplete(MatchResult result)
        {
            // 경영지도 업데이트
            if (careerSystem != null)
            {
                // 승리/패배 경영 경험치 업데이트
                if (result.winner == playerFighter)
                {
                    // TODO: 경영 경험치 추가 로직
                }
            }

            // 돈 계산
            if (economySystem != null && result.winner == playerFighter)
            {
                int earnings = economySystem.CalculateMatchEarnings(
                    playerFighter,
                    MatchEventType.RegularFight,
                    result.winner == playerFighter,
                    result.method == VictoryMethod.KO || result.method == VictoryMethod.TKO,
                    result.method == VictoryMethod.Submission
                );

                Debug.Log($"[MainGameManager] 매치 수입: ${earnings:N0}");
            }

            // UI 콜백
            // EventSystem을 통해 UI 업데이트
            ShowNotification($"매치 완료: {result.winner.DisplayName} 승리!");
        }

        private void ShowNotification(string message)
        {
            // EventSystem을 통해 알림 표시
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.ShowNotification("알림", message, 2f);
            }
            else
            {
                Debug.Log(message);
            }
        }

        private void HideGameSetupScreen()
        {
            // TODO: GameSetupScreen 닫기
            Debug.Log("[MainGameManager] Game Setup 화면 숨김");
        }
    }

    /// <summary>
    /// 게임 상태
    /// </summary>
    public enum GameState
    {
        CareerMode,        // 경영 진행 모드
        QuickMatch,      // 빠른 매치
        PromotionMode,    // 승격제 모드
        TournamentMode,   // 토너먼트 모드
        MainMenu         // 메인 메뉴
    }
}
