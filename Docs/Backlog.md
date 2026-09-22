# Cheese Dash — Product Backlog v1.0

Derived from the 22 original backlog stories plus the mechanics the Planning Doc mandates but the backlog omitted. Priority is **MoSCoW**; estimates are **dev-days** for one person working alone.

**Owners:** Andy M (art, audio, UI) · Andrew Z (core, spawning, build) · Joe S (world, player motion) · Josh C (score, lives, data)
*Owner assignments differ from the original Planning Doc — see `Task-Plan.md` §3 for why.*

---

## Priority definitions

| Level | Meaning |
|---|---|
| **MUST** | v1 does not ship without it. This is the MVP. |
| **SHOULD** | Real value, small cost. Build it if the MVP lands on schedule. |
| **COULD** | Nice. Only if everything above is done and verified. |
| **WON'T** | Deliberately excluded from v1. Not a rejection — a deferred decision. |

---

## MUST — the MVP

### Foundation

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M0** | *As a team, we need a shared skeleton, so that four people don't collide.* | `GameManager` state machine (`Title`/`Playing`/`Paused`/`GameOver`) exists and every system reads from it. Active Input Handling set to **Both** and committed alone. Folder structure + dev scenes per member exist. A "hello world" **WebGL build loads in a browser**. | Andrew | 1.5 |
| **M18** | *As a player, I want crisp pixel art, so the game doesn't look cheap.* | One Sprite Preset (Point filter, no compression, Full Rect, PPU 100, Clamp) applied to every sprite in `Assets/sprites/`; Preset Manager set so new imports inherit it. | Andy | 0.5 |

### Motion and feel

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M1** | *As a player, I want the world to scroll continuously, so that I feel like I'm running.* | Ground and background scroll toward the player at `WorldSpeed.Current`; the player never moves in world X; motion is frame-rate independent and stops dead when paused. | Joe | 0.5 |
| **M2** | *As a player, I want to jump with `Space`, so I can clear obstacles.* | Jump works on `Space`. **Variable height**: tap = short hop, hold = full jump. **Coyote time** ~0.10 s and **input buffer** ~0.12 s so a slightly-early or slightly-late press still registers. No double jump. Sprite animation switches run → jump → land. | Joe | 1.0 |
| **M3** | *As a player, I want to crouch with `C`, so I can duck under hazards.* | Hold `C` while grounded shrinks the collider and squashes the sprite; release restores it. Cannot crouch mid-air. **At least one obstacle type is only passable by crouching.** | Joe | 0.5 |
| **M14** | *As a player, I want theme-fitting scrolling backgrounds, so the game looks interesting.* | ≥2 parallax background layers at distinct factors (0.2–0.7), seamlessly tiled and looping with no visible seam. Ground tiles endlessly. Pixel Perfect Camera configured (480×270, upscale on) so art stays crisp. | Joe (env) + Andy (art) | 2.0 |

### Obstacles and difficulty

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M4** | *As a player, I want active obstacles to dodge, so there's a game to play.* | Spawner emits all four archetypes (LOW/WIDE = jump, HIGH = crouch, FAST = earlier reaction). Obstacles approach, are dodged, and recycle through a **pool** with no per-spawn `Instantiate`/`Destroy`. | Andrew | 2.0 |
| **M5** | *As a player, I want the game to get harder, so it stays challenging.* | A `DifficultyCurve` **ScriptableObject** drives speed, gap, mix, and heart chance — all tunable in the Inspector with no code edit. **Gaps are computed in seconds and converted to units at spawn time**, so reaction time stays constant as speed rises. Variety (new archetypes) is introduced before speed, per spec §2.7. | Andrew | 1.0 |

### Lives, score, and feedback

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M6** | *As a player, I want to lose a life when I'm hit, so mistakes have consequences.* | Start with 3 lives. A hit costs 1 life, grants ~1.5 s invulnerability, and flashes the sprite. The run and the world speed continue uninterrupted. | Josh | 1.0 |
| **M7** | *As a player, I want hearts to give me a life back, so I can recover.* | A heart grants +1 life up to the cap of 3, with a sparkle VFX and SFX. **Hearts spawn only when `lives < 3`** and are always reachable (never inside or immediately behind an obstacle's landing zone). | Josh | 0.5 |
| **M8** | *As a player, I want to see my score, so I can track how far I've come.* | Score accrues as `WorldSpeed.Current × pointsPerUnit × dt`, displayed as an integer **top-centre**, updating live, frozen while paused. | Josh | 0.5 |
| **M9** | *As a player, I want to see my health, so I know when to dodge.* | Lives shown as **3 heart icons, top-left**, one dimmed per life lost, updating live. | Josh | 0.5 |
| **M11** | *As a player, I want to pause, so I can stop safely.* | `P` freezes the world, the difficulty ramp, and the score. Overlay offers resume (`Space`), restart (`R`), and quit to title (`Q`). No time-based system advances while paused. | Josh | 0.5 |

### Screens

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M10** | *As a player, I want a start menu and a controls legend, so I know how to play.* | Title screen shows the game title, the one-line premise ("You stole the cheese. Tom saw you. RUN."), a control legend (`Space` jump · `C` crouch · `P` pause), the best score, and the top-3 leaderboard. `Space` starts the run. **A first-time player can play correctly without being told anything verbally.** | Andy | 1.0 |
| **M12** | *As a player, I want a game over screen, so the run feels finished.* | At 0 lives Tom pounces and catches Jerry. Screen shows "Tom caught you!", final score, distance/time survived, a `NEW BEST!` flag when applicable, the top-3 leaderboard, and "Press `Space` to return to Title". | Andy (UI) + Josh (logic) | 1.0 |
| **M13** | *As a player, I want a leaderboard, so I have a reason to keep playing.* | Top-10 scores persisted locally in `PlayerPrefs` as JSON, surviving closing the browser. Ranked descending, ties broken by most recent. Top-3 shown on Title and Game Over. | Josh | 1.0 |

### Presentation and delivery

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **M15** | *As a player, I want sound effects, so actions feel responsive.* | SFX on jump, land, hit/life lost, heart pickup, game over (Tom yowl), new best, and UI confirm. All short 8-bit bites, all CC0 or original, with attribution recorded. **No background music** (spec §7 #1). | Andy | 1.0 |
| **M16** | *As a player, I want the game to fit my screen, so it's comfortable to play.* | WebGL canvas fills the window and letterboxes correctly, staying pixel-crisp at any aspect ratio and window size. | Joe | 0.5 |
| **M17** | *As a player, I want to play in my browser without installing anything.* | WebGL build hosted (GitHub Pages or itch.io), loads in <30 s on a normal connection, playable with keyboard only, no console errors. Verified on a machine that is not a dev's. | Andrew (lead) | 1.0 |

**MUST total: 17.0 dev-days.**

---

## SHOULD

| ID | Story | Acceptance criteria | Owner | Est |
|---|---|---|---|---|
| **S1** | *As a player, I want Tom visibly closing in, so I feel hunted.* | **Proximity life meter:** Tom's X-position lerps between "far behind" at 3 lives and "on Jerry's heels" at 1 life; he lunges on a hit and pounces at 0. **Promote to MUST the moment Tom's run sheet exists.** | Joe | 1.0 |
| **S2** | *As a player, I want bonus pickups, so there's risk and reward.* | Cheese pickup grants +50 points with VFX/SFX, placed only where grabbing it is a real decision. *(Art already in repo.)* | Josh | 0.5 |
| **S3** | *As a player, I want a timer, so I can see how long I've been going.* | Elapsed time shown (either with the score or on the Game Over screen), frozen while paused. | Josh | 0.25 |
| **S4** | *As a player, I want my settings to persist, so I don't redo them every session.* | Best score, leaderboard, and any options survive a browser restart. *(Largely subsumed by M13.)* | Josh | 0.25 |
| **S5** | *As a player, I want music that fits, so the game feels polished.* | One looping 8-bit track + a volume slider/mute affecting music and SFX separately. **Blocked on the no-music decision in spec §7 #1 — only build if the team overturns it.** | Andy | 1.0 |
| **S6** | *As a player, I want hits to feel impactful, so feedback is clear.* | Brief screen shake and hit-stop (~0.08 s) on damage. Must not disturb the pixel-perfect camera or the spawn fairness. | Joe | 0.5 |

**SHOULD total: 3.5 dev-days.**

---

## COULD

| ID | Story | Owner | Est |
|---|---|---|---|
| **C1** | Achievements (e.g. "survive 60 s", "no-hit 500 points") with a toast notification | Josh | 1.5 |
| **C2** | Easter eggs hidden in backgrounds | Andy | 0.5 |
| **C3** | Options screen with a jump-height slider | Josh | 0.5 |
| **C4** | Separate music/SFX volume sliders | Andy | 0.5 |

---

## WON'T (this release)

| ID | Story | Why it's out | Revisit if |
|---|---|---|---|
| **W1** | Upload my own picture as the character | Runtime image decoding, file dialogs, and validation — in WebGL this is bigger than the entire core game, and it breaks the art direction. | Never, for v1. |
| **W2** | Customise my character | Same pipeline cost, no gameplay value in a game whose identity is *being* Jerry. | A v2 art system exists. |
| **W3** | Meta-progress / get stronger over time | Directly contradicts "performance doesn't change mechanics." Progression would make the leaderboard measure grinding instead of skill. | Never — it would undermine the design. |
| **W4** | Gadgets / power-ups | Same reason. Hearts are the only item per the Planning Doc. | A v2 with a wider item economy. |
| **W5** | Online leaderboard / multiplayer | No server, and the audience is explicitly offline browser users. | Someone will own a backend. |
| **W6** | Story cutscene | Contradicts "there won't be a plot or storyline." Delivered as one Title-screen line instead. | Never, for this game. |
| **W7** | Save and resume a run | A run lasts 30–180 s. Score/settings persistence is all this can sensibly mean. | Runs become meaningfully long. |

---

## Traceability — what happened to all 22 original stories

Every original story is accounted for. Nothing was silently dropped.

| # | Original story | Disposition |
|---|---|---|
| 1 | See my health | **MUST** → M9 |
| 2 | Start menu | **MUST** → M10 |
| 3 | Leaderboard for motivation | **MUST** → M13 (local, not online — spec §7 #3) |
| 4 | Theme-fitting music | **SHOULD** → S5, conflicts with the Planning Doc's "no background music" (spec §7 #1) |
| 5 | Intro scene with backstory | **MUST (reduced)** → M10, as a one-line premise instead of a cutscene (spec §7 #2) |
| 6 | Tutorial / control explanation | **MUST** → M10, control legend on the Title screen |
| 7 | Easter eggs | **COULD** → C2 |
| 8 | Meta-progress | **WON'T** → W3 |
| 9 | Adjust my jump | **MUST (reinterpreted)** → M2, as variable jump height rather than a settings slider (spec §7 #8) |
| 10 | Adjust volume | **MUST (reduced)** → mute via config; full sliders in S5/C4, blocked on the music decision |
| 11 | See my score / progress | **MUST** → M8 |
| 12 | Timer | **SHOULD** → S3 |
| 13 | Use my keyboard to move | **MUST** → M2 + M3 |
| 14 | See my character | **MUST** → satisfied by M1/M2/M3 + M14 (Jerry is always visible at fixed X) |
| 15 | Set my game tab size | **MUST** → M16, satisfied by the WebGL canvas + Pixel Perfect Camera |
| 16 | Upload my own picture | **WON'T** → W1 |
| 17 | Save my game | **SHOULD (reduced)** → S4 + M13; full run-resume is W7 |
| 18 | Customise my character | **WON'T** → W2 |
| 19 | Theme-fitting backgrounds | **MUST** → M14 |
| 20 | Responsive controls | **MUST** → M2 + M3, plus pooling in M4 to prevent stutter (spec §5.5) |
| 21 | Gadgets to help me in-game | **WON'T** → W4 |
| 22 | Achievements | **COULD** → C1 |

### Stories this reconciliation added

The backlog omitted several things the Planning Doc already mandates, and one thing that had no owner at all. These are now stories:

| Added | Became | Why it was missing |
|---|---|---|
| Pause (`P`) | M11 | The Planning Doc specifies it; the backlog has no story for it. |
| A reason to crouch | M3 | Crouch was required but had no purpose — nothing in the game was duckable. |
| Game Over → Title flow | M12 | Implied by the Planning Doc, absent from the backlog. |
| Difficulty ramp | M5 | Stated in the Planning Doc, never made a story. |
| Tom's presence | S1 + M12 | The antagonist had no story and no mechanic defined. |
| Hearts as pickups | M7 | "Obtainable items include heart" — never became a story. |
| Sound effects | M15 | Implied, never owned. |
| Input feel (coyote/buffer/variable jump) | M2 | Directly targets playtest question 1. |
| WebGL build and hosting | M17 | **The target platform had no owner and no story.** |
| Pixel-art visual correctness | M18 | Blurry art was the most likely "this looks bad" outcome. |
| Leaderboard persistence | M13 | "Scoreboard" was assumed to exist without anyone building it. |

---

## Budget

| Phase | Dev-days |
|---|---|
| MUST | 17.0 |
| SHOULD | 3.5 |
| COULD | 3.0 |

Per-person MUST load after the rebalancing in `Task-Plan.md` §3:

| Owner | Dev-days | Notes |
|---|---|---|
| Andrew | 5.5 | Critical path — spawning + difficulty + WebGL (M17 falls in the integration phase, so 4.5 in the parallel phase) |
| Andy | 5.0 | Art + UI screens + audio |
| Josh | 4.0 | Score, lives, data, leaderboard |
| Joe | 3.5 | World motion + player controller |

Parallel phase ceiling is **5.0 dev-days**, which fits a 5-day phase with the critical path at 5.0. See `Task-Plan.md` for the schedule.
