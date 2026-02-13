using System;
using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// Scouting system for discovering new fighters through various routes
    /// </summary>
    public class ScoutingSystem : MonoBehaviour
    {
        private static ScoutingSystem _instance;
        public static ScoutingSystem Instance => _instance;

        [Header("Scouting Settings")]
        [SerializeField] private int maxScoutsPerWeek = 3;
        [SerializeField] private int baseScoutingCost = 500;
        [SerializeField] private float[] scoutingRouteSuccessRates = { 0.3f, 0.5f, 0.6f, 0.4f, 0.7f, 0.8f, 0.9f, 0.6f };
        
        [Header("Current Scouts")]
        [SerializeField] private List<ActiveScout> activeScouts = new List<ActiveScout>();

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
        /// Start a new scouting mission
        /// </summary>
        public ActiveScout StartScouting(ScoutingRoute route, int durationWeeks)
        {
            if (activeScouts.Count >= maxScoutsPerWeek)
            {
                Debug.LogWarning("Maximum scouts per week reached");
                return null;
            }

            ActiveScout scout = new ActiveScout
            {
                route = route,
                weeksRemaining = durationWeeks,
                startWeek = DateTime.Now,
                cost = CalculateScoutingCost(route, durationWeeks)
            };

            activeScouts.Add(scout);
            return scout;
        }

        /// <summary>
        /// Process scouting results for completed scouts
        /// </summary>
        public List<ScoutingResult> ProcessScoutingResults()
        {
            List<ScoutingResult> results = new List<ScoutingResult>();

            for (int i = activeScouts.Count - 1; i >= 0; i--)
            {
                ActiveScout scout = activeScouts[i];
                
                if (scout.weeksRemaining <= 0)
                {
                    ScoutingResult result = GenerateScoutingResult(scout);
                    results.Add(result);
                    activeScouts.RemoveAt(i);
                }
                else
                {
                    scout.weeksRemaining--;
                }
            }

            return results;
        }

        /// <summary>
        /// Generate a scouting result based on the route
        /// </summary>
        private ScoutingResult GenerateScoutingResult(ActiveScout scout)
        {
            ScoutingResult result = new ScoutingResult
            {
                route = scout.route,
                success = UnityEngine.Random.value < scoutingRouteSuccessRates[(int)scout.route]
            };

            if (result.success)
            {
                result.fighter = GenerateFighterFromRoute(scout.route);
                result.message = $"Found a promising fighter: {result.fighter.DisplayName}";
            }
            else
            {
                result.message = GetFailureMessage(scout.route);
            }

            return result;
        }

        /// <summary>
        /// Generate a fighter based on the scouting route
        /// </summary>
        private Fighter GenerateFighterFromRoute(ScoutingRoute route)
        {
            Fighter fighter = new Fighter();
            
            switch (route)
            {
                case ScoutingRoute.SchoolClub:
                    // Young fighters with high potential but low stats
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(17, 22), 
                        (WeightClass)UnityEngine.Random.Range(0, 8));
                    fighter.SetStat(Models.StatType.Potential, UnityEngine.Random.Range(70, 95));
                    break;

                case ScoutingRoute.StreetFighting:
                    // Tough fighters with high STR and DEF
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(20, 28), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    fighter.SetStat(Models.StatType.Strength, UnityEngine.Random.Range(50, 75));
                    fighter.SetStat(Models.StatType.Defense, UnityEngine.Random.Range(45, 70));
                    fighter.SetStat(Models.StatType.Potential, UnityEngine.Random.Range(50, 80));
                    break;

                case ScoutingRoute.AmateurTournament:
                    // Well-rounded fighters with good records
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(18, 25), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    for (int i = 0; i < UnityEngine.Random.Range(3, 10); i++)
                    {
                        fighter.AddWin(UnityEngine.Random.value < 0.4f, UnityEngine.Random.value < 0.3f);
                    }
                    fighter.SetStat(Models.StatType.Technique, UnityEngine.Random.Range(50, 70));
                    break;

                case ScoutingRoute.GymVisit:
                    // Fighters with good coaching background
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(19, 27), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    fighter.SetStat(Models.StatType.Wrestling, UnityEngine.Random.Range(45, 70));
                    fighter.SetStat(Models.StatType.BJJ, UnityEngine.Random.Range(45, 70));
                    break;

                case ScoutingRoute.OnlineVideos:
                    // Unknown fighters, could be hit or miss
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(18, 30), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    fighter.SetStat(Models.StatType.Potential, UnityEngine.Random.Range(30, 90));
                    break;

                case ScoutingRoute.Referral:
                    // Recommended fighters with reliable info
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(20, 28), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    fighter.SetStat(Models.StatType.Potential, UnityEngine.Random.Range(60, 85));
                    break;

                case ScoutingRoute.Overseas:
                    // International fighters with unique styles
                    fighter = new Fighter(GetRandomInternationalName(), "", 
                        UnityEngine.Random.Range(21, 29), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    fighter.SetStat(Models.StatType.Potential, UnityEngine.Random.Range(65, 90));
                    break;

                case ScoutingRoute.ScoutReport:
                    // Analyzed fighters with detailed stats
                    fighter = new Fighter(GetRandomName(), GetRandomSurname(), 
                        UnityEngine.Random.Range(19, 26), 
                        (WeightClass)UnityEngine.Random.Range(0, 11));
                    // All stats are more predictable
                    for (int i = 0; i < 7; i++)
                    {
                        fighter.SetStat((Models.StatType)i, UnityEngine.Random.Range(45, 65));
                    }
                    break;
            }

            return fighter;
        }

        private int CalculateScoutingCost(ScoutingRoute route, int weeks)
        {
            float routeMultiplier = route switch
            {
                ScoutingRoute.SchoolClub => 0.3f,
                ScoutingRoute.StreetFighting => 0.5f,
                ScoutingRoute.AmateurTournament => 0.8f,
                ScoutingRoute.GymVisit => 0.6f,
                ScoutingRoute.OnlineVideos => 0.2f,
                ScoutingRoute.Referral => 0.4f,
                ScoutingRoute.Overseas => 1.5f,
                ScoutingRoute.ScoutReport => 1.0f,
                _ => 1.0f
            };

            return Mathf.RoundToInt(baseScoutingCost * routeMultiplier * weeks);
        }

        private string GetFailureMessage(ScoutingRoute route)
        {
            string[] messages = route switch
            {
                ScoutingRoute.SchoolClub => new[] { "No promising students found.", "The school season ended." },
                ScoutingRoute.StreetFighting => new[] { "Underground fights were cancelled.", "Nothing noteworthy." },
                ScoutingRoute.AmateurTournament => new[] { "No standouts detected.", "All fighters already signed." },
                ScoutingRoute.GymVisit => new[] { "Gym was closed.", "No free agents available." },
                ScoutingRoute.OnlineVideos => new[] { "Video was fake.", "Fighter already retired." },
                ScoutingRoute.Referral => new[] { "Referral didn't pan out.", "Fighter wasn't interested." },
                ScoutingRoute.Overseas => new[] { "Visa issues.", "Fighter signed elsewhere." },
                ScoutingRoute.ScoutReport => new[] { "Report was inaccurate.", "Fighter retired." },
                _ => new[] { "Scouting failed." }
            };

            return messages[UnityEngine.Random.Range(0, messages.Length)];
        }

        private string GetRandomName()
        {
            string[] names = { "John", "Mike", "Chris", "Alex", "Marcus", "Carlos", "Anderson", "Jose", "Max", "Kamaru" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }

        private string GetRandomSurname()
        {
            string[] surnames = { "Smith", "Silva", "Jones", "Dos Anjos", "Machida", "Covington", "Masvidal", "Gaethje" };
            return surnames[UnityEngine.Random.Range(0, surnames.Length)];
        }

        private string GetRandomInternationalName()
        {
            string[] names = { "Khabib", "Islam", "Volkanovski", "Adesanya", "Ngannou", "Usman", "Yan" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }
    }

    #region Data Classes
    [Serializable]
    public class ActiveScout
    {
        public ScoutingRoute route;
        public int weeksRemaining;
        public DateTime startWeek;
        public int cost;
        public string description;
    }

    [Serializable]
    public class ScoutingResult
    {
        public ScoutingRoute route;
        public bool success;
        public Fighter fighter;
        public string message;
    }
    #endregion

    #region Enums
    public enum ScoutingRoute
    {
        SchoolClub,         // 학교 격투기 부
        StreetFighting,     // 길거리 스트리트 파이트
        AmateurTournament,  // 아마추어 대회
        GymVisit,           // 체육관 방문
        OnlineVideos,       // 온라인 비디오
        Referral,           // 추천
        Overseas,           // 해외 스카우팅
        ScoutReport         // 스카우트 리포트
    }
    #endregion
}