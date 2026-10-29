# Platform builds

## Current readiness

| Target | Project setup | Local verification |
|---|---|---|
| WebGL | Enabled scene, Gzip compression, responsive safe-area HUD, and build menu command | A 7 MB build succeeded in Unity; a small HUD allocation reduction was applied afterward, so rebuild the current source before browser QA |
| Android | ARM64 target, Android 8.0/API 26 minimum, auto-rotation, low quality preset, and development APK command | Not built here; Android Build Support is not installed in this Unity Editor |
| iOS | Auto-rotation, low quality preset, and Xcode project command | Not built here; iOS Build Support is not installed, and final Xcode build/signing needs macOS and Xcode |

The Unity Editor installed for this checkout has WebGL and Windows Standalone support only. No Android or iOS player modules are present. A menu command checks module availability and stops with a direct message instead of beginning an unavailable build.

## Build commands

Open this project in Unity **6000.6.7f1**, then use **Turbo Boats > Build**:

- **WebGL** writes to `Builds/WebGL`.
- **Android Development APK** writes to `Builds/Android/TurboBoats.apk` and uses a development build for device checks.
- **iOS Development Project** writes the Xcode project to `Builds/iOS`.

The commands use enabled scenes from Build Settings. `Assets/Scenes/CanalRun.unity` is currently the enabled startup scene. Android and iOS build outputs remain ignored by Git. The current WebGL output is checked in under `Builds/WebGL` so it can be downloaded with the project; rebuild that folder after source changes before sharing a browser build.

### Web hosting

The project selects Gzip compression for WebGL. Host the output over HTTPS and configure the server to return `Content-Encoding: gzip` for compressed Unity files. If a WebAssembly file is served for streaming, configure its MIME type as `application/wasm`. Verify the exact response headers after deployment; incorrect headers can prevent startup or force slower JavaScript decompression. See Unity's [compressed WebGL deployment guide](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-deploying.html).

The successful build produced `index.html`, compressed `.gz` data/framework/Wasm files, and the Unity loader. It is an editor build artifact, not yet tested across browsers or embedded in a publishing page. Browser compatibility, mobile Chrome fullscreen behavior, load time, and download size still need release checks. Unity documents that WebGL autorotation is limited to mobile Chrome in fullscreen mode; the in-game HUD adapts to the available safe area and aspect ratio but does not force browser fullscreen. See [Unity WebGL browser compatibility](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-browsercompatibility.html).

After that successful build, the HUD text and touch-rectangle helpers were changed to avoid repeated string and array allocations. A second build attempt from an isolated project copy could not initialize Unity licensing in this environment, so those final source changes have not been included in a new WebGL artifact yet.

## Mobile release setup still required

- Select the final Android application ID and iOS bundle ID before store submission.
- Configure Android release signing credentials locally; never commit a keystore or passwords.
- Configure the Apple developer team, provisioning profile, and signing in Xcode.
- Install the relevant Unity platform modules in Unity Hub.
- Profile and test on representative Android and iOS devices. The Low preset is a starting point, not a measured performance guarantee.
- Check app background/resume behavior, safe areas, browser touch controls, thermal behavior, and orientation on devices.

The project uses a shared IMGUI HUD so this graybox prototype can keep its runtime dependencies small. It scales typography and HUD spacing against the usable safe-area width, stacks metrics on narrow screens, and provides hold-to-steer touch pads plus drag steering. This interface needs a visual device/browser pass before release.
