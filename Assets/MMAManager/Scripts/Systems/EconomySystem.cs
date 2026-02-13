using System;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// 경영 경제 시스템 - 수입, 지출, 비용 관리
    /// </summary>
    public class EconomySystem : MonoBehaviour
    {
        private static EconomySystem _instance;
        public static EconomySystem Instance => _instance;

        [Header("Financial Settings")]
        [SerializeField] private int gymMonthlyCost = 2000;
        [SerializeField] private int coachSalaryPerWeek = 500;
        [SerializeField] private int sponsorshipBonusMultiplier = 2;

        [Header("Player Economy")]
        [SerializeField] private PlayerWallet playerWallet;

        [Header("Expense Categories")]
        [SerializeField] private int trainingCostPerPoint = 100;
        [SerializeField] private int medicalBaseCost = 500;
        [SerializeField] private int travelCostPerEvent = 200;

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
        /// 매치에서 획득한 돈 계산
        /// </summary>
        public int CalculateMatchEarnings(Fighter fighter, MatchEventType eventType, bool win, bool knockout, bool submission)
        {
            // 기본 파이트
            int basePurse = GetEventPurse(eventType);

            // 승리 보너스
            int winBonus = win ? 5000 : 0;

            // KO/서브미션 보너스
            int finishBonus = 0;
            if (knockout) finishBonus += 5000;
            if (submission) finishBonus += 3000;

            // 스폰서십 보너스
            int popularityBonus = fighter.Popularity * 100;

            return basePurse + winBonus + finishBonus + popularityBonus;
        }

        private int GetEventPurse(MatchEventType eventType)
        {
            return eventType switch
            {
                MatchEventType.RegularFight => 10000,
                MatchEventType.PrelimFight => 5000,
                MatchEventType.MainCard => 25000,
                MatchEventType.CoMainEvent => 50000,
                MatchEventType.MainEvent => 100000,
                MatchEventType.TitleFight => 250000,
                _ => 10000
            };
        }

        /// <summary>
        /// 매치 경험치(인기) 획득
        /// </summary>
        public void ProcessMatchResult(Fighter fighter, MatchResult result, MatchEventType eventType)
        {
            int earnings = CalculateMatchEarnings(fighter, eventType,
                result.winner == fighter,
                result.method == VictoryMethod.KO || result.method == VictoryMethod.TKO,
                result.method == VictoryMethod.Submission);

            playerWallet.AddCash(earnings);

            // 경영 경험치 증가
            if (result.winner == fighter)
            {
                playerWallet.AddExperience(100);
                playerWallet.AddPopularity(5);

                if (result.method == VictoryMethod.KO || result.method == VictoryMethod.TKO)
                    playerWallet.AddKOCount();
                if (result.method == VictoryMethod.Submission)
                    playerWallet.AddSubmissionCount();
            }

            Debug.Log($"[Economy] 매치 완료: {fighter.DisplayName} -> ${earnings:N0} 획득");
        }

        /// <summary>
        /// 주간 비용 계산
        /// </summary>
        public int CalculateWeeklyExpenses()
        {
            // 체육관 비용
            int gymCost = gymMonthlyCost / 4; // 주간으로 나눔

            // 코치 비용
            int coachCost = coachSalaryPerWeek;

            // 총 고정 비용
            int fixedCosts = gymCost + coachCost;

            // 총 훈련 포인트에 따른 가변 비용
            int trainingCost = playerWallet.GetWeeklyTrainingPoints() * trainingCostPerPoint;

            return fixedCosts + trainingCost;
        }

        /// <summary>
        /// 주간 경영 진행
        /// </summary>
        public void ProcessWeek()
        {
            int expenses = CalculateWeeklyExpenses();
            int income = playerWallet.GetWeeklyMatchIncome();

            // 순수지 계산
            playerWallet.cash += (income - expenses);

            Debug.Log($"[Economy] 주간 완료: 수입 ${income:N0} - 지출 ${expenses:N0} = ${(income - expenses):N0}");
        }
    }

    /// <summary>
    /// 플레이어 지갑 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName="Assets/MMAManager/ScriptableObjects/PlayerWallet")]
    public class PlayerWallet : ScriptableObject
    {
        [Header("Cash")]
        [SerializeField] public int cash = 5000;

        [Header("Experience")]
        [SerializeField] public int experience = 0;
        [SerializeField] public int trainingPoints = 50; // 주간 100pt 자동 회복

        [Header("Popularity")]
        [SerializeField] public int popularity = 10;

        [Header("Records")]
        [SerializeField] public int totalMatches = 0;
        [SerializeField] public int wins = 0;
        [SerializeField] public int losses = 0;
        [SerializeField] public int kos = 0; // KO 승리
        [SerializeField] public int submissions = 0; // 서브미션 승리

        [Header("Sponsorship")]
        [SerializeField] public bool hasSponsor = false;
        [SerializeField] public string sponsorName = "";
        [SerializeField] public int weeklySponsorBonus = 0;

        /// <summary>
        /// 주간 수입 계산
        /// </summary>
        public int GetWeeklyMatchIncome()
        {
            // 매치 수익 = 승리 * 평균 파이트
            int matchIncome = wins * 2000;

            // 스폰서십 보너스
            int performanceBonus = kos * 1000 + submissions * 500;

            // 스폰서십 보너스
            int sponsorBonus = hasSponsor ? weeklySponsorBonus : 0;

            return matchIncome + performanceBonus + sponsorBonus;
        }

        /// <summary>
        /// 주간 훈련 포인트 계산
        /// </summary>
        public int GetWeeklyTrainingPoints()
        {
            // 인기 회복 + 수동 추가
            int recoveredPoints = Mathf.FloorToInt(experience / 100); // 100xp당 1pt 회복

            // 주간 자동 지급
            int baseRecovery = 25; // 주간 25pt

            return baseRecovery + recoveredPoints;
        }

        /// <summary>
        /// 현금 추가
        /// </summary>
        public void AddCash(int amount)
        {
            cash += amount;
        }

        /// <summary>
        /// 경영 경험치 추가
        /// </summary>
        public void AddExperience(int amount)
        {
            experience += amount;
        }

        /// <summary>
        /// 인기도 추가
        /// </summary>
        public void AddPopularity(int amount)
        {
            popularity = Mathf.Clamp(popularity + amount, 0, 100);
        }

        /// <summary>
        /// KO 승리 추가
        /// </summary>
        public void AddKOCount()
        {
            kos++;
        }

        /// <summary>
        /// 서브미션 승리 추가
        /// </summary>
        public void AddSubmissionCount()
        {
            submissions++;
        }
    }
}
