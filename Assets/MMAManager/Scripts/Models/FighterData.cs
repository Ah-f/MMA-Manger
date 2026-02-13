using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Models
{
    /// <summary>
    /// ScriptableObject container for fighter data
    /// Use this to create individual fighter assets in the Resources folder
    /// </summary>
    [CreateAssetMenu(fileName = "NewFighter", menuName = "MMA Manager/Fighter Data")]
    public class FighterData : ScriptableObject
    {
        [Header("Basic Information")]
        public string fighterId;
        public string firstName;
        public string lastName;
        public string nickname;
        public int age;
        public WeightClass weightClass;
        public string nationality;
        public Sprite portrait;

        [Header("Core Stats (1-100)")]
        [Range(0, 100)] public int strength;      // STR
        [Range(0, 100)] public int technique;     // TEC
        [Range(0, 100)] public int speed;         // SPD
        [Range(0, 100)] public int stamina;       // STA
        [Range(0, 100)] public int defense;       // DEF
        [Range(0, 100)] public int wrestling;     // WREST
        [Range(0, 100)] public int bjj;           // BJJ
        [Range(0, 100)] public int potential;     // POT

        [Header("Condition")]
        [Range(0, 100)] public int condition;
        [Range(0, 100)] public int fatigue;
        [Range(0, 100)] public int health;

        [Header("Record")]
        public int wins;
        public int losses;
        public int draws;
        public int knockoutWins;
        public int submissionWins;
        public int decisionWins;

        [Header("Development")]
        public int trainingPoints;
        public int totalTrainingWeeks;

        [Header("Contract")]
        public int contractLength;
        public int monthlySalary;
        public int winBonus;
        [Range(0, 100)] public int popularity;

        [Header("Traits")]
        public FightingStyle preferredStyle;
        public FighterTrait traits;

        /// <summary>
        /// Convert this ScriptableObject to a Fighter instance
        /// </summary>
        public Fighter ToFighter()
        {
            return new Fighter
            {
                // This is a simplified conversion
                // In a full implementation, you'd use a proper constructor or copy method
            };
        }

        /// <summary>
        /// Create a FighterData from a Fighter instance
        /// </summary>
        public void CopyFromFighter(Fighter fighter)
        {
            fighterId = fighter.FighterId;
            // Copy other properties...
            // This would be implemented with proper reflection or explicit copying
        }
    }
}