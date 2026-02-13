using System;
using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// Weekly training schedule for a fighter
    /// </summary>
    [Serializable]
    public class WeeklyTrainingSchedule
    {
        public DayOfWeekTraining[] weekDays = new DayOfWeekTraining[7];
        
        public WeeklyTrainingSchedule()
        {
            for (int i = 0; i < 7; i++)
            {
                weekDays[i] = new DayOfWeekTraining((DayOfWeek)i);
            }
        }
        
        public DayOfWeekTraining GetDayTraining(DayOfWeek day)
        {
            return weekDays[(int)day];
        }
    }

    /// <summary>
    /// Training for a single day
    /// </summary>
    [Serializable]
    public class DayOfWeekTraining
    {
        public DayOfWeek day;
        public bool restDay = false;
        public TrainingProgram trainingProgram;
        public TrainingIntensity intensity = TrainingIntensity.Normal;
        
        public DayOfWeekTraining(DayOfWeek day)
        {
            this.day = day;
            // Sunday is default rest day
            if (day == DayOfWeek.Sunday)
            {
                restDay = true;
            }
            else
            {
                trainingProgram = TrainingProgram.GeneralConditioning;
                intensity = TrainingIntensity.Normal;
            }
        }
    }

    /// <summary>
    /// Training system for managing fighter development
    /// </summary>
    public class TrainingSystem : MonoBehaviour
    {
        private static TrainingSystem _instance;
        public static TrainingSystem Instance => _instance;

        [Header("Training Settings")]
        [SerializeField] private int maxTrainingPointsPerWeek = 20;
        [SerializeField] private float intensityFatigueMultiplier = 0.5f;
        [SerializeField] private int baseRecoveryAmount = 10;

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
        /// Execute a day of training for a fighter
        /// </summary>
        public TrainingResult ExecuteDayTraining(Fighter fighter, DayOfWeekTraining training)
        {
            TrainingResult result = new TrainingResult();
            
            if (training.restDay)
            {
                // Rest day - recover
                int recovery = baseRecoveryAmount;
                fighter.Recover(recovery);
                result.recoveryAmount = recovery;
                result.message = "Rest day - fighter recovered";
                return result;
            }

            // Check if fighter can train
            if (fighter.Condition < 30)
            {
                result.message = "Fighter is too tired to train!";
                result.success = false;
                return result;
            }

            // Calculate training effectiveness
            float intensityMultiplier = GetIntensityMultiplier(training.intensity);
            int statGain = CalculateStatGain(fighter, training.trainingProgram, training.intensity);
            int fatigueGain = CalculateFatigueGain(training.intensity);

            // Apply training
            Models.StatType primaryStat = GetPrimaryStatForProgram(training.trainingProgram);
            fighter.TrainStat(primaryStat, statGain);
            
            // Apply fatigue
            fighter.fatigue = Mathf.Clamp(fighter.fatigue + fatigueGain, 0, 100);
            
            // Reduce condition based on fatigue
            if (fighter.fatigue > 70)
            {
                fighter.condition = Mathf.Clamp(fighter.condition - 5, 0, 100);
            }

            result.statTrained = primaryStat;
            result.statGain = statGain;
            result.fatigueGain = fatigueGain;
            result.success = true;
            result.message = $"Training {training.trainingProgram} at {training.intensity} intensity - {primaryStat} +{statGain}";

            return result;
        }

        /// <summary>
        /// Process a full week of training
        /// </summary>
        public List<TrainingResult> ExecuteWeekTraining(Fighter fighter, WeeklyTrainingSchedule schedule)
        {
            List<TrainingResult> results = new List<TrainingResult>();

            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                DayOfWeekTraining training = schedule.GetDayTraining(day);
                
                TrainingResult result = ExecuteDayTraining(fighter, training);
                results.Add(result);
                
                // Check for injuries
                if (fighter.fatigue >= 90 && UnityEngine.Random.value < 0.2f)
                {
                    // Injury chance
                    fighter.health = Mathf.Clamp(fighter.health - 20, 0, 100);
                    result.message += " - INJURY WARNING!";
                }
            }

            return results;
        }

        /// <summary>
        /// Get training effectiveness based on potential
        /// </summary>
        private int CalculateStatGain(Fighter fighter, TrainingProgram program, TrainingIntensity intensity)
        {
            float baseGain = 2f;
            float potentialFactor = (100 - fighter.Overall) / 100f; // Lower stats = more growth
            float potentialMultiplier = fighter.POT / 100f; // Higher potential = more growth
            
            float intensityMult = GetIntensityMultiplier(intensity);
            
            // Program-specific bonus
            float programBonus = GetProgramBonus(program);
            
            int gain = Mathf.RoundToInt(baseGain * potentialFactor * potentialMultiplier * intensityMult * programBonus);
            
            // Random variance
            gain += UnityEngine.Random.Range(-1, 2);
            
            return Mathf.Clamp(gain, 0, 5);
        }

        private int CalculateFatigueGain(TrainingIntensity intensity)
        {
            return intensity switch
            {
                TrainingIntensity.VeryLight => 3,
                TrainingIntensity.Light => 5,
                TrainingIntensity.Normal => 8,
                TrainingIntensity.Hard => 12,
                TrainingIntensity.VeryHard => 18,
                _ => 8
            };
        }

        private float GetIntensityMultiplier(TrainingIntensity intensity)
        {
            return intensity switch
            {
                TrainingIntensity.VeryLight => 0.5f,
                TrainingIntensity.Light => 0.75f,
                TrainingIntensity.Normal => 1.0f,
                TrainingIntensity.Hard => 1.3f,
                TrainingIntensity.VeryHard => 1.5f,
                _ => 1.0f
            };
        }

        private float GetProgramBonus(TrainingProgram program)
        {
            return program switch
            {
                TrainingProgram.Striking => 1.1f,
                TrainingProgram.Wrestling => 1.1f,
                TrainingProgram.BJJ => 1.1f,
                TrainingProgram.StrengthAndConditioning => 1.2f,
                TrainingProgram.Cardio => 1.15f,
                TrainingProgram.Sparring => 1.3f,
                TrainingProgram.FilmStudy => 0.8f,
                TrainingProgram.Recovery => 0.5f,
                TrainingProgram.GeneralConditioning => 1.0f,
                TrainingProgram.WeightCut => 0.7f,
                _ => 1.0f
            };
        }

        private Models.StatType GetPrimaryStatForProgram(TrainingProgram program)
        {
            return program switch
            {
                TrainingProgram.Striking => Models.StatType.Technique,
                TrainingProgram.Wrestling => Models.StatType.Wrestling,
                TrainingProgram.BJJ => Models.StatType.BJJ,
                TrainingProgram.StrengthAndConditioning => Models.StatType.Strength,
                TrainingProgram.Cardio => Models.StatType.Stamina,
                TrainingProgram.Sparring => Models.StatType.Defense,
                TrainingProgram.FilmStudy => Models.StatType.Technique,
                TrainingProgram.GeneralConditioning => Models.StatType.Speed,
                _ => Models.StatType.Stamina
            };
        }
    }

    #region Data Classes
    public class TrainingResult
    {
        public bool success;
        public string message;
        public Models.StatType statTrained;
        public int statGain;
        public int fatigueGain;
        public int recoveryAmount;
    }
    #endregion

    #region Enums
    public enum TrainingIntensity
    {
        VeryLight,   // 아주 가볍게
        Light,       // 가볍게
        Normal,      // 보통
        Hard,        // 강하게
        VeryHard     // 아주 강하게
    }

    public enum TrainingProgram
    {
        GeneralConditioning,  // 일반 컨디셔닝
        Striking,             // 타격 훈련
        Wrestling,            // 레슬링
        BJJ,                  // 주짓수
        StrengthAndConditioning, // 근력 및 컨디셔닝
        Cardio,               // 유산소 훈련
        Sparring,             // 스파링
        FilmStudy,            // 비디오 분석
        Recovery,             // 회복 훈련
        WeightCut             // 체중 감량
    }

    public enum DayOfWeek
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
    #endregion
}