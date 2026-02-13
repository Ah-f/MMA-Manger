using System;
using UnityEngine;
using MMAManager.Models;
using MMAManager.Visual;
using MMAManager.Systems;

namespace MMAManager.Test
{
    /// <summary>
    /// Test script for 3D Fighter visualization
    /// Creates two fighters and an octagon in the scene
    /// </summary>
    public class FighterVisualTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Fighter3DCreator fighterCreator;

        [NonSerialized]
        private Fighter fighter1;
        [NonSerialized]
        private Fighter fighter2;
        private GameObject fighter1Obj;
        private GameObject fighter2Obj;
        private GameObject octagon;

        void Start()
        {
            Debug.Log("=== FIGHTER VISUAL TEST START ===");

            // Create Fighter3DCreator if not assigned
            if (fighterCreator == null)
            {
                GameObject creatorObj = new GameObject("Fighter3DCreator");
                fighterCreator = creatorObj.AddComponent<Fighter3DCreator>();
            }

            fighter1 = CreateTestFighter("Korean", "Zombie", 28, WeightClass.Lightweight);
            fighter2 = CreateTestFighter("Islam", "Makhachev", 27, WeightClass.Lightweight);

            // Set fighter stats
            SetFighterStats(fighter1, 85, 88, 82, 75, 70, 40, 45, 90);
            SetFighterStats(fighter2, 75, 85, 78, 80, 75, 88, 85, 88);

            // Create octagon
            octagon = fighterCreator.CreateOctagon(Vector3.zero);
            Debug.Log("Octagon created at center");

            // Create fighters on opposite sides
            fighter1Obj = fighterCreator.CreateFighter3D(fighter1, new Vector3(-2, 0, 0), true);
            fighter1Obj.transform.rotation = Quaternion.Euler(0, 90, 0);
            Debug.Log($"Fighter 1 created: {fighter1.DisplayName}");

            fighter2Obj = fighterCreator.CreateFighter3D(fighter2, new Vector3(2, 0, 0), false);
            fighter2Obj.transform.rotation = Quaternion.Euler(0, -90, 0);
            Debug.Log($"Fighter 2 created: {fighter2.DisplayName}");

            // Add main camera if not exists
            SetupCamera();

            // Add lighting
            SetupLighting();

            Debug.Log("=== FIGHTER VISUAL TEST COMPLETE ===");
            Debug.Log("Press Space to simulate a punch animation");
        }

        void Update()
        {
            // Test animations with keyboard input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Testing punch animations...");
                if (fighter1Obj != null)
                {
                    FighterAnimator animator1 = fighter1Obj.GetComponent<FighterAnimator>();
                    if (animator1 != null)
                    {
                        animator1.Punch();
                    }
                }
                if (fighter2Obj != null)
                {
                    FighterAnimator animator2 = fighter2Obj.GetComponent<FighterAnimator>();
                    if (animator2 != null)
                    {
                        animator2.Punch();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("Testing kick animations...");
                if (fighter1Obj != null)
                {
                    FighterAnimator animator1 = fighter1Obj.GetComponent<FighterAnimator>();
                    if (animator1 != null)
                    {
                        animator1.Kick();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Testing takedown animation...");
                if (fighter1Obj != null)
                {
                    FighterAnimator animator1 = fighter1Obj.GetComponent<FighterAnimator>();
                    if (animator1 != null)
                    {
                        animator1.Takedown();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("Testing get hit animation...");
                if (fighter2Obj != null)
                {
                    FighterAnimator animator2 = fighter2Obj.GetComponent<FighterAnimator>();
                    if (animator2 != null)
                    {
                        animator2.GetHit();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("Testing knockdown animation...");
                if (fighter2Obj != null)
                {
                    FighterAnimator animator2 = fighter2Obj.GetComponent<FighterAnimator>();
                    if (animator2 != null)
                    {
                        animator2.Knockdown();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Restarting test...");
                RestartTest();
            }
        }

        private Fighter CreateTestFighter(string firstName, string lastName, int age, WeightClass weightClass)
        {
            Fighter fighter = new Fighter(firstName, lastName, age, weightClass);
            return fighter;
        }

        private void SetFighterStats(Fighter fighter, int str, int tec, int spd, int sta, int def, int wrest, int bjj, int pot)
        {
            fighter.SetStat(StatType.Strength, str);
            fighter.SetStat(StatType.Technique, tec);
            fighter.SetStat(StatType.Speed, spd);
            fighter.SetStat(StatType.Stamina, sta);
            fighter.SetStat(StatType.Defense, def);
            fighter.SetStat(StatType.Wrestling, wrest);
            fighter.SetStat(StatType.BJJ, bjj);
            fighter.SetStat(StatType.Potential, pot);
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            mainCamera.transform.position = new Vector3(0, 3, -8);
            mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            Debug.Log("Camera setup complete");
        }

        private void SetupLighting()
        {
            // Check if lights already exist
            Light[] lights = FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                Debug.Log("Lights already exist, skipping setup");
                return;
            }

            // Main directional light (spotlight effect)
            GameObject mainLightObj = new GameObject("MainLight");
            Light mainLight = mainLightObj.AddComponent<Light>();
            mainLight.type = LightType.Spot;
            mainLight.intensity = 2f;
            mainLight.range = 20f;
            mainLight.spotAngle = 45f;
            mainLightObj.transform.position = new Vector3(0, 8, -5);
            mainLightObj.transform.rotation = Quaternion.Euler(45, 0, 0);
            mainLight.color = new Color(1f, 0.95f, 0.9f);

            // Fill light
            GameObject fillLightObj = new GameObject("FillLight");
            Light fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 0.5f;
            fillLight.range = 15f;
            fillLightObj.transform.position = new Vector3(5, 3, 0);
            fillLight.color = new Color(0.8f, 0.9f, 1f);

            // Rim light (from behind)
            GameObject rimLightObj = new GameObject("RimLight");
            Light rimLight = rimLightObj.AddComponent<Light>();
            rimLight.type = LightType.Point;
            rimLight.intensity = 0.3f;
            rimLight.range = 15f;
            rimLightObj.transform.position = new Vector3(0, 2, 5);

            Debug.Log("Lighting setup complete");
        }

        private void RestartTest()
        {
            if (fighter1Obj != null) Destroy(fighter1Obj);
            if (fighter2Obj != null) Destroy(fighter2Obj);
            if (octagon != null) Destroy(octagon);

            Start();
        }

        void OnGUI()
        {
            GUI.skin.box.fontSize = 16;
            GUI.skin.button.fontSize = 14;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Box("Fighter Visual Test Controls");
            GUILayout.Space(10);

            GUILayout.Label($"Fighter 1: {fighter1?.DisplayName ?? "N/A"}");
            GUILayout.Label($"Fighter 2: {fighter2?.DisplayName ?? "N/A"}");
            GUILayout.Space(10);

            GUILayout.Label("Controls:");
            GUILayout.Label("Space - Punch animation");
            GUILayout.Label("K - Kick animation");
            GUILayout.Label("T - Takedown animation");
            GUILayout.Label("H - Get Hit animation");
            GUILayout.Label("N - Knockdown animation");
            GUILayout.Label("R - Restart test");

            GUILayout.Space(20);

            if (GUILayout.Button("Start Match Simulation"))
            {
                StartMatchSimulation();
            }

            GUILayout.EndArea();
        }

        private void StartMatchSimulation()
        {
            if (MatchSimulationSystem.Instance != null)
            {
                Debug.Log("Starting match simulation...");
                MatchResult result = MatchSimulationSystem.Instance.SimulateMatch(fighter1, fighter2);
                Debug.Log($"Result: {result.description}");
            }
            else
            {
                Debug.LogWarning("MatchSimulationSystem instance not found!");
            }
        }
    }
}
