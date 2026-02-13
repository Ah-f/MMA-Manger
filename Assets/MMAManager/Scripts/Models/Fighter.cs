using System;
using UnityEngine;

namespace MMAManager.Models
{
    /// <summary>
    /// Fighter class representing an MMA fighter with all stats and properties
    /// </summary>
    [Serializable]
    public class Fighter
    {
        #region Basic Info
        [Header("Basic Information")]
        [SerializeField] private string fighterId;
        [SerializeField] private string firstName;
        [SerializeField] private string lastName;
        [SerializeField] private string nickname;
        [SerializeField] private int age;
        [SerializeField] private WeightClass weightClass;
        [SerializeField] private string nationality;
        [SerializeField] private Sprite portrait;
        #endregion

        #region Core Stats (1-100)
        [Header("Core Stats")]
        [SerializeField] private int strength;      // STR - 힘
        [SerializeField] private int technique;     // TEC - 기술
        [SerializeField] private int speed;         // SPD - 스피드
        [SerializeField] private int stamina;       // STA - 체력
        [SerializeField] private int defense;       // DEF - 방어
        [SerializeField] private int wrestling;     // WREST - 레슬링
        [SerializeField] private int bjj;           // BJJ - 주짓수
        [SerializeField] private int potential;     // POT - 잠재력
        #endregion

        #region Condition System
        [Header("Condition")]
        [SerializeField] public int condition;     // 컨디션 (0-100)
        [SerializeField] public int fatigue;       // 피로도 (0-100)
        [SerializeField] public int health;        // 건강 (0-100)
        #endregion

        #region Fight Record
        [Header("Record")]
        [SerializeField] private int wins;
        [SerializeField] private int losses;
        [SerializeField] private int draws;
        [SerializeField] private int knockoutWins;
        [SerializeField] private int submissionWins;
        [SerializeField] private int decisionWins;
        #endregion

        #region Training & Development
        [Header("Development")]
        [SerializeField] private int trainingPoints;      // 남은 트레이닝 포인트
        [SerializeField] private int[] statGrowthHistory; // 스탯 성장 기록
        [SerializeField] private int totalTrainingWeeks;  // 총 훈련 주차
        #endregion

        #region Contract & Financial
        [Header("Contract")]
        [SerializeField] private int contractLength;  // 계약 기간 (월)
        [SerializeField] private int monthlySalary;   // 월급 ($)
        [SerializeField] private int winBonus;        // 승리 보너스 ($)
        [SerializeField] private int popularity;      // 인기도 (0-100)
        #endregion

        #region Traits & Skills
        [Header("Traits")]
        [SerializeField] private FighterTrait[] traits;
        [SerializeField] private FightingStyle preferredStyle;
        #endregion

        #region Properties
        public string FighterId => fighterId;
        public string FirstName => firstName;
        public string LastName => lastName;
        public string FullName => $"{firstName} {lastName}";
        public string DisplayName => string.IsNullOrEmpty(nickname) ? FullName : $"{nickname} \"{FullName}\"";
        public int Age => age;
        public WeightClass WeightClass => weightClass;

        // Core Stats (0-100)
        public int STR => strength;
        public int TEC => technique;
        public int SPD => speed;
        public int STA => stamina;
        public int DEF => defense;
        public int WREST => wrestling;
        public int BJJ => bjj;
        public int POT => potential;

        // Overall rating (average of core stats)
        public int Overall => (strength + technique + speed + stamina + defense + wrestling + bjj) / 7;

        // Condition (0-100)
        public int Condition => condition;
        public int Fatigue => fatigue;
        public int Health => health;

        // Popularity from contract section
        public int Popularity => popularity;

        // Record
        public int Wins => wins;
        public int Losses => losses;
        public int Draws => draws;
        public int TotalFights => wins + losses + draws;
        #endregion

        #region Constructors
        public Fighter()
        {
            fighterId = Guid.NewGuid().ToString();
            InitializeDefaultsStatic();
        }

        public Fighter(string firstName, string lastName, int age, WeightClass weightClass)
        {
            fighterId = Guid.NewGuid().ToString();
            this.firstName = firstName;
            this.lastName = lastName;
            this.age = age;
            this.weightClass = weightClass;
            InitializeDefaultsStatic();
        }
        #endregion

        #region Initialization
        private void InitializeDefaultsStatic()
        {
            condition = 100;
            fatigue = 0;
            health = 100;

            wins = 0;
            losses = 0;
            draws = 0;

            trainingPoints = 50;
            totalTrainingWeeks = 0;

            contractLength = 12;
            monthlySalary = 2000;
            winBonus = 1000;
            popularity = 20;
            preferredStyle = FightingStyle.Balanced;
        }

        public void RandomizeStats()
        {
            strength = UnityEngine.Random.Range(30, 70);
            technique = UnityEngine.Random.Range(30, 70);
            speed = UnityEngine.Random.Range(30, 70);
            stamina = UnityEngine.Random.Range(40, 70);
            defense = UnityEngine.Random.Range(30, 70);
            wrestling = UnityEngine.Random.Range(20, 60);
            bjj = UnityEngine.Random.Range(20, 60);
            potential = UnityEngine.Random.Range(50, 95);

            monthlySalary = 2000 + UnityEngine.Random.Range(0, 3000);
            winBonus = 1000 + UnityEngine.Random.Range(0, 2000);
            popularity = UnityEngine.Random.Range(10, 40);
            preferredStyle = (FightingStyle)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(FightingStyle)).Length);
        }
        #endregion

        #region Stat Modification
        public void TrainStat(StatType stat, int amount)
        {
            if (trainingPoints < amount)
                amount = trainingPoints;

            switch (stat)
            {
                case StatType.Strength:
                    strength = Mathf.Clamp(strength + amount, 0, potential);
                    break;
                case StatType.Technique:
                    technique = Mathf.Clamp(technique + amount, 0, potential);
                    break;
                case StatType.Speed:
                    speed = Mathf.Clamp(speed + amount, 0, potential);
                    break;
                case StatType.Stamina:
                    stamina = Mathf.Clamp(stamina + amount, 0, potential);
                    break;
                case StatType.Defense:
                    defense = Mathf.Clamp(defense + amount, 0, potential);
                    break;
                case StatType.Wrestling:
                    wrestling = Mathf.Clamp(wrestling + amount, 0, potential);
                    break;
                case StatType.BJJ:
                    bjj = Mathf.Clamp(bjj + amount, 0, potential);
                    break;
            }

            trainingPoints -= amount;
            totalTrainingWeeks++;

            // Training increases fatigue
            fatigue = Mathf.Clamp(fatigue + amount / 2, 0, 100);
        }

        /// <summary>
        /// Directly set a stat value (for initialization)
        /// </summary>
        public void SetStat(StatType stat, int value)
        {
            switch (stat)
            {
                case StatType.Strength:
                    strength = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Technique:
                    technique = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Speed:
                    speed = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Stamina:
                    stamina = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Defense:
                    defense = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Wrestling:
                    wrestling = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.BJJ:
                    bjj = Mathf.Clamp(value, 0, 100);
                    break;
                case StatType.Potential:
                    potential = Mathf.Clamp(value, 0, 100);
                    break;
            }
        }

        public void Recover(int amount)
        {
            condition = Mathf.Clamp(condition + amount, 0, 100);
            fatigue = Mathf.Clamp(fatigue - amount, 0, 100);
            health = Mathf.Clamp(health + amount, 0, 100);
        }

        public void AddWin(bool knockout, bool submission)
        {
            wins++;
            if (knockout) knockoutWins++;
            if (submission) submissionWins++;
            if (!knockout && !submission) decisionWins++;

            popularity = Mathf.Clamp(popularity + 5, 0, 100);
        }

        public void AddLoss()
        {
            losses++;
            popularity = Mathf.Clamp(popularity - 3, 0, 100);
        }

        public void AddDraw()
        {
            draws++;
        }
        #endregion

        #region Utility Methods
        public bool IsReadyToFight()
        {
            return health >= 70 && condition >= 50 && fatigue <= 50;
        }

        public int GetWinRate()
        {
            if (TotalFights == 0) return 0;
            return (wins * 100) / TotalFights;
        }

        public int GetExpectedSalary()
        {
            int baseSalary = monthlySalary;
            int bonus = (int)(popularity * 50);
            int performanceBonus = (wins * 200);
            return baseSalary + bonus + performanceBonus;
        }
        #endregion
    }

    #region Enums
    public enum WeightClass
    {
        Atomweight,      // 105 lbs (47.6 kg)
        Strawweight,     // 115 lbs (52.2 kg)
        Flyweight,       // 125 lbs (56.7 kg)
        Bantamweight,    // 135 lbs (61.2 kg)
        Featherweight,   // 145 lbs (65.8 kg)
        Lightweight,     // 155 lbs (70.3 kg)
        Welterweight,    // 170 lbs (77.1 kg)
        Middleweight,    // 185 lbs (83.9 kg)
        LightHeavyweight,// 205 lbs (93.0 kg)
        Cruiserweight,   // 225 lbs (102.1 kg)
        Heavyweight      // 265 lbs (120.2 kg)
    }

    public enum StatType
    {
        Strength,
        Technique,
        Speed,
        Stamina,
        Defense,
        Wrestling,
        BJJ,
        Potential
    }

    public enum FightingStyle
    {
        Striker,         // 타격기 위주
        Grappler,        // 그래플링 위주
        Wrestler,        // 레슬링 위주
        Balanced,        // 밸런스형
        CounterFighter,  // 카운터형
        PressureFighter  // 프레셔형
    }

    [Flags]
    public enum FighterTrait
    {
        None = 0,
        IronChin = 1 << 0,        // 강한 턱
        HeavyHands = 1 << 1,      // 강한 펀치
        WrestlingWizard = 1 << 2, // 레슬링 달인
        BJJSpecialist = 1 << 3,   // 주짓수 전문가
        CardioMachine = 1 << 4,   // 체력 괴물
        GlassCannon = 1 << 5,     // 유리 대포 (강한 펀치/약한 턱)
        KOArtist = 1 << 6,        // KO 전문가
        SubmissionArtist = 1 << 7,// 서브미션 전문가
        DecisionMachine = 1 << 8  // 결정 전문가
    }
    #endregion
}
