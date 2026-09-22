using NUnit.Framework;
using UnityEngine;
using PoliceChase.Vehicle;
using PoliceChase.Core;

namespace PoliceChase.Tests
{
    public class VehicleTests
    {
        private GameObject carObj;
        private VehicleController controller;
        private VehicleConfig config;

        [SetUp]
        public void Setup()
        {
            config = ScriptableObject.CreateInstance<VehicleConfig>();
            config.Mass = 1500f;
            config.MaxSpeed = 30f;
            config.MotorForce = 2500f;
            config.BrakeForce = 8000f;
            config.SteerAngle = 30f;
            config.BodyColor = Color.red;

            carObj = new GameObject("TestCar");
            var rb = carObj.AddComponent<Rigidbody>();
            var physics = carObj.AddComponent<VehiclePhysics>();
            var wheels = carObj.AddComponent<WheelSystem>();
            controller = carObj.AddComponent<VehicleController>();
            controller.Config = config;
        }

        [TearDown]
        public void Teardown()
        {
            if (carObj != null) Object.DestroyImmediate(carObj);
            if (config != null) Object.DestroyImmediate(config);
        }

        [Test]
        public void Vehicle_Creation()
        {
            Assert.IsNotNull(controller, "VehicleController should be created");
            Assert.IsNotNull(carObj.GetComponent<Rigidbody>(), "Rigidbody should exist");
        }

        [Test]
        public void Vehicle_Forward_Movement()
        {
            controller.SetInputs(1f, 0f, 0f, false);
            Assert.AreEqual(1f, controller.MotorInput, "Motor input should be 1");
        }

        [Test]
        public void Vehicle_Backward_Movement()
        {
            controller.SetInputs(-1f, 0f, 0f, false);
            Assert.AreEqual(-1f, controller.MotorInput, "Motor input should be -1 for backward");
        }

        [Test]
        public void Vehicle_Turn_Left()
        {
            controller.SetInputs(0f, -1f, 0f, false);
            Assert.AreEqual(-1f, controller.SteerInput, "Steer left should be -1");
        }

        [Test]
        public void Vehicle_Turn_Right()
        {
            controller.SetInputs(0f, 1f, 0f, false);
            Assert.AreEqual(1f, controller.SteerInput, "Steer right should be 1");
        }

        [Test]
        public void Vehicle_Brake()
        {
            controller.SetInputs(0f, 0f, 1f, false);
            Assert.AreEqual(1f, controller.BrakeInput, "Brake should be 1");
        }

        [Test]
        public void Vehicle_Handbrake()
        {
            controller.SetInputs(0f, 0f, 0f, true);
            Assert.IsTrue(controller.Handbrake, "Handbrake should be active");
        }

        [Test]
        public void Vehicle_MaxSpeed()
        {
            float max = config.MaxSpeed;
            controller.SetMaxSpeed(max);
            Assert.AreEqual(max, controller.CurrentMaxSpeed, 0.01f, "Max speed should match config");
        }

        [Test]
        public void Nitro_Increases_MaxSpeed()
        {
            var settings = new VehicleSettings();
            float policeSpeed = settings.PlayerMaxSpeed * settings.PoliceMaxSpeedMultiplier;
            float nitroSpeed = policeSpeed * settings.NitroBoostMultiplier;

            Assert.Greater(nitroSpeed, policeSpeed, "Nitro speed should be greater than police speed");
            Assert.Greater(policeSpeed, settings.PlayerMaxSpeed, "Police should be 1% faster than player");
            Assert.AreEqual(1.01f, settings.PoliceMaxSpeedMultiplier, 0.001f);
            Assert.AreEqual(1.25f, settings.NitroBoostMultiplier, 0.001f);
        }
    }
}
