using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMAManager.Models;
using MMAManager.Visual;
using MMAManager.Systems;
using MMAManager.Combat;
using MMAManager.UI;

namespace MMAManager.Test
{
    /// <summary>
    /// 3D 폴더의 MMA 에셋을 테스트하는 스크립트
    /// Remy 모델과 MMA 애니메이션을 사용
    /// </summary>
    public class MMAAssetsTest : MonoBehaviour
    {
        [Header("Models")]
        [Tooltip("여기에 Remy.fbx를 드래그앤드롭하세요")]
        [SerializeField] private GameObject remyModel;

        [Header("Fighters")]
        [NonSerialized]
        private Fighter fighter1;
        [NonSerialized]
        private Fighter fighter2;
        private GameObject fighter1Obj;
        private GameObject fighter2Obj;

        [Header("Animation States")]
        private Animator animator1;
        private Animator animator2;
        private RuntimeAnimatorController animatorController;

        [Header("Combat")]
        private Combat3DManager combatManager;
        private FightHUD fightHUD;
        private bool combatMode = false;
        private bool hasStartedFight = false;

        private Dictionary<int, string> AnimationNameMapping = new Dictionary<int, string>
        {
            { 0, "Idle" },
            { 1, "Jab Cross" },
            { 2, "Hook" },
            { 3, "Mma Kick" },
            { 4, "Double Leg Takedown - Attacker" },
            { 5, "Head Hit" },
            { 6, "Hit To Body" },
            { 7, "Defeated" },
            { 8, "Center Block" },
            { 9, "Combo Punch" }
        };

        void Start()
        {
            Debug.Log("=== MMA ASSETS TEST START ===");
 
            // LoadAnimatorController();
 
            // LoadRemyModel();
 
            // CreateTestFighters();
 
            // CreateFighterObjects();
 
            // SetupCamera();
 
            // SetupCombatSystem();
             
            Debug.Log("=== SETUP COMPLETE ===");
            // LogControls();
            
            // 자동으로 바로 시작
            if (combatManager != null)
            {
                Debug.Log("Starting auto combat...");
                combatManager.StartFight();
            }
        }
        
        void StartAutoCombat()
        {
            if (combatManager != null)
            {
                Debug.Log("Starting auto combat...");
                combatManager.InitializeFight();
                combatManager.StartFight();
            }
            else
            {
                Debug.LogError("Combat Manager is null!");
            }
        }
        
        void Update()
        {
            // 자동 시작 (한 번만)
            if (!hasStartedFight)
            {
                Debug.Log($"[Update] hasStartedFight={hasStartedFight}, combatManager={combatManager != null}");
                
                if (combatManager != null)
                {
                    hasStartedFight = true;
                    Debug.Log("[Update] Auto-starting fight...");
                    combatManager.InitializeFight();
                    combatManager.StartFight();
                }
                else
                {
                    // 계속 찾기
                    if (Time.frameCount % 60 == 0)
                    {
                        combatManager = FindObjectOfType<Combat3DManager>();
                        Debug.Log($"[Update] Still looking for combatManager... found={combatManager != null}");
                    }
                }
            }

            // 자동 싸움 모드 토글
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCombatMode();
            }

            if (!combatMode)
            {
                // Fighter 1 애니메이션 (Q~P)
                if (Input.GetKeyDown(KeyCode.Q)) PlayAnimation(1, 1);
                if (Input.GetKeyDown(KeyCode.W)) PlayAnimation(1, 2);
                if (Input.GetKeyDown(KeyCode.E)) PlayAnimation(1, 3);
                if (Input.GetKeyDown(KeyCode.R)) RestartTest();
                if (Input.GetKeyDown(KeyCode.T)) PlayAnimation(1, 4);
                if (Input.GetKeyDown(KeyCode.Y)) PlayAnimation(1, 5);
                if (Input.GetKeyDown(KeyCode.U)) PlayAnimation(1, 6);
                if (Input.GetKeyDown(KeyCode.I)) PlayAnimation(1, 7);
                if (Input.GetKeyDown(KeyCode.O)) PlayAnimation(1, 8);
                if (Input.GetKeyDown(KeyCode.P)) PlayAnimation(1, 9);

                // Fighter 2 애니메이션 (A~L)
                if (Input.GetKeyDown(KeyCode.A)) PlayAnimation(2, 1);
                if (Input.GetKeyDown(KeyCode.S)) PlayAnimation(2, 2);
                if (Input.GetKeyDown(KeyCode.D)) PlayAnimation(2, 3);
                if (Input.GetKeyDown(KeyCode.F)) PlayAnimation(2, 4);
                if (Input.GetKeyDown(KeyCode.G)) PlayAnimation(2, 5);
                if (Input.GetKeyDown(KeyCode.H)) PlayAnimation(2, 6);
                if (Input.GetKeyDown(KeyCode.J)) PlayAnimation(2, 7);
                if (Input.GetKeyDown(KeyCode.K)) PlayAnimation(2, 8);
                if (Input.GetKeyDown(KeyCode.L)) PlayAnimation(2, 9);

                // Idle
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    PlayAnimation(1, 0);
                    PlayAnimation(2, 0);
                }
            }
        }

        private void ToggleCombatMode()
        {
            combatMode = !combatMode;
            Debug.Log($"Combat Mode: {(combatMode ? "AUTO" : "MANUAL")}");

            if (combatMode)
            {
                if (combatManager != null)
                {
                    Debug.Log("Combat Manager found, starting fight...");
                    combatManager.StartFight();
                }
                else
                {
                    Debug.LogError("Combat Manager is null! Cannot start fight.");
                }
            }
        }

        private void PlayAnimation(int fighterIndex, int animIndex)
        {
            Animator animator = fighterIndex == 1 ? animator1 : animator2;

            if (AnimationNameMapping.TryGetValue(animIndex, out string animName))
            {
                if (animator != null)
                {
                    animator.Play(animName, 0, 0f);
                    Debug.Log($"Fighter {fighterIndex}: {animName}");
                }
            }
        }

        private void RestartTest()
        {
            combatMode = false;

            if (fighter1Obj != null) Destroy(fighter1Obj);
            if (fighter2Obj != null) Destroy(fighter2Obj);

            if (combatManager != null) Destroy(combatManager.gameObject);
            if (fightHUD != null) Destroy(fightHUD.gameObject);

            Start();
        }

        void OnGUI()
        {
            int yPos = 10;
            GUILayout.BeginArea(new Rect(10, yPos, 350, 700));
            GUILayout.Box("MMA Assets Test");

            GUILayout.Space(10);
            GUILayout.Label($"Model: {(remyModel != null ? "Remy (Loaded)" : "None")}");
            GUILayout.Label($"Fighter 1: {fighter1?.DisplayName ?? "N/A"}");
            GUILayout.Label($"Fighter 2: {fighter2?.DisplayName ?? "N/A"}");
            GUILayout.Label($"Mode: {(combatMode ? "AUTO COMBAT" : "MANUAL")}");

            GUILayout.Space(10);
            if (!combatMode)
            {
                GUILayout.Label("=== Fighter 1 Controls (Q-P) ===");
                GUILayout.Label("Q - Jab Cross");
                GUILayout.Label("W - Hook");
                GUILayout.Label("E - MMA Kick");
                GUILayout.Label("T - Takedown");
                GUILayout.Label("Y - Head Hit");
                GUILayout.Label("U - Hit To Body");
                GUILayout.Label("I - Defeated (Knockdown)");
                GUILayout.Label("O - Center Block");
                GUILayout.Label("P - Combo Punch");

                GUILayout.Space(10);
                GUILayout.Label("=== Fighter 2 Controls (A-L) ===");
                GUILayout.Label("A~L - Same animations");

                GUILayout.Space(10);
                GUILayout.Label("SPACE - Both Idle");
                GUILayout.Label("R - Restart");
            }
            else
            {
                GUILayout.Label("=== AUTO COMBAT MODE ===");
                GUILayout.Label("Fighters are fighting automatically");
                GUILayout.Label("AI makes decisions based on stats");
                GUILayout.Label("C - Toggle back to Manual");
            }

            GUILayout.Space(10);
            GUILayout.Label("C - Toggle Combat Mode");
            
            if (GUILayout.Button("Start Auto Combat"))
            {
                if (!combatMode)
                {
                    ToggleCombatMode();
                }
            }
            
            if (GUILayout.Button("Stop Auto Combat"))
            {
                if (combatMode)
                {
                    ToggleCombatMode();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("=== Available Animations ===");
            GUILayout.Label("• Idle");
            GUILayout.Label("• Jab Cross (1-4)");
            GUILayout.Label("• Hook (1-4)");
            GUILayout.Label("• MMA Kick");
            GUILayout.Label("• Body Jab Cross");
            GUILayout.Label("• Combo Punch");
            GUILayout.Label("• Flying Knee Punch Combo");
            GUILayout.Label("• Drop Kick");
            GUILayout.Label("• Double Leg Takedown (Attacker/Victim)");
            GUILayout.Label("• Head Hit / Big Hit To Head");
            GUILayout.Label("• Hit To Body / Stomach Hit");
            GUILayout.Label("• Center Block / Left Block / Right Block");
            GUILayout.Label("• Defeated");
            GUILayout.Label("• Walking / Steps");

            GUILayout.EndArea();
        }

        // 애니메이션 이름 리스트 (참고용)
        private string[] AvailableAnimations = new string[]
        {
            "Idle",
            "Jab Cross", "Jab Cross (1)", "Jab Cross (2)", "Lead Jab (1-4)",
            "Hook", "Hook (1-4)",
            "Mma Kick", "Mma Kick (1)",
            "Body Jab Cross", "Body Jab Cross (1-2)",
            "Combo Punch", "Punch Combo",
            "Flying Knee Punch Combo",
            "Drop Kick",
            "Double Leg Takedown - Attacker",
            "Double Leg Takedown - Victim", "Double Leg Takedown - Victim (1)",
            "Head Hit", "Head Hit (1-4)",
            "Light Hit To Head", "Light Hit To Head (1-2)",
            "Medium Hit To Head", "Medium Hit To Head (1-3)",
            "Big Hit To Head",
            "Hit To Body", "Hit To Body (1-3)",
            "Stomach Hit", "Big Stomach Hit",
            "Kidney Hit", "Big Kidney Hit",
            "Rib Hit", "Big Rib Hit",
            "Side Hit", "Big Side Hit",
            "Center Block", "Left Block", "Left Block (1)", "Right Block",
            "Defeated",
            "Walking Backwards", "Walking Backwards (1)",
            "Long Step Forward", "Short Step Forward",
            "Step Backward", "Step Backward (1-2)",
            "Long Left Side Step", "Medium Left Side Step", "Short Left Side Step",
            "Short Right Side Step",
            "Receiving An Uppercut", "Receiving A Big Uppercut",
            "Capoeira", "Taunt",
            "Illegal Elbow Punch", "Illegal Knee", "Illegal Headbutt"
        };
    }
}
