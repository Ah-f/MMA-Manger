using UnityEngine;
using MMAManager.Models;
using MMAManager.Systems;

namespace MMAManager.Test
{
    public class SimpleFighterTest : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== FIGHTER TEST START ===");
            
            // Create two fighters
            Fighter fighter1 = new Fighter("Korean", "Zombie", 28, WeightClass.Lightweight);
            Fighter fighter2 = new Fighter("Islam", "Makhachev", 27, WeightClass.Lightweight);
            
            // Set their stats
            fighter1.SetStat(StatType.Strength, 85);
            fighter1.SetStat(StatType.Technique, 88);
            fighter1.SetStat(StatType.Speed, 82);
            fighter1.SetStat(StatType.Stamina, 75);
            fighter1.SetStat(StatType.Defense, 70);
            fighter1.SetStat(StatType.Wrestling, 40);
            fighter1.SetStat(StatType.BJJ, 45);
            fighter1.SetStat(StatType.Potential, 90);
            
            fighter2.SetStat(StatType.Strength, 75);
            fighter2.SetStat(StatType.Technique, 85);
            fighter2.SetStat(StatType.Speed, 78);
            fighter2.SetStat(StatType.Stamina, 80);
            fighter2.SetStat(StatType.Defense, 75);
            fighter2.SetStat(StatType.Wrestling, 88);
            fighter2.SetStat(StatType.BJJ, 85);
            fighter2.SetStat(StatType.Potential, 88);
            
            Debug.Log($"FIGHTER 1: {fighter1.DisplayName}");
            Debug.Log($"  STR: {fighter1.STR} | TEC: {fighter1.TEC} | SPD: {fighter1.SPD} | STA: {fighter1.STA}");
            Debug.Log($"  WREST: {fighter1.WREST} | BJJ: {fighter1.BJJ} | Overall: {fighter1.Overall}");
            Debug.Log($"  Ready to fight: {fighter1.IsReadyToFight()}");
            
            Debug.Log($"\nFIGHTER 2: {fighter2.DisplayName}");
            Debug.Log($"  STR: {fighter2.STR} | TEC: {fighter2.TEC} | SPD: {fighter2.SPD} | STA: {fighter2.STA}");
            Debug.Log($"  WREST: {fighter2.WREST} | BJJ: {fighter2.BJJ} | Overall: {fighter2.Overall}");
            Debug.Log($"  Ready to fight: {fighter2.IsReadyToFight()}");
            
            // Test training
            Debug.Log($"\n=== TRAINING TEST ===");
            fighter1.TrainStat(StatType.Strength, 5);
            Debug.Log($"{fighter1.FullName} trained STR +5 -> New STR: {fighter1.STR}");
            Debug.Log($"Fatigue increased to: {fighter1.Fatigue}");
            Debug.Log($"Fatigue increased to: {fighter1.Fatigue}");
            
            // Test recovery
            fighter1.Recover(20);
            Debug.Log($"After recovery - Condition: {fighter1.Condition}, Fatigue: {fighter1.Fatigue}");
            
            Debug.Log("\n=== FIGHTER TEST COMPLETE ===");
        }
    }
}