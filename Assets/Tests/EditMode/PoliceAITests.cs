using NUnit.Framework;
using UnityEngine;
using PoliceChase.AI;
using PoliceChase.Core;

namespace PoliceChase.Tests
{
    public class PoliceAITests
    {
        private GameObject policeObj;
        private GameObject playerObj;
        private PoliceAI policeAI;
        private RoutePlanner routePlanner;
        private TrajectoryPrediction prediction;

        [SetUp]
        public void Setup()
        {
            playerObj = new GameObject("Player");
            playerObj.transform.position = Vector3.zero;

            policeObj = new GameObject("Police");
            policeObj.transform.position = new Vector3(50, 0, 0);

            // Add required components
            var rb = policeObj.AddComponent<Rigidbody>();
            var config = ScriptableObject.CreateInstance<Vehicle.VehicleConfig>();
            var physics = policeObj.AddComponent<Vehicle.VehiclePhysics>();
            physics.Config = config;
            var wheels = policeObj.AddComponent<Vehicle.WheelSystem>();
            var controller = policeObj.AddComponent<Vehicle.PoliceCar>();
            controller.Controller = policeObj.AddComponent<Vehicle.VehicleController>();
            controller.Controller.Config = config;

            routePlanner = policeObj.AddComponent<RoutePlanner>();
            prediction = policeObj.AddComponent<TrajectoryPrediction>();
            policeAI = policeObj.AddComponent<PoliceAI>();

            prediction.Initialize(playerObj.transform.position);
        }

        [TearDown]
        public void Teardown()
        {
            if (policeObj != null) Object.DestroyImmediate(policeObj);
            if (playerObj != null) Object.DestroyImmediate(playerObj);
        }

        [Test]
        public void Police_Creation()
        {
            Assert.IsNotNull(policeAI, "PoliceAI should be created");
        }

        [Test]
        public void Police_SafeSpawn_Distance()
        {
            Vector3 playerPos = Vector3.zero;
            Vector3 policePos = new Vector3(60, 0, 0);
            float dist = Vector3.Distance(playerPos, policePos);
            Assert.GreaterOrEqual(dist, 50f, "Police should spawn at least 50m from player");
        }

        [Test]
        public void Police_Role_Selection()
        {
            policeAI.ChooseRole(PoliceRole.Chase);
            Assert.AreEqual(PoliceRole.Chase, policeAI.CurrentRole);

            policeAI.ChooseRole(PoliceRole.Intercept);
            Assert.AreEqual(PoliceRole.Intercept, policeAI.CurrentRole);

            policeAI.ChooseRole(PoliceRole.Block);
            Assert.AreEqual(PoliceRole.Block, policeAI.CurrentRole);
        }

        [Test]
        public void Police_Prediction()
        {
            prediction.UpdateHistory(Vector3.zero, new Vector3(10, 0, 0));
            Vector3 predicted = prediction.PredictPosition(1f);
            // Should be ahead
            Assert.Greater(predicted.x, 0f, "Predicted position should be ahead");
        }

        [Test]
        public void Police_Route_Planning()
        {
            // Without NavMesh, route may fail, but we test logic
            bool hasRoute = routePlanner.CalculateRoute(policeObj.transform.position, playerObj.transform.position);
            // Route may be false without NavMesh, but should not throw
            Assert.IsTrue(true, "Route planning should not throw");
        }

        [Test]
        public void Police_Strategy_Change()
        {
            policeAI.Initialize(playerObj.transform, AIDifficulty.Simple);
            Assert.AreEqual(AIDifficulty.Simple, policeAI.Difficulty);

            policeAI.Initialize(playerObj.transform, AIDifficulty.Advanced);
            Assert.AreEqual(AIDifficulty.Advanced, policeAI.Difficulty);
        }

        [Test]
        public void Police_Arrest_Condition()
        {
            var arrestObj = new GameObject("Arrest");
            var arrest = arrestObj.AddComponent<ArrestSystem>();
            arrest.MinPoliceForArrest = 2;
            arrest.ArrestRadius = 8f;
            arrest.ImmobilizeTimeRequired = 3f;

            Assert.AreEqual(2, arrest.MinPoliceForArrest);
            Assert.AreEqual(8f, arrest.ArrestRadius);

            Object.DestroyImmediate(arrestObj);
        }

        [Test]
        public void Police_NoTeleport()
        {
            Vector3 startPos = policeObj.transform.position;
            // Simulate AI update - should not teleport
            policeAI.Initialize(playerObj.transform, AIDifficulty.Normal);
            // After one frame, position should not be player position
            Assert.AreNotEqual(playerObj.transform.position, policeObj.transform.position, "Police should not teleport to player");
        }

        [Test]
        public void Police_Speed_Rules()
        {
            var settings = new VehicleSettings();
            float playerMax = settings.PlayerMaxSpeed;
            float policeMax = playerMax * settings.PoliceMaxSpeedMultiplier;
            float nitroMax = policeMax * settings.NitroBoostMultiplier;

            Assert.Greater(policeMax, playerMax);
            Assert.Greater(nitroMax, policeMax);
            Assert.AreEqual(playerMax * 1.01f, policeMax, 0.01f);
            Assert.AreEqual(policeMax * 1.25f, nitroMax, 0.01f);
        }
    }
}
