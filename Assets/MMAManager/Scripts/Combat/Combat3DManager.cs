using UnityEngine;
using MMAManager.Models;
using MMAManager.UI;

namespace MMAManager.Combat
{
    public class Combat3DManager : MonoBehaviour
    {
        [Header("Fighters")]
        public Fighter fighter1;
        public Fighter fighter2;

        [Header("3D Objects")]
        public GameObject fighter1Obj;
        public GameObject fighter2Obj;

        [Header("Fight Settings")]
        public bool autoStartFight = true;
        public float fighterSpacing = 3.5f;

        private FighterAgent agent1;
        private FighterAgent agent2;
        private RoundManager roundManager;
        private bool fightInitialized = false;

        public FighterAgent Fighter1 => agent1;
        public FighterAgent Fighter2 => agent2;
        public RoundManager RoundManager => roundManager;

        void Start()
        {
            // Ensure hit effects manager exists
            if (FindObjectOfType<HitEffectManager>() == null)
                gameObject.AddComponent<HitEffectManager>();

            // Ensure combat sound manager exists
            if (FindObjectOfType<CombatSoundManager>() == null)
                gameObject.AddComponent<CombatSoundManager>();

            EnsureFighterData();
            InitializeFight();

            if (autoStartFight)
                StartFight();
        }

        private void EnsureFighterData()
        {
            if (fighter1 == null || string.IsNullOrEmpty(fighter1.FirstName))
            {
                fighter1 = new Fighter("John", "Smith", 28, WeightClass.Middleweight);
                fighter1.RandomizeStats();
            }
            if (fighter2 == null || string.IsNullOrEmpty(fighter2.FirstName))
            {
                fighter2 = new Fighter("Mike", "Johnson", 30, WeightClass.Middleweight);
                fighter2.RandomizeStats();
            }
        }

        public void InitializeFight()
        {
            if (fightInitialized) return;

            if (fighter1Obj == null || fighter2Obj == null)
            {
                Debug.LogError("[Combat3DManager] Fighter objects not assigned!");
                return;
            }

            // Position fighters facing each other
            fighter1Obj.transform.position = new Vector3(-fighterSpacing / 2f, 0f, 0f);
            fighter2Obj.transform.position = new Vector3(fighterSpacing / 2f, 0f, 0f);
            fighter1Obj.transform.LookAt(fighter2Obj.transform);
            fighter2Obj.transform.LookAt(fighter1Obj.transform);

            // Round manager
            roundManager = GetComponent<RoundManager>();
            if (roundManager == null)
                roundManager = gameObject.AddComponent<RoundManager>();

            roundManager.OnRoundStart += OnRoundStart;
            roundManager.OnRoundEnd += OnRoundEnd;
            roundManager.OnFightEnd += OnFightEnd;

            // Fighter agents
            agent1 = fighter1Obj.GetComponent<FighterAgent>();
            if (agent1 == null)
                agent1 = fighter1Obj.AddComponent<FighterAgent>();

            agent2 = fighter2Obj.GetComponent<FighterAgent>();
            if (agent2 == null)
                agent2 = fighter2Obj.AddComponent<FighterAgent>();

            agent1.Initialize(fighter1, agent2);
            agent2.Initialize(fighter2, agent1);

            agent1.OnKnockout += () => OnKnockout(fighter1, fighter2);
            agent2.OnKnockout += () => OnKnockout(fighter2, fighter1);

            Debug.Log($"=== FIGHT CARD ===");
            Debug.Log($"{fighter1.FullName} vs {fighter2.FullName}");
            Debug.Log($"  [{fighter1.FirstName}] HP:{agent1.maxHP} STR:{fighter1.STR} TEC:{fighter1.TEC} SPD:{fighter1.SPD} DEF:{fighter1.DEF} WREST:{fighter1.WREST}");
            Debug.Log($"  [{fighter2.FirstName}] HP:{agent2.maxHP} STR:{fighter2.STR} TEC:{fighter2.TEC} SPD:{fighter2.SPD} DEF:{fighter2.DEF} WREST:{fighter2.WREST}");

            // Fight HUD
            FightHUD hud = FindObjectOfType<FightHUD>();
            if (hud == null)
            {
                GameObject hudObj = new GameObject("FightHUD");
                hud = hudObj.AddComponent<FightHUD>();
            }
            hud.Initialize(agent1, agent2, roundManager);

            // Combat Camera
            CombatCamera combatCam = FindObjectOfType<CombatCamera>();
            if (combatCam == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                    combatCam = mainCam.gameObject.AddComponent<CombatCamera>();
            }
            if (combatCam != null)
            {
                combatCam.Initialize(fighter1Obj.transform, fighter2Obj.transform);
                combatCam.SetMode(CameraMode.RoundStart);
            }

            fightInitialized = true;
        }

        public void StartFight()
        {
            if (!fightInitialized)
            {
                Debug.LogError("[Combat3DManager] Fight not initialized!");
                return;
            }

            agent1.isFighting = true;
            agent2.isFighting = true;
            roundManager.StartFight();
            Debug.Log(">>> FIGHT! <<<");
        }

        private void OnRoundStart(int round)
        {
            Debug.Log($"=== ROUND {round} ===");
            if (agent1 != null) agent1.isFighting = true;
            if (agent2 != null) agent2.isFighting = true;

            var combatCam = CombatCamera.Instance;
            if (combatCam != null) combatCam.SetMode(CameraMode.RoundStart);
        }

        private void OnRoundEnd(int round)
        {
            Debug.Log($"--- Round {round} END ---");
            Debug.Log($"  {fighter1.FirstName}: {agent1.currentHP}/{agent1.maxHP} HP");
            Debug.Log($"  {fighter2.FirstName}: {agent2.currentHP}/{agent2.maxHP} HP");

            if (agent1 != null) agent1.isFighting = false;
            if (agent2 != null) agent2.isFighting = false;
        }

        private bool endedByKO = false;

        private void OnFightEnd()
        {
            if (agent1 != null) agent1.isFighting = false;
            if (agent2 != null) agent2.isFighting = false;

            if (endedByKO) return;

            string result;
            if (agent1.currentHP > agent2.currentHP)
                result = $"Winner: {fighter1.FullName} (Decision)";
            else if (agent2.currentHP > agent1.currentHP)
                result = $"Winner: {fighter2.FullName} (Decision)";
            else
                result = "DRAW";

            Debug.Log($"=== FIGHT OVER ===");
            Debug.Log($"  {fighter1.FirstName}: {agent1.currentHP}/{agent1.maxHP} HP");
            Debug.Log($"  {fighter2.FirstName}: {agent2.currentHP}/{agent2.maxHP} HP");
            Debug.Log($"  {result}");
        }

        private void OnKnockout(Fighter loser, Fighter winner)
        {
            endedByKO = true;
            Debug.Log($"*** KO! {loser.FullName} is knocked out! ***");
            Debug.Log($"*** Winner by KO: {winner.FullName} ***");
            roundManager.EndFight();
        }
    }
}
