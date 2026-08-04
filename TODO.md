# HMZ Engine

## 🎯 Current Focus

Foundations and testability: make the engine easier to validate while keeping the intentional `Engine.X` runtime API.

### Next Actions

No additional next actions currently recorded beyond the active project tasks below.

---

## Active Projects

### Build & Project Setup

**Goal**

Keep the solution lean, documented, and continuously verifiable.

**Tasks**

### [ ] Trim `Hmz.Core.csproj` of unused Silk.NET packages

Drop unused backend packages and retain only the libraries used by the engine.

### [ ] Add a test project

Add `Hmz.Core.Tests`; there are currently zero tests in the repo.

### [ ] Add CI

Remove the leftover template name.

### [ ] Add a README

Describe the engine's architecture and intent.

**Notes**

Re-add `Silk.NET.OpenAL` deliberately once audio lands, not as leftover scaffolding.

**Completed**

### [x] Add a solution file

`Hamaze.slnx` exists at the repo root and lists the projects.

### [x] Add shared `Directory.Build.props`

Shared TFM, nullable, implicit-usings, and language settings live at the repo root.

### Engine Core

**Goal**

Keep the static engine-facing API while making core behavior deterministic and testable.

**Tasks**

### [ ] Improve Engine testability without replacing `Engine.X`

Add `Engine.ResetForTests()` or scoped static-state cleanup, internal seams for runtime services, pure helpers for viewport/input/scene/collision logic, fake `IGraphics` and `IPlatformWindowService` implementations, and a headless test host.

### [ ] Wire or remove the `HandleInput` pipeline

`Engine.Update` should call `Engine.Scenes.HandleInput()`, or the unused pipeline should be removed and gameplay should standardize on polling `Engine.Input`.

### [ ] Make ImGui integration configurable

Make ImGui opt-in through `GameOptions` and retain the callback-lifetime safety workaround.

**Notes**

Do not replace the public static surface with constructor DI or a service-locator redesign. The engine supports one active runtime host; tests must reset static state between cases.

**Completed**

### [x] Reset `GameTime.TotalTime`

Added `GameTime.Reset()` and wrapping at 24 hours to bound float accumulation error; mutation now goes through `GameTime.Advance()` instead of direct property sets.

### [x] Smooth the FPS counter

`Performance.FPS` now averages over a 30-sample rolling window instead of `1 / deltaTime`.

### [x] Add a fixed-timestep game loop

`Engine.Update` accumulates real frame time and runs `Scenes.Update`/`Collisions.UpdateCollisions()` at a fixed `Engine.FixedDeltaTime` (1/60s) step, capped at 5 catch-up steps per call to avoid a spiral of death after a stall. Rendering still runs at the variable display rate; no render-state interpolation between fixed steps yet.

### Game Objects & Scenes

**Goal**

Make scene and object lifetime, transforms, and draw behavior explicit and scalable.

**Tasks**

### [ ] Remove dead commented-out code in `Scene` and `SceneManager`

Implement UI/save hooks or delete the scaffolding.

### [ ] Fix typos and exception types in `SceneManager.cs`

Correct error messages, use appropriate exception types, and replace the stack-underflow `Console.WriteLine` path with consistent error handling.

### [x] Define explicit start-scene selection

Add a `SetStartScene<T>()` or equivalent entry-point API.

### [ ] Test or remove scene-stack machinery

Exercise `Push`, `Pop`, and `SwitchTo`, or trim unused behavior.

### [ ] Decide whether multiple components of one type are allowed

The current type-keyed dictionary permits only one instance.

### [ ] Add scene/save serialization

Implement the empty `RestoreSaveData` path.

### [ ] Reduce `Player`/`Tree` boilerplate

Introduce a shared modeled-object base or factory if more cases appear.

**Notes**

The engine commits to 3D (no separate 2D draw path); `_2D` primitives remain but are not part of the depth-tested scene graph's ordering.

**Completed**

### [x] Cache `GameObject` world transforms

`Transform` now bumps an internal `Version` whenever `Position`/`Rotation`/`Scale` changes. `GameObject.WorldMatrix`/`WorldTransform` cache against that version plus the parent's own `WorldVersion` (and parent identity, for re-parenting), so the matrix/decompose only recompute when something in the ancestor chain actually changed.

### [x] Reconcile Y-sort with 3D depth testing

Decided: the engine is 3D. GPU depth testing (already enabled in `StartMode3D`) resolves per-pixel visibility, so the `.ThenBy(i => i.WorldTransform.Position.Y)` tie-breaker in `Scene.Draw`/`GameObject.Draw` was a leftover 2D painter's-algorithm technique and has been removed; `DrawOrder` alone now controls draw sequencing.

### [x] Replace Euler rotation or offer quaternion accessors

`Transform.Rotation` is now a `Quaternion` (source of truth, matching the animation system's sampling/`Slerp`), with `Transform.EulerAngles` as a derived yaw/pitch/roll convenience accessor. `Movement` steers via `Quaternion.Slerp` instead of manual angle lerping; `Cube`/`Sphere` render matrices build from the quaternion directly. The editor inspector edits quaternion properties as degrees.

### Editor

**Goal**

Provide an in-game editor scene for inspecting and editing the current game state.

**Tasks**

No open tasks currently recorded.

**Completed**

### [x] Add an editor scene and open it with F2

`EditorScene` lives in its own `Hmz.Editor` project (referencing only `Hmz.Core`) so it can be included/excluded from a build independently of `Hmz.Game`. `UntitledGame` registers it and toggles `SceneManager.Push<EditorScene>()`/`Pop()` on F2.

### [x] Add an ImGui entity inspector

`EditorScene` draws an "Entities" list (the scene below it on the stack) and an "Inspector" panel that reflects over the selected entity's `Transform` and attached components, rendering an editable ImGui widget per public settable property.

### Rendering

**Goal**

Make rendering efficient, visually capable, diagnosable, and deliberate about backend scope.

**Tasks**

### [ ] Add lighting and materials

Extract normals and add a basic lit shader while retaining the unlit shader for UI/debug draws.

### [ ] Add frustum culling

Skip models and shapes outside the camera view.

### [ ] Unify 2D and 3D render paths

Introduce a common camera/render-pass abstraction instead of parallel hardcoded modes.

### [ ] Fix rendering resource leaks

Dispose GL textures, mesh textures, and duplicate font atlases correctly.

### [ ] Cache shader uniform locations

Avoid resolving uniform locations on every call.

### [ ] Add per-call GL diagnostics

Consider `KHR_debug` in addition to the end-of-frame error check.

### [ ] Make `Color` an immutable value type

Align it with sibling value types and add helpers such as `Lerp` and `WithAlpha`.

### [ ] Generate texture mipmaps

Prevent aliasing when textures are minified in 3D scenes.

### [ ] Decide whether a second rendering backend is needed

Commit to OpenGL-only or plan another backend deliberately.

### [ ] Decide the fate of ImGui

Build a debug UI or remove the integration until it is used.

**Notes**

`OpenGLGraphics` is currently the only implementation of `IGraphics`.

**Completed**

### [x] Add draw-call batching and instancing

2D shapes/lines and text glyphs batch into per-flush draw calls (`EndMode2D`, per `DrawText` call). Cube/Sphere and repeated non-skinned model meshes (e.g. `Tree`) use GPU instancing (`glDrawElementsInstanced`) flushed at `EndMode3D`. Skinned meshes (`Player`) keep the direct per-instance draw path since bone matrices vary per instance.

### 2D Features

**Goal**

Grow the current primitive-shape and orthographic-camera support into a usable 2D rendering path.

**Tasks**

### [ ] Add a `Sprite`/`SpriteRenderer` component

Support textured quads and source rectangles for spritesheets.

### [ ] Add sprite animation

Add frame sequences and timers.

### [ ] Add tilemap support

Render a grid from a texture atlas.

### [ ] Wire up or remove 2D collision primitives

Connect `_2D/Rectangle` and `_2D/Circle` to a 2D collision path or remove them.

### [ ] Use or remove `tiny_dungeon.png`

It is currently never loaded.

### [ ] Decide the 2D/3D `GameObject` model split

Decide whether both modes should share one object/transform model.

**Notes**

The 2D side currently provides primitive shapes and an orthographic camera, but no sprite system.

### 3D Features

**Goal**

Expand the 3D pipeline with useful geometry, lighting data, and scalable asset loading.

**Tasks**

### [ ] Extract and use vertex normals for lighting

Add normals to the glTF import pipeline.

### [ ] Add procedural mesh primitives

Consider capsule, plane, and cylinder meshes beyond `Cube` and `Sphere`.

### [ ] Add async/background model and texture loading

Keep file loading off the main thread and upload GPU resources on the main thread.

**Completed**

### [x] Consider instanced rendering

Repeated meshes such as trees and props now instance via `glDrawElementsInstanced`, grouped by shared `Mesh` (`ContentManager` already caches `Model`/`Mesh` by asset path, so multiple instances of the same asset share one `Vao`).

### Physics & Collision

**Goal**

Keep the current kinematic AABB approach correct and scalable without introducing a full rigid-body engine.

**Tasks**

### [ ] Add spatial partitioning

Replace O(n^2) all-pairs collision checks with a uniform grid or similar structure.

### [ ] Add non-AABB colliders

Add at least a sphere collider for round objects.

### [ ] Add a raycasting API

Support ground checks, line-of-sight, and picking.

### [ ] Use or remove `CollisionLayer.Enemy` and `NPC`

They are defined but unused.

### [ ] Validate Solid/Trigger mask configuration

Warn about invalid or contradictory layer/mask setups.

### [ ] Add a 2D collision path

Coordinate this with the 2D feature decision.

**Notes**

`MoveAndCollide` remains the intended level; gravity, velocity, and projectiles stay in game-side components. Do not add a physics engine for the current scope. A thin `CharacterBody` helper could accumulate velocity and call `MoveAndCollide` without introducing rigid-body simulation.

### Input

**Goal**

Provide complete digital, analog, text, and remappable input behavior.

**Tasks**

### [ ] Add analog action strength

Support analog gamepad axes in action bindings instead of returning only `0f`/`1f`.

### [ ] Add text-input event capture

Hook text input and key repeat for UI, chat, and debug-console use.

### [ ] Persist input remapping

Save and load custom bindings.

### [ ] Detect mouse double-clicks

Add a double-click query/event.

### Content & Assets

**Goal**

Make game-authored assets loadable, cacheable, and safe to stream.

**Tasks**

### [ ] Implement `ContentManager.LoadShader`

Support user-authored shaders loaded by game projects instead of only embedded core resources.

### [ ] Add async/streaming asset loading

Move blocking file reads off the main thread and upload GPU resources on the main thread.

### [ ] Add a data-driven asset/scene descriptor format

Support a simple JSON or binary format instead of hardcoding every object and level in constructors.

**Completed**

### [x] Wire up `ContentManager`'s asset cache

Texture, model, and font loads reuse cached assets by resolved path; texture GPU disposal remains separate.

### Audio

**Goal**

Add basic sound, music, mixing, and eventually positional audio.

**Tasks**

### [ ] Implement audio

Add an `AudioManager`/`Engine.Audio` service, sound-effect loading and playback, music streaming/looping, and master/SFX/music mixing.

### [ ] Add positional 3D audio

Add it after basic playback works.

### Platform & Hosting

**Goal**

Choose and implement the supported host platforms without accidental backend bloat.

**Tasks**

### [ ] Add Linux/macOS hosts if cross-platform is approved

Add platform-appropriate `IPlatformWindowService` implementations.

**Notes**

`Hmz.Windows` is currently the only executable/host and the only platform window service implementation. Decide whether cross-platform hosting is a goal. If Windows-only is intentional, remove unused non-Windows native asset bloat and document the decision in the README.

**Completed**

### [x] Preserve aspect ratio on resize

Fixed logical resolution and fitted viewport; added black letterboxing.

### [x] Allow fullscreen mode

Added fullscreen/windowed switching and windowed-size cycling from gameplay input.

### Cross-Cutting Cleanup

**Goal**

Keep the young engine free of stale scaffolding and duplicated utility logic.

**Tasks**

### [ ] Sweep for and remove other dead code

Delete commented-out blocks and unused enum members; prefer git history over comments for old code.

**Completed**

### [x] Add a shared math utility helper

Added `Hmz.Core/MathHelper.cs`; replaced duplicated angle and zoom clamp logic.

---

## Backlog

No additional backlog items beyond the active project tasks currently recorded below.

---

## Ideas

- Future rendering backends beyond OpenGL, if the engine's scope requires them.
- A debug UI if ImGui becomes useful after its integration is made configurable.
- Additional procedural primitives such as capsule, plane, and cylinder meshes.

---

## Technical Notes

### Engine static service locator

Keep the static `Engine.X` API. Do not replace it with constructor DI or a service-locator redesign.

Testing should instead use:

- `Engine.ResetForTests()` or scoped static-state cleanup.
- Internal seams for runtime services.
- Pure helpers for viewport, input, scene, and collision logic.
- Fake `IGraphics` and `IPlatformWindowService` implementations.
- A headless test host without a real window or OpenGL context.

The engine assumes one active runtime host per process. Tests must reset static state between cases.

### Rendering backend scope

`OpenGLGraphics` is the only `IGraphics` implementation today. Either commit to OpenGL-only and remove unused D3D/Vulkan/WebGPU packages, or plan a second backend deliberately.

### 2D and 3D object model

The current 2D side has primitive shapes and an orthographic camera, while the 3D side has models, meshes, and depth testing. Decide whether both modes share one `GameObject`/`Transform` model or need distinct lightweight 2D variants.

### Physics scope

The intended physics level is kinematic/character movement through direct AABB displacement and resolution. Gravity, velocity, knockback, projectile motion, and similar behavior remain the responsibility of game-side components.

### Platform scope

`Hmz.Windows` is currently the only host even though Silk.NET is cross-platform and native runtime assets for Linux/macOS may be pulled into `bin/`. The cross-platform decision should determine whether to add hosts or remove the unused native asset bloat.

### Audio scope

`Silk.NET.OpenAL`/`XAudio` may be referenced, but the engine currently has no audio code, `AudioManager`, `Sound`, or `Music` types. Basic playback should come before positional 3D audio.
