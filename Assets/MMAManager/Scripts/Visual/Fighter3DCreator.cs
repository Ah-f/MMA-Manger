using UnityEngine;
using MMAManager.Models;

namespace MMAManager.Visual
{
    /// <summary>
    /// 3D Fighter Creator - Generates procedural 3D fighter models
    /// </summary>
    public class Fighter3DCreator : MonoBehaviour
    {
        [Header("Fighter Colors")]
        [SerializeField] private Color fighter1Color = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color fighter2Color = new Color(0.8f, 0.2f, 0.2f);

        [Header("Model Settings")]
        [SerializeField] private float fighterScale = 1.8f;
        [SerializeField] private Material defaultMaterial;

        private void Start()
        {
            if (defaultMaterial == null)
            {
                defaultMaterial = new Material(Shader.Find("Standard"));
            }
        }

        /// <summary>
        /// Create a 3D representation of a fighter
        /// </summary>
        public GameObject CreateFighter3D(Fighter fighter, Vector3 position, bool isFighter1)
        {
            GameObject fighterObj = new GameObject($"Fighter_{fighter.DisplayName.Replace(" ", "_").Replace("\"", "")}");
            fighterObj.transform.position = position;

            // Add FighterVisualInfo component
            FighterVisualInfo visualInfo = fighterObj.AddComponent<FighterVisualInfo>();
            visualInfo.Initialize(fighter);

            // Create body parts
            Transform bodyTransform = CreateBody(fighterObj.transform, isFighter1);

            // Add animator for basic animations
            FighterAnimator animator = fighterObj.AddComponent<FighterAnimator>();
            animator.Initialize(bodyTransform);

            return fighterObj;
        }

        private Transform CreateBody(Transform parent, bool isFighter1)
        {
            Color bodyColor = isFighter1 ? fighter1Color : fighter2Color;
            GameObject body = new GameObject("Body");
            body.transform.SetParent(parent);
            body.transform.localPosition = Vector3.zero;

            // Create body parts
            CreateHead(body.transform, bodyColor);
            CreateTorso(body.transform, bodyColor);
            CreateArms(body.transform, bodyColor);
            CreateLegs(body.transform, bodyColor);

            return body.transform;
        }

        private void CreateHead(Transform parent, Color color)
        {
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(parent);
            head.transform.localPosition = new Vector3(0, 1.7f, 0);
            head.transform.localScale = Vector3.one * 0.25f;

            Renderer renderer = head.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.7f, 0.6f); // Skin tone
            }

            // Create eyes
            CreateEye(head.transform, new Vector3(-0.08f, 0.05f, 0.2f));
            CreateEye(head.transform, new Vector3(0.08f, 0.05f, 0.2f));
        }

        private void CreateEye(Transform parent, Vector3 localPosition)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(parent);
            eye.transform.localPosition = localPosition;
            eye.transform.localScale = Vector3.one * 0.1f;

            Renderer renderer = eye.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.black;
            }
        }

        private void CreateTorso(Transform parent, Color color)
        {
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(parent);
            torso.transform.localPosition = new Vector3(0, 1.2f, 0);
            torso.transform.localScale = new Vector3(0.4f, 0.6f, 0.25f);
            torso.transform.rotation = Quaternion.Euler(0, 0, 90);

            Renderer renderer = torso.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        private void CreateArms(Transform parent, Color color)
        {
            // Left Arm
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(parent);
            leftArm.transform.localPosition = new Vector3(-0.3f, 1.2f, 0);
            leftArm.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            leftArm.transform.rotation = Quaternion.Euler(0, 0, 90);

            Renderer leftRenderer = leftArm.GetComponent<Renderer>();
            if (leftRenderer != null)
            {
                leftRenderer.material.color = color;
            }

            // Right Arm
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(parent);
            rightArm.transform.localPosition = new Vector3(0.3f, 1.2f, 0);
            rightArm.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            rightArm.transform.rotation = Quaternion.Euler(0, 0, 90);

            Renderer rightRenderer = rightArm.GetComponent<Renderer>();
            if (rightRenderer != null)
            {
                rightRenderer.material.color = color;
            }

            // Hands
            CreateHand(leftArm.transform, color, true);
            CreateHand(rightArm.transform, color, false);
        }

        private void CreateHand(Transform parent, Color color, bool isLeft)
        {
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.name = isLeft ? "LeftHand" : "RightHand";
            hand.transform.SetParent(parent);
            hand.transform.localPosition = new Vector3(isLeft ? -0.3f : 0.3f, 0, 0);
            hand.transform.localScale = Vector3.one * 0.15f;

            Renderer renderer = hand.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.7f, 0.6f);
            }
        }

        private void CreateLegs(Transform parent, Color color)
        {
            // Left Leg
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(parent);
            leftLeg.transform.localPosition = new Vector3(-0.1f, 0.5f, 0);
            leftLeg.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            leftLeg.transform.rotation = Quaternion.Euler(0, 0, 90);

            Renderer leftRenderer = leftLeg.GetComponent<Renderer>();
            if (leftRenderer != null)
            {
                leftRenderer.material.color = color;
            }

            // Right Leg
            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(parent);
            rightLeg.transform.localPosition = new Vector3(0.1f, 0.5f, 0);
            rightLeg.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            rightLeg.transform.rotation = Quaternion.Euler(0, 0, 90);

            Renderer rightRenderer = rightLeg.GetComponent<Renderer>();
            if (rightRenderer != null)
            {
                rightRenderer.material.color = color;
            }

            // Feet
            CreateFoot(leftLeg.transform, color);
            CreateFoot(rightLeg.transform, color);
        }

        private void CreateFoot(Transform parent, Color color)
        {
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.name = "Foot";
            foot.transform.SetParent(parent);
            foot.transform.localPosition = new Vector3(0, -0.35f, 0.05f);
            foot.transform.localScale = new Vector3(0.12f, 0.08f, 0.2f);

            Renderer renderer = foot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.black;
            }
        }

        /// <summary>
        /// Create an octagon cage/ring
        /// </summary>
        public GameObject CreateOctagon(Vector3 position)
        {
            GameObject octagon = new GameObject("Octagon");
            octagon.transform.position = position;

            // Create floor
            CreateOctagonFloor(octagon.transform);

            // Create posts
            CreateOctagonPosts(octagon.transform);

            // Create fence
            CreateOctagonFence(octagon.transform);

            return octagon;
        }

        private void CreateOctagonFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(10f, 0.1f, 10f);

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.3f, 0.2f, 0.1f);
            }
        }

        private void CreateOctagonPosts(Transform parent)
        {
            float radius = 5f;
            int postCount = 8;

            for (int i = 0; i < postCount; i++)
            {
                float angle = i * (360f / postCount) * Mathf.Deg2Rad;
                Vector3 postPos = new Vector3(Mathf.Cos(angle) * radius, 1.5f, Mathf.Sin(angle) * radius);

                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Post_{i}";
                post.transform.SetParent(parent);
                post.transform.position = postPos;
                post.transform.localScale = new Vector3(0.15f, 3f, 0.15f);

                Renderer renderer = post.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
        }

        private void CreateOctagonFence(Transform parent)
        {
            float radius = 5f;
            int segments = 8;

            for (int i = 0; i < segments; i++)
            {
                float startAngle = i * (360f / segments) * Mathf.Deg2Rad;
                float endAngle = ((i + 1) * (360f / segments)) * Mathf.Deg2Rad;

                Vector3 startPos = new Vector3(Mathf.Cos(startAngle) * radius, 1.5f, Mathf.Sin(startAngle) * radius);
                Vector3 endPos = new Vector3(Mathf.Cos(endAngle) * radius, 1.5f, Mathf.Sin(endAngle) * radius);

                CreateFenceSegment(parent, startPos, endPos);
            }
        }

        private void CreateFenceSegment(Transform parent, Vector3 start, Vector3 end)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = "FenceSegment";
            segment.transform.SetParent(parent);

            Vector3 direction = end - start;
            float length = direction.magnitude;
            Vector3 center = (start + end) / 2;

            segment.transform.position = center;
            segment.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);
            segment.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }

    /// <summary>
    /// Component that stores fighter data on the visual object
    /// </summary>
    public class FighterVisualInfo : MonoBehaviour
    {
        [SerializeField] private Fighter fighter;
        [SerializeField] private string fighterId;

        public Fighter Fighter => fighter;

        public void Initialize(Fighter fighterData)
        {
            fighter = fighterData;
            fighterId = fighterData.FighterId;
        }
    }

    /// <summary>
    /// Basic animator for fighter visual
    /// </summary>
    public class FighterAnimator : MonoBehaviour
    {
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private float punchRange = 0.5f;
        [SerializeField] private float kickRange = 0.6f;

        private bool isAnimating = false;
        private float animationTime = 0f;
        private FightAction currentAction = FightAction.Idle;

        public void Initialize(Transform body)
        {
            bodyTransform = body;
        }

        public void Punch()
        {
            StartAction(FightAction.Punch);
        }

        public void Kick()
        {
            StartAction(FightAction.Kick);
        }

        public void Takedown()
        {
            StartAction(FightAction.Takedown);
        }

        public void GetHit()
        {
            StartAction(FightAction.GetHit);
        }

        public void Knockdown()
        {
            StartAction(FightAction.Knockdown);
        }

        private void StartAction(FightAction action)
        {
            currentAction = action;
            animationTime = 0f;
            isAnimating = true;
        }

        private void Update()
        {
            if (!isAnimating || bodyTransform == null) return;

            animationTime += Time.deltaTime * animationSpeed;

            switch (currentAction)
            {
                case FightAction.Punch:
                    AnimatePunch();
                    break;
                case FightAction.Kick:
                    AnimateKick();
                    break;
                case FightAction.Takedown:
                    AnimateTakedown();
                    break;
                case FightAction.GetHit:
                    AnimateGetHit();
                    break;
                case FightAction.Knockdown:
                    AnimateKnockdown();
                    break;
            }

            if (animationTime >= 1f)
            {
                isAnimating = false;
                currentAction = FightAction.Idle;
                ResetPose();
            }
        }

        private void AnimatePunch()
        {
            float punchPhase = Mathf.Sin(animationTime * Mathf.PI);
            Transform rightArm = bodyTransform.Find("Body/RightArm");
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(0, 0, -30f * punchPhase);
            }
        }

        private void AnimateKick()
        {
            float kickPhase = Mathf.Sin(animationTime * Mathf.PI);
            Transform rightLeg = bodyTransform.Find("Body/RightLeg");
            if (rightLeg != null)
            {
                rightLeg.localRotation = Quaternion.Euler(kickPhase * 60f, 0, 0);
            }
        }

        private void AnimateTakedown()
        {
            float takedownPhase = Mathf.Sin(animationTime * Mathf.PI * 0.5f);
            bodyTransform.localPosition = new Vector3(0, -0.5f * takedownPhase, 0.3f * takedownPhase);
            bodyTransform.localRotation = Quaternion.Euler(-30f * takedownPhase, 0, 0);
        }

        private void AnimateGetHit()
        {
            float hitPhase = Mathf.Sin(animationTime * Mathf.PI);
            bodyTransform.localRotation = Quaternion.Euler(0, 0, 15f * hitPhase);
        }

        private void AnimateKnockdown()
        {
            float knockdownPhase = Mathf.Min(animationTime * 2f, 1f);
            bodyTransform.localRotation = Quaternion.Euler(-90f * knockdownPhase, 0, 0);
            bodyTransform.localPosition = new Vector3(0, -0.8f * knockdownPhase, 0);
        }

        private void ResetPose()
        {
            if (bodyTransform != null)
            {
                bodyTransform.localRotation = Quaternion.identity;
                bodyTransform.localPosition = Vector3.zero;
            }
        }

        private enum FightAction
        {
            Idle,
            Punch,
            Kick,
            Takedown,
            GetHit,
            Knockdown
        }
    }
}
