using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// Central database system for managing all fighters in the game
    /// </summary>
    public class FighterDatabase : MonoBehaviour
    {
        private static FighterDatabase _instance;
        public static FighterDatabase Instance => _instance;

        [Header("Database")]
        [SerializeField] private List<Fighter> allFighters = new List<Fighter>();
        
        [Header("Settings")]
        [SerializeField] private int maxFighters = 200;
        [SerializeField] private bool autoSave = true;

        private Dictionary<string, Fighter> fighterDict;

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            fighterDict = new Dictionary<string, Fighter>();
            LoadFighters();
        }
        #endregion

        #region Fighter Management
        /// <summary>
        /// Add a new fighter to the database
        /// </summary>
        public bool AddFighter(Fighter fighter)
        {
            if (fighter == null) return false;
            if (allFighters.Count >= maxFighters)
            {
                Debug.LogWarning($"Fighter database is full (max: {maxFighters})");
                return false;
            }
            if (fighterDict.ContainsKey(fighter.FighterId))
            {
                Debug.LogWarning($"Fighter with ID {fighter.FighterId} already exists");
                return false;
            }

            allFighters.Add(fighter);
            fighterDict[fighter.FighterId] = fighter;

            if (autoSave) SaveFighters();

            return true;
        }

        /// <summary>
        /// Remove a fighter from the database
        /// </summary>
        public bool RemoveFighter(string fighterId)
        {
            if (!fighterDict.TryGetValue(fighterId, out Fighter fighter))
                return false;

            allFighters.Remove(fighter);
            fighterDict.Remove(fighterId);

            if (autoSave) SaveFighters();

            return true;
        }

        /// <summary>
        /// Get a fighter by ID
        /// </summary>
        public Fighter GetFighter(string fighterId)
        {
            return fighterDict.GetValueOrDefault(fighterId);
        }

        /// <summary>
        /// Get all fighters
        /// </summary>
        public List<Fighter> GetAllFighters()
        {
            return new List<Fighter>(allFighters);
        }

        /// <summary>
        /// Get fighters by weight class
        /// </summary>
        public List<Fighter> GetFightersByWeightClass(WeightClass weightClass)
        {
            return allFighters.Where(f => f.WeightClass == weightClass).ToList();
        }

        /// <summary>
        /// Get available fighters (not injured, ready to fight)
        /// </summary>
        public List<Fighter> GetAvailableFighters()
        {
            return allFighters.Where(f => f.IsReadyToFight()).ToList();
        }

        /// <summary>
        /// Get fighters sorted by overall rating
        /// </summary>
        public List<Fighter> GetFightersByOverall()
        {
            return allFighters.OrderByDescending(f => f.Overall).ToList();
        }

        /// <summary>
        /// Get fighters sorted by popularity
        /// </summary>
        public List<Fighter> GetFightersByPopularity()
        {
            return allFighters.OrderByDescending(f => f.Condition).ToList();
        }
        #endregion

        #region Fighter Generation
        /// <summary>
        /// Generate a random fighter with the given parameters
        /// </summary>
        public Fighter GenerateRandomFighter(string firstName, string lastName, int age, WeightClass weightClass)
        {
            Fighter fighter = new Fighter(firstName, lastName, age, weightClass);
            return fighter;
        }

        /// <summary>
        /// Generate a random fighter with random name
        /// </summary>
        public Fighter GenerateRandomFighter(WeightClass weightClass)
        {
            string firstName = GetRandomFirstName();
            string lastName = GetRandomLastName();
            int age = UnityEngine.Random.Range(18, 35);
            
            return GenerateRandomFighter(firstName, lastName, age, weightClass);
        }

        /// <summary>
        /// Generate an elite retired fighter comeback
        /// </summary>
        public Fighter GenerateEliteComebackFighter(EliteBackground background)
        {
            Fighter fighter = new Fighter();
            
            // Elite fighters have higher base stats
            switch (background)
            {
                case EliteBackground.OlympicWrestler:
                    fighter = new Fighter("John", "Smith", 28, WeightClass.Welterweight);
                    // WREST would be very high
                    break;
                case EliteBackground.WorldChampionBoxer:
                    fighter = new Fighter("Mike", "Tyson", 30, WeightClass.Heavyweight);
                    // STR, TEC, DEF would be very high
                    break;
                case EliteBackground.NCAAChampion:
                    fighter = new Fighter("Daniel", "Cormier", 29, WeightClass.LightHeavyweight);
                    // WREST would be high
                    break;
                case EliteBackground.WorldBJJChampion:
                    fighter = new Fighter("Charles", "Oliveira", 28, WeightClass.Lightweight);
                    // BJJ would be very high
                    break;
                case EliteBackground.FormerUFCFighter:
                    fighter = new Fighter("Conor", "McGregor", 32, WeightClass.Welterweight);
                    // All stats would be high but past prime
                    break;
            }

            return fighter;
        }
        #endregion

        #region Save/Load
        private void LoadFighters()
        {
            // Load from PlayerPrefs or file
            string json = PlayerPrefs.GetString("FighterDatabase", "");
            if (!string.IsNullOrEmpty(json))
            {
                // Deserialize and load fighters
                // This would use JSON serialization
            }
        }

        private void SaveFighters()
        {
            // Save to PlayerPrefs or file
            // This would use JSON serialization
        }
        #endregion

        #region Name Generation
        private string GetRandomFirstName()
        {
            string[] firstNames = {
                "John", "Mike", "David", "Chris", "Alex", "Marcus", "Daniel", "Ryan",
                "Carlos", "Jorge", "Anderson", "Fabricio", "Jose", "Max", "Kamaru",
                "Stipe", "Francis", "Jon", "Daniel", "Justin", "Conor", "Khabib"
            };
            return firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
        }

        private string GetRandomLastName()
        {
            string[] lastNames = {
                "Smith", "Jones", "Williams", "Brown", "Davis", "Miller", "Wilson",
                "Silva", "Santos", "Oliveira", "Souza", "Machida", "Aldo", "Dos Anjos",
                "Miocic", "Ngannou", "Jones", "Cormier", "Gaethje", "McGregor", "Nurmagomedov"
            };
            return lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
        }
        #endregion

        #region Debug
        private void OnGUI()
        {
            if (Debug.isDebugBuild)
            {
                GUILayout.Label($"Fighters: {allFighters.Count}/{maxFighters}");
            }
        }
        #endregion
    }

    #region Enums
    public enum EliteBackground
    {
        OlympicWrestler,      // 올림픽 레슬링 메달리스트
        WorldChampionBoxer,   // 세계 복싱 챔피언
        NCAAChampion,         // NCAA 레슬링 챔피언
        WorldBJJChampion,     // 세계 주짓수 챔피언
        FormerUFCFighter      // 전 UFC 파이터
    }
    #endregion
}