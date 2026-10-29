# Turbo Boats

![Turbo Boats concept art: a Thai longtail boat racing through a Bangkok canal](docs/images/tropical-canal-boat-race.png)

*Concept art generated from the Turbo Boats prompt; current in-game visuals are still graybox placeholders.*

**A fast, readable river-running game about steering a Thai longtail boat through changing inland waterways.**

Turbo Boats is being built in Unity with C#. The current project is an early, single-player graybox prototype: it establishes the endless river, boat steering, hazards, tokens, hull damage, smoke, and health pickups. The long-term goal is to make it easy to play in a desktop browser and on mobile devices, with the quick pick-up-and-play accessibility of browser games such as [Smash Karts](https://smashkarts.io/). Smash Karts is a reference for reach and approachability; Turbo Boats remains its own river-runner game and does not currently include kart combat or online multiplayer.

> **Project stage:** prototype. The gameplay and art are still being developed. Web and mobile releases are goals, not shipped builds.

## Play the prototype

1. Open this project folder in Unity Hub with **Unity 6000.6.7f1**.
2. Open `Assets/Scenes/CanalRun.unity` (the editor bootstrap can create the graybox scene if it is missing).
3. Press **Play** in the Unity Editor.

### Controls

| Device | Steering | Restart after a crash |
|---|---|---|
| Keyboard | Hold **A/D** or **←/→** for continuous steering | **R** or the on-screen button |
| Gamepad | Move the left stick horizontally | On-screen button |
| Touchscreen | Drag left or right | On-screen button |

The boat has three hull points. A bow collision ends the run; a stern collision removes a point and emits smoke. Green first-aid pickups restore one point, up to the hull maximum. The HUD displays distance, tokens, hull, and the current river zone.

## What exists today

- A procedural graybox river made from sections that spawn ahead and are removed behind the boat.
- Four environment themes: Bangkok canal, floating market, rice-field waterway, and forest river.
- Continuous lateral steering with keyboard, gamepad, and touch input.
- Floating hazards, collectible tokens, a three-point hull, stern damage feedback, and health pickups.
- A chase camera and a basic gameplay HUD.
- Unity's Input System package; gameplay scripts no longer call the legacy `UnityEngine.Input` API.

Current visuals use primitive geometry and generated materials. The prototype is not yet tuned or optimized for release devices.

## Technology

- **Engine:** Unity 6000.6.7f1
- **Code:** C#
- **Input:** Unity Input System package
- **Physics:** Unity 3D physics and trigger colliders
- **Version control:** Git with a private GitHub remote
- **Build targets:** WebGL and mobile are planned; they are not currently release builds

## Platform roadmap

The intended first release path is a browser build, followed by device builds:

1. **Web:** produce and profile a Unity WebGL build, then host it on a static web host that supports Unity Web builds. Check load time, memory use, browser compatibility, and touch input on real phones before calling it mobile-web ready.
2. **Android:** add the Android Build Support module in Unity Hub, build an installable package, and profile controls, memory, battery use, and frame rate on representative phones.
3. **iOS:** add iOS Build Support and create the Xcode project; final iOS builds require macOS with Xcode.
4. **Shared quality bar:** adapt the HUD to aspect ratios and safe areas, preserve readable touch targets, test suspend/resume, and keep performance stable on lower-end devices.

Online multiplayer, accounts, matchmaking, and leaderboards are not part of the current prototype. If multiplayer becomes a goal, it needs a separate design and network architecture before it is added.

## Project structure

| Path | Responsibility |
|---|---|
| `Assets/Scripts/BoatRunnerController.cs` | Boat movement, input, collision zones, and damage smoke |
| `Assets/Scripts/BoatCollisionZone.cs` | Identifies bow and stern trigger contacts |
| `Assets/Scripts/GameManager.cs` | Run state, hull health, token count, HUD, and restart |
| `Assets/Scripts/EndlessRiverWorld.cs` | Procedural river sections, scenery, hazards, tokens, and health pickups |
| `Assets/Scripts/HealthBooster.cs` | Health-pickup behavior |
| `Assets/Scripts/CameraFollow.cs` | Chase-camera following |
| `Assets/Editor/BootstrapScene.cs` | Creates the initial graybox scene and boat |
| `Packages/manifest.json` | Unity package dependencies |
| `DevelopmentLogs/` | Local-only engineering journal; ignored by Git |

## Engineering journal

Detailed development notes are kept in `DevelopmentLogs/` on the development PC. The folder is deliberately excluded from Git and GitHub so private study and interview-preparation notes stay local. The journal is organized for later PDF export and includes the project timeline, design explanations, source map, validation status, glossary, and interview practice questions.

## License and third-party material

No distribution license has been selected yet. Review art, audio, fonts, and other third-party assets for redistribution rights before publishing a build.
