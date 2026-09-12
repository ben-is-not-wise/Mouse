# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Mouse is a 2D side-scroller game built in Unity 6000.6.0f1 (Unity 6). It is a Unity Editor project — there is no CLI build/test/lint pipeline; development happens through the Unity Editor and Visual Studio/Rider, and is typically driven through the UnityMCP integration when available in this environment.

There is no NavMesh/A* pathing in this game — enemy navigation uses steering (`wallInFront`/`dropInFront` checks), not pathfinding.

## Code Style (Assets/Scripts/README.md)

- Standard C# camelCase; braces always used, always on a new line.
- Private fields: plain camelCase, no `_`/`m_` prefix — reference via `this.` inside the class. Exception: `[SerializeField]`/`[SerializeReference]` fields, which skip the `this.` prefix.
- Most specific visibility modifier everywhere, except standard Unity messages (`void Update()`, no modifier).
- No `public` fields — use properties, or `[SerializeField] private`/`protected` for editor-exposed values.
- Interfaces prefixed with `I`.
- Prefer full English words over abbreviations (except common ones like max/min).
- Use `this.` when referencing MonoBehaviour members (`gameObject`, `transform`, etc.).
- `Debug.Log` calls inside a MonoBehaviour/ScriptableObject should pass `this` as the context arg.
- Favor comment-free code; don't add comments unless asked.

Follows https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md where not overridden above.

## Architecture

All game code lives under `Assets/Scripts`, namespace `HackedDesign`, split into a few assembly definitions (`Mouse.asmdef` main assembly, plus `Mouse.Editor.asmdef`, `Mouse.Extensions.asmdef`, `Mouse.Utils.asmdef`).

### Game state machine

`Game` (`Assets/Scripts/Game.cs`) is an `AutoSingleton<Game>` and the root object. It owns `Player`, `Level`, `EnemyManager`, `DialogManager`, `GameData` (save/session data), `GameSettings` (a `ScriptableObject` of designer-tunable values and debug skip-flags), and the `UIManager`. Unity's `Update`/`LateUpdate`/`FixedUpdate` on `Game` simply delegate to `CurrentState`.

Top-level game flow is a hand-rolled state machine: `IState` implementations under `Assets/Scripts/State/` (menu, intro cards per act, playing, paused, level load, mission select, intermission, death, level end, etc.). `Game.SetState*()` methods swap `CurrentState`, calling `End()`/`Begin()` (and `Suspend()`/`Resume()` for pause). New top-level flow states go here, following the existing `AbstractState`/`IState` pattern; `TransitionsToAttribute` documents allowed transitions between states.

### Character

`Assets/Scripts/Character/` holds the player character: `Character/State/` for per-character states (`ICharacterState`: idle, battle, sitting, sleeping, dead), `Character/Commands/` for a command pattern driving character actions (`ICharacterCommand`/`ICharacterExecute`: move, jump, roll, crouch, aim, interact, knockback, freeze, outfit swap, etc. — issued via `Character.ExecuteCommand(...)`), and `Character/OS/` for the in-fiction "hacking" operating system abilities (`OperatingSystem`, `Hack`, `Ping`, `EMP`, `Sandy`, `OSHex`).

### Enemies

`Assets/Scripts/Enemies/` — `EnemyManager` tracks/spawns enemies (`EnemySettings`, `EnemySpawnSet`, `EnemyType`), `EnemyController` drives a single enemy. Enemy AI (`Enemies/Ai/`) is mid-migration from a legacy `IEnemyState` state machine (`Enemies/State/`) to an opt-in Utility AI brain (`UtilityBrain`, `DefaultBrain`, `IUtilityAction`, `AiSteering`, `EnvSensor` for wall/drop detection). When touching enemy behavior, check whether the enemy's brain is on the utility system or the legacy state machine before assuming a pattern — a status-effect override hook for the utility brain is still deferred/unimplemented.

### Other areas

- `Environment/` — level/building/prop scripts (`Environment/Fixed/` for fixed set-pieces). Building prefabs are prefab **variants** — edit shared parts on the base assets directly rather than via `PrefabUtility.LoadPrefabContents`/`SavePrefabAsset`, which throws on variants.
- `Items/` — pickups/equipment, `Items/Barriers/` for obstacle/barrier objects.
- `Missions/` — `Mission`/`MissionTimer` for mission definitions and timed objectives.
- `Projectiles/`, `FX/` (incl. `FX/DamageNumbers/`) — projectile behavior and visual feedback.
- `UI/` — `UIManager`/`IUIManager` and per-screen presenters (`UI/Computer/` for the in-game "computer"/OS UI).
- `Data/` — `GameData`, persisted/session game state (distinct from `GameSettings`, which is designer-authored config). Note: inventory-related config currently lives in `CharacterSettings` rather than a dedicated OS settings `ScriptableObject` — a split has been discussed but is deferred.
- `Extensions/`, `Utils/` — shared helpers (own assemblies); `Extensions.EnsureNotNull(...)` is used for null-guarding required references (see `Game.Initialization()`).
- `Editor/` (and per-folder `Editor/` subfolders, e.g. `Character/Editor/`) — editor-only tooling, in `Mouse.Editor.asmdef`.

### Unity asset conventions

- Don't hand-write `.controller` (Animator Controller) YAML — generate/modify via the `AnimatorController` API. An NRE in the Animator window after an external rewrite usually means a stale window, not a corrupted asset.
- UI `Image` components used as cursors/reticles need Raycast Target disabled, or they will block clicks on interactable UI underneath them.
