using UnityEngine;

namespace MMAManager.Combat
{
    public enum CameraMode
    {
        Normal,
        CloseUp,
        KO,
        RoundStart
    }

    public class CombatCamera : MonoBehaviour
    {
        private static CombatCamera instance;
        public static CombatCamera Instance => instance;

        private Camera cam;
        private Transform fighter1;
        private Transform fighter2;

        // Camera parameters (portrait mobile - further back)
        private float baseDistance = 9f;
        private float minDistance = 6f;
        private float maxDistance = 12f;
        private float baseHeight = 4f;
        private float minHeight = 2.5f;
        private float maxHeight = 5.5f;
        private float baseFOV = 60f;

        // Orbit
        private float orbitAngle = 0f;
        private float orbitSpeed = 8f;
        private float targetOrbitSpeed = 8f;

        // Smooth movement
        private float positionSmooth = 3f;
        private float rotationSmooth = 4f;

        // Current state
        private CameraMode currentMode = CameraMode.Normal;
        private Vector3 targetPosition;
        private Vector3 targetLookAt;
        private float currentDistance;
        private float currentHeight;

        // KO mode
        private Transform koTarget;
        private float koOrbitSpeed = 25f;

        // Shake support
        private Vector3 shakeOffset;
        public bool IsShaking { get; set; }

        void Awake()
        {
            instance = this;
        }

        public void Initialize(Transform f1, Transform f2)
        {
            fighter1 = f1;
            fighter2 = f2;

            cam = Camera.main;
            if (cam == null) return;

            // Switch to perspective
            cam.orthographic = false;
            cam.fieldOfView = baseFOV;

            // Initial position
            currentDistance = baseDistance;
            currentHeight = baseHeight;
            orbitAngle = 180f; // Start behind

            UpdateCameraImmediate();
        }

        void LateUpdate()
        {
            if (cam == null || fighter1 == null || fighter2 == null) return;

            switch (currentMode)
            {
                case CameraMode.Normal:
                    UpdateNormal();
                    break;
                case CameraMode.CloseUp:
                    UpdateCloseUp();
                    break;
                case CameraMode.KO:
                    UpdateKO();
                    break;
                case CameraMode.RoundStart:
                    UpdateRoundStart();
                    break;
            }

            // Apply position with smoothing
            cam.transform.position = Vector3.Lerp(
                cam.transform.position, targetPosition, Time.unscaledDeltaTime * positionSmooth);

            // Apply rotation with smoothing
            Vector3 lookDir = targetLookAt - cam.transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                cam.transform.rotation = Quaternion.Slerp(
                    cam.transform.rotation, targetRot, Time.unscaledDeltaTime * rotationSmooth);
            }

            // Apply shake offset
            if (IsShaking)
                cam.transform.position += shakeOffset;
        }

        #region Camera Modes

        private void UpdateNormal()
        {
            Vector3 midpoint = GetMidpoint();
            float fighterDist = GetFighterDistance();

            // Orbit slowly
            orbitSpeed = Mathf.Lerp(orbitSpeed, targetOrbitSpeed, Time.unscaledDeltaTime * 2f);
            orbitAngle += orbitSpeed * Time.unscaledDeltaTime;

            // Distance based on fighter separation
            float distFactor = Mathf.InverseLerp(1f, 6f, fighterDist);
            float targetDist = Mathf.Lerp(minDistance, maxDistance, distFactor);
            currentDistance = Mathf.Lerp(currentDistance, targetDist, Time.unscaledDeltaTime * 2f);

            // Height adjusts with distance
            float targetHeight = Mathf.Lerp(minHeight, maxHeight, distFactor);
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.unscaledDeltaTime * 2f);

            // Switch to close-up when fighters are very close
            if (fighterDist < 1.5f)
            {
                SetMode(CameraMode.CloseUp);
                return;
            }

            CalculateOrbitPosition(midpoint);
        }

        private void UpdateCloseUp()
        {
            Vector3 midpoint = GetMidpoint();
            float fighterDist = GetFighterDistance();

            // Slower orbit during close combat
            orbitAngle += 5f * Time.unscaledDeltaTime;

            // Close distance, lower angle
            currentDistance = Mathf.Lerp(currentDistance, minDistance, Time.unscaledDeltaTime * 3f);
            currentHeight = Mathf.Lerp(currentHeight, minHeight, Time.unscaledDeltaTime * 3f);

            // Switch back to normal when fighters separate
            if (fighterDist > 3f)
            {
                SetMode(CameraMode.Normal);
                return;
            }

            CalculateOrbitPosition(midpoint);
        }

        private void UpdateKO()
        {
            if (koTarget == null) return;

            Vector3 center = koTarget.position + Vector3.up * 0.8f;

            // Dramatic orbit around KO'd fighter
            orbitAngle += koOrbitSpeed * Time.unscaledDeltaTime;

            currentDistance = Mathf.Lerp(currentDistance, 5f, Time.unscaledDeltaTime * 2f);
            currentHeight = Mathf.Lerp(currentHeight, 2.5f, Time.unscaledDeltaTime * 2f);

            CalculateOrbitPosition(center);
        }

        private void UpdateRoundStart()
        {
            Vector3 midpoint = GetMidpoint();

            // Wide shot, slowly zooming in
            currentDistance = Mathf.Lerp(currentDistance, baseDistance, Time.unscaledDeltaTime * 1.5f);
            currentHeight = Mathf.Lerp(currentHeight, baseHeight, Time.unscaledDeltaTime * 1.5f);
            orbitAngle += 3f * Time.unscaledDeltaTime;

            CalculateOrbitPosition(midpoint);

            // Auto-switch to normal after a moment
            if (Mathf.Abs(currentDistance - baseDistance) < 0.3f)
                SetMode(CameraMode.Normal);
        }

        #endregion

        #region Helpers

        private void CalculateOrbitPosition(Vector3 center)
        {
            float rad = orbitAngle * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad) * currentDistance;
            float z = Mathf.Cos(rad) * currentDistance;

            targetPosition = center + new Vector3(x, currentHeight, z);
            targetLookAt = center + Vector3.up * 0.8f;
        }

        private void UpdateCameraImmediate()
        {
            Vector3 midpoint = GetMidpoint();
            CalculateOrbitPosition(midpoint);
            cam.transform.position = targetPosition;
            cam.transform.LookAt(targetLookAt);
        }

        private Vector3 GetMidpoint()
        {
            return (fighter1.position + fighter2.position) * 0.5f;
        }

        private float GetFighterDistance()
        {
            return Vector3.Distance(fighter1.position, fighter2.position);
        }

        #endregion

        #region Public API

        public void SetMode(CameraMode mode)
        {
            currentMode = mode;

            switch (mode)
            {
                case CameraMode.RoundStart:
                    currentDistance = maxDistance;
                    currentHeight = maxHeight;
                    targetOrbitSpeed = 3f;
                    break;
                case CameraMode.Normal:
                    targetOrbitSpeed = 8f;
                    break;
                case CameraMode.CloseUp:
                    targetOrbitSpeed = 5f;
                    break;
                case CameraMode.KO:
                    targetOrbitSpeed = koOrbitSpeed;
                    break;
            }
        }

        public void TriggerKOCamera(Transform loser)
        {
            koTarget = loser;
            SetMode(CameraMode.KO);
        }

        public void SetShakeOffset(Vector3 offset)
        {
            shakeOffset = offset;
        }

        #endregion
    }
}
