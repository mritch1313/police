using UnityEngine;
using PoliceChase.Vehicle;
using PoliceChase.World;
using PoliceChase.AI;
using PoliceChase.Core;

namespace PoliceChase.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject PlayerCarPrefab;
        public GameObject PoliceCarPrefab;

        private void Awake()
        {
            // Ensure core managers exist
            EnsureManager<GameState>();
            EnsureManager<SettingsManager>();
            EnsureManager<SaveManager>();
            EnsureManager<PerformanceManager>();
            EnsureManager<SpawnManager>();
            EnsureManager<NavigationManager>();
            EnsureManager<PoliceManager>();
            EnsureManager<PoliceStrategy>();
            EnsureManager<ChaseManager>();
            EnsureManager<WorldStreamer>();
            EnsureManager<WorldGenerator>();

            // Ensure player exists
            if (FindObjectOfType<PlayerCar>() == null)
            {
                CreatePlayerCar();
            }

            // Ensure camera controller
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<Camera.CameraController>() == null)
            {
                var cc = cam.gameObject.AddComponent<Camera.CameraController>();
                var player = FindObjectOfType<PlayerCar>();
                if (player != null) cc.Target = player.transform;
            }

            // Ensure ground plane for NavMesh
            CreateGround();

            // Load settings
            var save = FindObjectOfType<SaveManager>();
            save?.LoadSettings();

            Debug.Log("[GameBootstrap] Game initialized");
        }

        void EnsureManager<T>() where T : MonoBehaviour
        {
            if (FindObjectOfType<T>() == null)
            {
                var go = new GameObject(typeof(T).Name);
                go.AddComponent<T>();
            }
        }

        void CreatePlayerCar()
        {
            GameObject carGO = new GameObject("PlayerCar");
            carGO.tag = "Player";
            carGO.layer = LayerMask.NameToLayer("Player");
            if (carGO.layer == -1) carGO.layer = 0;
            carGO.transform.position = new Vector3(0, 1, 0);

            var rb = carGO.AddComponent<Rigidbody>();
            var config = ScriptableObject.CreateInstance<VehicleConfig>();
            config.Mass = 1500f;
            config.MaxSpeed = 30f;
            config.MotorForce = 2500f;
            config.BrakeForce = 8000f;
            config.SteerAngle = 30f;
            config.BodyColor = Color.red;

            var physics = carGO.AddComponent<VehiclePhysics>();
            physics.Config = config;
            var wheels = carGO.AddComponent<WheelSystem>();
            var controller = carGO.AddComponent<VehicleController>();
            controller.Config = config;
            var player = carGO.AddComponent<PlayerCar>();
            player.Controller = controller;
            var nitro = carGO.AddComponent<NitroSystem>();
            player.Nitro = nitro;
            carGO.AddComponent<Input.MobileInput>();

            // Add collider
            var col = carGO.AddComponent<BoxCollider>();
            col.size = new Vector3(1.8f, 0.8f, 4.2f);
            col.center = new Vector3(0, 0.6f, 0);

            // Model
            var modelGO = new GameObject("CarModel");
            modelGO.transform.SetParent(carGO.transform);
            modelGO.transform.localPosition = Vector3.zero;
            var factory = modelGO.AddComponent<CarModelFactory>();
            factory.Initialize(config, modelGO.transform);

            Debug.Log("[GameBootstrap] Player car created");
        }

        void CreateGround()
        {
            if (GameObject.Find("GroundPlane") != null) return;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(100, 1, 100);
            ground.layer = LayerMask.NameToLayer("Ground");
            if (ground.layer == -1) ground.layer = 0;
            ground.isStatic = true;

            var rend = ground.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.3f, 0.3f, 0.3f);
                rend.material = mat;
            }

            // Add NavMeshSurface if available
            var surface = ground.GetComponent<UnityEngine.AI.NavMeshSurface>();
            if (surface == null)
            {
                // Try to add via reflection if package exists
                var type = System.Type.GetType("UnityEngine.AI.NavMeshSurface, Unity.AI.Navigation");
                if (type != null)
                {
                    ground.AddComponent(type);
                }
            }
        }
    }
}
