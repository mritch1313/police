using UnityEngine;
using PoliceChase.Vehicle;
using PoliceChase.Core;

namespace PoliceChase.AI
{
    /// <summary>
    /// Advanced Police AI with roles, prediction, route planning, no teleport, real physics.
    /// </summary>
    [RequireComponent(typeof(PoliceCar))]
    [RequireComponent(typeof(RoutePlanner))]
    [RequireComponent(typeof(TrajectoryPrediction))]
    public class PoliceAI : MonoBehaviour
    {
        [Header("AI")]
        public PoliceRole CurrentRole = PoliceRole.Chase;
        public AIDifficulty Difficulty = AIDifficulty.Normal;
        public Transform PlayerTarget;

        [Header("Tuning")]
        public float DetectionRadius = 100f;
        public float ReplanInterval = 1.5f;
        public float StuckThreshold = 2f;
        public float MaxStuckTime = 3f;

        private PoliceCar policeCar;
        private RoutePlanner routePlanner;
        private TrajectoryPrediction prediction;
        private VehicleController vehicleController;

        private Vector3 lastPosition;
        private float stuckTimer = 0f;
        private float replanTimer = 0f;
        private Vector3 currentTargetPos;
        private bool hasRoute = false;

        // Role behavior timers
        private float roleEvaluationTimer = 0f;
        private const float ROLE_EVAL_INTERVAL = 2f;

        private void Awake()
        {
            policeCar = GetComponent<PoliceCar>();
            routePlanner = GetComponent<RoutePlanner>();
            prediction = GetComponent<TrajectoryPrediction>();
            vehicleController = GetComponent<VehicleController>();
            lastPosition = transform.position;
        }

        public void Initialize(Transform player, AIDifficulty diff)
        {
            PlayerTarget = player;
            Difficulty = diff;
            prediction.Initialize(player.position);
            ChooseRole(PoliceRole.Chase);
        }

        private void Update()
        {
            if (PlayerTarget == null) return;
            if (!policeCar.IsActive) return;

            replanTimer += Time.deltaTime;
            roleEvaluationTimer += Time.deltaTime;

            // Update prediction history
            var playerCar = PlayerTarget.GetComponent<PlayerCar>();
            Vector3 playerVel = playerCar != null ? playerCar.GetVelocity() : Vector3.zero;
            prediction.UpdateHistory(PlayerTarget.position, playerVel);

            // Check if stuck
            CheckStuck();

            // Role evaluation
            if (roleEvaluationTimer > ROLE_EVAL_INTERVAL)
            {
                roleEvaluationTimer = 0f;
                EvaluateRole();
            }

            // Replan route
            if (replanTimer > ReplanInterval || !hasRoute || routePlanner.IsRouteFinished(transform.position))
            {
                replanTimer = 0f;
                PlanRoute();
            }

            // Drive towards waypoint
            DriveToWaypoint();
        }

        void CheckStuck()
        {
            float moved = Vector3.Distance(transform.position, lastPosition);
            if (moved < StuckThreshold * Time.deltaTime)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > MaxStuckTime)
                {
                    // Try to unstuck: reverse and turn
                    HandleStuck();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPosition = transform.position;
        }

        void HandleStuck()
        {
            // Reverse a bit and replan
            vehicleController.SetInputs(-1f, Random.Range(-1f, 1f), 0f, false);
            Invoke(nameof(ClearReverse), 1f);
            hasRoute = false;
        }

        void ClearReverse()
        {
            vehicleController.SetInputs(0f, 0f, 0f, false);
        }

        void EvaluateRole()
        {
            if (Difficulty == AIDifficulty.Simple) return;

            float distToPlayer = Vector3.Distance(transform.position, PlayerTarget.position);

            // Simple role distribution logic based on distance and situation
            // This will be overridden by PoliceManager for coordination
            if (distToPlayer < 15f && Difficulty == AIDifficulty.Advanced)
            {
                // Close: try to PIN
                if (Random.value > 0.7f)
                    ChooseRole(PoliceRole.Pin);
            }
            else if (distToPlayer > 50f)
            {
                ChooseRole(PoliceRole.Intercept);
            }
            else
            {
                ChooseRole(PoliceRole.Chase);
            }
        }

        public void ChooseRole(PoliceRole role)
        {
            if (CurrentRole == role) return;
            CurrentRole = role;
            hasRoute = false; // force replan
            Debug.Log($"[PoliceAI] {gameObject.name} role -> {role}");
        }

        void PlanRoute()
        {
            if (PlayerTarget == null) return;

            Vector3 targetPos = PlayerTarget.position;

            switch (CurrentRole)
            {
                case PoliceRole.Chase:
                    targetPos = GetChaseTarget();
                    break;
                case PoliceRole.Intercept:
                    targetPos = GetInterceptTarget();
                    break;
                case PoliceRole.Block:
                    targetPos = GetBlockTarget();
                    break;
                case PoliceRole.Pin:
                    targetPos = GetPinTarget();
                    break;
                case PoliceRole.Support:
                    targetPos = GetSupportTarget();
                    break;
            }

            // Validate target with NavigationManager
            if (NavigationManager.Instance != null)
            {
                targetPos = NavigationManager.Instance.GetClosestValidPosition(targetPos);
            }

            currentTargetPos = targetPos;
            hasRoute = routePlanner.CalculateRoute(transform.position, targetPos);

            if (!hasRoute && Difficulty != AIDifficulty.Simple)
            {
                // Try alternative
                System.Collections.Generic.List<UnityEngine.AI.NavMeshPath> alts;
                if (routePlanner.CalculateAlternativeRoutes(transform.position, targetPos, 3, out alts) && alts.Count > 0)
                {
                    hasRoute = true;
                }
            }
        }

        Vector3 GetChaseTarget()
        {
            if (Difficulty == AIDifficulty.Simple)
                return PlayerTarget.position;

            // Normal and Advanced use prediction
            float predictTime = Difficulty == AIDifficulty.Normal ? 1.5f : 2.5f;
            return prediction.PredictPosition(predictTime);
        }

        Vector3 GetInterceptTarget()
        {
            // Try to get ahead of player
            Vector3 playerVel = prediction.GetCurrentVelocity();
            float speed = playerVel.magnitude;
            if (speed < 2f) return PlayerTarget.position;

            // Predict where player will be
            float timeToIntercept = Vector3.Distance(transform.position, PlayerTarget.position) / Mathf.Max(vehicleController.GetSpeed(), 5f);
            Vector3 predicted = prediction.PredictPosition(timeToIntercept);

            // Offset to get in front
            Vector3 ahead = predicted + playerVel.normalized * 10f;
            return ahead;
        }

        Vector3 GetBlockTarget()
        {
            // Block predicted path - find intersection
            Vector3 predicted = prediction.PredictPosition(2f);
            // Try to position slightly ahead on predicted trajectory
            Vector3 playerVel = prediction.GetCurrentVelocity();
            if (playerVel.magnitude > 1f)
            {
                return predicted + playerVel.normalized * 5f;
            }
            return predicted;
        }

        Vector3 GetPinTarget()
        {
            // Try to stay beside player to limit movement
            Vector3 playerPos = PlayerTarget.position;
            Vector3 toPlayer = playerPos - transform.position;
            Vector3 perp = Vector3.Cross(toPlayer.normalized, Vector3.up).normalized;
            // Choose side that limits player
            return playerPos + perp * 3f;
        }

        Vector3 GetSupportTarget()
        {
            // Stay behind and to side
            Vector3 playerVel = prediction.GetCurrentVelocity();
            if (playerVel.magnitude < 1f)
                return PlayerTarget.position + Random.insideUnitSphere * 10f;

            return PlayerTarget.position - playerVel.normalized * 10f + Random.insideUnitSphere * 5f;
        }

        void DriveToWaypoint()
        {
            if (!hasRoute) return;

            Vector3 waypoint = routePlanner.GetCurrentWaypoint();
            Vector3 toWaypoint = waypoint - transform.position;
            toWaypoint.y = 0;

            if (toWaypoint.magnitude < 1f)
            {
                routePlanner.AdvanceWaypoint(transform.position);
                return;
            }

            // Steering
            float angle = Vector3.SignedAngle(transform.forward, toWaypoint.normalized, Vector3.up);
            float steer = Mathf.Clamp(angle / 45f, -1f, 1f);

            // Motor - adjust based on angle and difficulty
            float motor = 1f;
            if (Mathf.Abs(angle) > 30f)
                motor = 0.5f;
            if (Mathf.Abs(angle) > 90f)
                motor = -0.3f; // need to reverse?

            // Add some randomness for Simple AI
            if (Difficulty == AIDifficulty.Simple)
            {
                steer += Random.Range(-0.2f, 0.2f);
                if (Random.value < 0.05f) motor *= 0.5f; // occasional mistake
            }

            // Braking for sharp turns
            float brake = 0f;
            if (Mathf.Abs(angle) > 60f && vehicleController.GetSpeed() > 10f)
                brake = 0.5f;

            // Handbrake for drift in Advanced
            bool handbrake = false;
            if (Difficulty == AIDifficulty.Advanced && Mathf.Abs(angle) > 40f && vehicleController.GetSpeed() > 15f)
            {
                handbrake = Random.value > 0.7f;
            }

            policeCar.SetInput(motor, steer, brake, handbrake);

            // Advance waypoint if close
            routePlanner.AdvanceWaypoint(transform.position);
        }

        public void OnPlayerNitro()
        {
            // React to nitro - try to keep up
            if (Difficulty == AIDifficulty.Advanced)
            {
                // Increase urgency
                ReplanInterval = 0.5f;
                Invoke(nameof(ResetReplan), 3f);
            }
        }

        void ResetReplan() => ReplanInterval = 1.5f;

        public Vector3 GetCurrentTarget() => currentTargetPos;
        public bool HasRoute() => hasRoute;
    }
}
