using System.Collections.Generic;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Systems
{
    /// <summary>
    /// 테스트용: F5=파이터 20명 (반복 가능), F6=DNA 균일값 테스트 10명
    /// </summary>
    public class UMATestSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int spawnCount = 20;
        [SerializeField] private float spacing = 2f;
        [SerializeField] private KeyCode spawnKey = KeyCode.F5;
        [SerializeField] private KeyCode dnaTestKey = KeyCode.F6;

        private List<GameObject> spawnedObjects = new List<GameObject>();
        private List<GameObject> dnaTestObjects = new List<GameObject>();

        void Update()
        {
            if (Input.GetKeyDown(spawnKey))
            {
                ClearSpawned(spawnedObjects);
                SpawnFighters();
            }

            if (Input.GetKeyDown(dnaTestKey))
            {
                ClearSpawned(dnaTestObjects);
                SpawnUniformDnaTest();
            }
        }

        private void ClearSpawned(List<GameObject> list)
        {
            foreach (var go in list)
            {
                if (go != null) Destroy(go);
            }
            list.Clear();
        }

        private void SpawnFighters()
        {
            var generator = UMAFighterGenerator.Instance;
            if (generator == null)
            {
                Debug.LogError("[UMATestSpawner] UMAFighterGenerator not found!");
                return;
            }

            string[] firstNames = {
                "John", "Mike", "Carlos", "Jorge", "Anderson",
                "Fabricio", "Jose", "Max", "Kamaru", "Stipe",
                "Francis", "Jon", "Daniel", "Justin", "Conor",
                "Khabib", "Alex", "Marcus", "Ryan", "Chris"
            };

            string[] lastNames = {
                "Smith", "Jones", "Silva", "Santos", "Oliveira",
                "Souza", "Aldo", "Holloway", "Usman", "Miocic",
                "Ngannou", "Bones", "Cormier", "Gaethje", "McGregor",
                "Eagle", "Pereira", "Davis", "Hall", "Weidman"
            };

            WeightClass[] classes = (WeightClass[])System.Enum.GetValues(typeof(WeightClass));

            // 5열 4행 배치
            int cols = 5;
            for (int i = 0; i < spawnCount; i++)
            {
                int row = i / cols;
                int col = i % cols;
                float x = (col - cols / 2f + 0.5f) * spacing;
                float z = -row * spacing;

                var wc = classes[i % classes.Length];
                var fighter = new Fighter(firstNames[i], lastNames[i], 20 + i, wc);
                fighter.RandomizeStats();

                Vector3 pos = new Vector3(x, 0, z);
                var go = generator.GenerateFighterObject(fighter, pos, Quaternion.identity, (obj) =>
                {
                    Debug.Log($"[UMATestSpawner] Spawned: {fighter.FullName} ({wc})");
                });
                if (go != null) spawnedObjects.Add(go);
            }

            Debug.Log($"[UMATestSpawner] {spawnCount}명 생성 시작!");
        }

        /// <summary>
        /// DNA 전체를 0.1, 0.2, ... 1.0으로 통일한 캐릭터 10명 생성
        /// </summary>
        private void SpawnUniformDnaTest()
        {
            var generator = UMAFighterGenerator.Instance;
            if (generator == null)
            {
                Debug.LogError("[UMATestSpawner] UMAFighterGenerator not found!");
                return;
            }

            // 10명: DNA 0.1 ~ 1.0 (한 줄로 배치)
            for (int i = 0; i < 10; i++)
            {
                float dnaValue = (i + 1) * 0.1f;
                float x = (i - 4.5f) * spacing;
                Vector3 pos = new Vector3(x, 0, 5f);

                var go = generator.GenerateUniformDnaCharacter(dnaValue, pos, Quaternion.identity);
                if (go != null) dnaTestObjects.Add(go);
            }

            Debug.Log("[UMATestSpawner] DNA 균일값 테스트 10명 생성! (0.1 ~ 1.0)");
        }
    }
}
