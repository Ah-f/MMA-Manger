using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMAManager.Models
{
    /// <summary>
    /// Represents a single MMA match between two fighters
    /// </summary>
    [Serializable]
    public class Match
    {
        public string matchId;
        public Fighter fighter1;
        public Fighter fighter2;
        public MatchResult result;
        public List<Round> rounds;
        public MatchEventType eventType;
        public int basePurse;
        
        public Match(Fighter f1, Fighter f2, MatchEventType eventType = MatchEventType.RegularFight)
        {
            matchId = Guid.NewGuid().ToString();
            fighter1 = f1;
            fighter2 = f2;
            this.eventType = eventType;
            rounds = new List<Round>();
            SetPurse();
        }
        
        private void SetPurse()
        {
            basePurse = eventType switch
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
        
        public int GetEstimatedAttendance()
        {
            int baseAttendance = 5000;
            int popularityBonus = (fighter1.Popularity + fighter2.Popularity) * 100;
            int eventBonus = (int)eventType * 5000;
            return baseAttendance + popularityBonus + eventBonus;
        }
    }

    /// <summary>
    /// Result of a match
    /// </summary>
    [Serializable]
    public class MatchResult
    {
        public Fighter winner;
        public Fighter loser;
        public VictoryMethod method;
        public int winningRound;
        public float winningTime;
        public string description;
        
        // Score card for decisions
        public int[] fighter1Scores;
        public int[] fighter2Scores;
    }

    /// <summary>
    /// Single round data
    /// </summary>
    [Serializable]
    public class Round
    {
        public int roundNumber;
        public List<StrikeEvent> strikes;
        public List<GrapplingEvent> grapplingEvents;
        public List<SubmissionAttempt> submissionAttempts;
        public Fighter roundWinner;
        
        // Simulation fields
        public List<object> events = new List<object>();
        public int strikesLanded;
        public int takedownsLanded;
        public bool endedByKO;
        public Fighter koWinner;
        public bool endedBySubmission;
        public Fighter submissionWinner; // For scoring
        
        // Round statistics
        public int fighter1SignificantStrikes;
        public int fighter2SignificantStrikes;
        public int fighter1Takedowns;
        public int fighter2Takedowns;
        public int fighter1ControlTime;
        public int fighter2ControlTime;
    }

    #region Events
    [Serializable]
    public class StrikeEvent
    {
        public Fighter striker;
        public StrikeType type;
        public StrikeTarget target;
        public bool significant;
        public bool knockedDown;
        public float damage;
        public float timestamp;
    }

    [Serializable]
    public class GrapplingEvent
    {
        public Fighter initiator;
        public GrapplingAction action;
        public bool successful;
        public float controlTime;
        public float timestamp;
    }

    [Serializable]
    public class SubmissionAttempt
    {
        public Fighter attacker;
        public SubmissionType type;
        public bool finished;
        public float duration;
        public float closenessToFinish; // 0-1
        public float timestamp;
    }
    #endregion

    #region Enums
    public enum VictoryMethod
    {
        KO,                // Knockout
        TKO,               // Technical Knockout
        Submission,        // Submission
        Decision_Unanimous, // Unanimous Decision
        Decision_Split,     // Split Decision
        Decision_Majority,  // Majority Decision
        Draw,              // Draw
        NoContest,         // No Contest
        Disqualification   // Disqualification
    }

    public enum MatchEventType
    {
        RegularFight = 1,
        PrelimFight = 2,
        MainCard = 3,
        CoMainEvent = 4,
        MainEvent = 5,
        TitleFight = 6
    }

    public enum StrikeType
    {
        Jab,
        Cross,
        Hook,
        Uppercut,
        Overhand,
        SpinningBackfist,
        Elbow,
        Knee,
        Kick_Low,
        Kick_Body,
        Kick_Head,
        SpinningKick
    }

    public enum StrikeTarget
    {
        Head,
        Body,
        Legs
    }

    public enum GrapplingAction
    {
        Takedown,
        SingleLeg,
        DoubleLeg,
        Trip,
        Throw,
        Slam,
        GuardPull,
        Sweep,
        PassGuard,
        Mount,
        BackTake,
        GroundAndPound
    }

    public enum SubmissionType
    {
        RearNakedChoke,
        Guillotine,
        TriangleChoke,
        Armbar,
        Kimura,
        Americana,
        Omoplata,
        AnkleLock,
        HeelHook,
        Kneebar,
        DArceChoke,
        AnacondaChoke
    }
    #endregion
}