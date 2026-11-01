# Stylized Art and Texture Pass

**Date:** 2026-11-01  
**Status:** Source changes and a Unity Build & Run completed; final browser appearance still needs a visual review.

## Direction

The approved reference is a colorful, stylized Thai floating market viewed from behind a longtail boat. The Unity scene is built from runtime geometry and repeatable material maps; the reference image itself is not used as a game texture. The current target is a readable, polished arcade look, rather than physically realistic water or a photorealistic scene.

## Work recorded

- Added repeatable albedo, normal, and surface maps for teak, market timber, canal water, terracotta roof tiles, and woven awnings in `Assets/Resources/TurboBoats/Textures/`.
- Added `Assets/Shaders/StylizedCel.shader` and `Assets/Scripts/StylizedTextureLibrary.cs` to provide cel-shaded lighting and shared runtime texture loading/application.
- Added editor texture import configuration for repeat wrapping, mipmaps, filtering, normal decoding, and linear surface data.
- Applied the maps to the runtime longtail boat, canal water, market timber, roofs, canopies, and obstacles; added UVs to procedural meshes that need them.
- Expanded the first market environment with stalls, produce, shopfronts, palms, and a bridge, and adjusted the camera framing for the boat chase view.
- Updated the README to describe the texture assets and their runtime use.

## Build and review status

- Unity **Build & Run** completed successfully on 2026-11-01 and opened the WebGL player in Chrome.
- The final browser frame was not visually inspected in that session, so the appearance should still be reviewed in Unity/browser and tuned against the approved reference.
- Build output under `Builds/` is ignored by Git. The source assets, shader, importer, scene/script changes, and documentation are the files intended for this source update.
- This is still a prototype art pass. It does not yet establish Subway Surfers-level polish, production-ready physics, performance qualification, or release readiness.

## Main files

- `Assets/Scripts/EndlessRiverWorld.cs` — market layout, scenery, materials, and procedural UVs.
- `Assets/Scripts/ThaiLongtailBoatVisual.cs` — boat materials and hull UVs.
- `Assets/Scripts/StylizedTextureLibrary.cs` — runtime material and map setup.
- `Assets/Editor/StylizedTextureImporter.cs` — map import settings.
- `Assets/Shaders/StylizedCel.shader` — stylized lighting shader.
- `Tools/generate_stylized_textures.py` — source generator for the repeatable texture maps.

## Follow-up

Review the running game beside the concept image, then tune map scale, lighting, water readability, environment density, and boat framing based on that comparison. Keep movement responsive and forgiving for the arcade direction; tune physics separately from the art pass.
