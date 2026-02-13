using UnityEngine;
using MMAManager.Models;
using MMAManager.Systems;

namespace MMAManager.Test
{
    /// <summary>
    /// Test script for Match Simulation System
    /// Attach this to a GameObject to test the simulation
    /// </summary>
    public class MatchSimulationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool autoRunOnStart = true;
        [SerializeField] private int numberOfTests = 1;
        
        private MatchSimulationSystem simulationSystem;

        private void Start()
        {
            simulationSystem = MatchSimulationSystem.Instance;
            
            if (autoRunOnStart)
            {
                Invoke(nameof(RunTest), 1f);
            }
        }

        private void Update()
        {
            // Press Space to run a test match
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RunTest();
            }
            
            // Press T to run 10 test matches
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunMultipleTests(10);
            }
        }

        public void RunTest()
        {
            Debug.Log("========================================");
            Debug.Log("        MATCH SIMULATION TEST");
            Debug.Log("========================================");
            
            // Create two test fighters
            Fighter fighter1 = CreateTestFighter("Korean", "Zombie", 28, WeightClass.Lightweight);
            Fighter fighter2 = CreateTestFighter("Islam", "Makhachev", 27, WeightClass.Lightweight);
            
            // Customize stats for more interesting matchups
            CustomizeFighterStats(fighter1, FighterType.Striker);
            CustomizeFighterStats(fighter2, FighterType.Grappler);
            
            Debug.Log($"\nFIGHTER 1: {fighter1.DisplayName}");
            Debug.Log($"  STR: {fighter1.STR} | TEC: {fighter1.TEC} | SPD: {fighter1.SPD} | STA: {fighter1.STA}");
            Debug.Log($"  WREST: {fighter1.WREST} | BJJ: {fighter1.BJJ} | Overall: {fighter1.Overall}");
            
            Debug.Log($"\nFIGHTER 2: {fighter2.DisplayName}");
            Debug.Log($"  STR: {fighter2.STR} | TEC: {fighter2.TEC} | SPD: {fighter2.SPD} | STA: {fighter2.STA}");
            Debug.Log($"  WREST: {fighter2.WREST} | BJJ: {fighter2.BJJ} | Overall: {fighter2.Overall}");
            Debug.Log("\n----------------------------------------");
            
            // Run simulation
            MatchResult result = simulationSystem.SimulateMatch(fighter1, fighter2);
            
            Debug.Log("\nFINAL RESULT:");
            Debug.Log($"  Winner: {result.winner?.DisplayName ?? "Draw"}");
            Debug.Log($"  Method: {result.method}");
            Debug.Log($"  Round: {result.winningRound}");
            Debug.Log($"  Time: {FormatTime(result.winningTime)}");
            
            // Show record updates
            Debug.Log($"\nUpdated Records:");
            Debug.Log($"  {fighter1.FullName}: {fighter1.Wins}-{fighter1.Losses}-{fighter1.Draws}");
            Debug.Log($"  {fighter2.FullName}: {fighter2.Wins}-{fighter2.Losses}-{fighter2.Draws}");
            
            Debug.Log("========================================\n");
        }

        public void RunMultipleTests(int count)
        {
            Debug.Log($"Running {count} simulation tests...\n");
            
            int f1Wins = 0;
            int f2Wins = 0;
            int submissions = 0;
            int knockouts = 0;
            int decisions = 0;
            
            Fighter fighter1 = CreateTestFighter("Striker", "One", 26, WeightClass.Welterweight);
            Fighter fighter2 = CreateTestFighter("Grappler", "Two", 26, WeightClass.Welterweight);
            
            CustomizeFighterStats(fighter1, FighterType.Striker);
            CustomizeFighterStats(fighter2, FighterType.Grappler);
            
            for (int i = 0; i < count; i++)
            {
                // Reset records
                ResetFighterRecord(fighter1);
                ResetFighterRecord(fighter2);
                
                MatchResult result = simulationSystem.SimulateMatch(fighter1, fighter2);
                
                if (result.winner == fighter1) f1Wins++;
                if (result.winner == fighter2) f2Wins++;
                
                if (result.method == VictoryMethod.Submission) submissions++;
                else if (result.method == VictoryMethod.KO || result.method == VictoryMethod.TKO) knockouts++;
                else decisions++;
            }
            
            Debug.Log("========================================");
            Debug.Log($"        TEST RESULTS ({count} fights)");
            Debug.Log("========================================");
            Debug.Log($"Striker: {f1Wins} wins ({(float)f1Wins/count*100:F1}%)");
            Debug.Log($"Grappler: {f2Wins} wins ({(float)f2Wins/count*100:F1}%)");
            Debug.Log($"\nFinish Breakdown:");
            Debug.Log($"  KOs: {knockouts} ({(float)knockouts/count*100:F1}%)");
            Debug.Log($"  Submissions: {submissions} ({(float)submissions/count*100:F1}%)");
            Debug.Log($"  Decisions: {decisions} ({(float)decisions/count*100:F1}%)");
            Debug.Log("========================================\n");
        }

        public void RunChampionshipMatch()
        {
            Debug.Log("========================================");
            Debug.Log("        CHAMPIONSHIP MATCH");
            Debug.Log("========================================");
            
            // Create elite fighters
            Fighter champion = CreateEliteFighter("Anderson", "Silva", 33, WeightClass.Middleweight);
            Fighter challenger = CreateEliteFighter("Chris", "Weidman", 29, WeightClass.Middleweight);
            
            Debug.Log($"\nCHAMPION: {champion.DisplayName}");
            Debug.Log($"  Record: {champion.Wins}-{champion.Losses}-{champion.Draws}");
            Debug.Log($"  STR: {champion.STR} | TEC: {champion.TEC} | SPD: {champion.SPD} | STA: {champion.STA}");
            Debug.Log($"  WREST: {champion.WREST} | BJJ: {champion.BJJ} | Overall: {champion.Overall}");
            
            Debug.Log($"\nCHALLENGER: {challenger.DisplayName}");
            Debug.Log($"  Record: {challenger.Wins}-{challenger.Losses}-{challenger.Draws}");
            Debug.Log($"  STR: {challenger.STR} | TEC: {challenger.TEC} | SPD: {challenger.SPD} | STA: {challenger.STA}");
            Debug.Log($"  WREST: {challenger.WREST} | BJJ: {challenger.BJJ} | Overall: {challenger.Overall}");
            Debug.Log("\n----------------------------------------");
            
            MatchResult result = simulationSystem.SimulateMatch(champion, challenger, MatchEventType.TitleFight);
            
            Debug.Log("\nCHAMPIONSHIP RESULT:");
            if (result.winner == champion)
            {
                Debug.Log($"  {champion.FullName} DEFENDS the belt!");
            }
            else
            {
                Debug.Log($"  AND NEW! {challenger.FullName} is the champion!");
            }
            Debug.Log($"  Method: {result.method}");
            Debug.Log("========================================\n");
        }

        private Fighter CreateTestFighter(string firstName, string lastName, int age, WeightClass weightClass)
        {
            Fighter fighter = new Fighter(firstName, lastName, age, weightClass);
            return fighter;
        }

        private Fighter CreateEliteFighter(string firstName, string lastName, int age, WeightClass weightClass)
        {
            Fighter fighter = new Fighter(firstName, lastName, age, weightClass);
            
            // Elite fighters have high stats
            fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(70, 90));
            fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(75, 92));
            fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(70, 88));
            fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(75, 90));
            fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(70, 85));
            fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(65, 85));
            fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(65, 85));
            fighter.SetStat(StatType.Potential, UnityEngine.Random.Range(80, 95));
            
            // Give them some wins
            for (int i = 0; i < UnityEngine.Random.Range(8, 15); i++)
            {
                fighter.AddWin(UnityEngine.Random.value < 0.4f, UnityEngine.Random.value < 0.3f);
            }
            for (int i = 0; i < UnityEngine.Random.Range(0, 3); i++)
            {
                fighter.AddLoss();
            }
            
            return fighter;
        }

        private void CustomizeFighterStats(Fighter fighter, FighterType type)
        {
            switch (type)
            {
                case FighterType.Striker:
                    fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(75, 90));
                    fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(70, 85));
                    fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(75, 88));
                    fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(65, 80));
                    fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(30, 55));
                    fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(25, 50));
                    break;
                    
                case FighterType.Grappler:
                    fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(65, 80));
                    fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(65, 80));
                    fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(70, 85));
                    fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(65, 80));
                    fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(75, 90));
                    fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(75, 90));
                    break;
                    
                case FighterType.Balanced:
                    fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(60, 75));
                    fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(60, 75));
                    break;
                    
                case FighterType.AllRounder:
                    fighter.SetStat(StatType.Strength, UnityEngine.Random.Range(80, 90));
                    fighter.SetStat(StatType.Technique, UnityEngine.Random.Range(80, 90));
                    fighter.SetStat(StatType.Speed, UnityEngine.Random.Range(75, 85));
                    fighter.SetStat(StatType.Stamina, UnityEngine.Random.Range(80, 90));
                    fighter.SetStat(StatType.Defense, UnityEngine.Random.Range(75, 85));
                    fighter.SetStat(StatType.Wrestling, UnityEngine.Random.Range(70, 82));
                    fighter.SetStat(StatType.BJJ, UnityEngine.Random.Range(70, 82));
                    break;
            }
        }

        private void ResetFighterRecord(Fighter fighter)
        {
            for (int i = 0; i < fighter.Wins; i++)
            {
                // Hacky way to reset - in real implementation add proper method
            }
        }

        private string FormatTime(float time)
        {
            if (time <= 0) return "0:00";
            int mins = Mathf.FloorToInt(time / 60);
            int secs = Mathf.FloorToInt(time % 60);
            return $"{mins}:{secs:D2}";
        }
    }

    public enum FighterType
    {
        Striker,
        Grappler,
        Balanced,
        AllRounder
    }
}