# Gecko Out — Case Study Clone

A grid-based "snake-out" puzzle clone (reference: *Gecko Out* by Dalak Games),
built as a study case with one goal: **sustainable, testable, SOLID architecture**.

> Visuals are intentionally minimal — the deliverable is the code.

- Unity **6000.3.8f1**, URP (mobile), portrait
- **85 EditMode unit tests** covering the entire game logic
- 10 data-driven levels, one gameplay scene

## How to play

Drag a gecko from **either end** (head or tail). The body follows the exact
path you draw; walls, other geckos and your own body block the way. Bring a
gecko to the hole of its own color to send it out. Clear every gecko before
the timer runs out. Dragging an end back onto the cell it just left **undoes**
that step.

## Architecture

Five assemblies with compiler-enforced dependency direction (arrows = "references"):

    UI ───────────┐
                  ├──► Data ──► Core
    Presentation ─┘
    App (composition root) ──► all

- **Core** — pure C# game logic, `noEngineReferences: true`. Grid, gecko
  bodies, move validation, BFS pathfinding, reversible commands, session
  (timer + win/lose). It does not know Unity exists, which is what makes it
  fully unit-testable.
- **Data** — level JSON schema (DTOs), loader, validator, factory. DTOs are
  deliberately separate from domain types: the file format and the domain
  model must be able to evolve independently.
- **Presentation** — renders Core state and translates pointer input into
  Core *intents*. Contains zero game rules.
- **UI** — HUD and result panels; talks to the session only through events
  and read-only polling.
- **App** — composition root: the only place that news up and wires the
  pieces; owns the level flow (next / retry).

### Patterns used, and why

| Pattern | Where | Why it earns its place |
|---|---|---|
| Command | `StepCommand` / `MoveHistory` | Backwards dragging *is* `Undo` — the mechanic itself, not decoration |
| Strategy (OCP) | `IExitRule` | Special exits (frozen, toll gate) plug in without touching `MoveValidator` |
| Factory | `LevelFactory` | Single gate where untrusted DTO data becomes guarded domain objects |
| Object pool | `ObjectPool<T>` | One generic implementation; used where churn exists (gecko segments) |
| Facade / intent API | `LevelSession` | Input produces intents; all rules resolve inside the tested core |

### Key decisions & trade-offs

- **One gameplay scene, data-driven boards.** Adding level 11 means adding
  one JSON file, not one scene.
- **Levels are JSON with a collecting validator** — all file problems are
  reported in a single pass, with designer-readable messages.
- **Exit resolves atomically in the model**; the sink animation is purely
  visual. Trade-off: freed cells become walkable a moment before the body
  visually finishes sinking. Accepted for MVP simplicity.
- **Validation lives at boundaries** (constructors, level validator) —
  invariants are then trusted; no defensive null-checks scattered in logic.
- **Deliberate non-unifications:** the in-body duplicate-cell check
  (`GeckoBody`) vs the board-wide overlap check (`LevelValidator`) look
  similar but encode different knowledge and stay separate. Shared knowledge
  (chain connectivity, bounds check, self-overlap rule) was extracted to
  single sources instead.
- **After the last level the flow loops** back to level 1 (no meta/menu —
  out of scope).

### Extension points (not implemented, on purpose)

- `IExitRule` — e.g. `FrozenExitRule` (exit opens after N geckos leave)
  without modifying the validator.
- `IReversibleCommand` — new reversible actions join the same history.
- `LevelCatalogSO` — level packs / remote catalogs behind the same interface.

## Running the tests

Window → General → Test Runner → **EditMode** → Run All. 85 tests cover the
board, gecko body mechanics, move validation, pathfinding, command undo,
session flow (win/lose/timer) and the level data pipeline.

## Level format

```json
{
  "levelId": 1,
  "timeLimitSeconds": 30,
  "gridWidth": 5,
  "gridHeight": 5,
  "walls":  [ { "x": 1, "y": 0 } ],
  "exits":  [ { "x": 4, "y": 2, "color": "Red" } ],
  "geckos": [
    { "color": "Red", "cells": [ { "x": 1, "y": 2 }, { "x": 0, "y": 2 } ] }
  ]
}