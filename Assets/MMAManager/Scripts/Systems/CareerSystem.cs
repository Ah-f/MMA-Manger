using System;
using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;
using UnityEditor.SceneManagement;

namespace MMAManager.Systems
{
    /// <summary>
    /// 선수 경영 시스템 - 모든 경영 데이터를 관리
    /// </summary>
    public class CareerSystem : MonoBehaviour
    {
        private static CareerSystem _instance;
        public static CareerSystem Instance => _instance;

        [Header("Player Data")]
        [SerializeField] private PlayerCareer playerCareer;

        [Header("Settings")]
        [SerializeField] private int maxFightersInRoster = 200;
        [SerializeField] private int startingCash = 5000;
        [SerializeField] private int startingPopularity = 10;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 새로운 경영 시작
        /// </summary>
        public void StartNewCareer(string fighterName, int age, WeightClass weightClass)
        {
            PlayerCareer newCareer = ScriptableObject.CreateInstance<PlayerCareer>();

            // 초기 선수 생성
            Fighter initialFighter = new Fighter(fighterName, "Fighter", age, weightClass);
            newCareer.fighters = new List<Fighter> { initialFighter };
            newCareer.currentFighter = initialFighter;
            newCareer.cash = startingCash;
            newCareer.popularity = startingPopularity;

            // 초기 데이터 설정
            SetInitialStats(initialFighter);

            SaveCareer(newCareer);
            LoadCareerScene();
        }

        private void SetInitialStats(Fighter fighter)
        {
            // 새로운 선수는 낮은 스탯으로 시작
            fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(35, 50));
            fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(35, 50));
            fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(35, 50));
            fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(40, 55));
            fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(35, 50));
            fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(20, 45));
            fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(20, 45));
            fighter.SetStat(StatType.Potential, UnityEngine.Random.Range(50, 70));

            Debug.Log($"[CareerSystem] 새로운 경영 시작: {fighter.DisplayName}");
        }

        /// <summary>
        /// 경영 데이터 저장
        /// </summary>
        public void SaveCareer(PlayerCareer career)
        {
            string path = $"Assets/SavedData/Careers/{career.careerId}.asset";

            #if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEditor.AssetDatabase.CreateAsset(career, path);
            }
            #else
            {
                // 런타임에서는 Resources 폴더 사용
                career = Resources.Load<PlayerCareer>($"SavedData/Careers/{career.careerId}");
            }
            #endif
        }

        /// <summary>
        /// 경영 데이터 로드
        /// </summary>
        public PlayerCareer LoadCareer(string careerId)
        {
            string path = $"Assets/SavedData/Careers/{careerId}.asset";

            PlayerCareer career = null;

            #if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                    career = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerCareer>(path);
                    }
            #else
            {
                    career = Resources.Load<PlayerCareer>(path);
                    if (career == null)
                    {
                        Debug.LogError($"[CareerSystem] 경영 데이터를 찾을 수 없습니다: {path}");
                        return null;
                    }
                    }
            }
            #endif

            if (career != null)
            {
                playerCareer = career;
                LoadCareerScene();
            }

            return career;
        }

        private void LoadCareerScene()
        {
            #if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                    EditorSceneManager.OpenScene("Assets/Scenes/CareerScene.unity");
                    }
            #else
            {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("CareerScene");
                    }
            #endif
        }

        /// <summary>
        /// 선수 명단 관리
        /// </summary>
        public List<Fighter> GetAvailableFighters()
        {
            if (playerCareer != null)
            {
                return new List<Fighter>(playerCareer.fighters);
            }
            return new List<Fighter>();
        }

        /// <summary>
        /// 챔피언십 순위 계산
        /// </summary>
        public int CalculateChampionshipRank(int totalPopularity)
        {
            // 인기: 챔피언십 순위는 대략 100명 (상위 Top 5)
            // 1순위 = 15명 * 4 = 60인
            // 2순위 = 5명 * 3 = 15인
            // 3순위 = 1명

            if (totalPopularity >= 100) return 1; // World class
            if (totalPopularity >= 60) return 2; // Major class
            if (totalPopularity >= 30) return 3; // Intercontinental class
            return 4; // Bronze class
        }

        /// <summary>
        /// 매치 생성 시스템
        /// </summary>
        public void GenerateSchedule()
        {
            List<MatchEvent> upcomingMatches = new List<MatchEvent>();

            // 현재 챔피언십에 따라 다른 등급의 매치
            if (playerCareer != null && playerCareer.currentFighter != null)
            {
                int rank = CalculateChampionshipRank(playerCareer.currentFighter.Popularity);

                // 4주일 매치 일정
                for (int week = 0; week < 4; week++)
                {
                    MatchEvent match = GenerateMatchForWeek(week, rank);
                    upcomingMatches.Add(match);
                }

                // 챔피언십 전 (5주일 후)
                MatchEvent titleMatch = new MatchEvent
                {
                    eventType = MatchEventType.TitleFight,
                    opponent = GenerateChampionshipOpponent(rank),
                    purse = 50000
                };
                upcomingMatches.Add(titleMatch);
            }
        }

        private MatchEvent GenerateMatchForWeek(int week, int rank)
        {
            // 등급에 따른 상금과 대전력
            int basePurse = 10000 * (5 - rank); // 높은 등급 = 높은 상금
            int purse = basePurse + UnityEngine.Random.Range(-2000, 2000);

            WeightClass opponentClass = (WeightClass)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(WeightClass)).Length);

            return new MatchEvent
            {
                week = week,
                eventType = rank <= 2 ? MatchEventType.PrelimFight : MatchEventType.RegularFight,
                purse = purse,
                opponent = GenerateOpponent(opponentClass)
            };
        }

        private Fighter GenerateOpponent(WeightClass weightClass)
        {
            // 해당 체급의 랜덤한 선수 생성
            Fighter opponent = new Fighter(
                $"Opponent",
                "Fighter",
                UnityEngine.Random.Range(25, 35),
                (WeightClass)weightClass
            );

            // 스탯는 인기 선수보다 낮게 설정
            opponent.SetStat(StatType.Strength, UnityEngine.Random.Range(30, 45));
            opponent.SetStat(StatType.Technique, UnityEngine.Random.Range(30, 45));
            opponent.SetStat(StatType.Speed, UnityEngine.Random.Range(30, 45));
            opponent.SetStat(StatType.Stamina, UnityEngine.Random.Range(30, 45));
            opponent.SetStat(StatType.Defense, UnityEngine.Random.Range(30, 45));
            opponent.SetStat(StatType.Wrestling, UnityEngine.Random.Range(20, 40));
            opponent.SetStat(StatType.BJJ, UnityEngine.Random.Range(20, 40));
            opponent.SetStat(StatType.Potential, UnityEngine.Random.Range(40, 60));

            return opponent;
        }

        private Fighter GenerateChampionshipOpponent(int rank)
        {
            // 랭컨텐더/미들급 챔피언십 생성
            string[] names = { "Anderson", "Fedor", "Nogueira", "Couture", "Liddell", "Mir", "Lesnar", "Silva", "Shogun", "Wanderlei", "Jones" };

            Fighter champion = new Fighter(
                names[UnityEngine.Random.Range(0, names.Length)],
                "Champion",
                UnityEngine.Random.Range(28, 35),
                WeightClass.Lightweight
            );

            // 챔피언십은 높은 스탯
            champion.SetStat(StatType.Strength, UnityEngine.Random.Range(75, 95));
            champion.SetStat(StatType.Technique, UnityEngine.Random.Range(75, 95));
            champion.SetStat(StatType.Speed, UnityEngine.Random.Range(75, 95));
            champion.SetStat(StatType.Stamina, UnityEngine.Random.Range(75, 95));
            champion.SetStat(StatType.Defense, UnityEngine.Random.Range(75, 95));
            champion.SetStat(StatType.Wrestling, UnityEngine.Random.Range(65, 90));
            champion.SetStat(StatType.BJJ, UnityEngine.Random.Range(65, 90));
            champion.SetStat(StatType.Potential, 90);

            return champion;
        }

        /// <summary>
        /// 일정 매치 이벤트
        /// </summary>
        [Serializable]
        public class MatchEvent
        {
            public int week;
            public MatchEventType eventType;
            public Fighter opponent;
            public int purse;
        }
    }

    /// <summary>
    /// 플레이어 경영 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName="PlayerCareer", menuName="MMAManager/PlayerCareer")]
    public class PlayerCareer : ScriptableObject
    {
        [Header("Career Info")]
        [SerializeField] public string careerId = Guid.NewGuid().ToString();
        [SerializeField] public List<Fighter> fighters = new List<Fighter>();
        [SerializeField] public Fighter currentFighter;
        [SerializeField] public int cash = 5000;
        [SerializeField] public int popularity = 10;
        [SerializeField] public int currentWeek = 0;
        [SerializeField] public int totalWeeks = 0;

        [Header("Promotion")]
        [SerializeField] public ChampionshipRank championshipRank = ChampionshipRank.Bronze;
        [SerializeField] public int titleMatchesWon = 0;

        [Header("Records")]
        [SerializeField] public int totalWins = 0;
        [SerializeField] public int totalLosses = 0;
        [SerializeField] public int totalKOs = 0;
        [SerializeField] public int totalSubmissions = 0;

        [Header("Achievements")]
        [SerializeField] public List<string> achievements = new List<string>();
    }

    /// <summary>
    /// 챔피언십 순위
    /// </summary>
    public enum ChampionshipRank
    {
        Unranked = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        WorldChampion = 4,
        HallOfFame = 5
    }
}
