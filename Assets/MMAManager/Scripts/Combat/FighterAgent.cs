using System.Collections;
using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Combat
{
    public enum FighterState
    {
        Idle,
        Approaching,
        Attacking,
        Defending,
        Hit,
        KO
    }

    public enum AttackType
    {
        Jab,
        Hook,
        BodyPunch,
        Kick,
        Combo,
        Takedown,
        SpecialCombo,
        ElbowKnee
    }

    public enum HitZone
    {
        Head,
        Body,
        Legs
    }

    public enum CornerStrategy
    {
        Balanced,
        Aggressive,
        Defensive,
        BodyWork,
        Takedown,
        Finish
    }

    public class AttackInfo
    {
        public string animName;
        public AttackType type;
        public HitZone hitZone;
        public float duration;
        public float damageMultiplier;

        public AttackInfo(string name, AttackType type, HitZone zone, float dur, float dmgMul)
        {
            animName = name;
            this.type = type;
            hitZone = zone;
            duration = dur;
            damageMultiplier = dmgMul;
        }
    }

    public class FighterAgent : MonoBehaviour
    {
        [Header("Data")]
        public Fighter fighterData;
        public FighterAgent opponent;

        [Header("Runtime State")]
        public FighterState currentState = FighterState.Idle;
        public bool isFighting = false;
        public int currentHP;
        public int maxHP;

        private Animator animator;
        private float decisionTimer;
        private float decisionInterval;
        private float baseDecisionInterval;
        private float moveSpeed;
        private float baseMoveSpeed;
        private bool isPerformingAction;

        // Strategy
        public CornerStrategy currentStrategy = CornerStrategy.Balanced;

        // Footwork
        private float circleDir = 1f;
        private float circleTimer;
        private float circleSwitchInterval;
        private float footworkAnimTimer;
        private float bobTimer;

        // Procedural idle sway
        private float swayTimer;
        private Transform spineBone;

        private const float ATTACK_RANGE = 1.8f;
        private const float CLOSE_RANGE = 2.5f;
        private const float FAR_RANGE = 4.5f;

        public System.Action<int> OnHit;
        public System.Action OnKnockout;

        #region Attack Database

        private static readonly AttackInfo[] attackDatabase = new AttackInfo[]
        {
            new AttackInfo("Jab Cross",       AttackType.Jab,       HitZone.Head, 0.7f, 0.7f),
            new AttackInfo("Jab Cross (1)",   AttackType.Jab,       HitZone.Head, 0.7f, 0.7f),
            new AttackInfo("Jab Cross (2)",   AttackType.Jab,       HitZone.Head, 0.7f, 0.7f),

            new AttackInfo("Hook",            AttackType.Hook,      HitZone.Head, 0.9f, 1.2f),
            new AttackInfo("Hook (1)",        AttackType.Hook,      HitZone.Head, 0.9f, 1.2f),
            new AttackInfo("Hook (2)",        AttackType.Hook,      HitZone.Head, 0.9f, 1.2f),
            new AttackInfo("Hook (3)",        AttackType.Hook,      HitZone.Head, 0.9f, 1.2f),
            new AttackInfo("Hook (4)",        AttackType.Hook,      HitZone.Head, 0.9f, 1.2f),

            new AttackInfo("Body Jab Cross",       AttackType.BodyPunch, HitZone.Body, 0.8f, 0.9f),
            new AttackInfo("Body Jab Cross (1)",   AttackType.BodyPunch, HitZone.Body, 0.8f, 0.9f),
            new AttackInfo("Body Jab Cross (2)",   AttackType.BodyPunch, HitZone.Body, 0.8f, 0.9f),

            new AttackInfo("Mma Kick",        AttackType.Kick,      HitZone.Body, 1.1f, 1.4f),
            new AttackInfo("Mma Kick (1)",    AttackType.Kick,      HitZone.Body, 1.1f, 1.4f),
            new AttackInfo("Drop Kick",       AttackType.Kick,      HitZone.Legs, 1.3f, 1.6f),

            new AttackInfo("Combo Punch",               AttackType.Combo,        HitZone.Head, 1.4f, 1.8f),
            new AttackInfo("Punch Combo",               AttackType.Combo,        HitZone.Head, 1.4f, 1.8f),
            new AttackInfo("Flying Knee Punch Combo",   AttackType.SpecialCombo, HitZone.Head, 1.8f, 2.5f),

            new AttackInfo("Double Leg Takedown - Attacker", AttackType.Takedown, HitZone.Body, 1.6f, 2.0f),

            // Special strikes
            new AttackInfo("Illegal Elbow Punch",     AttackType.ElbowKnee, HitZone.Head, 0.9f, 1.5f),
            new AttackInfo("Illegal Elbow Punch (1)", AttackType.ElbowKnee, HitZone.Head, 0.9f, 1.5f),
            new AttackInfo("Illegal Knee",            AttackType.ElbowKnee, HitZone.Body, 1.0f, 1.6f),
            new AttackInfo("Illegal Knee (1)",        AttackType.ElbowKnee, HitZone.Body, 1.0f, 1.6f),
            new AttackInfo("Illegal Headbutt",        AttackType.ElbowKnee, HitZone.Head, 1.0f, 1.8f),
            new AttackInfo("Capoeira",                AttackType.Kick,      HitZone.Head, 1.5f, 2.2f),
        };

        private static readonly string[] blockAnims =
            { "Center Block", "Left Block", "Left Block (1)", "Right Block" };

        private static readonly string[] approachAnims =
            { "Long Step Forward", "Short Step Forward" };

        private static readonly string[] retreatAnims =
            { "Step Backward", "Step Backward (1)", "Step Backward (2)" };

        private static readonly string[] sideStepLeftAnims =
            { "Long Left Side Step", "Medium Left Side Step", "Short Left Side Step" };

        private static readonly string[] sideStepRightAnims =
            { "Short Right Side Step" };

        private static readonly string[] walkBackAnims =
            { "Walking Backwards", "Walking Backwards (1)" };

        private static readonly string[] lightHeadHits =
            { "Head Hit", "Head Hit (1)", "Head Hit (2)", "Head Hit (3)", "Head Hit (4)",
              "Light Hit To Head", "Light Hit To Head (1)", "Light Hit To Head (2)",
              "Medium Hit To Head", "Medium Hit To Head (1)", "Medium Hit To Head (2)", "Medium Hit To Head (3)" };

        private static readonly string[] heavyHeadHits =
            { "Big Hit To Head", "Receiving A Big Uppercut", "Receiving An Uppercut" };

        private static readonly string[] lightBodyHits =
            { "Hit To Body", "Hit To Body (1)", "Hit To Body (2)", "Hit To Body (3)",
              "Stomach Hit", "Kidney Hit", "Rib Hit", "Side Hit" };

        private static readonly string[] heavyBodyHits =
            { "Big Stomach Hit", "Big Kidney Hit", "Big Rib Hit", "Big Side Hit" };

        #endregion

        #region Initialization

        public void Initialize(Fighter fighter, FighterAgent opp)
        {
            fighterData = fighter;
            opponent = opp;
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                animator.applyRootMotion = false;
                try { spineBone = animator.GetBoneTransform(HumanBodyBones.Spine); }
                catch { spineBone = null; }
            }

            CalculateStats();
            currentHP = maxHP;
            isFighting = false;
            isPerformingAction = false;
            currentState = FighterState.Idle;
            decisionTimer = Random.Range(0f, decisionInterval * 0.5f);

            circleDir = Random.value < 0.5f ? 1f : -1f;
            circleSwitchInterval = Random.Range(2f, 4f);
            circleTimer = 0f;
            footworkAnimTimer = 0f;
            bobTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        private void CalculateStats()
        {
            maxHP = Mathf.Clamp(80 + (fighterData.STR / 3) + (fighterData.STA / 4), 80, 150);
            baseDecisionInterval = Mathf.Clamp(3f - (fighterData.SPD / 40f), 0.6f, 2.5f);
            decisionInterval = baseDecisionInterval;
            baseMoveSpeed = Mathf.Clamp(1.5f + (fighterData.SPD / 50f), 1.5f, 3.5f);
            moveSpeed = baseMoveSpeed;
        }

        private float GetHPRatio() => (float)currentHP / maxHP;

        private void UpdateHPPenalties()
        {
            float hpRatio = GetHPRatio();
            float speedMod, intervalMod;

            if (hpRatio > 0.6f)
            {
                speedMod = 1f;
                intervalMod = 1f;
            }
            else if (hpRatio > 0.3f)
            {
                speedMod = 0.85f;
                intervalMod = 1.2f;
            }
            else
            {
                speedMod = 0.7f;
                intervalMod = 1.4f;
            }

            moveSpeed = baseMoveSpeed * speedMod;
            decisionInterval = baseDecisionInterval * intervalMod;
        }

        public void SetStrategy(CornerStrategy strategy)
        {
            currentStrategy = strategy;
        }

        public float GetDecisionInterval() => decisionInterval;

        #endregion

        #region Update Loop

        void Update()
        {
            if (!isFighting || currentState == FighterState.KO) return;

            UpdateHPPenalties();
            FaceOpponent();

            if (isPerformingAction) return;

            float dist = GetDistance();

            // Decision timer
            decisionTimer += Time.deltaTime;
            if (decisionTimer >= decisionInterval)
            {
                MakeDecision();
                decisionTimer = 0f;
            }

            // Continuous footwork when not performing an action
            PerformFootwork(dist);

            // Approaching: move toward opponent (with angle)
            if (currentState == FighterState.Approaching)
            {
                MoveTowardOpponent();
            }
        }

        private void FaceOpponent()
        {
            if (opponent == null) return;
            Vector3 dir = opponent.transform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 8f
                );
            }
        }

        void LateUpdate()
        {
            if (!isFighting || currentState == FighterState.KO) return;
            ApplyIdleSway();
        }

        private void ApplyIdleSway()
        {
            if (spineBone == null) return;

            swayTimer += Time.deltaTime;

            // Subtle lateral sway (side to side)
            float lateralSway = Mathf.Sin(swayTimer * 1.2f) * 2f;
            // Subtle forward-back sway
            float forwardSway = Mathf.Sin(swayTimer * 0.8f + 0.5f) * 1.5f;

            // Reduce sway during actions for realism
            float intensity = isPerformingAction ? 0.2f : 1f;

            spineBone.localRotation *= Quaternion.Euler(
                forwardSway * intensity,
                0f,
                lateralSway * intensity
            );
        }

        #endregion

        #region Footwork

        private void PerformFootwork(float dist)
        {
            // Circle direction switching
            circleTimer += Time.deltaTime;
            if (circleTimer >= circleSwitchInterval)
            {
                circleDir = -circleDir;
                circleSwitchInterval = Random.Range(2f, 5f);
                circleTimer = 0f;
            }

            // Footwork animation timer
            footworkAnimTimer += Time.deltaTime;

            if (dist <= ATTACK_RANGE * 0.8f && dist > 0.5f)
            {
                // Very close: minimal circling
                CircleOpponent(moveSpeed * 0.2f);
            }
            else if (dist <= CLOSE_RANGE)
            {
                // Close range: slow circling + subtle bob
                CircleOpponent(moveSpeed * 0.3f);
                BobDistance(dist, 0.15f);

                if (footworkAnimTimer > 1.5f)
                {
                    PlayCircleAnim();
                    footworkAnimTimer = 0f;
                }
            }
            else if (dist <= FAR_RANGE)
            {
                // Mid range: wider circling
                CircleOpponent(moveSpeed * 0.45f);

                if (footworkAnimTimer > 1.2f)
                {
                    PlayCircleAnim();
                    footworkAnimTimer = 0f;
                }
            }
        }

        private void CircleOpponent(float speed)
        {
            if (opponent == null) return;

            Vector3 toMe = transform.position - opponent.transform.position;
            toMe.y = 0;
            float dist = toMe.magnitude;
            if (dist < 0.01f) return;

            // Orbit around opponent preserving distance
            float angle = (speed / dist) * Time.deltaTime * circleDir;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 newOffset = new Vector3(
                toMe.x * cos - toMe.z * sin,
                0f,
                toMe.x * sin + toMe.z * cos
            );

            transform.position = opponent.transform.position + newOffset;
        }

        private void BobDistance(float currentDist, float amplitude)
        {
            if (opponent == null) return;

            bobTimer += Time.deltaTime * 2f;
            float bobValue = Mathf.Sin(bobTimer) * amplitude;

            Vector3 toOpponent = (opponent.transform.position - transform.position).normalized;
            toOpponent.y = 0;
            transform.position += toOpponent * bobValue * moveSpeed * Time.deltaTime;
        }

        private void MoveTowardOpponent()
        {
            if (opponent == null) return;

            float dist = GetDistance();
            if (dist <= ATTACK_RANGE)
            {
                currentState = FighterState.Idle;
                return;
            }

            Vector3 toOpponent = (opponent.transform.position - transform.position).normalized;
            toOpponent.y = 0;

            // Angled approach: 75% forward + 25% lateral
            Vector3 lateral = Vector3.Cross(Vector3.up, toOpponent) * circleDir;
            Vector3 moveDir = (toOpponent * 0.75f + lateral * 0.25f).normalized;

            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        private float GetDistance()
        {
            if (opponent == null) return float.MaxValue;
            return Vector3.Distance(transform.position, opponent.transform.position);
        }

        private void PlayCircleAnim()
        {
            if (circleDir > 0)
                PlayMoveAnim(sideStepLeftAnims);
            else
                PlayMoveAnim(sideStepRightAnims);
        }

        #endregion

        #region AI Decision Making

        private void MakeDecision()
        {
            float dist = GetDistance();

            // Strategy modifiers
            float attackMod = 1f, blockMod = 1f, retreatMod = 1f, approachMod = 1f;
            switch (currentStrategy)
            {
                case CornerStrategy.Aggressive:
                    attackMod = 1.25f; blockMod = 0.7f; retreatMod = 0.5f; approachMod = 1.2f;
                    break;
                case CornerStrategy.Defensive:
                    attackMod = 0.7f; blockMod = 1.4f; retreatMod = 1.5f; approachMod = 0.7f;
                    break;
                case CornerStrategy.Finish:
                    attackMod = 1.4f; blockMod = 0.5f; retreatMod = 0.5f; approachMod = 1.3f;
                    break;
                case CornerStrategy.Takedown:
                    approachMod = 1.2f; attackMod = 1.1f;
                    break;
                case CornerStrategy.BodyWork:
                    attackMod = 1.1f;
                    break;
            }

            // Far: approach
            if (dist > FAR_RANGE)
            {
                BeginApproach();
                return;
            }

            // Mid range
            if (dist > CLOSE_RANGE)
            {
                float roll = Random.value;
                float rushChance = Mathf.Clamp01(0.45f * approachMod);
                if (roll < rushChance)
                {
                    BeginApproach();
                }
                else if (roll < rushChance + 0.20f)
                {
                    BeginFeint();
                }
                else if (roll < rushChance + 0.40f)
                {
                    circleDir = -circleDir;
                    PlayCircleAnim();
                }
                else
                {
                    PlayMoveAnim(walkBackAnims);
                    StartCoroutine(SlideInDirection(GetRetreatDir(), 0.4f, moveSpeed * 0.4f));
                }
                return;
            }

            // Close range: combat

            // Block if opponent attacking
            if (opponent.currentState == FighterState.Attacking)
            {
                float blockChance = GetBlockChance() * blockMod;
                // HP penalty: block harder at low HP
                if (GetHPRatio() < 0.3f) blockChance *= 0.8f;

                if (Random.value < blockChance)
                {
                    BeginDefend();
                    return;
                }
            }

            // Attack
            float atkChance = GetAttackChance(dist) * attackMod;
            if (Random.value < atkChance)
            {
                BeginAttack();
                return;
            }

            // In-out: back off to mid range then re-enter
            if (Random.value < 0.3f * retreatMod)
            {
                BeginRetreat();
                return;
            }

            // Low HP: retreat more aggressively
            if (GetHPRatio() < 0.3f && Random.value < 0.5f * retreatMod)
            {
                BeginRetreat();
                return;
            }

            // Switch circle direction
            if (Random.value < 0.4f)
            {
                circleDir = -circleDir;
                PlayCircleAnim();
            }
        }

        private float GetAttackChance(float dist)
        {
            float chance = 0.45f;
            chance += (CLOSE_RANGE - dist) / CLOSE_RANGE * 0.2f;
            chance += fighterData.TEC / 300f;
            if (opponent.currentHP < opponent.maxHP * 0.3f) chance += 0.15f;
            return Mathf.Clamp01(chance);
        }

        private float GetBlockChance()
        {
            float defSkill = (fighterData.DEF + fighterData.WREST + fighterData.BJJ) / 3f;
            float atkSpd = opponent.fighterData.SPD;
            return Mathf.Clamp01(defSkill / (defSkill + atkSpd));
        }

        #endregion

        #region Actions

        private void BeginApproach()
        {
            currentState = FighterState.Approaching;
            PlayMoveAnim(approachAnims);
        }

        private void BeginFeint()
        {
            PlayMoveAnim(approachAnims);
            StartCoroutine(FeintSequence());
        }

        private void BeginRetreat()
        {
            // Diagonal retreat: backward + lateral
            Vector3 back = GetRetreatDir();
            Vector3 lateral = Vector3.Cross(Vector3.up, back) * circleDir;
            Vector3 retreatDir = (back * 0.7f + lateral * 0.3f).normalized;

            PlayMoveAnim(retreatAnims);
            StartCoroutine(SlideInDirection(retreatDir, 0.6f, moveSpeed * 0.6f));
        }

        private void BeginAttack()
        {
            AttackInfo attack = ChooseAttack();
            currentState = FighterState.Attacking;
            isPerformingAction = true;

            PlayAnim(attack.animName);
            StartCoroutine(ExecuteAttackSequence(attack));
        }

        private void BeginDefend()
        {
            currentState = FighterState.Defending;
            isPerformingAction = true;

            string anim = blockAnims[Random.Range(0, blockAnims.Length)];
            PlayAnim(anim);
            StartCoroutine(FinishActionAfter(0.8f));
        }

        private AttackInfo ChooseAttack()
        {
            float[] weights = new float[attackDatabase.Length];
            float total = 0f;
            float hpRatio = GetHPRatio();

            for (int i = 0; i < attackDatabase.Length; i++)
            {
                float w = 1f;
                var atk = attackDatabase[i];

                switch (atk.type)
                {
                    case AttackType.Jab:        w = 3.0f; break;
                    case AttackType.Hook:        w = 2.0f + fighterData.STR / 120f; break;
                    case AttackType.BodyPunch:   w = 1.5f + fighterData.TEC / 120f; break;
                    case AttackType.Kick:        w = 1.5f + fighterData.TEC / 120f; break;
                    case AttackType.Combo:       w = 0.7f + fighterData.TEC / 150f; break;
                    case AttackType.Takedown:    w = 0.4f + fighterData.WREST / 80f; break;
                    case AttackType.SpecialCombo:w = 0.2f; break;
                    case AttackType.ElbowKnee:  w = 0.6f + fighterData.STR / 150f; break;
                }

                // Strategy weight modifiers
                switch (currentStrategy)
                {
                    case CornerStrategy.BodyWork:
                        if (atk.hitZone == HitZone.Body) w *= 3f;
                        else if (atk.hitZone == HitZone.Head) w *= 0.5f;
                        break;
                    case CornerStrategy.Takedown:
                        if (atk.type == AttackType.Takedown) w *= 5f;
                        break;
                    case CornerStrategy.Finish:
                        if (atk.type == AttackType.Combo || atk.type == AttackType.SpecialCombo)
                            w *= 3f;
                        break;
                }

                // HP penalty: complex attacks harder at low HP
                if (hpRatio < 0.3f)
                {
                    if (atk.type == AttackType.Combo || atk.type == AttackType.SpecialCombo)
                        w *= 0.5f;
                }

                weights[i] = w;
                total += w;
            }

            float roll = Random.Range(0f, total);
            float cumul = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumul += weights[i];
                if (roll <= cumul)
                    return attackDatabase[i];
            }
            return attackDatabase[0];
        }

        private Vector3 GetRetreatDir()
        {
            Vector3 dir = (transform.position - opponent.transform.position).normalized;
            dir.y = 0;
            return dir;
        }

        #endregion

        #region Coroutines

        private IEnumerator ExecuteAttackSequence(AttackInfo attack)
        {
            yield return new WaitForSeconds(attack.duration * 0.4f);

            if (opponent != null && opponent.currentState != FighterState.KO)
            {
                float dist = GetDistance();
                if (dist <= ATTACK_RANGE + 0.5f)
                {
                    float dmg = CalculateDamage(attack);

                    if (opponent.currentState == FighterState.Defending)
                    {
                        dmg *= 0.25f;
                        Debug.Log($"[{fighterData.FirstName}] {attack.animName} BLOCKED ({dmg:F0})");
                        var blockSfx = CombatSoundManager.Instance;
                        if (blockSfx != null) blockSfx.PlayBlockSound();
                    }
                    else
                    {
                        Debug.Log($"[{fighterData.FirstName}] {attack.animName} HIT ({dmg:F0})");
                    }

                    opponent.ReceiveHit(dmg, attack.hitZone);
                }
                else
                {
                    Debug.Log($"[{fighterData.FirstName}] {attack.animName} MISSED");
                }
            }

            yield return new WaitForSeconds(attack.duration * 0.6f);
            isPerformingAction = false;
            currentState = FighterState.Idle;
        }

        private IEnumerator FeintSequence()
        {
            // Quick step forward
            Vector3 fwd = (opponent.transform.position - transform.position).normalized;
            fwd.y = 0;
            float t = 0f;
            while (t < 0.25f)
            {
                transform.position += fwd * moveSpeed * 0.8f * Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }

            // Quick step back
            PlayMoveAnim(retreatAnims);
            t = 0f;
            while (t < 0.35f)
            {
                transform.position -= fwd * moveSpeed * 0.6f * Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FinishActionAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            isPerformingAction = false;
            currentState = FighterState.Idle;
        }

        private IEnumerator SlideInDirection(Vector3 dir, float duration, float speed)
        {
            float t = 0f;
            while (t < duration)
            {
                transform.position += dir * speed * Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
        }

        #endregion

        #region Receiving Damage

        public void ReceiveHit(float damage, HitZone zone)
        {
            int dmg = Mathf.RoundToInt(damage);
            currentHP -= dmg;
            OnHit?.Invoke(dmg);

            // Hit effects
            bool isHeavy = dmg > 7;
            SpawnHitEffect(zone, isHeavy);

            // Hit sound
            var sfx = CombatSoundManager.Instance;
            if (sfx != null) sfx.PlayHitSound(isHeavy);

            if (currentHP <= 0)
            {
                currentHP = 0;
                KnockOut();
                return;
            }

            StopAllCoroutines();
            isPerformingAction = true;
            currentState = FighterState.Hit;

            string hitAnim = PickHitAnimation(zone, dmg);
            PlayAnim(hitAnim);

            float stunTime = isHeavy ? 0.8f : 0.5f;
            StartCoroutine(FinishActionAfter(stunTime));
        }

        private void SpawnHitEffect(HitZone zone, bool isHeavy)
        {
            var fx = HitEffectManager.Instance;
            if (fx == null) return;

            // Hit position based on zone
            Vector3 hitPos = transform.position;
            if (zone == HitZone.Head)
                hitPos += Vector3.up * 1.6f;
            else if (zone == HitZone.Body)
                hitPos += Vector3.up * 1.1f;
            else
                hitPos += Vector3.up * 0.5f;

            // Slight offset toward attacker
            if (opponent != null)
            {
                Vector3 toAttacker = (opponent.transform.position - transform.position).normalized;
                hitPos += toAttacker * 0.2f;
            }

            fx.PlayHitEffect(hitPos, isHeavy);
            fx.PlayFlash(gameObject, isHeavy);
        }

        private void KnockOut()
        {
            StopAllCoroutines();
            currentState = FighterState.KO;
            isPerformingAction = true;
            isFighting = false;

            // KO effect - extra dramatic
            var fx = HitEffectManager.Instance;
            if (fx != null)
            {
                Vector3 headPos = transform.position + Vector3.up * 1.6f;
                fx.PlayHitEffect(headPos, true);
                fx.PlayFlash(gameObject, true);
            }

            // KO sound
            var sfx = CombatSoundManager.Instance;
            if (sfx != null) sfx.PlayKOSound();

            PlayAnim("Falling Back Death");
            StartCoroutine(KOSlowMotion());

            // KO camera
            var combatCam = CombatCamera.Instance;
            if (combatCam != null)
                combatCam.TriggerKOCamera(transform);

            OnKnockout?.Invoke();
        }

        private IEnumerator KOSlowMotion()
        {
            // Dramatic slow motion
            Time.timeScale = 0.2f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(2f);

            // Gradually restore
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 0.8f;
                Time.timeScale = Mathf.Lerp(0.2f, 1f, t);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        private string PickHitAnimation(HitZone zone, int damage)
        {
            bool heavy = damage > 7;

            switch (zone)
            {
                case HitZone.Head:
                    return heavy
                        ? heavyHeadHits[Random.Range(0, heavyHeadHits.Length)]
                        : lightHeadHits[Random.Range(0, lightHeadHits.Length)];

                case HitZone.Body:
                case HitZone.Legs:
                    return heavy
                        ? heavyBodyHits[Random.Range(0, heavyBodyHits.Length)]
                        : lightBodyHits[Random.Range(0, lightBodyHits.Length)];

                default:
                    return "Hit To Body";
            }
        }

        #endregion

        #region Damage Calculation

        private float CalculateDamage(AttackInfo attack)
        {
            float baseDmg = (fighterData.STR + fighterData.TEC) / 10f;
            float multiplier = attack.damageMultiplier;

            // Longer animations deal more damage
            float durationBonus = Mathf.Lerp(0.8f, 1.3f,
                (attack.duration - 0.7f) / (1.8f - 0.7f));

            // HP penalty: weaker attacks at low HP
            float hpRatio = GetHPRatio();
            float hpDmgMod = hpRatio > 0.6f ? 1f : hpRatio > 0.3f ? 0.9f : 0.8f;

            float defReduce = opponent.fighterData.DEF / 15f;
            float rng = Random.Range(0.85f, 1.15f);
            float result = Mathf.Max(2f, (baseDmg * multiplier * durationBonus * hpDmgMod - defReduce) * rng);
            return Mathf.Clamp(result, 2f, 20f);
        }

        #endregion

        #region Animation Helpers

        private void PlayAnim(string stateName)
        {
            if (animator != null)
                animator.Play(stateName, 0, 0f);
        }

        private void PlayMoveAnim(string[] anims)
        {
            if (animator != null && anims.Length > 0)
                PlayAnim(anims[Random.Range(0, anims.Length)]);
        }

        #endregion
    }
}
