# Police Chase - Mobile 3D Open World Pursuit

**Unity 6.3 LTS (6000.3) | URP | Android Portrait | AI Navigation**

Мобильная 3D игра про автомобильную погоню в огромном открытом мире. Игрок начинает внутри качественной машины, преследуется полицией с продвинутым AI.

## 🎮 Особенности

- **Открытый мир** с World Streaming, Chunks, LOD, Frustum/Occlusion Culling
- **Физическая машина**: масса, ускорение, торможение, ручник, дрифт, инерция, трение
- **Камера 3-го лица** с свободным вращением, collision, плавным следом
- **Мобильное управление** портрет, виртуальный джойстик, газ/тормоз, ручник, NITRO, свайп камеры
- **NITRO**: PoliceMax = PlayerMax *1.01, PlayerNitro = PoliceMax *1.25, реальный разгон без телепорта
- **Полицейский AI**: CHASE, INTERCEPT, BLOCK, PIN, SUPPORT, динамическое распределение ролей, координация, предсказание траектории
- **Безопасный спавн**: проверка расстояния, земли, коллизий, маршрута, зданий
- **Арест**: только при физической обездвиженности, учет скорости, пространства, количества полиции

## 🏗️ Архитектура

```
Assets/Scripts/
├── Core/
│   ├── GameState - состояние игры, режимы, сложность
│   ├── SettingsManager - настройки авто, мира, графики
│   ├── SaveManager - сохранение
│   └── PerformanceManager - FPS, качество
├── Vehicle/
│   ├── VehicleConfig (ScriptableObject) - параметры авто
│   ├── ICarModel - интерфейс модели (заменяемость)
│   ├── CarModelFactory - процедурная модель (кузов, стекла, колеса, фары)
│   ├── WheelSystem - WheelColliders, трение
│   ├── VehiclePhysics - Rigidbody, масса, drag
│   ├── VehicleController - физика, не Transform.position
│   ├── PlayerCar - игрок
│   ├── PoliceCar - полиция
│   └── NitroSystem - заряд, расход, буст
├── World/
│   ├── WorldChunk - чанк мира
│   ├── WorldStreamer - загрузка/выгрузка по расстоянию
│   └── WorldGenerator - город, пригород, поля, пустыня, дороги, здания, заборы
├── AI/
│   ├── PoliceRole - CHASE, INTERCEPT, BLOCK, PIN, SUPPORT
│   ├── TrajectoryPrediction - предсказание позиции, скорости, поворота
│   ├── RoutePlanner - NavMesh маршруты, альтернативы
│   ├── NavigationManager - проверка NavMesh
│   ├── PoliceAI - преследование, перехват, координация, физика
│   ├── PoliceManager - спавн, распределение ролей
│   ├── PoliceStrategy - стратегия, обработка ситуаций
│   ├── SpawnManager - безопасный спавн
│   ├── ChaseManager - управление погоней
│   └── ArrestSystem - проверка обездвиженности
├── Camera/
│   └── CameraController - свободное вращение, collision
├── Input/
│   └── MobileInput - джойстик, кнопки, свайп
├── UI/
│   └── UIManager - HUD, скорость, нитро, статус
└── Utils/
    ├── ObjectPool - пулинг
    └── LODManager - LOD
```

## 🚗 Замена модели автомобиля

Архитектура позволяет заменить модель без переписывания логики:

1. Создайте префаб с моделью (кузов, колеса как отдельные меши)
2. Реализуйте `ICarModel`:
```csharp
public class MyCarModel : MonoBehaviour, ICarModel {
    public void Initialize(VehicleConfig config, Transform parent) { ... }
    public void UpdateWheels(float steerAngle, float[] rpm, bool[] grounded) { ... }
    public Transform GetBodyTransform() => transform;
    public void SetColor(Color c) { ... }
}
```
3. В `VehicleController` замените `CarModelFactory` на ваш компонент
4. Назначьте в `VehicleConfig.CarModelPrefab` ваш префаб

Логика `VehicleController`, `VehiclePhysics`, `WheelSystem`, `PoliceAI` не зависит от визуала.

## ⚙️ Настройки

Все параметры в `SettingsManager` и `VehicleConfig`:

**Скорость, ускорение, торможение:**
- `Assets/Resources/VehicleConfigs/PlayerConfig.asset` → Mass, MaxSpeed, MotorForce, BrakeForce, SteerAngle
- `SettingsManager.Vehicle.PlayerMaxSpeed`

**Сцепление:**
- `VehicleConfig.ForwardGrip`, `SidewaysGrip`, `HandbrakeGripMultiplier`

**Полиция:**
- `SettingsManager.Vehicle.PoliceMaxSpeedMultiplier = 1.01f`
- `PoliceCount` в `ChaseManager`
- `DetectionRadius`, `ReplanInterval` в `PoliceAI`

**Nitro:**
- `NitroBoostMultiplier = 1.25f`, `NitroDuration`, `ConsumptionRate`

**Мир:**
- `WorldChunkSize = 200f`, `RenderDistance = 3`, `DespawnDistance = 600f`

**Графика:**
- Quality: Low/Medium/High → `SettingsManager.SetQuality(0/1/2)`
- `RenderDistance`, `Shadows`, `LODDistance`

**AI сложность:**
- `GameState.Difficulty = Simple/Normal/Advanced`

## 🎯 AI Режимы

- **Simple**: прямое преследование, ошибки, плохой маршрут
- **Normal**: анализ скорости, предсказание, перехват, альтернативные маршруты
- **Advanced**: прогноз, оценка маршрутов, выгодные позиции, разделение ролей, координация

Роли:
- `CHASE` - преследование
- `INTERCEPT` - выход вперед
- `BLOCK` - блокировка пути
- `PIN` - ограничение движения
- `SUPPORT` - поддержка

## 📱 Мобильное управление (Portrait)

- Левый джойстик: руль + газ/тормоз
- Кнопки: ГАЗ, ТОРМОЗ, ЛЕВО, ПРАВО, РУЧНИК, СТОП, NITRO
- Свайп по правой половине: вращение камеры
- Кнопка "НАЧАТЬ" в правом верхнем углу запускает погоню

Камера:
- Свободное вращение независимо от авто
- Не блокируется при стоянии
- Collision, плавное приближение

## 🌍 Мир

- Город (центр), пригород, поля, пустыня
- Дороги, развязки, перекрестки, шоссе
- Здания, стены, заборы, препятствия
- Участки без дорог
- Streaming: загрузка чанков в радиусе, LOD, упрощенные коллизии, пулинг, активация по расстоянию

## 🔧 Сборка Android

### Локально
1. Unity 6000.3.0f1 + Android Build Support + URP + Input System + AI Navigation
2. Открыть проект
3. File → Build Settings → Android → Switch Platform
4. Player Settings: Portrait, Company com.police.chase, Min API 24
5. Build → `Builds/PoliceChase.apk`

Или через меню: `PoliceChase/Build Android APK`

### GitHub Actions
Workflow `.github/workflows/build.yml`:
- Устанавливает Unity 6000.3.0f1
- Проверяет структуру
- Запускает EditMode тесты
- Собирает APK
- Сохраняет артефакты: APK и логи

Требует секреты: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` (GameCI).

## ✅ Проверки

- `Assets/Editor/BuildScript.VerifyProject` - проверка проекта
- EditMode тесты: движение, поворот, тормоз, нитро, спавн, роли, маршруты, предсказание, арест
- `BUILD VERIFIED` только если APK реально создан
- `DEVICE RUNTIME NOT VERIFIED` если не тестировалось на устройстве

## 📦 Структура проекта

```
Assets/
├── Scenes/Main.unity
├── Scripts/ (Core, Vehicle, World, AI, Camera, Input, UI, Utils)
├── Prefabs/ (PlayerCar, PoliceCar)
├── Materials/
├── Settings/Rendering/ (URP assets)
├── Resources/VehicleConfigs/
├── Editor/BuildScript.cs
└── Tests/EditMode/
ProjectSettings/ (Android, URP, InputSystem)
Packages/manifest.json
.github/workflows/build.yml
```

## 🚀 Этапы разработки

1. Минимальная игра: машина, карта, камера, UI, Android build ✓
2. Физика автомобиля ✓
3. Большая карта + чанки ✓
4. Одна полицейская машина ✓
5. Базовый AI ✓
6. Обычный AI ✓
7. Продвинутый AI ✓
8. Несколько полицейских + координация ✓
9. Nitro ✓
10. Оптимизация ✓
11. Финальная Android сборка ✓

## 📄 Лицензия

Собственные ассеты, процедурная модель авто, без чужих платных ассетов.
