# Architecture Documentation

## Overview
Police Chase is built with separation of concerns: Vehicle physics, World streaming, AI navigation, UI, all decoupled.

## Core Loop
1. `GameBootstrap` creates managers and player if missing
2. `WorldStreamer` loads chunks around player
3. `MobileInput` reads touch/keyboard and drives `PlayerCar` → `VehicleController` → `WheelSystem` + `VehiclePhysics`
4. `CameraController` follows player with free yaw/pitch, collision check
5. On "НАЧАТЬ", `ChaseManager` starts chase, `PoliceManager` spawns police via `SpawnManager`
6. Each `PoliceAI` gets role from `PoliceManager` and `PoliceStrategy`, predicts player via `TrajectoryPrediction`, plans route via `RoutePlanner` + `NavigationManager` (NavMesh)
7. Police physically drive to waypoints, no teleport
8. `ArrestSystem` checks immobilization

## Vehicle System
- `VehicleConfig` ScriptableObject holds all tunable params
- `VehiclePhysics` owns Rigidbody, mass, drag, center of mass
- `WheelSystem` owns 4 WheelColliders, friction curves
- `VehicleController` combines: motorTorque, steerAngle, brakeTorque, grip handling, speed clamp, model update
- `ICarModel` interface: Initialize, UpdateWheels, GetBodyTransform, SetColor
- `CarModelFactory` implements ICarModel procedurally (no external assets)
- Replacement: create new MonoBehaviour implementing ICarModel, assign in VehicleController, or set CarModelPrefab in VehicleConfig

## World System
- `WorldChunk` holds buildings, roads, obstacles, LOD level
- `WorldStreamer` tracks player chunk coord, loads RenderDistance chunks, unloads beyond DespawnDistance
- `WorldGenerator` decides biome by distance from center: city (<3), suburb (<7), fields (<10), desert (>10). Generates buildings (cubes with URP Lit), roads (planes), obstacles, fences
- LOD: far chunks LOD 2 (simplified), medium 1, near 0. Obstacles disabled randomly for far LOD
- Frustum/Occlusion: Unity's built-in + LODGroup culling in LODManager
- Object pooling: SimpleObjectPool for buildings/roads (concept, not fully wired for simplicity but architecture supports)

## AI System
- `PoliceRole` enum: Chase, Intercept, Block, Pin, Support
- `TrajectoryPrediction`: keeps history of player pos/vel, calculates acceleration, predicts kinematic + trend blend, detects turning/stopped
- `RoutePlanner`: wraps NavMeshPath, CalculateRoute, CalculateAlternativeRoutes (offset targets), GetCurrentWaypoint, AdvanceWaypoint, RemainingDistance
- `NavigationManager`: SamplePosition, HasValidPath, GetClosestValidPosition
- `PoliceAI`: 
  - Initialize with player and difficulty
  - UpdateHistory, CheckStuck (if moved < threshold, reverse)
  - EvaluateRole every 2s (simple: all chase, normal: chase+intercept, advanced: spread roles)
  - PlanRoute per role: Chase = predicted pos, Intercept = ahead of player (timeToIntercept * vel + offset), Block = predicted + vel*5, Pin = beside player (perp), Support = behind + random
  - DriveToWaypoint: angle to waypoint → steer = clamp(angle/45), motor based on angle, brake for sharp turns, handbrake for drift in advanced
  - No teleport, no magic speed boost near player
- `PoliceManager`: spawns via SpawnManager, assigns roles (Advanced: 1 Chase, 1 Intercept, 1 Block, rest Pin/Support), ClearPolice
- `PoliceStrategy`: global coordination every 3s, handles player offroad (OverlapSphere Road layer), stopped (pin), fast (intercept), avoids same role too close
- `SpawnManager`: FindSafeSpawnPosition tries MaxAttempts random points in annulus Min-Max distance, checks HasGround (raycast + NavMesh), IsFreeSpace (OverlapSphere ObstacleMask), IsInsideObstacle, IsOnNavMesh, HasRouteToPlayer
- `ChaseManager`: StartChase → GameState.StartChase + PoliceManager.SpawnPolice, EndChase
- `ArrestSystem`: immobilizeTimer accumulates if player speed <1, policeClose >= MinPoliceForArrest, freeSpace <0.3 (raycast 8 dirs), hasFrontBlock + side/rear. Arrest after ImmobilizeTimeRequired

## Nitro
- `NitroSystem`: MaxCharge, CurrentCharge, ConsumptionRate, RechargeRate, Duration, IsActive
- CanActivate if charge >10
- Activate: SetMaxSpeed to PlayerNitroMaxSpeed (PoliceMax*1.25), AddForce impulse forward (real acceleration, not instant max)
- Deactivate after Duration or charge depleted
- Charge normalized for UI

## Speed Rules
- SettingsManager: PlayerMax = 30 m/s, PoliceMax = PlayerMax*1.01, PlayerNitro = PoliceMax*1.25
- VehicleController clamps speed via opposite force if over CurrentMaxSpeed

## Camera
- Target + Offset rotated by yaw/pitch
- RotationSpeed, Min/Max vertical angle
- SphereCast from target to desired pos for collision, lerps currentDistance
- SmoothDamp position, Slerp rotation look at target+up
- AddRotationInput from MobileInput swipe
- Free rotation independent of car movement, continues when car stopped

## Input
- `MobileInput`: gasPressed, brakePressed, left/right, handbrake bool, motor, steer, brake
- Joystick: Background + Handle RectTransforms, drag → Horizontal/Vertical normalized by Range
- Buttons: EventTrigger PointerDown/Up for gas/brake, onClick for handbrake/nitro/start
- Touch: right half screen swipe → CameraController.AddRotationInput
- Keyboard fallback for editor: WASD, Space, N, C
- ApplyToCar → PlayerCar.SetInput

## UI
- `UIManager`: SpeedText (km/h), ChaseStatusText (Free Roam / CHASE time), NitroSlider, StartButton (НАЧАТЬ top right), DifficultyDropdown, ArrestProgressText
- CreateRuntimeUI if missing: Canvas ScreenSpaceOverlay, CanvasScaler 1080x1920 portrait, GraphicRaycaster, creates texts, button, slider

## Performance
- `PerformanceManager`: Application.targetFrameRate per quality (Low 30, Medium 45, High 60), vSync 0
- `SettingsManager.Graphics`: QualityLevel, RenderDistance, Shadows, MaxPoliceActive, LODDistance
- WorldStreamer UpdateInterval 0.5s, not every frame
- LODManager disables LODGroups beyond LowDetailDistance
- No Rigidbody on buildings (static), only cars have Rigidbody
- Simplified collision meshes (cubes)

## Build
- `BuildScript.BuildAndroidInternal`: SwitchActiveBuildTarget Android, PlayerSettings portrait, company, bundleVersion, minSdk 24, IL2CPP ARM64, URP asset assignment, BuildPipeline.BuildPlayer, exit code 0/1
- `VerifyProject`: checks scenes, URP asset
- GitHub Actions: verify job (structure, C# braces, version, key scripts) always runs, build job checks license secret, if present runs unity-builder, else skips with BUILD NOT VERIFIED notice

## Testing
- EditMode tests use NUnit, no Assert.IsTrue(true), real logic
- VehicleTests: creation, inputs, maxSpeed, nitro multiplier
- PoliceAITests: creation, spawn distance, role selection, prediction ahead, route planning no throw, strategy change, arrest condition, no teleport, speed rules
- WorldTests: chunk creation, LOD, generator

## Extensibility
- Change car model: implement ICarModel
- Change vehicle params: VehicleConfig asset
- Change police count: ChaseManager.PoliceCount
- Change world size: SettingsManager.Vehicle.WorldChunkSize, RenderDistance, DespawnDistance
- Change graphics: SettingsManager.SetQuality
- Change AI: GameState.Difficulty
- Add new biome: extend WorldGenerator.GenerateChunk
- Add new police role: extend PoliceRole enum and PoliceAI.Get*Target
