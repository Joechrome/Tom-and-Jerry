# Cheese Dash — Task Plan v1.0

**Companion documents:** `Game-Spec.md` (what we're building) · `Backlog.md` (what's in scope, and for whom)

---

## 1. The headline change to the original plan

The Planning Doc specifies four tasks **in sequential order** totalling 14 dev-days:

> Finding Assets (2d, Andy) → Randomization (5d, Andrew) → Dynamic Environment (5d, Joe) → Points & Lives (2d, Josh)

This is unworkable as written, for three reasons:

1. **It idles three people.** If the tasks are sequential, Josh waits 12 days before starting, and Joe waits 7. The project takes 14 days of wall-clock time while three teammates watch.
2. **It under-counts the work.** Four tasks are listed, but the game needs at least eight systems. **Nothing owns the UI, the screens, the audio, the player controller, the WebGL build, or the leaderboard.** The WebGL build in particular is the entire delivery target and had no owner.
3. **It assumes sequential dependency that mostly doesn't exist.** Only the *shared skeleton* is a genuine blocker. Once the state machine and a few interfaces exist, the four tracks are independent and can run in parallel.

**Revised shape:** a 2-day foundation that freezes the interfaces, then 5 days of genuine parallel work, then integration. **11 days wall-clock instead of 14 — with everyone busy.**

---

## 2. Schedule

| Phase | Days | Goal | Exit criteria |
|---|---|---|---|
| **0 — Foundation** | 1–2 | Freeze interfaces so 4 people can work without touching each other's files | State machine runs, input works, `WorldSpeed`/`LivesSystem`/`ScoreSystem` stubs publish their contracts, a placeholder scene scrolls, and a hello-world **WebGL build is live at a URL** |
| **1 — Parallel build** | 3–7 | Four independent tracks against frozen contracts | Every MUST story complete to its acceptance criteria in each owner's dev scene |
| **2 — Integration** | 8–9 | Wire it into one scene; ship the real WebGL build | The full playable loop works end-to-end from a hosted URL, on a non-dev machine |
| **3 — Playtest & polish** | 10–11 | Answer the three questions in the Planning Doc; fix what they expose | Three playtest questions answered with data; jump feel and difficulty curve tuned |

### Phase 0 in detail — the interface freeze (Days 1–2)

This phase exists so that Phase 1 has no dependencies. It delivers **contracts, not features.**

| Deliverable | Owner | Notes |
|---|---|---|
| `GameManager` state machine (`Title`/`Playing`/`Paused`/`GameOver`) with a public `State` and `OnStateChanged` | Andrew | Every other system reads this. Nothing else may define game state. |
| **Flip Active Input Handling to `Both`** and commit **alone** | Andrew | See spec §5.1 — with the current setting, every `Input.GetKey` call throws. Blocks all movement code. Tell all four members the moment it lands; it forces a re-import and an editor restart. |
| `WorldSpeed` with `Current` + `Changed` (stub returning a constant) | Joe | Andrew's spawner depends on this. Must exist on day 2 even if it returns `5f`. |
| `LivesSystem.Current` + `OnLivesChanged`, `ScoreSystem.Current` | Josh | Joe's Tom-pursuit and Andy's HUD depend on these. Stubs are fine. |
| Folder structure + one dev scene per member | Andrew | `Assets/Scenes/Dev/{Andy,Andrew,Joe,Josh}.unity` |
| Sprite Preset created and applied; Preset Manager configured | Andy | Spec §5.2 |
| Pixel Perfect Camera + a scrolling placeholder in the dev scene | Joe | Proves the motion model (spec §2.8) before real art arrives |
| **Hello-world WebGL build hosted at a real URL** | Andrew | De-risks M17 on day 2 instead of day 11. If hosting is going to be a problem, find out now. |

> **Rule:** after Phase 0, the interfaces in this table are **frozen**. Changing one requires telling the whole team, because three other people are building against it.

---

## 3. Ownership

### Why this differs from the Planning Doc

The original roles are kept as the *core* of each person's load — Andy on art, Andrew on obstacle randomization, Joe on the dynamic environment, Josh on points and lives. Three additions were necessary:

- **The player controller** (jump/crouch feel) had no owner. It goes to **Joe**, because "dynamic movement of the environment relative to the character" is inseparable from the character's own motion, and because it pairs with the speed system Joe owns.
- **The UI screens** had no owner. They go to **Andy**, who has the lightest load and whose work is visual anyway. **Josh keeps all the data behind them** — score, lives, and leaderboard logic.
- **The WebGL build and hosting** had no owner. It goes to **Andrew** as tech lead, and it starts in Phase 0 rather than at the end.

### Per-person load (MUST stories)

| Owner | Dev-days | Stories | Verdict |
|---|---|---|---|
| **Andrew** | 5.5 | M0, M4, M5, M17 | Critical path. M17 sits in Phase 2, so 4.5 dev-days inside the parallel phase. |
| **Andy** | 5.0 | M14 (art), M18, M10, M12 (UI), M15 | Well loaded; art is also the top schedule risk (see §6). |
| **Josh** | 4.0 | M6, M7, M8, M9, M11, M13 | Balanced after the UI reassignment. |
| **Joe** | 3.5 | M1, M2, M3, M14 (env), M16 | Lightest — deliberately, because the jump *feel* work (M2) is the least predictable estimate here and the most important to get right. |

Parallel-phase ceiling: **5.0 dev-days**, which fits a 5-day phase. Total MUST effort is 17.0 dev-days across 4 people.

### File ownership map — one owner per file

**Nobody edits a file they do not own.** If you need a change in someone else's file, ask them or add a public method — do not "just quickly fix it." This single rule prevents most merge conflicts in a Unity project.

| Path | Owner |
|---|---|
| `Assets/Scripts/Core/GameManager.cs`, `GameConfig.cs`, `InputReader.cs` | Andrew |
| `Assets/Scripts/Spawning/ObstacleSpawner.cs`, `SpawnTable.cs`, `DifficultyCurve.cs`, `ObjectPool.cs` | Andrew |
| `Assets/Scripts/World/WorldSpeed.cs`, `Scroller.cs`, `ParallaxLayer.cs`, `GroundScroller.cs`, `TomPursuit.cs` | Joe |
| `Assets/Scripts/Player/PlayerController.cs`, `PlayerAnimator.cs` | Joe |
| `Assets/Scripts/Progression/ScoreSystem.cs`, `LivesSystem.cs`, `HeartSpawnRule.cs` | Josh |
| `Assets/Scripts/Data/LeaderboardStore.cs` | Josh |
| `Assets/Scripts/UI/HUD.cs`, `TitleScreen.cs`, `PauseScreen.cs`, `GameOverScreen.cs` | Andy |
| `Assets/Art/Sprites/**`, `Assets/Art/Audio/**`, sprite presets | Andy |
| `Assets/Scenes/SampleScene.unity` | **Andrew only** |
| `Assets/Scenes/Dev/{Name}.unity` | Each member, individually |
| `Assets/Prefabs/**` | Whoever creates the prefab; **one owner per prefab** |
| `ProjectSettings/**`, `Packages/manifest.json` | Andrew only |

### `ScriptableObject` configs are shared — treat them as contested

`DifficultyCurve`, `SpawnTable`, and `GameConfig` are edited by tuners rather than coders, so they will conflict. Rule: **one person tunes at a time.** Tuners announce "I'm tuning the curve" before touching it. This is still far better than four people editing gameplay constants in code.

---

## 4. Git workflow

### 4.1 Repository hygiene fixes to make first

1. **Both solution files are tracked and should not be.** `Chasing Game.slnx` and `Chasing-Game.slnx` are Unity-generated, are not ignored (`.gitignore` covers `*.sln` but not `*.slnx`), and will churn on every machine. Add `*.slnx` to `.gitignore` and `git rm --cached` both. *(`Library/` is already correctly ignored and the repo is a healthy 2.9 MB — this is the only hygiene problem found.)*
2. **Add Unity's YAML merge driver** to `.gitattributes`:

```gitattributes
*.unity   merge=unityyamlmerge
*.prefab  merge=unityyamlmerge
*.asset   merge=unityyamlmerge
*.meta    merge=unityyamlmerge
```

   Each member then runs this **once, in their own clone** (it is local config):

```powershell
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'
```

   *`UnityYAMLMerge` is a heuristic, not a solver. When it fails, do not hand-merge a `.unity` file — take one side wholesale and re-apply the other person's change by hand in the editor.*

### 4.2 Rules

- **Branches:** one per story or per person — `feat/joe-scrolling`, `feat/andrew-spawner`. Never commit directly to `main`.
- **Small, frequent commits.** A commit that spans three systems cannot be reviewed or reverted.
- **Pull before you start; push when you stop.** Do not leave work local overnight.
- **The input-handler change gets its own commit, alone.** Mixing a `ProjectSettings` re-import into a feature branch makes that branch unreviewable.
- **Never commit to the main scene.** See §4.3.

### 4.3 The one-scene problem

Four people building one game will want to edit `SampleScene.unity` constantly. Two people editing it in parallel *always* conflicts, and a bad scene merge can silently delete objects.

**The approach:**

1. **Everyone works in their own dev scene.** Each member wires and tests their own system in `Assets/Scenes/Dev/{Name}.unity`. These almost never conflict with each other.
2. **Build prefabs, not scene edits.** A feature should be a prefab (e.g. `ObstacleSet_A.prefab`, `HUD.prefab`) plus scripts. Prefabs merge far more reliably than scenes, and they let integration be drag-and-drop.
3. **Andrew owns `SampleScene.unity`.** He is the only person who edits it. When a phase ends, everyone posts their prefab + a two-line description of where it goes, and Andrew wires it.
4. **Integration windows:** end of each day, and formally at Phase 2. Outside those windows, no commits to the main scene.
5. **If a scene conflict still happens: do not hand-merge.** Choose one side, then re-apply the other's changes in the editor and commit the result.

---

## 5. Dependencies and critical path

```
Phase 0 (Days 1–2)
  GameManager ──┬─▶ HUD / screens (Andy)          ─┐
  Input = Both ─┼─▶ PlayerController (Joe)         │
  WorldSpeed ───┼─▶ ObstacleSpawner (Andrew)       ├─▶ Integration (Days 8–9)
  LivesSystem ──┼─▶ TomPursuit (Joe)               │      └─▶ WebGL build (M17)
  ScoreSystem ──┴─▶ Leaderboard (Josh)             ─┘
```

**Hard couplings to respect:**

| Dependency | From → To | Consequence if broken |
|---|---|---|
| `WorldSpeed.Current` | Joe → Andrew | Spawner cannot compute fair, speed-correct gaps. **The most important interface in the project.** |
| `LivesSystem.Current` + `OnLivesChanged` | Josh → Joe | Tom proximity can't track health. |
| `GameManager.State` | Andrew → everyone | Pause and state gating break; systems keep running on the Title screen. |
| Sprite import preset | Andy → everyone | Art lands blurry and every sprite must be re-imported later. |
| Tom's spritesheet | Andy → Joe | Tom pursuit can't be finished. **Joe must build against a placeholder rectangle so he is never blocked.** |

**Critical path:** Andrew's M4+M5 (3.0 dev-days, the largest single load) → integration → WebGL. Andrew is the bottleneck; if he slips, Joe should take M17.

---

## 6. Risk register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| **No Tom art exists** | High | High | The antagonist has **zero** art while Jerry does. Andy prioritises the Tom run sheet in week 1. Joe builds `TomPursuit` against a coloured rectangle so code never waits on art. |
| **Legacy input crashes at runtime** | **Certain** if unaddressed | High | Project is set to New Input System only, so `Input.GetKey` **throws**. Flip to `Both` in Phase 0 and announce it. Otherwise three people independently write broken jump code. |
| **Pixel art renders blurry** | High | Medium | All sprites are currently Bilinear + compressed + Tight-meshed. Preset in Phase 0 (M18), Pixel Perfect Camera in M14. |
| **Four people, one scene** | High | High | Dev scenes + prefabs + single scene owner + integration windows (§4.3). |
| **WebGL hosting or build fails late** | Medium | High | Hello-world WebGL build hosted on **day 2**, not day 11. Memory setting bumped if load fails. |
| **Difficulty becomes unfair at speed** | Medium | High | Gaps computed in **seconds** and converted to units at spawn (spec §2.3). Explicit test at max speed in Phase 2. |
| **Jump feels bad** | Medium | High | It is playtest question 1 and the hardest thing to fix late. Tune it in week 1 with 2–3 testers, never at the end. Variable height + coyote time + input buffer are all in M2. |
| **Scope creep from the backlog** | High | High | `Backlog.md` §WON'T is binding until v1 ships. New ideas go into COULD, not into the current sprint. |
| **Editor version mismatch** | Medium | Medium | All four run **6000.3.23f1**. `ProjectVersion.txt` is committed. Opening in a different version rewrites `ProjectSettings` and creates noisy diffs. |
| **Audio sourcing/licensing** | Medium | Low | SFX only, no music. Use CC0 sources and record attributions in `Assets/Art/Audio/CREDITS.md` as you go, not at the end. |
| **A member's track blocks the others** | Medium | Medium | Phase 0 interface freeze. Whoever finishes early takes work off the critical path (§3), rather than polishing their own system. |

---

## 7. Playtest plan (Phase 3, Days 10–11)

The Planning Doc names three questions. Here is how to actually answer them.

**Method:** 5+ testers, mixed experience. **Do not explain the controls** — the Title screen is supposed to teach them, so a tester who can't start is a finding about M10, not about the tester. Watch in silence. Ask what they were thinking *after*, not during.

| Planning Doc question | What to measure | Bad reading means |
|---|---|---|
| **"Does the jump feel unsmooth?"** | 1–5 rating; count of "I pressed jump and nothing happened"; whether testers die specifically to mistimed jumps | Input buffering or coyote time is off, or pooling is causing frame hitches. Tune gravity/jump height and `jumpBuffer`. |
| **"Does the UI placement make sense and is it intuitive?"** | Can they find their lives without being told? Do they notice a life lost? Do they notice a heart appear? | Icons are too small or too far from the play area; heart spawn isn't telegraphed. |
| **"Is the increase in difficulty too aggressive?"** | Distance/time at death for each tester; histogram of where deaths cluster | A tight cluster at one distance = a specific spawn-gap or mix problem, not a general "too hard." Fix by widening the gap floor or delaying the speed ramp — **do not** flatten the whole curve. |

**Additional metrics worth collecting:**

- **Crouch usage rate.** If nobody crouches, HIGH obstacles aren't telegraphed well enough and M3's core purpose fails.
- **"Could you tell why you died?"** If not, the failure wasn't readable — usually an unfair gap or a collision box that doesn't match the sprite.
- **Run-to-run variance.** Three runs by the same tester should not differ by more than ~2× in score. Otherwise randomness, not skill, is deciding.
- **Time to first successful jump.** Should be under 10 seconds.

**Because difficulty lives in a `ScriptableObject`** (M5), tuning after playtest is editing numbers in the Inspector — no code changes, no rebuild risk, no merge conflicts.

---

## 8. Joe's slice — detailed implementation plan

*You own the world's motion and the player's motion. This is the most feel-critical work in the project, and it has the clearest contracts.*

### 8.1 What you own

| File | Responsibility |
|---|---|
| `World/WorldSpeed.cs` | **Single source of truth for scroll speed.** Everything reads it; nothing else stores a speed. |
| `World/Scroller.cs` | Generic: translates a transform in −X at `WorldSpeed.Current × parallaxFactor`, wraps when off-screen. Reused by every background layer and the ground. |
| `World/GroundScroller.cs` | Endless tiled ground using two leapfrogging copies. |
| `World/ParallaxLayer.cs` | Sets the factor per background layer (sky 0.2, far 0.4, mid 0.7, ground 1.0). |
| `World/TomPursuit.cs` | Tom's X driven by remaining lives; lunge on hit; pounce at 0. |
| `Player/PlayerController.cs` | Jump (variable height), crouch, coyote time, input buffer, grounding check. |
| `Player/PlayerAnimator.cs` | Drives the Animator from player state. |
| `CameraRig.cs` / camera setup | Pixel Perfect Camera config, reference resolution. |

### 8.2 Contracts you publish (Phase 0 — do these first)

```csharp
// World/WorldSpeed.cs  — the project's most depended-upon value
public class WorldSpeed : MonoBehaviour
{
    public static float Current { get; private set; }
    public static event System.Action<float> Changed;
    // Reads DifficultyCurve, ramps over time, gated on GameManager.State == Playing
}

// World/TomPursuit.cs reads, never writes:
//   LivesSystem.Current  +  LivesSystem.OnLivesChanged   (Josh)
// PlayerController reads, never writes:
//   GameManager.State                                     (Andrew)
```

**Unity gotcha:** `static` state survives between Play sessions when *Enter Play Mode Options* has domain reload disabled, which Unity 6 defaults toward for speed. Reset statics explicitly:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void ResetStatics() { Current = 0f; Changed = null; }
```

Otherwise `WorldSpeed` keeps the last run's value on the next play and the game starts at max speed.

**Andrew depends on your contract**, so land `WorldSpeed` on day 2 even if it returns a hard-coded `5f`. He cannot compute fair gaps without it.

### 8.3 Why speed must be centralized

Spec §2.8 locks the motion model: **the player never moves in world X; the world moves toward the player.** Two consequences you must enforce:

1. **Every scrolling object reads `WorldSpeed.Current`.** If the ground owns its speed and the obstacles own theirs, they drift apart and the game visibly tears — obstacles sliding against the ground. This reads as a broken game, not a tuning issue.
2. **Gaps must be computed in seconds, not units.** Andrew's spawner does `gapUnits = gapSeconds × WorldSpeed.Current`. This is what keeps difficulty fair as speed rises; it is the fix for playtest question 3, and it is *your* contract that makes it possible.

**Pause implementation:** set `Time.timeScale = 0f` and `AudioListener.pause = true`. This freezes everything uniformly and automatically, so no system can forget to check the state. **Every UI animation must then use `Time.unscaledDeltaTime`**, or the pause menu's own animations will freeze too.

### 8.4 Suggested order

| Day | Work |
|---|---|
| 1–2 (Phase 0) | Dev scene; `WorldSpeed` stub; camera + Pixel Perfect; one placeholder quad scrolling correctly |
| 3 | Parallax layers + tiled ground, all reading `WorldSpeed`; verify no seam and no drift |
| 4 | `PlayerController`: jump with variable height, coyote time, input buffer; `PlayerAnimator` with placeholder art. **Tune with 2 testers the same day** — do not defer feel work. |
| 5 | `GroundScroller` polish, `Time.timeScale` pause, screen fit / letterbox (M16) |
| 6 | `TomPursuit` against a placeholder rectangle; wire `LivesSystem` events |
| 7 | Buffer day / help the critical path (M17 WebGL if Andrew is behind) |

### 8.5 Verify before you call it done

- Frame-rate independence: at 30 fps and 144 fps, the world covers the same distance per second.
- No drift: after 3 minutes, obstacles still sit exactly on the scrolling ground.
- Pause truly freezes: no scroll, no difficulty ramp, no score, no spawns.
- Jump feel: a tester says "the jump feels smooth" without prompting (this is playtest question 1).
- `WorldSpeed` resets correctly on a second Play session.

---

## 9. Definition of Done

A story is done when **all** of these hold:

1. It meets every acceptance criterion in `Backlog.md`.
2. It works in the owner's dev scene **and** after integration into `SampleScene`.
3. It behaves correctly in a **WebGL build**, not just in the editor. (WebGL is stricter about threading and audio than the editor.)
4. It respects `GameManager.State` — nothing runs on the Title screen or while paused.
5. No warnings or errors in the console.
6. It has been seen working by someone other than its author.
7. It's committed on a branch and pushed.
