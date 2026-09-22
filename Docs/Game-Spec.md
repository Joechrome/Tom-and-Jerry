# Cheese Dash — Game Specification

**Version 1.0** · Reconciled from *First Game Planning Doc* and *Product Backlog*
**Team:** Andy M · Andrew Z · Joe S · Josh C

---

## 0. How to read this document

The **Planning Doc is the vision of record.** Where the Backlog contradicted it, the Planning Doc wins — unless §7 says otherwise. §7 records every conflict and its resolution so nobody has to re-litigate it. §8 lists what is still genuinely undecided.

This document keeps the Planning Doc's MDA structure (Mechanics / Dynamics / Aesthetics) so it maps directly onto the original assignment.

---

## 1. Overall Vision

A single-player, competitive **endless runner**. You play Jerry, who has just stolen a piece of cheese and been spotted by Tom. You run, forever, through a bright cartoon kitchen. You cannot fight back and you cannot stop — you can only jump, duck, and keep going. Obstacles come at you faster and faster; hearts let you survive your own mistakes. Your score is how far you got, and the leaderboard is why you try again.

- **Genre:** endless runner / reflex
- **Mode:** single player, competitive via a scoreboard
- **Audience:** any age, no game experience required
- **Platform:** browser (WebGL), keyboard only
- **Session length:** 30 seconds to 3 minutes

---

## 2. Mechanics — what the game does

### 2.1 Core loop

```
Title ──Space──▶ Run ──hit, lives > 0──▶ (continue, faster)
                  │  │
                  │  └──collect heart──▶ +1 life (max 3)
                  │
                  ├──P──▶ Pause ──P──▶ Run
                  │
                  └──lives == 0──▶ Tom catches Jerry ──▶ Game Over ──Space──▶ Title
```

### 2.2 Player actions

| Action | Key | Behaviour |
|---|---|---|
| Jump | `Space` | **Variable height** — tap for a short hop, hold for a full jump. Cannot jump while airborne (no double jump). |
| Crouch | `C` (hold) | Shrinks the collider and squashes the sprite. **Only while grounded.** Required to pass under overhead hazards. |
| Pause | `P` | Freezes the world. Not an action — see §2.9. |

**Feel requirements** (these are the ones playtest question 1 will judge):
- **Coyote time:** ~0.10 s of grace to jump after walking off an edge.
- **Input buffer:** ~0.12 s — a jump pressed slightly too early still fires on landing.
- **Crouch is never mandatory to survive a jumpable obstacle**, and jump is never mandatory to survive a duckable one. Every obstacle must be solvable by exactly one obvious action, telegraphed by its shape.

> **Gap found:** the Planning Doc requires crouch but never says *why* you would crouch in a game where everything comes at you along the ground. Without overhead hazards, crouch is a dead button. §2.3 fixes this by defining three obstacle archetypes. **This is a design change and needs team sign-off.**

### 2.3 Obstacles

All obstacles are **active** — every one scrolls toward the player. Nothing is static. There are three archetypes, defined by the action they demand:

| Archetype | Occupies | Required action | Purpose |
|---|---|---|---|
| **LOW** | Ground level | `Space` — jump over | The core reflex test |
| **HIGH** | Raised (chest/head height), gap beneath | `C` — crouch under | Makes crouch meaningful |
| **WIDE** | Ground level, wide footprint | `Space` — early / full-height jump | Punishes late or short jumps |
| **FAST** | Any, but travels faster than world speed | React sooner than usual | Spikes tension; Tom's paw swiping in |

**Fairness invariant (non-negotiable, enforced in the spawner):** every gap is computed in **seconds, not world units**.

```
gapUnits        = gapSeconds        × WorldSpeed.Current
reactionSeconds ≥ 0.50                          // floor, at every speed
gapSeconds      ≥ jumpAirTime + 0.15            // consecutive jumpable obstacles
```

This is the single most important line in the whole spec. If the spawner places obstacles a fixed *distance* apart, then every speed increase silently halves the player's reaction time and the game becomes unfair at speed 2× — which is exactly what playtest question 3 ("is the increase in difficulty too aggressive") will be asking about. **Express distances in seconds; convert to units at spawn time.**

### 2.4 Collectibles

| Item | Effect | Spawn rule |
|---|---|---|
| **Heart** | +1 life, up to the cap of 3 | **Only spawns when `lives < 3`.** Never spawns while at full health, so a pickup is never wasted. Must be reachable: never inside or immediately behind an obstacle's landing zone. |
| **Cheese** | Bonus points (`+50`) | *(Should-have, not MVP.)* The `cheese_WMRemoved_BGRemoved.png` sprite is already imported, so this is nearly free. |

### 2.5 Lives and failure

- Start with **3 lives**. Maximum **3 lives**. They are rechargeable, matching the Planning Doc.
- Taking a hit costs **1 life**, grants **~1.5 s of invulnerability**, and flashes Jerry. He is not stopped and the world does not slow down — the run continues.
- At **0 lives**, Tom catches Jerry: a short pounce, then the Game Over screen.
- **Tom is a presentation of the fail state, not an AI.** He has no pathfinding and no hitbox. This keeps the scope sane and is on-theme.

**Proposed signature mechanic — "Tom proximity = life meter".** *(Should-have; promote to MVP if Tom's art lands in week 1.)*
Tom's X position is driven by remaining lives: far behind at 3 lives (partly off-screen), right on Jerry's heels at 1 life. On a hit he lunges at the camera and eases back. One spritesheet, a lerp, and no new systems — but it turns the health bar into a visible predator and delivers exactly the "stress" the Planning Doc asks for (§3).

### 2.6 Score

- Score accrues with distance travelled: `score += WorldSpeed.Current × pointsPerUnit × dt` (start `pointsPerUnit = 10`).
- Consequence: scoring faster is a side effect of surviving to higher speeds. You never choose between "score points" and "stay alive" — the Planning Doc is explicit that staying alive *is* the scoring mechanism.
- Displayed as an integer, top-centre.
- Persisted best score, plus a **local top-10 leaderboard** (§2.9).

### 2.7 Difficulty

A single tunable curve drives everything, authored as a **ScriptableObject** (`DifficultyCurve`) so it can be tuned in the Inspector without anyone editing code:

| Parameter | Start | Max | Notes |
|---|---|---|---|
| `worldSpeed` | 5 u/s | 16 u/s | Slow ramp, then plateau. `timeToMax` ≈ 120 s. |
| `minGapSeconds` | 1.4 s | 0.55 s | Never below `reactionSeconds` floor of 0.50 s. |
| `obstacleMix` | LOW only | all four archetypes | Widen the mix before shortening the gap. |
| `heartChance` | 0.15 | 0.08 | Hearts get rarer as you improve, not more common. |

Rule: **introduce variety before speed.** A player should meet every obstacle type at a comfortable speed before the game gets fast. Ramp the mix over the first ~40 s; ramp speed over ~120 s.

Difficulty must **freeze while paused**.

### 2.8 World and motion model — LOCKED DECISION

The Planning Doc assigns "coding the dynamic movement of the environment relative to the character." This is resolved as follows and should not be revisited:

> **The player never moves in world X. The world moves toward the player.**

- Jerry sits at a fixed `x = 0`. Only his **Y** changes (jumps) and his **collider/sprite** changes (crouch).
- The camera is static.
- Background layers, ground, obstacles, hearts, and Tom all translate in **−X**.
- **Score is therefore ∫ speed dt**, and everything is deterministic and trivially poolable.

Every scrolling object reads its speed from **one** place — `WorldSpeed.Current` — and multiplies by its own `parallaxFactor` (1.0 for gameplay objects; 0.2–0.7 for background layers). No object may own its own speed variable. If two objects disagree about speed, obstacles visibly slide against the ground and the game looks broken.

### 2.9 Screens and HUD

| Screen | Contents | Trigger |
|---|---|---|
| **Title** | Game title/logo, control legend (`Space` jump · `C` crouch · `P` pause), best score, top-3 leaderboard, "Press Space to start" | Boot / from Game Over |
| **Playing** | Score (top-centre, text) · Lives (**top-left**) · *(optional)* timer | Space from Title |
| **Pause** | Dim overlay, "PAUSED", `Space` resume · `R` restart · `Q` to title | `P` |
| **Game Over** | "Tom caught you!", final score, distance/time survived, `NEW BEST!` flag, top-3 leaderboard, "Press Space to return to Title" | Lives reach 0 |

**HUD deviation to sign off:** the Planning Doc says health is shown "in the form of text" in the top-left. This spec instead shows **three heart icons** and keeps text for the score only. Icons are read faster than numerals during a reflex game, the `pixel-heart.png` art already exists, and the heart is already the game's life currency — so the HUD and the pickup use one shared visual language. Functionally identical, and it satisfies the Backlog's "I want to see my health so that I know when to dodge."

**Leaderboard is local.** Top-10 in `PlayerPrefs` as JSON. Rationale in §7.

---

## 3. Dynamics — how it feels

- **Skills rewarded:** reflexes and pattern recognition. Difficulty rises so the player is pushed to read obstacle *shapes* faster, not to memorise a layout.
- **Performance changes nothing but difficulty.** No upgrades, no unlocks, no power curve. The player gets better; the game gets faster. This is deliberate and is why most of the Backlog's progression ideas are cut in §6.
- **Emotional target:** stress during play; frustration and pride at failure and success. Tom closing in is the main lever for this; the leaderboard is the second.
- **Tension curve per run:** first 20 s teaches, 20–60 s is the sweet spot, past 60 s the player is gambling. Deaths should feel *earned* — the player should be able to say what they did wrong.
- **Proficiency looks like:** jumping earlier than feels natural, ducking without looking at the HUD, and noticing Tom's position rather than the life icons.

---

## 4. Aesthetics — what it looks and sounds like

- **Style:** pixel art, bright and colourful, goofy "ah!" cartoon energy. Minimalistic — no world-building, no lore.
- **Setting:** a cartoon kitchen. Cheese, condiments, ledges, mouse traps, ladles. The `Kitchen_Ledge`, `condiments`, and `Amazing background` assets already establish this.
- **Animation:** image-based sprite animation (Jerry run, Jerry jump, Jerry crouch, Jerry hurt, Tom chase, Tom pounce) plus a sparkle VFX burst on heart pickup.
- **Audio:** **sound effects only — no background music in v1**, per the Planning Doc. Short 8-bit bites: jump, land, crouch, hit/life lost, heart pickup, cheese pickup, Tom yowl (game over), new-best, UI confirm.
- **Camera:** pixel-perfect, integer-scaled, crisp at any browser window size.

**Assets required but not yet in the repository** (this is the real art gap):

| Asset | Status | Needed for |
|---|---|---|
| Jerry run sheet + animator | ✅ present | Core |
| Cheese, heart, kitchen ledge, obstacles, backgrounds | ✅ present | Core |
| **Jerry crouch** | ❌ missing | Crouch mechanic |
| **Jerry jump / hurt** | ❌ missing | Jump, hit feedback |
| **Tom chase** | ❌ missing | The title premise, game over |
| **Audio (any)** | ❌ none in repo | All SFX |

Tom is the game's antagonist and has **no art at all yet**. That is the top asset risk — see the risk table in `Task-Plan.md`.

---

## 5. Technical constraints and standards

- **Engine:** Unity **6000.3.23f1**. All four members must run this exact version; `ProjectVersion.txt` is committed. Mismatched editors rewrite `ProjectSettings` on open and cause noisy diffs.
- **Render pipeline:** URP 2D (`com.unity.render-pipelines.universal` 17.3.0). Already configured.
- **Target:** WebGL, keyboard + browser, no install.
- **UI:** `com.unity.ugui` 2.0.0 (TextMeshPro is included in Unity 6's ugui package — no extra dependency needed).

### 5.1 ⚠️ Input — must be fixed before anyone writes movement code

`ProjectSettings/ProjectSettings.asset` currently has **`activeInputHandler: 1`**, meaning *Input System Package (New)* only. Under this setting, every legacy call — `Input.GetKey(KeyCode.Space)`, `Input.GetKeyDown`, `Input.GetAxis` — **throws `InvalidOperationException` at runtime.** Four people are about to write jump code; three of them will write the legacy version from memory or a tutorial, and it will fail in a way that looks like their own bug.

**Decision:** set **Edit → Project Settings → Player → Other Settings → Active Input Handling = `Both`**, and use the simple legacy API for this project's three keys.

```csharp
bool jumpPressed  = Input.GetKeyDown(KeyCode.Space);
bool crouchHeld   = Input.GetKey(KeyCode.C);
bool pausePressed = Input.GetKeyDown(KeyCode.P);
```

Rationale: three fixed keys do not need an action-mapping layer, the Planning Doc specifies physical keys, and "Both" keeps the new Input System available if someone later wants rebinding or gamepad. The existing `Assets/Settings/InputSystem_Actions.inputactions` can stay in the project unused.

*Note:* changing this setting triggers a project-wide re-import and an editor restart. Do it in Phase 0, once, on one machine, and commit it alone.

### 5.2 ⚠️ Pixel-art import — currently wrong on every sprite

Measured from the `.meta` files in `Assets/sprites/`, all imported sprites are at Unity defaults, which are tuned for 3D photos, not pixel art:

| Setting | Current | Required | Why |
|---|---|---|---|
| `filterMode` | `1` Bilinear | **`0` Point (no filter)** | Bilinear smears every pixel edge — the single biggest cause of "my pixel art looks blurry and cheap" |
| `textureCompression` | `1` Compressed | **`0` None** | Block compression puts artefacts on flat colour and hard edges |
| `spriteMeshType` | `1` Tight | **`0` Full Rect** | Tight meshes shift pixels during squash/tiling and cause seams |
| `spritePixelsToUnits` | `100` | **one agreed value, project-wide** | Already consistent at 100 — keep it, and keep it identical for every new sprite |
| `alphaIsTransparency` | `1` | keep | Correct |
| `wrapMode` | default Repeat | **Clamp** | Prevents edge bleed on tiled backgrounds |

**Action:** create one **Sprite Preset** with these values and apply it to every sprite in `Assets/sprites/`, then enforce it in the Preset Manager so new imports inherit it automatically. Owner: Andy, in Phase 0.

### 5.3 Camera — pixel perfect

Add a **Pixel Perfect Camera** component (URP 2D) with Reference Resolution **480 × 270**, *Upscale Render Texture* on, and Assets Pixels Per Unit matching the sprite PPU from §5.2. This gives integer scaling and crisp art at any window size, and it satisfies the Backlog's "set my game tab size so the game can fit on my screen" for free.

### 5.4 WebGL and distribution

- Build target **WebGL**, compression **Brotli** (or gzip), and bump `webGLInitialMemorySize` (currently a low 32 MB) if the build fails to load.
- **A WebGL build cannot be reliably opened from `file://`** — it must be served over HTTP. Host it on **GitHub Pages** or **itch.io**.
- **Honest caveat on the "no wifi" audience:** a browser game must be downloaded once before it can run. A player with no connection at all cannot load it. Realistic reading of the requirement: *no install, no account, works on any machine with a browser* — which WebGL satisfies fully. If genuinely-offline play is required, that is a different deliverable and needs to be raised now, not at the end.
- The Backlog's "adjust the volume" and "resizable tab" stories are satisfied by the WebGL canvas configuration and the audio mixer, not by custom UI.

### 5.5 Architecture standards

- **One `GameManager` state machine** (`Title`, `Playing`, `Paused`, `GameOver`). Every other system reads state and never writes it. All time-based systems (scroll, spawn, difficulty) must be gated on `Playing` so pause is correct by construction.
- **ScriptableObject config** for all tuning (`DifficultyCurve`, `SpawnTable`, `GameConfig`). Tuners edit assets, not code — this removes most merge conflicts *and* most accidental gameplay changes.
- **Object pooling** for obstacles and hearts. Instantiate/Destroy per spawn will cause GC hitches and stutter on WebGL, which looks exactly like "the controls feel unresponsive" (playtest question 1's failure mode).
- **Prefabs, not scene edits.** See the file-ownership rules in `Task-Plan.md` — this is what keeps four people out of each other's way in one Unity scene.

---

## 6. Out of scope for v1

Explicitly cut. Each of these came from the Backlog and each is defensible as a *future* feature — but each also conflicts with the Planning Doc's "minimalistic, linear, no meta-progress" vision and none fits the remaining schedule.

| Cut | Why |
|---|---|
| Custom character image upload | Requires runtime image decoding, file dialogs, and validation — in WebGL this alone is bigger than the entire core game. Also breaks the pixel-art art direction. |
| Character customisation | Same art-pipeline cost as above; no gameplay value in a game whose identity is *being* Jerry. |
| Meta-progress / "get stronger the longer I play" | Directly contradicts "the player's performance doesn't change any game mechanics, but makes the game more difficult." Progression would destroy the leaderboard's meaning, since scores would reflect grinding rather than skill. |
| Gadgets / power-ups | Same reason. Hearts are the only item, per the Planning Doc. |
| Achievements | Nice, but pure overhead for a 2-week build with no persistence backend. Revisit after v1 ships. |
| Easter eggs | Same. Fun, but zero effect on the three playtest questions. |
| Online leaderboard / multiplayer | The target audience is explicitly offline browser users, and there is no server. Local top-10 keeps the competitive motivation the Planning Doc wants. |
| Story cutscene | Contradicts "there won't be a plot or storyline." The premise is delivered in one line on the Title screen instead. |
| Full save/resume of a run | An endless runner is 30–180 s long. Persisting the high score and settings is all "save my game" can sensibly mean here. |

---

## 7. Decision log — conflicts between the two documents

| # | Topic | Planning Doc | Product Backlog | **Decision** | Rationale |
|---|---|---|---|---|---|
| 1 | Background music | "There will be no background music." | Wants theme music + a volume slider | **SFX only in v1.** Music is a Should-have; a volume slider ships only if music does. | The Planning Doc is the vision of record. Nobody on the team does audio, and sourcing a licensed 8-bit loop is a schedule risk with no effect on the three playtest questions. |
| 2 | Story / intro | "There won't be a plot or storyline." | Wants an intro scene with the background story | **One-line premise on the Title screen.** No cutscene. | Satisfies both: the player learns why they are running ("You stole the cheese. Tom saw you. RUN.") at zero art and zero schedule cost. The Planning Doc *already* requires a controls intro page. |
| 3 | Leaderboard | Scoreboard is the stated long-term motivation, but no task owns it | "So I have the motivation to grind" | **Local top-10 in `PlayerPrefs`, shown on Title + Game Over.** | The audience is offline browser users with no server. Local persistence delivers the motivation without a backend. |
| 4 | Health display | Score top-centre, health top-left, "in the form of text" | "I want to see my health" | **Score as text (top-centre); lives as 3 heart icons (top-left).** | Icons read faster under time pressure, the heart sprite already exists, and it unifies HUD and pickup. Needs sign-off. |
| 5 | Crouch | Required (`C`), but no reason to ever use it | "I want to be able to adjust my jump" | **HIGH overhead obstacles make crouch mandatory.** Jump is variable-height. | Without overhead hazards, crouch is a dead button and the mechanic is unimplementable as written. |
| 6 | Tom's role | Jerry runs "away from Tom, try not to be caught" — never specified mechanically | No story for Tom at all | **Tom is the fail-state presentation + the proximity life meter.** Not an AI. | Removes an unowned, unscoped AI system while making the antagonist the most visible thing on screen. |
| 7 | Pause | Specified (`P`) | **No story at all** | **MVP requirement.** | The Backlog missed a mechanic the Planning Doc already mandates. Added as a story. |
| 8 | Adjustable jump | Not mentioned | "Adjust my jump" | **Variable jump height (hold = higher).** No settings slider. | Interpreted as a *feel* request, not a config request — and it directly targets playtest question 1, "does the jump feel unsmooth." A slider is a Come-later. |
| 9 | Difficulty scaling | Speed increases progressively | "Is the increase in difficulty too aggressive?" | **Gaps expressed in seconds; variety before speed.** | A fixed-unit gap becomes unfair at 2× speed. This is the concrete fix for playtest question 3. |
| 10 | Volume | Not mentioned | "Adjust the volume" | **Mute toggle only in v1** (satisfied by the WebGL/audio mixer config). | Only meaningful once music exists — see #1. |

---

## 8. Open decisions — still need the team

1. **Health as icons vs text** (§7 #4) — I recommend icons. Sign off or overrule.
2. **Tom proximity life meter** (§2.5) — recommend building it *if* Tom's run sheet exists by end of Phase 1. It is the highest-value-per-hour feature in the whole design.
3. **Cheese pickups** (§2.4) — art already exists; +50 points adds a risk/reward decision to an otherwise pure-survival game. Recommend Should-have.
4. **Starting lives = 3?** The Planning Doc says max 3 but never says the start. 3 is assumed.
5. **Online leaderboard** — only if someone wants to own a backend. Recommend no.
6. **Camera reference resolution** — 480 × 270 assumed.

---

## 9. Definition of Done (for v1)

v1 ships when a player can open a URL in a browser and:

1. See a Title screen with the title, control legend, and best score.
2. Press `Space` and immediately start running in a colourful pixel-art kitchen that scrolls smoothly with parallax.
3. Jump (`Space`, variable height) over LOW and WIDE obstacles and duck (`C`) under HIGH ones, with the jump feeling responsive — no dropped inputs, no stutter.
4. Watch the game visibly speed up, and be able to name why they died each time.
5. Lose a life on a hit, flash, and keep running; collect a heart and get it back.
6. See score top-centre and lives top-left, both updating live.
7. Pause with `P` and resume, with the world and difficulty genuinely frozen.
8. Get caught by Tom at 0 lives and see a Game Over screen with their score and best.
9. Return to the Title with `Space` and find their score on a local top-10 leaderboard that survives closing the browser.
10. Hear SFX on jump, hit, heart pickup, and game over.
11. Report that the three playtest questions were answered with **"yes, it feels right"** by a majority of testers.
