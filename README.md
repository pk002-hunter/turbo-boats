# Turbo Boats

> **An endless river runner in development.** Take the helm of a Thai longtail boat, find a clear path through changing waterways, and keep the hull intact.

![Turbo Boats concept art: a Thai longtail boat navigating a Bangkok canal](docs/images/tropical-canal-boat-race.png)

<p align="center"><sub>Concept art for the project direction. The current Unity scene uses graybox placeholder visuals.</sub></p>

**Status:** Prototype &nbsp; · &nbsp; **Engine:** Unity 6000.6.7f1 &nbsp; · &nbsp; **Language:** C# &nbsp; · &nbsp; **Targets:** Web and mobile planned

Turbo Boats is a single-player boat runner set across Thai inland rivers. Steer continuously to avoid floating hazards, collect river tokens, and manage hull damage as the scenery moves from city canals to markets, rice fields, and forest waterways.

## Gameplay

- **Steer through the river:** guide the boat smoothly across the available water.
- **Avoid hazards:** a bow impact ends the run; a stern scrape damages the hull and produces smoke.
- **Recover:** collect green health packs to restore a hull point.
- **Travel farther:** navigate procedural river sections and collect tokens along the route.

The prototype starts with three hull points. The HUD displays distance, tokens, remaining hull, and the current river zone.

### Controls

| Device | Steer | Restart after a crash |
|---|---|---|
| Keyboard | Hold **A/D** or **←/→** | **R** or the on-screen button |
| Gamepad | Move the left stick horizontally | On-screen button |
| Touchscreen | Drag left or right | On-screen button |

## Run in Unity

1. Open the repository folder in Unity Hub using **Unity 6000.6.7f1**.
2. Open `Assets/Scenes/CanalRun.unity`.
3. Press **Play** in the Unity Editor.

If the scene file is missing, the editor bootstrap can create the graybox scene. The repository currently contains an editor project, not a published browser or mobile build.

## Project status

### Implemented prototype systems

- Continuous boat steering for keyboard, gamepad, and touchscreen input.
- Procedurally spawned river sections with four graybox environment themes.
- Floating hazards, token pickups, hull health, stern-damage smoke, and health boosters.
- Chase camera, run-state handling, HUD, and restart flow.
- Unity's Input System package for gameplay input.

### Planned release work

1. **Web:** build and profile for Unity Web, then verify load time, memory, browser compatibility, and mobile-browser touch controls.
2. **Android:** add Android Build Support and test performance and controls on real devices.
3. **iOS:** add iOS Build Support and generate an Xcode project; final iOS builds require macOS and Xcode.
4. **All platforms:** adapt the HUD to screen sizes and safe areas, tune touch targets, and verify pause/resume and performance on lower-end devices.

These platforms are goals; no release build or device-performance qualification is included yet. Online multiplayer is outside the current prototype scope.

## Technology

| Area | Technology |
|---|---|
| Game engine | Unity 6000.6.7f1 |
| Programming language | C# |
| Input | Unity Input System |
| Physics | Unity 3D colliders and triggers |
| Source control | Git and a private GitHub repository |

## Code map

| Location | Responsibility |
|---|---|
| `Assets/Scripts/BoatRunnerController.cs` | Boat movement, input, collision handling, and damage feedback |
| `Assets/Scripts/BoatCollisionZone.cs` | Bow and stern collision-zone trigger forwarding |
| `Assets/Scripts/GameManager.cs` | Run state, hull health, tokens, HUD, and restart |
| `Assets/Scripts/EndlessRiverWorld.cs` | River sections, scenery, obstacles, tokens, and health pickups |
| `Assets/Scripts/HealthBooster.cs` | Health-pickup behavior |
| `Assets/Scripts/CameraFollow.cs` | Chase-camera follow behavior |
| `Assets/Editor/BootstrapScene.cs` | Initial graybox scene setup |
| `Packages/manifest.json` | Unity package dependencies |

## License

No distribution license has been selected. Review the licenses for art, audio, fonts, and other third-party materials before publishing a build.
