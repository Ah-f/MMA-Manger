using UnityEngine;

namespace MMAManager.Combat
{
    public class RoundManager : MonoBehaviour
    {
        private int currentRound = 1;
        private const int maxRounds = 3;
        private const float roundDuration = 300f;
        private float roundTimer = 0f;
        private bool roundActive = false;
        private bool fightPaused = false;

        private const float restDuration = 60f;
        private float restTimer = 0f;

        public int CurrentRound => currentRound;
        public float RoundTimer => roundTimer;
        public float RestTimer => restTimer;
        public bool IsRoundActive => roundActive;
        public bool IsResting => fightPaused;

        public System.Action<int> OnRoundStart;
        public System.Action<int> OnRoundEnd;
        public System.Action OnFightEnd;

        void Update()
        {
            if (roundActive)
            {
                roundTimer += Time.deltaTime;
                if (roundTimer >= roundDuration)
                {
                    EndRound();
                }
            }
            else if (fightPaused)
            {
                restTimer += Time.deltaTime;
                if (restTimer >= restDuration)
                {
                    StartNextRound();
                }
            }
        }

        public void StartFight()
        {
            currentRound = 1;
            StartRound();
        }

        public void StartRound()
        {
            roundActive = true;
            fightPaused = false;
            roundTimer = 0f;
            OnRoundStart?.Invoke(currentRound);
        }

        public void EndRound()
        {
            roundActive = false;
            fightPaused = true;
            restTimer = 0f;
            OnRoundEnd?.Invoke(currentRound);

            if (currentRound >= maxRounds)
            {
                EndFight();
            }
        }

        public void StartNextRound()
        {
            currentRound++;
            StartRound();
        }

        public void EndFight()
        {
            roundActive = false;
            fightPaused = false;
            OnFightEnd?.Invoke();
        }

        public string GetRoundTimeDisplay()
        {
            if (fightPaused) return "REST";
            
            int minutes = Mathf.FloorToInt(roundTimer / 60f);
            int seconds = Mathf.FloorToInt(roundTimer % 60f);
            return $"Round {currentRound}: {minutes:00}:{seconds:00}";
        }
    }
}
