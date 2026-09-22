using UnityEngine;
using System.Collections.Generic;

namespace PoliceChase.AI
{
    public class TrajectoryPrediction : MonoBehaviour
    {
        [Header("Prediction")]
        public float PredictionTime = 3f;
        public float SampleInterval = 0.2f;
        public int HistorySize = 20;

        private Queue<Vector3> positionHistory = new Queue<Vector3>();
        private Queue<Vector3> velocityHistory = new Queue<Vector3>();
        private Vector3 lastPosition;
        private Vector3 currentVelocity;
        private Vector3 currentAcceleration;

        public void Initialize(Vector3 startPos)
        {
            lastPosition = startPos;
            positionHistory.Clear();
            velocityHistory.Clear();
        }

        public void UpdateHistory(Vector3 position, Vector3 velocity)
        {
            // Velocity
            currentVelocity = velocity;
            Vector3 accel = (velocity - (velocityHistory.Count > 0 ? velocityHistory.ToArray()[velocityHistory.Count -1] : velocity)) / Time.deltaTime;
            currentAcceleration = Vector3.Lerp(currentAcceleration, accel, 0.1f);

            positionHistory.Enqueue(position);
            velocityHistory.Enqueue(velocity);

            if (positionHistory.Count > HistorySize)
                positionHistory.Dequeue();
            if (velocityHistory.Count > HistorySize)
                velocityHistory.Dequeue();

            lastPosition = position;
        }

        public Vector3 PredictPosition(float timeAhead)
        {
            // Simple kinematic prediction + history trend
            Vector3 predicted = lastPosition + currentVelocity * timeAhead + 0.5f * currentAcceleration * timeAhead * timeAhead;

            // Consider previous trajectory curvature
            if (positionHistory.Count >= 3)
            {
                var hist = positionHistory.ToArray();
                Vector3 avgDir = (hist[hist.Length -1] - hist[0]).normalized;
                float avgSpeed = currentVelocity.magnitude;
                // Blend with straight line
                Vector3 trend = lastPosition + avgDir * avgSpeed * timeAhead;
                predicted = Vector3.Lerp(predicted, trend, 0.3f);
            }

            return predicted;
        }

        public Vector3[] PredictTrajectory(int steps)
        {
            Vector3[] points = new Vector3[steps];
            for (int i = 0; i < steps; i++)
            {
                float t = (i + 1) * SampleInterval;
                points[i] = PredictPosition(t);
            }
            return points;
        }

        public Vector3 GetCurrentVelocity() => currentVelocity;
        public Vector3 GetCurrentAcceleration() => currentAcceleration;

        public bool IsPlayerTurning()
        {
            if (velocityHistory.Count < 5) return false;
            var velArray = velocityHistory.ToArray();
            Vector3 first = velArray[0].normalized;
            Vector3 last = velArray[velArray.Length -1].normalized;
            float angle = Vector3.Angle(first, last);
            return angle > 30f;
        }

        public bool IsPlayerStopped() => currentVelocity.magnitude < 1f;
    }
}
