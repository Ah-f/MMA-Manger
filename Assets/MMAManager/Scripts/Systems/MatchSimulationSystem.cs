using System;
using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// Core match simulation engine for MMA fights
    /// Simulates fights round by round with realistic striking, grappling, and submissions
    /// </summary>
    public class MatchSimulationSystem : MonoBehaviour
    {
        private static MatchSimulationSystem _instance;
        public static MatchSimulationSystem Instance => _instance;

        [Header("Simulation Settings")]
        [SerializeField] private bool detailedLogging = true;
        [SerializeField] private float simulationSpeed = 1.0f;
        
        // Match state
        private Match currentMatch;
        private int currentRound;
        private float roundTime;
        private Fighter fighter1;
        private Fighter fighter2;
        
        // Fighter states
        private FighterState f1State;
        private FighterState f2State;
        
        // Events log
        private List<MatchEvent> matchEvents;
        
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
        /// Simulate a complete match between two fighters
        /// </summary>
        public MatchResult SimulateMatch(Fighter f1, Fighter f2, MatchEventType eventType = MatchEventType.RegularFight)
        {
            // Initialize match
            currentMatch = new Match(f1, f2, eventType);
            fighter1 = f1;
            fighter2 = f2;
            
            // Initialize fighter states
            f1State = new FighterState(f1);
            f2State = new FighterState(f2);
            
            matchEvents = new List<MatchEvent>();
            
            if (detailedLogging) Debug.Log($"=== MATCH: {f1.DisplayName} vs {f2.DisplayName} ===");
            
            // Simulate each round
            bool fightFinished = false;
            for (currentRound = 1; currentRound <= 3; currentRound++) // 3 rounds standard
            {
                Round round = SimulateRound(currentRound);
                currentMatch.rounds.Add(round);
                
                // Check if fight ended
                if (round.endedByKO || round.endedBySubmission)
                {
                    fightFinished = true;
                    break;
                }
            }
            
            // Determine result
            MatchResult result = DetermineResult(fightFinished);
            currentMatch.result = result;
            
            // Apply results to fighters
            ApplyMatchResults(result);
            
            if (detailedLogging)
            {
                Debug.Log($"=== WINNER: {result.winner.DisplayName} by {result.method} ===");
            }
            
            return result;
        }

        /// <summary>
        /// Simulate a single round
        /// </summary>
        private Round SimulateRound(int roundNum)
        {
            Round round = new Round { roundNumber = roundNum };
            roundTime = 0;
            
            // Reset positions
            f1State.position = FighterPosition.Standing;
            f2State.position = FighterPosition.Standing;
            
            if (detailedLogging) Debug.Log($"--- Round {roundNum} ---");
            
            // Simulate round in segments
            while (roundTime < 300f) // 5 minutes
            {
                float segmentTime = UnityEngine.Random.Range(5f, 30f);
                roundTime = Mathf.Min(roundTime + segmentTime, 300f);
                
                // Determine action based on fighter stats and positions
                MatchAction action = DetermineNextAction(round);
                
                // Execute action
                ActionResult result = ExecuteAction(action, round);
                
                round.events.Add(result);
                
                // Check for stoppage
                if (result.knockdown || result.submissionClose)
                {
                    // Check if fight should end
                    if (ShouldStopFight(result))
                    {
                        if (result.knockdown)
                        {
                            round.endedByKO = true;
                            round.koWinner = result.initiator;
                        }
                        else if (result.submissionClose)
                        {
                            round.endedBySubmission = true;
                            round.submissionWinner = result.initiator;
                        }
                        break;
                    }
                }
            }
            
            // Score the round
            round.roundWinner = ScoreRound(round);
            
            return round;
        }

        /// <summary>
        /// Determine what action happens next based on fighters' stats and positions
        /// </summary>
        private MatchAction DetermineNextAction(Round round)
        {
            MatchAction action = new MatchAction();
            
            // Determine who initiates based on aggression and stats
            float f1Aggression = CalculateAggression(fighter1, f1State);
            float f2Aggression = CalculateAggression(fighter2, f2State);
            
            Fighter initiator = f1Aggression + UnityEngine.Random.Range(-10, 10) > 
                              f2Aggression + UnityEngine.Random.Range(-10, 10) ? fighter1 : fighter2;
            
            action.initiator = initiator;
            action.defender = initiator == fighter1 ? fighter2 : fighter1;
            
            // Determine action type based on positions and stats
            if (f1State.position == FighterPosition.Standing && f2State.position == FighterPosition.Standing)
            {
                action = DetermineStrikingAction(action, round);
            }
            else if (f1State.position == FighterPosition.Grounded || f2State.position == FighterPosition.Grounded)
            {
                action = DetermineGrapplingAction(action, round);
            }
            
            action.timestamp = roundTime;
            
            return action;
        }

        private MatchAction DetermineStrikingAction(MatchAction action, Round round)
        {
            // Calculate striking advantage
            Fighter striker = action.initiator;
            Fighter defender = action.defender;
            
            float strikingOdds = (striker.TEC * 0.4f + striker.SPD * 0.3f + striker.STR * 0.3f) -
                                (defender.DEF * 0.3f + defender.SPD * 0.3f);
            
            strikingOdds += UnityEngine.Random.Range(-20, 20);
            
            if (strikingOdds > 30)
            {
                action.actionType = ActionType.Striking_Combo;
                action.damage = UnityEngine.Random.Range(5, 15);
            }
            else if (strikingOdds > 10)
            {
                action.actionType = ActionType.Striking_Single;
                action.damage = UnityEngine.Random.Range(2, 8);
            }
            else if (strikingOdds > -10)
            {
                // Could lead to clinch or takedown attempt
                float takedownChance = striker.WREST * 0.5f + striker.BJJ * 0.2f;
                if (UnityEngine.Random.value * 100 < takedownChance)
                {
                    action.actionType = ActionType.TakedownAttempt;
                    action.damage = 0;
                }
                else
                {
                    action.actionType = ActionType.Clinch;
                    action.damage = UnityEngine.Random.Range(1, 5);
                }
            }
            else
            {
                action.actionType = ActionType.Striking_Miss;
                action.damage = 0;
            }
            
            // Check for knockout potential
            if (action.actionType == ActionType.Striking_Combo && striker.STR > 70)
            {
                float koChance = (striker.STR - 50) * 0.01f;
                if (UnityEngine.Random.value < koChance)
                {
                    action.knockdownChance = koChance;
                }
            }
            
            return action;
        }

        private MatchAction DetermineGrapplingAction(MatchAction action, Round round)
        {
            Fighter attacker = action.initiator;
            Fighter defender = action.defender;
            
            // Check positions
            bool f1Grounded = f1State.position == FighterPosition.Grounded;
            Fighter groundedFighter = f1Grounded ? fighter1 : fighter2;
            Fighter standingFighter = f1Grounded ? fighter2 : fighter1;
            
            // If one fighter is on bottom
            if (f1State.isGuardBottom || f2State.isGuardBottom)
            {
                // Ground and pound or submission attempt
                float grapplingAdvantage = attacker.WREST * 0.4f + attacker.BJJ * 0.4f;
                grapplingAdvantage -= defender.DEF * 0.3f + defender.BJJ * 0.2f;
                
                grapplingAdvantage += UnityEngine.Random.Range(-15, 15);
                
                if (grapplingAdvantage > 20)
                {
                    action.actionType = ActionType.GroundAndPound;
                    action.damage = UnityEngine.Random.Range(3, 10);
                }
                else if (grapplingAdvantage > 5)
                {
                    action.actionType = ActionType.SubmissionAttempt;
                    action.damage = UnityEngine.Random.Range(1, 5);
                    
                    // Set submission type based on fighter's strengths
                    action.submissionType = attacker.BJJ > attacker.WREST ? 
                        SubmissionType.RearNakedChoke : SubmissionType.Guillotine;
                }
                else if (grapplingAdvantage > -10)
                {
                    action.actionType = ActionType.PositionalImprovement;
                    action.damage = 1;
                }
                else
                {
                    action.actionType = ActionType.SweepAttempt;
                    action.damage = 0;
                }
            }
            else
            {
                // Standing grappling - clinch work
                action.actionType = ActionType.Clinch;
                action.damage = UnityEngine.Random.Range(1, 4);
            }
            
            return action;
        }

        /// <summary>
        /// Execute the determined action
        /// </summary>
        private ActionResult ExecuteAction(MatchAction action, Round round)
        {
            ActionResult result = new ActionResult
            {
                initiator = action.initiator,
                defender = action.defender,
                actionType = action.actionType,
                timestamp = action.timestamp
            };
            
            FighterState initiatorState = action.initiator == fighter1 ? f1State : f2State;
            FighterState defenderState = action.defender == fighter1 ? f1State : f2State;
            
            switch (action.actionType)
            {
                case ActionType.Striking_Single:
                case ActionType.Striking_Combo:
                    result.success = UnityEngine.Random.value < 0.7f;
                    if (result.success)
                    {
                        defenderState.stamina -= action.damage * 0.5f;
                        defenderState.health -= action.damage;
                        round.strikesLanded++;
                        
                        if (action.knockdownChance > 0 && UnityEngine.Random.value < action.knockdownChance)
                        {
                            result.knockdown = true;
                            defenderState.isKnockedDown = true;
                        }
                    }
                    break;
                    
                case ActionType.TakedownAttempt:
                    float takedownSuccess = CalculateTakedownSuccess(action.initiator, action.defender);
                    result.success = UnityEngine.Random.value < takedownSuccess;
                    if (result.success)
                    {
                        defenderState.position = FighterPosition.Grounded;
                        defenderState.isGuardBottom = true;
                        initiatorState.position = FighterPosition.Grounded;
                        initiatorState.hasTopPosition = true;
                        round.takedownsLanded++;
                    }
                    break;
                    
                case ActionType.SubmissionAttempt:
                    float subSuccess = CalculateSubmissionSuccess(action.initiator, action.defender, action.submissionType);
                    result.submissionCloseness = subSuccess;
                    result.submissionType = action.submissionType;
                    
                    if (subSuccess > 0.8f)
                    {
                        result.submissionClose = true;
                    }
                    else if (subSuccess > 0.4f)
                    {
                        result.submissionAttempt = true;
                        defenderState.stamina -= subSuccess * 20;
                    }
                    break;
                    
                case ActionType.GroundAndPound:
                    result.success = UnityEngine.Random.value < 0.6f;
                    if (result.success)
                    {
                        defenderState.health -= action.damage;
                        defenderState.stamina -= action.damage * 0.3f;
                        round.strikesLanded++;
                    }
                    break;
                    
                case ActionType.SweepAttempt:
                    float sweepSuccess = CalculateSweepSuccess(action.initiator, action.defender);
                    result.success = UnityEngine.Random.value < sweepSuccess;
                    if (result.success)
                    {
                        // Swap positions
                        initiatorState.hasTopPosition = true;
                        initiatorState.isGuardBottom = false;
                        defenderState.hasTopPosition = false;
                        defenderState.isGuardBottom = true;
                    }
                    break;
            }
            
            return result;
        }

        #region Calculations
        private float CalculateAggression(Fighter fighter, FighterState state)
        {
            float aggression = fighter.STR * 0.3f + fighter.SPD * 0.3f + fighter.TEC * 0.2f;
            
            // Fatigue reduces aggression
            aggression *= (1 - state.fatigue * 0.3f);
            
            // Behind in rounds increases aggression
            // (would need round scoring to implement fully)
            
            return aggression;
        }

        private float CalculateTakedownSuccess(Fighter attacker, Fighter defender)
        {
            float offense = attacker.WREST * 0.6f + attacker.STR * 0.2f + attacker.TEC * 0.2f;
            float defense = defender.WREST * 0.5f + defender.DEF * 0.3f + defender.SPD * 0.2f;
            
            float successChance = (offense - defense + 50) / 150f;
            return Mathf.Clamp(successChance, 0.1f, 0.8f);
        }

        private float CalculateSubmissionSuccess(Fighter attacker, Fighter defender, SubmissionType type)
        {
            float offense = attacker.BJJ * 0.6f + attacker.WREST * 0.2f + attacker.TEC * 0.2f;
            float defense = defender.BJJ * 0.5f + defender.DEF * 0.2f + defender.STR * 0.1f;
            
            // Stamina affects defense
            FighterState defenderState = defender == fighter1 ? f1State : f2State;
            defense *= (1 - defenderState.stamina * 0.5f);
            
            float successChance = (offense - defense + 50) / 150f;
            return Mathf.Clamp(successChance, 0f, 1f);
        }

        private float CalculateSweepSuccess(Fighter attacker, Fighter defender)
        {
            float offense = attacker.BJJ * 0.5f + attacker.WREST * 0.3f;
            float defense = defender.WREST * 0.4f + defender.BJJ * 0.2f + defender.STR * 0.2f;
            
            float successChance = (offense - defense + 40) / 140f;
            return Mathf.Clamp(successChance, 0.1f, 0.7f);
        }

        private bool ShouldStopFight(ActionResult result)
        {
            if (result.knockdown)
            {
                // Additional strikes to downed opponent
                return UnityEngine.Random.value < 0.7f;
            }
            
            if (result.submissionCloseness > 0.8f)
            {
                return true; // Submission locked in
            }
            
            return false;
        }

        private Fighter ScoreRound(Round round)
        {
            // Simplified scoring: strikes + takedowns + control
            int f1Score = round.fighter1SignificantStrikes * 2 + 
                         round.fighter1Takedowns * 3 + 
                         round.fighter1ControlTime / 10;
            
            int f2Score = round.fighter2SignificantStrikes * 2 + 
                         round.fighter2Takedowns * 3 + 
                         round.fighter2ControlTime / 10;
            
            return f1Score > f2Score ? fighter1 : fighter2;
        }

        private MatchResult DetermineResult(bool fightFinished)
        {
            MatchResult result = new MatchResult();
            
            if (fightFinished)
            {
                Round lastRound = currentMatch.rounds[currentMatch.rounds.Count - 1];
                
                if (lastRound.endedByKO)
                {
                    result.winner = lastRound.koWinner;
                    result.loser = result.winner == fighter1 ? fighter2 : fighter1;
                    result.method = VictoryMethod.KO;
                    result.winningRound = currentRound;
                    result.winningTime = roundTime;
                    result.description = $"{result.winner.DisplayName} wins by KO at {FormatTime(roundTime)} of Round {currentRound}";
                }
                else if (lastRound.endedBySubmission)
                {
                    result.winner = lastRound.submissionWinner;
                    result.loser = result.winner == fighter1 ? fighter2 : fighter1;
                    result.method = VictoryMethod.Submission;
                    result.winningRound = currentRound;
                    result.winningTime = roundTime;
                    result.description = $"{result.winner.DisplayName} wins by Submission at {FormatTime(roundTime)} of Round {currentRound}";
                }
            }
            else
            {
                // Go to decision
                int f1RoundsWon = 0;
                int f2RoundsWon = 0;
                
                foreach (var round in currentMatch.rounds)
                {
                    if (round.roundWinner == fighter1) f1RoundsWon++;
                    else if (round.roundWinner == fighter2) f2RoundsWon++;
                }
                
                if (f1RoundsWon > f2RoundsWon)
                {
                    result.winner = fighter1;
                    result.loser = fighter2;
                    result.method = f1RoundsWon == 3 ? VictoryMethod.Decision_Unanimous : VictoryMethod.Decision_Split;
                }
                else if (f2RoundsWon > f1RoundsWon)
                {
                    result.winner = fighter2;
                    result.loser = fighter1;
                    result.method = f2RoundsWon == 3 ? VictoryMethod.Decision_Unanimous : VictoryMethod.Decision_Split;
                }
                else
                {
                    result.method = VictoryMethod.Draw;
                }
                
                result.description = result.method == VictoryMethod.Draw ? 
                    "Fight ends in a Draw" : 
                    $"{result.winner.DisplayName} wins by Decision";
            }
            
            return result;
        }

        private void ApplyMatchResults(MatchResult result)
        {
            if (result.winner != null && result.loser != null)
            {
                if (result.method == VictoryMethod.KO || result.method == VictoryMethod.TKO)
                {
                    result.winner.AddWin(knockout: true, submission: false);
                    result.loser.AddLoss();
                }
                else if (result.method == VictoryMethod.Submission)
                {
                    result.winner.AddWin(knockout: false, submission: true);
                    result.loser.AddLoss();
                }
                else if (result.method != VictoryMethod.Draw)
                {
                    result.winner.AddWin(knockout: false, submission: false);
                    result.loser.AddLoss();
                }
            }
            
            // Update popularity based on performance
            // (would be implemented with more detail)
        }

        private string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{mins}:{secs:D2}";
        }
        #endregion
    }

    #region Supporting Classes
    public class FighterState
    {
        public FighterPosition position;
        public float health = 100f;
        public float stamina = 100f;
        public float fatigue = 0f;
        public bool isGuardBottom = false;
        public bool hasTopPosition = false;
        public bool isKnockedDown = false;
        
        public FighterState(Fighter fighter)
        {
            // Initialize from fighter stats
            stamina = fighter.STA;
            health = fighter.Condition;
        }
    }

    public class SimulationRound
    {
        public int roundNumber;
        public List<ActionResult> events = new List<ActionResult>();
        public int strikesLanded;
        public int takedownsLanded;
        public Fighter winner;
        
        public bool endedByKO;
        public Fighter koWinner;
        public bool endedBySubmission;
        public Fighter submissionWinner;
        
        // Stats
        public int fighter1SignificantStrikes;
        public int fighter2SignificantStrikes;
        public int fighter1Takedowns;
        public int fighter2Takedowns;
        public int fighter1ControlTime;
        public int fighter2ControlTime;
    }

    public class MatchAction
    {
        public Fighter initiator;
        public Fighter defender;
        public ActionType actionType;
        public float damage;
        public float timestamp;
        public float knockdownChance;
        public SubmissionType submissionType;
        public float submissionCloseness;
    }

    public class ActionResult
    {
        public Fighter initiator;
        public Fighter defender;
        public ActionType actionType;
        public bool success;
        public float damage;
        public float timestamp;
        public bool knockdown;
        public bool submissionAttempt;
        public bool submissionClose;
        public float submissionCloseness;
        public SubmissionType submissionType;
    }

    public class MatchEvent
    {
        public float timestamp;
        public string description;
        public Fighter primaryFighter;
        public EventType eventType;
    }
    #endregion

    #region Enums
    public enum FighterPosition
    {
        Standing,
        Clinch,
        Grounded,
        Guard_Top,
        Guard_Bottom,
        Mount_Top,
        Mount_Bottom,
        Back_Top,
        Back_Bottom
    }

    public enum ActionType
    {
        Striking_Single,
        Striking_Combo,
        Striking_Miss,
        TakedownAttempt,
        Clinch,
        SubmissionAttempt,
        GroundAndPound,
        PositionalImprovement,
        SweepAttempt
    }

    public enum EventType
    {
        Strike,
        Takedown,
        Submission,
        Knockdown,
        Injury,
        Timeout
    }
    #endregion
}